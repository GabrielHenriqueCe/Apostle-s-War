using ApostlesWar;
using ApostlesWar.Skills.Ativas;
using ApostlesWar.Skills.Buffs;
using ApostlesWar.Skills.Debuffs;
using ApostlesWar.Skills.Passivas;
using ApostlesWar.View;
using ApostlesWar.Controllers;

namespace ApostlesWar.Services
{
    internal class CombateService
    {
        #region Construtor

        private readonly ArsenalService _arsenalService;
        private readonly CampeoesService _campeoesService;
        private readonly PersonagemService _personagemService;
        private readonly CombateView _combateView;
        private readonly SelecaoDeAlvoService _selecaoDeAlvoService;
        private readonly IControladorDeTurno _controladorJogador;
        private readonly IControladorDeTurno _controladorBot;
        private readonly IApresentacao _apresentacao;

        public CombateService(ArsenalService arsenalService,
            CampeoesService campeoesService, PersonagemService personagemService, CombateView combateView,
            SelecaoDeAlvoService selecaoDeAlvoService, IControladorDeTurno controladorJogador,
            IControladorDeTurno controladorBot, IApresentacao apresentacao)
        {
            _arsenalService = arsenalService;
            _campeoesService = campeoesService;
            _personagemService = personagemService;
            _combateView = combateView;
            _selecaoDeAlvoService = selecaoDeAlvoService;
            _controladorJogador = controladorJogador;
            _controladorBot = controladorBot;
            _apresentacao = apresentacao;
        }

        /// <summary>O controlador que DECIDE ação/alvo deste combatente (humano/bot; futuro: auto).</summary>
        private IControladorDeTurno ControladorDe(Combate combatente)
            => combatente is Jogador ? _controladorJogador : _controladorBot;

        /// <summary>
        /// Espera dramática entre eventos que ESCUTA o Esc: se o jogador pediu pra encerrar e confirmou,
        /// aborta a batalha (BatalhaAbortada, capturada em ExecutarFase → a fase vira derrota). Todas as
        /// esperas do combate passam por aqui — é o ponto único do cancelamento.
        /// </summary>
        private void Aguardar(int ms)
        {
            if (_apresentacao.AguardarAnimacao(ms) && _combateView.ConfirmarEncerramento())
                throw new BatalhaAbortada();
        }

        #endregion

        #region Loop principal

        private bool ExecutarCombate(List<Combate> jogador, List<Combate> inimigo, List<Combate> combatentes)
        {
            do
            {
                for (int c = 0; c < combatentes.Count; c++)
                {
                    if (!combatentes[c].EstaVivo()) continue;
                    if (!inimigo.Any(i => i.EstaVivo()) || !jogador.Any(j => j.EstaVivo())) break;

                    ExecutarTurnoCompleto(combatentes[c], jogador, inimigo);

                    // Turno extra: dispara enquanto a flag estiver setada.
                    // Loop teórico infinito é permitido por design (RNG decide quando para).
                    while (combatentes[c].TemTurnoExtra)
                    {
                        combatentes[c].ConsumirTurnoExtra();
                        if (!combatentes[c].EstaVivo()) break;
                        if (!inimigo.Any(i => i.EstaVivo()) || !jogador.Any(j => j.EstaVivo())) break;
                        ExecutarTurnoCompleto(combatentes[c], jogador, inimigo);
                    }
                }
            } while (jogador.Any(j => j.EstaVivo()) && inimigo.Any(i => i.EstaVivo()));

            return jogador.Any(j => j.EstaVivo());
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
        private void ExecutarTurnoCompleto(Combate atacante, List<Combate> jogador, List<Combate> inimigo)
        {
            List<Combate> aliados = atacante is Jogador ? jogador : inimigo;
            List<Combate> defensores = atacante is Jogador ? inimigo : jogador;

            var turno = new TurnoDoPersonagem(atacante);

            var ticks = turno.Iniciar();
            MostrarTicks(jogador, inimigo, ticks);   // veneno/queima/cura-contínua VISÍVEIS no início do turno
            DispararEventoInicioDeTurno(atacante, aliados, defensores);
            if (!atacante.EstaVivo()) return;

            if (atacante.StatusAtivos.Any(s => s is Preso))
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

            _combateView.LimparTela();
            _combateView.ExibirPartida(jogador, inimigo);
            foreach (var ev in ticks)
            {
                if (ev is EventoDano d) _combateView.ExibirDanoDeStatus(d);
                else if (ev is EventoCura c) _combateView.ExibirCura(c);
                Aguardar(1500);
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
            _combateView.LimparTela();

            // Irritar: força A1 automático no aplicador (mas Medo pode interromper)
            var irritar = atacante.StatusAtivos.OfType<Irritar>().FirstOrDefault();
            if (irritar != null)
            {
                _combateView.ExibirPartida(aliados, defensores);
                _combateView.ExibirMensagemPassiva(
                    $"{atacante.Personagem.Simbolo} está irritado e ataca {irritar.Aplicador.Personagem.Simbolo} automaticamente!");
                Aguardar(1500);

                if (VerificarMedoEAplicar(atacante)) return;

                var resultado = atacante.Atacar(irritar.Aplicador);
                ExecutarAtos(new List<EventoCombate> { resultado }, atacante, aliados, defensores, TipoAtaque.Sequencial);
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
        /// Verifica se o portador tem Medo e se trigga.
        /// Retorna true se a ação foi paralizada (chamador deve abortar).
        /// </summary>
        private bool VerificarMedoEAplicar(Combate atacante)
        {
            var medo = atacante.StatusAtivos.OfType<Medo>().FirstOrDefault();
            if (medo == null) return false;
            if (!medo.TentaParalizar()) return false;

            _combateView.ExibirMensagemPassiva(
                $"{atacante.Personagem.Simbolo} {atacante.Personagem.Nome} estava com medo e não conseguiu agir!");
            Aguardar(1500);
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
            if (hab.TipoLista == TipoLista.Inimigos)
            {
                var disponiveis = _selecaoDeAlvoService.ResolverAlvosDisponiveis(defensores);
                return controlador.EscolherAlvo(disponiveis, aliados, defensores);
            }

            if (hab.TipoLista == TipoLista.Aliados && hab.NumeroDeAlvos != int.MaxValue)
            {
                // Pick real de alvo aliado (por estado). Sem candidato no estado pedido (ex: revive
                // sem mortos): pula o pick — ResolverAlvos devolve vazio pra ação que herda o alvo, e
                // as demais ações (escopos próprios) rodam normalmente (DocesDeAbobora sem mortos
                // ainda vale pelo Reflexo).
                var disponiveis = _selecaoDeAlvoService.ResolverAlvosDisponiveis(aliados, hab.EstadoAlvo);
                return disponiveis.Count == 0 ? atacante : controlador.EscolherAlvo(disponiveis, aliados, defensores);
            }

            return atacante; // hit-all (NumeroDeAlvos=MaxValue) ou próprio: a habilidade resolve sozinha
        }

        /// <summary>
        /// Roda os Atos de reação sobre cada resultado produzido pelo AtoExecucao.
        /// Ordem do ADR: AtoReacaoDoAlvo → AtoMorte → AtoReacaoDoAtacante.
        /// Compartilhado pelo caminho normal (ExecutarHabilidade) e pelo Irritar.
        /// </summary>
        private void ExecutarAtos(List<EventoCombate> resultados, Combate atacante,
            List<Combate> aliados, List<Combate> defensores, TipoAtaque tipoAtaque)
        {
            foreach (var ev in resultados)
            {
                // Cura é irmã do dano no stream, mas só EXIBE — não dispara reação de dano.
                if (ev is EventoCura cura)
                {
                    _combateView.ExibirCura(cura);
                    Aguardar(1500);
                    continue;
                }

                var r = (EventoDano)ev;
                _combateView.ExibirResultadoAtaque(atacante, r.Alvo, r);
                Aguardar(1500);

                ProcessarReacoesAlvo(r.Alvo, atacante, r, aliados, defensores);
                ProcessarReacoesAntesDeMorrer(r.Alvo, atacante, r, aliados, defensores);
                ProcessarReacoesAtacanteMorte(atacante, r.Alvo, r, aliados, defensores);
                ProcessarReacoesAoMorrer(r.Alvo, atacante, r, aliados, defensores);
                ProcessarReacoesAtacantePorAlvo(atacante, r.Alvo, r, aliados, defensores);

                if (tipoAtaque == TipoAtaque.Sequencial)
                    ProcessarReacoesAtacantePorAtaque(atacante, r.Alvo, r, aliados, defensores);
            }

            var danos = resultados.OfType<EventoDano>().ToList();
            if (tipoAtaque == TipoAtaque.AreaDeEfeito && danos.Count > 0)
                ProcessarReacoesAtacantePorAtaque(atacante, danos[0].Alvo, danos[0], aliados, defensores);
        }

        private void ExecutarHabilidade(Combate atacante, HabilidadeAtiva hab, Combate alvoInicial,
            List<Combate> defensores, List<Combate> aliados)
        {
            var ctx = new ContextoCombate(atacante, aliados, defensores);

            // Setup: UX de preparação (inimigo com A1)
            if (atacante is Inimigo && hab is AtaqueBasico)
            {
                _combateView.ExibirPreparacaoAtaque(atacante, defensores);
                Aguardar(1500);
            }

            // Setup: Medo trigga DEPOIS da escolha (jogador escolhe, vê o medo, perde cooldown)
            if (VerificarMedoEAplicar(atacante))
            {
                atacante.Cooldowns[hab].Usar();
                return;
            }

            // Os Atos
            var resultados = hab.Ativar(ctx, alvoInicial);                             // AtoExecucao

            // Nome da habilidade PRIMEIRO — "X usou {hab}!" antes dos resultados (narrativa).
            if (hab is not AtaqueBasico)
            {
                _combateView.ExibirUsoHabilidade(atacante, hab);
                Aguardar(1500);
            }

            ExecutarAtos(resultados, atacante, aliados, defensores, hab.TipoAtaque);   // Reação + Morte + Atacante
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
        private void ProcessarReacoesAlvo(Combate alvo, Combate atacante, EventoDano r,
            List<Combate> aliadosDoAtacante, List<Combate> inimigosDoAtacante, int profundidade = 0)
        {
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;

            // Portador da reação é o ALVO: inverte os times (como ProcessarPassivasAlvo).
            var aliadosDoAlvo = inimigosDoAtacante;
            var inimigosDoAlvo = aliadosDoAtacante;

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
                _combateView.ExibirResultadoAtaque(alvo, revide.Alvo, revide);
                Aguardar(1500);
                // No revide, o portador do próximo nível é o revidado; passa os times do
                // ponto de vista atual (alvo é quem revida agora → seus aliados/inimigos).
                ProcessarReacoesAlvo(res.Revide.Alvo, alvo, revide, inimigosDoAtacante, aliadosDoAtacante, profundidade + 1);
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
        /// ATENÇÃO (contrato, auditoria jul/2026): o consumo acontece AO COLETAR, antes de
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
        private void ProcessarReacoesAtacantePorAlvo(Combate atacante, Combate alvo, EventoDano r,
            List<Combate> aliadosDoAtacante, List<Combate> inimigosDoAtacante)
        {
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;

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
        private void ProcessarReacoesAtacantePorAtaque(Combate atacante, Combate alvoRef, EventoDano r,
            List<Combate> aliadosDoAtacante, List<Combate> inimigosDoAtacante)
        {
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;

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
                    _combateView.ExibirMensagemPassiva(res.Mensagem);

                if (res.Dano != null)
                    _combateView.ExibirResultadoAtaque(origem, res.Dano.Alvo, res.Dano);

                // Cura: exibição a definir num sub-PR (depende de método na CombateView).
                // C1 não tem implementador de cura ainda, então fica como TODO.

                if (res.Mensagem != "" || res.Dano != null)
                    Aguardar(1500);
            }
        }

        /// <summary>
        /// Intervém ANTES das consequências da morte (IReageAntesDeMorrer). Dispara se
        /// o alvo morreu; a passiva pode chamar AplicarRevive para reverter a transição —
        /// se isso acontecer, ProcessarReacoesAtacanteMorte e AoMorrer não disparam
        /// (ambos checam EstaVivo() antes de agir).
        /// Portador = quem quase morreu; Contraparte = quem matou.
        /// </summary>
        private void ProcessarReacoesAntesDeMorrer(Combate alvo, Combate atacante, EventoDano r,
            List<Combate> aliadosDoAtacante, List<Combate> inimigosDoAtacante)
        {
            if (alvo.EstaVivo()) return;
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;

            var aliadosDoAlvo = inimigosDoAtacante;
            var inimigosDoAlvo = aliadosDoAtacante;

            var ctx = new ContextoReacao(alvo, atacante, r.DanoEfetivo, r.Natureza,
                r.Critico, aliadosDoAlvo, inimigosDoAlvo);

            var resultados = ColetarReacoes<IReageAntesDeMorrer>(alvo, x => x.AntesDeMorrer(ctx));
            ExibirResultadosReacao(alvo, resultados);
        }

        /// <summary>
        /// Dispara as reações "ao matar" (IReageAoMatar) do atacante, por alvo morto.
        /// Chamado DEPOIS de IReageAntesDeMorrer — se a Guarda reverteu a morte, o alvo
        /// voltou a EstaVivo() e este método retorna sem disparar.
        /// </summary>
        private void ProcessarReacoesAtacanteMorte(Combate atacante, Combate alvoMorto, EventoDano r,
            List<Combate> aliadosDoAtacante, List<Combate> inimigosDoAtacante)
        {
            if (alvoMorto.EstaVivo()) return;
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;

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
        private void ProcessarReacoesAoMorrer(Combate morto, Combate atacante, EventoDano r,
            List<Combate> aliadosDoAtacante, List<Combate> inimigosDoAtacante)
        {
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;
            if (morto.EstaVivo()) return;  // só dispara se realmente morreu

            var aliadosDoMorto = inimigosDoAtacante;
            var inimigosDoMorto = aliadosDoAtacante;

            var ctx = new ContextoReacao(morto, atacante, r.DanoEfetivo, r.Natureza,
                r.Critico, aliadosDoMorto, inimigosDoMorto);

            var resultados = ColetarReacoes<IReageAoMorrer>(morto, x => x.AoMorrer(ctx));
            ExibirResultadosReacao(morto, resultados);
        }

        #endregion

        #region Fluxo de fase

        public ResultadoFase ExecutarFase(Faccao capitulo, Fases fase)
        {
            Fase fas = Campanha.ObterFase((int)fase);
            MultiplicadorFase mult = new MultiplicadorFase
            {
                HP = (0.5f * (float)capitulo) + (0.1f * (float)fase),
                Ataque = (0.5f * (float)capitulo) + (0.1f * (float)fase),
                Defesa = (0.5f * (float)capitulo) + (0.1f * (float)fase)
            };

            var time = _campeoesService.SelecionarTime();
            if (time.Count == 0) return ResultadoFase.Cancelou;   // desistiu na seleção de time — sem derrota

            var jogador = time.Select(p => (Combate)new Jogador(p)).ToList();
            foreach (Combate c in jogador)
                _arsenalService.AplicarItens(c);

            // NOVO: captura HPMaximoInicial DOS JOGADORES depois de mult + itens
            // (jogadores não recebem multiplicador de fase, mas mantemos pra simetria/consistência)
            foreach (Combate c in jogador)
                c.IniciarCombate();

            try
            {
                bool venceu = ExecutarRodada(jogador, fas.Rodada1, capitulo, mult)
                           && ExecutarRodada(jogador, fas.Rodada2, capitulo, mult);
                _combateView.ExibirResumoBatalha(jogador);   // resumo (vitória ou derrota; não em abandono)
                return venceu ? ResultadoFase.Venceu : ResultadoFase.Perdeu;
            }
            catch (BatalhaAbortada)
            {
                return ResultadoFase.Perdeu;   // encerrou a batalha no meio → derrota, sem recompensa
            }
        }

        private bool ExecutarRodada(List<Combate> jogador, List<Slot> slotsInimigos, Faccao capitulo, MultiplicadorFase mult)
        {
            var inimigo = new List<Combate>();
            foreach (Slot slot in slotsInimigos)
            {
                var novoInimigo = new Inimigo(_personagemService.ObterPersonagem(capitulo, slot), mult);
                novoInimigo.IniciarCombate();  // NOVO: snapshot do HP máximo do inimigo nesta rodada
                inimigo.Add(novoInimigo);
            }

            var combatentes = new List<Combate>();
            combatentes.AddRange(jogador);
            combatentes.AddRange(inimigo);

            return ExecutarCombate(jogador, inimigo, combatentes);
        }

        #endregion
    }
}