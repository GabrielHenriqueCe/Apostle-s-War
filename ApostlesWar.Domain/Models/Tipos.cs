namespace ApostlesWar.Domain
{
    /// <summary>
    /// Tabela estática Tipo → Símbolo (emoji). O NOME do tipo vive no próprio enum
    /// <see cref="TipoDeApostolo"/> (via `[Description]`, lido por `DescricaoDeEnum.Descricao()`),
    /// então aqui fica só o símbolo. Plural do enum, convenção do repo (ex.: <see cref="Faccoes"/>).
    ///
    /// Os quatro emojis são os do docs/GDD-combate.md §2 — o doc já falava neles, aqui eles viram dado.
    /// </summary>
    public static class Tipos
    {
        private static readonly Dictionary<TipoDeApostolo, string> simbolos = new()
        {
            { TipoDeApostolo.Guardiao, "🛡️" },
            { TipoDeApostolo.Combatente, "⚔️" },
            { TipoDeApostolo.Atirador, "🏹" },
            { TipoDeApostolo.Suporte, "💗" }
        };

        public static string Simbolo(TipoDeApostolo tipo) => simbolos[tipo];
    }
}
