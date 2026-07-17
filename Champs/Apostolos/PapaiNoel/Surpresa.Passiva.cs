using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.Apostolos
{
    /// <summary>
    /// Ao ser atacado, 10% de chance de aplicar Preso 1t no atacante. Migrada para
    /// o modelo de reação (IReageAoSerAtacado). "Presente surpresa que te prende."
    /// Só declara mensagem quando dispara.
    /// </summary>
    class Surpresa : HabilidadePassiva, IReageAoSerAtacado
    {
        private const double ChancePreso = 0.10;
        private static readonly Random _random = new Random();

        public Surpresa() : base("Surpresa", "🎁", 0,
            "10% de chance de Prender o atacante ao ser atacado.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Contraparte.EstaVivo()) return new List<ResultadoReacao>();
            if (_random.NextDouble() >= ChancePreso) return new List<ResultadoReacao>();

            new Preso(turnos: 1).Aplicar(ctx.Contraparte);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"🎁 A Surpresa prendeu {ctx.Contraparte.Personagem.Nome}!"
                )
            };
        }
    }
}