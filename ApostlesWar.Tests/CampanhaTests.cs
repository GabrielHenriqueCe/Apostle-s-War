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
        private static IEnumerable<Fase> TodasAsFases(Dificuldade dificuldade)
            => Enum.GetValues<Fases>().Select(f => Campanha.ObterFase((int)f, dificuldade));

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

        /// <summary>
        /// O FÁCIL apresenta a facção um por fase, então a contagem cresce até o time fechar em
        /// quatro na fase 4. É esta escada que a segunda tabela NÃO tem.
        /// </summary>
        [Theory]
        [InlineData(Fases.Fase1, 2)]
        [InlineData(Fases.Fase2, 3)]
        [InlineData(Fases.Fase3, 5)]
        [InlineData(Fases.Fase4, 7)]
        [InlineData(Fases.Fase5, 8)]
        [InlineData(Fases.Fase6, 8)]
        [InlineData(Fases.Fase7, 8)]
        public void ContagemDeInimigos_NoFacil(Fases fase, int esperado)
        {
            Fase f = Campanha.ObterFase((int)fase, Dificuldade.Facil);
            Assert.Equal(esperado, f.Rodada1.Count + f.Rodada2.Count);
        }

        /// <summary>
        /// Da Normal pra cima são SEMPRE quatro, nas duas rodadas: lá o jogador já conhece os quatro,
        /// e o que muda de fase pra fase é a formação, não quantos.
        /// </summary>
        [Theory]
        [InlineData(Dificuldade.Normal)]
        [InlineData(Dificuldade.Dificil)]
        [InlineData(Dificuldade.Pesadelo)]
        public void ContagemDeInimigos_ForaDoFacil_SempreQuatro(Dificuldade dificuldade)
        {
            foreach (Fase f in TodasAsFases(dificuldade))
            {
                Assert.Equal(4, f.Rodada1.Count);
                Assert.Equal(4, f.Rodada2.Count);
            }
        }

        /// <summary>
        /// As três dificuldades acima do Fácil compartilham a MESMA tabela — se um dia uma delas
        /// ganhar composição própria, é este teste que cai primeiro.
        /// </summary>
        [Fact]
        public void NormalDificilEPesadelo_CompartilhamATabela()
        {
            foreach (Fases fase in Enum.GetValues<Fases>())
            {
                Fase normal = Campanha.ObterFase((int)fase, Dificuldade.Normal);
                Assert.Equal(normal.Rodada2, Campanha.ObterFase((int)fase, Dificuldade.Dificil).Rodada2);
                Assert.Equal(normal.Rodada2, Campanha.ObterFase((int)fase, Dificuldade.Pesadelo).Rodada2);
            }
        }

        /// <summary>
        /// Invariante 1 do GDD §A COMPOSIÇÃO DAS FASES: a formação estreia como rodada 2 e volta como
        /// aquecimento da fase seguinte. Sem isto as sete fases viram lutas soltas.
        /// </summary>
        [Theory]
        [InlineData(Dificuldade.Facil)]
        [InlineData(Dificuldade.Normal)]
        public void Rodada1_RepeteARodada2_DaFaseAnterior(Dificuldade dificuldade)
        {
            for (int fase = 2; fase <= 7; fase++)
                Assert.Equal(Campanha.ObterFase(fase - 1, dificuldade).Rodada2,
                             Campanha.ObterFase(fase, dificuldade).Rodada1);
        }

        /// <summary>
        /// Invariante 2: G/C nas casas 1-2, A/S nas 3-4 — e a ordem da lista É a casa. Ninguém entra
        /// fora de posição; a variedade vem da composição.
        /// </summary>
        [Theory]
        [InlineData(Dificuldade.Facil)]
        [InlineData(Dificuldade.Normal)]
        public void NinguemEntraForaDePosicao(Dificuldade dificuldade)
        {
            foreach (Fase fase in TodasAsFases(dificuldade))
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
