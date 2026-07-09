using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Champs.Reino
{
    /// <summary>
    /// Guarda — champ como DADO (ver ADR-composicao-de-acoes §10): stats + habilidades montadas
    /// como config, na forma-construtor. Este arquivo é a VIEW do champ. O comportamento real
    /// (a passiva) mora ao lado, em GuardaInvencivel.Passiva.cs.
    /// </summary>
    static class Guarda
    {
        public static Personagem Definir() => new(
            1, Faccao.Reino, "Guarda", "💂", 1200, 160, 200,
            Protetor(), Esgrima(), new GuardaInvencivel());

        static HabilidadeAtiva Protetor() => new(
            "Protetor", "🛡️", turnos: 4, "Aplica Provocar (2t) e Bloqueio Total (1t) em si mesmo.",
            numeroDeAlvos: 0, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Self,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.NaoAtaque,
            acoes: new()
            {
                new AplicarBuff(() => new Provocar(turnos: 2), Escopo.ProprioAtacante),
                new AplicarBuff(() => new BloqueioTotal(turnos: 1), Escopo.ProprioAtacante),
            });

        static HabilidadeAtiva Esgrima() => new(
            "Esgrima", "🤺", turnos: 3, "Ataca 2 inimigos aleatórios com +50% ATK.",
            numeroDeAlvos: 2, tipoAlvo: TipoAlvo.Aleatorio, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(1.5),
            });
    }
}
