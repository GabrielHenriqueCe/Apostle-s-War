using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace Tests
{
    /// <summary>
    /// PRECISÃO × RESISTÊNCIA (GDD §1): o malefício cola? A chance é
    /// <c>min(100%, Precisão ÷ (Resistência × 2))</c>, e quem colou pode vir com um turno a menos.
    ///
    /// Os testes de PROBABILIDADE são das funções puras — o sorteio em si sai pelas duas pontas
    /// determinísticas (chance 1 e chance 0), que é onde dá pra afirmar sem contar moeda.
    /// </summary>
    public class PrecisaoEResistenciaTests
    {
        private static Combate Com(int precisao, int resistencia) =>
            new Jogador(new Personagem(1, Faccao.Humanos, "T", "🧪", 1000, 100, 0)
                .ComPrecisao(precisao).ComResistencia(resistencia));

        // ---------- a curva ----------

        [Theory]
        [InlineData(500, 500, 0.50)]    // empate → meio a meio
        [InlineData(1000, 500, 1.00)]   // o DOBRO da Resistência → garantido
        [InlineData(250, 500, 0.25)]
        [InlineData(750, 500, 0.75)]
        [InlineData(3000, 500, 1.00)]   // satura: não existe "mais que garantido"
        [InlineData(0, 500, 0.00)]      // sem Precisão nenhuma, nada cola
        public void ChanceDeColar_BateComOGDD(int precisao, int resistencia, double esperada)
        {
            Assert.Equal(esperada, Com(precisao, resistencia).ChanceDeColarEm(Com(0, resistencia)), 10);
        }

        /// <summary>
        /// A saturação é a diferença de propósito entre esta curva e a da DEF: na defesa se QUER que
        /// nunca sature; aqui é preciso poder aplicar o efeito cheio, senão nenhuma habilidade faz o
        /// que está escrito nela.
        /// </summary>
        [Fact]
        public void QuemChegaAoTeto_ColaSempreEColaCheio()
        {
            var certeiro = Com(precisao: 1000, resistencia: 100);
            var alvo = Com(precisao: 0, resistencia: 500);

            Assert.Equal(1.0, certeiro.ChanceDeColarEm(alvo), 10);
            // A segunda rolagem morre junto com a primeira — sem regra separada pra isso.
            Assert.Equal(0.0, certeiro.ChanceDeAparaUmTurnoEm(alvo), 10);
        }

        [Fact]
        public void AlvoSemResistencia_ApanhaTudo()
        {
            Assert.Equal(1.0, Com(1, 0).ChanceDeColarEm(Com(0, 0)), 10);
        }

        /// <summary>
        /// A chance de vir aparado é `(1 − colar) ÷ 2`, e ela é CONDICIONAL a ter colado. No empate:
        /// 50% de nada, e dos 50% que colam, um quarto vem com um turno a menos.
        /// </summary>
        [Fact]
        public void AChanceDeAparar_EMetadeDoQueFaltaPraGarantia()
        {
            var atacante = Com(precisao: 500, resistencia: 0);
            var alvo = Com(precisao: 0, resistencia: 500);

            Assert.Equal(0.25, atacante.ChanceDeAparaUmTurnoEm(alvo), 10);
        }

        // ---------- as duas pontas, ponta-a-ponta ----------

        private static void Aplicar(Combate atacante, Combate alvo, int duracao) =>
            new AplicarDebuff(() => new ReducaoAtaque(duracao))
                .Executar(atacante, alvo, new List<EventoCombate>());

        [Fact]
        public void ComPrecisaoNoTeto_ODebuffColaSempreEComADuracaoCheia()
        {
            var atacante = Com(precisao: 1000, resistencia: 0);

            for (int i = 0; i < 50; i++)
            {
                var alvo = Com(precisao: 0, resistencia: 500);
                Aplicar(atacante, alvo, duracao: 3);

                var posto = Assert.Single(alvo.StatusAtivos.OfType<ReducaoAtaque>());
                Assert.Equal(3, posto.DuracaoRestante);
            }
        }

        [Fact]
        public void SemPrecisao_ODebuffNuncaCola()
        {
            var atacante = Com(precisao: 0, resistencia: 0);

            for (int i = 0; i < 50; i++)
            {
                var alvo = Com(precisao: 0, resistencia: 500);
                Aplicar(atacante, alvo, duracao: 3);

                Assert.Empty(alvo.StatusAtivos.OfType<ReducaoAtaque>());
            }
        }

        /// <summary>
        /// O piso de 1 turno: já colou, então dura. Sem ele, aparar um debuff de 1 turno o apagaria
        /// pela porta dos fundos e "colou" deixaria de querer dizer alguma coisa.
        /// </summary>
        [Fact]
        public void DebuffDeUmTurno_NuncaEAparadoAteSumir()
        {
            var atacante = Com(precisao: 1, resistencia: 0);   // quase nunca cola, mas quando cola…

            for (int i = 0; i < 200; i++)
            {
                var alvo = Com(precisao: 0, resistencia: 500);
                Aplicar(atacante, alvo, duracao: 1);

                foreach (var posto in alvo.StatusAtivos.OfType<ReducaoAtaque>())
                    Assert.Equal(1, posto.DuracaoRestante);
            }
        }

        /// <summary>
        /// AUTO-MALEFÍCIO NÃO ROLA: o que se impõe a si mesmo é escolha, não imposição. Um custo de
        /// habilidade que às vezes não se paga seria outra mecânica.
        /// </summary>
        [Fact]
        public void EmSiMesmo_ODebuffNaoPassaPelaDisputa()
        {
            var sozinho = Com(precisao: 0, resistencia: 500);   // contra si, a chance seria zero

            Aplicar(sozinho, sozinho, duracao: 2);

            Assert.Single(sozinho.StatusAtivos.OfType<ReducaoAtaque>());
        }
    }
}
