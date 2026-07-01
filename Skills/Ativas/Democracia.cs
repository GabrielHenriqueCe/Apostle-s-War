using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class Democracia : HabilidadeAtiva
    {
        public Democracia() : base("Democracia", "🗳️", 3,
            "Cura todos os aliados em 30% do HP máximo.")
        { }
        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                AplicarCura(a, 0.30);
            return SemDano();
        }
    }
}