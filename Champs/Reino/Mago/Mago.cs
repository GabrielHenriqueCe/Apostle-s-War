using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Champs.Reino
{
    /// <summary>
    /// Mago — champ como DADO (ver ADR-composicao-de-acoes §10): stats + habilidades montadas
    /// como config, na forma-construtor. Este arquivo é a VIEW do champ: tudo que ele faz
    /// (números, alvos, descrições, ações) se lê aqui, sem rodar o jogo. O comportamento real
    /// (a passiva) mora ao lado, em Piromancer.cs.
    /// </summary>
    static class Mago
    {
        public static Personagem Definir() => new(
            3, Faccao.Reino, "Mago", "🧙", 1000, 280, 120,
            BolaDeFogo(), Incendio(), new Piromancer());

        static HabilidadeAtiva BolaDeFogo() => new(
            "Bola de Fogo", "🔥", turnos: 4, "Causa +100% ATK em 1 inimigo e aplica Queima (2t).",
            numeroDeAlvos: 1, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos,
            acoes: new()
            {
                new Dano((atk, alvo) => 2.0 * Piromancer.MultExtra(atk, alvo)),
                new AplicarDebuff(() => new Queima(2)),
            });

        static HabilidadeAtiva Incendio() => new(
            "Incêndio", "🌋", turnos: 4, "Ataca todos os inimigos com +50% ATK.",
            numeroDeAlvos: int.MaxValue, tipoAlvo: TipoAlvo.Explicito, tipoLista: TipoLista.Inimigos,
            estadoAlvo: EstadoAlvo.Vivos, tipoAtaque: TipoAtaque.AreaDeEfeito,
            acoes: new()
            {
                new Dano((atk, alvo) => 1.5 * Piromancer.MultExtra(atk, alvo)),
            });
    }
}
