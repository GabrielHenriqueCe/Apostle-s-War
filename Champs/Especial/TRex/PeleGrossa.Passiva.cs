using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.Especial
{
    /// <summary>
    /// Ao ser atacado, 25% de chance de aplicar ReducaoDefesa 2t no atacante.
    /// Migrada para o modelo de reação (IReageAoSerAtacado). Só declara mensagem
    /// quando dispara.
    /// </summary>
    class PeleGrossa : HabilidadePassiva, IReageAoSerAtacado
    {
        private const double ChanceReducao = 0.25;
        private static readonly Random _random = new Random();

        public PeleGrossa() : base("Pele Grossa", "🦖", 0,
            "25% de chance de reduzir DEF do atacante ao ser atacado.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Contraparte.EstaVivo()) return new List<ResultadoReacao>();
            if (_random.NextDouble() >= ChanceReducao) return new List<ResultadoReacao>();

            new ReducaoDefesa(turnos: 2).Aplicar(ctx.Contraparte);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"🦖 Pele Grossa enfraqueceu a defesa de {ctx.Contraparte.Personagem.Nome}!"
                )
            };
        }
    }
}
