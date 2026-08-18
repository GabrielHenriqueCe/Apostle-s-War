using ApostlesWar.Domain;

namespace Tests
{
    /// <summary>
    /// A VELOCIDADE ANDA NA ESTRELA (GDD-combate §1): <b>+2 por estrela</b> — 2·4·6·8·10·12 do
    /// nv 10 ao 60.
    ///
    /// É o único stat em degrau; HP/ATK/DEF sobem por curva contínua. Estes testes existem porque o
    /// topo deixou de ser declarado e passou a ser CONSEQUÊNCIA da regra: se ela mudar, é aqui que
    /// aparece, e não numa tabela que alguém esqueceu de acompanhar.
    /// </summary>
    public class VelocidadePorEstrelaTests
    {
        private static int Vel(int nivel) => Arquetipos.Velocidade(TipoDeApostolo.Guardiao, nivel);

        private const int Base = 85;   // Guardião no nv 1

        [Theory]
        [InlineData(1, 0)]     // recém-descoberto: nenhuma estrela
        [InlineData(9, 0)]     // véspera da primeira
        [InlineData(10, 2)]
        [InlineData(19, 2)]    // a estrela é que paga, não o nível
        [InlineData(20, 4)]
        [InlineData(50, 10)]
        [InlineData(59, 10)]
        [InlineData(60, 12)]
        public void OGanhoSegueAEstrela(int nivel, int ganho)
        {
            Assert.Equal(Base + ganho, Vel(nivel));
        }

        /// <summary>
        /// A CADÊNCIA É ÚNICA: toda estrela vale o mesmo, inclusive a do teto. É o que dispensa
        /// caso especial no cálculo — e é a diferença entre esta regra e a primeira versão dela,
        /// que dava +5 só na sexta.
        /// </summary>
        [Fact]
        public void TodaEstrelaValeOMesmo()
        {
            Assert.Equal(Arquetipos.GanhoPorEstrela, Vel(10) - Vel(9));
            Assert.Equal(Arquetipos.GanhoPorEstrela, Vel(50) - Vel(49));
            Assert.Equal(Arquetipos.GanhoPorEstrela, Vel(60) - Vel(59));
        }

        /// <summary>
        /// O ganho é o MESMO pros quatro tipos, então a ordem entre eles é a da ficha do nv 1 e não
        /// muda nunca. É isso que faz o bônus do 60 não desequilibrar ninguém.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(35)]
        [InlineData(60)]
        public void TodosOsTiposSobemIgual(int nivel)
        {
            int guardiao = Arquetipos.Velocidade(TipoDeApostolo.Guardiao, nivel);
            int combatente = Arquetipos.Velocidade(TipoDeApostolo.Combatente, nivel);
            int suporte = Arquetipos.Velocidade(TipoDeApostolo.Suporte, nivel);
            int atirador = Arquetipos.Velocidade(TipoDeApostolo.Atirador, nivel);

            Assert.Equal(10, combatente - guardiao);
            Assert.Equal(10, suporte - combatente);
            Assert.Equal(5, atirador - suporte);
        }

        /// <summary>
        /// O SUPORTE SOBE COMO OS OUTROS. Ele ficou parado em 105 por uma tabela com as duas pontas
        /// escritas à mão que discordavam entre si — e nada quebrava, porque não havia regra pra
        /// contradizer.
        /// </summary>
        [Fact]
        public void OSuporteNaoFicaParado()
        {
            Assert.Equal(105, Arquetipos.Velocidade(TipoDeApostolo.Suporte, 1));
            Assert.Equal(117, Arquetipos.Velocidade(TipoDeApostolo.Suporte, 60));
        }

        [Fact]
        public void ForaDaFaixa_ProNivelMaisProximo()
        {
            Assert.Equal(Vel(1), Vel(0));
            Assert.Equal(Vel(60), Vel(61));
        }
    }
}
