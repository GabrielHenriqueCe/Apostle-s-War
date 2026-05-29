using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, tem 25% de chance de aplicar Medo 1t no atacante.
    /// </summary>
    class PassivaOgro : HabilidadePassiva
    {
        private const double ChanceMedo = 0.25;

        private static readonly Random _random = new Random();

        public PassivaOgro() : base("Intimidador", "👹", 0,
            "25% de chance de aplicar Medo no atacante ao ser atacado.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeSerAtacado && ctx.AlvoVivo;

        // ctx.Atacante = Ogro (portador); alvo = quem atacou o Ogro
        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            if (!alvo.EstaVivo()) return SemDano();
            if (_random.NextDouble() >= ChanceMedo) return SemDano();

            new Medo(turnos: 1).Aplicar(alvo);
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}