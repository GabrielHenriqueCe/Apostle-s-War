using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;
using ApostlesWar.Domain.Skills.Passivas;

namespace ApostlesWar.Domain.Champs.LadoSombrio
{
    /// <summary>
    /// Zumbi — champ como DADO (ver ADR-composicao-de-acoes §10), vocabulário puro (Nível 1).
    /// Putrefação é o 1º cliente da Ação Explodir (molde único das explosões — detona o que o
    /// status faz); a cura é um EXTRA da habilidade (ação separada, 20% de todo o dano causado
    /// via PorDanoCausado — a explosão entra na conta porque registra seus EventoDano), por
    /// isso a explosão é reutilizável a seco por outros champs. Passiva: "Horda" (EscalaComMortos)
    /// — 1º cliente da capacidade que escala com os mortos no campo.
    /// </summary>
    public static class Zumbi
    {
        public static Personagem Definir() => new(
            4, Faccao.LadoSombrio, "Zumbi", "🧟", 1400, 200, 120,
            VomitoToxico(), Putridao(),
            new EscalaComMortos("Horda", "🧟", "+10% ATK por combatente morto no campo (dos dois times).",
                EscopoMortos.AmbosOsTimes, porMorto: 0.10, v => new BuffAtaque(duracao: 2, percentual: v)));

        static HabilidadeAtiva VomitoToxico() => new(
            "Vômito Tóxico", "🤢", cooldown: 4, "Ataca todos e aplica Veneno (5% HP/turno por stack).",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(1.0),
                new AplicarDebuff(() => new Veneno(stacks: 1)),
            });

        static HabilidadeAtiva Putridao() => new(
            "Putrefação", "💀", cooldown: 4, "Ataca todos. Explode os Venenos causando o dano imediato. Cura 20% do dano causado.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(1.0),
                new Explodir(Seletor.Tipo<Veneno>()),
                new Cura(Valor.PorDanoCausado(0.20), Escopo.ProprioAtacante),
            });
    }
}
