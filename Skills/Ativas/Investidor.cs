using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class Investidor : HabilidadeAtiva
    {
        public Investidor() : base("Investidor", "📈", 3,
            "Ataca todos os inimigos com +25% ATK e aplica Provocar (1t).")
        { }
        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, lista))
            {
                resultados.Add(AplicarDano(atacante, a, 1.25));
                AplicarBuff(a, new Provocar(turnos: 1));
            }
            return resultados;
        }
    }
}