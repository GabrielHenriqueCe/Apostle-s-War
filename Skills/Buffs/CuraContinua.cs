using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Cura % do HP máximo no início do turno do portador. Espelho positivo do Veneno.
    /// </summary>
    class CuraContinua : Buff
    {
        public CuraContinua(int turnos = 1, double percentual = 0.10)
            : base("Cura Contínua", "💚", turnos, percentual,
                $"Cura {percentual * 100:F0}% HP no início do turno.")
        { }

        public override void AoIniciarTurno(Combate portador)
        {
            portador.Curar((int)(portador.HPMaximo * Valor));
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}