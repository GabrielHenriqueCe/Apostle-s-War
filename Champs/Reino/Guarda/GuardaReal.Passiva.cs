using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Reino
{
    /// <summary>
    /// Reverte a transição Vivo→Morto antes das consequências (IReageAntesDeMorrer).
    /// O portador é restaurado com 1 HP e ganha Invencível 1 turno. Por disparar
    /// antes do Vilão (IReageAoMatar), o Vilão nunca enxerga a morte — ImpedirRessurreicao
    /// não é aplicado e a Necromância também não precisa intervir.
    /// Nome de jogo "Guarda Real" (era "Invencível", que colidia com o buff Skills.Buffs.Invencivel
    /// no display; a classe já não colidia).
    /// </summary>
    class GuardaReal : HabilidadePassiva, IReageAntesDeMorrer
    {
        public GuardaReal() : base("Guarda Real", "⚜️", 4,
            "Ao receber ataque fatal, sobrevive com 1 HP e ganha invencibilidade por 1 turno.")
        { }

        public List<ResultadoReacao> AntesDeMorrer(ContextoReacao ctx)
        {
            var portador = ctx.Portador;

            portador.AplicarRevive(1);
            new Invencivel(turnos: 1).Aplicar(portador);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{portador.Personagem.Simbolo} {portador.Personagem.Nome} sobreviveu pela passiva! Invencível por 1 turno!")
            };
        }
    }
}
