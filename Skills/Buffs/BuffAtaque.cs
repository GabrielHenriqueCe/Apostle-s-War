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
            _valorAdicionado = (int)(alvo.Ataque * Valor);
            alvo.ModificarAtaque(_valorAdicionado);
            base.Aplicar(alvo);
        }

        public override void Remover(Combate alvo)
        {
            alvo.ModificarAtaque(-_valorAdicionado);
            alvo.StatusAtivos.Remove(this);
        }
    }
}
