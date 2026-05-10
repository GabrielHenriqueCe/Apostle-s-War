namespace ApostlesWar
{
    #region HabilidadeAtiva

    /// <summary>
    /// Habilidade ativada manualmente pelo jogador no seu turno
    /// </summary>
    abstract class HabilidadeAtiva : Habilidade
    {
        public HabilidadeAtiva(string nome, string simbolo, int turnos, string descricao = "")
            : base(nome, simbolo, turnos, descricao) { }
        public abstract int NumeroDeAlvos { get; }
    }

    #endregion
}