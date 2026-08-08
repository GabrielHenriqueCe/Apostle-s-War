using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Apostolos.Especial
{
    /// <summary>
    /// Vilão — apóstolo como DADO. DestruindoDia é o 2º cliente do RemoverDebuffs (cleanse dos aliados
    /// + ataque); Vilania é o molde do Tiroteio (2 alvos aleatórios). Passiva: Sentenca.Passiva.cs.
    /// </summary>
    public static class Vilao
    {
        public static Personagem Definir() => new(
            3, Faccao.Especial, "Vilão", "🦹", TipoDeApostolo.Combatente,
            DestruindoDia(), Vilania(), new Sentenca());

        static HabilidadeAtiva DestruindoDia() => new(
            "Destruindo o Dia", "🦹", cooldown: 3, "Limpa maleficios dos aliados e ataca todos com 325% do ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new RemoverDebuffs(Seletor.Todos(), Escopo.TodosAliados),
                new Dano(2.0),
            });

        static HabilidadeAtiva Vilania() => new(
            "Vilania", "👿", cooldown: 3, "2 ataques aleatórios com 250% ATK.",
            numeroDeAlvos: 2, tipoAlvo: TipoAlvo.Aleatorio, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(2.5),
            });
    }
}
