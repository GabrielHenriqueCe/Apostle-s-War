using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Buff temporário de DEF. Não muta o stat — apenas existe em StatusAtivos.
    /// Combate.Defesa calcula o bônus sob demanda: percentual sobre
    /// (base + multiplicador + itens + bônus permanente − redução permanente).
    /// 
    /// Não acumula: mantém o de maior Valor; em empate, o de maior duração.
    /// ContribuicaoDefesa expõe quanto este buff soma agora, pra habilidades
    /// que ignoram buffs de defesa no ataque (ex: Vendaval).
    /// </summary>
    class BuffDefesa : Buff
    {
        public BuffDefesa(int turnos = 2, double percentual = 0.30)
            : base("DEF+", "🛡️", turnos, percentual, $"+{percentual * 100:F0}% DEF.") { }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<BuffDefesa>().FirstOrDefault();
            if (existente != null)
            {
                if (Valor < existente.Valor) return;
                if (Valor == existente.Valor && TurnosRestantes <= existente.TurnosRestantes) return;
                alvo.StatusAtivos.Remove(existente);
            }

            alvo.StatusAtivos.Add(this);
        }

        /// <summary>
        /// Quanto este buff soma à defesa AGORA: percentual sobre a base com
        /// itens e stacks permanentes (mesma base que o getter de Defesa usa).
        /// </summary>
        public override int ContribuicaoDefesa(Combate portador) =>
            (int)(portador.DefesaComStacks * Valor);

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}