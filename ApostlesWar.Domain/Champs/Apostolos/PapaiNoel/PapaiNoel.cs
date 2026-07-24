using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Champs.Apostolos
{
    /// <summary>
    /// Papai Noel — champ como DADO. Saco de Presente buffa os aliados, ataca todos e aplica Medo.
    /// Fábrica de Presente reduz a DEF dos inimigos antes de atacar (o próprio golpe se beneficia,
    /// pela ordem das ações). Passiva: Surpresa.Passiva.cs.
    /// </summary>
    public static class PapaiNoel
    {
        public static Personagem Definir() => new(
            4, Faccao.Apostolos, "Papai Noel", "🎅", 1000, 200, 200,
            SacoDePresente(), FabricaDePresente(), new Surpresa());

        static HabilidadeAtiva SacoDePresente() => new(
            "Saco de Presente", "🎅", cooldown: 3, "Ataca todos +75% ATK + Medo. +25% ATK aliados (2t).",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new AplicarBuff(() => new BuffAtaque(duracao: 2, percentual: 0.25), Escopo.TodosAliados),
                new Dano(1.75),
                new AplicarDebuff(() => new Medo(duracao: 1)),
            });

        static HabilidadeAtiva FabricaDePresente() => new(
            "Fábrica de Presente", "🏭", cooldown: 3, "Reduz DEF dos inimigos e ataca todos +75% ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new AplicarDebuff(() => new ReducaoDefesa(duracao: 2)),
                new Dano(1.75),
            });
    }
}
