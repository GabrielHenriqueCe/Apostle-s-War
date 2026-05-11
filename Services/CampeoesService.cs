using ApostlesWar;
using GHUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace v1_Apostle_s_War.Services
{
    internal class CampeoesService
    {
        #region Construtor

        private readonly PersonagemService _personagemService;
        private readonly CampanhaService _campanhaService;
        private readonly MenuService _menuService;
        private readonly CapitulosService _capitulosService;

        #endregion

        #region Campeoes

        private List<Personagem> desbloqueados = new List<Personagem>();

        public CampeoesService(PersonagemService personagemService, CampanhaService campanhaService, MenuService menuService, CapitulosService capitulosService)
        {
            _personagemService = personagemService;
            _campanhaService = campanhaService;
            _menuService = menuService;
            _capitulosService = capitulosService;

            desbloqueados.Add(_personagemService.ObterPersonagem(Faccao.Humanos, Slot.Slot1));
            desbloqueados.Add(_personagemService.ObterPersonagem(Faccao.Humanos, Slot.Slot2));
            desbloqueados.Add(_personagemService.ObterPersonagem(Faccao.Humanos, Slot.Slot3));
            desbloqueados.Add(_personagemService.ObterPersonagem(Faccao.Humanos, Slot.Slot4));
        }

        public List<Personagem> ObterDesbloqueados() => desbloqueados;

        public void DesbloquearCampeoes(Faccao faccao, Fases fase)
        {
            Fase fas = _campanhaService.ObterFase((int)fase);

            foreach (Slot slot in fas.Rodada1)
            {
                Personagem p = _personagemService.ObterPersonagem(faccao, slot);
                if (!desbloqueados.Contains(p))
                    desbloqueados.Add(p);
            }

            foreach (Slot slot in fas.Rodada2)
            {
                Personagem p = _personagemService.ObterPersonagem(faccao, slot);
                if (!desbloqueados.Contains(p))
                    desbloqueados.Add(p);
            }
        }

        /// <summary>
        /// Solicita ao jogador a escolha de 4 campeões sem repetição e retorna o time selecionado
        /// </summary>
        public List<Personagem> SelecionarTime()
        {
            var time = new List<Personagem>();
            var desbloqueados = ObterDesbloqueados();
            int selecionado = 1;

            while (time.Count < 4)
            {
                _menuService.MenuSelecaoTime(desbloqueados, selecionado);
                Console.WriteLine($"Slot {time.Count + 1}/4 — já selecionados: {string.Join(" ", time.Select(p => p.Simbolo))}");

                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Personagem escolhido = desbloqueados[selecionado - 1];
                    if (time.Contains(escolhido))
                        Console.WriteLine("Campeão já selecionado, escolha outro.");
                    else
                        time.Add(escolhido);
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    // tratar depois
                }
                else
                {
                    selecionado = ConsoleUtils.SelecionarComCursor(selecionado, 1, desbloqueados.Count, key.Key, false);
                }
            }
            return time;
        }

        public void CarregarCampeoes()
        {
            foreach (Capitulos cap in _capitulosService.ObterTodos())
            {
                foreach (Fases fase in Enum.GetValues<Fases>())
                {
                    if (cap.FaseConcluida[(int)fase - 1])
                    {
                        DesbloquearCampeoes(cap.Faccao, fase);
                    }
                }
            }
        }

        #endregion
    }
}
