using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Cada ataque reduz a DEF do alvo em 5% (sobre DefesaComItens), até -25% no alvo.
    /// A redução mora no alvo (cap compartilhado). Migrada para IReagePorAtaque (por alvo atingido).
    /// </summary>
    class Sorrateiro : HabilidadePassiva, IReagePorAtaque
    {
        private const double ReducaoPorHit = 0.05;
        private const double Cap = 0.25;

        public Sorrateiro() : base("Sorrateiro", "👁️", 0,
            "Cada ataque reduz a DEF do inimigo em 5%, até 25%.")
        { }

        public List<ResultadoReacao> PorAtaque(ContextoReacao ctx)
        {
            int reducaoMaxima = (int)(ctx.Outro.DefesaComItens * Cap);
            if (ctx.Outro.ReducaoDefesaPermanente >= reducaoMaxima) return new List<ResultadoReacao>();

            int incremento = (int)(ctx.Outro.DefesaComItens * ReducaoPorHit);
            int aplicar = Math.Min(incremento, reducaoMaxima - ctx.Outro.ReducaoDefesaPermanente);
            if (aplicar <= 0) return new List<ResultadoReacao>();

            ctx.Outro.AdicionarReducaoDefesaPermanente(aplicar);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Mensagem: $"👁️ O Sorrateiro corroeu a defesa de {ctx.Outro.Personagem.Nome}!")
            };
        }
    }
}