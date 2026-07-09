using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Skills.Passivas
{
    /// <summary>
    /// Ao matar um inimigo, aplica BloqueioTotal 3t em si mesma. Migrada para
    /// IReageAoMatar. O buff vai no PORTADOR (a Fada).
    /// </summary>
    class Voar : HabilidadePassiva, IReageAoMatar
    {
        public Voar() : base("Voar", "🧚", 0,
            "Após matar, fica imune a dano por 3 turnos.")
        { }

        public List<ResultadoReacao> AoMatar(ContextoReacao ctx)
        {
            new BloqueioTotal(turnos: 3).Aplicar(ctx.Portador);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Mensagem: $"🧚 {ctx.Portador.Personagem.Nome} alçou voo — imune por 3 turnos!")
            };
        }
    }
}