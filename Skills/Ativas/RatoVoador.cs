using ApostlesWar;
using ApostlesWar.Skills.Buffs;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Aplica Medo 1t em todos os inimigos, BuffAtaque 2t e BuffTaxaCrit 25% 2t em si mesmo,
    /// e concede um turno extra (jogar de novo).
    /// </summary>
    class RatoVoador : HabilidadeAtiva
    {
        public RatoVoador() : base("Rato Voador", "🐀", 4,
            "Medo 1t em todos os inimigos, +25% ATK e +25% Crit em si (2t) e ganha turno extra.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            // Medo em todos os inimigos
            foreach (Combate i in ctx.Inimigos.Where(c => c.EstaVivo()))
                new Medo(turnos: 1).Aplicar(i);

            // Buffs em si mesmo
            new BuffAtaque(turnos: 2, percentual: 0.25).Aplicar(ctx.Atacante);
            new BuffTaxaCrit(turnos: 2, valor: 0.25).Aplicar(ctx.Atacante);

            // Turno extra
            ctx.Atacante.ConcederTurnoExtra();

            return SemDano();
        }
    }
}