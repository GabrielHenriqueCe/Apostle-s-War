namespace ApostlesWar.Application.Portas
{
    /// <summary>
    /// O que acabou de ser narrado — a BATIDA da narrativa, não a duração dela.
    ///
    /// O motor diz "narrei um golpe"; QUANTO TEMPO isso fica na tela é decisão de pele: uma pele com
    /// sprites animados quer o tempo da animação, uma pele de teste quer zero, e o modo automático
    /// quer que passe voando. Antes o motor mandava `1500` e a pele DIVIDIA o número pra corrigir —
    /// o que é o sintoma clássico de quem não devia estar escolhendo estar escolhendo.
    /// </summary>
    public enum Momento
    {
        /// <summary>Veneno/queima/cura-contínua tickando no início do turno.</summary>
        Tick,

        /// <summary>Uma frase: "está irritado", "estava com medo", "usou X", reação de passiva.</summary>
        Narracao,

        /// <summary>Um número saltando: dano ou cura chegando num alvo.</summary>
        Golpe,

        /// <summary>O beat antes do ataque de um combatente controlado por bot ("prepara o ataque!").</summary>
        Preparacao,
    }

    /// <summary>
    /// Seam de apresentação do combate. Encapsula a ESPERA (as pausas dramáticas entre eventos) — e é
    /// o ponto único onde o cancelamento pluga: a espera ESCUTA o pedido de sair e avisa quem chamou
    /// (em vez de dormir cega). Cada Presentation traz seu adaptador; o combate não muda.
    /// (Forma 1 — espera interrompível.)
    /// </summary>
    public interface IApresentacao
    {
        /// <summary>
        /// Deixa a batida <paramref name="momento"/> respirar antes do próximo evento. Quanto dura é
        /// com a impl. Retorna TRUE se o jogador pediu pra encerrar a batalha durante a espera —
        /// quem chama decide o que fazer com isso.
        /// </summary>
        bool AguardarAnimacao(Momento momento);
    }
}
