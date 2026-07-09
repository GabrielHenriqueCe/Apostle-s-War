using ApostlesWar;

namespace ApostlesWar.Skills.Debuffs
{
    /// <summary>
    /// Debuff temporário de DEF (-30%). Não muta o stat — apenas existe em
    /// StatusAtivos. Combate.Defesa subtrai o percentual sob demanda, sobre
    /// (base + multiplicador + itens + bônus permanente − redução permanente).
    /// 
    /// Não acumula: mantém o de maior Valor; em empate, o de maior duração.
    /// ContribuicaoDefesa expõe (negativo) quanto este debuff tira agora, pra
    /// habilidades que ignoram status de defesa no ataque.
    /// </summary>
    class ReducaoDefesa : Debuff, IContribuiDefesa
    {
        public ReducaoDefesa(int turnos = 2)
            : base("Redução DEF", "🔎", turnos, 0.30, "-30% DEF.") { }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<ReducaoDefesa>().FirstOrDefault();
            if (existente != null)
            {
                if (Valor < existente.Valor) return;
                if (Valor == existente.Valor && TurnosRestantes <= existente.TurnosRestantes) return;
                alvo.StatusAtivos.Remove(existente);
            }

            alvo.StatusAtivos.Add(this);
        }

        /// <summary>
        /// Quanto este debuff tira da defesa AGORA (valor negativo): percentual
        /// sobre a base com itens e stacks permanentes.
        /// </summary>
        public int ContribuicaoDefesa(Combate portador) =>
            -(int)(portador.DefesaComStacks * Valor);

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}