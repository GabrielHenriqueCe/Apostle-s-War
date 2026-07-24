namespace ApostlesWar
{
    /// <summary>
    /// Tabela estática Facção → Símbolo (emoji). O NOME da facção vive no próprio enum `Faccao`
    /// (via `[Description]`, lido por Helper.GetDescricao), então aqui fica só o símbolo. Era o
    /// `FaccaoService` (tabela pura disfarçada de service injetável); virou dado. Plural do enum,
    /// convenção do repo (ex.: `NaturezasDano` p/ `NaturezaDano`).
    /// </summary>
    static class Faccoes
    {
        private static readonly Dictionary<Faccao, string> simbolos = new()
        {
            { Faccao.Humanos, "🛠️" },
            { Faccao.Reino, "👑" },
            { Faccao.LadoSombrio, "🌑" },
            { Faccao.Tecnologicos, "⚙️" },
            { Faccao.Folclore, "🪬" },
            { Faccao.Misticos, "🐉" },
            { Faccao.Especial, "⭐" },
            { Faccao.Decaidos, "🔱" },
            { Faccao.Apostolos, "🌬️" }
        };

        public static string Simbolo(Faccao faccao) => simbolos[faccao];
    }
}
