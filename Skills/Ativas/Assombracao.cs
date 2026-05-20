using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos com 100% ATK. Cura o Fantasma em 20% do dano causado em cada hit.
    /// </summary>
    class Assombracao : HabilidadeAtiva
    {
        public Assombracao() : base("Assombração", "👻", 3,
            "Ataca todos. Cura 20% do dano causado em cada inimigo.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                var r = AplicarDano(ctx.Atacante, a, 1.0);
                resultados.Add(r);
                ctx.Atacante.Curar((int)(r.Dano * 0.20));
            }
            return resultados;
        }
    }
}