namespace ApostlesWar
{
    /// <summary>
    /// Status que modifica o dano recebido durante o cálculo (Escudo, Bloqueio,
    /// Invencível, Proteção, Redução). DeveAgir decide se o modificador atua neste
    /// golpe; ModificarDanoRecebido aplica a modificação.
    /// </summary>
    interface IModificaDanoRecebido
    {
        /// <summary>
        /// O modificador deve agir neste golpe? Hoje decide pela natureza do dano.
        /// Futuro (unificação dos mecanismos de ignorar): considerará também a lista
        /// de ignorados e passivas como IIgnoraStatusNoAtaque.
        /// </summary>
        bool DeveAgir(NaturezaDano natureza);

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
    /// efeito remanescente e se remove, em vez de esperar o próprio turno do portador. Fragmento
    /// de Valor do lado do status (ADR-composicao-de-acoes §5.5), consumido pela Ação Explodir
    /// (per-alvo, Inferno) e por agregações bespoke que precisam do valor pra além de só aplicar
    /// o dano (ExplodirVenenoECurarMedia, Putrefação — precisa do % pra tirar a média).
    /// Implementadores: Veneno, Queima.
    /// </summary>
    interface IStatusComTick
    {
        /// <summary>Aplica o efeito remanescente de uma vez, remove o status e retorna o dano causado.</summary>
        int Detonar(Combate portador);
    }
}