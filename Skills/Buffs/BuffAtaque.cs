using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Aumenta o ATK do portador em um percentual calculado no momento da aplicação.
    /// Ao expirar, restaura o ATK original.
    /// </summary>
    class BuffAtaque : Buff
    {
        private int _valorAdicionado;

        public BuffAtaque(int turnos = 2, double percentual = 0.25)
            : base("ATK+", "⚔️", turnos, percentual, $"+{percentual * 100:F0}% ATK.") { }

        public override void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.OfType<BuffAtaque>().FirstOrDefault();
            if (existente != null)
            {
                if (this.TurnosRestantes <= existente.TurnosRestantes) return;
                alvo.StatusAtivos.Remove(existente);
                alvo.ModificarAtaque(-existente._valorAdicionado);
            }

            _valorAdicionado = (int)(alvo.Ataque * Valor);
            alvo.ModificarAtaque(_valorAdicionado);
            alvo.StatusAtivos.Add(this);
        }

        public override void Remover(Combate alvo)
        {
            alvo.ModificarAtaque(-_valorAdicionado);
            alvo.StatusAtivos.Remove(this);
        }
    }
}