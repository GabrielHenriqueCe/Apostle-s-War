namespace ApostlesWar
{
    #region HabilidadeAtiva

    /// <summary>
    /// Habilidade ativada manualmente pelo jogador no seu turno
    /// </summary>
    abstract class HabilidadeAtiva : Habilidade
    {
        public HabilidadeAtiva(string nome, int turnos, string descricao = "") : base(nome, turnos, descricao) { }
    }

    #endregion
}