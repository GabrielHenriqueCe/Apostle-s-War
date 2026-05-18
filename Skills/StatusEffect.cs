namespace ApostlesWar
{
    #region StatusEffect

    /// <summary>
    /// Classe base para todos os efeitos de status (Buffs e Debuffs).
    /// Subclasses podem sobrescrever:
    /// - Aplicar: efeitos colaterais ao aplicar
    /// - ModificarDanoRecebido: alterar o dano que o portador recebe
    /// - Bloqueia: bloquear a entrada de outros status
    /// - AoIniciarTurno: efeitos no início do turno do portador (ex: Veneno)
    /// </summary>
    abstract class StatusEffect
    {
        public string Nome { get; }
        public string Simbolo { get; }
        public int Turnos { get; }
        public string Descricao { get; }
        public double Valor { get; }
        public int TurnosRestantes { get; protected set; }
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
        /// </summary>
        public virtual int ModificarDanoRecebido(Combate portador, int dano) => dano;

        /// <summary>
        /// Sobrescreva pra bloquear a entrada de outros status no mesmo portador.
        /// Ex: ImpedirBeneficios retorna true se o novo status for Buff.
        /// </summary>
        public virtual bool Bloqueia(StatusEffect novo) => false;

        /// <summary>
        /// Hook executado no início do turno do portador, antes da escolha de ação.
        /// Usado por Veneno (causa dano) e similares.
        /// </summary>
        public virtual void AoIniciarTurno(Combate portador) { }

        public virtual void Aplicar(Combate alvo)
        {
            // Porteiro de status — algum ativo bloqueia este?
            if (!alvo.PodeReceber(this)) return;

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
