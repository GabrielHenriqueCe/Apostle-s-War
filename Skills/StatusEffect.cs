namespace ApostlesWar
{
    #region StatusEffect

    /// <summary>
    /// Classe base para todos os efeitos de status (Buffs e Debuffs).
    /// Hooks disponíveis pra sobrescrever:
    /// - Aplicar: efeitos colaterais ao aplicar
    /// - Remover: limpeza ao expirar
    /// - AoIniciarTurno: efeitos no início do turno do portador (Veneno, CuraContinua)
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

        /// <summary>
        /// Se false, o status não pode ser removido/roubado por efeitos de terceiros
        /// (Copiando, PassivaCientista, cleanses de buff). Continua expirando por
        /// duração normalmente — não é sobre imunidade a tempo, é sobre imunidade a
        /// remoção externa. Usado pelo Fantasma (Intocável permanente).
        /// </summary>
        public bool Removivel { get; }

        public StatusEffect(string nome, string simbolo, int turnosRestantes, double valor, string descricao = "", bool removivel = true)
        {
            Nome = nome;
            Simbolo = simbolo;
            Turnos = turnosRestantes;
            Valor = valor;
            Descricao = descricao;
            TurnosRestantes = turnosRestantes;
            AcabouDeAplicar = true;
            Removivel = removivel;
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

        public void AumentarDuracao(int turnos) => TurnosRestantes += turnos;
        public void ReduzirDuracao(int turnos) => TurnosRestantes = Math.Max(0, TurnosRestantes - turnos);
        public virtual void AoIniciarTurno(Combate portador) { }


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