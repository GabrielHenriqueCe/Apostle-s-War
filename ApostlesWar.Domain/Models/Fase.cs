using System.Collections.Generic;

namespace ApostlesWar.Domain
{
    #region Fase

    /// <summary>
    /// Define a composição de inimigos nas duas rodadas de uma fase da campanha, por PAPEL
    /// (<see cref="TipoDeApostolo"/>) — cada facção tem exatamente um de cada, então o tipo basta
    /// pra identificar quem entra.
    ///
    /// A ORDEM da lista é a casa no tabuleiro: quem posiciona conta o índice a partir de 1
    /// (`CombateService.Posicionar`). Reordenar uma rodada muda a distância de todo mundo nela.
    /// </summary>
    public class Fase
    {
        public List<TipoDeApostolo> Rodada1 { get; }
        public List<TipoDeApostolo> Rodada2 { get; }

        public Fase(List<TipoDeApostolo> rodada1, List<TipoDeApostolo> rodada2)
        {
            Rodada1 = rodada1;
            Rodada2 = rodada2;
        }
    }

    #endregion
}