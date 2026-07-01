namespace ApostlesWar
{
    /// <summary>
    /// Quanto de reação um dano provoca em quem o recebe.
    /// Completa: dispara tudo (contra-ataque, reflexo, espinhos, passivas). Ataque normal.
    /// Nenhuma: não dispara reação alguma. Dano que é consequência de uma ação já
    ///   resolvida (auto-dano, redirecionamento, reflexo) — só Escudo/BloqueioTotal agem.
    /// </summary>
    enum TipoReacao { Completa, Nenhuma }

    /// <summary>
    /// Descreve como uma instância de dano se comporta: o que ela ignora
    /// (defesa, escudo, bloqueio) e se provoca retaliação de contra-ataque.
    /// 
    /// Não executa nada — é só a descrição das regras. O ReceberDano consulta
    /// esses campos pra decidir o que aplicar.
    /// 
    /// Use os perfis prontos em NaturezasDano em vez de montar manualmente.
    /// </summary>
    record NaturezaDano(
        bool IgnoraDefesa = false,
        bool IgnoraEscudo = false,
        bool IgnoraBloqueio = false,
        TipoReacao Reacao = TipoReacao.Completa
    );

    /// <summary>
    /// Perfis prontos de NaturezaDano pros casos do jogo. Centraliza as regras
    /// pra ninguém montar combinação errada de flags na chamada.
    /// </summary>
    static class NaturezasDano
    {
        /// Ataque normal: passa por defesa/escudo/bloqueio, dispara todas as reações.
        /// Usado também pro revide (contra-ataque) — mecanicamente é um ataque igual
        /// qualquer outro; o loop A↔B é quebrado por profundidade no executor, não
        /// por uma Natureza especial (ver ProcessarReacoesAlvo).
        public static readonly NaturezaDano Ataque = new();

        /// Veneno (tick e explosão): ignora defesa; escudo absorve, bloqueio bloqueia.
        /// Não dispara reação (é dano de status, não um ataque).
        public static readonly NaturezaDano Veneno = new(IgnoraDefesa: true, Reacao: TipoReacao.Nenhuma);

        /// Queima (dano à vida): ignora defesa e escudo; bloqueio ainda bloqueia.
        /// Não dispara reação.
        public static readonly NaturezaDano QueimaDano = new(IgnoraDefesa: true, IgnoraEscudo: true, Reacao: TipoReacao.Nenhuma);

        /// Dano indireto: consequência de uma ação já resolvida (auto-dano do
        /// Fantasma, redirecionamento da Proteção, reflexo). Passa por defesa,
        /// escudo e bloqueio normalmente, mas NÃO dispara reação alguma.
        public static readonly NaturezaDano DanoIndireto = new(Reacao: TipoReacao.Nenhuma);

        /// Direto puro: ignora defesa, escudo e bloqueio. Nada segura. Sem reação.
        public static readonly NaturezaDano Direto = new(IgnoraDefesa: true, IgnoraEscudo: true, IgnoraBloqueio: true, Reacao: TipoReacao.Nenhuma);
    }
}