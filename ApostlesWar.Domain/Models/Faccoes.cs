namespace ApostlesWar.Domain
{
    /// <summary>
    /// Tabela estática Facção → Símbolo (emoji). O NOME da facção vive no próprio enum `Faccao`
    /// (via `[Description]`, lido por `DescricaoDeEnum.Descricao()`), então aqui fica só o símbolo.
    /// Plural do enum, convenção do repo (ex.: `NaturezasDano` p/ `NaturezaDano`).
    /// </summary>
    public static class Faccoes
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
            { Faccao.Ascendentes, "❄️" }
        };

        public static string Simbolo(Faccao faccao) => simbolos[faccao];
    }
}
