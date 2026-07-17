namespace ApostlesWar
{
    /// <summary>
    /// Contexto passado para uma reação. Portador = quem reage; Contraparte = a outra
    /// parte (atacante, no lado do alvo; alvo, no lado do atacante); DanoCausado
    /// = dano efetivo que disparou; Natureza = natureza do golpe; FoiCritico = se
    /// o golpe foi crítico (já vem correto da fonte — golpe que não causou dano não
    /// é crítico); Aliados/Inimigos = times do PORTADOR.
    /// </summary>
    record ContextoReacao(
        Combate Portador,
        Combate Contraparte,
        int DanoCausado,
        NaturezaDano Natureza,
        bool FoiCritico,
        List<Combate> Aliados,
        List<Combate> Inimigos
    );

    /// <summary>
    /// O que uma reação produziu, pro CombateService exibir. A reação DECLARA
    /// o que fez; o orquestrador EXIBE. A reação nunca chama MenuService direto.
    /// Dano: se a reação causou dano (reflexo). Cura: se curou (Sedento).
    /// Revide: se a reação declarou um contra-ataque (ContraAtaque, Operário) —
    /// carrega QUAL habilidade usar, não executa nada aqui.
    /// </summary>
    record ResultadoReacao(
        string Mensagem = "",
        EventoDano? Dano = null,
        int Cura = 0,
        Revide? Revide = null
    );

    /// <summary>
    /// Declaração de um contra-ataque: qual habilidade usar e contra quem.
    /// A reação que declara (ContraAtaque, InstintoDoOperario) escolhe a Habilidade
    /// (A1 por padrão, Marretada no caso do Operário); o CombateService a
    /// executa polimorficamente via IAtivavelComNatureza, sem saber qual skill é.
    /// </summary>
    record Revide(IAtivavelComNatureza Habilidade, Combate Alvo);

    /// <summary>
    /// ISP: habilidades que podem ser usadas como um contra-ataque — precisam
    /// atacar UM alvo explícito com uma NaturezaDano escolhida por quem chama
    /// (em vez da Natureza default da Ativar normal). Só faz sentido pra
    /// habilidades de alvo único (A1, Marretada); uma AoE não implementa.
    /// </summary>
    interface IAtivavelComNatureza
    {
        EventoDano AtivarComNatureza(Combate atacante, Combate alvo, NaturezaDano natureza);
    }

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

    /// <summary>
    /// Reage ao evento de atacar, seguindo o TipoAtaque (AoE = 1x; Sequencial = por hit).
    /// Para efeitos que beneficiam o PRÓPRIO atacante. Implementadores: OlhoClinico, Virus.
    /// </summary>
    interface IReageAoAtacar
    {
        List<ResultadoReacao> AoAtacar(ContextoReacao ctx);
    }

    /// <summary>
    /// Reage POR ALVO atingido (Nx, sempre). Para efeitos aplicados em CADA alvo do golpe.
    /// Implementadores: Sorrateiro, Policial.
    /// </summary>
    interface IReagePorAtaque
    {
        List<ResultadoReacao> PorAtaque(ContextoReacao ctx);
    }

    /// <summary>
    /// Reage quando o portador MATA um alvo (o golpe reduziu o alvo a 0 HP).
    /// Dispara por alvo morto (numa AoE que mata vários, dispara por cada morto).
    /// Portador = quem matou; Contraparte = o morto.
    /// IMPORTANTE: roda DEPOIS de IReageAntesDeMorrer (Guarda) e ANTES de
    /// IReageAoMorrer (Necromancia). Se a Guarda reverteu a morte, este disparo
    /// não ocorre (alvo voltou a EstaVivo).
    /// Implementadores: Fada, Vilao.
    /// </summary>
    interface IReageAoMatar
    {
        List<ResultadoReacao> AoMatar(ContextoReacao ctx);
    }

    /// <summary>
    /// Intervém ANTES das consequências da morte (antes de IReageAoMatar e IReageAoMorrer).
    /// O portador chegou a 0 HP e está em estado Morto; esta interface pode REVERTER
    /// a transição chamando AplicarRevive — mantendo o personagem como Vivo, impedindo
    /// que o Vilão perceba a morte e que a Necromância tente reviver.
    /// Portador = quem quase morreu; Contraparte = quem matou.
    /// Implementador: GuardaReal (passiva do Guarda).
    /// </summary>
    interface IReageAntesDeMorrer
    {
        List<ResultadoReacao> AntesDeMorrer(ContextoReacao ctx);
    }

    /// <summary>
    /// Reage quando o portador MORRE (HP chegou a 0 por um golpe). Pós-morte.
    /// Dispara DEPOIS do IReageAoMatar — o Vilao já bloqueou o revive, se for o caso.
    /// O portador pode tentar reviver (Necromancia), respeitando PodeReviver.
    /// Portador = quem morreu; Contraparte = quem matou.
    /// Implementador: Necromancia.
    /// </summary>
    interface IReageAoMorrer
    {
        List<ResultadoReacao> AoMorrer(ContextoReacao ctx);
    }

    /// <summary>
    /// Reage no INÍCIO do turno do portador. Não há golpe (sem dano, sem Contraparte) —
    /// recebe o ContextoCombate (Atacante = portador, Aliados, Inimigos). Para efeitos
    /// que o portador renova/aplica a cada turno (RefletirDano do Genio, BuffAtaque do
    /// Tengu, cleanse do BonecoDeNeve).
    /// Implementadores: Genio, BonecoDeNeve, Tengu, Elfo.
    /// </summary>
    interface IReageAoInicioTurno
    {
        List<ResultadoReacao> AoInicioTurno(ContextoCombate ctx);
    }
}