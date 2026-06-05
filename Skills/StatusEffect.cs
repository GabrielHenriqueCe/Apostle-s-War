namespace ApostlesWar
{
    #region StatusEffect

    /// <summary>
    /// Classe base para todos os efeitos de status (Buffs e Debuffs).
    /// Hooks disponíveis pra sobrescrever:
    /// - Aplicar: efeitos colaterais ao aplicar
    /// - Remover: limpeza ao expirar
    /// - AoIniciarTurno: efeitos no início do turno do portador (Veneno, CuraContinua)
    /// - AoPassarTurno: hook depois de PassarTurno (resetar CDs internos)
    /// - ContribuicaoDefesa: quanto este status soma na DEF do portador
    ///   (usado quando habilidade decide ignorar buffs específicos do alvo)
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
        public virtual void AoIniciarTurno(Combate portador) { }

        /// <summary>
        /// Hook chamado depois que PassarTurno foi executado.
        /// </summary>
        protected virtual void AoPassarTurno() { }

        /// <summary>
        /// Quanto este status soma (positivo) ou subtrai (negativo) na DEF do portador.
        /// Usado quando um ataque decide ignorar buffs específicos — a contribuição é
        /// subtraída da DEF efetiva no cálculo do dano. Default: 0 (não afeta DEF).
        /// </summary>
        public virtual int ContribuicaoDefesa(Combate portador) => 0;

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