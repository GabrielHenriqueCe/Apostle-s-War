using ApostlesWar;
using ApostlesWar.Champs.Reino;
using ApostlesWar.Skills.Debuffs;

namespace Tests
{
    /// <summary>
    /// Testes do IModificaDanoCausado (modificador de dano do ATACANTE — FILA A #10). A passiva
    /// (Piromancer) é consultada pela Ação Dano, que dobra o multiplicador antes do (int) do Atacar.
    /// Crítico zerado com ReducaoTaxaCrit (0.15 base → 0) pra o DanoBruto ser determinístico.
    /// </summary>
    public class ModificaDanoCausadoTests
    {
        // Mago-like: carrega a passiva Piromancer. ATK 200, sem crítico.
        private static Combate MagoComPiromancer()
        {
            var c = new Jogador(new Personagem(3, Faccao.Reino, "Mago", "🧙", 1000, 200, 0, new Piromancer()));
            new ReducaoTaxaCrit(valor: 0.25).Aplicar(c);   // TaxaCrit 0.15 → 0: nunca critica
            return c;
        }

        private static Combate Alvo() => new Jogador(new Personagem(1, Faccao.Humanos, "Alvo", "🎯", 100000, 0, 0));

        [Fact]
        public void Piromancer_Multiplicador_25PorCentoSoContraQueima()
        {
            var atacante = MagoComPiromancer();
            var comQueima = Alvo(); comQueima.StatusAtivos.Add(new Queima(2));
            var semQueima = Alvo();

            var piro = new Piromancer();
            Assert.Equal(1.25, piro.MultiplicadorDeDano(atacante, comQueima), 3);
            Assert.Equal(1.00, piro.MultiplicadorDeDano(atacante, semQueima), 3);
        }

        [Fact]
        public void Dano_DobraOModificadorDoAtacante_InclusiveNoA1()
        {
            var atacante = MagoComPiromancer();                 // ATK 200, sem crit
            var comQueima = Alvo(); comQueima.StatusAtivos.Add(new Queima(2));
            var semQueima = Alvo();

            // A1 = Dano(1.0): contra alvo queimando ganha os 25% (a mudança do #10); sem Queima, não.
            var evQueima = new List<EventoCombate>();
            new Dano(1.0).Executar(atacante, comQueima, evQueima);
            var evSem = new List<EventoCombate>();
            new Dano(1.0).Executar(atacante, semQueima, evSem);

            Assert.Equal(250, ((EventoDano)evQueima[0]).DanoBruto);   // (int)(200 × 1.0 × 1.25)
            Assert.Equal(200, ((EventoDano)evSem[0]).DanoBruto);      // (int)(200 × 1.0)
        }
    }
}
