using ApostlesWar.Domain;
using ApostlesWar.Domain.Champs.Tecnologicos;
using ApostlesWar.Domain.Skills;
using ApostlesWar.Domain.Skills.Buffs;

namespace Tests
{
    /// <summary>
    /// Status PERMANENTE tem que aguentar que alguém some turnos nele.
    ///
    /// Era `int.MaxValue` cru, e o <see cref="StatusEffect.AumentarDuracao"/> — que o Raio-X do Robô
    /// chama pra ESTENDER benefícios — estourava o int e caía em negativo, que o
    /// <see cref="StatusEffect.Expirou"/> lê como acabado. A habilidade que promete prolongar o buff
    /// APAGAVA justamente o permanente, que é o que mais dói perder (o Intocável do Fantasma).
    ///
    /// Achado pela BANCADA, e por um número esquisito: o Robô media dano em múltiplos de 30 entre
    /// corridas, e 30 é exatamente a diferença entre um A1 crítico e um não-crítico ali. O crítico
    /// cravado da bancada é um buff de duração permanente — o Raio-X dele o apagava no 1º uso e o
    /// champ voltava à taxa base de 15%. Nenhuma tela mostraria isso; só o número.
    /// </summary>
    public class DuracaoPermanenteTests
    {
        private static Combate Novo()
            => new Jogador(new Personagem(1, Faccao.Humanos, "Teste", "🧪", 1000, 200, 100));

        /// <summary>
        /// O INVARIANTE, e o motivo de a constante não ser `int.MaxValue`: tem que sobrar espaço pra
        /// somar. É este assert que quebra se alguém devolver o valor cru pra constante.
        /// </summary>
        [Fact]
        public void Permanente_TemFolgaProCaminhoQueSomaTurnos()
        {
            Assert.True(StatusEffect.Permanente < int.MaxValue - 1_000_000);
        }

        [Fact]
        public void AumentarDuracao_NumPermanente_NaoEstouraNemExpira()
        {
            var buff = new Intocavel(duracao: StatusEffect.Permanente);

            buff.AumentarDuracao(1);

            Assert.True(buff.DuracaoRestante > 0);   // com int.MaxValue isto virava int.MinValue
            Assert.False(buff.Expirou);
        }

        /// <summary>
        /// O caminho de verdade, ponta a ponta: o Raio-X do Robô sobre um aliado que carrega o
        /// Intocável PERMANENTE do Fantasma. Antes, o buff sumia; agora ele fica — e mais longo, que
        /// é o que a habilidade promete.
        /// </summary>
        [Fact]
        public void RaioXDoRobo_EstendeOIntocavelPermanente_EmVezDeApagar()
        {
            Combate aliado = Novo();
            new Intocavel(duracao: StatusEffect.Permanente, removivel: false).Aplicar(aliado);

            HabilidadeAtiva raioX = Robo.Definir().Habilidades
                .OfType<HabilidadeAtiva>().First(h => h.Nome == "Raio-X");
            Acao estender = raioX.Acoes.OfType<EstenderBuffs>().Single();

            estender.Executar(Novo(), aliado, new List<EventoCombate>());

            Intocavel? depois = aliado.StatusAtivos.OfType<Intocavel>().FirstOrDefault();
            Assert.NotNull(depois);
            Assert.False(depois!.Expirou);
            Assert.Equal(StatusEffect.Permanente + 1, depois.DuracaoRestante);
        }
    }
}
