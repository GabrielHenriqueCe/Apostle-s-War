using ApostlesWar;

namespace ApostlesWar.Skills.Buffs
{
    /// <summary>
    /// Cura % do HP máximo no início do turno do portador. Espelho positivo do Veneno.
    /// </summary>
    class CuraContinua : Buff
    {
        public CuraContinua(int duracao = 1, double percentual = 0.10)
            : base("Cura Contínua", "💚", duracao, percentual,
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