using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class Protetor : HabilidadeAtiva
    {
        public Protetor() : base("Protetor", "🛡️", 4,
            "Aplica Provocar (2t) e Bloqueio Total (1t) em si mesmo.")
        { }
        public override int NumeroDeAlvos => 0;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Self;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            AplicarBuff(atacante, new Provocar(turnos: 2));
            AplicarBuff(atacante, new BloqueioTotal(turnos: 1));
            return SemDano();
        }
    }
}
