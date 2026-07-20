using ApostlesWar.View;

namespace Tests
{
    /// <summary>
    /// Testes do helper de navegação 1D (Navegacao.MoverCursor) — o único pedaço PURO da porta de
    /// entrada (o EntradaConsole em si depende de Console.ReadKey e não roda headless). Cobre a
    /// paridade com o antigo ConsoleUtils.SelecionarComCursor, incluindo o atalho numérico.
    /// </summary>
    public class NavegacaoTests
    {
        [Fact]
        public void Cima_Decrementa_ComClampNoMin()
        {
            Assert.Equal(1, Navegacao.MoverCursor(2, 1, 5, new Comando(TipoComando.Cima)));
            Assert.Equal(1, Navegacao.MoverCursor(1, 1, 5, new Comando(TipoComando.Cima)));   // não passa do min
        }

        [Fact]
        public void Baixo_Incrementa_ComClampNoMax()
        {
            Assert.Equal(3, Navegacao.MoverCursor(2, 1, 5, new Comando(TipoComando.Baixo)));
            Assert.Equal(5, Navegacao.MoverCursor(5, 1, 5, new Comando(TipoComando.Baixo)));   // não passa do max
        }

        [Fact]
        public void Selecionar_PulaDiretoSeNoIntervalo_SenaoFica()
        {
            Assert.Equal(3, Navegacao.MoverCursor(1, 1, 5, new Comando(TipoComando.Selecionar, 3)));   // atalho numérico
            Assert.Equal(1, Navegacao.MoverCursor(1, 1, 5, new Comando(TipoComando.Selecionar, 9)));   // fora do range → fica
        }

        [Fact]
        public void OutrosComandos_NaoMovem()
        {
            Assert.Equal(2, Navegacao.MoverCursor(2, 1, 5, new Comando(TipoComando.Esquerda)));   // menu 1D vertical ignora horizontal
            Assert.Equal(2, Navegacao.MoverCursor(2, 1, 5, new Comando(TipoComando.Confirmar)));
            Assert.Equal(2, Navegacao.MoverCursor(2, 1, 5, new Comando(TipoComando.Nenhum)));
        }
    }
}
