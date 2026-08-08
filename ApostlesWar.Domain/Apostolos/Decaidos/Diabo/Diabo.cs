using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Apostolos.Decaidos
{
    /// <summary>
    /// Diabo — apóstolo como DADO. Inferno migra o shim `Queima.Explodir` pra ação `Explodir`
    /// genérica (Seletor.Tipo&lt;Queima&gt;) — os EventoDano da explosão passam a entrar no
    /// pipeline (aparecem na tela e contam nas ações seguintes; antes o Inferno os descartava).
    /// Anjo Caído quebra a Sentença dos mortos (RemoverDebuffs), revive e cura os vivos — a ordem
    /// das ações faz a Cura pegar os recém-revividos. Passiva: CresceComDor.Passiva.cs.
    /// </summary>
    public static class Diabo
    {
        public static Personagem Definir() => new(
            4, Faccao.Decaidos, "Diabo", "😈", TipoDeApostolo.Suporte,
            Inferno(), AnjoCaido(), new CresceComDor());

        static HabilidadeAtiva Inferno() => new(
            "Inferno", "🔥", cooldown: 3, "Aplica 2 Queima em todos os inimigos e explode imediatamente." +
            "\nEntão ataca todos os inimigos com 300% do ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarDebuff(() => new Queima(stacks: 2), Escopo.TodosInimigos),
                new Explodir(Seletor.Tipo<Queima>(), Escopo.TodosInimigos),
                new Dano(3.0),
            });

        static HabilidadeAtiva AnjoCaido() => new(
            "Anjo Caído", "😇", cooldown: 3, "Revive aliados com 50% de HP e cura todos com 30% HP." +
            "\nImpedir Ressurreição não impede o Diabo de tirar alguém do Inferno!",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Mortos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new RemoverDebuffs(Seletor.Tipo<ImpedirRessurreicao>(), Escopo.TodosAliados, EstadoAlvo.Mortos),
                new Reviver(0.50),
                new Cura(Valor.PorHP(0.30), Escopo.TodosAliados),
            });
    }
}
