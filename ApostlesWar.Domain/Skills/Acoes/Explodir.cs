namespace ApostlesWar.Domain
{
    /// <summary>
    /// Detona status de tick do alvo (IStatusComTick) conforme um Seletor — o molde ÚNICO das
    /// explosões (regra de Gabriel): esta ação orquestra igual pra todos, e cada status detona
    /// fazendo o que ELE faz (Veneno só dano; Queima dano + redução de HP máximo). Os EventoDano
    /// das detonações entram no eventos (invariante ADR-composicao-de-acoes §7): aparecem na
    /// exibição, contam no PorDanoCausado das ações seguintes e a morte-por-explosão passa
    /// pelos Atos de morte. A natureza de status (Reacao.Nenhuma) garante que a detonação NÃO
    /// proca reações de ataque (contra-ataque, Sorrateiro etc — guards no CombateService).
    ///
    /// Clientes: Putrefação (Seletor.Tipo&lt;Veneno&gt;()); Inferno usa Seletor.Tipo&lt;Queima&gt;()
    /// quando migrar (Decaídos). Efeitos EXTRAS da habilidade (ex: a cura da Putrefação) NÃO
    /// moram aqui — são ações separadas na lista, por isso a explosão é reutilizável a seco.
    /// </summary>
    public class Explodir : Acao
    {
        private readonly Seletor _seletor;

        public Explodir(Seletor seletor, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) => _seletor = seletor;

        public override Utilidade Utilidade => Utilidade.Ferir;

        /// <summary>
        /// Explodir quem não tem o status detonável não causa nada. Mesmo filtro do Executar
        /// (IStatusComTick + seletor), pra a pergunta e a execução concordarem.
        /// </summary>
        public override bool TemEfeitoUtil(Combate atacante, IReadOnlyList<Combate> alvos)
            => alvos.Any(a => a.StatusAtivos.Any(s => s is IStatusComTick && _seletor.Filtro(s)));

        /// <summary>
        /// A soma das detonações que cairiam neste alvo. Mesmo filtro e mesmo `Quantos` do Executar —
        /// mas sem `Aleatorio`, porque prever qual o sorteio vai pegar é impossível; com os candidatos
        /// na ordem natural, a estimativa fica estável.
        /// </summary>
        public override int PreverVidaRemovida(Combate atacante, Combate alvo)
            => alvo.StatusAtivos
                .Where(s => s is IStatusComTick && _seletor.Filtro(s))
                .Take(_seletor.Quantos)
                .Sum(s => ((IStatusComTick)s).PreverDetonacao(alvo));

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
        {
            IEnumerable<StatusEffect> candidatos = alvo.StatusAtivos
                .Where(s => s is IStatusComTick && _seletor.Filtro(s));
            if (_seletor.Aleatorio)
                candidatos = candidatos.OrderBy(_ => Guid.NewGuid());

            foreach (var status in candidatos.Take(_seletor.Quantos).ToList())
                eventos.Add(((IStatusComTick)status).Detonar(alvo, atacante));
        }
    }
}
