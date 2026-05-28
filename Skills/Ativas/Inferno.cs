using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica 2 stacks de Queima em todos os inimigos e imediatamente explode a Queima.
    /// Não causa ataque básico — só queima e explosão.
    /// </summary>
    class Inferno : HabilidadeAtiva
    {
        public Inferno() : base("Inferno", "🔥", 3,
            "Aplica 2 stacks de Queima em todos os inimigos e explode imediatamente.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                new Queima(stacks: 2).Aplicar(a);

                var queima = a.StatusAtivos.OfType<Queima>().FirstOrDefault();
                queima?.Explodir(a);
            }
            return SemDano();
        }
    }
}