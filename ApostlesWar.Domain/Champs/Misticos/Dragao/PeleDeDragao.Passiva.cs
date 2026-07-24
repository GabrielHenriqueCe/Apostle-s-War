using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Misticos
{
    /// <summary>
    /// Imune a Veneno e Queima durante todo o combate. Capacidade direta
    /// (IBloqueiaStatus) — a passiva É o bloqueio, sem buff de contorno.
    /// </summary>
    public class PeleDeDragao : HabilidadePassiva, IBloqueiaStatus
    {
        public PeleDeDragao() : base("Pele de Dragão", "🐉", 0,
            "Imune a Veneno e Queima.")
        { }

        public bool Bloqueia(StatusEffect novo) => novo is Veneno || novo is Queima;

        public override List<EventoCombate> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}
