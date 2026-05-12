using System.ComponentModel;

namespace ApostlesWar
{
    #region Enums

    // Facções disponíveis no jogo
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

    // Posição do personagem dentro da facção
    enum Slot { Slot1 = 1, Slot2 = 2, Slot3 = 3, Slot4 = 4 }

    // Fases de cada capítulo
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

    enum EventoCombate { AntesDeReceberDano, DepoisDeReceberDano }

    // Opções de confirmação
    enum SimOuNao
    {
        [Description("Sim")] Sim = 1,
        [Description("Não")] Nao = 2
    }

    // Opções do menu principal
    enum OpcoesMenu
    {
        [Description("📜 - Jogar Campanha")] JogarCampanha = 1,
        [Description("💰 - Inventário")] Inventario = 2
    }

    /// Tipos de stat que um item pode alterar
    enum TipoStat { ATKFlat, HPFlat, DEFFlat, HPPct, DEFPct, TaxaCritPct, DanoCritPct }

    #endregion
}
