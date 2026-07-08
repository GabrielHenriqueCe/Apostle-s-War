namespace ApostlesWar
{
    /// <summary>
    /// Revive combatentes mortos com um percentual do HP máximo. Nasce com 7 clientes reais
    /// mapeados (família do revive — ADR-composicao-de-acoes §9): Nigiri, Tecnology, Céu,
    /// AnjoCaído, DocesDeAbobora, Circo, Atlantis.
    ///
    /// A Sentença (ImpedirRessurreicao) é checada CENTRAL no Morto.Reviver — esta ação não
    /// escreve nada disso. O "quantos" (DocesDeAbobora revive só 1) entra quando esse cliente
    /// migrar (LadoSombrio). EstadoAlvo default Mortos — o único que faz sentido; o filtro do
    /// interpretador garante que a ação nem visita os vivos.
    /// </summary>
    class Reviver : Acao
    {
        private readonly double _percentualHP;

        public Reviver(double percentualHP, Escopo escopo = Escopo.TodosAliados,
            EstadoAlvo estadoAlvo = EstadoAlvo.Mortos)
            : base(escopo, estadoAlvo) => _percentualHP = percentualHP;

        public override void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos)
            => alvo.Reviver((int)(alvo.HPMaximo * _percentualHP));
    }
}
