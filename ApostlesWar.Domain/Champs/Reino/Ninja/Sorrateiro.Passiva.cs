using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Champs.Reino
{
    /// <summary>
    /// Cada ataque reduz a DEF do alvo em 5% (sobre DefesaComItens), até -25% no alvo.
    /// A redução mora no alvo (cap compartilhado). Migrada para IReagePorAtaque (por alvo atingido).
    /// </summary>
    public class Sorrateiro : HabilidadePassiva, IReagePorAtaque
    {
        private const double ReducaoPorHit = 0.05;
        private const double Cap = 0.25;

        public Sorrateiro() : base("Sorrateiro", "👁️", 0,
            "Cada ataque reduz a DEF do inimigo em 5%, até 25%.")
        { }

        public List<ResultadoReacao> PorAtaque(ContextoReacao ctx)
        {
            int reducaoMaxima = (int)(ctx.Contraparte.DefesaComItens * Cap);
            if (ctx.Contraparte.ReducaoDefesaPermanente >= reducaoMaxima) return new List<ResultadoReacao>();

            int incremento = (int)(ctx.Contraparte.DefesaComItens * ReducaoPorHit);
            int aplicar = Math.Min(incremento, reducaoMaxima - ctx.Contraparte.ReducaoDefesaPermanente);
            if (aplicar <= 0) return new List<ResultadoReacao>();

            ctx.Contraparte.AdicionarReducaoDefesaPermanente(aplicar);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Mensagem: $"👁️ O Sorrateiro corroeu a defesa de {ctx.Contraparte.Personagem.Nome}!")
            };
        }
    }
}
