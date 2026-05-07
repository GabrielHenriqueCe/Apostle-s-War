namespace ApostlesWar

{
    #region Campeoes

    /// <summary>
    /// Gerencia os campeões desbloqueados e a seleção do time do jogador
    /// </summary>
    class Campeoes
    {
        private static readonly List<Personagem> desbloqueados = new List<Personagem>
        {
            SelecaoSimbolo.ObterPersonagem(Faccao.Humanos, Slot.Slot1),
            SelecaoSimbolo.ObterPersonagem(Faccao.Humanos, Slot.Slot2),
            SelecaoSimbolo.ObterPersonagem(Faccao.Humanos, Slot.Slot3),
            SelecaoSimbolo.ObterPersonagem(Faccao.Humanos, Slot.Slot4),
        };

        public static void DesbloquearCampeoes(Faccao faccao, Fases fase)
        {
            Fase fas = Campanha.ObterFase((int)fase);

            foreach (Slot slot in fas.Rodada1)
            {
                Personagem p = SelecaoSimbolo.ObterPersonagem(faccao, slot);
                if (!desbloqueados.Contains(p))
                    desbloqueados.Add(p);
            }

            foreach (Slot slot in fas.Rodada2)
            {
                Personagem p = SelecaoSimbolo.ObterPersonagem(faccao, slot);
                if (!desbloqueados.Contains(p))
                    desbloqueados.Add(p);
            }
        }

        public static List<Personagem> ObterDesbloqueados() => desbloqueados;

        /// <summary>
        /// Solicita ao jogador a escolha de 4 campeões sem repetição e retorna o time selecionado
        /// </summary>
        public static List<Personagem> SelecionarTime()
        {
            var time = new List<Personagem>();
            var desbloqueados = ObterDesbloqueados();

            while (time.Count < 4)
            {
                Menu.MenuSelecaoTime(desbloqueados);
                Console.WriteLine($"Slot {time.Count + 1}/4 — já selecionados: {string.Join(" ", time.Select(p => p.Simbolo))}");

                if (int.TryParse(Console.ReadLine(), out int escolha) && escolha >= 1 && escolha <= desbloqueados.Count)
                {
                    Personagem selecionado = desbloqueados[escolha - 1];
                    if (time.Contains(selecionado))
                        Console.WriteLine("Campeão já selecionado, escolha outro.");
                    else
                        time.Add(selecionado);
                }
                else
                {
                    Console.WriteLine("Opção inválida.");
                }
            }
            return time;
        }
    }

    #endregion
}