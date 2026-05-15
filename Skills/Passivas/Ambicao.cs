using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    class Ambicao : HabilidadePassiva
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

        public Ambicao() : base("Ambição", "???", 0,
            "Ao receber golpe, aumenta o próprio ATK em 5% até 25%.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeReceberDano && ctx.AlvoVivo;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            var estado = ObterEstado<Estado>(atacante);
            if (estado.TotalAumentado >= Cap) return SemDano();

            double aumentar = Math.Min(AumentoPorHit, Cap - estado.TotalAumentado);
            int delta = (int)(atacante.Ataque * aumentar);
            if (delta <= 0) return SemDano();

            atacante.ModificarAtaque(delta);
            estado.TotalAumentado += aumentar;
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}
