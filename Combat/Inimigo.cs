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
            HPMaximo = (int)(HPBase * mult.HP);
            HPAtual = (int)(HPBase * mult.HP);
            MultiplicadorAtaque = mult.Ataque;
            Defesa = (int)(Defesa * mult.Defesa);
        }
    }

    #endregion
}
