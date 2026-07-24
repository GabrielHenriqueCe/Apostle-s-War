using ApostlesWar.Domain;
using ApostlesWar.Application.Portas;
using GHUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApostlesWar.Application.Services
{
    public class CampeoesService
    {
        #region Construtor

        private readonly PersonagemService _personagemService;
        private readonly ITelaDeMenu _menuView;
        private readonly CapitulosService _capitulosService;

        #endregion

        #region Campeoes

        private List<Personagem> desbloqueados = new List<Personagem>();

        public CampeoesService(PersonagemService personagemService, ITelaDeMenu menuService, CapitulosService capitulosService)
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
        public List<Personagem> SelecionarTimeArena() => _menuView.NavegarSelecaoTime(TodosOsCampeoes());

        /// <summary>
        /// O pool COMPLETO (9 facções × 4 slots), sem passar por tela nenhuma. Separado do
        /// SelecionarTimeArena porque montar a lista é DADO e escolher dela é TELA: o front sorteia
        /// daqui pra cair direto numa batalha, sem depender do menu de console.
        /// </summary>
        public List<Personagem> TodosOsCampeoes()
        {
            var todos = new List<Personagem>();
            foreach (Faccao faccao in Enum.GetValues<Faccao>())
                foreach (Slot slot in Enum.GetValues<Slot>())
                    todos.Add(_personagemService.ObterPersonagem(faccao, slot));
            return todos;
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
