using ApostlesWar;
using GHUtils;
using System;
using System.Collections.Generic;
using v1_Apostle_s_War.Skills.Ativas;

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

        /// <summary>
        /// Representa o que o atacante decidiu fazer no turno.
        /// Habilidade null = ataque básico.
        /// </summary>
        private record AcaoEscolhida(HabilidadeAtiva? Habilidade);

        #endregion

        #region Loop principal de combate

        /// <summary>
        /// Itera pelos combatentes em ordem, alternando turnos até que um lado seja eliminado.
        /// Retorna true se o jogador vencer.
        /// </summary>
        private bool ExecutarCombate(List<Combate> jogador, List<Combate> inimigo, List<Combate> combatentes)
        {
            do
            {
                for (int c = 0; c < combatentes.Count; c++)
                {
                    if (!combatentes[c].EstaVivo()) continue;
                    if (!inimigo.Any(i => i.EstaVivo()) || !jogador.Any(j => j.EstaVivo())) break;

                    // Preso é verificado antes do turno, mas AvancarStatus vem depois
                    if (combatentes[c].StatusAtivos.Any(s => s is Skills.Debuffs.Preso))
                    {
                        AvancarStatus(combatentes[c]);
                        AvancarCooldowns(combatentes[c]);
                        continue;
                    }

                    List<Combate> defensores = combatentes[c] is Jogador ? inimigo : jogador;
                    List<Combate> aliados = combatentes[c] is Jogador ? jogador : inimigo;

                    ExecutarTurno(combatentes[c], defensores, aliados);

                    AvancarStatus(combatentes[c]);   // ← depois do turno
                    AvancarCooldowns(combatentes[c]);
                }
            } while (jogador.Any(j => j.EstaVivo()) && inimigo.Any(i => i.EstaVivo()));

            return jogador.Any(j => j.EstaVivo());
        }


        #endregion

        #region Turno individual

        /// <summary>
        /// Conduz o turno de um único combatente: escolhe ação, escolhe alvo, executa.
        /// </summary>
        private void ExecutarTurno(Combate atacante, List<Combate> defensores, List<Combate> aliados)
        {
            Console.Clear();

            AcaoEscolhida acao = atacante is Jogador
                ? EscolherAcaoJogador(atacante, defensores, aliados)
                : EscolherAcaoInimigo(atacante);

            ExecutarAcao(atacante, acao, defensores, aliados);
        }

        #endregion

        #region Escolha de ação — Jogador

        /// <summary>
        /// Exibe o menu de ações e aguarda o jogador escolher entre ataque básico e habilidades disponíveis.
        /// </summary>
        private AcaoEscolhida EscolherAcaoJogador(Combate atacante, List<Combate> defensores, List<Combate> aliados)
        {
            var habilidadesAtivas = atacante.Personagem.Habilidades.OfType<HabilidadeAtiva>().ToList();
            int totalOpcoes = habilidadesAtivas.Count + 1; // +1 = ataque básico
            int acao = 1;

            while (true)
            {
                Console.Clear();
                _menuService.ExibirPartida(aliados, defensores);
                _menuService.ExibirAcoes(atacante, acao);

                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;

                int novaAcao = ConsoleUtils.SelecionarComCursor(acao, 1, totalOpcoes, key.Key);

                // Bloqueia seleção de habilidade indisponível
                if (novaAcao >= 2)
                {
                    var hab = habilidadesAtivas[novaAcao - 2];
                    if (!atacante.Cooldowns[hab].Disponivel) continue;
                }

                acao = novaAcao;
            }

            return acao == 1
                ? new AcaoEscolhida(null)
                : new AcaoEscolhida(habilidadesAtivas[acao - 2]);
        }

        #endregion

        #region Escolha de ação — Inimigo

        /// <summary>
        /// Inimigo sempre ataca básico (sem IA de habilidade por enquanto).
        /// </summary>
        private AcaoEscolhida EscolherAcaoInimigo(Combate atacante)
        {
            return new AcaoEscolhida(null);
        }

        #endregion

        #region Execução de ação

        /// <summary>
        /// Dispatcher: roteia para ataque básico ou habilidade.
        /// </summary>
        private void ExecutarAcao(Combate atacante, AcaoEscolhida acao, List<Combate> defensores, List<Combate> aliados)
        {
            if (acao.Habilidade == null)
                ExecutarAtaqueBasico(atacante, defensores, aliados);
            else
                ExecutarHabilidade(atacante, acao.Habilidade, defensores, aliados);
        }

        #endregion

        #region Ataque básico

        /// <summary>
        /// Executa o ataque básico: escolhe alvo, ataca, processa passivas pós-ataque.
        /// </summary>
        private void ExecutarAtaqueBasico(Combate atacante, List<Combate> defensores, List<Combate> aliados)
        {
            Combate alvo = atacante is Jogador
                ? EscolherAlvoJogador(atacante, defensores, aliados)
                : EscolherAlvoAleatorio(defensores);

            if (atacante is Inimigo)
            {
                _menuService.ExibirPartida(defensores, new List<Combate>());
                Console.WriteLine($"\n{atacante.Personagem.Simbolo} {atacante.Personagem.Nome} prepara o ataque!");
                Thread.Sleep(1500);
            }

            atacante.Atacar(alvo);
            Console.WriteLine($"hp atual do {alvo.Personagem.Simbolo} é de {alvo.HPAtual}");
            Thread.Sleep(1500);

            ProcessarPassivasAlvo(alvo, atacante);
            ProcessarPassivasAtacante(atacante, alvo, aliados);
        }

        #endregion

        #region Habilidade

        /// <summary>
        /// Executa uma habilidade ativa. Centraliza o cast para AtivarComAtacante quando necessário,
        /// resolvendo o problema de habilidades que escalam com ATK do atacante.
        /// </summary>
        private void ExecutarHabilidade(Combate atacante, HabilidadeAtiva hab, List<Combate> defensores, List<Combate> aliados)
        {
            if (hab is Marretada marretada)
            {
                Combate alvo = EscolherAlvoUnico(atacante, hab, defensores, aliados);
                marretada.AtivarComAtacante(atacante, alvo);
            }
            else if (hab is Tiroteio tiroteio)
            {
                tiroteio.AtivarComAtacante(atacante, defensores);
            }
            else if (hab is Sushi || hab is Nigiri || hab is ParedeDeTijolos)
            {
                hab.Ativar(atacante, aliados);
            }
            else if (hab is Espionagem)
            {
                hab.Ativar(defensores.First(d => d.EstaVivo()), defensores);
            }
            else if (hab is Furtividade)
            {
                hab.Ativar(atacante, defensores);
            }
            else if (hab.NumeroDeAlvos == 1)
            {
                Combate alvo = EscolherAlvoUnico(atacante, hab, defensores, aliados);
                hab.Ativar(alvo);
            }
            else if (hab.NumeroDeAlvos > 1 && hab.NumeroDeAlvos != int.MaxValue)
            {
                var alvos = defensores
                    .Where(d => d.EstaVivo())
                    .OrderBy(_ => _random.Next())
                    .Take(hab.NumeroDeAlvos);
                foreach (Combate a in alvos)
                    hab.Ativar(a);
            }
            else
            {
                hab.Ativar(defensores.First(), defensores);
            }

            atacante.Cooldowns[hab].Usar();
            Console.WriteLine($"{atacante.Personagem.Simbolo} usou {hab.Nome}!");
            Thread.Sleep(1500);
        }

        #endregion

        #region Seleção de alvos

        /// <summary>
        /// Permite ao jogador escolher um único alvo com cursor.
        /// </summary>
        private Combate EscolherAlvoJogador(Combate atacante, List<Combate> defensores, List<Combate> aliados)
        {
            var alvosVivos = defensores.Where(d => d.EstaVivo()).ToList();
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

        /// <summary>
        /// Wrapper para escolha de alvo único de habilidade (mesma UX do ataque básico).
        /// </summary>
        private Combate EscolherAlvoUnico(Combate atacante, HabilidadeAtiva hab, List<Combate> defensores, List<Combate> aliados)
        {
            return EscolherAlvoJogador(atacante, defensores, aliados);
        }

        /// <summary>
        /// Sorteia um alvo vivo aleatório (usado pelo inimigo).
        /// </summary>
        private Combate EscolherAlvoAleatorio(List<Combate> defensores)
        {
            var vivos = defensores.Where(d => d.EstaVivo()).ToList();
            return vivos[_random.Next(vivos.Count)];
        }

        #endregion

        #region Passivas e status

        /// <summary>
        /// Avança o cooldown de todas as habilidades do combatente.
        /// </summary>
        private void AvancarCooldowns(Combate combatente)
        {
            foreach (var cd in combatente.Cooldowns.Values)
                cd.PassarTurno();
        }

        /// <summary>
        /// Avança a contagem de turnos de todos os status ativos e remove os expirados.
        /// </summary>
        private void AvancarStatus(Combate combatente)
        {
            foreach (StatusEffect status in combatente.StatusAtivos.ToList())
            {
                status.PassarTurno();
                if (status.Expirou)
                    status.Remover(combatente);
            }
        }

        /// <summary>
        /// Processa passivas do alvo que reagem a receber dano (ex: Necromancia).
        /// </summary>
        private void ProcessarPassivasAlvo(Combate alvo, Combate atacante)
        {
            foreach (Habilidade hab in alvo.Personagem.Habilidades)
            {
                if (hab is not HabilidadePassiva passiva) continue;
                if (!passiva.DeveAtivar(EventoCombate.DepoisDeReceberDano)) continue;
                if (!alvo.Cooldowns[hab].Disponivel) continue;
                if (alvo.EstaVivo() && !(hab is Skills.Passivas.PassivaOperario)) continue;

                // Passiva do Operário: contra-ataque com Marretada (alvo vivo)
                if (hab is Skills.Passivas.PassivaOperario && alvo.EstaVivo())
                {
                    passiva.Ativar(alvo, new List<Combate> { atacante });
                    alvo.Cooldowns[hab].Usar();
                    continue;
                }

                // Passivas que reagem à morte (Necromancia)
                if (!alvo.EstaVivo())
                {
                    passiva.Ativar(alvo);
                    alvo.Cooldowns[hab].Usar();
                    Console.WriteLine(passiva.Revive()
                        ? passiva.MensagemSobreviveu(alvo.Personagem)
                        : passiva.MensagemMorreu(alvo.Personagem));
                    Thread.Sleep(1500);
                }
            }
        }

        /// <summary>
        /// Processa passivas do atacante que reagem após atacar (ex: PassivaPolicial estender Preso).
        /// </summary>
        private void ProcessarPassivasAtacante(Combate atacante, Combate alvo, List<Combate> aliados)
        {
            foreach (Habilidade hab in atacante.Personagem.Habilidades)
            {
                if (hab is Skills.Passivas.PassivaPolicial policial)
                    policial.Ativar(alvo);

                if (hab is Skills.Passivas.PassivaDetetive detetive)
                {
                    // Recalcula DanoCrit do Detetive baseado em debuffs inimigos
                    detetive.Ativar(atacante, /* inimigos = */ aliados);
                }
            }
        }

        #endregion

        #region Fluxo de fase

        /// <summary>
        /// Executa uma fase completa (rodada 1 + rodada 2).
        /// </summary>
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
            var jogador = time.Select(p => (Combate)new Jogador(p)).ToList();
            foreach (Combate c in jogador)
                _arsenalService.AplicarItens(c);

            if (!ExecutarRodada(jogador, fas.Rodada1, capitulo, mult)) return false;
            return ExecutarRodada(jogador, fas.Rodada2, capitulo, mult);
        }

        /// <summary>
        /// Executa uma rodada da fase: monta inimigos e roda o combate.
        /// </summary>
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
