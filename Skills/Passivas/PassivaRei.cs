using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao receber um golpe, aumenta a própria DEF em 5% até máximo de 25% durante o combate.
    /// </summary>
    class PassivaRei : HabilidadePassiva
    {
        private const double AumentoPorHit = 0.05;
        private const double Cap = 0.25;

        /// <summary>
        /// Estado per-combate desta passiva.
        /// </summary>
        private class Estado
        {
            public double TotalAumentado;
        }

        public PassivaRei() : base("Coroa do Soberano", "👑", 0,
            "Ao receber golpe, aumenta a própria DEF em 5% até 25%.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeReceberDano && ctx.AlvoVivo;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            var estado = ObterEstado<Estado>(atacante);
            if (estado.TotalAumentado >= Cap) return SemDano();

            double aumentar = Math.Min(AumentoPorHit, Cap - estado.TotalAumentado);
            int delta = (int)(atacante.Defesa * aumentar);
            if (delta <= 0) return SemDano();

            atacante.ModificarDefesa(delta);
            estado.TotalAumentado += aumentar;
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}
