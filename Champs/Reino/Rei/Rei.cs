using ApostlesWar;

namespace ApostlesWar.Champs.Reino
{
    /// <summary>
    /// Rei — champ como DADO (ver ADR-composicao-de-acoes §10). Lealdade estreia a Ação de
    /// vocabulário AplicarEscudo (§5.5, valor por fragmento — compartilha o valor com Cura).
    /// Passiva: CoroaDoSoberano.Passiva.cs.
    /// </summary>
    static class Rei
    {
        public static Personagem Definir() => new(
            4, Faccao.Reino, "Rei", "🫅", 1000, 200, 200,
            Democracia(), Lealdade(), new CoroaDoSoberano());

        static HabilidadeAtiva Democracia() => new(
            "Democracia", "🗳️", turnos: 3, "Cura todos os aliados em 30% do HP máximo.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Cura(Valor.PorHP(0.30), Escopo.TodosAliados),
            });

        static HabilidadeAtiva Lealdade() => new(
            "Lealdade", "🎖️", turnos: 3, "Aplica Escudo de 30% do HP máximo em todos os aliados por 2 turnos.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarEscudo(Valor.PorHP(0.30), turnos: 2, Escopo.TodosAliados),
            });
    }
}
