namespace ApostlesWar
{
    /// <summary>
    /// Revive combatentes mortos com um percentual do HP máximo. Nasce com 7 clientes reais
    /// mapeados (família do revive — ADR-composicao-de-acoes §9): Nigiri, Tecnology, Céu,
    /// AnjoCaído, DocesDeAbobora, Circo, Atlantis.
    ///
    /// A Sentença (ImpedirRessurreicao) é checada CENTRAL no Morto.Reviver — esta ação não
    /// escreve nada disso. `quantos` (default todos) cobre o 2º cliente, DocesDeAbobora, que
    /// revive só o 1º elegível — por isso é AcaoSobreConjunto: "só o primeiro do conjunto"
    /// precisa enxergar o conjunto inteiro de uma vez (um Reviver per-alvo não sabe se já
    /// reviveu alguém nesta ativação — Acao não carrega estado por-ativação, ADR §3.1).
    /// EstadoAlvo default Mortos — o único que faz sentido; o filtro do interpretador garante
    /// que a ação nem visita os vivos.
    /// </summary>
    class Reviver : AcaoSobreConjunto
    {
        private readonly double _percentualHP;
        private readonly int _quantos;

        public Reviver(double percentualHP, int quantos = int.MaxValue, Escopo escopo = Escopo.TodosAliados,
            EstadoAlvo estadoAlvo = EstadoAlvo.Mortos)
            : base(escopo, estadoAlvo)
        {
            _percentualHP = percentualHP;
            _quantos = quantos;
        }

        public override void Executar(Combate atacante, IReadOnlyList<Combate> conjunto, List<EventoDano> eventos)
        {
            foreach (var alvo in conjunto.Take(_quantos).ToList())
                alvo.Reviver((int)(alvo.HPMaximo * _percentualHP));
        }
    }
}
