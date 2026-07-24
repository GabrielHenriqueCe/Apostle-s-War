using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Reino
{
    public class Piromancer : HabilidadePassiva, IModificaDanoCausado
    {
        public Piromancer() : base("Piromancer", "🪄", 0,
            "Causa 25% mais dano contra alvos com Queima.")
        { }

        public override List<EventoCombate> Ativar(ContextoCombate ctx, Combate alvo)
            => SemDano();

        /// <summary>
        /// Multiplicador de dano do atacante: 25% a mais se o alvo tem Queima. Consultado pela
        /// Ação Dano (a checagem "atacante tem a passiva" some — se esta instância está sendo
        /// consultada, é porque vive nas Habilidades do atacante).
        /// </summary>
        public double MultiplicadorDeDano(Combate atacante, Combate alvo)
            => alvo.StatusAtivos.Any(s => s is Queima) ? 1.25 : 1.0;
    }
}