using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
namespace v1_Apostle_s_War.Skills.Ativas
{
    class Furtividade : HabilidadeAtiva
    {
        public Furtividade() : base("Furtividade", "🕳️", 4,
            "Intocável por 2 turnos. Ao expirar, ataca todos os inimigos com 100% ATK.")
        { }
        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            AplicarBuff(atacante, new Intocavel(turnos: 2));

            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, lista))
                resultados.Add(AplicarDano(atacante, a, 1.0));
            return resultados;
        }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
    }
}