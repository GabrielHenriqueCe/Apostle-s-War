namespace ApostlesWar
{
    #region HabilidadePassiva

    abstract class HabilidadePassiva : Habilidade
    {
        public HabilidadePassiva(string nome, string simbolo, int turnos, string descricao = "")
            : base(nome, simbolo, turnos, descricao) { }

        public virtual bool Revive() => false;

        /// <summary>
        /// Passivas do sistema VELHO (enum/evento) sobrescrevem isto pra decidir se
        /// disparam. Passivas MIGRADAS para o modelo de reação (IReageAo*) NÃO
        /// sobrescrevem — herdam o default false e são processadas pelo dispatch de
        /// reações (ProcessarReacoesAlvo), não por aqui.
        /// </summary>
        public virtual bool DeveAtivar(EventoCombate evento, ContextoPassiva contexto) => false;

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

        protected List<EventoDano> SemDano() => new List<EventoDano>();

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();

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