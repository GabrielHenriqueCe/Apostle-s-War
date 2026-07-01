using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca 6 vezes inimigos escolhidos aleatoriamente (com repetição) com +50% ATK cada.
    /// Cada hit cura o Troll em 30% do dano causado.
    /// </summary>
    class Porradeiro : HabilidadeAtiva
    {
        private const int NumeroDeHits = 6;
        private const double MultiplicadorAtaque = 1.5;
        private const double CuraPorHit = 0.30;

        private static readonly Random _random = new Random();

        public Porradeiro() : base("Porradeiro", "🥊", 4,
            "6 ataques aleatórios +50% ATK. Cura 30% do dano causado.")
        { }

        public override int NumeroDeAlvos => 1;  // alvo inicial é só pra a UI
        public override TipoAlvo TipoAlvo => TipoAlvo.Aleatorio;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<EventoDano>();

            for (int i = 0; i < NumeroDeHits; i++)
            {
                var vivos = ctx.Inimigos.Where(c => c.EstaVivo()).ToList();
                if (vivos.Count == 0) break;

                var sorteado = vivos[_random.Next(vivos.Count)];
                var r = ctx.Atacante.Atacar(sorteado, MultiplicadorAtaque);
                resultados.Add(r);

                if (r.DanoEfetivo > 0)
                    ctx.Atacante.Curar((int)(r.DanoEfetivo * CuraPorHit));
            }

            return resultados;
        }
    }
}