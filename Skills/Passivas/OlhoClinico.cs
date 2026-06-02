using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// A cada ataque, ganha +5% de TaxaCrit acumulável, até 25%.
    /// </summary>
    class OlhoClinico : HabilidadePassiva
    {
        private const double AumentoPorHit = 0.05;
        private const double Cap = 0.25;

        private class Estado
        {
            public double TotalAumentado;
        }

        public OlhoClinico() : base("Olho Clínico", "🚬", 0,
            "A cada ataque, ganha +5% de TaxaCrit, até 25%.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeAtacar;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var estado = ObterEstado<Estado>(ctx.Atacante);
            if (estado.TotalAumentado >= Cap) return SemDano();

            double aumentar = Math.Min(AumentoPorHit, Cap - estado.TotalAumentado);
            ctx.Atacante.AdicionarBonusTaxaCritPermanente(aumentar);
            estado.TotalAumentado += aumentar;
            return SemDano();
        }
    }
}