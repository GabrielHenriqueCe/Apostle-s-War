namespace ApostlesWar
{
    #region Combate

    /// <summary>
    /// Resultado de um ataque: dano causado, se foi crítico, alvo e HP restante.
    /// </summary>
    record ResultadoAtaque(int Dano, bool Critico, Combate Alvo, int HPRestante);

    abstract class Combate
    {
        private static readonly Random random = new Random();
        public abstract Personagem Personagem { get; }
        public Dictionary<Habilidade, SkillCooldown> Cooldowns { get; private set; }

        /// <summary>
        /// Estado de runtime das habilidades nesta partida.
        /// Cada habilidade que precisa de estado guarda aqui, type-safe via HabilidadePassiva.ObterEstado.
        /// Nasce vazio com o Combate e morre com ele — sem vazamento entre fases.
        /// </summary>
        public Dictionary<Habilidade, object> EstadoHabilidades { get; private set; }

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
            EstadoHabilidades = new Dictionary<Habilidade, object>();
            foreach (Habilidade hab in personagem.Habilidades)
                Cooldowns[hab] = new SkillCooldown(hab.Turnos);
        }

        /// <summary>
        /// Porteiro de status — decide se um novo status pode entrar.
        /// Pergunta a cada status ativo se ele bloqueia o que está tentando entrar.
        /// Mantém SRP: o Combate não conhece tipos específicos, só orquestra a pergunta.
        /// </summary>
        public bool PodeReceber(StatusEffect novo)
        {
            foreach (var ativo in StatusAtivos)
                if (ativo.Bloqueia(novo)) return false;
            return true;
        }

        /// <summary>
        /// Calcula dano após defesa, depois pergunta aos status ativos como modificar.
        /// Cada status decide seu próprio comportamento — sem ifs específicos aqui.
        /// </summary>
        public int ReceberDano(int ataque)
        {
            double reducao = Math.Min((Defesa / 1000.0) * 0.75, 0.75);
            int danoFinal = (int)(ataque * (1 - reducao));

            // Cada status modifica o dano conforme sua própria lógica
            foreach (var status in StatusAtivos.ToList())
                danoFinal = status.ModificarDanoRecebido(this, danoFinal);

            HPAtual -= danoFinal;
            return danoFinal;
        }

        /// <summary>
        /// Aplica dano direto: ignora defesa e modificadores de status.
        /// Usado por Veneno, contra-dano de habilidades, etc.
        /// </summary>
        public int ReceberDanoDireto(int dano)
        {
            HPAtual -= dano;
            return dano;
        }

        public ResultadoAtaque Atacar(Combate alvo)
        {
            bool critico = random.NextDouble() < TaxaCrit;
            int dano = critico ? (int)(Ataque * (1 + DanoCrit)) : Ataque;
            int danoReal = alvo.ReceberDano(dano);
            return new ResultadoAtaque(danoReal, critico, alvo, Math.Max(0, alvo.HPAtual));
        }

        /// <summary>
        /// Ataca com multiplicador. Suporta ignorar percentual da defesa e forçar crítico.
        /// </summary>
        public ResultadoAtaque AtacarComMultiplicador(Combate alvo, double multiplicador,
            double ignorarDefesaPct = 0.0, bool forcaCritico = false)
        {
            bool critico = forcaCritico || random.NextDouble() < TaxaCrit;
            int danoBase = (int)(Ataque * multiplicador);
            int dano = critico ? (int)(danoBase * (1 + DanoCrit)) : danoBase;

            int defesaOriginal = alvo.Defesa;
            if (ignorarDefesaPct > 0)
                alvo.Defesa = (int)(alvo.Defesa * (1.0 - ignorarDefesaPct));

            int danoReal = alvo.ReceberDano(dano);
            alvo.Defesa = defesaOriginal;

            return new ResultadoAtaque(danoReal, critico, alvo, Math.Max(0, alvo.HPAtual));
        }

        public bool EstaVivo() => HPAtual > 0;
        public void Reviver(int hp) => HPAtual = hp;
        public void DefinirDanoCrit(double valor) => DanoCrit = valor;
        public void ModificarDefesa(int delta) => Defesa = Math.Max(0, Defesa + delta);
        public void ModificarAtaque(int delta) => Ataque = Math.Max(0, Ataque + delta);
        public void ModificarTaxaCrit(double delta) => TaxaCrit = Math.Clamp(TaxaCrit + delta, 0, 1);
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

        public void ModificarHPMaximo(int delta)
        {
            HPMaximo = Math.Max(1, HPMaximo + delta);
            HPAtual = Math.Min(HPAtual, HPMaximo);
        }
    }


    #endregion
}
