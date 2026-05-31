using v1_Apostle_s_War.Skills.Passivas;

namespace ApostlesWar
{
    #region Combate

    record ResultadoAtaque(int Dano, bool Critico, Combate Alvo, int HPRestante);

    abstract class Combate
    {
        private static readonly Random random = new Random();
        // Balanceamento de defesa: cada N pontos de DEF reduzem 1 ponto percentual
        // de dano, com cap máximo. Modificar aqui afeta todos os combatentes.
        private const double DefesaPorPontoReducao = 1000.0;
        private const double ReducaoMaximaPorDefesa = 0.75;
        public abstract Personagem Personagem { get; }
        public Dictionary<Habilidade, SkillCooldown> Cooldowns { get; private set; }
        public Dictionary<Habilidade, object> EstadoHabilidades { get; private set; }

        public int HPMaximo { get; protected set; }
        public int HPAtual { get; protected set; }
        public int HPBase { get; private set; }

        /// <summary>
        /// HP máximo capturado uma vez, depois de aplicar multiplicadores de fase e itens.
        /// Usado por status como Queima e Maldição que referenciam o "HP cheio" do combate.
        /// Inicializado via IniciarCombate(), chamado pelo CombateService.
        /// </summary>
        public int HPMaximoInicial { get; private set; }

        /// <summary>
        /// Total de HP máximo reduzido neste combate (acumulado).
        /// Cada habilidade redutora soma aqui; cada habilidade restauradora abate daqui.
        /// </summary>
        public int HPMaximoReduzidoTotal { get; private set; }

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
        /// Captura o HP máximo "cheio" do combate, depois de multiplicadores e itens.
        /// Também aplica buffs iniciais das passivas que implementam IPassivaInicial.
        /// Deve ser chamado APÓS toda configuração inicial estar pronta (mult + itens),
        /// e ANTES do primeiro turno.
        /// </summary>
        public void IniciarCombate()
        {
            HPMaximoInicial = HPMaximo;

            // Aplica buffs iniciais permanentes de passivas (ex: PassivaDragao -> ImunidadeEspecifica)
            foreach (var passiva in Personagem.Habilidades.OfType<IPassivaInicial>())
                passiva.AplicarInicial(this);
        }

        /// <summary>
        /// Flag que sinaliza que este combatente deve jogar um turno extra
        /// imediatamente após o atual. Setada por habilidades/passivas (ex: RatoVoador).
        /// Consumida pelo CombateService antes de executar o turno extra.
        /// </summary>
        public bool TemTurnoExtra { get; private set; }

        /// <summary>
        /// Concede um turno extra ao combatente. Acumular múltiplas concessões antes do
        /// turno acontecer não tem efeito (flag é boolean). Mas o turno extra pode disparar
        /// outro turno extra durante sua execução — RNG decide quando acaba.
        /// </summary>
        public void ConcederTurnoExtra() => TemTurnoExtra = true;

        /// <summary>
        /// Zera a flag de turno extra. Chamado pelo CombateService antes de iniciar o
        /// turno extra (não depois) pra permitir que esse próprio turno conceda outro.
        /// </summary>
        public void ConsumirTurnoExtra() => TemTurnoExtra = false;

        public bool PodeReceber(StatusEffect novo)
        {
            foreach (var ativo in StatusAtivos)
                if (ativo.Bloqueia(novo)) return false;
            return true;
        }

        /// <summary>
        /// Flag explícita: o combatente pode ser ressuscitado por habilidades que respeitem
        /// esse bloqueio? Default true. Setada como false por passivas como PassivaVilao
        /// (Sentença), que impedem revive ao matar.
        /// 
        /// Habilidades de revive que RESPEITAM esse bloqueio: Necromancia, PassivaGuarda.
        /// Habilidades que IGNORAM proposital: AnjoCaido (Diabo — "traz do inferno").
        /// 
        /// Promovida de StatusEffect (MortePermanente) pra propriedade direta porque
        /// MortePermanente era uma flag fingindo ser Debuff: nao tinha duração real,
        /// nao tinha efeito recorrente, nao tinha removedor. Tudo que fazia era virar
        /// um booleano. Como Debuff, era acidentalmente bloqueada por ImunidadeDebuffs
        /// (CantoDeSereia) — comportamento questionável que agora desaparece.
        /// </summary>
        public bool PodeReviver { get; private set; } = true;

        /// <summary>
        /// Bloqueia revive deste combatente. Operação irreversível dentro do combate.
        /// </summary>
        public void BloquearRevive() => PodeReviver = false;

        public int ReceberDano(int ataque, Combate? atacante = null, IEnumerable<Type>? ignorarStatus = null)
        {
            var ignorados = ignorarStatus?.ToHashSet() ?? new HashSet<Type>();

            int defesaEfetiva = Defesa;
            foreach (var status in StatusAtivos)
            {
                if (ignorados.Contains(status.GetType()))
                    defesaEfetiva -= status.ContribuicaoDefesa(this);
            }
            defesaEfetiva = Math.Max(0, defesaEfetiva);

            double reducao = Math.Min(
                (defesaEfetiva / DefesaPorPontoReducao) * ReducaoMaximaPorDefesa,
                ReducaoMaximaPorDefesa);
            int danoFinal = (int)(ataque * (1 - reducao));

            foreach (var status in StatusAtivos.ToList())
            {
                if (ignorados.Contains(status.GetType())) continue;
                danoFinal = status.ModificarDanoRecebido(this, danoFinal);
            }

            HPAtual -= danoFinal;

            if (atacante != null)
            {
                foreach (var status in StatusAtivos.ToList())
                {
                    status.AoSerAtacado(this, atacante, danoFinal);
                    if (danoFinal > 0)
                        status.AoReceberDano(this, atacante, danoFinal);
                }
            }

            return danoFinal;
        }

        public int ReceberDanoDireto(int dano)
        {
            HPAtual -= dano;
            return dano;
        }

        public ResultadoAtaque Atacar(Combate alvo, IEnumerable<Type>? ignorarStatus = null)
        {
            bool critico = random.NextDouble() < TaxaCrit;
            int dano = critico ? (int)(Ataque * (1 + DanoCrit)) : Ataque;

            var ignorarFinal = ComporListaIgnorar(ignorarStatus);

            int danoReal = alvo.ReceberDano(dano, this, ignorarFinal);

            // Hook: notifica status do atacante sobre dano causado (Sedento, etc)
            foreach (var status in StatusAtivos.ToList())
                status.AoCausarDano(this, alvo, danoReal);

            return new ResultadoAtaque(danoReal, critico, alvo, Math.Max(0, alvo.HPAtual));
        }

        public ResultadoAtaque AtacarComMultiplicador(Combate alvo, double multiplicador,
            double ignorarDefesaPct = 0.0, bool forcaCritico = false,
            IEnumerable<Type>? ignorarStatus = null)
        {
            bool critico = forcaCritico || random.NextDouble() < TaxaCrit;
            int danoBase = (int)(Ataque * multiplicador);
            int dano = critico ? (int)(danoBase * (1 + DanoCrit)) : danoBase;

            int defesaOriginal = alvo.Defesa;
            if (ignorarDefesaPct > 0)
                alvo.Defesa = (int)(alvo.Defesa * (1.0 - ignorarDefesaPct));

            var ignorarFinal = ComporListaIgnorar(ignorarStatus);

            int danoReal = alvo.ReceberDano(dano, this, ignorarFinal);
            alvo.Defesa = defesaOriginal;

            // Hook: notifica status do atacante sobre dano causado
            foreach (var status in StatusAtivos.ToList())
                status.AoCausarDano(this, alvo, danoReal);

            return new ResultadoAtaque(danoReal, critico, alvo, Math.Max(0, alvo.HPAtual));
        }

        public bool EstaVivo() => HPAtual > 0;
        public void Reviver(int hp) => HPAtual = hp;
        public void DefinirDanoCrit(double valor) => DanoCrit = valor;
        public void ModificarDefesa(int delta) => Defesa = Math.Max(0, Defesa + delta);
        public void ModificarAtaque(int delta) => Ataque = Math.Max(0, Ataque + delta);
        public void ModificarTaxaCrit(double delta) => TaxaCrit = Math.Clamp(TaxaCrit + delta, 0, 1);
        public void Curar(int valor) => HPAtual = Math.Min(HPMaximo, HPAtual + valor);

        /// <summary>
        /// Reduz o HP máximo do portador. HPAtual é cortado se ficar acima do novo máximo.
        /// Soma no contador HPMaximoReduzidoTotal pra rastreio por habilidades redutoras/restauradoras.
        /// </summary>
        public void ReduzirHPMaximo(int delta)
        {
            HPMaximoReduzidoTotal += delta;
            HPMaximo = Math.Max(1, HPMaximo - delta);
            HPAtual = Math.Min(HPAtual, HPMaximo);
        }

        /// <summary>
        /// Restaura HP máximo perdido. Só aumenta HPMaximo, não cura HPAtual.
        /// Limitado ao total já reduzido (não passa do HP original).
        /// </summary>
        public void RestaurarHPMaximo(int delta)
        {
            int restaurar = Math.Min(delta, HPMaximoReduzidoTotal);
            HPMaximoReduzidoTotal -= restaurar;
            HPMaximo += restaurar;
        }

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
        /// Combina a lista passada na chamada com a lista permanente do atacante
        /// (passivas como PassivaVampiro que ignoram tipos específicos sempre).
        /// </summary>
        private IEnumerable<Type>? ComporListaIgnorar(IEnumerable<Type>? extra)
        {
            var permanente = Personagem.Habilidades
                .OfType<IIgnoraStatusNoAtaque>()
                .SelectMany(p => p.TiposIgnorados);

            if (extra == null) return permanente.Any() ? permanente : null;
            return permanente.Concat(extra);
        }

        public void ModificarHPMaximo(int delta)
        {
            HPMaximo = Math.Max(1, HPMaximo + delta);
            HPAtual = Math.Min(HPAtual, HPMaximo);
        }
    }

    #endregion
}