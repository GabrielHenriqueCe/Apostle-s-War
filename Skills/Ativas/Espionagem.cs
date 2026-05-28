using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;
namespace v1_Apostle_s_War.Skills.Ativas
{
    class Espionagem : HabilidadeAtiva
    {
        public Espionagem() : base("Espionagem", "🔎", 4,
            "-30% DEF em todos os inimigos por 2 turnos.")
        { }
        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;
        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                AplicarDebuff(a, new ReducaoDefesa(turnos: 2));
            return SemDano();
        }
    }
}