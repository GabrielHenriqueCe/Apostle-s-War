using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Apostolos.Ascendentes
{
    /// <summary>
    /// Boneco de Neve — apóstolo como DADO. Bola de Neve ataca 1 e prende (molde da Natureza).
    /// Gelado dá Escudo (PorHP) aos aliados e ataca todos (molde do gelo/Lealdade). Passiva:
    /// Derretendo.Passiva.cs.
    /// </summary>
    public static class BonecoDeNeve
    {
        public static Personagem Definir() => new(
            1, Faccao.Ascendentes, "Boneco de Neve", "⛄", 1000, 200, 200,
            BolaDeNeve(), Gelado(), new Derretendo());

        static HabilidadeAtiva BolaDeNeve() => new(
            "Bola de Neve", "⛄", cooldown: 3, "Ataca com 425% do ATK e aplica Preso por 1 turno no alvo.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(4.25),
                new AplicarDebuff(() => new Preso(duracao: 1)),
            });

        static HabilidadeAtiva Gelado() => new(
            "Gelado", "❄️", cooldown: 3, "Aplica Escudo de 30% HP nos aliados por 2 turnos." +
            "\nDepois ataca todos 350% do ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new AplicarEscudo(Valor.PorHP(0.30), duracao: 2, Escopo.TodosAliados),
                new Dano(3.5),
            });
    }
}
