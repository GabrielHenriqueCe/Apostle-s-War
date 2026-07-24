using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Folclore
{
    /// <summary>
    /// Troll — champ como DADO. Pancada estreia o `chance` do AplicarDebuff (Medo 50% por alvo).
    /// Porradeiro é o molde do Tiroteio (numeroDeAlvos:6 + Aleatorio, com repetição) + a cura por
    /// dano do Zumbi (PorDanoCausado no próprio — 30% do dano total dos 6 hits). Passiva:
    /// Ambicao.Passiva.cs.
    /// </summary>
    public static class Troll
    {
        public static Personagem Definir() => new(
            4, Faccao.Folclore, "Troll", "🧌", 1200, 160, 200,
            Pancada(), Porradeiro(), new Ambicao());

        static HabilidadeAtiva Pancada() => new(
            "Pancada", "🤜", cooldown: 3, "Ataca todos +75% ATK e 50% chance de Medo 1t em cada.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(1.75),
                new AplicarDebuff(() => new Medo(duracao: 1), chance: 0.50),
            });

        static HabilidadeAtiva Porradeiro() => new(
            "Porradeiro", "🥊", cooldown: 4, "6 ataques aleatórios +50% ATK. Cura 30% do dano causado.",
            numeroDeAlvos: 6, tipoAlvo: TipoAlvo.Aleatorio, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(1.5),
                new Cura(Valor.PorDanoCausado(0.30), Escopo.ProprioAtacante),
            });
    }
}
