using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Buff: cada vez que o portador é atacado, o atacante recebe Veneno (1 stack)
    /// e Queima (1 stack). Sem CD — dispara em cada hit.
    /// Usado pela PassivaElfo (aplicado permanente via IPassivaInicial).
    /// </summary>
    class EspinhosVenenosos : Buff
    {
        public EspinhosVenenosos(int turnos = int.MaxValue)
            : base("Espinhos", "🌿", turnos, 0,
                "Atacantes recebem Veneno e Queima.")
        { }

        public override void AoSerAtacado(Combate portador, Combate atacante, int danoCausado)
        {
            if (!atacante.EstaVivo()) return;

            new Veneno(stacks: 1).Aplicar(atacante);
            new Queima(stacks: 1).Aplicar(atacante);
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}