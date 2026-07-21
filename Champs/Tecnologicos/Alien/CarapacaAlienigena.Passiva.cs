using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Tecnologicos
{
    /// <summary>
    /// Ao receber dano (HP perdido), aplica Escudo de 5% do HP máximo por 1 turno em si.
    /// Reage via IReageAoReceberDano (só dispara com dano > 0).
    /// </summary>
    class CarapacaAlienigena : HabilidadePassiva, IReageAoReceberDano
    {
        public CarapacaAlienigena() : base("Carapaça Alienígena", "👽", 0,
            "Ao receber dano, ganha Escudo de 5% HP por 1 turno.")
        { }

        public List<ResultadoReacao> AoReceberDano(ContextoReacao ctx)
        {
            if (!ctx.Portador.EstaVivo()) return new List<ResultadoReacao>();

            int pontos = (int)(ctx.Portador.HPMaximo * 0.05);
            new Escudo(pontos, duracao: 1).Aplicar(ctx.Portador);

            return new List<ResultadoReacao>();
        }
    }
}
