using ApostlesWar.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApostlesWar.Domain.Skills
{
    #region Deuff

    /// <summary>
    /// Efeitos negativos que diminuem as capacidades do personagem, como diminuir   de ataque, defesa, velocidade, etc. 
    /// Geralmente duram por um número limitado de turnos e podem ser aplicados por habilidades ativas ou passivas.
    /// </summary>
    public abstract class Debuff : StatusEffect
    {
        public Debuff(string nome, string simbolo, int duracao, double valor, string descricao = "")
            : base(nome, simbolo, duracao, valor, descricao) { }
    }

    #endregion
}
