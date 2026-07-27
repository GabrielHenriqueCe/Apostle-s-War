using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Humanos
{
    /// <summary>Policial — champ como DADO. Comportamento real: AlgemasReforcadas.Passiva.cs.</summary>
    public static class Policial
    {
        public static Personagem Definir() => new(
            3, Faccao.Humanos, "Policial", "👮", 1000, 120, 280,
            Tiroteio(), Prender(), new AlgemasReforcadas());

        static HabilidadeAtiva Tiroteio() => new(
            "Tiroteio", "🔫", cooldown: 3, "Ataca 2 inimigos aleatórios com 350% do ATK. Pode acertar o mesmo alvo duas vezes.",
            numeroDeAlvos: 2, tipoAlvo: TipoAlvo.Aleatorio, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(3.5),
            });

        static HabilidadeAtiva Prender() => new(
            "Prender", "⛓️", cooldown: 3, "Prende todos os inimigos por 1 turno.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarDebuff(() => new Preso(duracao: 1)),
            });
    }
}
