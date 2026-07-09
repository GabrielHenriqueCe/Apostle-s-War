using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Champs.Humanos
{
    /// <summary>
    /// Sushiman — champ como DADO. Comportamento real: CodigoDoSushi.Passiva.cs.
    /// O Nigiri é o 1º cliente do Reviver: era EstadoAlvo.Ambos + Ativar bespoke; virou duas
    /// ações de estados diferentes (Mortos → Vivos), na ordem — os revividos pegam o buff.
    /// </summary>
    static class Sushiman
    {
        public static Personagem Definir() => new(
            4, Faccao.Humanos, "Sushiman ", "👲", 800, 280, 160,
            Sushi(), Nigiri(), new CodigoDoSushi());

        static HabilidadeAtiva Sushi() => new(
            "Sushi", "🍣", turnos: 4, "Cura todos os aliados em 30% do HP máximo.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Cura(Valor.PorHP(0.30), Escopo.TodosAliados),
            });

        static HabilidadeAtiva Nigiri() => new(
            "Nigiri", "🍙", turnos: 4, "Revive aliados (50% HP) e +25% ATK em todos por 2 turnos.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Reviver(0.50),                                                    // TodosAliados/Mortos (defaults)
                new AplicarBuff(() => new BuffAtaque(turnos: 2, percentual: 0.25),
                    Escopo.TodosAliados, EstadoAlvo.Vivos),                           // pega os revividos
            });
    }
}
