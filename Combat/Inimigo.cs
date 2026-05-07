namespace ApostlesWar
{
    #region Inimigo

    /// Define os multiplicadores de HP, Ataque e Defesa aplicados aos inimigos conforme o capítulo e fase
    struct MultiplicadorFase
    {
        public float HP;
        public float Ataque;
        public float Defesa;
    }

    class Inimigo : Combate
    {
        public override Personagem Personagem { get; }

        public Inimigo(Personagem personagem, MultiplicadorFase mult) : base(personagem)
        {
            Personagem = personagem;
            HPAtual = (int)(HPAtual * mult.HP);
            Ataque = (int)(Ataque * mult.Ataque);
            Defesa = (int)(Defesa * mult.Defesa);
        }
    }

    #endregion
}
