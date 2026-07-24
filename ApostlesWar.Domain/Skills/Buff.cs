using ApostlesWar.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApostlesWar.Domain.Skills
{
    #region Buff

    /// <summary>
    /// Efeitos positivos que aumentam as capacidades do personagem, como aumento de ataque, defesa, velocidade, etc. 
    /// Geralmente duram por um número limitado de turnos e podem ser aplicados por habilidades ativas ou passivas.
    /// </summary>
    public abstract class Buff : StatusEffect
    {
        public Buff(string nome, string simbolo, int duracao, double valor, string descricao = "", bool removivel = true)
            : base(nome, simbolo, duracao, valor, descricao, removivel) { }
    }

    #endregion
}
