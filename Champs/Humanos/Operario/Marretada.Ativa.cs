using ApostlesWar;
namespace ApostlesWar.Champs.Humanos
{
    /// <summary>
    /// Habilidade-CLASSE híbrida (ADR §4, Nível 1 + método custom): o Ativar é declarativo
    /// (Acoes), mas ela continua classe porque o InstintoDoOperario a encontra por tipo
    /// (OfType) e o contra-ataque entra pelo AtivarComNatureza.
    /// </summary>
    class Marretada : HabilidadeAtiva, IAtivavelComNatureza
    {
        private const double Multiplicador = 1.25;

        public Marretada() : base("Marretada", "🔨", 3, "Causa 125% do ATK em 1 inimigo.") { }
        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;

        protected override List<Acao> Acoes => new()
        {
            new Dano(Multiplicador),
        };

        /// <summary>
        /// Entrada usada como contra-ataque (InstintoDoOperario busca a Marretada
        /// do portador). Alvo já é fixo (o agressor) — não passa por ResolverAlvos.
        /// </summary>
        public EventoDano AtivarComNatureza(Combate atacante, Combate alvo, NaturezaDano natureza)
            => atacante.Atacar(alvo, Multiplicador, natureza: natureza);
    }
}