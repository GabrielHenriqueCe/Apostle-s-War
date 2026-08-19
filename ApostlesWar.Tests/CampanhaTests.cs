using ApostlesWar.Application.Services;
using ApostlesWar.Domain;

namespace Tests
{
    /// <summary>
    /// A tabela de composição das fases (<see cref="Campanha"/>) e a premissa que ela assume: cada
    /// facção tem exatamente um apóstolo de cada <see cref="TipoDeApostolo"/>. É tudo dado estático,
    /// então roda headless — nada aqui pede combate.
    /// </summary>
    public class CampanhaTests
    {
        private static IEnumerable<Fase> TodasAsFases()
            => Enum.GetValues<Fases>().Select(f => Campanha.ObterFase((int)f));

        /// <summary>
        /// A tabela por PAPEL só tem resposta porque o roster é um-de-cada. Um apóstolo torto não
        /// quebraria o build: viraria exceção do `First` no meio de uma fase da campanha.
        /// </summary>
        [Fact]
        public void TodaFaccao_TemUmDeCadaTipo()
        {
            var servico = new PersonagemService();

            foreach (Faccao faccao in Enum.GetValues<Faccao>())
                foreach (TipoDeApostolo tipo in Enum.GetValues<TipoDeApostolo>())
                {
                    Personagem p = servico.ObterPorTipo(faccao, tipo);
                    Assert.Equal(faccao, p.Faccao);
                    Assert.Equal(tipo, p.Tipo);
                }
        }

        [Theory]
        [InlineData(Fases.Fase1, 2)]
        [InlineData(Fases.Fase2, 3)]
        [InlineData(Fases.Fase3, 5)]
        [InlineData(Fases.Fase4, 7)]
        [InlineData(Fases.Fase5, 8)]
        [InlineData(Fases.Fase6, 8)]
        [InlineData(Fases.Fase7, 8)]
        public void ContagemDeInimigos_PorFase(Fases fase, int esperado)
        {
            Fase f = Campanha.ObterFase((int)fase);
            Assert.Equal(esperado, f.Rodada1.Count + f.Rodada2.Count);
        }

        /// <summary>
        /// Invariante 1 do GDD §A COMPOSIÇÃO DAS FASES: a formação estreia como rodada 2 e volta como
        /// aquecimento da fase seguinte. Sem isto as sete fases viram lutas soltas.
        /// </summary>
        [Fact]
        public void Rodada1_RepeteARodada2_DaFaseAnterior()
        {
            for (int fase = 2; fase <= 7; fase++)
                Assert.Equal(Campanha.ObterFase(fase - 1).Rodada2, Campanha.ObterFase(fase).Rodada1);
        }

        /// <summary>
        /// Invariante 2: G/C nas casas 1-2, A/S nas 3-4 — e a ordem da lista É a casa. Ninguém entra
        /// fora de posição; a variedade vem da composição.
        /// </summary>
        [Fact]
        public void NinguemEntraForaDePosicao()
        {
            foreach (Fase fase in TodasAsFases())
                foreach (List<TipoDeApostolo> rodada in new[] { fase.Rodada1, fase.Rodada2 })
                {
                    Assert.True(rodada.Count <= 4);
                    for (int casa = 0; casa < rodada.Count; casa++)
                    {
                        bool frente = rodada[casa] is TipoDeApostolo.Guardiao or TipoDeApostolo.Combatente;
                        Assert.Equal(casa <= 1, frente);
                    }
                }
        }

        [Fact]
        public void Simbolo_CobreOsQuatroTipos()
        {
            var simbolos = Enum.GetValues<TipoDeApostolo>().Select(Tipos.Simbolo).ToList();
            Assert.DoesNotContain(simbolos, s => string.IsNullOrWhiteSpace(s));
            Assert.Equal(simbolos.Count, simbolos.Distinct().Count());
        }
    }
}
