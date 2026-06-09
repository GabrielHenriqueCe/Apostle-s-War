using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, 25% de chance de aplicar Medo 1t no atacante. Migrada para
    /// o modelo de reação (IReageAoSerAtacado). Só declara mensagem quando dispara.
    /// </summary>
    class PassivaOgro : HabilidadePassiva, IReageAoSerAtacado
    {
        private const double ChanceMedo = 0.25;
        private static readonly Random _random = new Random();

        public PassivaOgro() : base("Intimidador", "👹", 0,
            "25% de chance de aplicar Medo no atacante ao ser atacado.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Outro.EstaVivo()) return new List<ResultadoReacao>();
            if (_random.NextDouble() >= ChanceMedo) return new List<ResultadoReacao>();

            new Medo(turnos: 1).Aplicar(ctx.Outro);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"👹 O Intimidador encheu {ctx.Outro.Personagem.Nome} de Medo!"
                )
            };
        }
    }
}