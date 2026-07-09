using ApostlesWar;

namespace ApostlesWar.Skills.Buffs
{
    /// <summary>
    /// Buff temporário de TaxaCrit (pontos absolutos). Não muta o stat — apenas
    /// existe em StatusAtivos. Combate.TaxaCrit soma o Valor sob demanda.
    /// Ex: TaxaCrit base 0.15 + buff 0.25 → 0.40 enquanto ativo.
    /// 
    /// Não acumula: mantém o de maior Valor; em empate, o de maior duração.
    /// </summary>
    class BuffTaxaCrit : Buff
    {
        public BuffTaxaCrit(int turnos = 2, double valor = 0.25)
            : base("Crit+", "🎯", turnos, valor, $"+{valor * 100:F0}% TaxaCrit.") { }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<BuffTaxaCrit>().FirstOrDefault();
            if (existente != null)
            {
                if (Valor < existente.Valor) return;
                if (Valor == existente.Valor && TurnosRestantes <= existente.TurnosRestantes) return;
                alvo.StatusAtivos.Remove(existente);
            }

            alvo.StatusAtivos.Add(this);
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}