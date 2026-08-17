using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;

namespace ApostlesWar.Domain.Apostolos.Humanos
{
    /// <summary>
    /// Sushiman — apóstolo como DADO. Comportamento real: CodigoDoSushi.Passiva.cs.
    /// O Nigiri é o 1º cliente do Reviver: duas ações de estados diferentes (Mortos → Vivos), nesta
    /// ORDEM — é ela que faz os recém-revividos pegarem o buff.
    /// </summary>
    public static class Sushiman
    {
        public static Personagem Definir() => new(
            4, Faccao.Humanos, "Sushiman ", "👲", TipoDeApostolo.Suporte,
            Sushi(), Nigiri(), new CodigoDoSushi());

        static HabilidadeAtiva Sushi() => new(
            "Sushi", "🍣", cooldown: 3, "Cura todos os aliados em 30% do HP máximo.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Cura(Valor.PorHP(0.30), Escopo.TodosAliados),
            });

        static HabilidadeAtiva Nigiri() => new(
            "Nigiri", "🍙", cooldown: 3, "Revive aliados com 50% do HP e da um bônus de +50% de ATK em todos por 2 turnos." +
            "\nEssa habilidade funcionará mesmo se não houver aliados mortos.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Reviver(0.50),                                                    // TodosAliados/Mortos (defaults)
                new AplicarBuff(() => new BuffAtaque(duracao: 2, percentual: 0.5),
                    Escopo.TodosAliados, EstadoAlvo.Vivos),                           // pega os revividos
            });
    }
}
