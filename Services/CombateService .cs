using ApostlesWar;
using GHUtils;
using v1_Apostle_s_War.Skills.Buffs;
using v1_Apostle_s_War.Skills.Debuffs;

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
        private readonly Random _random = new Random();

        public CombateService(ArsenalService arsenalService, CampanhaService campanhaService,
            CampeoesService campeoesService, PersonagemService personagemService, MenuService menuService)
        {
            _arsenalService = arsenalService;
            _campanhaService = campanhaService;
            _campeoesService = campeoesService;
            _personagemService = personagemService;
            _menuService = menuService;
        }

        #endregion

        #region Estrutura de ação

        private record AcaoEscolhida(HabilidadeAtiva? Habilidade);

        #endregion

        #region Loop principal

        private bool ExecutarCombate(List<Combate> jogador, List<Combate> inimigo, List<Combate> combatentes)
        {
            // Trigger inicial de combate — passivas que devem agir no setup (ex: PassivaFantasma aplica Intocavel)
            foreach (var c in combatentes)
                AtivarPassivasIniciais(c);

            do
            {
                for (int c = 0; c < combatentes.Count; c++)
                {
                    if (!combatentes[c].EstaVivo()) continue;
                    if (!inimigo.Any(i => i.EstaVivo()) || !jogador.Any(j => j.EstaVivo())) break;

                    // Início do turno — Veneno e similares tickam aqui
                    ExecutarInicioDeTurno(combatentes[c]);

                    // Pode ter morrido pelo Veneno
                    if (!combatentes[c].EstaVivo()) continue;

                    if (combatentes[c].StatusAtivos.Any(s => s is Preso))
                    {
                        AvancarStatus(combatentes[c]);
                        AvancarCooldowns(combatentes[c]);
                        continue;
                    }

                    List<Combate> defensores = combatentes[c] is Jogador ? inimigo : jogador;
                    List<Combate> aliados = combatentes[c] is Jogador ? jogador : inimigo;

                    ExecutarTurno(combatentes[c], defensores, aliados);

                    AvancarStatus(combatentes[c]);
                    AvancarCooldowns(combatentes[c]);
                }
            } while (jogador.Any(j => j.EstaVivo()) && inimigo.Any(i => i.EstaVivo()));

            return jogador.Any(j => j.EstaVivo());
        }

        #endregion

        #region Hooks de turno

        /// <summary>
        /// Aplica efeitos no início do turno do combatente.
        /// Cada status decide se age (Veneno causa dano, outros ignoram).
        /// </summary>
        private void ExecutarInicioDeTurno(Combate combatente)
        {
            foreach (StatusEffect status in combatente.StatusAtivos.ToList())
                status.AoIniciarTurno(combatente);
        }

        /// <summary>
        /// Dispara passivas que devem agir no início do combate (não em resposta a evento).
        /// Cada passiva decide se aplica via DeveAtivar — aqui usamos um evento sintético.
        /// </summary>
        private void AtivarPassivasIniciais(Combate combatente)
        {
            // Procura passivas que se autoaplicam no setup do combate
            foreach (Habilidade hab in combatente.Personagem.Habilidades.OfType<HabilidadePassiva>())
            {
                var passiva = (HabilidadePassiva)hab;
                if (passiva is IPassivaInicial inicial)
                    inicial.AplicarInicial(combatente);
            }
        }

        #endregion

        #region Turno

        private void ExecutarTurno(Combate atacante, List<Combate> defensores, List<Combate> aliados)
        {
            Console.Clear();

            // Irritar — força A1 automático no aplicador
            var irritar = atacante.StatusAtivos.OfType<Irritar>().FirstOrDefault();
            if (irritar != null)
            {
                _menuService.ExibirPartida(aliados, defensores);
                _menuService.ExibirMensagemPassiva($"{atacante.Personagem.Simbolo} está irritado e ataca {irritar.Aplicador.Personagem.Simbolo} automaticamente!");
                Thread.Sleep(1500);
                var resultado = atacante.Atacar(irritar.Aplicador);
                _menuService.ExibirResultadoAtaque(atacante, irritar.Aplicador, resultado);
                Thread.Sleep(1500);
                ProcessarPassivasAlvo(irritar.Aplicador, atacante, aliados, resultado.Critico);
                ProcessarPassivasAtacante(atacante, irritar.Aplicador, aliados);
                return;
            }

            AcaoEscolhida acao = atacante is Jogador
                ? EscolherAcaoJogador(atacante, defensores, aliados)
                : EscolherAcaoInimigo(atacante);

            ExecutarAcao(atacante, acao, defensores, aliados);
        }

        #endregion

        #region Escolha de ação

        private AcaoEscolhida EscolherAcaoJogador(Combate atacante, List<Combate> defensores, List<Combate> aliados)
        {
            var habilidadesAtivas = atacante.Personagem.Habilidades.OfType<HabilidadeAtiva>().ToList();
            int totalOpcoes = habilidadesAtivas.Count + 1;
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

                while (novaAcao >= 2 && novaAcao != acao)
                {
                    var hab = habilidadesAtivas[novaAcao - 2];
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

            return acao == 1
                ? new AcaoEscolhida(null)
                : new AcaoEscolhida(habilidadesAtivas[acao - 2]);
        }

        private AcaoEscolhida EscolherAcaoInimigo(Combate atacante) => new AcaoEscolhida(null);

        #endregion

        #region Execução

        private void ExecutarAcao(Combate atacante, AcaoEscolhida acao, List<Combate> defensores, List<Combate> aliados)
        {
            if (acao.Habilidade == null)
                ExecutarAtaqueBasico(atacante, defensores, aliados);
            else
                ExecutarHabilidade(atacante, acao.Habilidade, defensores, aliados);
        }

        private void ExecutarAtaqueBasico(Combate atacante, List<Combate> defensores, List<Combate> aliados)
        {
            var alvosDisponiveis = ResolverListaDeAlvosDisponiveis(defensores, atacante);

            Combate alvo = atacante is Jogador
                ? EscolherAlvoDaLista(atacante, alvosDisponiveis, aliados, defensores)
                : EscolherAlvoAleatorio(alvosDisponiveis);

            if (atacante is Inimigo)
            {
                _menuService.ExibirPreparacaoAtaque(atacante, defensores);
                Thread.Sleep(1500);
            }

            var resultado = atacante.Atacar(alvo);
            _menuService.ExibirResultadoAtaque(atacante, alvo, resultado);
            Thread.Sleep(1500);

            ProcessarPassivasAlvo(alvo, atacante, aliados, resultado.Critico);
            ProcessarPassivasAtacante(atacante, alvo, aliados);
        }

        private void ExecutarHabilidade(Combate atacante, HabilidadeAtiva hab, List<Combate> defensores, List<Combate> aliados)
        {
            List<Combate> lista = hab.TipoLista switch
            {
                TipoLista.Aliados => aliados,
                TipoLista.Inimigos => defensores,
                TipoLista.Self => new List<Combate> { atacante },
                _ => defensores
            };

            Combate alvoInicial;
            if (hab.TipoLista == TipoLista.Inimigos)
            {
                var alvosDisponiveis = ResolverListaDeAlvosDisponiveis(defensores, atacante);
                alvoInicial = atacante is Jogador
                    ? EscolherAlvoDaLista(atacante, alvosDisponiveis, aliados, defensores)
                    : EscolherAlvoAleatorio(alvosDisponiveis);
            }
            else
            {
                alvoInicial = atacante;
            }

            var resultados = hab.Ativar(atacante, alvoInicial, lista);
            foreach (var r in resultados)
            {
                _menuService.ExibirResultadoAtaque(atacante, r.Alvo, r);
                // Processa passivas do alvo após cada hit (Invencível pode salvar)
                ProcessarPassivasAlvo(r.Alvo, atacante, aliados, r.Critico);
            }

            atacante.Cooldowns[hab].Usar();
            _menuService.ExibirUsoHabilidade(atacante, hab);
            Thread.Sleep(2500);
        }

        #endregion

        #region Seleção de alvos

        /// <summary>
        /// Resolve a lista de alvos disponíveis conforme prioridade:
        /// 1. Provocar — só quem tem
        /// 2. Sem BloqueioTotal e sem Intocável
        /// 3. Sem BloqueioTotal
        /// 4. Sem filtro
        /// </summary>
        private List<Combate> ResolverListaDeAlvosDisponiveis(List<Combate> candidatos, Combate atacante)
        {
            var vivos = candidatos.Where(c => c.EstaVivo()).ToList();

            var comProvocar = vivos.Where(c => c.StatusAtivos.Any(s => s is Provocar)).ToList();
            if (comProvocar.Count > 0) return comProvocar;

            var semTudo = vivos.Where(c =>
                !c.StatusAtivos.Any(s => s is BloqueioTotal) &&
                !c.StatusAtivos.Any(s => s is Intocavel)).ToList();
            if (semTudo.Count > 0) return semTudo;

            var semBloqueio = vivos.Where(c => !c.StatusAtivos.Any(s => s is BloqueioTotal)).ToList();
            if (semBloqueio.Count > 0) return semBloqueio;

            return vivos;
        }

        private Combate EscolherAlvoDaLista(Combate atacante, List<Combate> lista, List<Combate> aliados, List<Combate> defensores)
        {
            var alvosVivos = lista.Where(d => d.EstaVivo()).ToList();
            int idx = 1;

            while (true)
            {
                Console.Clear();
                _menuService.ExibirPartida(aliados, defensores);
                Console.WriteLine("\nAlvos:");
                for (int i = 0; i < alvosVivos.Count; i++)
                {
                    string cursor = idx == i + 1 ? "▶" : " ";
                    Console.WriteLine($"{cursor} {i + 1} - {alvosVivos[i].Personagem.Simbolo} {alvosVivos[i].Personagem.Nome} | HP:{alvosVivos[i].HPAtual} ATK:{alvosVivos[i].Ataque} DEF:{alvosVivos[i].Defesa}");
                }

                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) return alvosVivos[idx - 1];

                idx = ConsoleUtils.SelecionarComCursor(idx, 1, alvosVivos.Count, key.Key);
            }
        }

        private Combate EscolherAlvoAleatorio(List<Combate> candidatos)
        {
            var vivos = candidatos.Where(d => d.EstaVivo()).ToList();
            return vivos[_random.Next(vivos.Count)];
        }

        #endregion

        #region Passivas e status

        private void AvancarCooldowns(Combate combatente)
        {
            foreach (var cd in combatente.Cooldowns.Values)
                cd.PassarTurno();
        }

        private void AvancarStatus(Combate combatente)
        {
            foreach (StatusEffect status in combatente.StatusAtivos.ToList())
            {
                status.PassarTurno();
                if (status.Expirou)
                    status.Remover(combatente);
            }
        }

        private void ProcessarPassivasAlvo(Combate alvo, Combate atacante, List<Combate> aliados, bool foiCritico)
        {
            var ctx = new ContextoPassiva(alvo.EstaVivo(), foiCritico, aliados, atacante);

            foreach (Habilidade hab in alvo.Personagem.Habilidades)
            {
                if (hab is not HabilidadePassiva passiva) continue;
                if (!passiva.DeveAtivar(EventoCombate.DepoisDeReceberDano, ctx)) continue;
                if (!alvo.Cooldowns[hab].Disponivel) continue;

                var resultados = passiva.Ativar(alvo, atacante, aliados);
                foreach (var r in resultados)
                    _menuService.ExibirResultadoAtaque(alvo, r.Alvo, r);

                alvo.Cooldowns[hab].Usar();

                string msg = passiva.Revive()
                    ? passiva.MensagemSobreviveu(alvo.Personagem)
                    : passiva.MensagemMorreu(alvo.Personagem);

                if (!string.IsNullOrEmpty(msg))
                {
                    _menuService.ExibirMensagemPassiva(msg);
                    Thread.Sleep(1500);
                }
            }
        }

        private void ProcessarPassivasAtacante(Combate atacante, Combate alvo, List<Combate> inimigos)
        {
            var ctx = new ContextoPassiva(alvo.EstaVivo(), false, inimigos, atacante);

            foreach (Habilidade hab in atacante.Personagem.Habilidades)
            {
                if (hab is not HabilidadePassiva passiva) continue;
                if (!passiva.DeveAtivar(EventoCombate.DepoisDeAtacar, ctx)) continue;
                if (!atacante.Cooldowns[hab].Disponivel) continue;

                passiva.Ativar(atacante, alvo, inimigos);
            }
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

            if (!ExecutarRodada(jogador, fas.Rodada1, capitulo, mult)) return false;
            return ExecutarRodada(jogador, fas.Rodada2, capitulo, mult);
        }

        private bool ExecutarRodada(List<Combate> jogador, List<Slot> slotsInimigos, Faccao capitulo, MultiplicadorFase mult)
        {
            var inimigo = new List<Combate>();
            foreach (Slot slot in slotsInimigos)
                inimigo.Add(new Inimigo(_personagemService.ObterPersonagem(capitulo, slot), mult));

            var combatentes = new List<Combate>();
            combatentes.AddRange(jogador);
            combatentes.AddRange(inimigo);

            return ExecutarCombate(jogador, inimigo, combatentes);
        }

        #endregion
    }
}
