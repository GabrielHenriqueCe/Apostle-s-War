namespace ApostlesWar.Domain
{
    /// <summary>
    /// A matemática da progressão: quanto custa um nível, quanta XP uma fase põe na mesa e em que
    /// nível o inimigo entra. Função pura e sem estado, irmã do <see cref="Arquetipos"/> — quem
    /// guarda XP de alguém é o `ProgressaoService`.
    ///
    /// Os números são os do docs/GDD-progressao.md §A CURVA DE XP e §Os inimigos não têm itens, e o
    /// `ferramentas/calibrar-inimigo.js` reproduz as oito âncoras.
    /// </summary>
    public static class Progressao
    {
        /// <summary>
        /// O custo ACUMULADO de estar no nível: `100 × N` por degrau vira `50 × L × (L−1)`.
        /// Nível 30 = 43.500 · nível 60 = 177.000, que é o jogo inteiro.
        /// </summary>
        public static int XpParaNivel(int nivel) => 50 * nivel * (nivel - 1);

        /// <summary>
        /// O nível que esta XP paga, preso ao teto de <see cref="Arquetipos.NivelMaximo"/>.
        ///
        /// Esta sobrecarga é a do INIMIGO e a de quem não tem estrela pra consultar: o
        /// <see cref="NivelDoInimigo"/> chega a 428 e travá-lo no 9 esvaziaria a campanha.
        /// </summary>
        public static int NivelPorXp(int xp) => NivelPorXp(xp, Arquetipos.NivelMaximo);

        /// <summary>
        /// O nível que esta XP paga, preso ao <paramref name="teto"/> que as estrelas compradas
        /// permitem (<see cref="TetoPorEstrelas"/>).
        ///
        /// A XP que passa do teto NÃO se perde: quem guarda é o save, que grava a XP crua. Parar aqui
        /// é só deixar de APLICAR — comprar a estrela reaplica e o nível salta de uma vez.
        ///
        /// LAÇO INTEIRO, e não a inversa da quadrática: com `q = 19` a passada do Fácil erra o nível
        /// 28 por 28 pontos de XP, e nessa margem um `sqrt` em ponto flutuante decide o teto pelo
        /// arredondamento. Sessenta iterações no pior caso não são gargalo de nada.
        /// </summary>
        public static int NivelPorXp(int xp, int teto)
        {
            int nivel = Arquetipos.NivelMinimo;
            while (nivel < teto && XpParaNivel(nivel + 1) <= xp) nivel++;
            return nivel;
        }

        /// <summary>
        /// Até onde as estrelas COMPRADAS deixam subir: sem nenhuma trava no 9, e cada estrela abre a
        /// dezena seguinte. Seis estrelas, seis paredes — 9, 19, 29, 39, 49, 59 —, e a sexta é a que
        /// abre o 60 (docs/GDD-progressao.md §A ESTRELA).
        ///
        /// É ESTE o único teto de nível do jogo. Não existe regra escrita dizendo "o Fácil para no
        /// 30": a receita da 4ª estrela cobra Épico, e Épico não cai no Fácil (<see cref="Alma"/>).
        /// </summary>
        public static int TetoPorEstrelas(int estrelas)
            => Math.Min(10 * Math.Max(estrelas, 0) + 9, Arquetipos.NivelMaximo);

        /// <summary>Onde <paramref name="xp"/> cai DENTRO da faixa deste nível, em 0..100.</summary>
        public static int PctNaFaixa(int nivel, int xp)
        {
            int piso = XpParaNivel(nivel);
            int total = XpParaNivel(nivel + 1) - piso;
            if (total <= 0) return 100;
            return Math.Clamp((int)(100L * (xp - piso) / total), 0, 100);
        }

        /// <summary>
        /// A subida fatiada por NÍVEL, pra a barra da tela animar: um trecho por nível atravessado,
        /// cada um dizendo de onde até onde encher (0..100). Três níveis = encher, zerar, encher,
        /// zerar, encher.
        ///
        /// Mora aqui e não na tela porque saber ONDE um nível vira é a curva de XP — e uma segunda
        /// cópia dela do outro lado da ponte divergiria como duas cópias de um número.
        /// </summary>
        public static List<TrechoDeXp> Trechos(int nivelAntes, int xpAntes, int nivelDepois, int xpDepois)
        {
            var trechos = new List<TrechoDeXp>();
            for (int nivel = nivelAntes; nivel <= nivelDepois; nivel++)
                trechos.Add(new TrechoDeXp(nivel, PctNaFaixa(nivel, xpAntes), PctNaFaixa(nivel, xpDepois)));
            return trechos;
        }

        /// <summary>
        /// O índice da fase na campanha inteira: `k = 7 × capítulo + fase`, de 8 (a 1-1) a 63 (a 8-7).
        /// É ele que faz a XP e o nível do inimigo crescerem DENTRO do capítulo e ENTRE capítulos com
        /// uma conta só — a 8-7 vale 8× uma 1-1.
        /// </summary>
        public static int IndiceDaFase(int capitulo, int fase) => 7 * capitulo + fase;

        /// <summary>
        /// A XP que uma fase põe na mesa, para o time INTEIRO: `72 × k × dificuldade`.
        ///
        /// O 72 é o 18 de cada apóstolo vezes os quatro em campo — quem divide pelos que estão em
        /// campo é quem credita, e é isso que faz solar ser quatro vezes mais rápido pra um só.
        /// O multiplicador da dificuldade não é uma tabela: é o VALOR do enum.
        /// </summary>
        public static int PoteDaFase(int capitulo, int fase, Dificuldade dificuldade)
            => 72 * IndiceDaFase(capitulo, fase) * (int)dificuldade;

        /// <summary>
        /// Quantas estrelas um nível IMPLICA. Quem manda na ficha é a estrela COMPRADA, que vive no
        /// save — esta função deixou de ser a definição e virou a identidade que CONFERE.
        ///
        /// Ela vale porque a compra é a única forma de passar da parede: pra estar no 10 você comprou
        /// a 1ª, pra estar no 20 comprou a 2ª. O <see cref="Arquetipos.Velocidade"/> conta o degrau
        /// com a mesma expressão, e é por isso que ele não precisou trocar de fonte.
        /// </summary>
        public static int Estrelas(int nivel) => nivel / 10;

        // AS OITO ÂNCORAS: o nível do inimigo na 1-1 e na 8-7 de cada dificuldade. Não são gosto —
        // cada uma é o nível em que o time inimigo (que não tem item nenhum) empata em poder com o
        // time do jogador com o arsenal que o GDD-itens projeta pra aquele ponto.
        private static readonly Dictionary<Dificuldade, (int Inicio, int Fim)> ancoras = new()
        {
            { Dificuldade.Facil,    (5,   64) },
            { Dificuldade.Normal,   (84, 138) },
            { Dificuldade.Dificil,  (156, 226) },
            { Dificuldade.Pesadelo, (256, 428) },
        };

        /// <summary>
        /// O nível do inimigo: uma RETA entre as duas âncoras da dificuldade, ao longo dos 56 pontos
        /// da campanha. A curvatura que a reta perde é artefato do jogador travar no teto do material
        /// no meio da passada, não desenho — o erro máximo é 12–17%, em fases isoladas do miolo.
        ///
        /// Ele fica MUITO acima do jogador (7,1× no fim do Pesadelo) porque é o item do jogador que
        /// ele está compensando; a margem de quem joga é o kit, não um desconto no inimigo.
        /// </summary>
        public static int NivelDoInimigo(Dificuldade dificuldade, int capitulo, int fase)
        {
            var (inicio, fim) = ancoras[dificuldade];
            int k = IndiceDaFase(capitulo, fase);
            return (int)Math.Round(inicio + (fim - inicio) * (k - 8) / 55.0);
        }
    }
}
