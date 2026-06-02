using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    class CoroaDoSoberano : HabilidadePassiva
    {
        private const double AumentoPorHit = 0.05;
        private const double Cap = 0.25;

        private class Estado
        {
            public double TotalAumentado;
        }

        public CoroaDoSoberano() : base("Coroa do Soberano", "👑", 0,
            "Ao ser atacado, aumenta a própria DEF em 5% até 25%.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeSerAtacado && ctx.AlvoVivo;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var estado = ObterEstado<Estado>(ctx.Atacante);
            if (estado.TotalAumentado >= Cap) return SemDano();

            double aumentar = Math.Min(AumentoPorHit, Cap - estado.TotalAumentado);
            int delta = (int)(ctx.Atacante.DefesaComItens * aumentar);
            if (delta <= 0) return SemDano();

            ctx.Atacante.AdicionarBonusDefesaPermanente(delta);
            estado.TotalAumentado += aumentar;
            return SemDano();
        }
    }
}