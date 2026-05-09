namespace ApostlesWar
{
    #region StatusEffect

    /// <summary>
    /// Classe base para todas os efeitos de status — define nome, cooldown e o contrato de Ativar()
    /// </summary>
    abstract class StatusEffect
    {
        public string Nome { get; }
        public int Turnos { get; }
        public string Descricao { get; }
        public int TurnosRestantes { get; private set; }

        public StatusEffect(string nome, int turnosRestantes, double valor, string descricao = "")
        {
            Nome = nome;
            Turnos = turnosRestantes;
            Descricao = descricao;
            TurnosRestantes = turnosRestantes;
        }

        public void PassarTurno() => TurnosRestantes--;
        public bool Expirou => TurnosRestantes <= 0;

        public abstract void Aplicar(Combate alvo);
        public abstract void Remover(Combate alvo);
    }

    #endregion
}