namespace ApostlesWar.Domain
{
    /// <summary>
    /// A moeda do nível DO ITEM — irmã da <see cref="Alma"/>, mesma escada (<see cref="Material"/>),
    /// outra torneira: o pó cai por FASE concluída, não por inimigo.
    ///
    /// Aqui mora também a economia do nível do item, que a alma não tem equivalente: o item ganha
    /// nível de USO, por ciclo de combate. O pó tem DOIS destinos que competem — o pedágio de cada
    /// dezena (a têmpera) e acelerar o nível na bigorna (<see cref="PontosPorPo"/>) —, e é essa
    /// competição que faz cada punhado de pó ser uma escolha em vez de um acúmulo.
    ///
    /// <b>A RECEITA É MAIS BARATA QUE A DA ALMA DE PROPÓSITO, e o motivo é contagem.</b> A alma paga
    /// 1 objeto por apóstolo; o pó paga 7 (um por slot), 28 pra vestir um time de quatro. Com as
    /// constantes da alma, cada parede custaria sete vezes mais — que é exatamente a explosão de
    /// demanda que o §O MATERIAL teve de desarmar do lado do apóstolo.
    ///
    /// Os números são os do docs/GDD-itens.md §Como o nível sobe e docs/GDD-progressao.md §O PEDÁGIO.
    /// </summary>
    public static class Po
    {
        // A receita da estrela do item. Um quinto da alma, pelos 7 slots — ver o resumo da classe.
        public const int CustoDaPrimeira = 50;
        public const int CustoDaFaixa = 30;

        /// <summary>
        /// O dial de dificuldade do lado do item, pelo mesmo motivo do <see cref="Alma.CustoDaProxima"/>:
        /// é a faixa que cai mais devagar, então é sempre ela que aperta.
        /// </summary>
        public const int CustoDaProxima = 20;

        // Quanto cai por FASE concluída, nas três faixas que a dificuldade alcança. O volume é maior
        // que o da alma porque a torneira abre menos vezes: 56 fases contra 328 inimigos numa passada.
        public const int QuedaBaixa = 60;
        public const int QuedaMedia = 20;
        public const int QuedaAlta = 4;

        /// <summary>
        /// O custo BASE de um nível de item, na faixa 1–9. Dobra a cada dezena
        /// (ver <see cref="CustoDoNivel"/>), então a última dezena custa 32× a primeira.
        /// </summary>
        public const int CustoBaseDoNivel = 10;

        /// <summary>
        /// O teto de ciclos que UMA fase paga. Ele não existe pra impedir luta longa — a medição de
        /// ago/2026 achou 31 ciclos numa fase vencida e 63 na mais demorada, e as duas são normais.
        /// Ele existe porque <b>a batalha do jogo não tem limite de turnos</b>: um combate em que
        /// nenhum lado consegue matar o outro roda indefinidamente, e sem teto ele imprimiria nível
        /// de item sem fim. Mexer aqui pra baixo faz o ciclo virar enfeite (a 10, ele morderia toda
        /// luta e todo mundo receberia exatamente 10).
        /// </summary>
        public const int TetoDeCiclosPorFase = 60;

        /// <summary>O que a VITÓRIA paga além do acumulado. Fixo: derrota também leva o que andou.</summary>
        public const int BonusDeVitoria = 5;

        private static readonly TabelaDeMaterial Tabela =
            new(CustoDaPrimeira, CustoDaFaixa, CustoDaProxima);

        /// <summary>
        /// O que uma fase concluída derruba de pó. <b>Por FASE, não por inimigo</b> — é a única
        /// diferença de torneira contra a <see cref="Alma.QuedaPorInimigo"/>.
        /// </summary>
        public static IReadOnlyList<Custo> QuedaPorFase(Dificuldade dificuldade)
        {
            IReadOnlyList<Raridade> faixas = Material.FaixasQueCaem(dificuldade);
            return new[]
            {
                new Custo(faixas[0], QuedaBaixa),
                new Custo(faixas[1], QuedaMedia),
                new Custo(faixas[2], QuedaAlta),
            };
        }

        /// <summary>O que custa a <paramref name="estrela"/>-ésima estrela do item (1 a 6).</summary>
        public static IReadOnlyList<Custo> Receita(int estrela) => Material.Receita(Tabela, estrela);

        /// <summary>Quantos pontos de nível uma unidade de pó vale queimada: 1, 5, 25, 125, 625, 3.125.</summary>
        public static int PontosPorPo(Raridade raridade) => Material.ValorQueimado(raridade);

        /// <summary>
        /// Quantos pontos um CICLO de combate paga. É o valor do enum da dificuldade (1·2·3·4), e a
        /// coincidência não é acidente: a XP usa o mesmo número, porque as duas trilhas pagam mais
        /// onde dói pela mesma razão.
        ///
        /// <b>Sem isto, o jeito ótimo de subir item seria repetir a Fácil 1-1</b>, que acaba em duas
        /// rodadas — o teto por fase impede ARRASTAR a luta, não impede REPETIR a luta curta.
        /// </summary>
        public static int PontoPorCiclo(Dificuldade dificuldade) => (int)dificuldade;

        /// <summary>
        /// O que uma fase pagou ao item: os ciclos que ela durou, limitados pelo
        /// <see cref="TetoDeCiclosPorFase"/>, mais o <see cref="BonusDeVitoria"/> se venceu.
        ///
        /// O ciclo é FRACIONÁRIO no motor (é tempo, ver <c>FilaDeTurnos.Ciclos</c>) e o ponto é
        /// inteiro: trunca. Quem lutou meio ciclo não ganhou meio ponto — ganhou nada, e a fase
        /// seguinte recomeça do zero. Guardar a sobra faria a fase de dois ciclos valer o mesmo que
        /// a de três, repetida três vezes.
        /// </summary>
        public static int PontosDaFase(Dificuldade dificuldade, double ciclos, bool venceu)
        {
            int pagos = (int)Math.Min(ciclos, TetoDeCiclosPorFase);
            return pagos * PontoPorCiclo(dificuldade) + (venceu ? BonusDeVitoria : 0);
        }

        /// <summary>
        /// O que custa sair do <paramref name="nivel"/> pro seguinte. <b>Dobra a cada dezena</b>: 10
        /// na faixa 1–9, 20 na 10–19… 320 na 50–59, e 6.290 pontos do nível 1 ao 60.
        ///
        /// Calibrado contra a medição de ago/2026 (uma passada do Fácil rende ~1.740 ciclos): o item
        /// carregado desde a 1-1 chega ao 39 — o teto que o pedágio permite no Fácil — enquanto o
        /// apóstolo termina a mesma passada no 29. <b>É esta diferença que é o desenho</b>: a arma
        /// sobe mais rápido que quem a carrega, e quem a segura dali em diante é o PÓ, não o tempo.
        /// </summary>
        public static int CustoDoNivel(int nivel)
        {
            if (nivel < Arquetipos.NivelMinimo || nivel >= Arquetipos.NivelMaximo)
                throw new ArgumentOutOfRangeException(nameof(nivel));

            return CustoBaseDoNivel << (nivel / 10);
        }

        /// <summary>
        /// O acumulado necessário pra ESTAR no <paramref name="nivel"/> — 0 no nível 1, 690 no 30,
        /// 6.290 no 60. Soma em vez de fórmula fechada porque a escada é por dezena: a soma é a
        /// definição, e uma fórmula fechada seria a mesma conta escrita de um jeito que ninguém
        /// confere.
        /// </summary>
        public static int PontosParaNivel(int nivel)
        {
            int total = 0;
            for (int n = Arquetipos.NivelMinimo; n < nivel; n++) total += CustoDoNivel(n);
            return total;
        }

        /// <summary>
        /// Que nível <paramref name="pontos"/> pagam, preso ao <paramref name="teto"/> que as
        /// estrelas abriram. Ponto que passa do teto não se perde — fica guardado no item e vira
        /// nível assim que o pedágio seguinte for pago (é o mesmo desenho da XP na parede).
        /// </summary>
        public static int NivelPorPontos(int pontos, int teto)
        {
            int nivel = Arquetipos.NivelMinimo;
            while (nivel < teto && pontos >= PontosParaNivel(nivel + 1)) nivel++;
            return nivel;
        }
    }
}
