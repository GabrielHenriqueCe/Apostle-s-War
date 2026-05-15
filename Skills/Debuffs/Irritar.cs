using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Debuffs
{
    /// <summary>
    /// Força o portador a atacar com A1 quem aplicou. Verificado no ExecutarTurno.
    /// </summary>
    class Irritar : Debuff
    {
        public Combate Aplicador { get; }

        public Irritar(Combate aplicador, int turnos = 1)
            : base("Irritar", "😡", turnos, 0, "Força atacar com A1 quem aplicou.")
        {
            Aplicador = aplicador;
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
