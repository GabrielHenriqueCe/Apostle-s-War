namespace ApostlesWar
{
    #region StatusEffect

    /// <summary>
    /// Classe base para todos os efeitos de status (Buffs e Debuffs).
    /// Subclasses podem sobrescrever Aplicar para efeitos colaterais ao aplicar,
    /// e ModificarDanoRecebido para alterar o dano que o portador recebe.
    /// </summary>
    abstract class StatusEffect
    {
        public string Nome { get; }
        public string Simbolo { get; }
        public int Turnos { get; }
        public string Descricao { get; }
        public double Valor { get; }
        public int TurnosRestantes { get; private set; }
        public bool AcabouDeAplicar { get; private set; }

        public StatusEffect(string nome, string simbolo, int turnosRestantes, double valor, string descricao = "")
        {
            Nome = nome;
            Simbolo = simbolo;
            Turnos = turnosRestantes;
            Valor = valor;
            Descricao = descricao;
            TurnosRestantes = turnosRestantes;
            AcabouDeAplicar = true;
        }

        public void PassarTurno()
        {
            if (AcabouDeAplicar)
            {
                AcabouDeAplicar = false;
                return;
            }
            TurnosRestantes--;
        }

        public bool Expirou => TurnosRestantes <= 0;
        public abstract void Remover(Combate alvo);
        public void EstenderTurno() => TurnosRestantes++;

        /// <summary>
        /// Sobrescreva para alterar o dano recebido pelo portador.
        /// Recebe o dano calculado (após defesa) e retorna o dano final aplicado.
        /// Ex: BloqueioTotal retorna 0; Invencivel limita HP mínimo a 1.
        /// </summary>
        public virtual int ModificarDanoRecebido(Combate portador, int dano) => dano;

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
