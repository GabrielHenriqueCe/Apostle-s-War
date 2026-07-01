using ApostlesWar;
namespace v1_Apostle_s_War.Skills.Ativas
{
    class Tiroteio : HabilidadeAtiva
    {
        public Tiroteio() : base("Tiroteio", "🔫", 4,
            "Ataca 2 inimigos aleatórios com 75% ATK. Pode acertar o mesmo alvo duas vezes.")
        { }
        public override int NumeroDeAlvos => 2;
        public override TipoAlvo TipoAlvo => TipoAlvo.Aleatorio;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, a, 0.75));
            return resultados;
        }
    }
}