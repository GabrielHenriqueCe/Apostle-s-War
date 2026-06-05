using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Debuffs
{
    /// <summary>
    /// Enquanto ativo, o portador não pode receber novos Buffs.
    /// Verificado no porteiro de status (Combate.PodeReceber).
    /// </summary>
    class ImpedirBeneficios : Debuff, IBloqueiaStatus
    {
        public ImpedirBeneficios(int turnos = 2)
            : base("Impedir Benefícios", "🚫", turnos, 0, "Bloqueia novos benefícios.") { }

        public bool Bloqueia(StatusEffect novo) => novo is Buff;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
