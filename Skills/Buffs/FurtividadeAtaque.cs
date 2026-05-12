using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Quando expira, o Detetive ataca todos os inimigos com 100% ATK.
    /// O CombateService deve verificar este buff ao processar o turno.
    /// </summary>
    class FurtividadeAtaque : Buff
    {
        public List<Combate> Inimigos { get; }

        public FurtividadeAtaque(List<Combate> inimigos, int turnos = 2)
            : base("Furtividade Ataque", "💥", turnos, 1, "Ao expirar, ataca todos os inimigos.")
        {
            Inimigos = inimigos;
        }

        public override void Remover(Combate portador)
        {
            // Ao expirar, dispara o ataque em todos os inimigos vivos
            foreach (Combate inimigo in Inimigos.Where(i => i.EstaVivo()))
                portador.Atacar(inimigo);

            portador.StatusAtivos.Remove(this);
        }
    }
}
