namespace ApostlesWar
{
    #region SkillCooldown

    /// <summary>
    /// Controla o cooldown de uma habilidade — quanto tempo falta para poder usar novamente
    /// </summary>
    class SkillCooldown
    {
        public int TurnosRestantes => turnosRestantes;
        public int CooldownTotal => cooldownTotal;
        private int turnosRestantes = 0;
        private int cooldownTotal;

        public SkillCooldown(int cooldown)
        {
            cooldownTotal = cooldown;
        }

        public bool Disponivel => turnosRestantes == 0;

        public void Usar()
        {
            turnosRestantes = cooldownTotal;
        }

        public void PassarTurno()
        {
            if (turnosRestantes > 0)
                turnosRestantes--;
        }
    }

    #endregion
}