namespace ApostlesWar.Domain
{
    /// <summary>
    /// O BAILE do material, e ele é o MESMO nos dois objetos que sobem de nível: seis faixas, fusão
    /// 10:1 pra cima, diluição 1:5 pra baixo, uma estrela a cada dezena e um pedágio por estrela
    /// cobrando "muito da faixa atual + pouco da próxima".
    ///
    /// O que MUDA entre a <see cref="Alma"/> (do apóstolo) e o <see cref="Po"/> (do item) é só a
    /// TORNEIRA — de onde cai e quanto — e o tamanho da receita. Por isso a matemática mora aqui e
    /// cada um traz a própria <see cref="TabelaDeMaterial"/>: um só lugar pra corrigir a escada, e
    /// nenhuma chance de as duas moedas divergirem sem ninguém notar.
    ///
    /// Os números são os do docs/GDD-progressao.md §O MATERIAL e §O PEDÁGIO.
    /// </summary>
    public static class Material
    {
        /// <summary>O passo entre faixas, tanto na queima quanto na diluição.</summary>
        public const int Passo = 5;

        /// <summary>Quantas unidades de uma faixa fazem UMA da seguinte.</summary>
        public const int PorFusao = 10;

        /// <summary>Quantas estrelas se pode comprar ao todo — uma por dezena de nível.</summary>
        public const int EstrelaMaxima = 6;

        /// <summary>
        /// As três faixas que uma dificuldade derruba, da mais generosa à mais rara. É UMA regra
        /// deslizando: cada dificuldade pega três faixas consecutivas a partir do próprio valor do
        /// enum, e quem aperta é sempre a mais alta.
        ///
        /// <b>Nenhuma dificuldade derruba as faixas ABAIXO da sua.</b> O Pesadelo não produz Comum, e
        /// a 1ª estrela é Comum puro — quem começa do zero lá em cima depende da <see cref="Diluir"/>.
        /// </summary>
        public static IReadOnlyList<Raridade> FaixasQueCaem(Dificuldade dificuldade)
        {
            int piso = (int)dificuldade - 1;
            return new[] { (Raridade)piso, (Raridade)(piso + 1), (Raridade)(piso + 2) };
        }

        /// <summary>
        /// A faixa mais alta que a fusão pode PRODUZIR, dada a dificuldade mais alta já aberta: a
        /// mesma que aquela dificuldade derruba.
        ///
        /// <b>É esta trava que impede fabricar mítico farmando o Fácil</b> (§O MATERIAL). Sem ela a
        /// fusão fura o teto de dificuldade inteiro pela porta dos fundos: 10.000 Comuns virariam a
        /// estrela que só o Pesadelo deveria pagar. Vale para as duas moedas — pó inclusive, senão o
        /// item mítico se fabrica onde o apóstolo mítico não se fabrica.
        /// </summary>
        public static Raridade TetoDeFusao(Dificuldade maisAlta) => FaixasQueCaem(maisAlta)[^1];

        /// <summary>
        /// O que custa a <paramref name="estrela"/>-ésima estrela (1 a 6), pela tabela de quem paga:
        /// muito da faixa atual, pouco da próxima. É a faixa da PONTA que prende a compra à
        /// dificuldade — não se paga a 4ª sem Épico, e Épico não cai no Fácil, e é assim que o teto
        /// de nível de cada dificuldade existe sem uma linha escrita dizendo "teto".
        ///
        /// A 1ª é faixa única: não existe "faixa anterior" à Comum.
        /// </summary>
        public static IReadOnlyList<Custo> Receita(TabelaDeMaterial tabela, int estrela)
        {
            if (estrela < 1 || estrela > EstrelaMaxima)
                throw new ArgumentOutOfRangeException(nameof(estrela));

            if (estrela == 1) return new[] { new Custo(Raridade.Comum, tabela.CustoDaPrimeira) };

            return new[]
            {
                new Custo((Raridade)(estrela - 2), tabela.CustoDaFaixa),
                new Custo((Raridade)(estrela - 1), tabela.CustoDaProxima),
            };
        }

        /// <summary>
        /// Quanto uma unidade vale QUEIMADA: <see cref="Passo"/> elevado à faixa — 1, 5, 25, 125,
        /// 625, 3.125. É XP quando a moeda é alma e ponto de nível quando é pó.
        /// </summary>
        public static int ValorQueimado(Raridade raridade)
        {
            int valor = 1;
            for (int i = 0; i < (int)raridade; i++) valor *= Passo;
            return valor;
        }

        /// <summary>
        /// Desce uma faixa: 1 vira <see cref="Passo"/> da anterior. Perde contra a fusão de propósito
        /// (10 pra cima, 5 pra baixo), então o ida-e-volta come metade — ter a moeda CERTA vale mais
        /// que ter volume. A Comum não desce.
        /// </summary>
        public static Custo? Diluir(Raridade raridade, int quantidade)
            => raridade == Raridade.Comum || quantidade <= 0
                ? null
                : new Custo(raridade - 1, quantidade * Passo);

        /// <summary>
        /// Sobe uma faixa: <see cref="PorFusao"/> viram 1 da seguinte. O resto que não fecha um grupo
        /// não é consumido. A Mítica não sobe.
        /// </summary>
        public static Custo? Fundir(Raridade raridade, int quantidade)
            => raridade == Raridade.Mitico || quantidade < PorFusao
                ? null
                : new Custo(raridade + 1, quantidade / PorFusao);
    }

    /// <summary>
    /// O preço das seis estrelas de uma moeda. Três números governam a tabela inteira, e o
    /// <see cref="CustoDaProxima"/> é o DIAL DE DIFICULDADE: é a faixa que cai mais devagar, então é
    /// sempre ela que aperta, e mexer nela move o farm de todas as paredes de cima de uma vez.
    /// </summary>
    public record TabelaDeMaterial(int CustoDaPrimeira, int CustoDaFaixa, int CustoDaProxima);

    /// <summary>Uma quantia de material de uma faixa. Serve de queda, de preço e de saldo.</summary>
    public record Custo(Raridade Raridade, int Quantidade);
}
