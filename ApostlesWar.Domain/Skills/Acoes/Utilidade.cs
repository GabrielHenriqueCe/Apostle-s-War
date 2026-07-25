namespace ApostlesWar.Domain
{
    /// <summary>
    /// O tipo de bem que uma <see cref="Acao"/> faz. Serve a quem AVALIA uma habilidade sem
    /// executá-la — hoje o controlador automático, amanhã qualquer coisa que precise raciocinar
    /// sobre habilidades como dado (simulação em lote, dica de jogada, tutorial).
    ///
    /// É uma CLASSIFICAÇÃO, não uma escala: aqui não há "melhor" nem "pior". A ordem de preferência
    /// (curar antes de bater? limpar antes de buffar?) é decisão tática e vive no controlador, que
    /// pode mudá-la sem tocar em nenhuma ação. Separar o fato da opinião é o que deixa o domínio
    /// livre da estratégia — mesma divisão do <see cref="IPuneQuemAtaca"/>, onde o buff diz que TIPO
    /// de punição é e o controlador decide o quanto foge dela.
    /// </summary>
    public enum Utilidade
    {
        /// <summary>Tira vida do alvo (Dano, Explodir).</summary>
        Ferir,

        /// <summary>Devolve vida a quem está vivo (Cura).</summary>
        Curar,

        /// <summary>Traz um morto de volta (Reviver).</summary>
        Reviver,

        /// <summary>Dá algo bom a quem já tem (AplicarBuff, AplicarEscudo).</summary>
        Reforcar,

        /// <summary>Tira algo bom do outro (RemoverBuffs, MoverBuffs).</summary>
        TirarBuffs,

        /// <summary>Tira algo ruim do próprio lado (RemoverDebuffs).</summary>
        LimparDebuffs,

        /// <summary>Dá algo ruim ao outro (AplicarDebuff).</summary>
        Enfraquecer,

        /// <summary>Joga de novo (ConcederTurnoExtra).</summary>
        TurnoExtra,

        /// <summary>
        /// O PREÇO da habilidade, não o ganho — dano ou perda que o conjurador paga pra usá-la
        /// (AutoDano do Fantasma). Existe pra que uma ação assim não se disfarce de benefício: se
        /// fosse classificada como <see cref="Ferir"/>, quem avalia leria "esta habilidade causa
        /// dano" e a usaria pelo motivo errado. Habilidade com custo é julgada pelo que ela ENTREGA.
        /// </summary>
        Custo,
    }
}
