using ApostlesWar;

namespace ApostlesWar.Skills.Passivas
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

            int cura = (int)(ctx.DanoCausado * PercentualCura);
            ctx.Portador.Curar(cura);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Portador.Personagem.Nome} se cura em {cura} com Sedento de Sangue! 🦇",
                    Cura: cura
                )
            };
        }

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}
