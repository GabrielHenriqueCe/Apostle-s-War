namespace ApostlesWar
{
    #region Habilidade

    /// <summary>
    /// Classe base para todas as habilidades.
    /// Ativar recebe ContextoCombate (atacante + aliados + inimigos) e o alvo selecionado.
    /// </summary>
    abstract class Habilidade
    {
        public string Nome { get; }
        public int Turnos { get; }
        public string Descricao { get; }
        public string Simbolo { get; }

        public Habilidade(string nome, string simbolo, int turnos, string descricao = "")
        {
            Nome = nome;
            Simbolo = simbolo;
            Turnos = turnos;
            Descricao = descricao;
        }

        /// <summary>
        /// Executa a habilidade.
        /// ctx contém o atacante, aliados e inimigos.
        /// alvo é o alvo primário selecionado (jogador escolhe, ou aleatório pra inimigo).
        /// </summary>
        public abstract List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo);
    }

    #endregion
}