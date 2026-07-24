using ApostlesWar;

namespace ApostlesWar.Skills.Buffs
{
    /// <summary>
    /// Força inimigos a só poderem atacar quem tem este buff (ignora Intocável).
    /// Verificado em ResolverListaDeAlvosDisponiveis.
    /// </summary>
    class Provocar : Buff
    {
        public Provocar(int duracao = 2) : base("Provocar", "😤", duracao, 0,
            "Força inimigos a atacar apenas este personagem.")
        { }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}