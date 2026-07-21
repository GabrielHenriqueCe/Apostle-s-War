namespace ApostlesWar
{
    #region Habilidade

    /// <summary>
    /// Classe base para todas as habilidades. A identidade (Nome/Simbolo/Descricao) vem
    /// de ElementoDeJogo; aqui mora o Cooldown (turnos de recarga entre usos).
    /// Ativar recebe ContextoCombate (atacante + aliados + inimigos) e o alvo selecionado.
    /// </summary>
    abstract class Habilidade : ElementoDeJogo
    {
        public int Cooldown { get; }

        public Habilidade(string nome, string simbolo, int cooldown, string descricao = "")
            : base(nome, simbolo, descricao)
        {
            Cooldown = cooldown;
        }

        /// <summary>
        /// Executa a habilidade.
        /// ctx contém o atacante, aliados e inimigos.
        /// alvo é o alvo primário selecionado (jogador escolhe, ou aleatório pra inimigo).
        /// </summary>
        public abstract List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo);
    }

    #endregion
}
