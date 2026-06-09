using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, 10% de chance de aplicar Preso 1t no atacante. Migrada para
    /// o modelo de reação (IReageAoSerAtacado). "Presente surpresa que te prende."
    /// Só declara mensagem quando dispara.
    /// </summary>
    class PassivaPapaiNoel : HabilidadePassiva, IReageAoSerAtacado
    {
        private const double ChancePreso = 0.10;
        private static readonly Random _random = new Random();

        public PassivaPapaiNoel() : base("Surpresa", "🎁", 0,
            "10% de chance de Prender o atacante ao ser atacado.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Outro.EstaVivo()) return new List<ResultadoReacao>();
            if (_random.NextDouble() >= ChancePreso) return new List<ResultadoReacao>();

            new Preso(turnos: 1).Aplicar(ctx.Outro);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"🎁 A Surpresa prendeu {ctx.Outro.Personagem.Nome}!"
                )
            };
        }
    }
}