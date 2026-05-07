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
        public void Resetar()
        {
            this.turnosRestantes = 0;
        }

        /// <summary>
        /// Retorna o emoji de relógio proporcional ao progresso do cooldown
        /// </summary>
        public static string ObterRelogio(int turnosRestantes, int cooldownTotal)
        {
            if (turnosRestantes == 0) return "🕛";

            string[] relogios = { "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙" };
            int turnosPassados = cooldownTotal - turnosRestantes;
            int indice = (int)Math.Round((double)turnosPassados * 9 / cooldownTotal) - 1;
            indice = Math.Clamp(indice, 0, 8);
            return relogios[indice];
        }
    }

    #endregion
}