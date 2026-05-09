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
    abstract class Buff : Habilidade
    {
        public Buff(string nome, int turnos, string descricao = "") : base(nome, turnos, descricao) { }
        public virtual bool Revive() => false;
        public abstract bool DeveAtivar(EventoCombate evento);
        public abstract string MensagemSobreviveu(Personagem personagem);
        public abstract string MensagemMorreu(Personagem personagem);
    }

    #endregion
}
