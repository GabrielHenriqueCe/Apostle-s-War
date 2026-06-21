using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
namespace v1_Apostle_s_War.Skills.Ativas
{
    class ParedeDeTijolos : HabilidadeAtiva
    {
        public ParedeDeTijolos() : base("Parede de Tijolos", "🧱", 6,
            "Bloqueia 100% do dano de todos os aliados por 1 turno.")
        { }
        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;
        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                AplicarBuff(a, new BloqueioTotal());
            return SemDano();
        }
    }
}