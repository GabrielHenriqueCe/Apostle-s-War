using System.ComponentModel;

namespace ApostlesWar
{
    #region Enums

    enum Faccao
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

    enum Slot { Slot1 = 1, Slot2 = 2, Slot3 = 3, Slot4 = 4 }

    enum Fases
    {
        [Description("Arma")] Fase1 = 1,
        [Description("Elmo")] Fase2 = 2,
        [Description("Escudo")] Fase3 = 3,
        [Description("Manopla")] Fase4 = 4,
        [Description("Peitoral")] Fase5 = 5,
        [Description("Calça")] Fase6 = 6,
        [Description("Bota")] Fase7 = 7
    }

    enum EventoCombate { AntesDeReceberDano, DepoisDeReceberDano, DepoisDeAtacar }

    enum SimOuNao
    {
        [Description("Sim")] Sim = 1,
        [Description("Não")] Nao = 2
    }

    enum OpcoesMenu
    {
        [Description("📜 - Jogar Campanha")] JogarCampanha = 1,
        [Description("💰 - Inventário")] Inventario = 2
    }

    enum TipoStat { ATKFlat, HPFlat, DEFFlat, HPPct, DEFPct, TaxaCritPct, DanoCritPct }

    /// <summary>
    /// Define como os alvos adicionais são selecionados.
    /// Explicito: selecionado + próximos em ordem, sem repetição.
    /// Aleatorio: selecionado + demais sorteados, com repetição permitida.
    /// </summary>
    enum TipoAlvo { Explicito, Aleatorio }

    /// <summary>
    /// Define em qual lista a habilidade age.
    /// Inimigos: age nos defensores (requer seleção de alvo).
    /// Aliados: age no próprio time.
    /// Self: age apenas no próprio atacante.
    /// </summary>
    enum TipoLista { Inimigos, Aliados, Self }

    #endregion
}
