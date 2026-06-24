using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao receber dano (HP perdido), aplica Escudo de 5% do HP máximo por 1 turno em si.
    /// Reage via IReageAoReceberDano (só dispara com dano > 0).
    /// </summary>
    class PassivaAlien : HabilidadePassiva, IReageAoReceberDano
    {
        public PassivaAlien() : base("Carapaça Alienígena", "👽", 0,
            "Ao receber dano, ganha Escudo de 5% HP por 1 turno.")
        { }

        public List<ResultadoReacao> AoReceberDano(ContextoReacao ctx)
        {
            if (!ctx.Portador.EstaVivo()) return new List<ResultadoReacao>();

            int pontos = (int)(ctx.Portador.HPMaximo * 0.05);
            new Escudo(pontos, turnos: 1).Aplicar(ctx.Portador);

            return new List<ResultadoReacao>();
        }
    }
}