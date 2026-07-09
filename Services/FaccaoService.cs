using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApostlesWar.Services
{
    internal class FaccaoService
    {
        #region Faction

        Dictionary<Faccao, (string Nome, string Simbolo)> mapa = new Dictionary<Faccao, (string, string)>
        {
            { Faccao.Humanos, ("Humanos", "🛠️") },
            { Faccao.Reino, ("Reino", "👑") },
            { Faccao.LadoSombrio, ("Lado Sombrio", "🌑") },
            { Faccao.Tecnologicos, ("Tecnológicos", "⚙️") },
            { Faccao.Folclore, ("Folclore", "🪬") },
            { Faccao.Misticos, ("Místicos", "🐉") },
            { Faccao.Especial, ("Especial", "⭐") },
            { Faccao.Decaidos, ("Decaídos", "🔱") },
            { Faccao.Apostolos, ("Apóstolos", "🌬️") }
        };
        public string ObterSimbolo(Faccao faccao) => mapa[faccao].Simbolo;
        public string ObterNome(Faccao faccao) => mapa[faccao].Nome;

        #endregion
    }
}
