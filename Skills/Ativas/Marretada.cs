using ApostlesWar;
namespace v1_Apostle_s_War.Skills.Ativas
{
    class Marretada : HabilidadeAtiva
    {
        public Marretada() : base("Marretada", "🔨", 3, "Causa 125% do ATK em 1 inimigo.") { }
        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, a, 1.25));
            return resultados;
        }
    }
}