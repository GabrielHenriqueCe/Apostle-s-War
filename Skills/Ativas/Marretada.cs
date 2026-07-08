using ApostlesWar;
namespace v1_Apostle_s_War.Skills.Ativas
{
    class Marretada : HabilidadeAtiva, IAtivavelComNatureza
    {
        private const double Multiplicador = 1.25;

        public Marretada() : base("Marretada", "🔨", 3, "Causa 125% do ATK em 1 inimigo.") { }
        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(AplicarDano(ctx.Atacante, a, Multiplicador));
            return resultados;
        }

        /// <summary>
        /// Entrada usada como contra-ataque (InstintoDoOperario busca a Marretada
        /// do portador). Alvo já é fixo (o agressor) — não passa por ResolverAlvos.
        /// </summary>
        public EventoDano AtivarComNatureza(Combate atacante, Combate alvo, NaturezaDano natureza)
            => atacante.Atacar(alvo, Multiplicador, natureza: natureza);
    }
}