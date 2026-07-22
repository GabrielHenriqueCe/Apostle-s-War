using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.Reino
{
    class Piromancer : HabilidadePassiva
    {
        public Piromancer() : base("Piromancer", "🪄", 0,
            "Causa 25% mais dano contra alvos com Queima.")
        { }

        public override List<EventoCombate> Ativar(ContextoCombate ctx, Combate alvo)
            => SemDano();

        /// <summary>
        /// Multiplicador extra se o atacante tem essa passiva e o alvo tem Queima.
        /// </summary>
        public static double MultExtra(Combate atacante, Combate alvo)
        {
            bool temPassiva = atacante.Personagem.Habilidades.OfType<Piromancer>().Any();
            bool alvComQueima = alvo.StatusAtivos.Any(s => s is Queima);
            return (temPassiva && alvComQueima) ? 1.25 : 1.0;
        }
    }
}