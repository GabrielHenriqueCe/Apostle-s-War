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
        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            foreach (Combate a in ResolverAlvos(alvo, lista))
                AplicarDebuff(a, new Preso(turnos: 2));
            return SemDano();
        }
    }
}