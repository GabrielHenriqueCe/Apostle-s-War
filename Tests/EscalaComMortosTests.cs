using ApostlesWar;
using ApostlesWar.Skills.Buffs;
using ApostlesWar.Skills.Passivas;

namespace Tests
{
    /// <summary>
    /// Testes da capacidade EscalaComMortos (FILA A #12 — passiva-conta-mortos, cliente Zumbi).
    /// A passiva lê o tabuleiro (ContextoCombate.Aliados/Inimigos) e aplica um buff proporcional aos
    /// mortos do escopo. Puro/headless: monta o contexto à mão e assere o BuffAtaque no portador.
    /// </summary>
    public class EscalaComMortosTests
    {
        private static Combate Novo() => new Jogador(new Personagem(1, Faccao.Humanos, "T", "🧪", 1000, 200, 0));
        private static void Matar(Combate c) => c.ReceberDano(999999, NaturezasDano.DanoIndireto);

        private static EscalaComMortos Passiva(EscopoMortos escopo, double porMorto = 0.10)
            => new("Teste", "🧪", "", escopo, porMorto, v => new BuffAtaque(duracao: 2, percentual: v));

        private static double BuffAtk(Combate c)
            => c.StatusAtivos.OfType<BuffAtaque>().FirstOrDefault()?.Valor ?? 0.0;

        [Fact]
        public void AmbosOsTimes_ContaMortosDosDois()
        {
            var portador = Novo();
            var aliado = Novo(); var inimigo1 = Novo(); var inimigo2 = Novo();
            Matar(aliado); Matar(inimigo1);   // 2 mortos (1 aliado + 1 inimigo); inimigo2 vivo
            var ctx = new ContextoCombate(portador, new() { portador, aliado }, new() { inimigo1, inimigo2 });

            Passiva(EscopoMortos.AmbosOsTimes).AoInicioTurno(ctx);
            Assert.Equal(0.20, BuffAtk(portador), 3);   // 2 mortos × 10%
        }

        [Fact]
        public void ProprioTime_SoContaAliadosMortos()
        {
            var portador = Novo();
            var aliado = Novo(); var inimigo = Novo();
            Matar(aliado); Matar(inimigo);
            var ctx = new ContextoCombate(portador, new() { portador, aliado }, new() { inimigo });

            Passiva(EscopoMortos.ProprioTime).AoInicioTurno(ctx);
            Assert.Equal(0.10, BuffAtk(portador), 3);   // só o aliado morto (o inimigo morto não conta)
        }

        [Fact]
        public void TimeInimigo_SoContaInimigosMortos()
        {
            var portador = Novo();
            var aliado = Novo(); var inimigo = Novo();
            Matar(aliado); Matar(inimigo);
            var ctx = new ContextoCombate(portador, new() { portador, aliado }, new() { inimigo });

            Passiva(EscopoMortos.TimeInimigo).AoInicioTurno(ctx);
            Assert.Equal(0.10, BuffAtk(portador), 3);   // só o inimigo morto
        }

        [Fact]
        public void SemMortos_NaoAplicaBuff()
        {
            var portador = Novo();
            var ctx = new ContextoCombate(portador, new() { portador }, new() { Novo() });

            Passiva(EscopoMortos.AmbosOsTimes).AoInicioTurno(ctx);
            Assert.Empty(portador.StatusAtivos.OfType<BuffAtaque>());
        }
    }
}
