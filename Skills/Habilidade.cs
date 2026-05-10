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

        public abstract void Ativar(Combate alvo, List<Combate>? aliados = null);
    }

    #endregion
}