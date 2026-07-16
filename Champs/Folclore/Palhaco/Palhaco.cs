using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Folclore
{
    /// <summary>
    /// Palhaço — champ como DADO. Coringa é o 1º cliente do RemoverDebuffs (cleanse total dos
    /// aliados + ImunidadeDebuffs). Circo é o 4º da família do revive: dá Intocável **só nos
    /// revividos** via o parâmetro `buffNoRevivido` do Reviver (bugfix — antes aplicava em TODOS
    /// os outros vivos, que sempre foi errado vs a intenção; ver Reviver.cs e ADR §8.1). Passiva:
    /// PiadaDeMauGosto.Passiva.cs.
    /// </summary>
    static class Palhaco
    {
        public static Personagem Definir() => new(
            3, Faccao.Folclore, "Palhaço", "🤡", 800, 160, 280,
            Coringa(), Circo(), new PiadaDeMauGosto());

        static HabilidadeAtiva Coringa() => new(
            "Coringa", "🃏", turnos: 3, "Remove malefícios dos aliados e dá imunidade por 2 turnos.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new RemoverDebuffs(Seletor.Todos(), Escopo.TodosAliados),
                new AplicarBuff(() => new ImunidadeDebuffs(turnos: 2), Escopo.TodosAliados),
            });

        static HabilidadeAtiva Circo() => new(
            "Circo", "🎪", turnos: 4, "Revive aliados (50% HP) e dá Intocável aos revividos.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Reviver(0.50, buffNoRevivido: () => new Intocavel(turnos: 2)),
            });
    }
}
