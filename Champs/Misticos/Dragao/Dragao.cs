using ApostlesWar;
using ApostlesWar.Skills.Buffs;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.Misticos
{
    /// <summary>
    /// Dragão — champ como DADO. SoproDoDragão é vocabulário puro (dano + Queima). DragãoProtetor
    /// usa a Ação bespoke RestaurarHPMaximo (ver RestaurarHPMaximo.cs) entre o ContraAtaque e a Cura
    /// — cada coisa é uma ação (DECOMPOR). Passiva: PeleDeDragao.Passiva.cs (passiva-pura,
    /// IBloqueiaStatus — imune a Veneno/Queima).
    /// </summary>
    static class Dragao
    {
        public static Personagem Definir() => new(
            4, Faccao.Misticos, "Dragão", "🐲", 1400, 200, 120,
            SoproDoDragao(), DragaoProtetor(), new PeleDeDragao());

        static HabilidadeAtiva SoproDoDragao() => new(
            "Sopro do Dragão", "🔥", cooldown: 3, "Ataca todos com +100% ATK e aplica Queima (2 stacks).",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(2.0),
                new AplicarDebuff(() => new Queima(stacks: 2)),
            });

        static HabilidadeAtiva DragaoProtetor() => new(
            "Dragão Protetor", "🐲", cooldown: 3, "Contra-ataque, restaura HP máx perdido (até 25%) e cura 25% HP em todos.",
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
