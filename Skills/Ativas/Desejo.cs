using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica BuffDefesa 30% 2t em todos os aliados.
    /// Aplica Maldição 2 stacks (= 2t) em todos os inimigos.
    /// Maldição reduz 10% HP máximo por turno, sem dano, cap 50%.
    /// </summary>
    class Desejo : HabilidadeAtiva
    {
        public Desejo() : base("Desejo", "🪔", 3,
            "+30% DEF nos aliados (2t) e Maldição nos inimigos (2t).")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        // TipoLista.Aliados porque o foco principal é o buff defensivo
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            // Buff em todos os aliados
            foreach (Combate a in ResolverAlvos(alvo, ctx.Aliados))
                new BuffDefesa(turnos: 2, percentual: 0.30).Aplicar(a);

            // Maldição em todos os inimigos
            foreach (Combate i in ctx.Inimigos.Where(c => c.EstaVivo()))
                new Maldicao(stacks: 2).Aplicar(i);

            return SemDano();
        }
    }
}