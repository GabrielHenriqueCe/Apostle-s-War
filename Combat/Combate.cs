using ApostlesWar.Skills.Passivas;
using ApostlesWar.Skills.Buffs;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar
{
    #region Combate

    /// <summary>
    /// Base dos EVENTOS de combate — o "fato" do que aconteceu, produzido pelo motor/ticks e
    /// consumido pela exibição (console hoje, porte amanhã). É um STREAM ordenado: dano e cura são
    /// irmãos (EventoDano/EventoCura). Embrião do log/stream da FILA B (ADR). As reações olham só os
    /// EventoDano (`.OfType<EventoDano>()`).
    /// </summary>
    abstract record EventoCombate;

    /// <summary>
    /// Descrição completa de um golpe. Produzido pelo Atacar/ReceberDano e pelos ticks de DoT,
    /// consumido pelas reações (contexto) e pela exibição. É o Model do golpe.
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
    ) : EventoCombate;

    /// <summary>
    /// Fato de uma CURA (irmão do EventoDano). Curador = quem curou (no auto-heal, = Alvo);
    /// Quantidade = HP de fato recuperado; HPRestante = HP do alvo depois.
    /// </summary>
    record EventoCura(
        Combate Curador,
        Combate Alvo,
        int Quantidade,
        int HPRestante
    ) : EventoCombate;

    abstract class Combate
    {
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

        // === Estatísticas da batalha (acumuladas na fase, pro resumo de fim) ===
        // Somadas nos funis únicos: dano em ReceberDano (pega ataque, veneno, queima, explosão),
        // cura em AplicarCura. Os Jogador são recriados por fase, então zeram naturalmente.
        public int DanoCausado { get; private set; }
        public int DanoRecebido { get; private set; }
        public int CuraRecebida { get; private set; }

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
        /// Ataque com itens e bônus permanente (Ambicao), SEM buff/debuff temporário.
        /// É a base sobre a qual BuffAtaque e ReducaoAtaque calculam seu percentual.
        /// </summary>
        public int AtaqueComStacks => AtaqueComItens + BonusAtaquePermanente;

        /// <summary>
        /// Ataque final do combatente, calculado por camadas:
        /// (base × mult + itens) + bônus permanente + buff/debuff sobre esse total.
        /// Buff/debuff contribuem via IContribuiAtaque (soma com sinal), não por tipo concreto.
        /// </summary>
        public int Ataque
        {
            get
            {
                int total = AtaqueComStacks
                    + StatusAtivos.OfType<IContribuiAtaque>().Sum(c => c.ContribuicaoAtaque(this));
                return Math.Max(0, total);
            }
        }

        // === Camadas de Defesa (stat calculado) ===
        // DefesaBase: valor cru do personagem, imutável na fase.
        // MultiplicadorDefesa: multiplicador de fase (jogador=1.0, inimigo=mult da fase).
        // ItensDefesaFlat/Pct: contribuição de itens equipados.
        // BonusDefesaPermanente: stack-builder de aumento (CoroaDoSoberano), soma no getter.
        // ReducaoDefesaPermanente: stack-builder de redução no alvo (Sorrateiro),
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
        /// Buff/debuff contribuem via IContribuiDefesa (soma com sinal) — MESMA fonte que
        /// o ReceberDano usa, sem tipo concreto.
        /// </summary>
        public int Defesa
        {
            get
            {
                int total = DefesaComStacks
                    + StatusAtivos.OfType<IContribuiDefesa>().Sum(c => c.ContribuicaoDefesa(this));
                return Math.Max(0, total);
            }
        }

        // === Camadas de TaxaCrit e DanoCrit (stats calculados) ===
        // Crit é soma de pontos absolutos (não % de %): base + itens + permanente + buff.
        public double TaxaCritBase { get; private set; }
        public double ItensTaxaCrit { get; private set; }
        public double BonusTaxaCritPermanente { get; private set; }   // OlhoClinico

        public double DanoCritBase { get; private set; }
        public double ItensDanoCrit { get; private set; }
        public double BonusDanoCritPermanente { get; private set; }    // Virus

        /// <summary>
        /// Chance de crítico final: base + itens + bônus permanente (Detetive) +
        /// BuffTaxaCrit ativo. Clamp 0..1 (0% a 100%).
        /// </summary>
        public double TaxaCrit
        {
            get
            {
                double total = TaxaCritBase + ItensTaxaCrit + BonusTaxaCritPermanente
                    + StatusAtivos.OfType<IContribuiTaxaCrit>().Sum(c => c.ContribuicaoTaxaCrit(this));
                return Math.Clamp(total, 0, 1);
            }
        }

        /// <summary>
        /// Multiplicador de dano crítico final: base + itens + bônus permanente (Invasor)
        /// + buff/debuff temporário via IContribuiDanoCrit (soma com sinal).
        /// Sem teto superior (pode passar de +100%); piso em 0.
        /// </summary>
        public double DanoCrit
        {
            get
            {
                double total = DanoCritBase + ItensDanoCrit + BonusDanoCritPermanente
                    + StatusAtivos.OfType<IContribuiDanoCrit>().Sum(c => c.ContribuicaoDanoCrit(this));
                return Math.Max(0, total);
            }
        }

        /// <summary>
        /// Os status do ESTADO ATUAL (vivo ou morto). É uma view — aponta pra lista
        /// do estado em que o combatente está agora. Aplicar/remover status opera
        /// sobre a lista do estado atual. Ao transicionar (morrer/reviver), a lista
        /// muda junto (o novo estado tem a sua).
        /// </summary>
        public List<StatusEffect> StatusAtivos => _estado.Status;

        /// <summary>
        /// Estado de vida (Vivo/Morto). Começa Vivo. Trocado pela transição no
        /// ReceberDano (HP <= 0 → Morto) e pelo revive (Morto → Vivo). Invariante:
        /// HP <= 0 ⟺ Morto.
        /// </summary>
        private EstadoVida _estado = new Vivo();

        public Combate(Personagem personagem)
        {
            HPBase = personagem.HP;
            HPMaximo = personagem.HP;
            HPAtual = personagem.HP;
            AtaqueBase = personagem.Ataque;
            DefesaBase = personagem.Defesa;
            TaxaCritBase = personagem.TaxaCrit;
            DanoCritBase = personagem.DanoCrit;
            Cooldowns = new Dictionary<Habilidade, SkillCooldown>();
            EstadoHabilidades = new Dictionary<Habilidade, object>();
            foreach (Habilidade hab in personagem.Habilidades)
                Cooldowns[hab] = new SkillCooldown(hab.Cooldown);
            Turno = new TurnoDoPersonagem(this);
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

            // Aplica buffs iniciais permanentes de passivas (ex: Espectral -> Intocavel)
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

        /// <summary>
        /// O modelo de turno deste combatente: PERSISTENTE (um por combatente, vive o combate
        /// todo). Dono do estado turn-scoped (registro de contra-ataques hoje). O CombateService
        /// chama Turno.Iniciar()/Finalizar() a cada turno em vez de criar um novo.
        /// </summary>
        public TurnoDoPersonagem Turno { get; }

        /// <summary>
        /// Fachada de contra-ataque: delega ao Turno persistente, dono do estado turn-scoped.
        /// As passivas/buffs chamam `ctx.Portador.TentarContraAtacar(...)` e não precisam saber
        /// que o registro "1x por agressor" mora no Turno. Ver TurnoDoPersonagem.TentarContraAtacar.
        /// </summary>
        public bool TentarContraAtacar(Combate agressor, double chance)
            => Turno.TentarContraAtacar(agressor, chance);

        public bool PodeReceber(StatusEffect novo)
        {
            foreach (var bloqueador in StatusAtivos.OfType<IBloqueiaStatus>())
                if (bloqueador.Bloqueia(novo)) return false;

            // Passiva-pura (Abóbora, Dragão) também bloqueia — não vive em
            // StatusAtivos, vive em Personagem.Habilidades.
            foreach (var bloqueador in Personagem.Habilidades.OfType<IBloqueiaStatus>())
                if (bloqueador.Bloqueia(novo)) return false;

            return true;
        }

        public (int Efetivo, int AbsorvidoPeloEscudo) ReceberDano(
            int ataque, NaturezaDano natureza, Combate? atacante = null,
            IEnumerable<Type>? ignorarStatus = null, double ignorarDefesaPct = 0.0)
        {
            // Uma língua só de ignorar: golpe ∪ champ (já compostos no Atacar via ComporListaIgnorar)
            // ∪ natureza.Ignora. Match por tipo EXATO ou BASE (typeof(Buff) = todos os buffs).
            var ignorados = ignorarStatus?.ToHashSet() ?? new HashSet<Type>();
            ignorados.UnionWith(natureza.Ignora);
            int danoFinal = ataque;

            if (!natureza.IgnoraDefesa)
            {
                // Monta a defesa JÁ sem os status ignorados (em vez de somar tudo e descontar depois).
                // ContribuicaoDefesa já vem com sinal (BuffDefesa +, ReducaoDefesa −), então somar os
                // não-ignorados = DefesaComStacks + todos − ignorados (idêntico ao getter Defesa).
                int defesaEfetiva = DefesaComStacks
                    + StatusAtivos.OfType<IContribuiDefesa>()
                        .Where(c => !ignorados.Any(t => t.IsAssignableFrom(((StatusEffect)c).GetType())))
                        .Sum(c => c.ContribuicaoDefesa(this));

                defesaEfetiva = (int)(defesaEfetiva * (1.0 - ignorarDefesaPct));
                defesaEfetiva = Math.Max(0, defesaEfetiva);

                double reducao = Math.Min(
                    (defesaEfetiva / DefesaPorPontoReducao) * ReducaoMaximaPorDefesa,
                    ReducaoMaximaPorDefesa);
                danoFinal = (int)(danoFinal * (1 - reducao));
            }

            int absorvidoPeloEscudo = 0;

            // Passiva-pura (Sereia) processa ANTES do Escudo/BloqueioTotal — mesma ordem
            // que o buff de contorno (ReducaoDanoFixo) tinha, aplicado no IniciarCombate
            // antes de qualquer outro status. Não participa do mecanismo de ignorados
            // (lista é de tipos de StatusEffect; passiva não é status) nem do
            // absorvidoPeloEscudo (não é Escudo).
            foreach (var modificador in Personagem.Habilidades.OfType<IModificaDanoRecebido>())
            {
                // Passiva-pura (Aquagirl) sempre age: NÃO entra na lista de ignorados de
                // propósito (lista é de tipos de status; passiva não é status — PóMágico não a fura).
                danoFinal = modificador.ModificarDanoRecebido(this, danoFinal);
            }

            foreach (var modificador in StatusAtivos.OfType<IModificaDanoRecebido>().ToList())
            {
                var status = (StatusEffect)modificador;
                if (ignorados.Any(t => t.IsAssignableFrom(status.GetType()))) continue;   // gate ÚNICO: o dano fura este status?

                int antes = danoFinal;
                danoFinal = modificador.ModificarDanoRecebido(this, danoFinal);
                if (status is Escudo)
                    absorvidoPeloEscudo += antes - danoFinal;
            }

            HPAtual -= danoFinal;

            // Estatísticas do resumo: este é o funil único de todo dano.
            DanoRecebido += danoFinal;
            if (atacante != null) atacante.DanoCausado += danoFinal;

            // Piso de HP (Invencível, via IDefineHPMinimo): o dano acima já foi contado CHEIO — só o HP
            // é clampado pro maior piso ativo. Respeita o MESMO gate de ignorados (um golpe fura o status
            // pra "matar através"). Fica FORA da mitigação de dano de propósito: assim o DanoEfetivo segue
            // integral e o lifesteal enxerga o valor real, mesmo com o portador em 1 HP.
            var pisos = StatusAtivos.OfType<IDefineHPMinimo>()
                .Where(s => !ignorados.Any(t => t.IsAssignableFrom(((StatusEffect)s).GetType())))
                .Select(s => s.HPMinimo());
            if (pisos.Any())
                HPAtual = Math.Max(HPAtual, pisos.Max());

            // Confirma a morte AQUI (ponto único de dano — ataque, Veneno, Queima, explosão,
            // reflexo, todos passam por ReceberDano). Antes de finalizar, consulta o prevent-death
            // (IPrevineMorte, mesmo padrão do IModificaDanoRecebido acima): o Guarda sobrevive SEM
            // perder os status, porque nunca chega no `new Morto()`. As reações de morte (Vilão,
            // Necromancia) seguem no fluxo, lendo o estado já resolvido.
            ConfirmarMorte();

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

            bool critico = forcaCritico || Random.Shared.NextDouble() < TaxaCrit;
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

        public bool EstaVivo() => _estado.EstaVivo();
        public void Reviver(int hp) => _estado.Reviver(this, hp);

        /// <summary>
        /// Adiciona bônus permanente de DanoCrit (stack-builder Virus).
        /// Soma no getter de DanoCrit.
        /// </summary>
        public void AdicionarBonusDanoCritPermanente(double delta) =>
            BonusDanoCritPermanente += delta;


        /// <summary>
        /// Adiciona bônus permanente de Defesa (stack-builder CoroaDoSoberano).
        /// Soma no getter de Defesa, não muta a base.
        /// </summary>
        public void AdicionarBonusDefesaPermanente(int delta) =>
            BonusDefesaPermanente = Math.Max(0, BonusDefesaPermanente + delta);

        /// <summary>
        /// Adiciona redução permanente de Defesa (stack-builder Sorrateiro).
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
        /// Adiciona bônus permanente de TaxaCrit (stack-builder OlhoClinico).
        /// Soma no getter; o clamp 0..1 acontece no getter de TaxaCrit.
        /// </summary>
        public void AdicionarBonusTaxaCritPermanente(double delta) =>
            BonusTaxaCritPermanente += delta;
        public int Curar(int valor) => _estado.Curar(this, valor);

        /// <summary>
        /// Aplica a cura no HP. Chamado pelo estado Vivo. Não checar estado aqui —
        /// quem decide se cura é o EstadoVida.
        /// </summary>
        public int AplicarCura(int valor)
        {
            int antes = HPAtual;
            HPAtual = Math.Min(HPMaximo, HPAtual + valor);
            int curado = HPAtual - antes;   // só o que de fato entrou (cap no máximo)
            CuraRecebida += curado;          // funil único de cura
            return curado;
        }

        /// <summary>
        /// Confirma (ou não) a morte após um dano. Chamado SÓ pelo ReceberDano (funil único). Se o HP
        /// caiu a 0 e ainda está Vivo, dá a chance ao prevent-death (IPrevineMorte — Guarda hoje, itens
        /// no futuro; capacidade, não reação) de EVITAR a morte: se algum previne (e está fora de
        /// cooldown), o portador segue Vivo com os STATUS INTACTOS (não vira Morto). Senão, morre de
        /// fato. É o único lugar do `new Morto()`. Distingue "evitar a morte" (fica Vivo) de "reviver"
        /// (AplicarRevive, Vivo novo/limpo — Necromancia).
        /// </summary>
        private void ConfirmarMorte()
        {
            if (HPAtual > 0 || !_estado.EstaVivo()) return;

            foreach (Habilidade hab in Personagem.Habilidades)
            {
                if (hab is IPrevineMorte prevencao && Cooldowns[hab].Disponivel)
                {
                    prevencao.Prevenir(this);
                    Cooldowns[hab].Usar();
                    return;   // sobreviveu — não vira Morto, status preservados
                }
            }

            _estado = new Morto();   // morte de fato
        }

        /// <summary>
        /// Restaura o HP pra um valor fixo (usado pelo prevent-death pra "voltar à vida" partindo de
        /// HP ≤ 0). Diferente da cura, que soma a partir do HP atual (não serve quando está negativo).
        /// </summary>
        public void RestaurarVida(int hp) => HPAtual = hp;

        /// <summary>
        /// Aplica o revive: define o HP e transiciona Morto → Vivo. Chamado pelo estado
        /// Morto. A transição de volta vive aqui (invariante: HP > 0 após revive ⟺ Vivo).
        /// </summary>
        public void AplicarRevive(int hp)
        {
            HPAtual = hp;
            _estado = new Vivo();
        }

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
        /// (passivas como Drenagem que ignoram tipos específicos sempre).
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