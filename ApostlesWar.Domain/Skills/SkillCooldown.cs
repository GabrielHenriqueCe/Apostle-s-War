namespace ApostlesWar.Domain
{
    #region SkillCooldown

    /// <summary>
    /// Controla o cooldown de uma habilidade — quanto tempo falta para poder usar novamente
    /// </summary>
    public class SkillCooldown
    {
        public int CooldownRestante => cooldownRestante;
        public int CooldownTotal => cooldownTotal;
        private int cooldownRestante = 0;
        private int cooldownTotal;

        public SkillCooldown(int cooldown)
        {
            cooldownTotal = cooldown;
        }

        public bool Disponivel => cooldownRestante == 0;

        public void Usar()
        {
            cooldownRestante = cooldownTotal;
        }

        public void PassarTurno()
        {
            if (cooldownRestante > 0)
                cooldownRestante--;
        }
    }

    #endregion
}