using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Humanos
{
    /// <summary>Detetive — champ como DADO. Comportamento real: OlhoClinico.Passiva.cs.</summary>
    public static class Detetive
    {
        public static Personagem Definir() => new(
            2, Faccao.Humanos, "Detetive", "🕵️", 1400, 160, 160,
            Espionagem(), Furtividade(), new OlhoClinico());

        static HabilidadeAtiva Espionagem() => new(
            "Espionagem", "🔎", cooldown: 3, "-30% DEF em todos os inimigos por 2 turnos e ganha um turno extra",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarDebuff(() => new ReducaoDefesa(duracao: 2)),
                new ConcederTurnoExtra(),
            });

        static HabilidadeAtiva Furtividade() => new(
            "Furtividade", "🕳️", cooldown: 3, "Intocável por 2 turnos. Ataca todos os inimigos com 300% do ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new AplicarBuff(() => new Intocavel(duracao: 2), Escopo.ProprioAtacante),
                new Dano(3.0),
            });
    }
}
