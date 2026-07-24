using System.ComponentModel;

namespace ApostlesWar.Domain
{
    #region Enums

    public enum Faccao
    {
        [Description("Humanos")] Humanos,
        [Description("Reino")] Reino,
        [Description("Lado Sombrio")] LadoSombrio,
        [Description("Tecnológicos")] Tecnologicos,
        [Description("Folclore")] Folclore,
        [Description("Místicos")] Misticos,
        [Description("Especial")] Especial,
        [Description("Decaídos")] Decaidos,
        [Description("Apóstolos")] Apostolos
    }

    public enum Slot { Slot1 = 1, Slot2 = 2, Slot3 = 3, Slot4 = 4 }

    public enum Fases
    {
        [Description("Arma")] Fase1 = 1,
        [Description("Elmo")] Fase2 = 2,
        [Description("Escudo")] Fase3 = 3,
        [Description("Manopla")] Fase4 = 4,
        [Description("Peitoral")] Fase5 = 5,
        [Description("Calça")] Fase6 = 6,
        [Description("Bota")] Fase7 = 7
    }

    public enum SimOuNao
    {
        [Description("Sim")] Sim = 1,
        [Description("Não")] Nao = 2
    }

    public enum TipoStat { ATKFlat, HPFlat, DEFFlat, HPPct, DEFPct, TaxaCritPct, DanoCritPct }

    /// <summary>
    /// Define como os alvos adicionais são selecionados.
    /// Explicito: selecionado + próximos em ordem, sem repetição.
    /// Aleatorio: selecionado + demais sorteados, com repetição permitida.
    /// </summary>
    public enum TipoAlvo { Explicito, Aleatorio }

    /// <summary>
    /// Define em qual lista a habilidade age.
    /// Inimigos: age nos defensores (requer seleção de alvo).
    /// Aliados: age no próprio time.
    /// Self: age apenas no próprio atacante.
    /// </summary>
    public enum TipoLista { Inimigos, Aliados, Self }

    /// <summary>
    /// Define qual estado de vida a habilidade mira dentro da TipoLista escolhida.
    /// Vivos: maioria (ataques, curas, buffs). Mortos: revive de N fixo (seleção
    /// única sobre os mortos). Uma habilidade que "mira dois estados" (ex: revive mortos
    /// E buffa vivos) NÃO usa um valor especial — é só duas ações com EstadoAlvo diferente
    /// na mesma lista (cada ação filtra o seu estado na execução). Por isso o antigo `Ambos`
    /// morreu (jul/2026): quando o sweep migrou o último cliente — o Céu — ele deixou de existir.
    /// </summary>
    public enum EstadoAlvo { Vivos, Mortos }

    /// <summary>
    /// Em quais combatentes uma AÇÃO age (eixo da composição — ver ADR-composicao-de-acoes §5.2).
    /// Separado do TipoLista da habilidade (que manda no menu de alvo). O interpretador resolve
    /// o conjunto por ação e filtra pelo EstadoAlvo da própria ação, na hora de executar.
    /// - AlvosResolvidos: herda os alvos que a habilidade selecionou.
    /// - TodosAliados: o time do atacante (INCLUI o próprio atacante).
    /// - TodosInimigos: o time oposto.
    /// - ProprioAtacante: só o conjurador.
    /// - OutrosAliados: o time do atacante EXCETO o próprio atacante (ex: OssoDuroDeRoer,
    ///   Circo — ADR-composicao-de-acoes §5.2).
    /// </summary>
    public enum Escopo { AlvosResolvidos, TodosAliados, TodosInimigos, ProprioAtacante, OutrosAliados }

    /// <summary>
    /// Define a semântica do evento de ataque, usada pelo CombateService
    /// para decidir quantas vezes disparar as passivas reativas do atacante (DepoisDeAtacar).
    /// 
    /// AreaDeEfeito: conceitualmente UM evento que atinge vários. Passiva dispara 1x.
    ///   Ex: Sopro do Dragão, Incêndio, Pó Mágico.
    /// Sequencial: N ataques separados, cada um é um evento completo. Passiva dispara Nx.
    ///   Ex: Ataque Básico, Tiroteio, Porradeiro, Shuriken.
    /// NaoAtaque: habilidade que não causa dano. Passiva DepoisDeAtacar não dispara.
    ///   Ex: Democracia (cura), Lealdade (escudo), Abduzir (só Preso).
    /// 
    /// Nota: DepoisDeMatar dispara por alvo morto independente deste tipo —
    /// só DepoisDeAtacar é afetado por AreaDeEfeito vs Sequencial.
    /// </summary>
    public enum TipoAtaque { AreaDeEfeito, Sequencial, NaoAtaque }

    /// <summary>
    /// Desfecho de uma tentativa de fase. Distingue os três "não-vitória": Perdeu (batalha perdida
    /// ou encerrada no meio → derrota) vs Cancelou (jogador desistiu na seleção de time, ANTES da
    /// luta → sem derrota, volta silencioso). Consumido pelo GerenciadorDeJogo pra decidir a tela.
    /// </summary>
    public enum ResultadoFase { Venceu, Perdeu, Cancelou }

    #endregion
}
