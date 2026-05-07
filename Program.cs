using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ApostlesWar
{
    #region Program

    /// <summary>
    /// Ponto de entrada do jogo
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            GerenciadorDeJogo.Executar();
        }
    }
    
    #endregion
}