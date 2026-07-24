using ApostlesWar.Application.Portas;

namespace ApostlesWar.Presentation.ConsoleUI.Views
{
    /// <summary>Implementação de console da <see cref="IEntrada"/>: lê tecla e mapeia pra <see cref="Comando"/>.</summary>
    public class EntradaConsole : IEntrada
    {
        public Comando Ler()
        {
            ConsoleKey tecla = Console.ReadKey(true).Key;
            return tecla switch
            {
                ConsoleKey.W or ConsoleKey.UpArrow    => new(TipoComando.Cima),
                ConsoleKey.S or ConsoleKey.DownArrow  => new(TipoComando.Baixo),
                ConsoleKey.A or ConsoleKey.LeftArrow  => new(TipoComando.Esquerda),
                ConsoleKey.D or ConsoleKey.RightArrow => new(TipoComando.Direita),
                ConsoleKey.Enter                       => new(TipoComando.Confirmar),
                ConsoleKey.Escape                      => new(TipoComando.Cancelar),
                >= ConsoleKey.D1 and <= ConsoleKey.D9  => new(TipoComando.Selecionar, tecla - ConsoleKey.D0),
                _                                      => new(TipoComando.Nenhum),
            };
        }
    }
}
