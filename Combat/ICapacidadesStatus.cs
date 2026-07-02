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
    /// Implementadores: ImunidadeDebuffs, ImpedirBeneficios (status); PassivaAbobora,
    /// PassivaDragao (passivas-pura).
    /// </summary>
    interface IBloqueiaStatus
    {
        bool Bloqueia(StatusEffect novo);
    }
}