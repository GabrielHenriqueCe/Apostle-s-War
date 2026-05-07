namespace ApostlesWar
{
    #region Personagem

    /// <summary>
    /// Representa o Personagem
    /// </summary>
    class Personagem
    {
        public int Slot { get; private set; }
        public Faccao Faccao { get; private set; }
        public string Nome { get; private set; }
        public string Simbolo { get; private set; }
        public int HP { get; private set; }
        public int Ataque { get; private set; }
        public int Defesa { get; private set; }
        public double TaxaCrit { get; private set; } = 0.15;
        public double DanoCrit { get; private set; } = 0.60;
        public Habilidade? Habilidade { get; private set; }

        public Personagem(int slot, Faccao faccao, string nome, string simbolo, int hp, int ataque, int def, Habilidade? habilidade = null)
        {
            Slot = slot;
            Faccao = faccao;
            Nome = nome;
            Simbolo = simbolo;
            HP = hp;
            Ataque = ataque;
            Defesa = def;
            Habilidade = habilidade;
        }
    }

    #endregion
}