using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Aumenta o ATK do portador em um valor flat calculado como percentual no momento da aplicação.
    /// Ao expirar, restaura o ATK original.
    /// </summary>
    class BuffAtaque : Buff
    {
        private int _valorAdicionado;

        public BuffAtaque(int turnos = 2, double percentual = 0.25)
            : base("ATK+", "⚔️", turnos, percentual, $"+{percentual * 100:F0}% ATK.") { }

        public void AplicarEfeito(Combate alvo)
        {
            _valorAdicionado = (int)(alvo.Ataque * Valor);
            alvo.ModificarAtaque(_valorAdicionado);
            base.Aplicar(alvo);
        }

        public override bool Equals(object? obj)
        {
            return obj is BuffAtaque ataque &&
                   Nome == ataque.Nome &&
                   Simbolo == ataque.Simbolo &&
                   Turnos == ataque.Turnos &&
                   Descricao == ataque.Descricao &&
                   Valor == ataque.Valor &&
                   TurnosRestantes == ataque.TurnosRestantes &&
                   Expirou == ataque.Expirou &&
                   this._valorAdicionado == ataque._valorAdicionado &&
                   this._valorAdicionado == ataque._valorAdicionado;
        }

        public override void Remover(Combate alvo)
        {
            alvo.ModificarAtaque(-_valorAdicionado);
            alvo.StatusAtivos.Remove(this);
        }
    }
}
