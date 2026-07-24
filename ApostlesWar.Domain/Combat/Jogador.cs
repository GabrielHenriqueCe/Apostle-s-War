namespace ApostlesWar
{
    #region Jogador

    /// <summary>
    /// Representa um jogador com nome, índice e personagem selecionado
    /// </summary>
    class Jogador : Combate
    {
        public override Personagem Personagem { get; }

        public Jogador(Personagem personagem) : base(personagem)
        {
            Personagem = personagem;
        }
    }

    #endregion
}
