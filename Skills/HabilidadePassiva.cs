namespace ApostlesWar
{
    #region HabilidadePassiva

    abstract class HabilidadePassiva : Habilidade
    {
        public HabilidadePassiva(string nome, string simbolo, int turnos, string descricao = "")
            : base(nome, simbolo, turnos, descricao) { }

        public virtual bool Revive() => false;

        /// <summary>
        /// Cada passiva decide sozinha se deve disparar com base no evento e contexto.
        /// Sem ifs externos — o CombateService só passa o contexto.
        /// </summary>
        public abstract bool DeveAtivar(EventoCombate evento, ContextoPassiva contexto);

        public abstract string MensagemSobreviveu(Personagem personagem);
        public abstract string MensagemMorreu(Personagem personagem);

        protected List<ResultadoAtaque> SemDano() => new List<ResultadoAtaque>();

        public override abstract List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista);
    }

    #endregion
}
