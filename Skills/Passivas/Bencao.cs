using ApostlesWar;

namespace ApostlesWar.Skills.Passivas
{
    /// <summary>
    /// Recupera 5% HP no início do turno, permanentemente. Capacidade direta
    /// (IReageAoInicioTurno) — não usa mais buff de contorno (CuraContinua).
    /// </summary>
    class Bencao : HabilidadePassiva, IReageAoInicioTurno
    {
        private const double PercentualCura = 0.05;

        public Bencao() : base("Bênção", "😇", 0,
            "Recupera 5% HP por turno permanentemente.")
        { }

        public List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx)
        {
            int cura = (int)(ctx.Atacante.HPMaximo * PercentualCura);
            ctx.Atacante.Curar(cura);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Atacante.Personagem.Nome} recupera {cura} HP com Bênção! 😇",
                    Cura: cura
                )
            };
        }

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}
