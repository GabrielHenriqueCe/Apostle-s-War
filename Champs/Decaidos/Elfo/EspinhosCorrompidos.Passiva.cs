using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Decaidos
{
    /// <summary>
    /// Todo início de turno, aplica EspinhosVenenosos 2t (renova sempre). Quem ataca
    /// o Elfo recebe Veneno + Queima enquanto ativo. Nome de jogo "Espinhos Corrompidos"
    /// (era "Espinhos", que colidia no display com o buff EspinhosVenenosos — que exibe "Espinhos").
    /// </summary>
    class EspinhosCorrompidos : HabilidadePassiva, IReageAoInicioTurno
    {
        public EspinhosCorrompidos() : base("Espinhos Corrompidos", "🌿", 0,
            "Todo turno: atacantes recebem Veneno e Queima (2t).")
        { }

        public List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx)
        {
            new EspinhosVenenosos(turnos: 2).Aplicar(ctx.Atacante);
            return new List<ResultadoReacao>();
        }
    }
}