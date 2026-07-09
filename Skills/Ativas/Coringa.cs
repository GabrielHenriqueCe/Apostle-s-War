using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Remove todos os debuffs ativos dos aliados e aplica ImunidadeDebuffs 2t em todos.
    /// </summary>
    class Coringa : HabilidadeAtiva
    {
        public Coringa() : base("Coringa", "🃏", 3,
            "Remove malefícios dos aliados e dá imunidade por 2 turnos.")
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
                var debuffs = a.StatusAtivos.OfType<Debuff>().ToList();
                foreach (var d in debuffs)
                    d.Remover(a);

                new ImunidadeDebuffs(turnos: 2).Aplicar(a);
            }
            return SemDano();
        }
    }
}