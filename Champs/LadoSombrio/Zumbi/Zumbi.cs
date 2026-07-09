using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.LadoSombrio
{
    /// <summary>
    /// Zumbi — champ como DADO (ver ADR-composicao-de-acoes §10). Putridao usa a Ação bespoke
    /// local ExplodirVenenoECurarMedia.cs — 1ª AcaoSobreConjunto real do motor. Passiva:
    /// PutrefacaoContagiosa.Passiva.cs.
    /// </summary>
    static class Zumbi
    {
        public static Personagem Definir() => new(
            4, Faccao.LadoSombrio, "Zumbi", "🧟", 1400, 200, 120,
            Fedorento(), Putridao(), new PutrefacaoContagiosa());

        static HabilidadeAtiva Fedorento() => new(
            "Fedorento", "🤢", turnos: 4, "Ataca todos e aplica Veneno (5% HP/turno por stack).",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(1.0),
                new AplicarDebuff(() => new Veneno(stacks: 1)),
            });

        static HabilidadeAtiva Putridao() => new(
            "Putrefação", "💀", turnos: 4, "Ataca todos. Explode Venenos causando dano imediato. Cura média.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(1.0),
                new ExplodirVenenoECurarMedia(),
            });
    }
}
