using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Debuffs
{
    /// <summary>
    /// Força o portador a atacar com A1 automaticamente quem aplicou este debuff.
    /// Ignora Intocável e Provocar — tem prioridade máxima.
    /// </summary>
    class Irritar : Debuff
    {
        /// <summary>
        /// Quem aplicou o debuff — alvo forçado do próximo ataque.
        /// </summary>
        public Combate Aplicador { get; }

        public Irritar(Combate aplicador, int turnos = 1)
            : base("Irritar", "😡", turnos, 0, "Força atacar com A1 quem aplicou este debuff.")
        {
            Aplicador = aplicador;
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
