using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Champs.Reino
{
    /// <summary>
    /// Ao ser atacado, aumenta a própria DEF em 5% (sobre DefesaComItens) até um
    /// cap de 25% acumulado. Estado interno guarda o total já ganho. Migrada para
    /// o modelo de reação (IReageAoSerAtacado). O bônus vai no PORTADOR (si mesmo).
    /// </summary>
    public class CoroaDoSoberano : HabilidadePassiva, IReageAoSerAtacado
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

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            var estado = ObterEstado<Estado>(ctx.Portador);
            if (estado.TotalAumentado >= Cap) return new List<ResultadoReacao>();

            double aumentar = Math.Min(AumentoPorHit, Cap - estado.TotalAumentado);
            int delta = (int)(ctx.Portador.DefesaComItens * aumentar);
            if (delta <= 0) return new List<ResultadoReacao>();

            ctx.Portador.AdicionarBonusDefesaPermanente(delta);
            estado.TotalAumentado += aumentar;

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"👑 A Coroa do Soberano fortaleceu a defesa de {ctx.Portador.Personagem.Nome}!"
                )
            };
        }
    }
}
