using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Apostolos
{
    /// <summary>
    /// Ao ser atacado, 10% de chance de aplicar Preso 1t no atacante. Migrada para
    /// o modelo de reação (IReageAoSerAtacado). "Presente surpresa que te prende."
    /// Só declara mensagem quando dispara.
    /// </summary>
    public class Surpresa : HabilidadePassiva, IReageAoSerAtacado
    {
        private const double ChancePreso = 0.10;

        public Surpresa() : base("Surpresa", "🎁", 0,
            "10% de chance de Prender o atacante ao ser atacado.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Contraparte.EstaVivo()) return new List<ResultadoReacao>();
            if (Random.Shared.NextDouble() >= ChancePreso) return new List<ResultadoReacao>();

            new Preso(duracao: 1).Aplicar(ctx.Contraparte);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"🎁 A Surpresa prendeu {ctx.Contraparte.Personagem.Nome}!"
                )
            };
        }
    }
}