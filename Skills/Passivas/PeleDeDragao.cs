using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Skills.Passivas
{
    /// <summary>
    /// Imune a Veneno e Queima durante todo o combate. Capacidade direta
    /// (IBloqueiaStatus) — a passiva É o bloqueio, sem buff de contorno.
    /// </summary>
    class PeleDeDragao : HabilidadePassiva, IBloqueiaStatus
    {
        public PeleDeDragao() : base("Pele de Dragão", "🐉", 0,
            "Imune a Veneno e Queima.")
        { }

        public bool Bloqueia(StatusEffect novo) => novo is Veneno || novo is Queima;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}
