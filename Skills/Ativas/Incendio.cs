using ApostlesWar;
using v1_Apostle_s_War.Skills.Passivas;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class Incendio : HabilidadeAtiva
    {
        public Incendio() : base("Incêndio", "🌋", 4,
            "Ataca todos os inimigos com +50% ATK.")
        { }
        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, lista))
            {
                double mult = 1.5 * PassivaPiromancer.MultExtra(atacante, a);
                resultados.Add(AplicarDano(atacante, a, mult));
            }
            return resultados;
        }
    }
}