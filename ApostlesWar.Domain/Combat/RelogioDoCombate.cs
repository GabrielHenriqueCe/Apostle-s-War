namespace ApostlesWar.Domain
{
    /// <summary>
    /// Relógio GLOBAL do combate: conta os turnos jogados na batalha atual — avança 1 a cada
    /// turno de um combatente (inclui turnos-extra e turnos pulados por Preso: "cada vez que um
    /// personagem joga"). Mora no nível do CombateService (que o avança e reinicia) e é lido pela
    /// View (que o exibe). É o embrião do RelógioDoCombate do ADR-conceito-de-turno §7 (enrage /
    /// limite de turnos crescem daqui). NÃO confundir com o TurnoDoPersonagem (relógio POR
    /// combatente, dono do estado turn-scoped).
    /// </summary>
    public class RelogioDoCombate
    {
        public int NumeroDoTurno { get; private set; }

        /// <summary>Avança 1 turno. Chamado no início de cada turno jogado (ExecutarTurnoCompleto).</summary>
        public void Avancar() => NumeroDoTurno++;

        /// <summary>Zera pro início de uma nova batalha.</summary>
        public void Reiniciar() => NumeroDoTurno = 0;
    }
}
