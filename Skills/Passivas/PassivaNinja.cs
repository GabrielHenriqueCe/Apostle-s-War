using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    class PassivaSorrateiro : HabilidadePassiva
    {
        private const double ReducaoPorHit = 0.03;
        private const double Cap = 0.25;

        private class Estado
        {
            public Dictionary<Combate, double> TotalReduzido = new();
        }

        public PassivaSorrateiro() : base("Sorrateiro", "👁️", 0,
            "Cada ataque reduz a DEF do inimigo em 3%, até 25%.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeAtacar;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var estado = ObterEstado<Estado>(ctx.Atacante);

            if (!estado.TotalReduzido.ContainsKey(alvo))
                estado.TotalReduzido[alvo] = 0;

            if (estado.TotalReduzido[alvo] >= Cap) return SemDano();

            double reduzir = Math.Min(ReducaoPorHit, Cap - estado.TotalReduzido[alvo]);
            int delta = (int)(alvo.Defesa * reduzir);
            if (delta <= 0) return SemDano();

            alvo.ModificarDefesa(-delta);
            estado.TotalReduzido[alvo] += reduzir;
            return SemDano();
        }
    }
}