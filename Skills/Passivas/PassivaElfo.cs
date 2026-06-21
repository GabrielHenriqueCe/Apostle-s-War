using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Passiva permanente do Elfo: aplica EspinhosVenenosos no início do combate.
    /// Cada vez que o Elfo é atacado, atacante recebe Veneno + Queima (1 stack cada).
    /// </summary>
    class PassivaElfo : HabilidadePassiva, IPassivaInicial
    {
        public PassivaElfo() : base("Espinhos", "🌿", 0,
            "Atacantes recebem Veneno e Queima.")
        { }

        public void AplicarInicial(Combate portador)
        {
            new EspinhosVenenosos().Aplicar(portador);
        }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) => false;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}