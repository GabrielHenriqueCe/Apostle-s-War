using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos com 100% ATK e aplica 1 stack de Veneno.
    /// </summary>
    class Fedorento : HabilidadeAtiva
    {
        public Fedorento() : base("Fedorento", "🤢", 4,
            "Ataca todos e aplica Veneno (5% HP/turno por stack).")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, lista))
            {
                resultados.Add(AplicarDano(atacante, a, 1.0));
                new Veneno(stacks: 1).Aplicar(a);
            }
            return resultados;
        }
    }
}
