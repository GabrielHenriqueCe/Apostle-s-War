using ApostlesWar.Domain.Skills.Passivas;
using ApostlesWar.Domain.Skills.Buffs;
using ApostlesWar.Domain.Skills.Debuffs;

namespace ApostlesWar.Domain
{
    #region Combate

    /// <summary>
    /// Base dos EVENTOS de combate — o "fato" do que aconteceu, produzido pelo motor/ticks e
    /// consumido pela exibição (console hoje, porte amanhã). É um STREAM ordenado: dano e cura são
    /// irmãos (EventoDano/EventoCura). Embrião do log/stream da FILA B (ADR). As reações olham só os
    /// EventoDano (`.OfType<EventoDano>()`).
    /// </summary>
    public abstract record EventoCombate;

    /// <summary>
    /// Descrição completa de um golpe. Produzido pelo Atacar/ReceberDano e pelos ticks de DoT,
    /// consumido pelas reações (contexto) e pela exibição. É o Model do golpe.
    /// </summary>
    public record EventoDano(
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
    public record EventoCura(
        Combate Curador,
        Combate Alvo,
        int Quantidade,
        int HPRestante
    ) : EventoCombate;

    public abstract class Combate
    {
        /// <summary>
        /// A DEF que reduz exatamente METADE do dano — o único número a calibrar na curva
        /// <c>DEF / (DEF + k)</c> (GDD §1), e ela tem leitura direta: é o joelho.
        ///
        /// A curva NUNCA satura e NUNCA chega a 100%, então o ponto de DEF nunca vira lixo por ter
        /// passado de um número. O que ela substituiu era `min(DEF/1000 × 0,75; 0,75)`: linear até
        /// 1000 e valendo ZERO daí em diante — com o item de DEF valendo `55 × capítulo`, DOIS deles
        /// já saturavam, e o slot de defesa morria sozinho no fim da progressão.
        /// </summary>
        public const double DefesaDeMeiaReducao = 5000.0;
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

        /// <summary>Casa de quem não está numa fileira — o perfil de distância não vale, e o
        /// multiplicador dele é 1,00.</summary>
        public const int ForaDoTabuleiro = 0;

        /// <summary>
        /// A Velocidade que enche o medidor deste combatente. Hoje vem crua do
        /// <see cref="Personagem"/>: nada no motor a modifica, e o empurrão de medidor mexe na BARRA
        /// (<see cref="Medidor"/>), não na Velocidade. Quando nascer a primeira habilidade que
        /// acelera alguém, é AQUI que entram as camadas (base + itens + buff), no molde da Defesa —
        /// e quem lê a barra não precisa saber que elas apareceram.
        /// </summary>
        public int Velocidade => Personagem.Velocidade;

        /// <summary>O quanto os malefícios DELE colam. Ver <see cref="ChanceDeColarEm"/>.</summary>
        public int Precisao => Personagem.Precisao;

        /// <summary>O quanto ele escapa dos malefícios dos outros. Ver <see cref="ChanceDeColarEm"/>.</summary>
        public int Resistencia => Personagem.Resistencia;

        /// <summary>
        /// O quanto a Resistência do alvo vale contra a Precisão de quem aplica: com o fator em 2,
        /// EMPATE dá 50% e o DOBRO da Resistência garante 100%. É o botão do balanço — 1,5 deixa o
        /// controle fácil, 3 faz da Resistência um stat dominante.
        /// </summary>
        private const double FatorDaResistencia = 2.0;

        /// <summary>
        /// A chance de um malefício deste combatente COLAR no alvo (GDD §1).
        ///
        /// Satura em 100% de propósito, ao contrário da curva da DEF: na defesa se quer que nunca
        /// sature; no malefício é preciso PODER aplicar o efeito cheio, senão nenhuma habilidade faz
        /// o que está escrito nela. Sem PvP não há motivo pra negar a garantia a quem pagou por ela.
        ///
        /// Alvo sem Resistência nenhuma = sempre cola: é o caso dos bonecos de bancada, e é o que
        /// mantém a medição do KIT livre do dado.
        /// </summary>
        public double ChanceDeColarEm(Combate alvo)
        {
            if (alvo.Resistencia <= 0) return 1.0;
            return Math.Min(1.0, Precisao / (alvo.Resistencia * FatorDaResistencia));
        }

        /// <summary>
        /// Tendo colado, a chance de o malefício vir com UM TURNO A MENOS: `(1 − chance de colar) ÷ 2`.
        /// É a segunda rolagem, e ela morre junto com a primeira — quem chega a 100% de Precisão cola
        /// sempre E cola cheio, sem precisar de regra separada pra isso.
        /// </summary>
        public double ChanceDeAparaUmTurnoEm(Combate alvo) => (1.0 - ChanceDeColarEm(alvo)) / 2.0;

        /// <summary>
        /// A BARRA DE TURNO (GDD §1): enche pela própria Velocidade e, ao cruzar 100, dá o direito de
        /// agir. Quem manda nela é a <see cref="FilaDeTurnos"/> — este par de métodos existe pra que
        /// a regra viva num lugar só e a barra não seja gravável de fora.
        ///
        /// A SOBRA acima de 100 é real e CARREGA de um turno pro outro: é isso que faz um empurrão de
        /// medidor nunca se desperdiçar, mesmo dado a quem já estava pronto.
        /// </summary>
        public double Medidor { get; private set; }

        public void AcumularMedidor(double quantia) => Medidor += quantia;

        public void DescontarUmTurnoDoMedidor() => Medidor -= FilaDeTurnos.Limiar;

        /// <summary>
        /// A casa deste combatente na fileira do time: <b>1 = frente, 4 = fundo</b> (GDD §2).
        /// Preenchida no <see cref="IniciarCombate"/> com o índice em <c>Equipe.Membros</c> + 1 — a
        /// ordem da lista É a formação, de ponta a ponta (o `int[] Time` do front vira essa ordem).
        ///
        /// Fica <see cref="ForaDoTabuleiro"/> em quem nasce fora de uma equipe: os bonecos montados
        /// na mão dos testes. Não é descuido — sem as duas casas não existe distância.
        /// </summary>
        public int Casa { get; private set; } = ForaDoTabuleiro;

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
        /// <param name="casa">A casa na fileira (1 = frente … 4 = fundo), ou
        /// <see cref="ForaDoTabuleiro"/> pra quem luta sem formação. Ver <see cref="Casa"/>.</param>
        public void IniciarCombate(int casa)
        {
            Casa = casa;
            HPMaximoInicial = HPMaximo;
            Medidor = 0;   // todo mundo larga do zero: a 1ª vez é a ordem das Velocidades, sem herança da rodada anterior

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

        /// <summary>
        /// Fachada do orçamento de reação "1x por agressor por turno", POR CHAVE — delega ao Turno.
        /// As reações (Espinhos/Zumbi/Coco) chamam `ctx.Portador.TentarReagir(chave, agressor, chance)`
        /// no início do AoSerAtacado; se false, não disparam. Ver TurnoDoPersonagem.TentarReagir.
        /// </summary>
        public bool TentarReagir(object chave, Combate agressor, double chance)
            => Turno.TentarReagir(chave, agressor, chance);

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

        /// <summary>
        /// A lista unida de "quais status este golpe FURA": golpe ∪ apóstolo (já compostos no Atacar
        /// via ComporListaIgnorar) ∪ natureza.Ignora. Match por tipo EXATO ou BASE
        /// (typeof(Buff) = todos os buffs). Extraída pra o Prever e o Receber usarem a MESMA.
        /// </summary>
        private static HashSet<Type> ComporIgnorados(NaturezaDano natureza, IEnumerable<Type>? ignorarStatus)
        {
            var ignorados = ignorarStatus?.ToHashSet() ?? new HashSet<Type>();
            ignorados.UnionWith(natureza.Ignora);
            return ignorados;
        }

        private bool EIgnorado(HashSet<Type> ignorados, object status)
            => ignorados.Any(t => t.IsAssignableFrom(((StatusEffect)status).GetType()));

        /// <summary>
        /// Os modificadores de dano na ordem de MITIGAÇÃO (<see cref="OrdemDeMitigacao"/>), não na de
        /// aplicação: quem reduz de graça antes de quem gasta recurso. Extraída pra o Prever e o
        /// Receber usarem a MESMA — se divergirem, o bot mira errado em silêncio.
        ///
        /// `OrderBy` do LINQ é ESTÁVEL, então dentro do mesmo balde a ordem de aplicação continua
        /// valendo: quem gasta escudo × quem gasta HP de aliado não têm ordem "certa" entre si, e não
        /// é este PR que vai inventar uma. O `ToList` materializa antes do laço porque o Modificar
        /// pode remover o status de `StatusAtivos` no meio do caminho (o Escudo zerado se remove).
        /// </summary>
        private static List<IModificaDanoRecebido> OrdenarPorMitigacao(IEnumerable<StatusEffect> status)
            => status.OfType<IModificaDanoRecebido>()
                .OrderBy(m => m.OrdemDeMitigacao)
                .ToList();

        /// <summary>
        /// O dano depois da DEFESA — a parte da mitigação que só depende de stats. Pura.
        /// </summary>
        private int AplicarDefesa(int dano, NaturezaDano natureza, HashSet<Type> ignorados, double ignorarDefesaPct)
        {
            if (natureza.IgnoraDefesa) return dano;

            // Monta a defesa JÁ sem os status ignorados (em vez de somar tudo e descontar depois).
            // ContribuicaoDefesa já vem com sinal (BuffDefesa +, ReducaoDefesa −), então somar os
            // não-ignorados = DefesaComStacks + todos − ignorados (idêntico ao getter Defesa).
            int defesaEfetiva = DefesaComStacks
                + StatusAtivos.OfType<IContribuiDefesa>()
                    .Where(c => !EIgnorado(ignorados, c))
                    .Sum(c => c.ContribuicaoDefesa(this));

            defesaEfetiva = (int)(defesaEfetiva * (1.0 - ignorarDefesaPct));
            defesaEfetiva = Math.Max(0, defesaEfetiva);

            double reducao = defesaEfetiva / (defesaEfetiva + DefesaDeMeiaReducao);
            return (int)(dano * (1 - reducao));
        }

        /// <summary>
        /// Quanto dano ENTRARIA no HP se este golpe acontecesse agora — sem que ele aconteça.
        /// Espelho PURO do <see cref="ReceberDano"/>: mesma defesa, mesma ordem de modificadores
        /// (passiva-pura antes dos status), mesmo gate de ignorados. Não consome escudo, não
        /// redireciona pro protetor, não mata ninguém.
        ///
        /// Existe pro bot comparar alvos sem alterar a batalha que está avaliando. Se um dia os dois
        /// divergirem, o bot mira errado em silêncio — por isso há teste amarrando um ao outro.
        /// </summary>
        public int PreverDanoRecebido(
            int ataque, NaturezaDano natureza,
            IEnumerable<Type>? ignorarStatus = null, double ignorarDefesaPct = 0.0)
        {
            var ignorados = ComporIgnorados(natureza, ignorarStatus);
            int danoFinal = AplicarDefesa(ataque, natureza, ignorados, ignorarDefesaPct);

            foreach (var modificador in Personagem.Habilidades.OfType<IModificaDanoRecebido>())
                danoFinal = modificador.PreverDanoRecebido(this, danoFinal);

            foreach (var modificador in OrdenarPorMitigacao(StatusAtivos))
            {
                if (EIgnorado(ignorados, modificador)) continue;
                danoFinal = modificador.PreverDanoRecebido(this, danoFinal);
            }

            return danoFinal;
        }

        /// <summary>
        /// Quanta VIDA um golpe de `dano` de fato tiraria deste combatente — o dano capado pelo que
        /// há entre o HP atual e o piso (Invencível e afins, via <see cref="IDefineHPMinimo"/>).
        ///
        /// É a métrica que o bot compara, e não o dano cru, porque ela resolve sozinha duas regras
        /// que pareciam precisar de código próprio: alvo com bloqueio total dá 0 (o Prever já viu o
        /// modificador) e alvo Invencível dá quase 0 (o piso barra). E o mesmo número diz se MATA:
        /// `PreverVidaRemovida(d) == HPAtual`.
        /// </summary>
        public int PreverVidaRemovida(int dano)
        {
            var pisos = StatusAtivos.OfType<IDefineHPMinimo>().Select(s => s.HPMinimo()).ToList();
            int piso = pisos.Count > 0 ? pisos.Max() : 0;
            return Math.Clamp(dano, 0, Math.Max(0, HPAtual - piso));
        }

        public (int Efetivo, int AbsorvidoPeloEscudo) ReceberDano(
            int ataque, NaturezaDano natureza, Combate? atacante = null,
            IEnumerable<Type>? ignorarStatus = null, double ignorarDefesaPct = 0.0)
        {
            var ignorados = ComporIgnorados(natureza, ignorarStatus);
            int danoFinal = AplicarDefesa(ataque, natureza, ignorados, ignorarDefesaPct);

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

            foreach (var modificador in OrdenarPorMitigacao(StatusAtivos))
            {
                var status = (StatusEffect)modificador;
                if (EIgnorado(ignorados, modificador)) continue;   // gate ÚNICO: o dano fura este status?

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
                .Where(s => !EIgnorado(ignorados, s))
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
        /// O quanto a GEOMETRIA aumenta (ou corta) o golpe deste combatente neste alvo: o perfil de
        /// distância do tipo dele, lido na distância entre as duas casas (GDD §2). 1,00 se qualquer
        /// um dos dois estiver <see cref="ForaDoTabuleiro"/>.
        ///
        /// Entra do lado do ATACANTE, antes da mitigação — é irmão do ATK, não da DEF, e por isso
        /// NÃO tem nada a ver com a <c>OrdemDeMitigacao</c> (#185).
        ///
        /// A distância pressupõe as duas frentes se olhando, que é a única geometria que o jogo tem.
        /// Golpe em ALIADO não passa por aqui: quem fere o próprio time (AutoDano, redirecionamento,
        /// reflexo, DoT) chama <see cref="ReceberDano"/> direto, não o <see cref="Atacar"/>.
        /// </summary>
        public double MultiplicadorDePosicaoContra(Combate alvo)
        {
            if (Casa == ForaDoTabuleiro || alvo.Casa == ForaDoTabuleiro) return 1.0;

            return Arquetipos.MultiplicadorDePosicao(
                Personagem.Tipo, Arquetipos.DistanciaEntreCasas(Casa, alvo.Casa));
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
            int danoBase = (int)(Ataque * multiplicador * MultiplicadorDePosicaoContra(alvo));
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

        /// <summary>
        /// Quanta VIDA este ataque tiraria do alvo, sem desferi-lo. Espelho puro do
        /// <see cref="Atacar"/>: mesmo `Ataque × multiplicador × posição`, mesma composição da lista
        /// de ignorados, e daí a previsão do lado do alvo.
        ///
        /// O CRÍTICO entra como VALOR ESPERADO (`1 + TaxaCrit × DanoCrit`), não como sorteio — o bot
        /// não pode saber o resultado do dado. Entre alvos da MESMA habilidade o crit é um fator
        /// comum e não muda o ranking; entre HABILIDADES muda, porque a Kunai do Ninja tem
        /// `forcaCritico` e crita sempre. Ignorá-lo subestimaria justamente as habilidades de crit.
        /// </summary>
        public int PreverAtaque(Combate alvo, double multiplicador,
            double ignorarDefesaPct = 0.0, bool forcaCritico = false,
            IEnumerable<Type>? ignorarStatus = null,
            NaturezaDano? natureza = null)
        {
            var nat = natureza ?? NaturezasDano.Ataque;

            double fatorCritico = forcaCritico ? 1 + DanoCrit : 1 + (TaxaCrit * DanoCrit);
            // O (int) de dentro é o MESMO do Atacar, com o multiplicador de posição no mesmo lugar:
            // truncar em ponto diferente faria o bot mirar por um número que a batalha não usa.
            int dano = (int)((int)(Ataque * multiplicador * MultiplicadorDePosicaoContra(alvo)) * fatorCritico);

            var ignorarFinal = ComporListaIgnorar(ignorarStatus);
            int passaria = alvo.PreverDanoRecebido(dano, nat, ignorarFinal, ignorarDefesaPct);
            return alvo.PreverVidaRemovida(passaria);
        }

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