using ApostlesWar.Application.Portas;

namespace ApostlesWar.ConsoleUI.Views
{
    /// <summary>Console da <see cref="IApresentacao"/>: dorme em fatias curtas escutando Esc.</summary>
    public class ApresentacaoConsole : IApresentacao
    {
        public bool AguardarAnimacao(int ms)
        {
            const int fatia = 40;
            for (int passou = 0; passou < ms; passou += fatia)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    return true;
                Thread.Sleep(Math.Min(fatia, ms - passou));
            }
            return false;
        }
    }
}
