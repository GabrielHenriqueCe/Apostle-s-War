using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Bloqueia a aplicação de qualquer Debuff no portador.
    /// Aplicado pela passiva da Abóbora — é permanente (turnos altos suficiente).
    /// </summary>
    class ImunidadeDebuffs : Buff
    {
        public ImunidadeDebuffs() : base("Imunidade", "🎃", int.MaxValue, 0,
            "Imune a maleficios.")
        { }

        public override bool Bloqueia(StatusEffect novo) => novo is Debuff;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
