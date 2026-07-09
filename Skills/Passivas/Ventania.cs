using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Skills.Passivas
{
    /// <summary>
    /// Todo início de turno, aplica BuffAtaque 25% por 2 turnos (renova sempre).
    /// </summary>
    class Ventania : HabilidadePassiva, IReageAoInicioTurno
    {
        public Ventania() : base("Ventania", "👺", 0,
            "Todo turno: +25% ATK por 2 turnos.")
        { }

        public List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx)
        {
            new BuffAtaque(turnos: 2, percentual: 0.25).Aplicar(ctx.Atacante);
            return new List<ResultadoReacao>();
        }
    }
}