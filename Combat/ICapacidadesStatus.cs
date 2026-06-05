namespace ApostlesWar
{
    /// <summary>
    /// Capacidade B do modelo de capacidades: INTERVENÇÃO NO DANO.
    /// O status altera o dano recebido pelo portador ENQUANTO ele acontece
    /// (antes do HP cair) — diferente de uma reação, que dispara DEPOIS.
    /// Chamada dentro de Combate.ReceberDano, na ordem de StatusAtivos.
    /// Implementadores: Escudo, BloqueioTotal, Invencivel, ProtecaoAliado,
    /// ReducaoDanoFixo.
    /// </summary>
    interface IModificaDanoRecebido
    {
        int ModificarDanoRecebido(Combate portador, int dano);
    }

    /// <summary>
    /// Capacidade E do modelo de capacidades: BLOQUEIO DE APLICAÇÃO.
    /// O status impede que outro StatusEffect seja aplicado no portador.
    /// Chamada em Combate.PodeReceber, antes de adicionar um novo status.
    /// Implementadores: ImunidadeDebuffs, ImunidadeEspecifica, ImpedirBeneficios.
    /// </summary>
    interface IBloqueiaStatus
    {
        bool Bloqueia(StatusEffect novo);
    }
}