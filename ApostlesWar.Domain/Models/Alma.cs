namespace ApostlesWar.Domain
{
    /// <summary>
    /// A moeda do nível DO APÓSTOLO: de onde ela cai, quanto custa cada estrela dele e quanto vale
    /// queimada. A escada em si (fusão, diluição, a forma da receita) é do <see cref="Material"/> —
    /// aqui fica só o que é da alma, e o irmão dela é o <see cref="Po"/>.
    ///
    /// A alma tem DOIS verbos e eles se policiam sozinhos: ela COMPRA a estrela (que destrava o
    /// nível) e ela QUEIMA como XP (que sobe dentro do destravado). Despejar XP num apóstolo sem
    /// estrela não faz nada — ele para no <see cref="Progressao.TetoPorEstrelas"/> dele. É por isso
    /// que a queima pode ser generosa sem furar o teto de dificuldade.
    ///
    /// Os números são os do docs/GDD-progressao.md §O MATERIAL e §O PEDÁGIO.
    /// </summary>
    public static class Alma
    {
        /// <summary>Quanto da faixa ATUAL a receita da estrela cobra.</summary>
        public const int CustoDaFaixa = 150;

        /// <summary>
        /// Quanto da PRÓXIMA faixa a receita cobra. <b>É o dial de dificuldade do jogo inteiro</b>:
        /// esta faixa cai a <see cref="QuedaAlta"/> por inimigo e é sempre ela que aperta, então
        /// mexer aqui move o farm de todas as quatro paredes de cima.
        /// </summary>
        public const int CustoDaProxima = 100;

        /// <summary>A 1ª estrela é faixa única — não existe "faixa anterior" à Comum.</summary>
        public const int CustoDaPrimeira = 250;

        // Quanto cai por inimigo derrotado, nas três faixas que a dificuldade alcança.
        public const int QuedaBaixa = 15;
        public const int QuedaMedia = 5;
        public const int QuedaAlta = 1;

        private static readonly TabelaDeMaterial Tabela =
            new(CustoDaPrimeira, CustoDaFaixa, CustoDaProxima);

        /// <summary>
        /// O que um inimigo derrotado derruba. <b>Por INIMIGO, não por fase</b> — a oferta escala com
        /// o quanto se joga, e é isso que faz a Arena alimentar o elenco sem regra nova. É a única
        /// diferença de torneira entre a alma e o <see cref="Po.QuedaPorFase"/>.
        /// </summary>
        public static IReadOnlyList<Custo> QuedaPorInimigo(Dificuldade dificuldade)
        {
            IReadOnlyList<Raridade> faixas = Material.FaixasQueCaem(dificuldade);
            return new[]
            {
                new Custo(faixas[0], QuedaBaixa),
                new Custo(faixas[1], QuedaMedia),
                new Custo(faixas[2], QuedaAlta),
            };
        }

        /// <summary>O que custa a <paramref name="estrela"/>-ésima estrela do apóstolo (1 a 6).</summary>
        public static IReadOnlyList<Custo> Receita(int estrela) => Material.Receita(Tabela, estrela);

        /// <summary>Quanta XP uma alma vale queimada: 1, 5, 25, 125, 625, 3.125.</summary>
        public static int XpPorAlma(Raridade raridade) => Material.ValorQueimado(raridade);
    }

    /// <summary>
    /// Um pedaço da barra de XP dentro de UM nível, em 0..100. Ver <see cref="Progressao.Trechos"/>.
    /// </summary>
    public record TrechoDeXp(int Nivel, int De, int Ate);
}
