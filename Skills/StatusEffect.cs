namespace ApostlesWar
{
    #region StatusEffect

    /// <summary>
    /// Classe base para todos os efeitos de status (Buffs e Debuffs). A identidade
    /// (Nome/Simbolo/Descricao) vem de ElementoDeJogo; aqui mora a DuracaoRestante
    /// (quantos turnos o efeito ainda dura).
    /// Hooks disponíveis pra sobrescrever:
    /// - Aplicar: efeitos colaterais ao aplicar
    /// - Remover: limpeza ao expirar
    /// - AoIniciarTurno: efeitos no início do turno do portador (Veneno, CuraContinua)
    /// </summary>
    abstract class StatusEffect : ElementoDeJogo
    {
        public double Valor { get; }
        public int DuracaoRestante { get; protected set; }
        public bool AcabouDeAplicar { get; private set; }

        /// <summary>
        /// Se false, o status não pode ser removido/roubado por efeitos de terceiros
        /// (Copiando, AnaliseCritica, cleanses de buff). Continua expirando por
        /// duração normalmente — não é sobre imunidade a tempo, é sobre imunidade a
        /// remoção externa. Usado pelo Fantasma (Intocável permanente).
        /// </summary>
        public bool Removivel { get; }

        public StatusEffect(string nome, string simbolo, int duracao, double valor, string descricao = "", bool removivel = true)
            : base(nome, simbolo, descricao)
        {
            Valor = valor;
            DuracaoRestante = duracao;
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
            DuracaoRestante--;
        }

        public bool Expirou => DuracaoRestante <= 0;
        public abstract void Remover(Combate alvo);

        public void AumentarDuracao(int turnos) => DuracaoRestante += turnos;
        public void ReduzirDuracao(int turnos) => DuracaoRestante = Math.Max(0, DuracaoRestante - turnos);
        /// <summary>
        /// Tick de início do turno do portador. Devolve o EventoCombate do que aconteceu (dano do
        /// Veneno/Queima, cura da CuraContinua) pro combate EXIBIR, ou null se o tick não é visível
        /// (ex: Maldição só reduz HP máximo). O null morre na porta (TurnoDoPersonagem.Iniciar filtra).
        /// </summary>
        public virtual EventoCombate? AoIniciarTurno(Combate portador) => null;


        public virtual void Aplicar(Combate alvo)
        {
            if (!alvo.PodeReceber(this)) return;

            var existente = alvo.StatusAtivos.FirstOrDefault(s => s.GetType() == this.GetType());
            if (existente != null)
            {
                if (this.DuracaoRestante > existente.DuracaoRestante)
                    alvo.StatusAtivos.Remove(existente);
                else
                    return;
            }
            alvo.StatusAtivos.Add(this);
        }

    }

    #endregion
}
