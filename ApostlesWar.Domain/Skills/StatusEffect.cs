namespace ApostlesWar.Domain
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
    public abstract class StatusEffect : ElementoDeJogo
    {
        /// <summary>
        /// A duração de um status que NÃO ACABA (o Intocável do Fantasma, a Sentença do Vilão, a
        /// Couraça). Vale METADE do int.MaxValue, e a metade é o ponto: "permanente" precisa aguentar
        /// que alguém SOME turnos nela.
        ///
        /// Era `int.MaxValue` cru, e aí o <see cref="AumentarDuracao"/> — que o Raio-X do Robô chama
        /// pra ESTENDER benefícios — estourava o int e caía em negativo, o que o
        /// <see cref="Expirou"/> lê como acabado. A habilidade que promete prolongar o buff o APAGAVA,
        /// e só o permanente, que é justamente o que mais dói perder. Com folga de um bilhão de
        /// turnos, somar 1 é somar 1.
        ///
        /// O front mostra "∞" acima de um limiar bem menor (ver `duracaoTexto` no jogo.js), então a
        /// troca não muda nada na tela.
        /// </summary>
        public const int Permanente = int.MaxValue / 2;

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
