using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Reino
{
    /// <summary>
    /// Ninja — champ como DADO (ver ADR-composicao-de-acoes §10). Shuriken usa a Ação bespoke
    /// GolpeSeguidor.cs (Nível 3, ADR §9) pro acoplamento hit-a-hit. Passiva: Sorrateiro.Passiva.cs.
    /// </summary>
    static class Ninja
    {
        public static Personagem Definir() => new(
            2, Faccao.Reino, "Ninja", "🥷", 600, 280, 200,
            Shuriken(), Kunai(), new Sorrateiro());

        static HabilidadeAtiva Shuriken() => new(
            "Shuriken", "🌟", cooldown: 3, "Ataca 1 inimigo 2x +50% ATK. Se o 1º hit for crítico, o 2º ignora 25% da DEF.",
            numeroDeAlvos: 2, tipoAlvo: TipoAlvo.Aleatorio, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new GolpeSeguidor(1.5, ignorarDefesaPctSeAnteriorCritico: 0.25),
            });

        static HabilidadeAtiva Kunai() => new(
            "Kunai", "🗡️", cooldown: 4, "Intocável 2t. Ataca 1 inimigo +50% ATK, sempre crítico, ignora 75% DEF.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new AplicarBuff(() => new Intocavel(duracao: 2), Escopo.ProprioAtacante),
                new Dano(1.5, ignorarDefesaPct: 0.75, forcaCritico: true),
            });
    }
}
