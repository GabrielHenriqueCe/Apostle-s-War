using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, 25% de chance de aplicar ReducaoDefesa 2t no atacante.
    /// Migrada para o modelo de reação (IReageAoSerAtacado). Só declara mensagem
    /// quando dispara.
    /// </summary>
    class PassivaTRex : HabilidadePassiva, IReageAoSerAtacado
    {
        private const double ChanceReducao = 0.25;
        private static readonly Random _random = new Random();

        public PassivaTRex() : base("Pele Grossa", "🦖", 0,
            "25% de chance de reduzir DEF do atacante ao ser atacado.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Outro.EstaVivo()) return new List<ResultadoReacao>();
            if (_random.NextDouble() >= ChanceReducao) return new List<ResultadoReacao>();

            new ReducaoDefesa(turnos: 2).Aplicar(ctx.Outro);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"🦖 Pele Grossa enfraqueceu a defesa de {ctx.Outro.Personagem.Nome}!"
                )
            };
        }
    }
}