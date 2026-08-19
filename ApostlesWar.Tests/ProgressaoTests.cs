using ApostlesWar.Domain;

namespace Tests
{
    /// <summary>
    /// A matemática da progressão (<see cref="Progressao"/>): a curva de XP, o pote da fase, a reta do
    /// inimigo — e os dois CLAMPS do <see cref="Arquetipos"/>, um que saiu e um que ficou.
    /// Tudo puro, roda headless.
    /// </summary>
    public class ProgressaoTests
    {
        [Theory]
        [InlineData(1, 0)]
        [InlineData(30, 43_500)]
        [InlineData(60, 177_000)]
        public void XpParaNivel_AsPontasDaCurva(int nivel, int esperado)
            => Assert.Equal(esperado, Progressao.XpParaNivel(nivel));

        [Fact]
        public void NivelPorXp_EOInversoDoXpParaNivel()
        {
            for (int nivel = 1; nivel <= Arquetipos.NivelMaximo; nivel++)
            {
                Assert.Equal(nivel, Progressao.NivelPorXp(Progressao.XpParaNivel(nivel)));
                if (nivel > 1)
                    Assert.Equal(nivel - 1, Progressao.NivelPorXp(Progressao.XpParaNivel(nivel) - 1));
            }
        }

        [Fact]
        public void NivelPorXp_NaoPassaDoTeto()
            => Assert.Equal(Arquetipos.NivelMaximo, Progressao.NivelPorXp(int.MaxValue / 2));

        /// <summary>
        /// A CADÊNCIA DO FÁCIL, e ela é o critério de calibragem inteiro: jogando cada fase UMA vez, o
        /// jogador entra em cada capítulo nos níveis 1·5·9·12·15·18·21·24 e termina no 27 — três
        /// níveis abaixo do teto, que só a repetição paga.
        ///
        /// Se alguém mexer no 72 (o `q = 18` × 4 em campo), é aqui que aparece.
        /// </summary>
        [Fact]
        public void PassadaSuaveDoFacil_ParaNo27()
        {
            var entradas = new List<int>();
            int acumulado = 0;

            for (int capitulo = 1; capitulo <= 8; capitulo++)
            {
                entradas.Add(Progressao.NivelPorXp(acumulado));
                for (int fase = 1; fase <= 7; fase++)
                    acumulado += Progressao.PoteDaFase(capitulo, fase, Dificuldade.Facil) / 4;   // time cheio
            }

            Assert.Equal(new[] { 1, 5, 9, 12, 15, 18, 21, 24 }, entradas);
            Assert.Equal(27, Progressao.NivelPorXp(acumulado));
            Assert.True(acumulado < Progressao.XpParaNivel(30), "uma passada suave NÃO pode chegar ao teto");
        }

        /// <summary>O valor do enum É o multiplicador: o Pesadelo paga 4× o Fácil na mesma fase.</summary>
        [Fact]
        public void PoteDaFase_EscalaPeloValorDaDificuldade()
        {
            int facil = Progressao.PoteDaFase(3, 5, Dificuldade.Facil);
            Assert.Equal(facil * 2, Progressao.PoteDaFase(3, 5, Dificuldade.Normal));
            Assert.Equal(facil * 4, Progressao.PoteDaFase(3, 5, Dificuldade.Pesadelo));
        }

        /// <summary>
        /// A 8-7 vale ~8× a 1-1 (63 ÷ 8, o índice `k` das duas) — é isso que dá à repetição um lugar
        /// certo pra acontecer, e o lugar é o mais difícil.
        /// </summary>
        [Fact]
        public void PoteDaFase_A87ValeQuaseOitoVezesA11()
        {
            Assert.Equal(8, Progressao.IndiceDaFase(1, 1));
            Assert.Equal(63, Progressao.IndiceDaFase(8, 7));
            Assert.Equal(63 / 8.0,
                (double)Progressao.PoteDaFase(8, 7, Dificuldade.Facil) / Progressao.PoteDaFase(1, 1, Dificuldade.Facil), 3);
        }

        /// <summary>
        /// AS OITO ÂNCORAS, cravadas nas pontas de cada dificuldade. Elas vêm da paridade de poder
        /// contra o arsenal projetado (ferramentas/calibrar-inimigo.js) — mudar uma sem refazer a
        /// calibragem é mover o jogo inteiro.
        /// </summary>
        [Theory]
        [InlineData(Dificuldade.Facil, 5, 64)]
        [InlineData(Dificuldade.Normal, 84, 138)]
        [InlineData(Dificuldade.Dificil, 156, 226)]
        [InlineData(Dificuldade.Pesadelo, 256, 428)]
        public void NivelDoInimigo_NasAncoras(Dificuldade dificuldade, int naPrimeira, int naUltima)
        {
            Assert.Equal(naPrimeira, Progressao.NivelDoInimigo(dificuldade, 1, 1));
            Assert.Equal(naUltima, Progressao.NivelDoInimigo(dificuldade, 8, 7));
        }

        [Fact]
        public void NivelDoInimigo_NuncaCaiAoLongoDaCampanha()
        {
            foreach (Dificuldade d in Enum.GetValues<Dificuldade>())
            {
                int anterior = 0;
                for (int capitulo = 1; capitulo <= 8; capitulo++)
                    for (int fase = 1; fase <= 7; fase++)
                    {
                        int nivel = Progressao.NivelDoInimigo(d, capitulo, fase);
                        Assert.True(nivel >= anterior, $"{d} {capitulo}-{fase} caiu de {anterior} pra {nivel}");
                        anterior = nivel;
                    }
            }
        }

        /// <summary>
        /// O CLAMP QUE SAIU. HP/ATK/DEF do inimigo têm de continuar subindo acima do nível 60 — com o
        /// teto de volta, o inimigo nv 428 do Pesadelo seria tratado como 60 e o jogo ficaria trivial,
        /// sem quebrar teste nenhum de dano.
        /// </summary>
        [Fact]
        public void FatorDoNivel_NaoSaturaNoTetoDoJogador()
        {
            Assert.True(Arquetipos.FatorDoNivel(428) > Arquetipos.FatorDoNivel(Arquetipos.NivelMaximo));
            Assert.Equal(30.0, Arquetipos.FatorDoNivel(Arquetipos.NivelMaximo), 3);
            Assert.Equal(1.0, Arquetipos.FatorDoNivel(0), 3);   // o PISO fica
        }

        /// <summary>
        /// O CLAMP QUE FICOU. Velocidade é quantos TURNOS se joga: sem teto, o inimigo do Pesadelo
        /// agiria quatro vezes por turno do jogador. Este teste é a única coisa entre isso e o jogo.
        /// </summary>
        [Fact]
        public void Velocidade_ClampaNoTeto()
        {
            foreach (TipoDeApostolo tipo in Enum.GetValues<TipoDeApostolo>())
                Assert.Equal(Arquetipos.Velocidade(tipo, Arquetipos.NivelMaximo),
                             Arquetipos.Velocidade(tipo, 428));
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(9, 0)]
        [InlineData(10, 1)]
        [InlineData(60, 6)]
        public void Estrelas_UmaACadaDezNiveis(int nivel, int esperado)
            => Assert.Equal(esperado, Progressao.Estrelas(nivel));

        /// <summary>
        /// A MINA: o inimigo da campanha sai do mesmo service que o roster do jogador. Se o nível for
        /// aplicado na instância compartilhada, o time do jogador sobe junto — sem quebrar build e sem
        /// ninguém ver. Por isso o inimigo é CÓPIA.
        /// </summary>
        [Fact]
        public void ComNivel_NaoMexeNoOriginal()
        {
            var original = new Personagem(1, Faccao.Reino, "Guarda", "💂", TipoDeApostolo.Guardiao);
            int hpAntes = original.HP;

            Personagem copia = original.ComNivel(200);

            Assert.Equal(Arquetipos.NivelMinimo, original.Nivel);
            Assert.Equal(hpAntes, original.HP);
            Assert.Equal(200, copia.Nivel);
            Assert.True(copia.HP > hpAntes);
        }
    }
}
