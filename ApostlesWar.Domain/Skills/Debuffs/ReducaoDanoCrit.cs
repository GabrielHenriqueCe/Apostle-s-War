using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Debuffs
{
    /// <summary>
    /// Debuff temporário de DanoCrit (pontos absolutos). Não muta o stat — apenas
    /// existe em StatusAtivos. Combate.DanoCrit subtrai o Valor sob demanda.
    ///
    /// Não acumula: mantém o de maior Valor; em empate, o de maior duração.
    /// ContribuicaoDanoCrit expõe (negativo) quanto este debuff tira agora.
    /// </summary>
    public class ReducaoDanoCrit : Debuff, IContribuiDanoCrit
    {
        public ReducaoDanoCrit(int duracao = 2, double valor = 0.25)
            : base("Redução Dano Crit", "💥", duracao, valor, $"-{valor * 100:F0}% Dano Crítico.") { }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<ReducaoDanoCrit>().FirstOrDefault();
            if (existente != null)
            {
                if (Valor < existente.Valor) return;
                if (Valor == existente.Valor && DuracaoRestante <= existente.DuracaoRestante) return;
                alvo.StatusAtivos.Remove(existente);
            }

            alvo.StatusAtivos.Add(this);
        }

        public double ContribuicaoDanoCrit(Combate portador) => -Valor;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
