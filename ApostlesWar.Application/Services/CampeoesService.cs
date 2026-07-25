using ApostlesWar.Domain;
using ApostlesWar.Application.Portas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApostlesWar.Application.Services
{
    public class CampeoesService
    {
        #region Construtor

        private readonly PersonagemService _personagemService;
        private readonly CapitulosService _capitulosService;

        #endregion

        #region Campeoes

        private List<Personagem> desbloqueados = new List<Personagem>();

        public CampeoesService(PersonagemService personagemService, CapitulosService capitulosService)
        {
            _personagemService = personagemService;
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
        /// O pool COMPLETO (9 facções × 4 slots) — laboratório da Arena: qualquer matchup,
        /// independente do progresso da campanha. É DADO: quem escolhe dele é a tela, que manda de
        /// volta o time já montado (`ExecutarArenaComTimes`). O service não pergunta nada a ninguém.
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
