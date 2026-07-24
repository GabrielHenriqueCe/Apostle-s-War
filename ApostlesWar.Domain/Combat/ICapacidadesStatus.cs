namespace ApostlesWar
{
    /// <summary>
    /// Status que modifica o dano recebido durante o cálculo (Escudo, Bloqueio,
    /// Invencível, Proteção, Redução). ModificarDanoRecebido aplica a modificação.
    ///
    /// Se o modificador AGE neste golpe é decidido fora, por UMA língua só (unificação
    /// jul/2026): o ReceberDano pergunta "este status está na lista de ignorados?" — a lista
    /// unida de natureza.Ignora + ignorarStatus (golpe) + IIgnoraStatusNoAtaque (champ). Não há
    /// mais `DeveAgir` por-status lendo flags da natureza; as regras que eram flags viraram
    /// entradas na lista `NaturezasDano` (ex: QueimaDano fura Escudo; todo dano sem reação fura
    /// ProtecaoAliado — o anti-loop de proteção mútua).
    /// </summary>
    interface IModificaDanoRecebido
    {
        int ModificarDanoRecebido(Combate portador, int dano);
    }

    /// <summary>
    /// Capacidade E do modelo de capacidades: BLOQUEIO DE APLICAÇÃO.
    /// O status impede que outro StatusEffect seja aplicado no portador.
    /// Chamada em Combate.PodeReceber, antes de adicionar um novo status.
    /// Implementadores: ImunidadeDebuffs, ImpedirBeneficios (status); CascaDura,
    /// PeleDeDragao (passivas-pura).
    /// </summary>
    interface IBloqueiaStatus
    {
        bool Bloqueia(StatusEffect novo);
    }

    /// <summary>
    /// Status com efeito de tick (dano periódico) que pode ser DETONADO — aplica de uma vez o
    /// efeito remanescente e se remove, em vez de esperar o próprio turno do portador. É o
    /// molde único das explosões (regra de Gabriel): a Ação Explodir orquestra igual pra todos,
    /// e cada status detona FAZENDO O QUE ELE FAZ (Veneno só dano; Queima dano + redução de HP
    /// máximo). Implementadores: Veneno (cliente: Putrefação), Queima (cliente: Inferno, migra
    /// nos Decaídos).
    /// </summary>
    interface IStatusComTick
    {
        /// <summary>
        /// Aplica o efeito remanescente de uma vez, remove o status e devolve o EventoDano da
        /// detonação (bruto/efetivo/absorvido/natureza) — o interpretador agrega esses eventos
        /// junto dos de Dano (invariante do ADR-composicao-de-acoes §7), então a explosão
        /// aparece na exibição, conta no PorDanoCausado e a morte-por-explosão passa pelos
        /// Atos de morte. O detonador entra como Atacante do evento.
        /// </summary>
        EventoDano Detonar(Combate portador, Combate detonador);
    }
}