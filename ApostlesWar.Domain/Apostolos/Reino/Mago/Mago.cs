using ApostlesWar.Domain;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain.Apostolos.Reino
{
    /// <summary>
    /// Mago — apóstolo como DADO (ver ADR-composicao-de-acoes §10): stats + habilidades montadas
    /// como config, na forma-construtor. Este arquivo é a VIEW do apóstolo: tudo que ele faz
    /// (números, alvos, descrições, ações) se lê aqui, sem rodar o jogo. O comportamento real
    /// (a passiva) mora ao lado, em Piromancer.cs.
    /// </summary>
    public static class Mago
    {
        public static Personagem Definir() => new(
            3, Faccao.Reino, "Mago", "🧙", TipoDeApostolo.Atirador,
            BolaDeFogo(), Incendio(), new Piromancer());

        static HabilidadeAtiva BolaDeFogo() => new(
            "Bola de Fogo", "🔥", cooldown: 3, "Causa 300% ATK em todos os inimigo e aplica 2 Queima.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano(3.0),   // +25% vs alvo com Queima é aplicado pela passiva (IModificaDanoCausado)
                new AplicarDebuff(() => new Queima(2)),
            });

        static HabilidadeAtiva Incendio() => new(
            "Incêndio", "🌋", cooldown: 3, "Ataca todos os inimigos com 350% ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano(3.5),   // idem: o bônus da Queima vem da passiva, não fiado na hab
            });
    }
}
