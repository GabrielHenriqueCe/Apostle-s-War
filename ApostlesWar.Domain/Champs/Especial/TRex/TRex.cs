using ApostlesWar;
using ApostlesWar.Skills.Buffs;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.Especial
{
    /// <summary>
    /// T-Rex — champ como DADO. Vocabulário puro: Rugido (Provocar + RefletirDano em si + Medo nos
    /// inimigos) e Pisada (ContraAtaque em si + ataque). Passiva: PeleGrossa.Passiva.cs.
    /// </summary>
    static class TRex
    {
        public static Personagem Definir() => new(
            4, Faccao.Especial, "T-Rex", "🦖", 1000, 160, 240,
            Rugido(), Pisada(), new PeleGrossa());

        static HabilidadeAtiva Rugido() => new(
            "Rugido", "🦖", cooldown: 3, "Provocar + Refletir Dano em si (2t) e Medo nos inimigos (1t).",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Self,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarBuff(() => new Provocar(duracao: 2), Escopo.ProprioAtacante),
                new AplicarBuff(() => new RefletirDano(duracao: 2), Escopo.ProprioAtacante),
                new AplicarDebuff(() => new Medo(duracao: 1), Escopo.TodosInimigos),
            });

        static HabilidadeAtiva Pisada() => new(
            "Pisada", "🦶", cooldown: 3, "Contra-ataque em si (2t) e ataca todos +125% ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new AplicarBuff(() => new ContraAtaque(duracao: 2), Escopo.ProprioAtacante),
                new Dano(2.25),
            });
    }
}
