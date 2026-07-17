using ApostlesWar;

namespace ApostlesWar.Skills.Buffs
{
    /// <summary>
    /// O portador pode receber dano normalmente, mas HP nunca vai abaixo de 1.
    /// </summary>
    class Invencivel : Buff, IModificaDanoRecebido
    {
        public Invencivel(int turnos = 1) : base("Invencível", "⚜️", turnos, 0,
            "Não pode morrer. HP mínimo de 1.")
        { }

        public int ModificarDanoRecebido(Combate portador, int dano)
        {
            // Limita dano para que HP não vá abaixo de 1
            int danoMaximo = Math.Max(0, portador.HPAtual - 1);
            return Math.Min(dano, danoMaximo);
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
