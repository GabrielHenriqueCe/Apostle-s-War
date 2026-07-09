using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
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

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
            => SemDano();
    }
}
