using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;

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

        #endregion
        public CombateService(ArsenalService arsenalService, CampanhaService campanhaService,
            CampeoesService campeoesService, PersonagemService personagemService, MenuService menuService)
        {
            _arsenalService = arsenalService;
            _campanhaService = campanhaService;
            _campeoesService = campeoesService;
            _personagemService = personagemService;
            _menuService = menuService;
        }

        #region Combate

        Random random = new Random();

        /// <summary>
        /// Exibe os alvos vivos, lê a escolha do jogador ou sorteia um alvo para o inimigo, executa o ataque e exibe o HP resultante
        /// </summary>
        void ExecutarTurno(Combate atacante, List<Combate> defensor, List<Combate> jogadores)
        {
            Console.Clear();
            int alvo = 0;
            bool usouHabilidade = false;

            if (atacante is Jogador)
            {
                int acao = 1;
                while (true)
                {
                    Console.Clear();
                    _menuService.ExibirPartida(jogadores, defensor);
                    _menuService.ExibirAcoes(atacante, acao);

                    ConsoleKeyInfo key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Enter)
                        break;

                    if (int.TryParse(key.KeyChar.ToString(), out int num) && num >= 1)
                    {
                        int totalAtivas = atacante.Personagem.Habilidades.Count(h => h is HabilidadeAtiva) + 1;
                        if (num <= totalAtivas)
                        {
                            if (num == 1)
                            {
                                acao = num;
                            }
                            else
                            {
                                var habs = atacante.Personagem.Habilidades.Where(h => h is HabilidadeAtiva).ToList();
                                int idx = num - 2;
                                if (idx >= 0 && idx < habs.Count)
                                {
                                    var cdKey = atacante.Cooldowns.Keys.FirstOrDefault(k => k.Nome == habs[idx].Nome);
                                    if (cdKey != null && atacante.Cooldowns[cdKey].Disponivel)
                                        acao = num;
                                    // se não disponível, ignora — seta fica onde está
                                }
                            }
                        }
                    }
                }

                if (acao == 1)
                {
                    var alvosVivos = defensor.Where(d => d.EstaVivo()).ToList();
                    int alvoSelecionado = 1;

                    while (true)
                    {
                        Console.Clear();
                        _menuService.ExibirPartida(jogadores, defensor);
                        _menuService.ExibirAcoes(atacante, acao);
                        Console.WriteLine("\nAlvos:");
                        for (int i = 0; i < alvosVivos.Count; i++)
                        {
                            string cursor = alvoSelecionado == i + 1 ? "▶" : " ";
                            Console.WriteLine($"{cursor} {i + 1} - {alvosVivos[i].Personagem.Simbolo} {alvosVivos[i].Personagem.Nome} | HP:{alvosVivos[i].HPAtual} ATK:{alvosVivos[i].Ataque} DEF:{alvosVivos[i].Defesa}");
                        }

                        ConsoleKeyInfo key = Console.ReadKey(true);

                        if (key.Key == ConsoleKey.Enter)
                        {
                            alvo = defensor.IndexOf(alvosVivos[alvoSelecionado - 1]) + 1;
                            break;
                        }

                        if (int.TryParse(key.KeyChar.ToString(), out int num) && num >= 1 && num <= alvosVivos.Count)
                            alvoSelecionado = num;
                    }
                }
                else
                {
                    var habilidadesAtivas = atacante.Personagem.Habilidades
                        .Where(h => h is HabilidadeAtiva)
                        .ToList();

                    int indiceHab = acao - 2;
                    if (indiceHab >= 0 && indiceHab < habilidadesAtivas.Count)
                    {
                        var hab = (HabilidadeAtiva)habilidadesAtivas[indiceHab];

                        if (hab.NumeroDeAlvos == 1)
                        {
                            Console.WriteLine("\nAlvos disponíveis:");
                            for (int i = 0; i < defensor.Count; i++)
                            {
                                if (defensor[i].EstaVivo())
                                    Console.WriteLine($"{i + 1} - {defensor[i].Personagem.Simbolo} | HP:{defensor[i].HPAtual} ATK:{defensor[i].Ataque} DEF:{defensor[i].Defesa}");
                            }
                            while (true)
                            {
                                if (int.TryParse(Console.ReadLine(), out alvo) && alvo >= 1 && alvo <= defensor.Count && defensor[alvo - 1].EstaVivo())
                                    break;
                                Console.WriteLine($"Digite entre 1 e {defensor.Count}, o alvo precisa estar vivo");
                            }
                            hab.Ativar(defensor[alvo - 1]);
                        }
                        else if (hab.NumeroDeAlvos == int.MaxValue)
                        {
                            hab.Ativar(defensor.First(), jogadores);
                        }
                        else
                        {
                            var alvosAleatorios = defensor.Where(d => d.EstaVivo())
                                .OrderBy(_ => random.Next())
                                .Take(hab.NumeroDeAlvos);
                            foreach (Combate alvoAleatorio in alvosAleatorios)
                                hab.Ativar(alvoAleatorio);
                        }

                        usouHabilidade = true;

                        var cdKey = atacante.Cooldowns.Keys.FirstOrDefault(k => k.Nome == hab.Nome);
                        if (cdKey != null)
                            atacante.Cooldowns[cdKey].Usar();

                        Console.WriteLine($"{atacante.Personagem.Simbolo} usou {hab.Nome}!");
                        Thread.Sleep(1500);
                    }
                    else
                    {
                        Console.WriteLine("Habilidade inválida.");
                    }
                }
            }
            else if (atacante is Inimigo)
            {
                while (true)
                {
                    alvo = random.Next(1, defensor.Count + 1);
                    if (alvo >= 1 && alvo <= defensor.Count && defensor[alvo - 1].EstaVivo())
                        break;
                }
            }

            if (atacante is Inimigo)
            {
                _menuService.ExibirPartida(defensor, new List<Combate>());
                Console.WriteLine($"\n{atacante.Personagem.Simbolo} {atacante.Personagem.Nome} prepara o ataque!");
                Thread.Sleep(1500);
            }

            if (!usouHabilidade)
            {
                atacante.Atacar(defensor[alvo - 1]);
                Console.WriteLine($"hp atual do {defensor[alvo - 1].Personagem.Simbolo} é de {defensor[alvo - 1].HPAtual}");

                if (atacante is Jogador)
                {
                    Thread.Sleep(1500);
                }

                var alvoAtacado = defensor[alvo - 1];
                foreach (Habilidade hab in alvoAtacado.Personagem.Habilidades)
                {
                    if (!alvoAtacado.EstaVivo()
                        && hab is HabilidadePassiva passiva
                        && passiva.DeveAtivar(EventoCombate.DepoisDeReceberDano)
                        && alvoAtacado.Cooldowns[hab].Disponivel)
                    {
                        passiva.Ativar(alvoAtacado);
                        alvoAtacado.Cooldowns[hab].Usar();
                        if (passiva.Revive())
                        {
                            Console.WriteLine(passiva.MensagemSobreviveu(alvoAtacado.Personagem));
                            Thread.Sleep(1500);
                        }
                        else
                        {
                            Console.WriteLine(passiva.MensagemMorreu(alvoAtacado.Personagem));
                            Thread.Sleep(1500);
                        }
                    }
                }

                if (atacante is Inimigo)
                {
                    Thread.Sleep(1500);
                }
            }
        }

        /// <summary>
        /// Itera pelos combatentes em ordem, alternando ataques entre jogador e inimigo, até que um dos lados seja completamente eliminado.
        /// Retorna true se o jogador vencer
        /// </summary>
        bool ExecutarCombate(List<Combate> jogador, List<Combate> inimigo, List<Combate> combatentes)
        {
            do
            {
                for (int c = 0; c < combatentes.Count; c++)
                {
                    if (!combatentes[c].EstaVivo()) continue;
                    if (!inimigo.Any(i => i.EstaVivo()) || !jogador.Any(j => j.EstaVivo())) break;

                    foreach (StatusEffect status in combatentes[c].StatusAtivos.ToList())
                    {
                        status.PassarTurno();
                        if (status.Expirou)
                            status.Remover(combatentes[c]);
                    }

                    if (combatentes[c] is Jogador)
                    {
                        ExecutarTurno(combatentes[c], inimigo, jogador);
                    }
                    else
                    {
                        ExecutarTurno(combatentes[c], jogador, jogador);
                    }

                    foreach (var cd in combatentes[c].Cooldowns.Values)
                    {
                        cd.PassarTurno();
                    }
                }
            } while (jogador.Any(j => j.EstaVivo()) && inimigo.Any(i => i.EstaVivo()));

            return jogador.Any(j => j.EstaVivo());
        }

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

            var inimigo = new List<Combate>();
            var combatentes = new List<Combate>();

            foreach (Slot slot in fas.Rodada1)
                inimigo.Add(new Inimigo(_personagemService.ObterPersonagem(capitulo, slot), mult));

            combatentes.AddRange(jogador);
            combatentes.AddRange(inimigo);

            if (!ExecutarCombate(jogador, inimigo, combatentes)) return false;

            inimigo.Clear();
            combatentes.Clear();

            foreach (Slot slot in fas.Rodada2)
                inimigo.Add(new Inimigo(_personagemService.ObterPersonagem(capitulo, slot), mult));

            combatentes.AddRange(jogador);
            combatentes.AddRange(inimigo);

            return ExecutarCombate(jogador, inimigo, combatentes);
        }

        #endregion
    }
}
