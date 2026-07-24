using ApostlesWar;

namespace ApostlesWar.Skills.Buffs
{
    /// <summary>
    /// Buff temporário de TaxaCrit (pontos absolutos). Não muta o stat — apenas
    /// existe em StatusAtivos. Combate.TaxaCrit soma o Valor sob demanda.
    /// Ex: TaxaCrit base 0.15 + buff 0.25 → 0.40 enquanto ativo.
    /// 
    /// Não acumula: mantém o de maior Valor; em empate, o de maior duração.
    /// ContribuicaoTaxaCrit expõe quanto este buff soma agora, pro getter de TaxaCrit.
    /// </summary>
    class BuffTaxaCrit : Buff, IContribuiTaxaCrit
    {
        public BuffTaxaCrit(int duracao = 2, double valor = 0.25)
            : base("Crit+", "🎯", duracao, valor, $"+{valor * 100:F0}% TaxaCrit.") { }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<BuffTaxaCrit>().FirstOrDefault();
            if (existente != null)
            {
                if (Valor < existente.Valor) return;
                if (Valor == existente.Valor && DuracaoRestante <= existente.DuracaoRestante) return;
                alvo.StatusAtivos.Remove(existente);
            }

            alvo.StatusAtivos.Add(this);
        }

        public double ContribuicaoTaxaCrit(Combate portador) => Valor;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}