using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class Democracia : HabilidadeAtiva
    {
        public Democracia() : base("Democracia", "🗳️", 3,
            "Cura todos os aliados em 30% do HP máximo.")
        { }
        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            foreach (Combate a in ResolverAlvos(alvo, lista))
                AplicarCura(a, 0.30);
            return SemDano();
        }
    }
}