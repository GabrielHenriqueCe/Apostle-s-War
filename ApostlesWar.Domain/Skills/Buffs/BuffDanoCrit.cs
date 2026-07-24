using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Buffs
{
    /// <summary>
    /// Buff temporário de DanoCrit (pontos absolutos). Não muta o stat — apenas
    /// existe em StatusAtivos. Combate.DanoCrit soma o Valor sob demanda.
    /// Ex: DanoCrit base 0.50 + buff 0.25 → 0.75 enquanto ativo.
    ///
    /// Não acumula: mantém o de maior Valor; em empate, o de maior duração.
    /// </summary>
    public class BuffDanoCrit : Buff, IContribuiDanoCrit
    {
        public BuffDanoCrit(int duracao = 2, double valor = 0.25)
            : base("DanoCrit+", "💥", duracao, valor, $"+{valor * 100:F0}% Dano Crítico.") { }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<BuffDanoCrit>().FirstOrDefault();
            if (existente != null)
            {
                if (Valor < existente.Valor) return;
                if (Valor == existente.Valor && DuracaoRestante <= existente.DuracaoRestante) return;
                alvo.StatusAtivos.Remove(existente);
            }

            alvo.StatusAtivos.Add(this);
        }

        public double ContribuicaoDanoCrit(Combate portador) => Valor;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
