using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Buffs
{
    /// <summary>
    /// O portador não pode ser selecionado como alvo (verificado em ResolverListaDeAlvosDisponiveis).
    /// NÃO bloqueia dano — apenas filtra seleção. Dano de habilidades em área ainda atinge.
    /// </summary>
    public class Intocavel : Buff
    {
        public Intocavel(int duracao = 2, bool removivel = true) : base("Intocável", "🕳️", duracao, 0,
            "Não pode ser selecionado como alvo.", removivel)
        { }

        // Não sobrescreve ModificarDanoRecebido — Intocável não afeta dano

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
