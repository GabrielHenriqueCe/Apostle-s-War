using ApostlesWar;
using ApostlesWar.Skills.Buffs;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Champs.Decaidos
{
    /// <summary>
    /// Morcego — champ como DADO. Mordida aplica Sangramento antes de atacar em área (a passiva
    /// Sedento de Sangue cura no próprio golpe). Rato Voador estreia a ação `ConcederTurnoExtra`
    /// (1º cliente real do verbo — ver catálogo): Medo nos inimigos, buffs próprios e joga de
    /// novo. Passiva: SedentoDeSangue.Passiva.cs.
    /// </summary>
    static class Morcego
    {
        public static Personagem Definir() => new(
            1, Faccao.Decaidos, "Morcego", "🦇", 800, 160, 280,
            Mordida(), RatoVoador(), new SedentoDeSangue());

        static HabilidadeAtiva Mordida() => new(
            "Mordida", "🦇", turnos: 3, "Aplica Sangramento (2t) e ataca todos com +100% ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new AplicarDebuff(() => new Sangramento(stacks: 2)),
                new Dano(2.0),
            });

        static HabilidadeAtiva RatoVoador() => new(
            "Rato Voador", "🐀", turnos: 4, "Medo 1t em todos os inimigos, +25% ATK e +25% Crit em si (2t) e ganha turno extra.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarDebuff(() => new Medo(turnos: 1), Escopo.TodosInimigos),
                new AplicarBuff(() => new BuffAtaque(turnos: 2, percentual: 0.25), Escopo.ProprioAtacante),
                new AplicarBuff(() => new BuffTaxaCrit(turnos: 2, valor: 0.25), Escopo.ProprioAtacante),
                new ConcederTurnoExtra(),
            });
    }
}
