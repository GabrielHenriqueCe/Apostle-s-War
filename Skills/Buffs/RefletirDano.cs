using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Reflete uma porcentagem do dano recebido de volta ao atacante.
    /// O CombateService deve chamar Refletir(atacante, dano) ao processar ataques.
    /// </summary>
    class RefletirDano : Buff
    {
        public RefletirDano(int turnos = 2, double percentual = 0.15)
            : base("Reflexo", "🥢", turnos, percentual, $"Reflete {percentual * 100:F0}% do dano recebido.") { }

        /// <summary>
        /// Aplica o reflexo de dano ao atacante original. Chamar no CombateService.
        /// </summary>
        public void Refletir(Combate atacante, int danoRecebido)
        {
            int danoRefletido = (int)(danoRecebido * Valor);
            if (danoRefletido > 0)
                atacante.ReceberDano(danoRefletido);
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
