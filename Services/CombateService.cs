using ApostlesWar;
using GHUtils;
using ApostlesWar.Skills.Ativas;
using ApostlesWar.Skills.Buffs;
using ApostlesWar.Skills.Debuffs;
using ApostlesWar.Skills.Passivas;

namespace ApostlesWar.Services
{
    internal class CombateService
    {
        #region Construtor

        private readonly ArsenalService _arsenalService;
        private readonly CampanhaService _campanhaService;
        private readonly CampeoesService _campeoesService;
        private readonly PersonagemService _personagemService;
        private readonly MenuService _menuService;
        private readonly SelecaoDeAlvoService _selecaoDeAlvoService;

        public CombateService(ArsenalService arsenalService, CampanhaService campanhaService,
            CampeoesService campeoesService, PersonagemService personagemService, MenuService menuService, SelecaoDeAlvoService selecaoDeAlvoService    )
        {
            _arsenalService = arsenalService;
            _campanhaService = campanhaService;
            _campeoesService = campeoesService;
            _personagemService = personagemService;
            _menuService = menuService;
            _selecaoDeAlvoService = selecaoDeAlvoService;
        }

        #endregion

        #region Estrutura de ação

        private record AcaoEscolhida(HabilidadeAtiva Habilidade);
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

            turno.Iniciar();
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
        /// efeitos que o portador aplica a cada turno.
        /// </summary>
        private void ProcessarReacoesInicioTurno(Combate combatente, ContextoCombate ctxCombate)
        {
            var resultados = new List<ResultadoReacao>();

            foreach (var p in ColetarPassivasReativas<IReageAoInicioTurno>(combatente))
                resultados.AddRange(p.AoInicioTurno(ctxCombate));

            ExibirResultadosReacao(combatente, resultados);
        }

        #endregion

        #region Turno

        private void ExecutarTurno(Combate atacante, List<Combate> defensores, List<Combate> aliados)
        {
            Console.Clear();

            // Irritar: força A1 automático no aplicador (mas Medo pode interromper)
            var irritar = atacante.StatusAtivos.OfType<Irritar>().FirstOrDefault();
            if (irritar != null)
            {
                _menuService.ExibirPartida(aliados, defensores);
                _menuService.ExibirMensagemPassiva(
                    $"{atacante.Personagem.Simbolo} está irritado e ataca {irritar.Aplicador.Personagem.Simbolo} automaticamente!");
                Thread.Sleep(1500);

                if (VerificarMedoEAplicar(atacante)) return;

                var resultado = atacante.Atacar(irritar.Aplicador);
                ExecutarAtos(new List<EventoDano> { resultado }, atacante, aliados, defensores, TipoAtaque.Sequencial);
                return;
            }

            AcaoEscolhida acao = atacante is Jogador
                ? EscolherAcaoJogador(atacante, defensores, aliados)
                : EscolherAcaoInimigo(atacante);

            ExecutarAcao(atacante, acao, defensores, aliados);
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

            _menuService.ExibirMensagemPassiva(
                $"{atacante.Personagem.Simbolo} {atacante.Personagem.Nome} estava com medo e não conseguiu agir!");
            Thread.Sleep(1500);
            return true;
        }

        #endregion

        #region Escolha de ação

        private AcaoEscolhida EscolherAcaoJogador(Combate atacante, List<Combate> defensores, List<Combate> aliados)
        {
            var habilidadesAtivas = atacante.Personagem.Habilidades.OfType<HabilidadeAtiva>().ToList();
            int totalOpcoes = habilidadesAtivas.Count;
            int acao = 1;

            while (true)
            {
                Console.Clear();
                _menuService.ExibirPartida(aliados, defensores);
                _menuService.ExibirAcoes(atacante, acao);

                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;

                int novaAcao = ConsoleUtils.SelecionarComCursor(acao, 1, totalOpcoes, key.Key);
                bool descendo = key.Key == ConsoleKey.S || key.Key == ConsoleKey.DownArrow;

                // Pula habilidades em cooldown. A1 (AtaqueBasico) tem cooldown 0 sempre,
                // então nunca é pulada — a A1 é sempre selecionável.
                while (novaAcao != acao)
                {
                    var hab = habilidadesAtivas[novaAcao - 1];
                    if (atacante.Cooldowns[hab].Disponivel) break;

                    int proximo = descendo ? novaAcao + 1 : novaAcao - 1;
                    if (proximo < 1 || proximo > totalOpcoes)
                    {
                        novaAcao = acao;
                        break;
                    }
                    novaAcao = proximo;
                }

                acao = novaAcao;
            }

            return new AcaoEscolhida(habilidadesAtivas[acao - 1]);
        }

        private AcaoEscolhida EscolherAcaoInimigo(Combate atacante)
        {
            // Inimigo usa sempre AtaqueBasico (A1) por enquanto.
            // Futuro: bot escolherá entre habilidades disponíveis (modo automático).
            var ataqueBasico = atacante.Personagem.Habilidades.OfType<AtaqueBasico>().First();
            return new AcaoEscolhida(ataqueBasico);
        }

        #endregion

        #region Execução

        private void ExecutarAcao(Combate atacante, AcaoEscolhida acao, List<Combate> defensores, List<Combate> aliados)
        {
            ExecutarHabilidade(atacante, acao.Habilidade, defensores, aliados);
        }

        /// <summary>
        /// Roda os Atos de reação sobre cada resultado produzido pelo AtoExecucao.
        /// Ordem do ADR: AtoReacaoDoAlvo → AtoMorte → AtoReacaoDoAtacante.
        /// Compartilhado pelo caminho normal (ExecutarHabilidade) e pelo Irritar.
        /// </summary>
        private void ExecutarAtos(List<EventoDano> resultados, Combate atacante,
            List<Combate> aliados, List<Combate> defensores, TipoAtaque tipoAtaque)
        {
            foreach (var r in resultados)
            {
                _menuService.ExibirResultadoAtaque(atacante, r.Alvo, r);
                Thread.Sleep(1500);

                ProcessarReacoesAlvo(r.Alvo, atacante, r, aliados, defensores);
                ProcessarReacoesAntesDeMorrer(r.Alvo, atacante, r, aliados, defensores);
                ProcessarReacoesAtacanteMorte(atacante, r.Alvo, r, aliados, defensores);
                ProcessarReacoesAoMorrer(r.Alvo, atacante, r, aliados, defensores);
                ProcessarReacoesAtacantePorAlvo(atacante, r.Alvo, r, aliados, defensores);

                if (tipoAtaque == TipoAtaque.Sequencial)
                    ProcessarReacoesAtacantePorAtaque(atacante, r.Alvo, r, aliados, defensores);
            }

            if (tipoAtaque == TipoAtaque.AreaDeEfeito && resultados.Count > 0)
                ProcessarReacoesAtacantePorAtaque(atacante, resultados[0].Alvo, resultados[0], aliados, defensores);
        }

        private void ExecutarHabilidade(Combate atacante, HabilidadeAtiva hab, List<Combate> defensores, List<Combate> aliados)
        {
            var ctx = new ContextoCombate(atacante, aliados, defensores);

            // Setup: seleção de alvo
            Combate alvoInicial;
            if (hab.TipoLista == TipoLista.Inimigos)
            {
                var disponiveis = _selecaoDeAlvoService.ResolverAlvosDisponiveis(defensores);
                alvoInicial = atacante is Jogador
                    ? _menuService.EscolherAlvoNaTela(disponiveis, aliados, defensores)
                    : _selecaoDeAlvoService.EscolherAlvoBot(disponiveis);
            }
            else if (hab.TipoLista == TipoLista.Aliados && hab.NumeroDeAlvos != int.MaxValue && hab.EstadoAlvo != EstadoAlvo.Ambos)
            {
                // Pick real de alvo aliado (por estado) — antes disso nunca existia,
                // porque toda habilidade de aliado mirava o time inteiro.
                // Sem candidato no estado pedido (ex: revive sem mortos): pula o pick —
                // ResolverAlvos devolve vazio pra ação que herda o alvo, e as demais ações
                // da habilidade (escopos próprios) rodam normalmente (DocesDeAbobora sem
                // mortos ainda vale pelo Reflexo).
                var disponiveis = _selecaoDeAlvoService.ResolverAlvosDisponiveis(aliados, hab.EstadoAlvo);
                alvoInicial = disponiveis.Count == 0 ? atacante
                    : atacante is Jogador
                        ? _menuService.EscolherAlvoNaTela(disponiveis, aliados, defensores)
                        : _selecaoDeAlvoService.EscolherAlvoBot(disponiveis);
            }
            else alvoInicial = atacante; // hit-all (NumeroDeAlvos=MaxValue) ou Ambos: a própria habilidade resolve

            // Setup: UX de preparação (inimigo com A1)
            if (atacante is Inimigo && hab is AtaqueBasico)
            {
                _menuService.ExibirPreparacaoAtaque(atacante, defensores);
                Thread.Sleep(1500);
            }

            // Setup: Medo trigga DEPOIS da escolha (jogador escolhe, vê o medo, perde cooldown)
            if (VerificarMedoEAplicar(atacante))
            {
                atacante.Cooldowns[hab].Usar();
                return;
            }

            // Os Atos
            var resultados = hab.Ativar(ctx, alvoInicial);                             // AtoExecucao
            ExecutarAtos(resultados, atacante, aliados, defensores, hab.TipoAtaque);   // Reação + Morte + Atacante
            atacante.Cooldowns[hab].Usar();                                            // AtoEncerramento
            if (hab is not AtaqueBasico)
            {
                _menuService.ExibirUsoHabilidade(atacante, hab);
                Thread.Sleep(2500);
            }
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
            {
                foreach (var s in alvo.StatusAtivos.OfType<IReageAoReceberDano>().ToList())
                    resultados.AddRange(s.AoReceberDano(ctx));
                foreach (var p in ColetarPassivasReativas<IReageAoReceberDano>(alvo))
                    resultados.AddRange(p.AoReceberDano(ctx));
            }

            foreach (var s in alvo.StatusAtivos.OfType<IReageAoSerAtacado>().ToList())
                resultados.AddRange(s.AoSerAtacado(ctx));
            foreach (var p in ColetarPassivasReativas<IReageAoSerAtacado>(alvo))
                resultados.AddRange(p.AoSerAtacado(ctx));

            ExibirResultadosReacao(alvo, resultados);

            foreach (var res in resultados)
            {
                if (res.Revide == null) continue;
                if (profundidade > 0) continue;
                if (!alvo.EstaVivo()) break;
                if (!res.Revide.Alvo.EstaVivo()) continue;

                var revide = res.Revide.Habilidade.AtivarComNatureza(alvo, res.Revide.Alvo, NaturezasDano.Ataque);
                _menuService.ExibirResultadoAtaque(alvo, revide.Alvo, revide);
                Thread.Sleep(1500);
                // No revide, o portador do próximo nível é o revidado; passa os times do
                // ponto de vista atual (alvo é quem revida agora → seus aliados/inimigos).
                ProcessarReacoesAlvo(res.Revide.Alvo, alvo, revide, inimigosDoAtacante, aliadosDoAtacante, profundidade + 1);
            }
        }

        /// <summary>
        /// Coleta as passivas do combatente que implementam a interface de reação T,
        /// respeitando o cooldown (passivas têm Cooldowns; buffs/status não). Consome o
        /// cooldown ao coletar — a passiva "usou" sua reação neste disparo. Passivas com
        /// cooldown 0 (a maioria das reativas) estão sempre disponíveis.
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
            var resultados = new List<ResultadoReacao>();

            foreach (var s in atacante.StatusAtivos.OfType<IReagePorAtaque>().ToList())
                resultados.AddRange(s.PorAtaque(ctx));
            foreach (var p in ColetarPassivasReativas<IReagePorAtaque>(atacante))
                resultados.AddRange(p.PorAtaque(ctx));

            if (r.DanoEfetivo > 0)
            {
                foreach (var s in atacante.StatusAtivos.OfType<IReageAoCausarDano>().ToList())
                    resultados.AddRange(s.AoCausarDano(ctx));
                foreach (var p in ColetarPassivasReativas<IReageAoCausarDano>(atacante))
                    resultados.AddRange(p.AoCausarDano(ctx));
            }

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
            var resultados = new List<ResultadoReacao>();

            foreach (var s in atacante.StatusAtivos.OfType<IReageAoAtacar>().ToList())
                resultados.AddRange(s.AoAtacar(ctx));
            foreach (var p in ColetarPassivasReativas<IReageAoAtacar>(atacante))
                resultados.AddRange(p.AoAtacar(ctx));

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
                    _menuService.ExibirMensagemPassiva(res.Mensagem);

                if (res.Dano != null)
                    _menuService.ExibirResultadoAtaque(origem, res.Dano.Alvo, res.Dano);

                // Cura: exibição a definir num sub-PR (depende de método no MenuService).
                // C1 não tem implementador de cura ainda, então fica como TODO.

                if (res.Mensagem != "" || res.Dano != null)
                    Thread.Sleep(1500);
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
            var resultados = new List<ResultadoReacao>();

            foreach (var s in alvo.StatusAtivos.OfType<IReageAntesDeMorrer>().ToList())
                resultados.AddRange(s.AntesDeMorrer(ctx));
            foreach (var p in ColetarPassivasReativas<IReageAntesDeMorrer>(alvo))
                resultados.AddRange(p.AntesDeMorrer(ctx));

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
            var resultados = new List<ResultadoReacao>();

            foreach (var s in atacante.StatusAtivos.OfType<IReageAoMatar>().ToList())
                resultados.AddRange(s.AoMatar(ctx));
            foreach (var p in ColetarPassivasReativas<IReageAoMatar>(atacante))
                resultados.AddRange(p.AoMatar(ctx));

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
            var resultados = new List<ResultadoReacao>();

            foreach (var s in morto.StatusAtivos.OfType<IReageAoMorrer>().ToList())
                resultados.AddRange(s.AoMorrer(ctx));

            foreach (var p in ColetarPassivasReativas<IReageAoMorrer>(morto))
                resultados.AddRange(p.AoMorrer(ctx));

            ExibirResultadosReacao(morto, resultados);
        }

        #endregion

        #region Fluxo de fase

        public bool ExecutarFase(Faccao capitulo, Fases fase)
        {
            Fase fas = _campanhaService.ObterFase((int)fase);
            MultiplicadorFase mult = new MultiplicadorFase
            {
                HP = (0.5f * (float)capitulo) + (0.1f * (float)fase),
                Ataque = (0.5f * (float)capitulo) + (0.1f * (float)fase),
                Defesa = (0.5f * (float)capitulo) + (0.1f * (float)fase)
            };

            var time = _campeoesService.SelecionarTime();
            if (time.Count == 0) return false;

            var jogador = time.Select(p => (Combate)new Jogador(p)).ToList();
            foreach (Combate c in jogador)
                _arsenalService.AplicarItens(c);

            // NOVO: captura HPMaximoInicial DOS JOGADORES depois de mult + itens
            // (jogadores não recebem multiplicador de fase, mas mantemos pra simetria/consistência)
            foreach (Combate c in jogador)
                c.IniciarCombate();

            if (!ExecutarRodada(jogador, fas.Rodada1, capitulo, mult)) return false;
            return ExecutarRodada(jogador, fas.Rodada2, capitulo, mult);
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