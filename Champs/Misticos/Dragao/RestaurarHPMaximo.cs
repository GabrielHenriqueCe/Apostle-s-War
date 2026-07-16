namespace ApostlesWar.Champs.Misticos
{
    /// <summary>
    /// Restaura HP máximo PERDIDO do alvo (o que Maldição/Queima reduziram do teto), até um cap de
    /// percentualCap × HPMaximoInicial. Bespoke LOCAL do Dragão — só o DragãoProtetor restaura HP
    /// máx hoje (ADR-composicao-de-acoes §9, Nível 2: promove pra Skills/Acoes/ no 2º cliente real).
    /// Verbo nichado: mexe no TETO (HPMaximo), não no HP atual — por isso não é Cura nem fragmento
    /// de Valor.
    /// </summary>
    class RestaurarHPMaximo : Acao
    {
        private readonly double _percentualCap;

        public RestaurarHPMaximo(double percentualCap, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) => _percentualCap = percentualCap;

        public override void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos)
            => alvo.RestaurarHPMaximo((int)(alvo.HPMaximoInicial * _percentualCap));
    }
}
