using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca 2 inimigos sorteados aleatoriamente (com repetição) com +200% ATK cada.
    /// </summary>
    class Vilania : HabilidadeAtiva
    {
        private const int NumeroDeHits = 2;
        private const double MultiplicadorAtaque = 3.0;

        private static readonly Random _random = new Random();

        public Vilania() : base("Vilania", "👿", 4,
            "2 ataques aleatórios +200% ATK.")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Aleatorio;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<ResultadoAtaque>();

            for (int i = 0; i < NumeroDeHits; i++)
            {
                var vivos = ctx.Inimigos.Where(c => c.EstaVivo()).ToList();
                if (vivos.Count == 0) break;

                var sorteado = vivos[_random.Next(vivos.Count)];
                resultados.Add(ctx.Atacante.AtacarComMultiplicador(sorteado, MultiplicadorAtaque));
            }

            return resultados;
        }
    }
}