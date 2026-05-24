using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Buff: cada vez que o portador causa dano direto, recupera um percentual desse dano como HP.
    /// Usado pela PassivaMorcego (aplicado permanente via IPassivaInicial).
    /// </summary>
    class Sedento : Buff
    {
        public Sedento(int turnos = int.MaxValue, double percentual = 0.15)
            : base("Sedento", "🩸", turnos, percentual,
                $"Cura {percentual * 100:F0}% do dano causado.")
        { }

        public override void AoCausarDano(Combate portador, Combate alvo, int danoCausado)
        {
            if (danoCausado <= 0) return;
            portador.Curar((int)(danoCausado * Valor));
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}