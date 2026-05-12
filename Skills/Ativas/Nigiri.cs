using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
namespace v1_Apostle_s_War.Skills.Ativas
{
    class Nigiri : HabilidadeAtiva
    {
        public Nigiri() : base("Nigiri", "🍙", 4,
            "Revive todos os aliados mortos + +25% ATK em todos por 2 turnos.")
        { }
        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            foreach (Combate aliado in lista.Where(a => !a.EstaVivo()))
                aliado.Reviver(1);
            foreach (Combate a in ResolverAlvos(alvo, lista))
                AplicarBuff(a, new BuffAtaque(turnos: 2, percentual: 0.25));
            return SemDano();
        }
    }
}