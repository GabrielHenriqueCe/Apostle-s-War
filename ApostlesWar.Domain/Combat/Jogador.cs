namespace ApostlesWar.Domain
{
    #region Jogador

    /// <summary>
    /// Representa um jogador com nome, índice e personagem selecionado
    /// </summary>
    public class Jogador : Combate
    {
        public override Personagem Personagem { get; }

        public Jogador(Personagem personagem) : base(personagem)
        {
            Personagem = personagem;
        }
    }

    #endregion
}
