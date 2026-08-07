using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Apostolos.Misticos
{
    /// <summary>
    /// Gênio — apóstolo como DADO. Vocabulário puro: Desejo (buff nos aliados + Maldição nos inimigos,
    /// dois escopos numa habilidade só) e Profecia (ReduçãoDEF + dano). Passiva: Realidade.Passiva.cs.
    /// </summary>
    public static class Genio
    {
        public static Personagem Definir() => new(
            1, Faccao.Misticos, "Gênio", "🧞", 1400, 120, 200,
            Desejo(), Profecia(), new Realidade());

        static HabilidadeAtiva Desejo() => new(
            "Desejo", "🪔", cooldown: 3, "+30% DEF nos aliados por 2 turnos e Maldição nos inimigos por 2 turnos.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Aliados,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarBuff(() => new BuffDefesa(duracao: 2, percentual: 0.30), Escopo.TodosAliados),
                new AplicarDebuff(() => new Maldicao(stacks: 2), Escopo.TodosInimigos),
            });

        static HabilidadeAtiva Profecia() => new(
            "Profecia", "🔮", cooldown: 3, "Aplica -30% DEF e Redução de 50% do Dano Crítico em todos os inimigos." +
            "\nEntão ataca todos com 300% do ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new AplicarDebuff(() => new ReducaoDefesa(duracao: 2)),
                new AplicarDebuff(() => new ReducaoDanoCrit(duracao: 2, valor: 0.5)),
                new Dano(3.0),
            });
    }
}
