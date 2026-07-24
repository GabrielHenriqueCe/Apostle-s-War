using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;
using ApostlesWar.Champs.Humanos;
using ApostlesWar.Champs.Reino;
using ApostlesWar.Champs.LadoSombrio;
using ApostlesWar.Champs.Tecnologicos;
using ApostlesWar.Champs.Folclore;
using ApostlesWar.Champs.Misticos;
using ApostlesWar.Champs.Especial;
using ApostlesWar.Champs.Decaidos;
using ApostlesWar.Champs.Apostolos;
using ApostlesWar.Skills.Ativas;
using ApostlesWar.Skills.Passivas;

namespace ApostlesWar.Services
{
    internal class PersonagemService
    {
        #region Personagem

        List<Personagem> personagens = new List<Personagem>
        {
            // Humanos — facção migrada pra forma final (Champs/Humanos/), cada arquivo é a view.
            Operario.Definir(),
            Detetive.Definir(),
            Policial.Definir(),
            Sushiman.Definir(),


            // O Reino — facção migrada pra forma final (Champs/Reino/), cada arquivo é a view.
            Guarda.Definir(),
            Ninja.Definir(),
            Mago.Definir(),
            Rei.Definir(),


            // Lado Sombrio — facção migrada pra forma final (Champs/LadoSombrio/), cada arquivo é a view.
            Caveira.Definir(),
            Fantasma.Definir(),
            Abobora.Definir(),
            Zumbi.Definir(),

            // Tecnológicos — facção migrada pra forma final (Champs/Tecnologicos/), cada arquivo é a view.
            Invasor.Definir(),
            Alien.Definir(),
            Robo.Definir(),
            Cientista.Definir(),

            // Folclore — facção migrada pra forma final (Champs/Folclore/), cada arquivo é a view.
            Ogro.Definir(),
            Tengu.Definir(),
            Palhaco.Definir(),
            Troll.Definir(),
 

            // Místicos — facção migrada pra forma final (Champs/Misticos/), cada arquivo é a view.
            Genio.Definir(),
            Sereia.Definir(),
            Fada.Definir(),
            Dragao.Definir(),

            // Especial — facção migrada pra forma final (Champs/Especial/), cada arquivo é a view.
            Coco.Definir(),
            Heroi.Definir(),
            Vilao.Definir(),
            TRex.Definir(),

            // Decaídos — facção migrada pra forma final (Champs/Decaidos/), cada arquivo é a view.
            Morcego.Definir(),
            Vampiro.Definir(),
            Elfo.Definir(),
            Diabo.Definir(),


            // Apóstolos — facção migrada pra forma final (Champs/Apostolos/), cada arquivo é a view.
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