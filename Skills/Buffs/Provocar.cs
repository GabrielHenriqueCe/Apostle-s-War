using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Força os inimigos a só poderem atacar quem tem este buff.
    /// Ignora Intocável. Se houver mais de um com Provocar, pode escolher entre eles.
    /// </summary>
    class Provocar : Buff
    {
        public Provocar(int turnos = 2) : base("Provocar", "😤", turnos, 0, "Força inimigos a atacar apenas este personagem.") { }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
