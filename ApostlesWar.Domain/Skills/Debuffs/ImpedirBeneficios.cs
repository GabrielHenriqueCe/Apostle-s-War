using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Debuffs
{
    /// <summary>
    /// Enquanto ativo, o portador não pode receber novos Buffs.
    /// Verificado no porteiro de status (Combate.PodeReceber).
    /// </summary>
    public class ImpedirBeneficios : Debuff, IBloqueiaStatus
    {
        public ImpedirBeneficios(int duracao = 2)
            : base("Impedir Benefícios", "🚫", duracao, 0, "Bloqueia novos benefícios.") { }

        public bool Bloqueia(StatusEffect novo) => novo is Buff;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
