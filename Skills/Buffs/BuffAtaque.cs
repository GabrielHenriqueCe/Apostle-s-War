using ApostlesWar;

namespace ApostlesWar.Skills.Buffs
{
    /// <summary>
    /// Buff temporário de ATK. Não muta o stat — apenas existe em StatusAtivos.
    /// O Combate.Ataque calcula o bônus sob demanda: percentual sobre
    /// (base + multiplicador + itens + bônus permanente).
    /// 
    /// Não acumula: se já houver BuffAtaque, mantém o de maior Valor;
    /// em empate de Valor, mantém o de maior duração.
    /// </summary>
    class BuffAtaque : Buff
    {
        public BuffAtaque(int duracao = 2, double percentual = 0.25)
            : base("ATK+", "⚔️", duracao, percentual, $"+{percentual * 100:F0}% ATK.") { }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<BuffAtaque>().FirstOrDefault();
            if (existente != null)
            {
                // Mais forte prevalece: maior Valor; empate decide pela duração.
                if (Valor < existente.Valor) return;
                if (Valor == existente.Valor && DuracaoRestante <= existente.DuracaoRestante) return;
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