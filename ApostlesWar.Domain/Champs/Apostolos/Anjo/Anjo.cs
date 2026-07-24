using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;

namespace ApostlesWar.Domain.Champs.Apostolos
{
    /// <summary>
    /// Anjo — champ como DADO. Celestial limpa debuffs e cura os aliados (molde Coringa+Sushi).
    /// Céu é o 7º e ÚLTIMO da família do revive: revive os mortos e buffa os vivos (incluindo os
    /// recém-revividos, pela ordem das ações) — era `EstadoAlvo.Ambos`, agora Reviver(Mortos) +
    /// buffs(Vivos), o que mata o último `Ambos` de champ do jogo. Passiva: Bencao.Passiva.cs.
    /// </summary>
    public static class Anjo
    {
        public static Personagem Definir() => new(
            3, Faccao.Apostolos, "Anjo", "😇", 1200, 160, 200,
            Celestial(), Ceu(), new Bencao());

        static HabilidadeAtiva Celestial() => new(
            "Celestial", "🌟", cooldown: 3, "Limpa debuffs dos aliados e cura 30% HP em todos.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new RemoverDebuffs(Seletor.Todos(), Escopo.TodosAliados),
                new Cura(Valor.PorHP(0.30), Escopo.TodosAliados),
            });

        static HabilidadeAtiva Ceu() => new(
            "Céu", "☁️", cooldown: 4, "Revive aliados (50% HP), +25% ATK e Bloqueio Total (2t).",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Reviver(0.50),
                new AplicarBuff(() => new BuffAtaque(duracao: 2, percentual: 0.25), Escopo.TodosAliados),
                new AplicarBuff(() => new BloqueioTotal(duracao: 2), Escopo.TodosAliados),
            });
    }
}
