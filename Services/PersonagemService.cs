using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;
using v1_Apostle_s_War.Skills.Ativas;
using v1_Apostle_s_War.Skills.Passivas;

namespace v1_Apostle_s_War.Services
{
    internal class PersonagemService
    {
        #region Personagem

        List<Personagem> personagens = new List<Personagem>
        {
            // Humanos
            new Personagem(1, Faccao.Humanos, "Operário", "👷",  1200, 240, 120,
            new ParedeDeTijolos(),
            new Marretada(),
            new PassivaOperario()),

            new Personagem(2, Faccao.Humanos, "Detetive", "🕵️", 1400, 160, 160,
            new Espionagem(),
            new Furtividade(),
            new OlhoClinico()),

            new Personagem(3, Faccao.Humanos, "Policial", "👮",  1000, 120, 280,
            new Tiroteio(),
            new Prender(),
            new PassivaPolicial()),

            new Personagem(4, Faccao.Humanos, "Sushiman ", "👲",  800, 280, 160,
            new Sushi(),
            new Nigiri(),
            new PassivaSushiman()),

            
            // O Reino
            new Personagem(1, Faccao.Reino, "Guarda", "💂", 1200, 160, 200,
            new Protetor(),
            new Esgrima(),
            new PassivaGuarda()),

            new Personagem(2, Faccao.Reino, "Ninja", "🥷", 600, 280, 200,
            new Shuriken(),
            new Kunai(),
            new Sorrateiro()),

            new Personagem(3, Faccao.Reino, "Mago", "🧙", 1000, 280, 120,
            new BolaDeFogo(),
            new Incendio(),
            new PassivaPiromancer()),

            new Personagem(4, Faccao.Reino, "Rei", "🫅", 1000, 200, 200,
            new Democracia(),
            new Lealdade(),
            new CoroaDoSoberano()),


            // Lado Sombrio
            new Personagem(1, Faccao.LadoSombrio, "Caveira", "💀",  600, 280, 200,
            new Ossinho(),
            new OssoDuroDeRoer(),
            new Necromancia()),

            new Personagem(2, Faccao.LadoSombrio, "Fantasma", "👻", 1400, 120, 200,
            new Assombracao(),
            new VindoDoAlem(),
            new PassivaFantasma()),

            new Personagem(3, Faccao.LadoSombrio, "Abóbora", "🎃",  600, 200, 280,
            new DocesOuTravessuras(),
            new DocesDeAbobora(),
            new PassivaAbobora()),

            new Personagem(4, Faccao.LadoSombrio, "Zumbi", "🧟", 1400, 200, 120,
            new Fedorento(),
            new Putridao(),
            new PassivaZumbi()),

            // Tecnológicos
            new Personagem(1, Faccao.Tecnologicos, "Invasor", "👾",  600, 240, 240,
            new Glitch(),
            new Barata(),
            new Virus()),

            new Personagem(2, Faccao.Tecnologicos, "Alien", "👽", 1200, 240, 120,
            new Abduzir(),
            new Galaxia(),
            new PassivaAlien()),

            new Personagem(3, Faccao.Tecnologicos, "Robô", "🤖", 1200, 120, 240,
            new RaioX(),
            new Tecnology(),
            new PassivaRobo()),

            new Personagem(4, Faccao.Tecnologicos, "Cientista", "🧑‍🔬", 1000, 200, 200,
            new Quimica(),
            new Fisica(),
            new PassivaCientista()),

            // Folclore
            new Personagem(1, Faccao.Folclore, "Ogro", "👹", 1400, 160, 160,
            new Esmagar(),
            new Quebrar(),
            new PassivaOgro()),

            new Personagem(2, Faccao.Folclore, "Tengu", "👺", 800, 280, 160,
            new CorteDeVento(),
            new Vendaval(),
            new PassivaTengu()),

            new Personagem(3, Faccao.Folclore, "Palhaço", "🤡", 800, 160, 280,
            new Coringa(),
            new Circo(),
            new PassivaPalhaco()),

            new Personagem(4, Faccao.Folclore, "Troll", "🧌", 1200, 160, 200,
            new Pancada(),
            new Porradeiro(),
            new Ambicao()),
 

            // Místicos
            new Personagem(1, Faccao.Misticos, "Gênio", "🧞", 1400, 120, 200,
            new Desejo(),
            new Profecia(),
            new PassivaGenio()),

            new Personagem(2, Faccao.Misticos, "Sereia", "🧜", 600, 280, 200,
            new CantoDeSereia(),
            new Atlantis(),
            new PassivaSereia()),

            new Personagem(3, Faccao.Misticos, "Fada", "🧚", 1000, 280, 120,
            new Sininho(),
            new PoMagico(),
            new PassivaFada()),

            new Personagem(4, Faccao.Misticos, "Dragão", "🐲", 1400, 200, 120,
            new SoproDoDragao(),
            new DragaoProtetor(),
            new PassivaDragao()),

            // Especial
            new Personagem(1, Faccao.Especial, "Cocô", "💩", 1200, 160, 200,
            new Descarga(),
            new Desentupidor(),
            new PassivaCoco()),

            new Personagem(2, Faccao.Especial, "Herói", "🦸", 800, 240, 200,
            new SalvandoDia(),
            new Super(),
            new PassivaHeroi()),

            new Personagem(3, Faccao.Especial, "Vilão", "🦹", 1200, 200, 160,
            new DestruindoDia(),
            new Vilania(),
            new PassivaVilao()),

            new Personagem(4, Faccao.Especial, "T-Rex", "🦖", 1000, 160, 240,
            new Rugido(),
            new Pisada(),
            new PassivaTRex()),

            // Decaídos
            new Personagem(1, Faccao.Decaidos, "Morcego", "🦇", 800, 160, 280,
            new Mordida(),
            new RatoVoador(),
            new PassivaMorcego()),

            new Personagem(2, Faccao.Decaidos, "Vampiro", "🧛", 800, 280, 160,
            new BatMan(),
            new CintoUtilidades(),
            new PassivaVampiro()),

            new Personagem(3, Faccao.Decaidos, "Elfo", "🧝", 1400, 160, 160,
            new ArvoreDoMundo(),
            new Natureza(),
            new PassivaElfo()),

            new Personagem(4, Faccao.Decaidos, "Diabo", "😈", 1400, 160, 160,
            new Inferno(),
            new AnjoCaido(),
            new PassivaDiabo()),
 

            // Apóstolos
            new Personagem(1, Faccao.Apostolos, "Boneco de Neve", "⛄", 1000, 200, 200,
            new BolaDeNeve(),
            new Gelado(),
            new PassivaBonecoDeNeve()),

            new Personagem(2, Faccao.Apostolos, "Mímico", "🎭", 1000, 200, 200,
            new Imitacao(),
            new Copiando(),
            new PassivaMimico()),

            new Personagem(3, Faccao.Apostolos, "Anjo", "😇", 1200, 160, 200,
            new Celestial(),
            new Ceu(),
            new PassivaAnjo()),

            new Personagem(4, Faccao.Apostolos, "Papai Noel", "🎅", 1000, 200, 200,
            new SacoDePresente(),
            new FabricaDePresente(),
            new PassivaPapaiNoel()),
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