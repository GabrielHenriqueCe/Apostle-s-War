using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Bloqueia a aplicação de qualquer Debuff no portador.
    /// Pode ser permanente (PassivaAbobora aplica com int.MaxValue) ou temporária
    /// (habilidades como Coringa do Palhaço e Canto de Sereia aplicam com turnos limitados).
    /// </summary>
    class ImunidadeDebuffs : Buff, IBloqueiaStatus
    {
        public ImunidadeDebuffs(int turnos = int.MaxValue)
            : base("Imunidade", "🎃", turnos, 0, "Imune a maleficios.")
        { }

        public bool Bloqueia(StatusEffect novo) => novo is Debuff;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}