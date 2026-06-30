using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Reverte a transição Vivo→Morto antes das consequências (IReageAntesDeMorrer).
    /// O portador é restaurado com 1 HP e ganha Invencível 1 turno. Por disparar
    /// antes do Vilão (IReageAoMatar), o Vilão nunca enxerga a morte — ImpedirRessurreicao
    /// não é aplicado e a Necromância também não precisa intervir.
    /// </summary>
    class PassivaGuarda : HabilidadePassiva, IReageAntesDeMorrer
    {
        public PassivaGuarda() : base("Invencível", "⚜️", 4,
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