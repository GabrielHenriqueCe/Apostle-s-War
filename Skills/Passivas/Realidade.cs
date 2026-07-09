using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Skills.Passivas
{
    /// <summary>
    /// Todo início de turno, aplica RefletirDano 2t em si mesmo (renova sempre).
    /// Se ImpedirBeneficios estiver ativo, falha silenciosamente.
    /// </summary>
    class Realidade : HabilidadePassiva, IReageAoInicioTurno
    {
        public Realidade() : base("Realidade", "🔮", 0,
            "Todo turno aplica Refletir Dano em si mesmo.")
        { }

        public List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx)
        {
            new RefletirDano(turnos: 2).Aplicar(ctx.Atacante);
            return new List<ResultadoReacao>();
        }
    }
}