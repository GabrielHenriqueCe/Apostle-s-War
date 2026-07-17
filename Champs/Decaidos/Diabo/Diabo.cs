using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.Decaidos
{
    /// <summary>
    /// Diabo — champ como DADO. Inferno migra o shim `Queima.Explodir` pra ação `Explodir`
    /// genérica (Seletor.Tipo&lt;Queima&gt;) — os EventoDano da explosão passam a entrar no
    /// pipeline (aparecem na tela e contam nas ações seguintes; antes o Inferno os descartava).
    /// Anjo Caído quebra a Sentença dos mortos (RemoverDebuffs), revive e cura os vivos — a ordem
    /// das ações faz a Cura pegar os recém-revividos. Passiva: CresceComDor.Passiva.cs.
    /// </summary>
    static class Diabo
    {
        public static Personagem Definir() => new(
            4, Faccao.Decaidos, "Diabo", "😈", 1400, 160, 160,
            Inferno(), AnjoCaido(), new CresceComDor());

        static HabilidadeAtiva Inferno() => new(
            "Inferno", "🔥", turnos: 3, "Aplica 2 stacks de Queima em todos os inimigos e explode imediatamente.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarDebuff(() => new Queima(stacks: 2), Escopo.TodosInimigos),
                new Explodir(Seletor.Tipo<Queima>(), Escopo.TodosInimigos),
            });

        static HabilidadeAtiva AnjoCaido() => new(
            "Anjo Caído", "😇", turnos: 3, "Revive aliados (50% HP, quebra a Sentença) e cura todos (30% HP).",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new RemoverDebuffs(Seletor.Tipo<ImpedirRessurreicao>(), Escopo.TodosAliados, EstadoAlvo.Mortos),
                new Reviver(0.50),
                new Cura(Valor.PorHP(0.30), Escopo.TodosAliados),
            });
    }
}
