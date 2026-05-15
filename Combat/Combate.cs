using v1_Apostle_s_War.Skills.Buffs;

namespace ApostlesWar
{
    #region Combate

    /// <summary>
    /// Resultado de um ataque: dano causado, se foi crítico, alvo e HP restante no momento do hit.
    /// </summary>
    record ResultadoAtaque(int Dano, bool Critico, Combate Alvo, int HPRestante);

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
                Cooldowns[hab] = new SkillCooldown(hab.Turnos);
        }

        public int ReceberDano(int ataque)
        {
            if (StatusAtivos.Any(s => s is BloqueioTotal || s is Intocavel)) return 0;
            double reducao = Math.Min((Defesa / 1000.0) * 0.75, 0.75);
            int danoFinal = (int)(ataque * (1 - reducao));
            HPAtual -= danoFinal;
            return danoFinal;
        }

        public ResultadoAtaque Atacar(Combate alvo)
        {
            bool critico = random.NextDouble() < TaxaCrit;
            int dano = critico ? (int)(Ataque * (1 + DanoCrit)) : Ataque;
            int danoReal = alvo.ReceberDano(dano);
            return new ResultadoAtaque(danoReal, critico, alvo, Math.Max(0, alvo.HPAtual));
        }

        public ResultadoAtaque AtacarComMultiplicador(Combate alvo, double multiplicador)
        {
            bool critico = random.NextDouble() < TaxaCrit;
            int danoBase = (int)(Ataque * multiplicador);
            int dano = critico ? (int)(danoBase * (1 + DanoCrit)) : danoBase;
            int danoReal = alvo.ReceberDano(dano);
            return new ResultadoAtaque(danoReal, critico, alvo, Math.Max(0, alvo.HPAtual));
        }

        public bool EstaVivo() => HPAtual > 0;
        public void Reviver(int hp) => HPAtual = hp;
        public void DefinirDanoCrit(double valor) => DanoCrit = valor;
        public void ModificarDefesa(int delta) => Defesa = Math.Max(0, Defesa + delta);
        public void ModificarAtaque(int delta) => Ataque = Math.Max(0, Ataque + delta);
        public void Curar(int valor) => HPAtual = Math.Min(HPMaximo, HPAtual + valor);

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
    }

    #endregion
}
