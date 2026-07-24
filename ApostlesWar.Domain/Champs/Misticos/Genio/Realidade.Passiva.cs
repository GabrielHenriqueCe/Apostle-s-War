using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;

namespace ApostlesWar.Domain.Champs.Misticos
{
    /// <summary>
    /// Todo início de turno, aplica RefletirDano 2t em si mesmo (renova sempre).
    /// Se ImpedirBeneficios estiver ativo, falha silenciosamente.
    /// </summary>
    public class Realidade : HabilidadePassiva, IReageAoInicioTurno
    {
        public Realidade() : base("Realidade", "🔮", 0,
            "Todo turno aplica Refletir Dano em si mesmo.")
        { }

        public List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx)
        {
            new RefletirDano(duracao: 2).Aplicar(ctx.Atacante);
            return new List<ResultadoReacao>();
        }
    }
}
