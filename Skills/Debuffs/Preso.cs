using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Debuffs
{
    /// <summary>
    /// O alvo pula seus próximos turnos enquanto este debuff estiver ativo.
    /// O CombateService deve checar este status antes de processar o turno do combatente.
    /// </summary>
    class Preso : Debuff
    {
        public Preso(int turnos = 2) : base("Preso", "⛓️", turnos, 0, "Pula os próximos turnos.") { }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
