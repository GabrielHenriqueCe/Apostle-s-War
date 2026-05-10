namespace ApostlesWar
{
    #region HabilidadePassiva

    /// <summary>
    /// Habilidade ativada automaticamente em resposta a eventos do jogo, início de turno
    /// </summary>
    abstract class HabilidadePassiva : Habilidade
    {
        public HabilidadePassiva(string nome, string simbolo, int turnos, string descricao = "") 
            : base(nome, simbolo, turnos, descricao) { }
        public virtual bool Revive() => false;
        public abstract bool DeveAtivar(EventoCombate evento);
        public abstract string MensagemSobreviveu(Personagem personagem);
        public abstract string MensagemMorreu(Personagem personagem);
    }

    #endregion
}