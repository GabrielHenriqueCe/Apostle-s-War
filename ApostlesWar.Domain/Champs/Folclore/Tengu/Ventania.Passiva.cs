using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;

namespace ApostlesWar.Domain.Champs.Folclore
{
    /// <summary>
    /// Todo início de turno, aplica BuffAtaque 25% por 2 turnos (renova sempre).
    /// </summary>
    public class Ventania : HabilidadePassiva, IReageAoInicioTurno
    {
        public Ventania() : base("Ventania", "👺", 0,
            "Todo turno: +25% ATK por 2 turnos.")
        { }

        public List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx)
        {
            new BuffAtaque(duracao: 2, percentual: 0.25).Aplicar(ctx.Atacante);
            return new List<ResultadoReacao>();
        }
    }
}
