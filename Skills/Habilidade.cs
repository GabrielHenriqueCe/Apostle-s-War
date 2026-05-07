namespace ApostlesWar
{
    #region Habilidade

    /// <summary>
    /// Classe base para todas as habilidades — define nome, cooldown e o contrato de Ativar()
    /// </summary>
    abstract class Habilidade
    {
        public string Nome { get; }
        public int Turnos { get; }
        public string Descricao { get; }
        public SkillCooldown Cooldown { get; }

        public Habilidade(string nome, int turnos, string descricao = "")
        {
            Nome = nome;
            Turnos = turnos;
            Descricao = descricao;
            Cooldown = new SkillCooldown(turnos);
        }

        public abstract void Ativar(Combate alvo);
    }

    #endregion
}