using ApostlesWar;

namespace ApostlesWar.Champs.Especial
{
    /// <summary>
    /// Vilão — champ como DADO. DestruindoDia é o 2º cliente do RemoverDebuffs (cleanse dos aliados
    /// + ataque); Vilania é o molde do Tiroteio (2 alvos aleatórios). Passiva: Sentenca.Passiva.cs.
    /// </summary>
    static class Vilao
    {
        public static Personagem Definir() => new(
            3, Faccao.Especial, "Vilão", "🦹", 1200, 200, 160,
            DestruindoDia(), Vilania(), new Sentenca());

        static HabilidadeAtiva DestruindoDia() => new(
            "Destruindo o Dia", "🦹", cooldown: 3, "Limpa maleficios dos aliados e ataca todos +100% ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new RemoverDebuffs(Seletor.Todos(), Escopo.TodosAliados),
                new Dano(2.0),
            });

        static HabilidadeAtiva Vilania() => new(
            "Vilania", "👿", cooldown: 4, "2 ataques aleatórios +200% ATK.",
            numeroDeAlvos: 2, tipoAlvo: TipoAlvo.Aleatorio, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(3.0),
            });
    }
}
