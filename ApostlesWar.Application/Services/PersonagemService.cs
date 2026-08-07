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

        List<Personagem> personagens = new List<Personagem>
        {
            // Humanos — facção migrada pra forma final (Apostolos/Humanos/), cada arquivo é a view.
            Operario.Definir(),
            Detetive.Definir(),
            Policial.Definir(),
            Sushiman.Definir(),


            // O Reino — facção migrada pra forma final (Apostolos/Reino/), cada arquivo é a view.
            Guarda.Definir(),
            Ninja.Definir(),
            Mago.Definir(),
            Rei.Definir(),


            // Lado Sombrio — facção migrada pra forma final (Apostolos/LadoSombrio/), cada arquivo é a view.
            Caveira.Definir(),
            Fantasma.Definir(),
            Abobora.Definir(),
            Zumbi.Definir(),

            // Tecnológicos — facção migrada pra forma final (Apostolos/Tecnologicos/), cada arquivo é a view.
            Invasor.Definir(),
            Alien.Definir(),
            Robo.Definir(),
            Cientista.Definir(),

            // Folclore — facção migrada pra forma final (Apostolos/Folclore/), cada arquivo é a view.
            Ogro.Definir(),
            Tengu.Definir(),
            Palhaco.Definir(),
            Troll.Definir(),
 

            // Místicos — facção migrada pra forma final (Apostolos/Misticos/), cada arquivo é a view.
            Genio.Definir(),
            Sereia.Definir(),
            Fada.Definir(),
            Dragao.Definir(),

            // Especial — facção migrada pra forma final (Apostolos/Especial/), cada arquivo é a view.
            Coco.Definir(),
            Heroi.Definir(),
            Vilao.Definir(),
            TRex.Definir(),

            // Decaídos — facção migrada pra forma final (Apostolos/Decaidos/), cada arquivo é a view.
            Morcego.Definir(),
            Vampiro.Definir(),
            Elfo.Definir(),
            Diabo.Definir(),


            // Ascendentes — facção migrada pra forma final (Apostolos/Ascendentes/), cada arquivo é a view.
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

        public Personagem ObterPersonagem(Faccao faccao, Slot slot)
        {
            return personagens.First(p => p.Faccao == faccao && p.Slot == (int)slot);
        }

        #endregion
    }
}