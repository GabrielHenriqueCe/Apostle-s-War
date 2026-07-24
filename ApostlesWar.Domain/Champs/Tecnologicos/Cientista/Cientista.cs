using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Tecnologicos
{
    /// <summary>
    /// Cientista — champ como DADO. Química e Física são vocabulário puro (Dano + AplicarDebuff),
    /// irmãs do Vômito Tóxico (Zumbi). Passiva: AnaliseCritica.Passiva.cs (a 3ª da família de passivas
    /// que mexem em duração de status — reduz buffs do atacante; ver a nota no ADR §9).
    /// </summary>
    public static class Cientista
    {
        public static Personagem Definir() => new(
            4, Faccao.Tecnologicos, "Cientista", "🧑‍🔬", 1000, 200, 200,
            Quimica(), Fisica(), new AnaliseCritica());

        static HabilidadeAtiva Quimica() => new(
            "Química", "🧪", cooldown: 3, "Ataca todos e aplica Veneno (5% HP/turno por stack).",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(1.0),
                new AplicarDebuff(() => new Veneno(stacks: 2)),
            });

        static HabilidadeAtiva Fisica() => new(
            "Física", "⚛️", cooldown: 3, "Ataca todos e aplica Queima 2t.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(1.0),
                new AplicarDebuff(() => new Queima(2)),
            });
    }
}
