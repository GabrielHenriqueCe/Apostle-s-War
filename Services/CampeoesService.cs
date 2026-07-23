using ApostlesWar;
using ApostlesWar.View;
using GHUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApostlesWar.Services
{
    internal class CampeoesService
    {
        #region Construtor

        private readonly PersonagemService _personagemService;
        private readonly MenuView _menuView;
        private readonly CapitulosService _capitulosService;

        #endregion

        #region Campeoes

        private List<Personagem> desbloqueados = new List<Personagem>();

        public CampeoesService(PersonagemService personagemService, MenuView menuService, CapitulosService capitulosService)
        {
            _personagemService = personagemService;
            _menuView = menuService;
            _capitulosService = capitulosService;

            desbloqueados.Add(_personagemService.ObterPersonagem(Faccao.Humanos, Slot.Slot1));
            desbloqueados.Add(_personagemService.ObterPersonagem(Faccao.Humanos, Slot.Slot2));
            desbloqueados.Add(_personagemService.ObterPersonagem(Faccao.Humanos, Slot.Slot3));
            desbloqueados.Add(_personagemService.ObterPersonagem(Faccao.Humanos, Slot.Slot4));
        }

        public List<Personagem> ObterDesbloqueados() => desbloqueados;

        public void DesbloquearCampeoes(Faccao faccao, Fases fase)
        {
            Fase fas = Campanha.ObterFase((int)fase);

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
            return _menuView.NavegarSelecaoTime(ObterDesbloqueados());
        }

        /// <summary>TODOS os campeões (9 facções × 4 slots) — pool da Arena (laboratório de rebalance:
        /// qualquer matchup, independente do progresso da campanha). Vazio = jogador desistiu.</summary>
        public List<Personagem> SelecionarTimeArena()
        {
            var todos = new List<Personagem>();
            foreach (Faccao faccao in Enum.GetValues<Faccao>())
                foreach (Slot slot in Enum.GetValues<Slot>())
                    todos.Add(_personagemService.ObterPersonagem(faccao, slot));
            return _menuView.NavegarSelecaoTime(todos);
        }

        public void CarregarCampeoes()
        {
            foreach (Capitulo cap in _capitulosService.ObterTodos())
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
