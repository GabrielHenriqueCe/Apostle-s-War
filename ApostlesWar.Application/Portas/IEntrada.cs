namespace ApostlesWar.Application.Portas
{
    /// <summary>
    /// Tipo de um comando de navegação — a intenção do usuário, já traduzida da tecla crua.
    /// `Selecionar` carrega um número (a opção escolhida direto, ex: atalho numérico do teclado;
    /// no futuro, o clique do mouse na opção N). Os demais não usam `Numero`.
    /// </summary>
    public enum TipoComando { Cima, Baixo, Esquerda, Direita, Confirmar, Cancelar, Selecionar, Nenhum }

    /// <summary>Comando de navegação já semântico. `Numero` só é usado quando `Tipo == Selecionar`.</summary>
    public readonly record struct Comando(TipoComando Tipo, int Numero = 0);

    /// <summary>
    /// Porta de ENTRADA — o par simétrico do <see cref="IApresentacao"/> (saída). Traduz o input cru
    /// num <see cref="Comando"/> semântico; quem consome dá switch na INTENÇÃO, não na tecla. Cada
    /// Presentation traz seu adaptador (`EntradaConsole` no console, `EntradaWeb` no front) — o resto
    /// não muda. Mouse (quando o alvo for escolhido) entra pelo `Selecionar(N)`, que já é "apontar direto".
    /// </summary>
    public interface IEntrada
    {
        /// <summary>Bloqueia até o usuário dar um comando e o devolve já traduzido.</summary>
        Comando Ler();
    }

    /// <summary>
    /// Helper de navegação 1D VERTICAL com atalho numérico — substitui o antigo
    /// `ConsoleUtils.SelecionarComCursor(atual, min, max, tecla)` (aceitaNumericos=true, sem
    /// horizontal). Cima/Baixo movem 1 (com clamp); Selecionar(N) pula pra N se estiver no intervalo.
    /// A navegação horizontal e a 2D (grids) tratam os comandos direto no seu próprio loop.
    /// Mora junto da porta (e não numa view) porque é LÓGICA pura sobre <see cref="Comando"/> —
    /// qualquer pele reaproveita, e os testes cobrem sem tocar em console.
    /// </summary>
    public static class Navegacao
    {
        public static int MoverCursor(int atual, int min, int max, Comando cmd) => cmd.Tipo switch
        {
            TipoComando.Cima  => Math.Max(min, atual - 1),
            TipoComando.Baixo => Math.Min(max, atual + 1),
            TipoComando.Selecionar when cmd.Numero >= min && cmd.Numero <= max => cmd.Numero,
            _ => atual,
        };
    }
}
