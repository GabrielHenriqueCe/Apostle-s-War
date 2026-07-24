using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Buffs
{
    /// <summary>
    /// Bloqueia a aplicação de qualquer Debuff no portador.
    /// Pode ser permanente (CascaDura aplica com int.MaxValue) ou temporária
    /// (habilidades como Coringa do Palhaço e Canto de Sereia aplicam com turnos limitados).
    /// </summary>
    public class ImunidadeDebuffs : Buff, IBloqueiaStatus
    {
        public ImunidadeDebuffs(int duracao = int.MaxValue)
            : base("Imunidade", "🎃", duracao, 0, "Imune a maleficios.")
        { }

        public bool Bloqueia(StatusEffect novo) => novo is Debuff;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}