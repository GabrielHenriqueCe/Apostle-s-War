using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Champs.Humanos
{
    /// <summary>Detetive — champ como DADO. Comportamento real: OlhoClinico.Passiva.cs.</summary>
    static class Detetive
    {
        public static Personagem Definir() => new(
            2, Faccao.Humanos, "Detetive", "🕵️", 1400, 160, 160,
            Espionagem(), Furtividade(), new OlhoClinico());

        static HabilidadeAtiva Espionagem() => new(
            "Espionagem", "🔎", turnos: 4, "-30% DEF em todos os inimigos por 2 turnos.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarDebuff(() => new ReducaoDefesa(turnos: 2)),
            });

        static HabilidadeAtiva Furtividade() => new(
            "Furtividade", "🕳️", turnos: 4, "Intocável por 2 turnos. Ataca todos os inimigos com 100% ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new AplicarBuff(() => new Intocavel(turnos: 2), Escopo.ProprioAtacante),
                new Dano(1.0),
            });
    }
}
