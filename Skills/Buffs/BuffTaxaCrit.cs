using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Aumenta a TaxaCrit do portador em pontos absolutos calculados no momento da aplicação.
    /// Ex: TaxaCrit 0.10 + buff 0.25 → vira 0.35 enquanto ativo.
    /// Ao expirar, restaura a TaxaCrit original.
    /// </summary>
    class BuffTaxaCrit : Buff
    {
        private double _valorAdicionado;

        public BuffTaxaCrit(int turnos = 2, double valor = 0.25)
            : base("Crit+", "🎯", turnos, valor, $"+{valor * 100:F0}% TaxaCrit.") { }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<BuffTaxaCrit>().FirstOrDefault();
            if (existente != null)
            {
                if (this.TurnosRestantes <= existente.TurnosRestantes) return;
                alvo.StatusAtivos.Remove(existente);
                alvo.ModificarTaxaCrit(-existente._valorAdicionado);
            }

            _valorAdicionado = Valor;
            alvo.ModificarTaxaCrit(_valorAdicionado);
            alvo.StatusAtivos.Add(this);
        }

        public override void Remover(Combate alvo)
        {
            alvo.ModificarTaxaCrit(-_valorAdicionado);
            alvo.StatusAtivos.Remove(this);
        }
    }
}