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

        #endregion
        public CombateService(ArsenalService arsenalService, CampanhaService campanhaService,
            CampeoesService campeoesService, PersonagemService personagemService)
        {
            _arsenalService = arsenalService;
            _campanhaService = campanhaService;
            _campeoesService = campeoesService;
            _personagemService = personagemService;
        }

        #region Combate

        Random random = new Random();

        /// <summary>
        /// Exibe os alvos vivos, lê a escolha do jogador ou sorteia um alvo para o inimigo, executa o ataque e exibe o HP resultante
        /// </summary>
        void ExecutarTurno(Combate atacante, List<Combate> defensor)
        {
            Console.Clear();
            Console.WriteLine($"{atacante.Personagem.Simbolo} ataca! | HP:{atacante.HPAtual} ATK:{atacante.Ataque} DEF:{atacante.Defesa}");
            Console.WriteLine("Alvos disponíveis:");
            for (int i = 0; i < defensor.Count; i++)
            {
                if (defensor[i].EstaVivo())
                    Console.WriteLine($"{i + 1} - {defensor[i].Personagem.Simbolo} | HP:{defensor[i].HPAtual} ATK:{defensor[i].Ataque} DEF:{defensor[i].Defesa}");
            }

            int alvo = 0;
            if (atacante is Jogador)
            {
                while (true)
                {
                    if (int.TryParse(Console.ReadLine(), out alvo) && alvo >= 1 && alvo <= defensor.Count && defensor[alvo - 1].EstaVivo())
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Digite entre 1 e {defensor.Count}, o alvo precisa estar vivo");
                    }
                }
            }
            else if (atacante is Inimigo)
            {
                while (true)
                {
                    alvo = random.Next(1, defensor.Count + 1);
                    if (alvo >= 1 && alvo <= defensor.Count && defensor[alvo - 1].EstaVivo())
                    {
                        break;
                    }
                }
            }

            if (atacante is Inimigo)
            {
                Console.WriteLine($"{atacante.Personagem.Simbolo} prepara o ataque!");
                Thread.Sleep(1500);
            }

            atacante.Atacar(defensor[alvo - 1]);
            Console.WriteLine($"hp atual do {defensor[alvo - 1].Personagem.Simbolo} é de {defensor[alvo - 1].HPAtual}");

            if (atacante is Jogador)
            {
                Thread.Sleep(1500);
            }

            var alvoAtacado = defensor[alvo - 1];
            if (!alvoAtacado.EstaVivo()
                && alvoAtacado.Personagem.Habilidade is HabilidadePassiva passiva
                && passiva.DeveAtivar(EventoCombate.DepoisDeReceberDano)
                && alvoAtacado.Cooldown != null
                && alvoAtacado.Cooldown.Disponivel)
            {
                passiva.Ativar(alvoAtacado);
                alvoAtacado.Cooldown.Usar();
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

            if (atacante is Inimigo)
            {
                Thread.Sleep(1500);
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
                    if (!inimigo.Any(i => i.EstaVivo()) || !jogador.Any(j => j.EstaVivo()))
                    { break; }
                    if (combatentes[c] is Jogador)
                    {
                        ExecutarTurno(combatentes[c], inimigo);
                        combatentes[c].Personagem.Habilidade?.Cooldown.PassarTurno();
                    }
                    else
                    {
                        ExecutarTurno(combatentes[c], jogador);
                        combatentes[c].Personagem.Habilidade?.Cooldown.PassarTurno();
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
