using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos com 100% ATK e aplica Queima 2t.
    /// </summary>
    class Fisica : HabilidadeAtiva
    {
        public Fisica() : base("Física", "⚛️", 3,
            "Ataca todos e aplica Queima 2t.")
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
                new Queima(turnos: 2).Aplicar(a);
            }
            return resultados;
        }
    }
}