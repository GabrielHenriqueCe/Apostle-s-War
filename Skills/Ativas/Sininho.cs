using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca 1 inimigo com +200% ATK (multiplicador 3.0).
    /// </summary>
    class Sininho : HabilidadeAtiva
    {
        public Sininho() : base("Sininho", "🔔", 3,
            "Ataca 1 inimigo com +200% ATK.")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, a, 3.0));
            return resultados;
        }
    }
}