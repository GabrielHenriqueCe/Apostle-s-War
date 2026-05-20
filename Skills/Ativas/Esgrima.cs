using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class Esgrima : HabilidadeAtiva
    {
        public Esgrima() : base("Esgrima", "🤺", 3,
            "Ataca 2 inimigos aleatórios com +50% ATK.")
        { }
        public override int NumeroDeAlvos => 2;
        public override TipoAlvo TipoAlvo => TipoAlvo.Aleatorio;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, a, 1.5));
            return resultados;
        }
    }
}