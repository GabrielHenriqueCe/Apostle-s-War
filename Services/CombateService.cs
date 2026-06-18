using ApostlesWar;
using GHUtils;
using v1_Apostle_s_War.Skills.Ativas;
using v1_Apostle_s_War.Skills.Buffs;
using v1_Apostle_s_War.Skills.Debuffs;
using v1_Apostle_s_War.Skills.Passivas;

namespace v1_Apostle_s_War.Services
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
        /// - Dispara passivas com EventoCombate.InicioDoTurno (PassivaGenio reaplica RefletirDano)
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
        /// Dispara o evento InicioDoTurno das passivas (ex: PassivaGenio reaplica
        /// RefletirDano). Fica no service porque depende do sistema de passivas
        /// (DeveAtivar/enum), que será migrado para interfaces no C5 — quando isso
        /// acontecer, reavaliar se o disparo vai para o Turno.
        /// O tick dos status já foi disparado por TurnoDoPersonagem.Iniciar().
        /// </summary>
        private void DispararEventoInicioDeTurno(Combate combatente, List<Combate> aliados, List<Combate> inimigos)
        {
            if (!combatente.EstaVivo()) return;

            var ctxPassiva = new ContextoPassiva(combatente.EstaVivo(), false, aliados, combatente);
            var ctxCombate = new ContextoCombate(combatente, aliados, inimigos);

            DispararEvento(EventoCombate.InicioDoTurno, combatente, combatente, ctxPassiva, ctxCombate);
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
                _menuService.ExibirResultadoAtaque(atacante, irritar.Aplicador, resultado);
                Thread.Sleep(1500);

                // Mesma ordem de ExecutarHabilidade: DepoisDeMatar antes de DepoisDeReceberDano.
                ProcessarPassivasAtacanteMorte(atacante, irritar.Aplicador, aliados, defensores);
                ProcessarReacoesAtacanteMorte(atacante, irritar.Aplicador, resultado);
                ProcessarPassivasAlvo(irritar.Aplicador, atacante, aliados, defensores, resultado.Critico);
                ProcessarPassivasAtacante(atacante, irritar.Aplicador, aliados, defensores);
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

        private void ExecutarHabilidade(Combate atacante, HabilidadeAtiva hab, List<Combate> defensores, List<Combate> aliados)
        {
            var ctx = new ContextoCombate(atacante, aliados, defensores);

            Combate alvoInicial;
            if (hab.TipoLista == TipoLista.Inimigos)
            {
                var alvosDisponiveis = _selecaoDeAlvoService.ResolverAlvosDisponiveis(defensores);
                alvoInicial = atacante is Jogador
                    ? _menuService.EscolherAlvoNaTela(alvosDisponiveis, aliados, defensores)
                    : _selecaoDeAlvoService.EscolherAlvoBot(alvosDisponiveis);
            }
            else
            {
                alvoInicial = atacante;
            }

            // Inimigo "prepara o ataque" antes (UX) — só pra ataque básico do bot.
            if (atacante is Inimigo && hab is AtaqueBasico)
            {
                _menuService.ExibirPreparacaoAtaque(atacante, defensores);
                Thread.Sleep(1500);
            }

            // Medo trigga DEPOIS da escolha (jogador escolhe, vê o medo, perde cooldown)
            if (VerificarMedoEAplicar(atacante))
            {
                atacante.Cooldowns[hab].Usar();
                return;
            }

            var resultados = hab.Ativar(ctx, alvoInicial);

            foreach (var r in resultados)
            {
                _menuService.ExibirResultadoAtaque(atacante, r.Alvo, r);
                Thread.Sleep(1500);

                ProcessarPassivasAtacanteMorte(atacante, r.Alvo, aliados, defensores);
                ProcessarReacoesAtacanteMorte(atacante, r.Alvo, r);
                ProcessarPassivasAlvo(r.Alvo, atacante, aliados, defensores, r.Critico);

                ProcessarReacoesAlvo(r.Alvo, atacante, r);
                ProcessarReacoesAtacantePorAlvo(atacante, r.Alvo, r);

                if (hab.TipoAtaque == TipoAtaque.Sequencial)
                {
                    ProcessarPassivasAtacante(atacante, r.Alvo, aliados, defensores);
                    ProcessarReacoesAtacantePorAtaque(atacante, r.Alvo, r);
                }
            }

            if (hab.TipoAtaque == TipoAtaque.AreaDeEfeito && resultados.Count > 0)
            {
                ProcessarPassivasAtacante(atacante, resultados[0].Alvo, aliados, defensores);
                ProcessarReacoesAtacantePorAtaque(atacante, resultados[0].Alvo, resultados[0]);
            }

            atacante.Cooldowns[hab].Usar();

            // Mensagem de uso só pra habilidades não-A1 (A1 já tem ExibirResultadoAtaque)
            if (hab is not AtaqueBasico)
            {
                _menuService.ExibirUsoHabilidade(atacante, hab);
                Thread.Sleep(2500);
            }
        }

        #endregion

        #region Passivas e status

        /// <summary>
        /// Processa passivas reativas do alvo (quem levou o golpe).
        /// Do ponto de vista do alvo da passiva:
        /// - ctx.Atacante = portador da passiva (alvo do golpe original)
        /// - ctx.Aliados = time do portador
        /// - ctx.Inimigos = time inimigo COMPLETO (não só quem golpeou)
        /// "alvo" do parâmetro = quem golpeou (referência ao atacante original).
        /// </summary>
        private void ProcessarPassivasAlvo(Combate alvo, Combate atacante, List<Combate> aliadosDoAtacante, List<Combate> inimigosDoAtacante, bool foiCritico)
        {
            var aliadosDoAlvo = inimigosDoAtacante;
            var inimigosDoAlvo = aliadosDoAtacante;

            var ctxPassiva = new ContextoPassiva(alvo.EstaVivo(), foiCritico, aliadosDoAlvo, atacante);
            var ctxCombate = new ContextoCombate(alvo, aliadosDoAlvo, inimigosDoAlvo);

            // DepoisDeSerAtacado: dispara sempre (mesmo se Escudo/Bloqueio absorveu).
            // DepoisDeReceberDano: passivas decidem via DeveAtivar (Necromancia/Guarda
            // checam !ctx.AlvoVivo; Alien/Sushiman precisariam de dano > 0, mas o
            // filtro de dano efetivo fica na escolha do evento, nao em guard manual).
            ExecutarPassivasReativas(alvo, atacante, ctxPassiva, ctxCombate, EventoCombate.DepoisDeSerAtacado);
            ExecutarPassivasReativas(alvo, atacante, ctxPassiva, ctxCombate, EventoCombate.DepoisDeReceberDano);
        }

        private void ExecutarPassivasReativas(Combate alvo, Combate atacante, ContextoPassiva ctxPassiva, ContextoCombate ctxCombate, EventoCombate evento)
        {
            foreach (Habilidade hab in alvo.Personagem.Habilidades)
            {
                if (hab is not HabilidadePassiva passiva) continue;
                if (!passiva.DeveAtivar(evento, ctxPassiva)) continue;
                if (!alvo.Cooldowns[hab].Disponivel) continue;

                var resultados = passiva.Ativar(ctxCombate, atacante);
                foreach (var r in resultados)
                    _menuService.ExibirResultadoAtaque(alvo, r.Alvo, r);

                alvo.Cooldowns[hab].Usar();

                // passiva.Revive() retorna a CAPACIDADE de reviver, nao o resultado.
                // Necromancia/Guarda podem ter capacidade mas terem sido bloqueadas
                // (PodeReviver = false aplicado por Vilao/Barata antes). Combinamos
                // capacidade + estado real do alvo pra decidir a mensagem.
                bool reviveuDeFato = passiva.Revive() && alvo.EstaVivo();
                string msg = reviveuDeFato
                    ? passiva.MensagemSobreviveu(alvo.Personagem)
                    : passiva.MensagemMorreu(alvo.Personagem);

                if (!string.IsNullOrEmpty(msg))
                {
                    _menuService.ExibirMensagemPassiva(msg);
                    Thread.Sleep(1500);
                }
            }
        }

        /// <summary>
        /// Dispara DepoisDeAtacar do atacante. Chamado conforme o TipoAtaque
        /// (Sequencial: por hit; AoE: 1x no fim).
        /// </summary>
        private void ProcessarPassivasAtacante(Combate atacante, Combate alvo, List<Combate> aliadosDoAtacante, List<Combate> inimigosDoAtacante)
        {
            var ctxPassiva = new ContextoPassiva(alvo.EstaVivo(), false, aliadosDoAtacante, atacante);
            var ctxCombate = new ContextoCombate(atacante, aliadosDoAtacante, inimigosDoAtacante);

            DispararEvento(EventoCombate.DepoisDeAtacar, atacante, alvo, ctxPassiva, ctxCombate);
        }

        /// <summary>
        /// Dispara DepoisDeMatar do atacante. Chamado SEMPRE por alvo morto,
        /// independente do TipoAtaque — Vilão precisa aplicar MortePermanente em
        /// CADA inimigo morto, mesmo numa AoE.
        /// </summary>
        private void ProcessarPassivasAtacanteMorte(Combate atacante, Combate alvoMorto, List<Combate> aliadosDoAtacante, List<Combate> inimigosDoAtacante)
        {
            if (alvoMorto.EstaVivo()) return;

            var ctxPassiva = new ContextoPassiva(false, false, aliadosDoAtacante, atacante);
            var ctxCombate = new ContextoCombate(atacante, aliadosDoAtacante, inimigosDoAtacante);

            DispararEvento(EventoCombate.DepoisDeMatar, atacante, alvoMorto, ctxPassiva, ctxCombate);
        }

        private void DispararEvento(EventoCombate evento, Combate atacante, Combate alvo,
    ContextoPassiva ctxPassiva, ContextoCombate ctxCombate)
        {
            foreach (Habilidade hab in atacante.Personagem.Habilidades)
            {
                if (hab is not HabilidadePassiva passiva) continue;
                if (!passiva.DeveAtivar(evento, ctxPassiva)) continue;
                if (!atacante.Cooldowns[hab].Disponivel) continue;

                passiva.Ativar(ctxCombate, alvo);
            }
        }

        #endregion

        #region Reacao

        /// <summary>
        /// Dispara as reações do ALVO a um golpe recebido. Ordem: Reflexo/Sangramento
        /// (dano > 0) -> Espinhos/ContraAtaque (sempre). O ContraAtaque declara um
        /// revide (RevidarAlvo); este método o executa com natureza Revide e propaga
        /// recursivamente as reações do alvo revidado. A natureza Revide
        /// (SemContraAtaque) impede novo contra-ataque -> profundidade máxima 1.
        /// </summary>
        private void ProcessarReacoesAlvo(Combate alvo, Combate atacante, ResultadoAtaque r)
        {
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;

            var ctx = new ContextoReacao(alvo, atacante, r.Dano, r.Natureza);
            var resultados = new List<ResultadoReacao>();

            // AoReceberDano: só com dano > 0. [buffs: Reflexo, Sangramento | passivas: futuras]
            if (r.Dano > 0)
            {
                foreach (var s in alvo.StatusAtivos.OfType<IReageAoReceberDano>().ToList())
                    resultados.AddRange(s.AoReceberDano(ctx));

                foreach (var p in ColetarPassivasReativas<IReageAoReceberDano>(alvo))
                    resultados.AddRange(p.AoReceberDano(ctx));
            }

            // AoSerAtacado: sempre. [buffs: Espinhos, ContraAtaque | passivas: Zumbi, Coco...]
            foreach (var s in alvo.StatusAtivos.OfType<IReageAoSerAtacado>().ToList())
                resultados.AddRange(s.AoSerAtacado(ctx));

            foreach (var p in ColetarPassivasReativas<IReageAoSerAtacado>(alvo))
                resultados.AddRange(p.AoSerAtacado(ctx));

            ExibirResultadosReacao(alvo, resultados);

            // Revide (ContraAtaque) — inalterado
            foreach (var res in resultados)
            {
                if (res.RevidarAlvo == null) continue;
                if (!alvo.EstaVivo()) break;
                if (!res.RevidarAlvo.EstaVivo()) continue;

                var revide = alvo.Atacar(res.RevidarAlvo, 1.0, natureza: NaturezasDano.Revide);
                _menuService.ExibirResultadoAtaque(alvo, revide.Alvo, revide);
                Thread.Sleep(1500);
                ProcessarReacoesAlvo(res.RevidarAlvo, alvo, revide);
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
        private void ProcessarReacoesAtacantePorAlvo(Combate atacante, Combate alvo, ResultadoAtaque r)
        {
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;

            var ctx = new ContextoReacao(atacante, alvo, r.Dano, r.Natureza);
            var resultados = new List<ResultadoReacao>();

            foreach (var s in atacante.StatusAtivos.OfType<IReagePorAtaque>().ToList())
                resultados.AddRange(s.PorAtaque(ctx));
            foreach (var p in ColetarPassivasReativas<IReagePorAtaque>(atacante))
                resultados.AddRange(p.PorAtaque(ctx));

            if (r.Dano > 0)
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
        private void ProcessarReacoesAtacantePorAtaque(Combate atacante, Combate alvoRef, ResultadoAtaque r)
        {
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;

            var ctx = new ContextoReacao(atacante, alvoRef, r.Dano, r.Natureza);
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
        /// Dispara as reações "ao matar" (IReageAoMatar) do atacante, por alvo morto.
        /// Chamado no MESMO ponto que ProcessarPassivasAtacanteMorte (antes das reações
        /// de "ao morrer"), preservando a ordem: bloquear revive (Vilao) precede tentar
        /// reviver (Necromancia/Guarda). Guarda: só dispara se o alvo realmente morreu.
        /// </summary>
        private void ProcessarReacoesAtacanteMorte(Combate atacante, Combate alvoMorto, ResultadoAtaque r)
        {
            if (alvoMorto.EstaVivo()) return;
            if (r.Natureza.Reacao == TipoReacao.Nenhuma) return;

            var ctx = new ContextoReacao(atacante, alvoMorto, r.Dano, r.Natureza);
            var resultados = new List<ResultadoReacao>();

            foreach (var s in atacante.StatusAtivos.OfType<IReageAoMatar>().ToList())
                resultados.AddRange(s.AoMatar(ctx));

            foreach (var p in ColetarPassivasReativas<IReageAoMatar>(atacante))
                resultados.AddRange(p.AoMatar(ctx));

            ExibirResultadosReacao(atacante, resultados);
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