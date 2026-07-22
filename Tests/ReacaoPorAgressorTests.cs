using ApostlesWar;

namespace Tests
{
    /// <summary>
    /// Testes do orçamento de reação "1x por agressor por turno" (FILA A #11 Fatia B). O estado
    /// mora no TurnoDoPersonagem persistente; o Combate expõe as fachadas TentarReagir (por chave)
    /// e TentarContraAtacar (chave compartilhada). Puro/headless — sem Console.
    /// </summary>
    public class ReacaoPorAgressorTests
    {
        private static Combate Novo() => new Jogador(new Personagem(1, Faccao.Humanos, "T", "🧪", 1000, 100, 0));
        private static readonly object ChaveA = new();
        private static readonly object ChaveB = new();

        [Fact]
        public void MesmaChaveMesmoAgressor_SoUmaVezPorTurno()
        {
            var defensor = Novo(); var agressor = Novo();
            Assert.True(defensor.TentarReagir(ChaveA, agressor, 1.0));    // 1º hit dispara
            Assert.False(defensor.TentarReagir(ChaveA, agressor, 1.0));   // 2º hit no mesmo turno: não
        }

        [Fact]
        public void AgressoresDiferentes_CadaUmDispara()
        {
            var defensor = Novo(); var a1 = Novo(); var a2 = Novo();
            Assert.True(defensor.TentarReagir(ChaveA, a1, 1.0));
            Assert.True(defensor.TentarReagir(ChaveA, a2, 1.0));          // agressor diferente: dispara
        }

        [Fact]
        public void ChavesDiferentes_OrcamentosSeparados()
        {
            var defensor = Novo(); var agressor = Novo();
            Assert.True(defensor.TentarReagir(ChaveA, agressor, 1.0));
            Assert.True(defensor.TentarReagir(ChaveB, agressor, 1.0));    // outra reação (chave diferente): dispara
        }

        [Fact]
        public void Finalizar_ReabreOOrcamento()
        {
            var defensor = Novo(); var agressor = Novo();
            Assert.True(defensor.TentarReagir(ChaveA, agressor, 1.0));
            Assert.False(defensor.TentarReagir(ChaveA, agressor, 1.0));
            defensor.Turno.Finalizar();                                   // fim do turno do defensor
            Assert.True(defensor.TentarReagir(ChaveA, agressor, 1.0));    // reabre
        }

        [Fact]
        public void ContraAtaque_FontesCompartilhamOLimite()
        {
            var defensor = Novo(); var agressor = Novo();
            // Duas "fontes" de contra-ataque (ex: buff + passiva) = duas chamadas ao TentarContraAtacar:
            Assert.True(defensor.TentarContraAtacar(agressor, 1.0));      // 1ª fonte contra-ataca
            Assert.False(defensor.TentarContraAtacar(agressor, 1.0));     // 2ª fonte: já contra-atacou (mesma chave)
        }
    }
}
