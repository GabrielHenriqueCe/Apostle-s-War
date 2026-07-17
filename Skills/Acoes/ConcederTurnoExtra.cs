namespace ApostlesWar
{
    /// <summary>
    /// Concede um turno extra ao combatente do escopo (joga de novo logo após o turno atual).
    /// Verbo atômico do vocabulário (ADR-composicao-de-acoes §9 / catálogo) — não tem valor nem
    /// fábrica, só marca o TemTurnoExtra via Combate.ConcederTurnoExtra. Default ProprioAtacante:
    /// o caso do jogo é o conjurador jogar de novo (Rato Voador). 1º cliente: Rato Voador (Morcego).
    /// </summary>
    class ConcederTurnoExtra : Acao
    {
        public ConcederTurnoExtra(Escopo escopo = Escopo.ProprioAtacante, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) { }

        public override void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos)
            => alvo.ConcederTurnoExtra();
    }
}
