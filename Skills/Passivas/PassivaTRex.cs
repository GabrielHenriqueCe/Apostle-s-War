using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, tem 25% de chance de aplicar ReducaoDefesa 2t no atacante.
    /// </summary>
    class PassivaTRex : HabilidadePassiva
    {
        private const double ChanceReducao = 0.25;

        private static readonly Random _random = new Random();

        public PassivaTRex() : base("Pele Grossa", "🦖", 0,
            "25% de chance de reduzir DEF do atacante ao ser atacado.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeSerAtacado && ctx.AlvoVivo;

        // ctx.Atacante = T-Rex (portador); alvo = quem atacou
        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            if (!alvo.EstaVivo()) return SemDano();
            if (_random.NextDouble() >= ChanceReducao) return SemDano();

            new ReducaoDefesa(turnos: 2).Aplicar(alvo);
            return SemDano();
        }
    }
}