using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Debuffs
{
    /// <summary>
    /// Força o portador a atacar com A1 quem aplicou. Carrega a capacidade IForcaAcao:
    /// o turno pergunta o AlvoForcado, não olha o tipo concreto.
    /// </summary>
    public class Irritar : Debuff, IForcaAcao
    {
        public Combate Aplicador { get; }

        public Irritar(Combate aplicador, int duracao = 1)
            : base("Irritar", "😡", duracao, 0, "Força atacar com A1 quem aplicou.")
        {
            Aplicador = aplicador;
        }

        /// <summary>Alvo da ação forçada: quem aplicou o Irritar.</summary>
        public Combate AlvoForcado() => Aplicador;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
