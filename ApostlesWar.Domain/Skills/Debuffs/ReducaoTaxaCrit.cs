using ApostlesWar;

namespace ApostlesWar.Skills.Debuffs
{
    /// <summary>
    /// Debuff temporário de TaxaCrit (pontos absolutos). Não muta o stat — apenas
    /// existe em StatusAtivos. Combate.TaxaCrit subtrai o Valor sob demanda.
    /// Ex: TaxaCrit base 0.40 − debuff 0.25 → 0.15 enquanto ativo.
    ///
    /// Não acumula: mantém o de maior Valor; em empate, o de maior duração.
    /// ContribuicaoTaxaCrit expõe (negativo) quanto este debuff tira agora.
    /// </summary>
    class ReducaoTaxaCrit : Debuff, IContribuiTaxaCrit
    {
        public ReducaoTaxaCrit(int duracao = 2, double valor = 0.25)
            : base("Redução Crit", "🎯", duracao, valor, $"-{valor * 100:F0}% TaxaCrit.") { }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<ReducaoTaxaCrit>().FirstOrDefault();
            if (existente != null)
            {
                if (Valor < existente.Valor) return;
                if (Valor == existente.Valor && DuracaoRestante <= existente.DuracaoRestante) return;
                alvo.StatusAtivos.Remove(existente);
            }

            alvo.StatusAtivos.Add(this);
        }

        public double ContribuicaoTaxaCrit(Combate portador) => -Valor;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
