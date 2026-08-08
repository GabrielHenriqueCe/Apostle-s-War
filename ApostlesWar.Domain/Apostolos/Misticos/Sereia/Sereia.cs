using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;

namespace ApostlesWar.Domain.Apostolos.Misticos
{
    /// <summary>
    /// Sereia — apóstolo como DADO. Atlantis é o 5º da família do revive e o cliente que estreou o
    /// `buffNoRevivido` do Reviver: Intocável SÓ nos revividos, numa ação só — dissolveu o "pipeline"
    /// (ADR §8.1), e o Circo (Folclore) foi consertado no mesmo movimento. Passiva: Aquagirl.Passiva.cs
    /// (passiva-pura, IModificaDanoRecebido -15%).
    /// </summary>
    public static class Sereia
    {
        public static Personagem Definir() => new(
            2, Faccao.Misticos, "Sereia", "🧜", TipoDeApostolo.Suporte,
            CantoDeSereia(), Atlantis(), new Aquagirl());

        static HabilidadeAtiva CantoDeSereia() => new(
            "Canto de Sereia", "🧜‍♀️", cooldown: 3, "Imunidade a malefícios e +50% de ATK em todos os aliados por 2 turnos.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarBuff(() => new ImunidadeDebuffs(duracao: 2), Escopo.TodosAliados),
                new AplicarBuff(() => new BuffAtaque(duracao: 2, percentual: 0.25), Escopo.TodosAliados),
            });

        static HabilidadeAtiva Atlantis() => new(
            "Atlantis", "🌊", cooldown: 3, "Revive aliados mortos com 50% de HP e aplica Intocável nos revividos.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Mortos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new Reviver(0.50, buffNoRevivido: () => new Intocavel(duracao: 2)),
            });
    }
}
