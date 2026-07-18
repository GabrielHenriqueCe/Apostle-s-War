namespace ApostlesWar.Services
{
    /// <summary>
    /// Seam de apresentação do combate. Hoje só encapsula a ESPERA (o `Thread.Sleep` das pausas
    /// dramáticas entre eventos). É o ponto ÚNICO onde o futuro pluga sem nova cirurgia no loop de
    /// turno: (a) a animação real do ataque (dano subindo na tela etc.) nasce aqui, e (b) o
    /// cancelamento da batalha (Esc → "encerrar?") mora aqui — a espera passa a escutar o teclado e
    /// sinalizar o cancelamento em vez de dormir cego. Ver ROADMAP / plano do refactor do ExecutarTurno.
    /// </summary>
    interface IApresentacao
    {
        /// <summary>Pausa dramática entre eventos do combate. Hoje bloqueia; amanhã anima/cancela.</summary>
        void AguardarAnimacao(int ms);
    }

    /// <summary>Implementação de console: a espera é um Thread.Sleep. Trocável (front/web, testes).</summary>
    internal class ApresentacaoConsole : IApresentacao
    {
        public void AguardarAnimacao(int ms) => Thread.Sleep(ms);
    }
}
