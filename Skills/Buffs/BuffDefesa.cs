using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Aumenta a DEF do portador em um percentual calculado no momento da aplicação.
    /// Ao expirar, restaura a DEF original.
    /// </summary>
    class BuffDefesa : Buff
    {
        private int _valorAdicionado;

        public BuffDefesa(int turnos = 2, double percentual = 0.30)
            : base("DEF+", "🛡️", turnos, percentual, $"+{percentual * 100:F0}% DEF.") { }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<BuffDefesa>().FirstOrDefault();
            if (existente != null)
            {
                if (this.TurnosRestantes <= existente.TurnosRestantes) return;
                alvo.StatusAtivos.Remove(existente);
                alvo.ModificarDefesa(-existente._valorAdicionado);
            }

            _valorAdicionado = (int)(alvo.Defesa * Valor);
            alvo.ModificarDefesa(_valorAdicionado);
            alvo.StatusAtivos.Add(this);
        }

        public override void Remover(Combate alvo)
        {
            alvo.ModificarDefesa(-_valorAdicionado);
            alvo.StatusAtivos.Remove(this);
        }
    }
}