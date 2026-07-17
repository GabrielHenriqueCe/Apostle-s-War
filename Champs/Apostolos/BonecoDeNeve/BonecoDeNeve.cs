using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.Apostolos
{
    /// <summary>
    /// Boneco de Neve — champ como DADO. Bola de Neve ataca 1 e prende (molde da Natureza).
    /// Gelado dá Escudo (PorHP) aos aliados e ataca todos (molde do gelo/Lealdade). Passiva:
    /// Derretendo.Passiva.cs.
    /// </summary>
    static class BonecoDeNeve
    {
        public static Personagem Definir() => new(
            1, Faccao.Apostolos, "Boneco de Neve", "⛄", 1000, 200, 200,
            BolaDeNeve(), Gelado(), new Derretendo());

        static HabilidadeAtiva BolaDeNeve() => new(
            "Bola de Neve", "⛄", turnos: 3, "+175% ATK e Preso 1t no alvo.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(2.75),
                new AplicarDebuff(() => new Preso(turnos: 1)),
            });

        static HabilidadeAtiva Gelado() => new(
            "Gelado", "❄️", turnos: 4, "Escudo 30% HP nos aliados (2t) e ataca todos +75% ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new AplicarEscudo(Valor.PorHP(0.30), turnos: 2, Escopo.TodosAliados),
                new Dano(1.75),
            });
    }
}
