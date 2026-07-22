using ApostlesWar;
using ApostlesWar.Skills;

namespace ApostlesWar.Champs.LadoSombrio
{
    /// <summary>
    /// Bloqueia a aplicação de qualquer Debuff no portador. Capacidade direta
    /// (IBloqueiaStatus) — não usa mais buff de contorno (ImunidadeDebuffs).
    /// </summary>
    class CascaDura : HabilidadePassiva, IBloqueiaStatus
    {
        public CascaDura() : base("Casca Dura", "🎃", 0,
            "Imune a maleficios.")
        { }

        public bool Bloqueia(StatusEffect novo) => novo is Debuff;

        public override List<EventoCombate> Ativar(ContextoCombate ctx, Combate alvo)
            => SemDano();
    }
}
