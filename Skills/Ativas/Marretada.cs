using ApostlesWar;
namespace v1_Apostle_s_War.Skills.Ativas
{
    class Marretada : HabilidadeAtiva
    {
        public Marretada() : base("Marretada", "🔨", 3, "Causa 125% do ATK em 1 inimigo.") { }
        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, lista))
                resultados.Add(AplicarDano(atacante, a, 1.25));
            return resultados;
        }
    }
}