namespace ApostlesWar
{
    #region Habilidade

    /// <summary>
    /// Classe base para todas as habilidades.
    /// Ativar recebe atacante, alvo selecionado e lista completa (aliados ou inimigos).
    /// </summary>
    abstract class Habilidade
    {
        public string Nome { get; }
        public int Turnos { get; }
        public string Descricao { get; }
        public string Simbolo { get; }
        public SkillCooldown Cooldown { get; }

        public Habilidade(string nome, string simbolo, int turnos, string descricao = "")
        {
            Nome = nome;
            Simbolo = simbolo;
            Turnos = turnos;
            Descricao = descricao;
            Cooldown = new SkillCooldown(turnos);
        }

        /// <summary>
        /// Executa a habilidade.
        /// atacante = quem usou a habilidade
        /// alvo     = alvo primário selecionado pelo jogador/inimigo
        /// lista    = lista completa de defensores ou aliados (contexto da habilidade)
        /// </summary>
        public abstract List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista);
    }

    #endregion
}
