using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao receber um golpe, aumenta a própria DEF em 5% permanentemente até máximo de 25%.
    /// </summary>
    class PassivaRei : HabilidadePassiva
    {
        private double _totalAumentado = 0;
        private const double AumentoPorHit = 0.05;
        private const double Cap = 0.25;

        public PassivaRei() : base("Coroa do Soberano", "👑", 0,
            "Ao receber golpe, aumenta a própria DEF em 5% até 25%.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeReceberDano && ctx.AlvoVivo;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            if (_totalAumentado >= Cap) return SemDano();

            double aumentar = Math.Min(AumentoPorHit, Cap - _totalAumentado);
            int delta = (int)(atacante.Defesa * aumentar);
            if (delta <= 0) return SemDano();

            atacante.ModificarDefesa(delta);
            _totalAumentado += aumentar;
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}
