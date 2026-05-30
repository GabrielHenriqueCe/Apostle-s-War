using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Cada ataque +5% DanoCrit, até 25%. Estado per-combate via ObterEstado.
    /// </summary>
    class PassivaInvasor : HabilidadePassiva
    {
        private const double AumentoPorHit = 0.05;
        private const double Cap = 0.25;

        private class Estado
        {
            public double TotalAumentado;
        }

        public PassivaInvasor() : base("Vírus", "👾", 0,
            "A cada ataque, +5% de Dano Crítico, até 25%.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeAtacar;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var estado = ObterEstado<Estado>(ctx.Atacante);
            if (estado.TotalAumentado >= Cap) return SemDano();

            double aumentar = Math.Min(AumentoPorHit, Cap - estado.TotalAumentado);
            ctx.Atacante.DefinirDanoCrit(ctx.Atacante.DanoCrit + aumentar);
            estado.TotalAumentado += aumentar;
            return SemDano();
        }
    }
}