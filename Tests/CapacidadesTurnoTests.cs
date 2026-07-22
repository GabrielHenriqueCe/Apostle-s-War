using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace Tests
{
    /// <summary>
    /// Testes da Capacidade D (comportamento de turno via IPulaTurno / IForcaAcao /
    /// IParalisaAcao). O CombateService deixou de decidir por tipo concreto — cada status
    /// carrega a própria capacidade. A fiação no fluxo do turno (pular/forçar/paralisar) não
    /// roda headless (precisa de Console); aqui provamos que os status EXPÕEM a capacidade
    /// certa, que é o contrato que o fluxo passa a consultar.
    /// </summary>
    public class CapacidadesTurnoTests
    {
        private static Combate Novo()
            => new Jogador(new Personagem(1, Faccao.Humanos, "Teste", "🧪", 1000, 200, 0));

        [Fact]
        public void Preso_CarregaIPulaTurno()
        {
            Assert.IsAssignableFrom<IPulaTurno>(new Preso());
        }

        [Fact]
        public void Medo_Paralisa_RespeitaAChance()
        {
            Assert.True(new Medo(chance: 1.0).Paralisa());    // sempre paralisa
            Assert.False(new Medo(chance: 0.0).Paralisa());   // nunca paralisa
        }

        [Fact]
        public void Irritar_AlvoForcado_EhQuemAplicou()
        {
            var aplicador = Novo();
            IForcaAcao irritar = new Irritar(aplicador);
            Assert.Same(aplicador, irritar.AlvoForcado());
        }
    }
}
