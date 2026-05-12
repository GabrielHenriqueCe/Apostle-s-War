namespace ApostlesWar
{
    #region StatusEffect

    /// <summary>
    /// Classe base para todos os efeitos de status (Buffs e Debuffs).
    /// Subclasses podem sobrescrever Aplicar para adicionar efeitos colaterais.
    /// </summary>
    abstract class StatusEffect
    {
        public string Nome { get; }
        public string Simbolo { get; }
        public int Turnos { get; }
        public string Descricao { get; }
        public double Valor { get; }
        public int TurnosRestantes { get; private set; }

        public StatusEffect(string nome, string simbolo, int turnosRestantes, double valor, string descricao = "")
        {
            Nome = nome;
            Simbolo = simbolo;
            Turnos = turnosRestantes;
            Valor = valor;
            Descricao = descricao;
            TurnosRestantes = turnosRestantes;
        }

        public void PassarTurno() => TurnosRestantes--;
        public bool Expirou => TurnosRestantes <= 0;
        public abstract void Remover(Combate alvo);

        /// <summary>
        /// Estende a duração do efeito em 1 turno. Usado pela PassivaPolicial.
        /// </summary>
        public void EstenderTurno() => TurnosRestantes++;

        /// <summary>
        /// Aplica o status ao alvo, gerenciando deduplicação por tipo.
        /// Subclasses que precisam de efeito colateral (modificar stat, etc.)
        /// devem sobrescrever este método e chamar base.Aplicar(alvo) ao final.
        /// </summary>
        public virtual void Aplicar(Combate alvo)
        {
            var existente = alvo.StatusAtivos.FirstOrDefault(s => s.GetType() == this.GetType());
            if (existente != null)
            {
                if (this.TurnosRestantes > existente.TurnosRestantes)
                    alvo.StatusAtivos.Remove(existente);
                else
                    return;
            }
            alvo.StatusAtivos.Add(this);
        }
    }

    #endregion
}
