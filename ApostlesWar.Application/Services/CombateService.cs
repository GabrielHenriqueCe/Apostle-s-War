using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Ativas;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;
using ApostlesWar.Domain.Skills.Passivas;
using ApostlesWar.Application.Portas;
using ApostlesWar.Application.Controllers;

namespace ApostlesWar.Application.Services
{
    public class CombateService
    {
        #region Construtor

        private readonly ArsenalService _arsenalService;
        private readonly ApostolosService _apostolosService;
        private readonly PersonagemService _personagemService;
        private readonly ProgressaoService _progressaoService;
        private readonly ITelaDeCombate _tela;
        private readonly SelecaoDeAlvoService _selecaoDeAlvoService;
        private readonly IControladorDeTurno _controladorJogador;
        private readonly IControladorDeTurno _controladorBot;
        private readonly IApresentacao _apresentacao;
        private readonly RelogioDoCombate _relogio;

        // Estrutura da batalha atual (times/perspectiva) + quem controla cada equipe. Setados por
        // rodada em ExecutarRodada (mesmo lifecycle do _relogio). No Versus, o ponto de entrada monta
        // times e controladores diferentes; daqui pra baixo o loop não sabe a diferença.
        private Batalha _batalha = null!;
        private Dictionary<Equipe, IControladorDeTurno> _controladores = new();

        public CombateService(ArsenalService arsenalService,
            ApostolosService apostolosService, PersonagemService personagemService,
            ProgressaoService progressaoService, ITelaDeCombate tela,
            SelecaoDeAlvoService selecaoDeAlvoService, IControladorDeTurno controladorJogador,
            IControladorDeTurno controladorBot, IApresentacao apresentacao, RelogioDoCombate relogio)
        {
            _arsenalService = arsenalService;
            _apostolosService = apostolosService;
            _personagemService = personagemService;
            _progressaoService = progressaoService;
            _tela = tela;
            _selecaoDeAlvoService = selecaoDeAlvoService;
            _controladorJogador = controladorJogador;
            _controladorBot = controladorBot;
            _apresentacao = apresentacao;
            _relogio = relogio;
        }

        /// <summary>O controlador que DECIDE ação/alvo deste combatente — pela EQUIPE que ele
        /// integra, não pela classe (Jogador/Inimigo). No Versus, uma equipe de "Jogadores" pode ser
        /// controlada por bot e vice-versa; a decisão vem do mapa montado no ponto de entrada.</summary>
        private IControladorDeTurno ControladorDe(Combate combatente)
            => _controladores[_batalha.EquipeDe(combatente)];

        /// <summary>
        /// Deixa a narrativa respirar entre eventos, e ESCUTA o pedido de sair: se o jogador pediu pra
        /// encerrar e confirmou, aborta a batalha (BatalhaAbortada, capturada em ExecutarFaseComTime →
        /// a fase vira derrota). Todas as esperas do combate passam por aqui — é o ponto único do
        /// cancelamento.
        ///
        /// Repare que o motor diz QUAL batida acabou de narrar, nunca quantos milissegundos ela dura:
        /// tempo de tela é assunto da pele (ver <see cref="Momento"/>).
        /// </summary>
        private void Aguardar(Momento momento)
        {
            if (_apresentacao.AguardarAnimacao(momento) && _tela.ConfirmarEncerramento())
                throw new BatalhaAbortada();
        }

        #endregion

        #region Loop principal

        /// <summary>
        /// O laço da batalha: enquanto os dois lados tiverem vivos, a <see cref="FilaDeTurnos"/> diz
        /// de quem é a vez e o turno acontece.
        ///
        /// Não existe mais RODADA — a fila não tem volta ao começo, e quem é rápido joga de novo
        /// antes de o lento jogar a primeira vez. O que existia aqui era um `for` sobre
        /// `Equipe1.Membros ++ Equipe2.Membros`, e ele dava à equipe 1 uma vantagem estrutural que
        /// ninguém tinha desenhado.
        /// </summary>
        // Quantas vezes a fila mostra. Oito é o campo cheio (4×4): a leitura interessante é "quem joga
        // antes de mim de novo", e ela cabe numa volta do tabuleiro.
        private const int VezesPrevistas = 8;

        private bool ExecutarCombate(Batalha batalha)
        {
            _batalha = batalha;
            _relogio.Reiniciar();   // nova batalha: zera o contador de turnos
            var fila = new FilaDeTurnos(batalha);

            while (batalha.Equipe1.TemVivos() && batalha.Equipe2.TemVivos())
            {
                if (fila.Proximo() is not Combate daVez) break;   // ninguém pode agir: a luta não tem como seguir

                // A ordem vai pra tela ANTES da ação: é a fila de agora, com o dono do turno na
                // frente. Depois da ação ela já é outra — e é isso que o jogador precisa ver.
                _tela.ExibirFilaDeTurnos(fila.Prever(VezesPrevistas));

                ExecutarTurnoCompleto(daVez);

                // Turno extra: dispara enquanto a flag estiver setada.
                // Loop teórico infinito é permitido por design (RNG decide quando para).
                // Ele NÃO passa pela fila de propósito: não desconta medidor nem cobra o custo da
                // ação. Se cobrasse, o prêmio de jogar de novo encheria a barra de todo mundo junto
                // — inclusive a dos inimigos —, e o turno extra pagaria por si mesmo.
                while (daVez.TemTurnoExtra)
                {
                    daVez.ConsumirTurnoExtra();
                    if (!daVez.EstaVivo()) break;
                    if (!batalha.Equipe1.TemVivos() || !batalha.Equipe2.TemVivos()) break;
                    ExecutarTurnoCompleto(daVez);
                }

                // Depois da ação, e não antes: um empurrão de medidor dado DURANTE o turno entra
                // antes do desconto, que é o que quem lançou a habilidade espera.
                fila.Consumir(daVez);
            }

            return batalha.Equipe1.TemVivos();   // Equipe1 = jogador na campanha
        }

        /// <summary>
        /// Executa um turno completo de um combatente:
        /// - Trigga AoIniciarTurno dos status (Veneno, Queima tickam aqui)
        /// - Dispara passivas com EventoCombate.InicioDoTurno (Realidade reaplica RefletirDano)
        /// - Se Preso: pula a ação mas avança status e cooldowns
        /// - Senão: executa a ação (a1, habilidade) e processa passivas reativas
        /// - Avança duração de status e cooldowns
        /// 
        /// Chamado uma vez por turno do round, e potencialmente N vezes a mais se o combatente
        /// tiver ConcederTurnoExtra acionado durante o próprio turno.
        /// </summary>
        private void ExecutarTurnoCompleto(Combate atacante)
        {
            _relogio.Avancar();   // "cada vez que um personagem joga aumenta o contador" (inclui turno-extra e Preso)

            var (aliados, defensores) = _batalha.PerspectivaDe(atacante);

            var turno = atacante.Turno;   // Turno PERSISTENTE (dono do estado turn-scoped), não mais criado por turno

            var ticks = turno.Iniciar();
            // Board FIXO na exibição dos ticks (Equipe1 = "Seu time", Equipe2 = "Inimigos"), como antes.
            MostrarTicks(_batalha.Equipe1.Membros, _batalha.Equipe2.Membros, ticks);   // veneno/queima/cura-contínua VISÍVEIS no início do turno
            DispararEventoInicioDeTurno(atacante, aliados, defensores);
            if (!atacante.EstaVivo()) return;

            if (atacante.StatusAtivos.OfType<IPulaTurno>().Any())
            {
                turno.Finalizar();
                return;
            }

            ExecutarTurno(atacante, defensores, aliados);

            turno.Finalizar();
        }

        #endregion

        #region Hooks de turno

        /// <summary>
        /// Mostra os eventos dos ticks de status do início do turno (veneno/queima causando dano,
        /// cura-contínua curando) — o HP já foi aplicado no TurnoDoPersonagem.Iniciar; aqui redesenha
        /// a partida e exibe cada evento com pausa, no mesmo padrão do ataque. Lista vazia = no-op.
        /// </summary>
        private void MostrarTicks(List<Combate> jogador, List<Combate> inimigo, List<EventoCombate> ticks)
        {
            if (ticks.Count == 0) return;

            _tela.LimparTela();
            _tela.ExibirPartida(jogador, inimigo);
            foreach (var ev in ticks)
            {
                if (ev is EventoDano d) _tela.ExibirDanoDeStatus(d);
                else if (ev is EventoCura c) _tela.ExibirCura(c);
                Aguardar(Momento.Tick);
            }
        }

        /// <summary>
        /// Dispara o evento InicioDoTurno das passivas via IReageAoInicioTurno (ex:
        /// Realidade reaplica RefletirDano). Fica no service por ora; reavaliar se
        /// o disparo vai para o Turno quando o resto do Turno (reset 1x-por-agressor +
        /// TimeAtualDoTurno) for feito.
        /// O tick dos status já foi disparado por TurnoDoPersonagem.Iniciar().
        /// </summary>
        private void DispararEventoInicioDeTurno(Combate combatente, List<Combate> aliados, List<Combate> inimigos)
        {
            if (!combatente.EstaVivo()) return;

            var ctxCombate = new ContextoCombate(combatente, aliados, inimigos);
            ProcessarReacoesInicioTurno(combatente, ctxCombate);
        }   

        /// <summary>
        /// Dispara as reações de início de turno (IReageAoInicioTurno) do combatente.
        /// Sem golpe — usa o ContextoCombate (portador = combatente). Renova buffs/
        /// efeitos que o portador aplica a cada turno. Varre as duas fontes como todo
        /// dispatch (hoje só passivas implementam; um buff que queira reagir já entra).
        /// </summary>
        private void ProcessarReacoesInicioTurno(Combate combatente, ContextoCombate ctxCombate)
        {
            var resultados = ColetarReacoes<IReageAoInicioTurno>(combatente, r => r.AoInicioTurno(ctxCombate));
            ExibirResultadosReacao(combatente, resultados);
        }

        #endregion

        #region Turno

        private void ExecutarTurno(Combate atacante, List<Combate> defensores, List<Combate> aliados)
        {
            _tela.LimparTela();

            // Ação forçada (Irritar): o status decide o alvo; o fluxo executa A1 (mas paralisia pode interromper)
            var forcaAcao = atacante.StatusAtivos.OfType<IForcaAcao>().FirstOrDefault();
            if (forcaAcao != null)
            {
                var alvoForcado = forcaAcao.AlvoForcado();
                _tela.ExibirPartida(aliados, defensores);
                _tela.ExibirMensagemPassiva(
                    $"{atacante.Personagem.Simbolo} está irritado e ataca {alvoForcado.Personagem.Simbolo} automaticamente!");
                Aguardar(Momento.Narracao);

                if (VerificarParalisia(atacante)) return;

                var resultado = atacante.Atacar(alvoForcado);
                ExecutarAtos(new List<EventoCombate> { resultado }, atacante, TipoAtaque.Sequencial);
                return;
            }

            // Seleção (quem decide) via controlador; execução (o que acontece) separada.
            // Loop: Esc no menu de AÇÃO = encerrar (hab null → aborta); Esc no ALVO = voltar (alvo null).
            var controlador = ControladorDe(atacante);
            while (true)
            {
                HabilidadeAtiva? hab = controlador.EscolherAcao(atacante, aliados, defensores);
                if (hab == null) throw new BatalhaAbortada();

                Combate? alvo = ResolverAlvoInicial(atacante, hab, defensores, aliados, controlador);
                if (alvo == null) continue;   // Esc no alvo → volta pra seleção de habilidade

                ExecutarHabilidade(atacante, hab, alvo, defensores, aliados);
                return;
            }
        }

        /// <summary>
        /// Consulta a capacidade de paralisia (Medo) do portador e rola o dado.
        /// Retorna true se a ação foi paralizada (chamador deve abortar).
        /// </summary>
        private bool VerificarParalisia(Combate atacante)
        {
            var paralisia = atacante.StatusAtivos.OfType<IParalisaAcao>().FirstOrDefault();
            if (paralisia == null) return false;
            if (!paralisia.Paralisa()) return false;

            _tela.ExibirMensagemPassiva(
                $"{atacante.Personagem.Simbolo} {atacante.Personagem.Nome} estava com medo e não conseguiu agir!");
            Aguardar(Momento.Narracao);
            return true;
        }

        #endregion

        #region Execução

        /// <summary>
        /// Resolve o alvo-semente do golpe (o que a habilidade recebe pra derivar seus AlvosResolvidos).
        /// A COLA (qual lista consultar, lista vazia → o próprio, hit-all → o próprio) fica aqui; o PICK
        /// em si (menu/bot) é do controlador. §8.2 (derivar o menu da ação) é slice à parte, depois.
        /// </summary>
        private Combate? ResolverAlvoInicial(Combate atacante, HabilidadeAtiva hab,
            List<Combate> defensores, List<Combate> aliados, IControladorDeTurno controlador)
        {
            // Self/hit-all não pedem alvo: a habilidade resolve sozinha. Regra ESTÁTICA na fonte única
            // (HabilidadeAtiva.PedeAlvoDoJogador) — o front lê a mesma pra montar o menu de habilidade.
            if (!hab.PedeAlvoDoJogador) return atacante;

            if (hab.TipoLista == TipoLista.Inimigos)
            {
                var disponiveis = _selecaoDeAlvoService.ResolverAlvosDisponiveis(defensores);
                return controlador.EscolherAlvo(disponiveis, aliados, defensores);
            }

            // Sobra o aliado finito (PedeAlvoDoJogador garante NumeroDeAlvos != MaxValue). Pick real de
            // alvo aliado (por estado). Sem candidato no estado pedido (ex: revive sem mortos): pula o
            // pick — ResolverAlvos devolve vazio pra ação que herda o alvo, e as demais ações (escopos
            // próprios) rodam normalmente (DocesDeAbobora sem mortos ainda vale pelo Reflexo). Esta
            // parte DEPENDE do tabuleiro, por isso fica aqui e não na propriedade.
            var disponiveisAliados = _selecaoDeAlvoService.ResolverAlvosDisponiveis(aliados, hab.EstadoAlvo);
            return disponiveisAliados.Count == 0
                ? atacante
                : controlador.EscolherAlvo(disponiveisAliados, aliados, defensores);
        }

        /// <summary>
        /// Roda os Atos de reação sobre cada resultado produzido pelo AtoExecucao.
        /// Ordem do ADR: AtoReacaoDoAlvo → AtoMorte → AtoReacaoDoAtacante.
        /// Compartilhado pelo caminho normal (ExecutarHabilidade) e pelo Irritar.
        ///
        /// O `TipoAtaque` decide duas coisas independentes: o RITMO (pinga alvo a alvo ou cai de uma
        /// vez) e a REGRA (a reação-por-ataque dispara). Só o Sequencial pinga; só a Área é ataque.
        /// </summary>
        private void ExecutarAtos(List<EventoCombate> resultados, Combate atacante, TipoAtaque tipoAtaque)
        {
            if (tipoAtaque == TipoAtaque.Sequencial)
            {
                ExecutarAtosGolpeAGolpe(resultados, atacante);
                return;
            }

            ExecutarAtosDeUmaVez(resultados, atacante,
                dispararReacaoPorAtaque: tipoAtaque == TipoAtaque.AreaDeEfeito);
        }

        /// <summary>
        /// Ataque SEQUENCIAL: um baque por golpe. Cada alvo aparece, espera a sua batida, e só então
        /// reage. A reação-por-ataque do atacante mora DENTRO do laço porque aqui ela é por hit.
        /// </summary>
        private void ExecutarAtosGolpeAGolpe(List<EventoCombate> resultados, Combate atacante)
        {
            foreach (var ev in resultados)
            {
                // Cura é irmã do dano no stream, mas só EXIBE — não dispara reação de dano.
                if (ev is EventoCura cura)
                {
                    _tela.ExibirCura(cura);
                    Aguardar(Momento.Golpe);
                    continue;
                }

                var r = (EventoDano)ev;
                _tela.ExibirResultadoAtaque(atacante, r.Alvo, r);
                Aguardar(Momento.Golpe);

                ProcessarReacoesAlvo(r.Alvo, atacante, r);
                ProcessarReacoesAtacanteMorte(atacante, r.Alvo, r);
                ProcessarReacoesAoMorrer(r.Alvo, atacante, r);
                ProcessarReacoesAtacantePorAlvo(atacante, r.Alvo, r);
                ProcessarReacoesAtacantePorAtaque(atacante, r.Alvo, r);
            }
        }

        /// <summary>
        /// UM BAQUE SÓ (área e cura em grupo): exibe todos os resultados sem pausa entre eles, UMA
        /// pausa, e só então as reações. Adiantar a exibição é seguro — o `hab.Ativar` já aplicou
        /// tudo no modelo antes de qualquer narração.
        ///
        /// `dispararReacaoPorAtaque` NÃO pode virar dedução do tipo "tem dano, logo foi ataque" — o
        /// Inferno do Diabo é `NaoAtaque` e causa dano em todos, e deduzir faria o IReageAoAtacar
        /// dele disparar.
        /// </summary>
        private void ExecutarAtosDeUmaVez(List<EventoCombate> resultados, Combate atacante,
            bool dispararReacaoPorAtaque)
        {
            foreach (var ev in resultados)
            {
                if (ev is EventoCura cura) _tela.ExibirCura(cura);
                else _tela.ExibirResultadoAtaque(atacante, ((EventoDano)ev).Alvo, (EventoDano)ev);
            }

            if (resultados.Count > 0) Aguardar(Momento.Golpe);

            var danos = resultados.OfType<EventoDano>().ToList();
            foreach (var r in danos)
            {
                ProcessarReacoesAlvo(r.Alvo, atacante, r);
                ProcessarReacoesAtacanteMorte(atacante, r.Alvo, r);
                ProcessarReacoesAoMorrer(r.Alvo, atacante, r);
                ProcessarReacoesAtacantePorAlvo(atacante, r.Alvo, r);
            }

            if (dispararReacaoPorAtaque && danos.Count > 0)
                ProcessarReacoesAtacantePorAtaque(atacante, danos[0].Alvo, danos[0]);
        }

        private void ExecutarHabilidade(Combate atacante, HabilidadeAtiva hab, Combate alvoInicial,
            List<Combate> defensores, List<Combate> aliados)
        {
            var ctx = new ContextoCombate(atacante, aliados, defensores);

            // Setup: UX de preparação — dá ao humano um beat pra ver o A1 de um combatente controlado
            // por BOT chegando (apresentação segue o CONTROLE, não a classe).
            if (ControladorDe(atacante) == _controladorBot && hab is AtaqueBasico)
            {
                _tela.ExibirPreparacaoAtaque(atacante, defensores);
                Aguardar(Momento.Preparacao);
            }

            // Setup: paralisia (Medo) trigga DEPOIS da escolha (jogador escolhe, vê o medo, perde cooldown)
            if (VerificarParalisia(atacante))
            {
                atacante.Cooldowns[hab].Usar();
                return;
            }

            // Os Atos
            var resultados = hab.Ativar(ctx, alvoInicial);                             // AtoExecucao

            // Nome da habilidade PRIMEIRO — "X usou {hab}!" antes dos resultados (narrativa).
            if (hab is not AtaqueBasico)
            {
                _tela.ExibirUsoHabilidade(atacante, hab);
                Aguardar(Momento.Narracao);
            }

            ExecutarAtos(resultados, atacante, hab.TipoAtaque);   // Reação + Morte + Atacante
            atacante.Cooldowns[hab].Usar();                                            // AtoEncerramento
        }

        #endregion

        #region Reacao

        /// <summary>
        /// Dispara as reações do ALVO a um golpe recebido. Ordem: Reflexo/Sangramento
        /// (dano > 0) -> Espinhos/ContraAtaque/Operário (sempre). ContraAtaque e
        /// InstintoDoOperario declaram um revide (ResultadoReacao.Revide: Habilidade +
        /// Alvo); este método o executa via IAtivavelComNatureza e propaga
        /// recursivamente as reações do alvo revidado. O parâmetro profundidade
        /// garante profundidade máxima 1 — só processa Revide na chamada de topo
        /// (profundidade 0); a recursão em si (profundidade 1) nunca declara outro
        /// revide, quebrando o loop A↔B. Não depende da Natureza do golpe.
        /// </summary>
        private void ProcessarReacoesAlvo(Combate alvo, Combate atacante, EventoDano r, int profundidade = 0)
        {
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;

            // Portador da reação é o ALVO: sua perspectiva vem da Batalha (um só caminho).
            var (aliadosDoAlvo, inimigosDoAlvo) = _batalha.PerspectivaDe(alvo);

            var ctx = new ContextoReacao(alvo, atacante, r.DanoEfetivo, r.Natureza,
                r.Critico, aliadosDoAlvo, inimigosDoAlvo);
            var resultados = new List<ResultadoReacao>();

            if (r.DanoEfetivo > 0)
                resultados.AddRange(ColetarReacoes<IReageAoReceberDano>(alvo, x => x.AoReceberDano(ctx)));

            resultados.AddRange(ColetarReacoes<IReageAoSerAtacado>(alvo, x => x.AoSerAtacado(ctx)));

            ExibirResultadosReacao(alvo, resultados);

            foreach (var res in resultados)
            {
                if (res.Revide == null) continue;
                if (profundidade > 0) continue;
                if (!alvo.EstaVivo()) break;
                if (!res.Revide.Alvo.EstaVivo()) continue;

                var revide = res.Revide.Habilidade.AtivarComNatureza(alvo, res.Revide.Alvo, NaturezasDano.Ataque);
                _tela.ExibirResultadoAtaque(alvo, revide.Alvo, revide);
                Aguardar(Momento.Golpe);
                // No revide, o portador do próximo nível é o revidado; a Batalha resolve a
                // perspectiva dele sozinha (não precisa mais passar/inverter times na mão).
                ProcessarReacoesAlvo(res.Revide.Alvo, alvo, revide, profundidade + 1);
            }
        }

        /// <summary>
        /// Varredura ÚNICA de reações: coleta quem implementa a capacidade T nas DUAS fontes
        /// (StatusAtivos + passivas do Personagem) e invoca a reação de cada um, na ordem
        /// status → passivas — a mesma que os métodos repetiam à mão. O invocar é lambda
        /// porque cada interface tem seu verbo (AoReceberDano, AoMatar...); o helper unifica
        /// a VARREDURA, não o verbo. Snapshot (ToList) dos status porque a reação pode
        /// remover/adicionar status do próprio portador.
        /// </summary>
        private List<ResultadoReacao> ColetarReacoes<T>(Combate portador,
            Func<T, IEnumerable<ResultadoReacao>> invocar) where T : class
        {
            var resultados = new List<ResultadoReacao>();
            foreach (var s in portador.StatusAtivos.OfType<T>().ToList())
                resultados.AddRange(invocar(s));
            foreach (var p in ColetarPassivasReativas<T>(portador))
                resultados.AddRange(invocar(p));
            return resultados;
        }

        /// <summary>
        /// Coleta as passivas do combatente que implementam a interface de reação T,
        /// respeitando o cooldown (passivas têm Cooldowns; buffs/status não). Consome o
        /// cooldown ao coletar — a passiva "usou" sua reação neste disparo. Passivas com
        /// cooldown 0 (a maioria das reativas) estão sempre disponíveis.
        /// ATENÇÃO (contrato): o consumo acontece AO COLETAR, antes de
        /// saber se a passiva vai agir. Com cooldown 0 é inofensivo; ao criar passiva
        /// reativa com cooldown E condição interna, mover o consumo pra depois da decisão.
        /// </summary>
        private IEnumerable<T> ColetarPassivasReativas<T>(Combate combatente) where T : class
        {
            var coletadas = new List<T>();
            foreach (var hab in combatente.Personagem.Habilidades)
            {
                if (hab is not T reativa) continue;
                if (!combatente.Cooldowns[hab].Disponivel) continue;

                coletadas.Add(reativa);
                combatente.Cooldowns[hab].Usar();
            }
            return coletadas;
        }

        /// <summary>
        /// Reações do atacante POR ALVO atingido (Nx). Chamado dentro do foreach.
        /// IReagePorAtaque (Sorrateiro, Policial) + IReageAoCausarDano (Sedento, dano > 0).
        /// </summary>
        private void ProcessarReacoesAtacantePorAlvo(Combate atacante, Combate alvo, EventoDano r)
        {
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;

            var (aliadosDoAtacante, inimigosDoAtacante) = _batalha.PerspectivaDe(atacante);
            var ctx = new ContextoReacao(atacante, alvo, r.DanoEfetivo, r.Natureza,
                r.Critico, aliadosDoAtacante, inimigosDoAtacante);

            var resultados = ColetarReacoes<IReagePorAtaque>(atacante, x => x.PorAtaque(ctx));

            if (r.DanoEfetivo > 0)
                resultados.AddRange(ColetarReacoes<IReageAoCausarDano>(atacante, x => x.AoCausarDano(ctx)));

            ExibirResultadosReacao(atacante, resultados);
        }

        /// <summary>
        /// Reações do atacante ao EVENTO de atacar (IReageAoAtacar), seguindo o TipoAtaque:
        /// chamado por hit (Sequencial) ou 1x no fim (AoE), lado a lado com ProcessarPassivasAtacante.
        /// Para efeitos no próprio atacante (OlhoClinico, Virus).
        /// </summary>
        private void ProcessarReacoesAtacantePorAtaque(Combate atacante, Combate alvoRef, EventoDano r)
        {
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;

            var (aliadosDoAtacante, inimigosDoAtacante) = _batalha.PerspectivaDe(atacante);
            var ctx = new ContextoReacao(atacante, alvoRef, r.DanoEfetivo, r.Natureza,
                r.Critico, aliadosDoAtacante, inimigosDoAtacante);

            var resultados = ColetarReacoes<IReageAoAtacar>(atacante, x => x.AoAtacar(ctx));
            ExibirResultadosReacao(atacante, resultados);
        }

        /// <summary>
        /// Exibe o que as reações produziram (dano, cura, mensagem). Centraliza a
        /// exibição — as reações declaram, aqui exibe.
        /// </summary>
        private void ExibirResultadosReacao(Combate origem, List<ResultadoReacao> resultados)
        {
            foreach (var res in resultados)
            {
                if (!string.IsNullOrEmpty(res.Mensagem))
                    _tela.ExibirMensagemPassiva(res.Mensagem);

                if (res.Dano != null)
                    _tela.ExibirResultadoAtaque(origem, res.Dano.Alvo, res.Dano);

                if (res.Cura != null)
                    _tela.ExibirCura(res.Cura);   // mesma view da cura de habilidade

                if (res.Mensagem != "" || res.Dano != null || res.Cura != null)
                    Aguardar(Momento.Narracao);
            }
        }

        /// <summary>
        /// Dispara as reações "ao matar" (IReageAoMatar) do atacante, por alvo morto.
        /// Se o prevent-death (Guarda) evitou a morte no ReceberDano, o alvo segue Vivo
        /// e este método retorna sem disparar (checa EstaVivo).
        /// </summary>
        private void ProcessarReacoesAtacanteMorte(Combate atacante, Combate alvoMorto, EventoDano r)
        {
            if (alvoMorto.EstaVivo()) return;
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;

            var (aliadosDoAtacante, inimigosDoAtacante) = _batalha.PerspectivaDe(atacante);
            var ctx = new ContextoReacao(atacante, alvoMorto, r.DanoEfetivo, r.Natureza,
                r.Critico, aliadosDoAtacante, inimigosDoAtacante);

            var resultados = ColetarReacoes<IReageAoMatar>(atacante, x => x.AoMatar(ctx));
            ExibirResultadosReacao(atacante, resultados);
        }

        /// <summary>
        /// Dispara as reações "ao morrer" (IReageAoMorrer) do que MORREU. Chamado
        /// DEPOIS do ProcessarReacoesAtacanteMorte (ao matar), preservando a ordem:
        /// o Vilao bloqueia o revive (ao matar) antes da Necromancia tentar reviver
        /// (ao morrer). Portador = quem morreu; times invertidos (aliados do morto =
        /// time do alvo), como no ProcessarReacoesAlvo.
        /// </summary>
        private void ProcessarReacoesAoMorrer(Combate morto, Combate atacante, EventoDano r)
        {
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;
            if (morto.EstaVivo()) return;  // só dispara se realmente morreu

            // Portador é o MORTO: sua perspectiva vem da Batalha (um só caminho).
            var (aliadosDoMorto, inimigosDoMorto) = _batalha.PerspectivaDe(morto);

            var ctx = new ContextoReacao(morto, atacante, r.DanoEfetivo, r.Natureza,
                r.Critico, aliadosDoMorto, inimigosDoMorto);

            var resultados = ColetarReacoes<IReageAoMorrer>(morto, x => x.AoMorrer(ctx));
            ExibirResultadosReacao(morto, resultados);
        }

        #endregion

        #region Fluxo de fase

        /// <summary>
        /// Põe um time no tabuleiro e abre o combate dele: o ÍNDICE na lista é a casa (1 = frente …
        /// 4 = fundo, GDD §2). A ordem chega intacta do front (`int[] Time` → `List&lt;Personagem&gt;`
        /// → `Membros`), então não há nada a traduzir — só a contagem a partir de 1.
        ///
        /// É o único lugar que chama o <c>IniciarCombate</c>: quem entra em batalha sem passar por
        /// aqui luta fora do tabuleiro e não tem perfil de distância.
        /// </summary>
        private static void Posicionar(IReadOnlyList<Combate> time)
        {
            for (int i = 0; i < time.Count; i++)
                time[i].IniciarCombate(casa: i + 1);
        }

        /// <summary>
        /// A fase da campanha a partir de um time JÁ ESCOLHIDO — quem monta o time é problema de quem
        /// chama (o clique no front). Roda as 2 rodadas com os itens equipados, credita a XP e devolve
        /// o que caiu. A recompensa (unlock/drop/save) é DEPOIS, no CampanhaService.
        ///
        /// <b>O NÚMERO DO CAPÍTULO é o valor do enum <see cref="Faccao"/></b> — Humanos = 0 (não é
        /// capítulo) e os oito capítulos são 1..8, na ordem em que a campanha os percorre. Era assim
        /// que o multiplicador de fase já contava; reordenar o enum move a XP e o nível do inimigo.
        /// </summary>
        public ResultadoDaFase ExecutarFaseComTime(List<Personagem> time, Faccao capitulo, Fases fase,
            Dificuldade dificuldade)
        {
            Fase fas = Campanha.ObterFase((int)fase, dificuldade);
            int nivelDoInimigo = Progressao.NivelDoInimigo(dificuldade, (int)capitulo, (int)fase);

            // A XP CAI POR INIMIGO MORTO, não por vitória: quem matou dois e perdeu leva os dois. O
            // pote da fase é dividido igual entre os inimigos das DUAS rodadas.
            int totalDeInimigos = fas.Rodada1.Count + fas.Rodada2.Count;
            int porInimigo = Progressao.PoteDaFase((int)capitulo, (int)fase, dificuldade) / totalDeInimigos;

            var jogador = time.Select(p => (Combate)new Jogador(p)).ToList();
            foreach (Combate c in jogador)
                _arsenalService.AplicarItens(c);

            Posicionar(jogador);   // captura o HPMaximoInicial DOS JOGADORES depois dos itens

            try
            {
                bool venceu = ExecutarRodada(jogador, fas.Rodada1, capitulo, nivelDoInimigo, out int mortos);
                if (venceu)
                {
                    venceu = ExecutarRodada(jogador, fas.Rodada2, capitulo, nivelDoInimigo, out int daSegunda);
                    mortos += daSegunda;
                }

                _tela.ExibirResumoBatalha(jogador);   // resumo (vitória ou derrota; não em abandono)

                int pote = mortos * porInimigo;
                _progressaoService.Creditar(time, pote);
                return new ResultadoDaFase(venceu ? ResultadoFase.Venceu : ResultadoFase.Perdeu,
                    time.Count == 0 ? 0 : pote / time.Count);
            }
            catch (BatalhaAbortada)
            {
                // Encerrou no meio: derrota, sem recompensa e SEM XP — desistir não paga o que a luta
                // pagaria, senão abandonar vira estratégia de farm.
                return new ResultadoDaFase(ResultadoFase.Perdeu, 0);
            }
        }

        private bool ExecutarRodada(List<Combate> jogador, List<TipoDeApostolo> tiposInimigos,
            Faccao capitulo, int nivelDoInimigo, out int mortos)
        {
            var inimigo = new List<Combate>();
            foreach (TipoDeApostolo tipo in tiposInimigos)
            {
                // CÓPIA no nível da fase, e não a instância do roster: o inimigo da campanha sai do
                // MESMO service que o time do jogador, então nivelar o original vazaria pro elenco
                // dele — sem quebrar build e sem ninguém ver.
                Personagem original = _personagemService.ObterPorTipo(capitulo, tipo);
                inimigo.Add(new Inimigo(original.ComNivel(nivelDoInimigo)));
            }

            Posicionar(inimigo);   // snapshot do HP máximo desta rodada + a casa de cada um

            // A ONDA NOVA COMEÇA EMPATADA. Os inimigos já nascem com a barra em 0 (o Posicionar acima);
            // sem esta linha o time do jogador entraria com o medidor de onde parou na onda anterior,
            // e abriria a 2ª onda com meio ciclo de vantagem que ninguém desenhou.
            foreach (Combate c in jogador) c.ZerarMedidor();

            var batalha = new Batalha(new Equipe(jogador), new Equipe(inimigo));

            // Mostra o board da rodada ANTES de lutar: é o que troca o board pros inimigos NOVOS
            // (sem isto a rodada 2 mostraria os da 1, mortos).
            _tela.ExibirPartida(jogador, inimigo);

            // Campanha: Equipe1 (jogador) = humano, Equipe2 (inimigos) = bot. No Versus, o ponto de
            // entrada monta este mapa conforme o modo escolhido (J×B / B×J / J×J / B×B).
            _controladores = new Dictionary<Equipe, IControladorDeTurno>
            {
                { batalha.Equipe1, _controladorJogador },
                { batalha.Equipe2, _controladorBot },
            };

            bool venceu = ExecutarCombate(batalha);

            // Conta os mortos DEPOIS da rodada, inclusive quando ela foi perdida: matou um e caiu,
            // ganhou o dele.
            mortos = inimigo.Count(c => !c.EstaVivo());
            return venceu;
        }

        /// <summary>
        /// Modo ARENA (laboratório de rebalance): duelo Equipe1 × Equipe2 a partir de times JÁ
        /// ESCOLHIDOS, com controle configurável (bot1/bot2 = cada equipe é bot?). Os dois lados lutam
        /// como estão — sem itens (leitura limpa de balance) e sem recompensa/save.
        /// Reusa o mesmo loop de combate — o seam Batalha/controlador faz tudo funcionar independente
        /// da classe. Ambos os times são Jogador (a estrutura, não o tipo, define quem é inimigo de
        /// quem). A seleção é problema de quem chama: o pick de apóstolos é TELA, e a luta não depende
        /// dela. Esc = sai sem drama.
        /// </summary>
        /// <returns>true = a batalha terminou naturalmente (resumo na tela); false = foi ABORTADA no
        /// meio (Esc/sair) — quem chama deve voltar ao menu sem esperar a tela de resultado.</returns>
        public bool ExecutarArenaComTimes(List<Personagem> time1, List<Personagem> time2, bool bot1, bool bot2)
        {
            var equipe1 = new Equipe(time1.Select(p => (Combate)new Jogador(p)).ToList());
            var equipe2 = new Equipe(time2.Select(p => (Combate)new Jogador(p)).ToList());
            Posicionar(equipe1.Membros);
            Posicionar(equipe2.Membros);

            var batalha = new Batalha(equipe1, equipe2);
            _controladores = new Dictionary<Equipe, IControladorDeTurno>
            {
                { equipe1, bot1 ? _controladorBot : _controladorJogador },
                { equipe2, bot2 ? _controladorBot : _controladorJogador },
            };

            // Arena é PVP: a tela fixa os lados na ordem montada (equipe1=esquerda, equipe2=direita),
            // não importa quem controla.
            _tela.ExibirInicioArena(equipe1.Membros, equipe2.Membros);

            try
            {
                bool venceu1 = ExecutarCombate(batalha);
                _tela.ExibirResumoArena(equipe1.Membros, equipe2.Membros, venceu1);
                return true;
            }
            catch (BatalhaAbortada)
            {
                return false;   // Esc/sair no meio: volta pro menu sem tela de derrota
            }
        }

        #endregion
    }

    /// <summary>
    /// O que sobrou de uma fase: o desfecho e a XP que CADA apóstolo em campo levou. A XP vem junto
    /// porque a fase é o único lugar que sabe quantos inimigos morreram — a tela só mostra o número.
    /// Ela já foi creditada quando este record aparece.
    /// </summary>
    public record ResultadoDaFase(ResultadoFase Resultado, int XpPorApostolo)
    {
        public bool Venceu => Resultado == ResultadoFase.Venceu;
    }
}