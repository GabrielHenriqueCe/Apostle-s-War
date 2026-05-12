using v1_Apostle_s_War.Skills.Buffs;

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
        public Dictionary<Habilidade, SkillCooldown> Cooldowns { get; private set; }
        public int HPMaximo { get; protected set; }
        public int HPAtual { get; protected set; }
        public int HPBase { get; private set; }
        public int Ataque { get; protected set; }
        public int Defesa { get; protected set; }
        public double TaxaCrit { get; protected set; }
        public double DanoCrit { get; protected set; }
        public List<StatusEffect> StatusAtivos { get; }

        public Combate(Personagem personagem)
        {
            HPBase = personagem.HP;
            HPMaximo = personagem.HP;
            HPAtual = personagem.HP;
            Ataque = personagem.Ataque;
            Defesa = personagem.Defesa;
            TaxaCrit = personagem.TaxaCrit;
            DanoCrit = personagem.DanoCrit;
            StatusAtivos = new List<StatusEffect>();
            Cooldowns = new Dictionary<Habilidade, SkillCooldown>();
            foreach (Habilidade hab in personagem.Habilidades)
            {
                Cooldowns[hab] = new SkillCooldown(hab.Turnos);
            }
        }

        /// <summary>
        /// Calcula e aplica o dano recebido descontando a redução por defesa
        /// </summary>
        /// <param name="ataque">Valor de ataque do atacante</param>
        public int ReceberDano(int ataque)
        {
            if (StatusAtivos.Any(s => s is BloqueioTotal || s is Intocavel)) return 0;
            double reducao = Math.Min((Defesa / 1000.0) * 0.75, 0.75);
            int danoFinal = (int)(ataque * (1 - reducao));
            HPAtual -= danoFinal;
            return danoFinal;
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
                case TipoStat.HPFlat:
                    HPMaximo += (int)item.Valor;
                    HPAtual += (int)item.Valor;
                    break;
                case TipoStat.DEFFlat: Defesa += (int)item.Valor; break;
                case TipoStat.HPPct:
                    HPMaximo += (int)(HPBase * item.Valor);
                    HPAtual += (int)(HPBase * item.Valor);
                    break;
                case TipoStat.DEFPct: Defesa += (int)(Defesa * item.Valor); break;
                case TipoStat.TaxaCritPct: TaxaCrit += item.Valor; break;
                case TipoStat.DanoCritPct: DanoCrit += item.Valor; break;
            }
        }

        /// <summary>
        /// Define o DanoCrit diretamente. Usado por passivas que recalculam o stat.
        /// </summary>
        public void DefinirDanoCrit(double valor)
        {
            DanoCrit = valor;
        }

        /// <summary>
        /// Ataca o alvo com um multiplicador de dano sobre o ATK base.
        /// Usado por habilidades que escalam com o ATK do atacante.
        /// </summary>
        public void AtacarComMultiplicador(Combate alvo, double multiplicador)
        {
            bool critico = random.NextDouble() < TaxaCrit;
            int danoBase = (int)(Ataque * multiplicador);
            int dano = critico ? (int)(danoBase * (1 + DanoCrit)) : danoBase;
            alvo.ReceberDano(dano);
        }

        /// <summary>
        /// Aplica um modificador flat na Defesa (pode ser negativo para debuffs).
        /// </summary>
        public void ModificarDefesa(int delta)
        {
            Defesa = Math.Max(0, Defesa + delta);
        }

        /// <summary>
        /// Aplica um modificador flat no Ataque (pode ser negativo para debuffs).
        /// </summary>
        public void ModificarAtaque(int delta)
        {
            Ataque = Math.Max(0, Ataque + delta);
        }

        /// <summary>
        /// Cura o combatente por um valor flat, respeitando o HP máximo.
        /// </summary>
        public void Curar(int valor)
        {
            HPAtual = Math.Min(HPMaximo, HPAtual + valor);
        }

    }

    #endregion
}
