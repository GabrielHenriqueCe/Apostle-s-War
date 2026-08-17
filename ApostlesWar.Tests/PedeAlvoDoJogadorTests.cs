using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills;

namespace Tests
{
    /// <summary>
    /// Testes da regra ESTÁTICA <see cref="HabilidadeAtiva.PedeAlvoDoJogador"/> — a "essa habilidade
    /// abre o passo de escolha de alvo?". Mora no domínio, e o front não a recalcula: quem pergunta
    /// é a tela E o CombateService.ResolverAlvoInicial, pela mesma fonte. Prova que ela bate com a
    /// regra: inimigos sempre; aliados só finito; Self/hit-all-aliado nunca.
    /// </summary>
    public class PedeAlvoDoJogadorTests
    {
        private static HabilidadeAtiva Hab(TipoLista lista, int alvos)
            => new("Teste", "🧪", 0, "", alvos, TipoAlvo.Explicito, lista, EstadoAlvo.Vivos, new List<Acao>());

        [Fact]
        public void Inimigos_SemprePedeAlvo()
        {
            // Até o AoE inimigo pede alvo (uma semente); o hit-all é resolvido depois.
            Assert.True(Hab(TipoLista.Inimigos, 1).PedeAlvoDoJogador);
            Assert.True(Hab(TipoLista.Inimigos, int.MaxValue).PedeAlvoDoJogador);
        }

        [Fact]
        public void Aliados_PedeAlvoSoQuandoFinito()
        {
            Assert.True(Hab(TipoLista.Aliados, 1).PedeAlvoDoJogador);         // buff em 1 aliado: escolhe quem
            Assert.False(Hab(TipoLista.Aliados, int.MaxValue).PedeAlvoDoJogador);   // buff em TODOS: sem escolha
        }

        [Fact]
        public void Self_NuncaPedeAlvo()
        {
            Assert.False(Hab(TipoLista.Self, 1).PedeAlvoDoJogador);
            Assert.False(Hab(TipoLista.Self, int.MaxValue).PedeAlvoDoJogador);
        }
    }
}
