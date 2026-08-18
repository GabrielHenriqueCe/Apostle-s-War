namespace ApostlesWar.Domain
{
    /// <summary>
    /// COMO um status cobra de quem encosta no portador. Cada valor é uma forma de punição
    /// diferente, não um grau — quem decide o quanto teme cada uma é quem está escolhendo o alvo.
    /// </summary>
    public enum TipoDePunicao
    {
        /// <summary>Gruda status no agressor (Espinhos: Veneno + Queima). O custo PERSISTE.</summary>
        AplicaStatus,

        /// <summary>Devolve um golpe (Contra-Ataque: a A1 do portador). Custo imediato, uma vez.</summary>
        ContraAtaca,

        /// <summary>Devolve uma fração do dano sofrido (Reflexo). Proporcional ao que passou.</summary>
        RefleteDano,
    }

    /// <summary>
    /// Status que faz ATACAR O PORTADOR CUSTAR CARO — a informação que quem escolhe alvo precisa
    /// pra desviar.
    ///
    /// Não dava pra reaproveitar <c>IReageAoSerAtacado</c> pra isso: REAGIR não é PUNIR. A passiva do
    /// Ogro reage a ser atacada se fortalecendo, o que não custa nada a quem atacou; os Espinhos
    /// reagem envenenando, o que custa. As duas implementam a mesma interface de reação e querem
    /// respostas opostas de quem mira. Esta capacidade responde só a pergunta "me atacar dói?".
    ///
    /// Também não dava pra listar os tipos concretos no avaliador: a família cresce (o Gabriel já
    /// planeja mais formas de punição), e cada nova exigiria editar quem escolhe alvo — o
    /// acoplamento que o modelo de capacidades existe pra evitar.
    ///
    /// Implementadores hoje são todos BUFF, e isso é decisão de jogo: passiva é identidade
    /// permanente do apóstolo, e fugir dela deixaria Herói, Elfo, Zumbi e Cocô praticamente
    /// inatacáveis. Buff é temporário — desviar dele é tática, não paralisia.
    /// </summary>
    public interface IPuneQuemAtaca
    {
        TipoDePunicao Punicao { get; }
    }
}
