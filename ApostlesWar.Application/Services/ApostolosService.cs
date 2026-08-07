using ApostlesWar.Domain;
using ApostlesWar.Application.Portas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApostlesWar.Application.Services
{
    public class ApostolosService
    {
        #region Construtor

        private readonly PersonagemService _personagemService;
        private readonly CapitulosService _capitulosService;

        #endregion

        #region Apostolos

        private List<Personagem> desbloqueados = new List<Personagem>();

        public ApostolosService(PersonagemService personagemService, CapitulosService capitulosService)
        {
            _personagemService = personagemService;
            _capitulosService = capitulosService;

            Resetar();
        }

        /// <summary>
        /// Devolve o roster ao estado de jogo novo: só os 4 Humanos, o time com que todo mundo começa.
        ///
        /// Não apaga slot de save nenhum porque este service não TEM um — o roster é derivado do
        /// FaseConcluida dos capítulos (ver <see cref="CarregarApostolos"/>). Mas precisa do reset
        /// mesmo assim: sem ele, "excluir conta" deixava os 36 apóstolos liberados em memória, prontos
        /// pra reaparecer no picker de avatar e na montagem de time da campanha.
        ///
        /// É o mesmo caminho do construtor de propósito — "como começa" tem que ser uma resposta só.
        /// </summary>
        public void Resetar()
        {
            desbloqueados = new List<Personagem>
            {
                _personagemService.ObterPersonagem(Faccao.Humanos, Slot.Slot1),
                _personagemService.ObterPersonagem(Faccao.Humanos, Slot.Slot2),
                _personagemService.ObterPersonagem(Faccao.Humanos, Slot.Slot3),
                _personagemService.ObterPersonagem(Faccao.Humanos, Slot.Slot4),
            };
        }

        public List<Personagem> ObterDesbloqueados() => desbloqueados;

        /// <summary>
        /// Este apóstolo já foi conquistado? Casa por (Faccao, Slot) e não por referência: depois de um
        /// carregamento, os objetos da lista não são necessariamente os mesmos que o
        /// <see cref="PersonagemService"/> devolve.
        ///
        /// Público porque DOIS lugares fazem a mesma pergunta — o picker de avatar e o compêndio — e
        /// a resposta é uma só. Estava escrita só dentro do <see cref="PerfilService.PodeUsarAvatar"/>,
        /// onde o nome falava de avatar e escondia que a regra era de progressão.
        /// </summary>
        public bool EstaDesbloqueado(Personagem apostolo)
            => desbloqueados.Any(p => p.Faccao == apostolo.Faccao && p.Slot == apostolo.Slot);

        public void DesbloquearApostolos(Faccao faccao, Fases fase)
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
        public List<Personagem> TodosOsApostolos()
        {
            var todos = new List<Personagem>();
            foreach (Faccao faccao in Enum.GetValues<Faccao>())
                foreach (Slot slot in Enum.GetValues<Slot>())
                    todos.Add(_personagemService.ObterPersonagem(faccao, slot));
            return todos;
        }

        public void CarregarApostolos()
        {
            foreach (Capitulo cap in _capitulosService.ObterTodos())
            {
                foreach (Fases fase in Enum.GetValues<Fases>())
                {
                    if (cap.FaseConcluida[(int)fase - 1])
                    {
                        DesbloquearApostolos(cap.Faccao, fase);
                    }
                }
            }
        }

        #endregion
    }
}
