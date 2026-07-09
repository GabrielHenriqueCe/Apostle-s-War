using ApostlesWar;

namespace ApostlesWar.Champs.Humanos
{
    /// <summary>
    /// Ao atacar, ganha +5% de TaxaCrit (cap 25% acumulado). Migrada para IReageAoAtacar
    /// (segue TipoAtaque: 1x em AoE, por hit em Sequencial). Ganho no PRÓPRIO atacante.
    /// </summary>
    class OlhoClinico : HabilidadePassiva, IReageAoAtacar
    {
        private const double AumentoPorHit = 0.05;
        private const double Cap = 0.25;

        private class Estado { public double TotalAumentado; }

        public OlhoClinico() : base("Olho Clínico", "🚬", 0,
            "A cada ataque, ganha +5% de TaxaCrit, até 25%.")
        { }

        public List<ResultadoReacao> AoAtacar(ContextoReacao ctx)
        {
            var estado = ObterEstado<Estado>(ctx.Portador);
            if (estado.TotalAumentado >= Cap) return new List<ResultadoReacao>();

            double aumentar = Math.Min(AumentoPorHit, Cap - estado.TotalAumentado);
            ctx.Portador.AdicionarBonusTaxaCritPermanente(aumentar);
            estado.TotalAumentado += aumentar;

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Mensagem: $"🚬 Olho Clínico afiou a mira de {ctx.Portador.Personagem.Nome}!")
            };
        }
    }
}