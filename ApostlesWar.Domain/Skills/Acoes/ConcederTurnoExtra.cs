namespace ApostlesWar.Domain
{
    /// <summary>
    /// Concede um turno extra ao combatente do escopo (joga de novo logo após o turno atual).
    /// Verbo atômico do vocabulário (ADR-composicao-de-acoes §9 / catálogo) — não tem valor nem
    /// fábrica, só marca o TemTurnoExtra via Combate.ConcederTurnoExtra. Default ProprioAtacante:
    /// o caso do jogo é o conjurador jogar de novo (Rato Voador). 1º cliente: Rato Voador (Morcego).
    /// </summary>
    public class ConcederTurnoExtra : Acao
    {
        public ConcederTurnoExtra(Escopo escopo = Escopo.ProprioAtacante, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) { }

        public override Utilidade Utilidade => Utilidade.TurnoExtra;

        /// <summary>
        /// A flag é booleana: conceder a quem já tem turno extra pendente não acumula nada.
        /// </summary>
        public override bool TemEfeitoUtil(Combate atacante, IReadOnlyList<Combate> alvos)
            => alvos.Any(a => !a.TemTurnoExtra);

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
            => alvo.ConcederTurnoExtra();
    }
}
