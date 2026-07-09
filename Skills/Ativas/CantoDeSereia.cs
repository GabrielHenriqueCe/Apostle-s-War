using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Aplica ImunidadeDebuffs (2t) e BuffAtaque +25% (2t) em todos os aliados.
    /// </summary>
    class CantoDeSereia : HabilidadeAtiva
    {
        public CantoDeSereia() : base("Canto de Sereia", "🧜‍♀️", 4,
            "Imunidade a malefícios e +25% ATK em todos os aliados (2t).")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                new ImunidadeDebuffs(turnos: 2).Aplicar(a);
                new BuffAtaque(turnos: 2, percentual: 0.25).Aplicar(a);
            }
            return SemDano();
        }
    }
}