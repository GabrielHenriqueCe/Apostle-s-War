using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Bloqueia 100% do dano recebido enquanto ativo.
    /// </summary>
    class BloqueioTotal : Buff, IModificaDanoRecebido
    {
        public BloqueioTotal(int turnos = 1) : base("Bloqueio Total", "🧱", turnos, 1,
            "Bloqueia todo o dano recebido.")
        { }

        public int ModificarDanoRecebido(Combate portador, int dano) => 0;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
