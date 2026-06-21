using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    class PassivaPiromancer : HabilidadePassiva
    {
        public PassivaPiromancer() : base("Piromancer", "🪄", 0,
            "Causa 25% mais dano contra alvos com Queima.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) => false;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
            => SemDano();

        /// <summary>
        /// Multiplicador extra se o atacante tem essa passiva e o alvo tem Queima.
        /// </summary>
        public static double MultExtra(Combate atacante, Combate alvo)
        {
            bool temPassiva = atacante.Personagem.Habilidades.OfType<PassivaPiromancer>().Any();
            bool alvComQueima = alvo.StatusAtivos.Any(s => s is Queima);
            return (temPassiva && alvComQueima) ? 1.25 : 1.0;
        }
    }
}