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

        /// <summary>
        /// Os ciclos de combate somados na FASE inteira — as duas rodadas de inimigos juntas.
        ///
        /// É separado do <see cref="NumeroDoTurno"/> porque atravessa o <see cref="Reiniciar"/>: a
        /// fase reinicia o relógio a cada onda (cada onda é uma batalha), mas o que paga o nível do
        /// item é a fase inteira — o teto do GDD é "por batalha" no sentido do jogador, que é a fase.
        /// Quem zera isto é o ponto de entrada da fase, não a onda.
        /// </summary>
        public double CiclosDaFase { get; private set; }

        /// <summary>Avança 1 turno. Chamado no início de cada turno jogado (ExecutarTurnoCompleto).</summary>
        public void Avancar() => NumeroDoTurno++;

        /// <summary>Soma os ciclos de uma onda ao total da fase. Ver <see cref="CiclosDaFase"/>.</summary>
        public void AcumularCiclos(double ciclos) => CiclosDaFase += ciclos;

        /// <summary>Zera pro início de uma nova batalha (ONDA). Não toca nos ciclos da fase.</summary>
        public void Reiniciar() => NumeroDoTurno = 0;

        /// <summary>Zera o acumulado da fase. Chamado pelo ponto de entrada da fase.</summary>
        public void ReiniciarFase() => CiclosDaFase = 0;
    }
}
