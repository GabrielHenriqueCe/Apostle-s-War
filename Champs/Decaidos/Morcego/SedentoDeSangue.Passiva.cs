using ApostlesWar;

namespace ApostlesWar.Champs.Decaidos
{
    /// <summary>
    /// Cura 15% do dano causado ao atacar. Capacidade direta (IReageAoCausarDano)
    /// — não usa mais buff de contorno (Sedento).
    /// </summary>
    class SedentoDeSangue : HabilidadePassiva, IReageAoCausarDano
    {
        private const double PercentualCura = 0.15;

        public SedentoDeSangue() : base("Sedento de Sangue", "🦇", 0,
            "Cura 15% do dano causado ao atacar.")
        { }

        public List<ResultadoReacao> AoCausarDano(ContextoReacao ctx)
        {
            if (ctx.DanoCausado <= 0)
                return new List<ResultadoReacao>();

            int cura = ctx.Portador.Curar((int)(ctx.DanoCausado * PercentualCura));   // retorno = cura real

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Cura: new EventoCura(ctx.Portador, ctx.Portador, cura, ctx.Portador.HPAtual))
            };
        }

        public override List<EventoCombate> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}
