using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;
namespace v1_Apostle_s_War.Skills.Ativas
{
    class Prender : HabilidadeAtiva
    {
        public Prender() : base("Prender", "⛓️", 4, "Inimigo pula os próximos 2 turnos.") { }
        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;
        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                AplicarDebuff(a, new Preso(turnos: 2));
            return SemDano();
        }
    }
}