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
            new PassivaDetetive()), 

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
            new PassivaSorrateiro()),

            new Personagem(3, Faccao.Reino, "Mago", "🧙", 1000, 280, 120,
            new BolaDeFogo(),
            new Incendio(),
            new PassivaPiromancer()),

            new Personagem(4, Faccao.Reino, "Rei", "🫅", 1000, 200, 200,
            new Democracia(),
            new Lealdade(),
            new PassivaRei()),


            // Lado Sombrio
            new Personagem(1, Faccao.LadoSombrio, "Caveira", "💀",  600, 280, 200, new Necromancia()),
            new Personagem(2, Faccao.LadoSombrio, "Fantasma", "👻", 1400, 120, 200),
            new Personagem(3, Faccao.LadoSombrio, "Abóbora", "🎃",  600, 200, 280),
            new Personagem(4, Faccao.LadoSombrio, "Zumbi", "🧟", 1400, 200, 120),

            // Tecnológicos
            new Personagem(1, Faccao.Tecnologicos, "Invasor", "👾",  600, 240, 240),
            new Personagem(2, Faccao.Tecnologicos, "Alien", "👽", 1200, 240, 120),
            new Personagem(3, Faccao.Tecnologicos, "Robô", "🤖", 1200, 120, 240),
            new Personagem(4, Faccao.Tecnologicos, "Cientista", "🧑‍🔬", 1000, 200, 200),

            // Folclore
            new Personagem(1, Faccao.Folclore, "Ogro", "👹", 1400, 160, 160),
            new Personagem(2, Faccao.Folclore, "Tengu", "👺",  800, 280, 160),
            new Personagem(3, Faccao.Folclore, "Palhaço", "🤡",  800, 160, 280),
            new Personagem(4, Faccao.Folclore, "Troll", "🧌", 1200, 160, 200),

            // Místicos
            new Personagem(1, Faccao.Misticos, "Gênio", "🧞", 1400, 120, 200),
            new Personagem(2, Faccao.Misticos, "Sereia", "🧜",  600, 280, 200),
            new Personagem(3, Faccao.Misticos, "Fada", "🧚", 1000, 280, 120),
            new Personagem(4, Faccao.Misticos, "Dragão", "🐲", 1400, 200, 120),

            // Especial
            new Personagem(1, Faccao.Especial, "Cocô", "💩", 1200, 160, 200),
            new Personagem(2, Faccao.Especial, "Herói", "🦸",  800, 240, 200),
            new Personagem(3, Faccao.Especial, "Vilão", "🦹", 1200, 200, 160),
            new Personagem(4, Faccao.Especial, "T-Rex", "🦖", 1000, 160, 240),

            // Decaídos 
            new Personagem(1, Faccao.Decaidos, "Morcego", "🦇",  800, 160, 280),
            new Personagem(2, Faccao.Decaidos, "Vampiro", "🧛",  800, 280, 160),
            new Personagem(3, Faccao.Decaidos, "Elfo", "🧝", 1400, 160, 160),
            new Personagem(4, Faccao.Decaidos, "Diabo", "😈", 1400, 160, 160),

            // Apóstolos
            new Personagem(1, Faccao.Apostolos, "Boneco de Neve", "☃️",  600, 240, 240),
            new Personagem(2, Faccao.Apostolos, "Mímico", "🎭", 1200, 240, 120),
            new Personagem(3, Faccao.Apostolos, "Anjo", "👼", 1200, 120, 240),
            new Personagem(4, Faccao.Apostolos, "Papai Noel", "🎅", 1400, 160, 160),
        };

        /// <summary>
        /// Retorna o personagem correspondente à facção e slot informados
        /// </summary>
        /// <param name="faccao">Facção do personagem</param>
        /// <param name="slot">Slot do personagem dentro da facção</param>
        public Personagem ObterPersonagem(Faccao faccao, Slot slot)
        {
            return personagens.First(p => p.Faccao == faccao && p.Slot == (int)slot);
        }

        #endregion
    }
}
