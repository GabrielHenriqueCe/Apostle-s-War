using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// O portador não pode ser selecionado como alvo (verificado em ResolverListaDeAlvosDisponiveis).
    /// NÃO bloqueia dano — apenas filtra seleção. Dano de habilidades em área ainda atinge.
    /// </summary>
    class Intocavel : Buff
    {
        public Intocavel(int turnos = 2, bool removivel = true) : base("Intocável", "🕳️", turnos, 0,
            "Não pode ser selecionado como alvo.", removivel)
        { }

        // Não sobrescreve ModificarDanoRecebido — Intocável não afeta dano

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
