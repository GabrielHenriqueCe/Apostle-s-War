using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    class Ambicao : HabilidadePassiva
    {
        private const double AumentoPorHit = 0.05;
        private const double Cap = 0.25;

        private class Estado
        {
            public double TotalAumentado;
        }

        public Ambicao() : base("Ambição", "???", 0,
            "Ao receber golpe, aumenta o próprio ATK em 5% até 25%.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeReceberDano && ctx.AlvoVivo;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var estado = ObterEstado<Estado>(ctx.Atacante);
            if (estado.TotalAumentado >= Cap) return SemDano();

            double aumentar = Math.Min(AumentoPorHit, Cap - estado.TotalAumentado);
            int delta = (int)(ctx.Atacante.Ataque * aumentar);
            if (delta <= 0) return SemDano();

            ctx.Atacante.ModificarAtaque(delta);
            estado.TotalAumentado += aumentar;
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}