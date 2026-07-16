using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.Especial
{
    /// <summary>
    /// Cocô — champ como DADO. Vocabulário puro: Descarga e Desentupidor são dano + Veneno.
    /// Passiva: PassivaCoco.Passiva.cs (nome de jogo "Fedorento").
    /// </summary>
    static class Coco
    {
        public static Personagem Definir() => new(
            1, Faccao.Especial, "Cocô", "💩", 1200, 160, 200,
            Descarga(), Desentupidor(), new PassivaCoco());

        static HabilidadeAtiva Descarga() => new(
            "Descarga", "🚽", turnos: 3, "+150% ATK e aplica 5 stacks de Veneno.",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(2.5),
                new AplicarDebuff(() => new Veneno(stacks: 5)),
            });

        static HabilidadeAtiva Desentupidor() => new(
            "Desentupidor", "🪠", turnos: 3, "Ataca todos +50% ATK e aplica 2 stacks de Veneno.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(1.5),
                new AplicarDebuff(() => new Veneno(stacks: 2)),
            });
    }
}
