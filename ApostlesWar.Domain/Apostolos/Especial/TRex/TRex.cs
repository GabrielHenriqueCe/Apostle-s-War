using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Apostolos.Especial
{
    /// <summary>
    /// T-Rex — apóstolo como DADO. Vocabulário puro: Rugido (Provocar + RefletirDano em si + Medo nos
    /// inimigos) e Pisada (ContraAtaque em si + ataque). Passiva: PeleGrossa.Passiva.cs.
    /// </summary>
    public static class TRex
    {
        public static Personagem Definir() => new(
            4, Faccao.Especial, "T-Rex", "🦖", 1000, 160, 240,
            Rugido(), Pisada(), new PeleGrossa());

        static HabilidadeAtiva Rugido() => new(
            "Rugido", "🦖", cooldown: 3, "Aplica Provocar e Refletir Dano em si por 2 turnos." +
            "\nEntão aplica Medo nos inimigos por 1 turno.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Self,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarBuff(() => new Provocar(duracao: 2), Escopo.ProprioAtacante),
                new AplicarBuff(() => new RefletirDano(duracao: 2), Escopo.ProprioAtacante),
                new AplicarDebuff(() => new Medo(duracao: 1), Escopo.TodosInimigos),
            });

        static HabilidadeAtiva Pisada() => new(
            "Pisada", "🦶", cooldown: 3, "Aplica Contra-ataque em si por 2 turnos e ataca todos com 325% do ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new AplicarBuff(() => new ContraAtaque(duracao: 2), Escopo.ProprioAtacante),
                new Dano(3.25),
            });
    }
}
