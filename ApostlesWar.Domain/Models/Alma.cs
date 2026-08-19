namespace ApostlesWar.Domain
{
    /// <summary>
    /// A moeda do NÍVEL: quanto cai por inimigo, quanto custa cada estrela, quanto vale queimada e
    /// como ela desce de faixa. Função pura e sem estado, irmã do <see cref="Progressao"/> — quem
    /// guarda saldo é o `AlmaService`.
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

        /// <summary>O passo entre faixas, tanto na queima quanto na diluição.</summary>
        public const int Passo = 5;

        /// <summary>Quantas estrelas um apóstolo pode comprar ao todo.</summary>
        public const int EstrelaMaxima = 6;

        /// <summary>
        /// O que um inimigo derrotado derruba. A tabela é UMA regra deslizando: cada dificuldade
        /// pega três faixas consecutivas a partir do próprio valor do enum
        /// (<see cref="Dificuldade"/>), e quem aperta é sempre a mais alta.
        ///
        /// <b>Nenhuma dificuldade derruba as faixas ABAIXO da sua.</b> O Pesadelo não produz Comum, e
        /// a 1ª estrela é Comum puro — quem tira um apóstolo novo do zero lá em cima depende da
        /// <see cref="Diluir"/>. Apagar a diluição fecha esse caminho sem quebrar nenhum teste óbvio.
        /// </summary>
        public static IReadOnlyList<Custo> QuedaPorInimigo(Dificuldade dificuldade)
        {
            int piso = (int)dificuldade - 1;
            return new[]
            {
                new Custo((Raridade)piso, QuedaBaixa),
                new Custo((Raridade)(piso + 1), QuedaMedia),
                new Custo((Raridade)(piso + 2), QuedaAlta),
            };
        }

        /// <summary>
        /// O que custa a <paramref name="estrela"/>-ésima estrela (1 a 6): muito da faixa atual, pouco
        /// da próxima. É a faixa da PONTA que prende a compra à dificuldade — não se paga a 4ª sem
        /// Épico, e Épico não cai no Fácil, e é assim que o teto 30 do Fácil existe sem regra escrita.
        /// </summary>
        public static IReadOnlyList<Custo> Receita(int estrela)
        {
            if (estrela < 1 || estrela > EstrelaMaxima)
                throw new ArgumentOutOfRangeException(nameof(estrela));

            if (estrela == 1) return new[] { new Custo(Raridade.Comum, CustoDaPrimeira) };

            return new[]
            {
                new Custo((Raridade)(estrela - 2), CustoDaFaixa),
                new Custo((Raridade)(estrela - 1), CustoDaProxima),
            };
        }

        /// <summary>
        /// Quanta XP uma alma vale queimada: <see cref="Passo"/> elevado à faixa — 1, 5, 25, 125,
        /// 625, 3.125.
        /// </summary>
        public static int XpPorAlma(Raridade raridade)
        {
            int xp = 1;
            for (int i = 0; i < (int)raridade; i++) xp *= Passo;
            return xp;
        }

        /// <summary>
        /// Desce uma faixa: 1 vira <see cref="Passo"/> da anterior. Perde contra a fusão de propósito
        /// (10 pra cima, 5 pra baixo), então o ida-e-volta come metade. A Comum não desce.
        /// </summary>
        public static Custo? Diluir(Raridade raridade, int quantidade)
            => raridade == Raridade.Comum || quantidade <= 0
                ? null
                : new Custo(raridade - 1, quantidade * Passo);

        /// <summary>Quantas almas de uma faixa fazem UMA da seguinte.</summary>
        public const int PorFusao = 10;

        /// <summary>
        /// Sobe uma faixa: <see cref="PorFusao"/> viram 1 da seguinte. O resto que não fecha um grupo
        /// não é consumido. A Mítica não sobe.
        /// </summary>
        public static Custo? Fundir(Raridade raridade, int quantidade)
            => raridade == Raridade.Mitico || quantidade < PorFusao
                ? null
                : new Custo(raridade + 1, quantidade / PorFusao);

        /// <summary>
        /// A faixa mais alta que a fusão pode PRODUZIR, dada a dificuldade mais alta que o jogador já
        /// abriu: a mesma que aquela dificuldade derruba (<see cref="QuedaPorInimigo"/>).
        ///
        /// <b>É esta trava que impede fabricar mítico farmando o Fácil</b> (docs/GDD-progressao.md
        /// §O MATERIAL). Sem ela a fusão fura o teto de dificuldade inteiro pela porta dos fundos:
        /// 10.000 Comuns virariam a estrela que só o Pesadelo deveria pagar.
        /// </summary>
        public static Raridade TetoDeFusao(Dificuldade maisAlta)
            => QuedaPorInimigo(maisAlta)[^1].Raridade;
    }

    /// <summary>Uma quantia de alma de uma faixa. Serve de queda, de preço e de saldo.</summary>
    public record Custo(Raridade Raridade, int Quantidade);

    /// <summary>
    /// Um pedaço da barra de XP dentro de UM nível, em 0..100. Ver <see cref="Progressao.Trechos"/>.
    /// </summary>
    public record TrechoDeXp(int Nivel, int De, int Ate);
}
