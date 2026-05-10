using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text;

namespace v1_Apostle_s_War.Skills
{
    #region Buff

    /// <summary>
    /// Efeitos positivos que aumentam as capacidades do personagem, como aumento de ataque, defesa, velocidade, etc. 
    /// Geralmente duram por um número limitado de turnos e podem ser aplicados por habilidades ativas ou passivas.
    /// </summary>
    abstract class Buff : StatusEffect
    {
        public Buff(string nome, string simbolo, int turnosRestantes, double valor, string descricao = "")
            : base(nome, simbolo, turnosRestantes, valor, descricao) { }
    }

    #endregion
}
