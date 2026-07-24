using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;

namespace ApostlesWar.Domain.Champs.Misticos
{
    /// <summary>
    /// Ao matar um inimigo, aplica BloqueioTotal 3t em si mesma. Migrada para
    /// IReageAoMatar. O buff vai no PORTADOR (a Fada).
    /// </summary>
    public class Voar : HabilidadePassiva, IReageAoMatar
    {
        public Voar() : base("Voar", "🧚", 0,
            "Após matar, fica imune a dano por 3 turnos.")
        { }

        public List<ResultadoReacao> AoMatar(ContextoReacao ctx)
        {
            new BloqueioTotal(duracao: 3).Aplicar(ctx.Portador);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(Mensagem: $"🧚 {ctx.Portador.Personagem.Nome} alçou voo — imune por 3 turnos!")
            };
        }
    }
}
