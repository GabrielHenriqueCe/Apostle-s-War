using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Ascendentes
{
    /// <summary>
    /// Papai Noel — champ como DADO. Saco de Presente buffa os aliados, ataca todos e aplica Medo.
    /// Fábrica de Presente reduz a DEF dos inimigos antes de atacar (o próprio golpe se beneficia,
    /// pela ordem das ações). Passiva: Surpresa.Passiva.cs.
    /// </summary>
    public static class PapaiNoel
    {
        public static Personagem Definir() => new(
            4, Faccao.Ascendentes, "Papai Noel", "🎅", 1000, 200, 200,
            SacoDePresente(), FabricaDePresente(), new Surpresa());

        static HabilidadeAtiva SacoDePresente() => new(
            "Saco de Presente", "🎅", cooldown: 3, "Aplica +55% ATK nos aliados por 2 turnos." +
            "\nAplica Medo em todos os inimigos e depois ataca com 325% do ATK",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new AplicarBuff(() => new BuffAtaque(duracao: 2, percentual: 0.25), Escopo.TodosAliados),
                new Dano(3.25),
                new AplicarDebuff(() => new Medo(duracao: 1)),
            });

        static HabilidadeAtiva FabricaDePresente() => new(
            "Fábrica de Presente", "🏭", cooldown: 3, "Aplica -30% DEF nos inimigos e ataca todos com 375% do ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new AplicarDebuff(() => new ReducaoDefesa(duracao: 2)),
                new Dano(3.75),
            });
    }
}
