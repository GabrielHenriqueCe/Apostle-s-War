using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca 1 inimigo com +200% ATK ignorando 50% da DEF do alvo.
    /// </summary>
    class BatMan : HabilidadeAtiva
    {
        public BatMan() : base("Bat Man", "🦇", 3,
            "+200% ATK ignorando 50% DEF.")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                var r = ctx.Atacante.Atacar(
                    a,
                    multiplicador: 3.0,
                    ignorarDefesaPct: 0.50);
                resultados.Add(r);
            }
            return resultados;
        }
    }
}