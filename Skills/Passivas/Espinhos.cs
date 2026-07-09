using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Skills.Passivas
{
    /// <summary>
    /// Todo início de turno, aplica EspinhosVenenosos 2t (renova sempre). Quem ataca
    /// o Elfo recebe Veneno + Queima enquanto ativo.
    /// </summary>
    class Espinhos : HabilidadePassiva, IReageAoInicioTurno
    {
        public Espinhos() : base("Espinhos", "🌿", 0,
            "Todo turno: atacantes recebem Veneno e Queima (2t).")
        { }

        public List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx)
        {
            new EspinhosVenenosos(turnos: 2).Aplicar(ctx.Atacante);
            return new List<ResultadoReacao>();
        }
    }
}