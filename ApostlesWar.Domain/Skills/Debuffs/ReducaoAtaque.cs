using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Debuffs
{
    /// <summary>
    /// Debuff temporário de ATK. Não muta o stat — apenas existe em StatusAtivos.
    /// Combate.Ataque subtrai o percentual sob demanda, sobre
    /// (base + multiplicador + itens + bônus permanente).
    ///
    /// Não acumula: mantém o de maior Valor; em empate, o de maior duração.
    /// ContribuicaoAtaque expõe (negativo) quanto este debuff tira agora — espelho
    /// do ReducaoDefesa.
    /// </summary>
    public class ReducaoAtaque : Debuff, IContribuiAtaque
    {
        public ReducaoAtaque(int duracao = 2, double percentual = 0.25)
            : base("Redução ATK", "⚔️", duracao, percentual, $"-{percentual * 100:F0}% ATK.") { }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<ReducaoAtaque>().FirstOrDefault();
            if (existente != null)
            {
                if (Valor < existente.Valor) return;
                if (Valor == existente.Valor && DuracaoRestante <= existente.DuracaoRestante) return;
                alvo.StatusAtivos.Remove(existente);
            }

            alvo.StatusAtivos.Add(this);
        }

        /// <summary>
        /// Quanto este debuff tira do ataque AGORA (valor negativo): percentual
        /// sobre a base com itens e stacks permanentes.
        /// </summary>
        public int ContribuicaoAtaque(Combate portador) =>
            -(int)(portador.AtaqueComStacks * Valor);

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
