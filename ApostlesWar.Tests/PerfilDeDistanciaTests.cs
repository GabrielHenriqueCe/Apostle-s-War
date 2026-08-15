using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace Tests
{
    /// <summary>
    /// O PERFIL DE DISTÂNCIA (GDD §2): quatro casas por lado com as frentes se olhando,
    /// `distância = casa do atacante + casa do alvo − 1`, e um multiplicador que é máximo na
    /// distância ideal do TIPO e cai 0,10 por casa de desvio pros dois lados.
    ///
    /// Duas metades, e a divisão é de propósito: a tabela é função PURA (<c>Arquetipos</c>), e o
    /// resto é o ponta-a-ponta pelo <c>Atacar</c> — que é onde a geometria pode entrar no lugar
    /// errado do pipeline sem ninguém ver.
    ///
    /// Crítico zerado com ReducaoTaxaCrit (0.15 base → 0) pra o DanoBruto ser determinístico, no
    /// mesmo molde do ModificaDanoCausadoTests.
    /// </summary>
    public class PerfilDeDistanciaTests
    {
        private const int Atk = 200;

        /// <summary>Combatente (o tipo da ficha crua), ATK 200, nunca crita.</summary>
        private static Combate Atacante(int casa)
        {
            var c = new Jogador(new Personagem(1, Faccao.Humanos, "Atacante", "🗡️", 1000, Atk, 0));
            new ReducaoTaxaCrit(valor: 0.25).Aplicar(c);
            c.IniciarCombate(casa);
            return c;
        }

        private static Combate Alvo(int casa, int def = 0)
        {
            var c = new Jogador(new Personagem(1, Faccao.Humanos, "Alvo", "🎯", 100_000, 0, def));
            c.IniciarCombate(casa);
            return c;
        }

        // ---------- a tabela pura ----------

        [Theory]
        [InlineData(TipoDeApostolo.Guardiao, 1.30, 1.20, 1.10, 1.00, 0.90, 0.80, 0.70)]
        [InlineData(TipoDeApostolo.Combatente, 1.00, 1.10, 1.20, 1.30, 1.20, 1.10, 1.00)]
        [InlineData(TipoDeApostolo.Atirador, 0.90, 1.00, 1.10, 1.20, 1.30, 1.20, 1.10)]
        [InlineData(TipoDeApostolo.Suporte, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00)]
        public void Tabela_BateComOGDD(TipoDeApostolo tipo, params double[] porDistancia)
        {
            for (int distancia = 1; distancia <= porDistancia.Length; distancia++)
                Assert.Equal(porDistancia[distancia - 1],
                    Arquetipos.MultiplicadorDePosicao(tipo, distancia), 10);
        }

        [Fact]
        public void Distancia_VaiDe1FrenteContraFrente_A7FundoContraFundo()
        {
            Assert.Equal(1, Arquetipos.DistanciaEntreCasas(1, 1));
            Assert.Equal(7, Arquetipos.DistanciaEntreCasas(4, 4));
            Assert.Equal(Arquetipos.DistanciaMinima, Arquetipos.DistanciaEntreCasas(1, 1));
            Assert.Equal(Arquetipos.DistanciaMaxima, Arquetipos.DistanciaEntreCasas(4, 4));

            // Simétrica: quem recua uma casa é indistinguível do alvo que recuou uma casa.
            Assert.Equal(Arquetipos.DistanciaEntreCasas(2, 3), Arquetipos.DistanciaEntreCasas(3, 2));
        }

        [Fact]
        public void Suporte_NaoTemDistanciaIdeal_ERendeIgualEmTodaCasa()
        {
            Assert.Null(Arquetipos.DistanciaIdeal(TipoDeApostolo.Suporte));
            Assert.Equal(1, Arquetipos.DistanciaIdeal(TipoDeApostolo.Guardiao));
            Assert.Equal(4, Arquetipos.DistanciaIdeal(TipoDeApostolo.Combatente));
            Assert.Equal(5, Arquetipos.DistanciaIdeal(TipoDeApostolo.Atirador));
        }

        /// <summary>
        /// O Atirador quer 5 e da casa 1 alcança 4 no máximo — é o único que não atinge o próprio
        /// pico estando na frente, e o GDD aceita isso de propósito (baixar o d* dele pra 4 o
        /// empataria com o Combatente).
        /// </summary>
        [Fact]
        public void Atirador_NaCasaDaFrente_NuncaChegaNoProprioPico()
        {
            for (int casaAlvo = 1; casaAlvo <= 4; casaAlvo++)
            {
                double mult = Arquetipos.MultiplicadorDePosicao(TipoDeApostolo.Atirador,
                    Arquetipos.DistanciaEntreCasas(Arquetipos.CasaDaFrente, casaAlvo));
                Assert.True(mult <= 1.20, $"casa do alvo {casaAlvo} deu {mult}");
            }
        }

        /// <summary>
        /// A fila só tem 7 distâncias. Se um chamador inventar uma 12ª, o multiplicador não pode
        /// virar negativo — dano negativo CURA o alvo, e ninguém veria isso acontecer.
        /// </summary>
        [Fact]
        public void DistanciaForaDaFila_NaoProduzMultiplicadorNegativo()
        {
            Assert.Equal(Arquetipos.MultiplicadorDePosicao(TipoDeApostolo.Guardiao, 7),
                Arquetipos.MultiplicadorDePosicao(TipoDeApostolo.Guardiao, 99));
            Assert.Equal(Arquetipos.MultiplicadorDePosicao(TipoDeApostolo.Atirador, 1),
                Arquetipos.MultiplicadorDePosicao(TipoDeApostolo.Atirador, -3));
        }

        // ---------- o ponta-a-ponta pelo Atacar ----------

        [Fact]
        public void Atacar_MultiplicaOBruto_PelaPosicaoDosDois()
        {
            // Combatente (d* 4) na casa 1: o pico dele cai na casa 4 do inimigo.
            Assert.Equal(200, Atacante(casa: 1).Atacar(Alvo(casa: 1), 1.0).DanoBruto);   // d1 → 1,00
            Assert.Equal(220, Atacante(casa: 1).Atacar(Alvo(casa: 2), 1.0).DanoBruto);   // d2 → 1,10
            Assert.Equal(240, Atacante(casa: 1).Atacar(Alvo(casa: 3), 1.0).DanoBruto);   // d3 → 1,20
            Assert.Equal(260, Atacante(casa: 1).Atacar(Alvo(casa: 4), 1.0).DanoBruto);   // d4 → 1,30

            // O pico ANDA com quem se move: da casa 2 ele passa a bater mais forte na casa 3.
            Assert.Equal(260, Atacante(casa: 2).Atacar(Alvo(casa: 3), 1.0).DanoBruto);
            Assert.Equal(240, Atacante(casa: 2).Atacar(Alvo(casa: 4), 1.0).DanoBruto);
        }

        /// <summary>
        /// A posição é do lado do ATACANTE e chega ANTES da mitigação — é irmã do ATK, não da DEF.
        /// Se um dia ela escorregar pra dentro da OrdemDeMitigacao (#185), a razão entre os dois
        /// alvos deixa de ser exatamente a razão dos multiplicadores.
        /// </summary>
        [Fact]
        public void Posicao_EntraAntesDaMitigacao()
        {
            var atacante = Atacante(casa: 1);

            // DEF 500 → 9,09% de redução (a curva do GDD §1), a mesma nos dois: só a geometria os separa.
            var perto = atacante.Atacar(Alvo(casa: 1, def: 500), 1.0);
            var longe = atacante.Atacar(Alvo(casa: 4, def: 500), 1.0);

            Assert.Equal(200, perto.DanoBruto);
            Assert.Equal(260, longe.DanoBruto);
            Assert.Equal(181, perto.DanoEfetivo);   // (int)(200 × 0,9091)
            Assert.Equal(236, longe.DanoEfetivo);   // (int)(260 × 0,9091)
        }

        /// <summary>
        /// Quem nasce fora de uma equipe não tem casa — e aí a geometria não existe, em vez de
        /// existir com um número inventado. É o caso dos bonecos de teste.
        /// </summary>
        [Fact]
        public void ForaDoTabuleiro_NaoMexeNoDano()
        {
            var semCasa = new Jogador(new Personagem(1, Faccao.Humanos, "Boneco", "🎯", 1000, Atk, 0));
            new ReducaoTaxaCrit(valor: 0.25).Aplicar(semCasa);

            Assert.Equal(Combate.ForaDoTabuleiro, semCasa.Casa);
            Assert.Equal(1.0, semCasa.MultiplicadorDePosicaoContra(Alvo(casa: 4)));
            Assert.Equal(1.0, Atacante(casa: 1).MultiplicadorDePosicaoContra(
                new Jogador(new Personagem(1, Faccao.Humanos, "Boneco", "🎯", 1000, 0, 0))));

            // Um dos dois fora basta pra não haver distância — nas duas direções.
            Assert.Equal(200, semCasa.Atacar(Alvo(casa: 4), 1.0).DanoBruto);
        }

        /// <summary>
        /// O <c>PreverDanoRecebido</c> é o caminho do bot, e ele passou a enxergar a geometria de
        /// graça. Se os dois divergirem o bot mira errado EM SILÊNCIO — por isso a previsão é
        /// comparada com o golpe de verdade, e não com uma conta repetida aqui.
        /// </summary>
        [Fact]
        public void PreverAtaque_EnxergaAMesmaPosicaoQueOAtacar()
        {
            for (int casaAlvo = 1; casaAlvo <= 4; casaAlvo++)
            {
                var atacante = Atacante(casa: 1);
                var alvo = Alvo(casaAlvo, def: 500);

                int previsto = atacante.PreverAtaque(alvo, 1.0);
                int real = atacante.Atacar(alvo, 1.0).DanoEfetivo;

                Assert.Equal(real, previsto);
            }
        }

        /// <summary>
        /// O bot escolhe pelo dano previsto, então a geometria vira escolha de alvo sem uma linha
        /// de bot: entre alvos idênticos, ele passa a preferir quem está na distância ideal.
        /// </summary>
        [Fact]
        public void OBotPassaAPreferirOAlvoNaDistanciaIdeal()
        {
            var combatente = Atacante(casa: 1);            // d* 4 → o pico é a casa 4 do inimigo
            var naFrente = Alvo(casa: 1);
            var noFundo = Alvo(casa: 4);

            Assert.True(combatente.PreverAtaque(noFundo, 1.0) > combatente.PreverAtaque(naFrente, 1.0));
        }
    }
}
