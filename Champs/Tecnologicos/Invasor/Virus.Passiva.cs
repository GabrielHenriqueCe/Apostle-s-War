using ApostlesWar;

namespace ApostlesWar.Champs.Tecnologicos
{
    /// <summary>
    /// Ao atacar, ganha +5% de Dano Crítico (cap 25% acumulado). Migrada para IReageAoAtacar
    /// (segue TipoAtaque). Ganho no PRÓPRIO atacante.
    /// </summary>
    class Virus : HabilidadePassiva, IReageAoAtacar
    {
        private const double AumentoPorHit = 0.05;
        private const double Cap = 0.25;

        private class Estado { public double TotalAumentado; }

        public Virus() : base("Vírus", "👾", 0,
            "A cada ataque, +5% de Dano Crítico, até 25%.")
        { }

        public List<ResultadoReacao> AoAtacar(ContextoReacao ctx)
        {
            var estado = ObterEstado<Estado>(ctx.Portador);
            if (estado.TotalAumentado >= Cap) return new List<ResultadoReacao>();

            double aumentar = Math.Min(AumentoPorHit, Cap - estado.TotalAumentado);
            ctx.Portador.AdicionarBonusDanoCritPermanente(aumentar);
            estado.TotalAumentado += aumentar;

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Mensagem: $"👾 O Vírus intensificou o dano crítico de {ctx.Portador.Personagem.Nome}!")
            };
        }
    }
}
