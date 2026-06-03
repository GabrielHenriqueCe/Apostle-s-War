namespace ApostlesWar
{
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
        bool EhRevide = false
    );

    /// <summary>
    /// Perfis prontos de NaturezaDano pros casos do jogo. Centraliza as regras
    /// pra ninguém montar combinação errada de flags na chamada.
    /// </summary>
    static class NaturezasDano
    {
        /// Ataque normal: defesa reduz, escudo absorve, bloqueio bloqueia, provoca reações.
        public static readonly NaturezaDano Ataque = new();

        /// Revide (contra-ataque): ataque gerado por retaliação. Comporta-se como
        /// ataque normal (é refletido pelo alvo, dispara reações), EXCETO que não
        /// provoca OUTRO contra-ataque de volta — é o que quebra o loop A↔B.
        /// O RefletirDano ignora EhRevide e reflete o revide normalmente.
        public static readonly NaturezaDano Revide = new(EhRevide: true);

        /// Veneno (tick e explosão): ignora defesa. Escudo absorve, bloqueio bloqueia.
        public static readonly NaturezaDano Veneno = new(IgnoraDefesa: true);

        /// Queima (dano à vida, tick e explosão): ignora defesa e escudo.
        /// Bloqueio AINDA bloqueia. (A redução de HP máximo é tratada à parte
        /// via ReduzirHPMaximo e ignora tudo.)
        public static readonly NaturezaDano QueimaDano = new(IgnoraDefesa: true, IgnoraEscudo: true);

        /// Direto puro: ignora defesa, escudo e bloqueio. Nada segura.
        public static readonly NaturezaDano Direto = new(IgnoraDefesa: true, IgnoraEscudo: true, IgnoraBloqueio: true);
    }
}