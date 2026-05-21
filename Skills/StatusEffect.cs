namespace ApostlesWar
{
    #region StatusEffect

    /// <summary>
    /// Classe base para todos os efeitos de status (Buffs e Debuffs).
    /// Subclasses podem sobrescrever:
    /// - Aplicar: efeitos colaterais ao aplicar
    /// - ModificarDanoRecebido: alterar o dano recebido pelo portador
    /// - Bloqueia: bloquear a entrada de outros status
    /// - AoIniciarTurno: efeitos no início do turno do portador (Veneno, CuraContinua)
    /// - AoReceberDano: reação a dano recebido (ContraAtaque)
    /// - AoPassarTurno: hook depois de PassarTurno (resetar CDs internos, etc)
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
                AoPassarTurno();
                return;
            }
            TurnosRestantes--;
            AoPassarTurno();
        }

        public bool Expirou => TurnosRestantes <= 0;
        public abstract void Remover(Combate alvo);
        public void EstenderTurno() => TurnosRestantes++;

        public void AumentarDuracao(int turnos) => TurnosRestantes += turnos;
        public void ReduzirDuracao(int turnos) => TurnosRestantes = Math.Max(0, TurnosRestantes - turnos);

        public virtual int ModificarDanoRecebido(Combate portador, int dano) => dano;
        public virtual bool Bloqueia(StatusEffect novo) => false;
        public virtual void AoIniciarTurno(Combate portador) { }

        /// <summary>
        /// Hook chamado depois que o portador recebe dano.
        /// Usado por ContraAtaque (e outros futuros).
        /// </summary>
        public virtual void AoReceberDano(Combate portador, Combate atacante) { }

        /// <summary>
        /// Hook chamado depois que PassarTurno foi executado.
        /// Usado por ContraAtaque pra resetar seu CD interno.
        /// </summary>
        protected virtual void AoPassarTurno() { }

        public virtual void Aplicar(Combate alvo)
        {
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