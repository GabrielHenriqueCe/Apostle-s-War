using ApostlesWar.Domain;

namespace Tests
{
    /// <summary>
    /// Testes da estrutura de Batalha/Equipe (FILA A #11 Fatia C). O "um só caminho" de perspectiva:
    /// PerspectivaDe(portador) deriva aliados/inimigos da ESTRUTURA (qual equipe), não do tipo. Puro/headless.
    /// </summary>
    public class BatalhaTests
    {
        private static Combate Novo() => new Jogador(new Personagem(1, Faccao.Humanos, "T", "🧪", 1000, 100, 0));

        private static Batalha Montar(out Combate a1, out Combate a2, out Combate b1)
        {
            a1 = Novo(); a2 = Novo(); b1 = Novo();
            return new Batalha(new Equipe(new List<Combate> { a1, a2 }), new Equipe(new List<Combate> { b1 }));
        }

        [Fact]
        public void PerspectivaDe_DeCadaLado_EhSimetrica()
        {
            var batalha = Montar(out var a1, out var a2, out var b1);

            var pa = batalha.PerspectivaDe(a1);
            Assert.Contains(a2, pa.Aliados);        // aliado de A é do time A
            Assert.Contains(b1, pa.Inimigos);       // inimigo de A é do time B

            var pb = batalha.PerspectivaDe(b1);
            Assert.Contains(b1, pb.Aliados);        // B se vê no próprio time
            Assert.Contains(a1, pb.Inimigos);       // inimigo de B é do time A

            // Simetria: aliados de A == inimigos de B, e vice-versa (o "flip" automático).
            Assert.Equal(pa.Aliados, pb.Inimigos);
            Assert.Equal(pa.Inimigos, pb.Aliados);
        }

        [Fact]
        public void EquipeDe_e_OponenteDe()
        {
            var batalha = Montar(out var a1, out _, out var b1);
            Assert.Same(batalha.Equipe1, batalha.EquipeDe(a1));
            Assert.Same(batalha.Equipe2, batalha.EquipeDe(b1));
            Assert.Same(batalha.Equipe2, batalha.OponenteDe(batalha.Equipe1));
            Assert.Same(batalha.Equipe1, batalha.OponenteDe(batalha.Equipe2));
        }

        [Fact]
        public void Combatentes_OrdemEquipe1DepoisEquipe2()
        {
            var batalha = Montar(out var a1, out var a2, out var b1);
            Assert.Equal(new[] { a1, a2, b1 }, batalha.Combatentes);
        }

        [Fact]
        public void TemVivos_FalsoQuandoTodosDoTimeMorrem()
        {
            var batalha = Montar(out var a1, out var a2, out _);
            Assert.True(batalha.Equipe1.TemVivos());

            a1.ReceberDano(999999, NaturezasDano.DanoIndireto);
            a2.ReceberDano(999999, NaturezasDano.DanoIndireto);

            Assert.False(batalha.Equipe1.TemVivos());   // time A todo morto
            Assert.True(batalha.Equipe2.TemVivos());
        }
    }
}
