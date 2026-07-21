using ApostlesWar;
using ApostlesWar.Skills;

namespace ApostlesWar.Champs.Misticos
{
    /// <summary>
    /// Fada — champ como DADO. Sininho é dano puro (3.0x). PoMágico também é vocabulário puro: usa
    /// o `ignorarStatus` do Dano passando `typeof(Buff)` — a checagem por tipo-BASE no Combate.cs
    /// (IsAssignableFrom) faz isso ignorar TODOS os buffs do alvo, sem bespoke. Passiva: Voar.Passiva.cs.
    /// </summary>
    static class Fada
    {
        public static Personagem Definir() => new(
            3, Faccao.Misticos, "Fada", "🧚", 1000, 280, 120,
            Sininho(), PoMagico(), new Voar());

        static HabilidadeAtiva Sininho() => new(
            "Sininho", "🔔", cooldown: 3, "Ataca 1 inimigo com +200% ATK.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(3.0),
            });

        static HabilidadeAtiva PoMagico() => new(
            "Pó Mágico", "✨", cooldown: 4, "Ataca todos ignorando TODOS os benefícios do alvo.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(1.5, ignorarStatus: new[] { typeof(Buff) }),
            });
    }
}
