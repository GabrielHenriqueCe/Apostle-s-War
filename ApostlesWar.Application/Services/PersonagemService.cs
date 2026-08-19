using ApostlesWar.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using ApostlesWar.Domain.Apostolos.Humanos;
using ApostlesWar.Domain.Apostolos.Reino;
using ApostlesWar.Domain.Apostolos.LadoSombrio;
using ApostlesWar.Domain.Apostolos.Tecnologicos;
using ApostlesWar.Domain.Apostolos.Folclore;
using ApostlesWar.Domain.Apostolos.Misticos;
using ApostlesWar.Domain.Apostolos.Especial;
using ApostlesWar.Domain.Apostolos.Decaidos;
using ApostlesWar.Domain.Apostolos.Ascendentes;
using ApostlesWar.Domain.Skills.Ativas;
using ApostlesWar.Domain.Skills.Passivas;

namespace ApostlesWar.Application.Services
{
    public class PersonagemService
    {
        #region Personagem

        // O ROSTER, agrupado por facção na ordem da campanha. Cada apóstolo é um arquivo em
        // Apostolos/<Faccao>/, e o `Definir()` dele é a view: quem mexe num apóstolo mexe lá, não
        // aqui — esta lista só diz quem existe e em que ordem.
        List<Personagem> personagens = new List<Personagem>
        {
            // Humanos
            Operario.Definir(),
            Detetive.Definir(),
            Policial.Definir(),
            Sushiman.Definir(),


            // O Reino
            Guarda.Definir(),
            Ninja.Definir(),
            Mago.Definir(),
            Rei.Definir(),


            // Lado Sombrio
            Caveira.Definir(),
            Fantasma.Definir(),
            Abobora.Definir(),
            Zumbi.Definir(),

            // Tecnológicos
            Invasor.Definir(),
            Alien.Definir(),
            Robo.Definir(),
            Cientista.Definir(),

            // Folclore
            Ogro.Definir(),
            Tengu.Definir(),
            Palhaco.Definir(),
            Troll.Definir(),
 

            // Místicos
            Genio.Definir(),
            Sereia.Definir(),
            Fada.Definir(),
            Dragao.Definir(),

            // Especial
            Coco.Definir(),
            Heroi.Definir(),
            Vilao.Definir(),
            TRex.Definir(),

            // Decaídos
            Morcego.Definir(),
            Vampiro.Definir(),
            Elfo.Definir(),
            Diabo.Definir(),


            // Ascendentes
            BonecoDeNeve.Definir(),
            Mimico.Definir(),
            Anjo.Definir(),
            PapaiNoel.Definir(),
        };

        public PersonagemService()
        {
            // Injeta AtaqueBasico como A1 em quem não declara A1 própria.
            // Hoje: todos recebem. Futuro: quem tiver A1 customizada (IAtaquePrimario), pula.
            foreach (Personagem p in personagens)
            {
                bool jaTemA1 = p.Habilidades.OfType<IAtaquePrimario>().Any();
                if (!jaTemA1)
                    p.Habilidades.Insert(0, new AtaqueBasico());
            }
        }

        /// <summary>
        /// O ROSTER inteiro, as instâncias de verdade — é sobre elas que o nível é aplicado. Quem quer
        /// o pool pra montar time usa o <see cref="ApostolosService.TodosOsApostolos"/>, que responde a
        /// mesma coisa em ordem de facção; este aqui é o dono da lista.
        /// </summary>
        public IReadOnlyList<Personagem> Todos() => personagens;

        public Personagem ObterPersonagem(Faccao faccao, Slot slot)
        {
            return personagens.First(p => p.Faccao == faccao && p.Slot == (int)slot);
        }

        /// <summary>
        /// O apóstolo de um PAPEL dentro da facção — é assim que a campanha monta os inimigos
        /// (`Campanha`, que fala em tipo e não em slot). Só existe resposta porque cada facção tem
        /// exatamente um de cada tipo; o `CampanhaTests` guarda essa premissa.
        /// </summary>
        public Personagem ObterPorTipo(Faccao faccao, TipoDeApostolo tipo)
        {
            return personagens.First(p => p.Faccao == faccao && p.Tipo == tipo);
        }

        #endregion
    }
}