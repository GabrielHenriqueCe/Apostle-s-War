using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;

namespace v1_Apostle_s_War.Skills
{
    #region Deuff

    /// <summary>
    /// Efeitos negativos que diminuem as capacidades do personagem, como diminuir   de ataque, defesa, velocidade, etc. 
    /// Geralmente duram por um número limitado de turnos e podem ser aplicados por habilidades ativas ou passivas.
    /// </summary>
    abstract class Debuff : StatusEffect
    {
        public Debuff(string nome, int turnosRestantes, double valor, string descricao = "")
            : base(nome, turnosRestantes, valor, descricao) { }
    }

    #endregion
}
