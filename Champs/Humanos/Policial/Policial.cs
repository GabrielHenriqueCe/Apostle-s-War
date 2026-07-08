using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Champs.Humanos
{
    /// <summary>Policial — champ como DADO. Comportamento real: AlgemasReforcadas.Passiva.cs.</summary>
    static class Policial
    {
        public static Personagem Definir() => new(
            3, Faccao.Humanos, "Policial", "👮", 1000, 120, 280,
            Tiroteio(), Prender(), new AlgemasReforcadas());

        static HabilidadeAtiva Tiroteio() => new(
            "Tiroteio", "🔫", turnos: 4, "Ataca 2 inimigos aleatórios com 75% ATK. Pode acertar o mesmo alvo duas vezes.",
            numeroDeAlvos: 2, tipoAlvo: TipoAlvo.Aleatorio, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(0.75),
            });

        static HabilidadeAtiva Prender() => new(
            "Prender", "⛓️", turnos: 4, "Inimigo pula os próximos 2 turnos.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarDebuff(() => new Preso(turnos: 2)),
            });
    }
}
