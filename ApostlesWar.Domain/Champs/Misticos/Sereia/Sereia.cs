using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;

namespace ApostlesWar.Domain.Champs.Misticos
{
    /// <summary>
    /// Sereia — champ como DADO. Atlantis é o 5º da família do revive e o cliente que estreou o
    /// `buffNoRevivido` do Reviver: Intocável SÓ nos revividos, numa ação só — dissolveu o "pipeline"
    /// (ADR §8.1), e o Circo (Folclore) foi consertado no mesmo movimento. Passiva: Aquagirl.Passiva.cs
    /// (passiva-pura, IModificaDanoRecebido -15%).
    /// </summary>
    public static class Sereia
    {
        public static Personagem Definir() => new(
            2, Faccao.Misticos, "Sereia", "🧜", 600, 280, 200,
            CantoDeSereia(), Atlantis(), new Aquagirl());

        static HabilidadeAtiva CantoDeSereia() => new(
            "Canto de Sereia", "🧜‍♀️", cooldown: 4, "Imunidade a malefícios e +25% ATK em todos os aliados (2t).",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarBuff(() => new ImunidadeDebuffs(duracao: 2), Escopo.TodosAliados),
                new AplicarBuff(() => new BuffAtaque(duracao: 2, percentual: 0.25), Escopo.TodosAliados),
            });

        static HabilidadeAtiva Atlantis() => new(
            "Atlantis", "🌊", cooldown: 4, "Revive aliados mortos (50% HP) e aplica Intocável nos revividos.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Reviver(0.50, buffNoRevivido: () => new Intocavel(duracao: 2)),
            });
    }
}
