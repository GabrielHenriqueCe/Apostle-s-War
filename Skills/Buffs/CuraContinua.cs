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

        public override EventoCombate? AoIniciarTurno(Combate portador)
        {
            int curado = portador.Curar((int)(portador.HPMaximo * Valor));
            return new EventoCura(portador, portador, curado, portador.HPAtual);   // mostra mesmo curando 0
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}