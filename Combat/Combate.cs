namespace ApostlesWar
{
    #region Combate

    /// <summary>
    /// Conduz o combate e status
    /// </summary>
    abstract class Combate
    {
        private static readonly Random random = new Random();
        public abstract Personagem Personagem { get; }
        public int HPAtual { get; protected set; }
        public int Ataque { get; protected set; }
        public int Defesa { get; protected set; }
        public double TaxaCrit { get; protected set; }
        public double DanoCrit { get; protected set; }

        public Combate(Personagem personagem)
        {
            HPAtual = personagem.HP;
            Ataque = personagem.Ataque;
            Defesa = personagem.Defesa;
            TaxaCrit = personagem.TaxaCrit;
            DanoCrit = personagem.DanoCrit;
        }

        /// <summary>
        /// Calcula e aplica o dano recebido descontando a redução por defesa
        /// </summary>
        /// <param name="ataque">Valor de ataque do atacante</param>
        public void ReceberDano(int ataque)
        {
            double reducao = Math.Min((Defesa / 1000.0) * 0.75, 0.75);
            int danoFinal = (int)(ataque * (1 - reducao));
            HPAtual -= danoFinal;
        }

        /// <summary>
        /// Executa um ataque contra o alvo informado
        /// </summary>
        /// <param name="alvo">Combatente que receberá o dano</param>
        public void Atacar(Combate alvo)
        {
            bool critico = random.NextDouble() < TaxaCrit;
            int dano = critico ? (int)(Ataque * (1 + DanoCrit)) : Ataque;
            alvo.ReceberDano(dano);
        }

        /// <summary>
        /// Verifica se o combatente ainda está vivo
        /// </summary>
        /// <returns>True se HP atual for maior que zero</returns>
        public bool EstaVivo()
        {
            return HPAtual > 0;
        }

        public void Reviver(int hp)
        {
            HPAtual = hp;
        }

        /// <summary>
        /// Aplica o stat de um item ao combatente
        /// </summary>
        public void AplicarItem(Item item)
        {
            switch (item.TipoStat)
            {
                case TipoStat.ATKFlat: Ataque += (int)item.Valor; break;
                case TipoStat.HPFlat: HPAtual += (int)item.Valor; break;
                case TipoStat.DEFFlat: Defesa += (int)item.Valor; break;
                case TipoStat.HPPct: HPAtual += (int)(HPAtual * item.Valor); break;
                case TipoStat.DEFPct: Defesa += (int)(Defesa * item.Valor); break;
                case TipoStat.TaxaCritPct: TaxaCrit += item.Valor; break;
                case TipoStat.DanoCritPct: DanoCrit += item.Valor; break;
            }
        }
    }

    #endregion
}
