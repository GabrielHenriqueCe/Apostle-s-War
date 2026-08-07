using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Apostolos.Misticos
{
    /// <summary>
    /// Dragão — apóstolo como DADO. SoproDoDragão é vocabulário puro (dano + Queima). DragãoProtetor
    /// usa a Ação bespoke RestaurarHPMaximo (ver RestaurarHPMaximo.cs) entre o ContraAtaque e a Cura
    /// — cada coisa é uma ação (DECOMPOR). Passiva: PeleDeDragao.Passiva.cs (passiva-pura,
    /// IBloqueiaStatus — imune a Veneno/Queima).
    /// </summary>
    public static class Dragao
    {
        public static Personagem Definir() => new(
            4, Faccao.Misticos, "Dragão", "🐲", 1400, 200, 120,
            SoproDoDragao(), DragaoProtetor(), new PeleDeDragao());

        static HabilidadeAtiva SoproDoDragao() => new(
            "Sopro do Dragão", "🔥", cooldown: 3, "Ataca todos com 300% ATK e aplica 2 Queimas.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(3.0),
                new AplicarDebuff(() => new Queima(stacks: 2)),
            });

        static HabilidadeAtiva DragaoProtetor() => new(
            "Dragão Protetor", "🐲", cooldown: 3, "Aplica Contra-ataque, restaura 25% do HP máx perdido e cura 25% do HP de todos os aliados.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarBuff(() => new ContraAtaque(duracao: 2), Escopo.TodosAliados),
                new RestaurarHPMaximo(0.25, Escopo.TodosAliados),
                new Cura(Valor.PorHP(0.25), Escopo.TodosAliados),
            });
    }
}
