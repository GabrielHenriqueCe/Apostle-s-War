using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica Sangramento (2 stacks = 2 turnos) em todos os inimigos e ataca com +100% ATK.
    /// Como o Sangramento é aplicado ANTES do ataque, a própria a2 já cura 15% do dano causado.
    /// </summary>
    class Mordida : HabilidadeAtiva
    {
        public Mordida() : base("Mordida", "🦇", 3,
            "Aplica Sangramento (2t) e ataca todos com +100% ATK.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<ResultadoAtaque>();

            // Aplica Sangramento primeiro, depois ataca — assim já cura no próprio ataque
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                new Sangramento(stacks: 2).Aplicar(a);

            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, a, 2.0));

            return resultados;
        }
    }
}