using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Todo início de turno, aplica BuffAtaque 25% por 2 turnos (renova sempre).
    /// </summary>
    class PassivaTengu : HabilidadePassiva, IReageAoInicioTurno
    {
        public PassivaTengu() : base("Ventania", "👺", 0,
            "Todo turno: +25% ATK por 2 turnos.")
        { }

        public List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx)
        {
            new BuffAtaque(turnos: 2, percentual: 0.25).Aplicar(ctx.Atacante);
            return new List<ResultadoReacao>();
        }
    }
}