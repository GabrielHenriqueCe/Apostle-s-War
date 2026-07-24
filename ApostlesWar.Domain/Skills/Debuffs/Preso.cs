using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Debuffs
{
    /// <summary>
    /// O alvo pula seus próximos turnos enquanto este debuff estiver ativo.
    /// Carrega a capacidade IPulaTurno: o turno pergunta a capacidade, não o tipo concreto.
    /// É o 1º membro da família de controle-de-turno (Congelar/Stun/… plugam via IPulaTurno).
    /// </summary>
    public class Preso : Debuff, IPulaTurno
    {
        public Preso(int duracao = 2) : base("Preso", "⛓️", duracao, 0, "Pula os próximos turnos.") { }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
