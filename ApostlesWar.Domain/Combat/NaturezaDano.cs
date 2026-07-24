namespace ApostlesWar
{
    /// <summary>
    /// Quanto de reação um dano provoca em quem o recebe.
    /// Completa: dispara tudo (contra-ataque, reflexo, espinhos, passivas). Ataque normal.
    /// Nenhuma: não dispara reação alguma. Dano que é consequência de uma ação já
    ///   resolvida (auto-dano, redirecionamento, reflexo).
    /// </summary>
    enum TipoReacao { Completa, Nenhuma }

    /// <summary>
    /// Descreve como uma instância de dano se comporta: o que ela ignora e se provoca
    /// retaliação de contra-ataque. Não executa nada — é só a descrição das regras;
    /// o ReceberDano consulta esses campos pra decidir o que aplicar.
    ///
    /// "Ignorar status" tem UMA língua só (unificação jul/2026): a lista `Ignora` de tipos de
    /// StatusEffect que o dano fura — é a MESMA gramática de `ignorarStatus` (por golpe) e
    /// `IIgnoraStatusNoAtaque` (por champ). O ReceberDano une as três numa lista e cada status
    /// pergunta "fui listado?" (não há mais `DeveAgir` por-status lendo flags). `IgnoraDefesa`
    /// segue bool porque DEFESA é um STAT, não um status na lista.
    ///
    /// Use os perfis prontos em NaturezasDano em vez de montar manualmente.
    /// </summary>
    record NaturezaDano(
        bool IgnoraDefesa = false,
        TipoReacao Reacao = TipoReacao.Completa)
    {
        /// <summary>Tipos de StatusEffect que este dano fura no cálculo (match por tipo EXATO ou
        /// BASE — typeof(Buff) furaria todos os buffs). Nunca nulo; default vazio.</summary>
        public IReadOnlyCollection<Type> Ignora { get; init; } = [];
    }

    /// <summary>
    /// Perfis prontos de NaturezaDano pros casos do jogo. Centraliza as regras
    /// pra ninguém montar combinação errada na chamada.
    /// </summary>
    static class NaturezasDano
    {
        /// Ataque normal: passa por defesa/escudo/bloqueio, dispara todas as reações.
        /// Usado também pro revide (contra-ataque) — mecanicamente é um ataque igual
        /// qualquer outro; o loop A↔B é quebrado por profundidade no executor, não
        /// por uma Natureza especial (ver ProcessarReacoesAlvo).
        public static readonly NaturezaDano Ataque = new();

        /// Veneno (tick e explosão): ignora defesa; escudo absorve, bloqueio bloqueia.
        /// Não dispara reação (é dano de status, não um ataque) — por isso NÃO é
        /// redirecionado pela ProtecaoAliado (na lista Ignora).
        public static readonly NaturezaDano Veneno = new(IgnoraDefesa: true, Reacao: TipoReacao.Nenhuma)
        {
            Ignora = [typeof(Skills.Buffs.ProtecaoAliado)]
        };

        /// Queima (dano à vida): ignora defesa e escudo; bloqueio ainda bloqueia.
        /// Não dispara reação (não redireciona pela ProtecaoAliado).
        public static readonly NaturezaDano QueimaDano = new(IgnoraDefesa: true, Reacao: TipoReacao.Nenhuma)
        {
            Ignora = [typeof(Skills.Buffs.Escudo), typeof(Skills.Buffs.ProtecaoAliado)]
        };

        /// Dano indireto: consequência de uma ação já resolvida (auto-dano do Fantasma,
        /// redirecionamento da Proteção, reflexo). Passa por defesa, escudo e bloqueio
        /// normalmente, mas NÃO dispara reação. A ProtecaoAliado na lista Ignora é o que
        /// corta o loop de proteção mútua A↔B (o redirect usa DanoIndireto → o 2º
        /// ProtecaoAliado é pulado → não re-redireciona).
        public static readonly NaturezaDano DanoIndireto = new(Reacao: TipoReacao.Nenhuma)
        {
            Ignora = [typeof(Skills.Buffs.ProtecaoAliado)]
        };
    }
}
