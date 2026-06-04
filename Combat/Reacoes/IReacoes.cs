namespace ApostlesWar
{
    /// <summary>
    /// Contexto passado para uma reação. Portador = quem reage; Outro = a outra
    /// parte (atacante, no lado do alvo; alvo, no lado do atacante); DanoCausado
    /// = dano efetivo que disparou; Natureza = natureza do golpe (pra reações
    /// consultarem TipoReacao, ex: ContraAtaque só reage a golpe Completa).
    /// </summary>
    record ContextoReacao(
        Combate Portador,
        Combate Outro,
        int DanoCausado,
        NaturezaDano Natureza
    );

    /// <summary>
    /// O que uma reação produziu, pro CombateService exibir. A reação DECLARA
    /// o que fez; o orquestrador EXIBE. A reação nunca chama MenuService direto.
    /// Dano: se a reação causou dano (revide, reflexo). Cura: se curou (Sedento).
    /// </summary>
    record ResultadoReacao(
        string Mensagem = "",
        ResultadoAtaque? Dano = null,
        int Cura = 0,
        Combate? RevidarAlvo = null   // se preenchido, o CombateService executa um revide neste alvo
    );

    /// <summary>
    /// Reage quando o portador é alvo de um ataque — dispara MESMO com dano 0
    /// (Escudo/Bloqueio absorveram). Reage ao ATO de ser atacado.
    /// Futuros implementadores: ContraAtaque, EspinhosVenenosos.
    /// </summary>
    interface IReageAoSerAtacado
    {
        List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx);
    }

    /// <summary>
    /// Reage quando o portador efetivamente PERDE HP (dano > 0). Não dispara se
    /// o dano foi todo absorvido. Reage ao SOFRIMENTO.
    /// Futuros implementadores: RefletirDano, Sangramento.
    /// </summary>
    interface IReageAoReceberDano
    {
        List<ResultadoReacao> AoReceberDano(ContextoReacao ctx);
    }

    /// <summary>
    /// Reage quando o portador CAUSA dano em alguém. Lado do atacante.
    /// Futuro implementador: Sedento.
    /// </summary>
    interface IReageAoCausarDano
    {
        List<ResultadoReacao> AoCausarDano(ContextoReacao ctx);
    }
}