using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Aplica BuffAtaque 25% 2t em todos os aliados e ataca todos os inimigos com +100% ATK.
    /// </summary>
    class Super : HabilidadeAtiva
    {
        public Super() : base("Super", "💪", 3,
            "+25% ATK aos aliados e ataca todos +100% ATK.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            // Buff em todos os aliados
            foreach (Combate a in ctx.Aliados.Where(c => c.EstaVivo()))
                new BuffAtaque(turnos: 2, percentual: 0.25).Aplicar(a);

            // Ataca todos os inimigos
            var resultados = new List<EventoDano>();
            foreach (Combate i in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, i, 2.0));

            return resultados;
        }
    }
}