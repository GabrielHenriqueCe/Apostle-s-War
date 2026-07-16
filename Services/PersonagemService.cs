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

            // Especial
            new Personagem(1, Faccao.Especial, "Cocô", "💩", 1200, 160, 200,
            new Descarga(),
            new Desentupidor(),
            new PassivaCoco()),

            new Personagem(2, Faccao.Especial, "Herói", "🦸", 800, 240, 200,
            new SalvandoDia(),
            new Super(),
            new Vigilante()),

            new Personagem(3, Faccao.Especial, "Vilão", "🦹", 1200, 200, 160,
            new DestruindoDia(),
            new Vilania(),
            new Sentenca()),

            new Personagem(4, Faccao.Especial, "T-Rex", "🦖", 1000, 160, 240,
            new Rugido(),
            new Pisada(),
            new PeleGrossa()),

            // Decaídos
            new Personagem(1, Faccao.Decaidos, "Morcego", "🦇", 800, 160, 280,
            new Mordida(),
            new RatoVoador(),
            new SedentoDeSangue()),

            new Personagem(2, Faccao.Decaidos, "Vampiro", "🧛", 800, 280, 160,
            new BatMan(),
            new CintoUtilidades(),
            new Drenagem()),

            new Personagem(3, Faccao.Decaidos, "Elfo", "🧝", 1400, 160, 160,
            new ArvoreDoMundo(),
            new Natureza(),
            new Espinhos()),

            new Personagem(4, Faccao.Decaidos, "Diabo", "😈", 1400, 160, 160,
            new Inferno(),
            new AnjoCaido(),
            new CresceComDor()),
 

            // Apóstolos
            new Personagem(1, Faccao.Apostolos, "Boneco de Neve", "⛄", 1000, 200, 200,
            new BolaDeNeve(),
            new Gelado(),
            new Derretendo()),

            new Personagem(2, Faccao.Apostolos, "Mímico", "🎭", 1000, 200, 200,
            new Imitacao(),
            new Copiando(),
            new Repetindo()),

            new Personagem(3, Faccao.Apostolos, "Anjo", "😇", 1200, 160, 200,
            new Celestial(),
            new Ceu(),
            new Bencao()),

            new Personagem(4, Faccao.Apostolos, "Papai Noel", "🎅", 1000, 200, 200,
            new SacoDePresente(),
            new FabricaDePresente(),
            new Surpresa()),
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