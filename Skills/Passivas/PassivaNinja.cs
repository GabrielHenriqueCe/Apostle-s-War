using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    class PassivaSorrateiro : HabilidadePassiva
    {
        private readonly Dictionary<Combate, double> _totalReduzido = new();
        private const double ReducaoPorHit = 0.03;
        private const double Cap = 0.25;

        public PassivaSorrateiro() : base("Sorrateiro", "👁️", 0,
            "Cada ataque reduz a DEF do inimigo em 3%, até 25%.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeAtacar;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            if (!_totalReduzido.ContainsKey(alvo))
                _totalReduzido[alvo] = 0;

            if (_totalReduzido[alvo] >= Cap) return SemDano();

            double reduzir = Math.Min(ReducaoPorHit, Cap - _totalReduzido[alvo]);
            int delta = (int)(alvo.Defesa * reduzir);
            if (delta <= 0) return SemDano();

            alvo.ModificarDefesa(-delta);
            _totalReduzido[alvo] += reduzir;
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}