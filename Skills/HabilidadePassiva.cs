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
        /// </summary>
        public abstract bool DeveAtivar(EventoCombate evento, ContextoPassiva contexto);

        /// <summary>
        /// Mensagem exibida quando a passiva impede a morte ou faz algo notavel
        /// no momento de ser atacada. Default vazio — sobrescreva apenas se a
        /// passiva tem mensagem real (ex: Guarda, Operario, Necromancia).
        /// </summary>
        public virtual string MensagemSobreviveu(Personagem personagem) => string.Empty;

        /// <summary>
        /// Mensagem exibida quando a passiva nao consegue evitar a morte
        /// (ex: Necromancia bloqueada por MortePermanente).
        /// Default vazio — sobrescreva apenas se a passiva tem mensagem real.
        /// </summary>
        public virtual string MensagemMorreu(Personagem personagem) => string.Empty;

        protected List<ResultadoAtaque> SemDano() => new List<ResultadoAtaque>();

        public override abstract List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo);

        /// <summary>
        /// Obtém (ou cria) o estado de runtime desta passiva para o combatente.
        /// </summary>
        protected T ObterEstado<T>(Combate combate) where T : new()
        {
            if (!combate.EstadoHabilidades.TryGetValue(this, out var estado) || estado is not T)
            {
                estado = new T();
                combate.EstadoHabilidades[this] = estado;
            }
            return (T)estado;
        }
    }

    #endregion
}