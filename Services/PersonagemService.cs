using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;
using ApostlesWar.Champs.Humanos;
using ApostlesWar.Champs.Reino;
using ApostlesWar.Champs.LadoSombrio;
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

            // Tecnológicos
            new Personagem(1, Faccao.Tecnologicos, "Invasor", "👾",  600, 240, 240,
            new Glitch(),
            new Barata(),
            new Virus()),

            new Personagem(2, Faccao.Tecnologicos, "Alien", "👽", 1200, 240, 120,
            new Abduzir(),
            new Galaxia(),
            new CarapacaAlienigena()),

            new Personagem(3, Faccao.Tecnologicos, "Robô", "🤖", 1200, 120, 240,
            new RaioX(),
            new Tecnology(),
            new ReparoAutomatico()),

            new Personagem(4, Faccao.Tecnologicos, "Cientista", "🧑‍🔬", 1000, 200, 200,
            new Quimica(),
            new Fisica(),
            new AnaliseCritica()),

            // Folclore
            new Personagem(1, Faccao.Folclore, "Ogro", "👹", 1400, 160, 160,
            new Esmagar(),
            new Quebrar(),
            new Intimidador()),

            new Personagem(2, Faccao.Folclore, "Tengu", "👺", 800, 280, 160,
            new CorteDeVento(),
            new Vendaval(),
            new Ventania()),

            new Personagem(3, Faccao.Folclore, "Palhaço", "🤡", 800, 160, 280,
            new Coringa(),
            new Circo(),
            new PiadaDeMauGosto()),

            new Personagem(4, Faccao.Folclore, "Troll", "🧌", 1200, 160, 200,
            new Pancada(),
            new Porradeiro(),
            new Ambicao()),
 

            // Místicos
            new Personagem(1, Faccao.Misticos, "Gênio", "🧞", 1400, 120, 200,
            new Desejo(),
            new Profecia(),
            new Realidade()),

            new Personagem(2, Faccao.Misticos, "Sereia", "🧜", 600, 280, 200,
            new CantoDeSereia(),
            new Atlantis(),
            new Aquagirl()),

            new Personagem(3, Faccao.Misticos, "Fada", "🧚", 1000, 280, 120,
            new Sininho(),
            new PoMagico(),
            new Voar()),

            new Personagem(4, Faccao.Misticos, "Dragão", "🐲", 1400, 200, 120,
            new SoproDoDragao(),
            new DragaoProtetor(),
            new PeleDeDragao()),

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