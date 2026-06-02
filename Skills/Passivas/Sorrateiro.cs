using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Cada ataque reduz a DEF do inimigo em 5% (sobre a DEF com itens),
    /// até um cap de -25% TOTAL no alvo.
    /// 
    /// A redução mora no próprio alvo (Combate.ReducaoDefesaPermanente), não
    /// na passiva. Assim, múltiplos Sorrateiros (e futuras maestrias) que
    /// atacam o mesmo alvo COMPARTILHAM o cap — a DEF do inimigo não cai além
    /// de -25% no total, independente de quantos reduzem.
    /// </summary>
    class Sorrateiro : HabilidadePassiva
    {
        private const double ReducaoPorHit = 0.05;
        private const double Cap = 0.25;

        public Sorrateiro() : base("Sorrateiro", "👁️", 0,
            "Cada ataque reduz a DEF do inimigo em 5%, até 25%.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeAtacar;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            int reducaoMaxima = (int)(alvo.DefesaComItens * Cap);
            if (alvo.ReducaoDefesaPermanente >= reducaoMaxima) return SemDano();

            int incremento = (int)(alvo.DefesaComItens * ReducaoPorHit);
            int aplicar = Math.Min(incremento, reducaoMaxima - alvo.ReducaoDefesaPermanente);
            if (aplicar <= 0) return SemDano();

            alvo.AdicionarReducaoDefesaPermanente(aplicar);
            return SemDano();
        }
    }
}