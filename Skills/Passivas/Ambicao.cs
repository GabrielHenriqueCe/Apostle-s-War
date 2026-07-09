using ApostlesWar;

namespace ApostlesWar.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, aumenta o próprio ATK em 5% (sobre AtaqueComItens) até um
    /// cap de 25% acumulado. Estado interno guarda o total já ganho. Migrada para
    /// o modelo de reação (IReageAoSerAtacado). O bônus vai no PORTADOR (si mesmo).
    /// </summary>
    class Ambicao : HabilidadePassiva, IReageAoSerAtacado
    {
        private const double AumentoPorHit = 0.05;
        private const double Cap = 0.25;

        private class Estado
        {
            public double TotalAumentado;
        }

        public Ambicao() : base("Ambição", "🧌", 0,
            "Ao ser atacado, aumenta o próprio ATK em 5% até 25%.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            var estado = ObterEstado<Estado>(ctx.Portador);
            if (estado.TotalAumentado >= Cap) return new List<ResultadoReacao>();

            double aumentar = Math.Min(AumentoPorHit, Cap - estado.TotalAumentado);
            int delta = (int)(ctx.Portador.AtaqueComItens * aumentar);
            if (delta <= 0) return new List<ResultadoReacao>();

            ctx.Portador.AdicionarBonusAtaquePermanente(delta);
            estado.TotalAumentado += aumentar;

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"🧌 A Ambição aumentou o ataque de {ctx.Portador.Personagem.Nome}!"
                )
            };
        }
    }
}