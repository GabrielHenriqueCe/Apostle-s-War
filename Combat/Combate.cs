using v1_Apostle_s_War.Skills.Passivas;
using v1_Apostle_s_War.Skills.Buffs;
using v1_Apostle_s_War.Skills.Debuffs;

namespace ApostlesWar
{
    #region Combate

    /// <summary>
    /// Descrição completa de um golpe (o "fato" do que aconteceu). Produzido pelo
    /// Atacar/ReceberDano, consumido pelas reações (contexto) e pela exibição (console
    /// hoje, web amanhã). É o Model do golpe — descreve o que houve, sem decidir como
    /// mostrar.
    /// </summary>
    record EventoDano(
        Combate Atacante,
        Combate Alvo,
        int DanoBruto,            // valor do golpe ao chegar, antes da mitigação do alvo
        int DanoEfetivo,          // o que de fato entrou no HP (era o antigo "Dano")
        int AbsorvidoPeloEscudo,  // quanto o Escudo aparou (0 se não havia escudo)
        bool Critico,
        int HPRestante,
        NaturezaDano Natureza
    );

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

        // === Camadas de Ataque (stat calculado) ===
        // AtaqueBase: valor cru do personagem, imutável na fase.
        // MultiplicadorAtaque: multiplicador de fase (jogador=1.0, inimigo=mult da fase).
        // ItensAtaqueFlat/Pct: contribuição de itens equipados.
        // BonusAtaquePermanente: acúmulo de stack-builders (Ambicao), some no getter.
        // BuffAtaque ativo: incide sobre (base+mult+itens+permanente).
        public int AtaqueBase { get; private set; }
        public double MultiplicadorAtaque { get; protected set; } = 1.0;
        public int ItensAtaqueFlat { get; private set; }
        public double ItensAtaquePct { get; private set; }
        public int BonusAtaquePermanente { get; private set; }

        /// <summary>
        /// Ataque após base, multiplicador de fase e itens — SEM bônus permanente nem buffs.
        /// É a referência sobre a qual os stack-builders (Ambicao) calculam seu incremento.
        /// </summary>
        public int AtaqueComItens
        {
            get
            {
                int comMult = (int)(AtaqueBase * MultiplicadorAtaque);
                return comMult + ItensAtaqueFlat + (int)(comMult * ItensAtaquePct);
            }
        }

        /// <summary>
        /// Ataque final do combatente, calculado por camadas:
        /// (base × mult + itens) + bônus permanente + buff sobre esse total.
        /// </summary>
        public int Ataque
        {
            get
            {
                int comPermanente = AtaqueComItens + BonusAtaquePermanente;
                int total = comPermanente;
                var buff = StatusAtivos.OfType<BuffAtaque>().FirstOrDefault();
                if (buff != null) total += (int)(comPermanente * buff.Valor);
                return Math.Max(0, total);
            }
        }

        // === Camadas de Defesa (stat calculado) ===
        // DefesaBase: valor cru do personagem, imutável na fase.
        // MultiplicadorDefesa: multiplicador de fase (jogador=1.0, inimigo=mult da fase).
        // ItensDefesaFlat/Pct: contribuição de itens equipados.
        // BonusDefesaPermanente: stack-builder de aumento (PassivaRei), soma no getter.
        // ReducaoDefesaPermanente: stack-builder de redução no alvo (PassivaSorrateiro),
        //   subtrai no getter. Mora no alvo pra que múltiplas fontes compartilhem o cap.
        // BuffDefesa/ReducaoDefesa: temporários, incidem sobre comStacks (independentes).
        public int DefesaBase { get; private set; }
        public double MultiplicadorDefesa { get; protected set; } = 1.0;
        public int ItensDefesaFlat { get; private set; }
        public double ItensDefesaPct { get; private set; }
        public int BonusDefesaPermanente { get; private set; }
        public int ReducaoDefesaPermanente { get; private set; }

        /// <summary>
        /// Defesa após base, multiplicador de fase e itens — SEM stacks permanentes
        /// nem buffs/debuffs. Referência sobre a qual os stack-builders (Rei aumenta,
        /// Sorrateiro reduz) calculam seu incremento.
        /// </summary>
        public int DefesaComItens
        {
            get
            {
                int comMult = (int)(DefesaBase * MultiplicadorDefesa);
                return comMult + ItensDefesaFlat + (int)(comMult * ItensDefesaPct);
            }
        }

        /// <summary>
        /// Defesa com itens e stacks permanentes (Rei aumenta, Sorrateiro reduz),
        /// mas SEM buffs/debuffs temporários. É a base sobre a qual BuffDefesa e
        /// ReducaoDefesa calculam seu percentual.
        /// </summary>
        public int DefesaComStacks => DefesaComItens + BonusDefesaPermanente - ReducaoDefesaPermanente;

        /// <summary>
        /// Defesa final do combatente, calculada por camadas:
        /// (base × mult + itens) + bônus permanente − redução permanente,
        /// e então buff/debuff temporários incidindo sobre esse total (independentes).
        /// </summary>
        public int Defesa
        {
            get
            {
                int total = DefesaComStacks;

                var buff = StatusAtivos.OfType<BuffDefesa>().FirstOrDefault();
                if (buff != null) total += (int)(DefesaComStacks * buff.Valor);

                var debuff = StatusAtivos.OfType<ReducaoDefesa>().FirstOrDefault();
                if (debuff != null) total -= (int)(DefesaComStacks * debuff.Valor);

                return Math.Max(0, total);
            }
        }

        // === Camadas de TaxaCrit e DanoCrit (stats calculados) ===
        // Crit é soma de pontos absolutos (não % de %): base + itens + permanente + buff.
        public double TaxaCritBase { get; private set; }
        public double ItensTaxaCrit { get; private set; }
        public double BonusTaxaCritPermanente { get; private set; }   // PassivaDetetive

        public double DanoCritBase { get; private set; }
        public double ItensDanoCrit { get; private set; }
        public double BonusDanoCritPermanente { get; private set; }    // PassivaInvasor

        /// <summary>
        /// Chance de crítico final: base + itens + bônus permanente (Detetive) +
        /// BuffTaxaCrit ativo. Clamp 0..1 (0% a 100%).
        /// </summary>
        public double TaxaCrit
        {
            get
            {
                double total = TaxaCritBase + ItensTaxaCrit + BonusTaxaCritPermanente;
                var buff = StatusAtivos.OfType<v1_Apostle_s_War.Skills.Buffs.BuffTaxaCrit>().FirstOrDefault();
                if (buff != null) total += buff.Valor;
                return Math.Clamp(total, 0, 1);
            }
        }

        /// <summary>
        /// Multiplicador de dano crítico final: base + itens + bônus permanente (Invasor).
        /// Sem teto superior (pode passar de +100%); piso em 0.
        /// </summary>
        public double DanoCrit
        {
            get
            {
                double total = DanoCritBase + ItensDanoCrit + BonusDanoCritPermanente;
                return Math.Max(0, total);
            }
        }

        public List<StatusEffect> StatusAtivos { get; }

        public Combate(Personagem personagem)
        {
            HPBase = personagem.HP;
            HPMaximo = personagem.HP;
            HPAtual = personagem.HP;
            AtaqueBase = personagem.Ataque;
            DefesaBase = personagem.Defesa;
            TaxaCritBase = personagem.TaxaCrit;
            DanoCritBase = personagem.DanoCrit;
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
            foreach (var bloqueador in StatusAtivos.OfType<IBloqueiaStatus>())
                if (bloqueador.Bloqueia(novo)) return false;
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

        public (int Efetivo, int AbsorvidoPeloEscudo) ReceberDano(
            int ataque, NaturezaDano natureza, Combate? atacante = null,
            IEnumerable<Type>? ignorarStatus = null, double ignorarDefesaPct = 0.0)
        {
            var ignorados = ignorarStatus?.ToHashSet() ?? new HashSet<Type>();
            int danoFinal = ataque;

            if (!natureza.IgnoraDefesa)
            {
                int defesaEfetiva = Defesa;
                foreach (var contribuidor in StatusAtivos.OfType<IContribuiDefesa>())
                {
                    var status = (StatusEffect)contribuidor;
                    if (ignorados.Contains(status.GetType()))
                        defesaEfetiva -= contribuidor.ContribuicaoDefesa(this);
                }
                defesaEfetiva = (int)(defesaEfetiva * (1.0 - ignorarDefesaPct));
                defesaEfetiva = Math.Max(0, defesaEfetiva);

                double reducao = Math.Min(
                    (defesaEfetiva / DefesaPorPontoReducao) * ReducaoMaximaPorDefesa,
                    ReducaoMaximaPorDefesa);
                danoFinal = (int)(danoFinal * (1 - reducao));
            }

            int absorvidoPeloEscudo = 0;
            foreach (var modificador in StatusAtivos.OfType<IModificaDanoRecebido>().ToList())
            {
                var status = (StatusEffect)modificador;
                if (ignorados.Contains(status.GetType())) continue;   // mecanismo lista (PoMagico/Vampiro) — fica
                if (!modificador.DeveAgir(natureza)) continue;         // mecanismo natureza — agora dentro do status

                int antes = danoFinal;
                danoFinal = modificador.ModificarDanoRecebido(this, danoFinal);
                if (status is Escudo)
                    absorvidoPeloEscudo += antes - danoFinal;
            }

            HPAtual -= danoFinal;

            return (danoFinal, absorvidoPeloEscudo);
        }

        /// <summary>
        /// Ataque com multiplicador de dano, opção de ignorar % de defesa, forçar
        /// crítico e ignorar status específicos do alvo.
        /// </summary>
        public EventoDano Atacar(Combate alvo, double multiplicador,
            double ignorarDefesaPct = 0.0, bool forcaCritico = false,
            IEnumerable<Type>? ignorarStatus = null,
            NaturezaDano? natureza = null)
        {
            var nat = natureza ?? NaturezasDano.Ataque;

            bool critico = forcaCritico || random.NextDouble() < TaxaCrit;
            int danoBase = (int)(Ataque * multiplicador);
            int dano = critico ? (int)(danoBase * (1 + DanoCrit)) : danoBase;

            var ignorarFinal = ComporListaIgnorar(ignorarStatus);
            var (efetivo, absorvidoEscudo) = alvo.ReceberDano(dano, nat, this, ignorarFinal, ignorarDefesaPct);

            return new EventoDano(
                Atacante: this,
                Alvo: alvo,
                DanoBruto: dano,
                DanoEfetivo: efetivo,
                AbsorvidoPeloEscudo: absorvidoEscudo,
                Critico: critico && (efetivo + absorvidoEscudo) > 0, 
                HPRestante: Math.Max(0, alvo.HPAtual),
                Natureza: nat
            );
        }

        /// <summary>
        /// Ataque básico (multiplicador 1.0). Sobrecarga de conveniência.
        /// </summary>
        public EventoDano Atacar(Combate alvo, IEnumerable<Type>? ignorarStatus = null)
            => Atacar(alvo, 1.0, ignorarStatus: ignorarStatus);

        public bool EstaVivo() => HPAtual > 0;
        public void Reviver(int hp) => HPAtual = hp;

        /// <summary>
        /// Adiciona bônus permanente de DanoCrit (stack-builder PassivaInvasor).
        /// Soma no getter de DanoCrit.
        /// </summary>
        public void AdicionarBonusDanoCritPermanente(double delta) =>
            BonusDanoCritPermanente += delta;


        /// <summary>
        /// Adiciona bônus permanente de Defesa (stack-builder PassivaRei).
        /// Soma no getter de Defesa, não muta a base.
        /// </summary>
        public void AdicionarBonusDefesaPermanente(int delta) =>
            BonusDefesaPermanente = Math.Max(0, BonusDefesaPermanente + delta);

        /// <summary>
        /// Adiciona redução permanente de Defesa (stack-builder PassivaSorrateiro).
        /// Mora no alvo — múltiplas fontes compartilham o mesmo acúmulo e cap.
        /// Subtrai no getter de Defesa.
        /// </summary>
        public void AdicionarReducaoDefesaPermanente(int delta) =>
            ReducaoDefesaPermanente = Math.Max(0, ReducaoDefesaPermanente + delta);

        /// <summary>
        /// Adiciona bônus permanente de Ataque (stack-builders como Ambicao).
        /// Soma no getter de Ataque, não muta a base.
        /// </summary>
        public void AdicionarBonusAtaquePermanente(int delta) =>
            BonusAtaquePermanente = Math.Max(0, BonusAtaquePermanente + delta);

        /// <summary>
        /// Adiciona bônus permanente de TaxaCrit (stack-builder PassivaDetetive).
        /// Soma no getter; o clamp 0..1 acontece no getter de TaxaCrit.
        /// </summary>
        public void AdicionarBonusTaxaCritPermanente(double delta) =>
            BonusTaxaCritPermanente += delta;
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
                case TipoStat.ATKFlat: ItensAtaqueFlat += (int)item.Valor; break;
                case TipoStat.HPFlat:
                    HPMaximo += (int)item.Valor;
                    HPAtual += (int)item.Valor;
                    break;
                case TipoStat.DEFFlat: ItensDefesaFlat += (int)item.Valor; break;
                case TipoStat.HPPct:
                    HPMaximo += (int)(HPBase * item.Valor);
                    HPAtual += (int)(HPBase * item.Valor);
                    break;
                case TipoStat.DEFPct: ItensDefesaPct += item.Valor; break;
                case TipoStat.TaxaCritPct: ItensTaxaCrit += item.Valor; break;
                case TipoStat.DanoCritPct: ItensDanoCrit += item.Valor; break;
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