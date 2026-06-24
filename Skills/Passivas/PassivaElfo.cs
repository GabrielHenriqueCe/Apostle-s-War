using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Todo início de turno, aplica EspinhosVenenosos 2t (renova sempre). Quem ataca
    /// o Elfo recebe Veneno + Queima enquanto ativo.
    /// </summary>
    class PassivaElfo : HabilidadePassiva, IReageAoInicioTurno
    {
        public PassivaElfo() : base("Espinhos", "🌿", 0,
            "Todo turno: atacantes recebem Veneno e Queima (2t).")
        { }

        public List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx)
        {
            new EspinhosVenenosos(turnos: 2).Aplicar(ctx.Atacante);
            return new List<ResultadoReacao>();
        }
    }
}