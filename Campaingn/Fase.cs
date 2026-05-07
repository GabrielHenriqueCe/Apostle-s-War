using System.Collections.Generic;

namespace ApostlesWar
{
    #region Fase

    /// <summary>
    /// Define a composição de inimigos nas duas rodadas de uma fase da campanha
    /// </summary>
    class Fase
    {
        public List<Slot> Rodada1 { get; }
        public List<Slot> Rodada2 { get; }

        public Fase(List<Slot> rodada1, List<Slot> rodada2)
        {
            Rodada1 = rodada1;
            Rodada2 = rodada2;
        }
    }

    #endregion
}