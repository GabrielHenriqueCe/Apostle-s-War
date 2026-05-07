namespace ApostlesWar
{
    #region Faction

    /// <summary>
    /// Mapeia facções para nome e símbolo
    /// </summary>
    class Faction
    {
        private static readonly Dictionary<Faccao, (string Nome, string Simbolo)> mapa = new Dictionary<Faccao, (string, string)>
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
        public static string ObterSimbolo(Faccao faccao) => mapa[faccao].Simbolo;
        public static string ObterNome(Faccao faccao) => mapa[faccao].Nome;
    }

    #endregion
}