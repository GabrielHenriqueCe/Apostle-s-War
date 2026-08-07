namespace ApostlesWar.Domain.Apostolos.Misticos
{
    /// <summary>
    /// Restaura HP máximo PERDIDO do alvo (o que Maldição/Queima reduziram do teto), até um cap de
    /// percentualCap × HPMaximoInicial. Bespoke LOCAL do Dragão — só o DragãoProtetor restaura HP
    /// máx hoje (ADR-composicao-de-acoes §9, Nível 2: promove pra Skills/Acoes/ no 2º cliente real).
    /// Verbo nichado: mexe no TETO (HPMaximo), não no HP atual — por isso não é Cura nem fragmento
    /// de Valor.
    /// </summary>
    public class RestaurarHPMaximo : Acao
    {
        private readonly double _percentualCap;

        public RestaurarHPMaximo(double percentualCap, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) => _percentualCap = percentualCap;

        // Devolve vida perdida — só que do TETO, não do HP atual. Pro avaliador é a mesma família.
        public override Utilidade Utilidade => Utilidade.Curar;

        /// <summary>Sem teto perdido pra devolver, não há o que restaurar.</summary>
        public override bool TemEfeitoUtil(Combate atacante, IReadOnlyList<Combate> alvos)
            => alvos.Any(a => a.HPMaximoReduzidoTotal > 0);

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
            => alvo.RestaurarHPMaximo((int)(alvo.HPMaximoInicial * _percentualCap));
    }
}
