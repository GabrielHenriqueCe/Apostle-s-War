using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// O portador não pode receber dano enquanto este buff estiver ativo.
    /// Funciona como BloqueioTotal mas dura múltiplos turnos.
    /// </summary>
    class Intocavel : Buff
    {
        public Intocavel(int turnos = 2) : base("Intocável", "🕳️", turnos, 1, "Imune a dano.") { }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
