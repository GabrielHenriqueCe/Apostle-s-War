using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Folclore
{
    /// <summary>
    /// Palhaço — champ como DADO. Coringa é o 1º cliente do RemoverDebuffs (cleanse total dos
    /// aliados + ImunidadeDebuffs). Circo é o 4º da família do revive (o Ambos morre) e mais um
    /// cliente de OutrosAliados: dá Intocável a todos os OUTROS vivos, incluindo os recém-revividos
    /// — a ordem das ações garante (revive antes, buff depois). Passiva: PiadaDeMauGosto.Passiva.cs.
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
            "Circo", "🎪", turnos: 4, "Revive aliados (50% HP) e dá Intocável aos demais.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Reviver(0.50),
                new AplicarBuff(() => new Intocavel(turnos: 2), Escopo.OutrosAliados),
            });
    }
}
