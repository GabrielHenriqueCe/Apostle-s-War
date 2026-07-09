using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Reino
{
    /// <summary>
    /// Reverte a transição Vivo→Morto antes das consequências (IReageAntesDeMorrer).
    /// O portador é restaurado com 1 HP e ganha Invencível 1 turno. Por disparar
    /// antes do Vilão (IReageAoMatar), o Vilão nunca enxerga a morte — ImpedirRessurreicao
    /// não é aplicado e a Necromância também não precisa intervir.
    /// Renomeada de PassivaGuarda (o nome antigo colidia com Skills.Buffs.Invencivel).
    /// </summary>
    class GuardaInvencivel : HabilidadePassiva, IReageAntesDeMorrer
    {
        public GuardaInvencivel() : base("Invencível", "⚜️", 4,
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
