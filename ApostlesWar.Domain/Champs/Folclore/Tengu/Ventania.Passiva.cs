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
            "Todo turno: +50% do ATK por 2 turnos.")
        { }

        public List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx)
        {
            new BuffAtaque(duracao: 2, percentual: 0.50).Aplicar(ctx.Atacante);
            return new List<ResultadoReacao>();
        }
    }
}
