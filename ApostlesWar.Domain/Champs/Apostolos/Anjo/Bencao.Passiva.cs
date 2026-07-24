using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Champs.Apostolos
{
    /// <summary>
    /// Recupera 5% HP no início do turno, permanentemente. Capacidade direta
    /// (IReageAoInicioTurno) — não usa mais buff de contorno (CuraContinua).
    /// </summary>
    public class Bencao : HabilidadePassiva, IReageAoInicioTurno
    {
        private const double PercentualCura = 0.05;

        public Bencao() : base("Bênção", "😇", 0,
            "Recupera 5% HP por turno permanentemente.")
        { }

        public List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx)
        {
            int cura = ctx.Atacante.Curar((int)(ctx.Atacante.HPMaximo * PercentualCura));   // retorno = cura real

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Cura: new EventoCura(ctx.Atacante, ctx.Atacante, cura, ctx.Atacante.HPAtual))
            };
        }

        public override List<EventoCombate> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}
