using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, tem 10% de chance de aplicar Preso 1t no atacante.
    /// Tematicamente: "presente surpresa que te prende".
    /// </summary>
    class PassivaPapaiNoel : HabilidadePassiva
    {
        private const double ChancePreso = 0.10;

        private static readonly Random _random = new Random();

        public PassivaPapaiNoel() : base("Surpresa", "🎁", 0,
            "10% de chance de Prender o atacante ao ser atacado.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeSerAtacado && ctx.AlvoVivo;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            if (!alvo.EstaVivo()) return SemDano();
            if (_random.NextDouble() >= ChancePreso) return SemDano();

            new Preso(turnos: 1).Aplicar(alvo);
            return SemDano();
        }
    }
}