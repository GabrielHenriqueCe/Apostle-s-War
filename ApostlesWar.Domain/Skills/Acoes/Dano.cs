namespace ApostlesWar.Domain
{
    /// <summary>
    /// Causa dano ao alvo com um multiplicador de ATK. O multiplicador pode depender do
    /// atacante e do alvo (ex: bônus da Piromancer contra alvo com Queima) — por isso
    /// aceita uma função. Habilidades de multiplicador fixo usam a sobrecarga com double.
    ///
    /// NOTA (ADR-composicao-de-acoes §12): FEITO. A Dano consulta os modificadores do atacante
    /// (IModificaDanoCausado — ex: Piromancer) sozinha no Executar, dobrando o multiplicador ANTES
    /// do (int) do Atacar. O multiplicador da habilidade voltou a ser só o número da hab.
    ///
    /// ignorarDefesaPct/forcaCritico/ignorarStatus espelham os parâmetros homônimos de
    /// Combate.Atacar — nascem parâmetro opcional da Ação (não bespoke). Clientes: Kunai
    /// (ignorarDefesaPct/forcaCritico); CorteDeVento (ignorarStatus: Escudo) e Vendaval
    /// (ignorarStatus: ProtecaoAliado+BuffDefesa + ignorarDefesaPct). O ignorarStatus é a
    /// LISTA de tipos de status que o golpe pula no cálculo (mecanismo 2 do fio "unificar
    /// ignorar" do ROADMAP — aqui só EXPOSTO no motor, sem unificação).
    /// </summary>
    public class Dano : Acao
    {
        private readonly Func<Combate, Combate, double> _multiplicador;
        private readonly double _ignorarDefesaPct;
        private readonly bool _forcaCritico;
        private readonly IEnumerable<Type>? _ignorarStatus;

        public Dano(double multiplicador, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos,
            double ignorarDefesaPct = 0.0, bool forcaCritico = false, IEnumerable<Type>? ignorarStatus = null)
            : this((_, _) => multiplicador, escopo, estadoAlvo, ignorarDefesaPct, forcaCritico, ignorarStatus) { }

        public Dano(Func<Combate, Combate, double> multiplicador, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos,
            double ignorarDefesaPct = 0.0, bool forcaCritico = false, IEnumerable<Type>? ignorarStatus = null)
            : base(escopo, estadoAlvo)
        {
            _multiplicador = multiplicador;
            _ignorarDefesaPct = ignorarDefesaPct;
            _forcaCritico = forcaCritico;
            _ignorarStatus = ignorarStatus;
        }

        public override Utilidade Utilidade => Utilidade.Ferir;

        /// <summary>
        /// Quanta VIDA este dano tiraria do alvo, sem desferi-lo — a métrica que o avaliador compara
        /// entre candidatos. Passa pelo mesmo multiplicador (incluindo os modificadores do atacante)
        /// e pela mesma previsão do lado do alvo, então bloqueio e piso de HP já entram no número.
        /// </summary>
        public override int PreverVidaRemovida(Combate atacante, Combate alvo)
        {
            double mult = MultiplicadorEfetivo(atacante, alvo);
            return atacante.PreverAtaque(alvo, mult,
                ignorarDefesaPct: _ignorarDefesaPct, forcaCritico: _forcaCritico, ignorarStatus: _ignorarStatus);
        }

        /// <summary>
        /// O multiplicador da habilidade DEPOIS dos modificadores do atacante (Piromancer e futuros).
        /// Extraído pra o Executar e a previsão usarem a MESMA conta — se divergirem, o avaliador
        /// mira por um número que o golpe não cumpre.
        /// </summary>
        private double MultiplicadorEfetivo(Combate atacante, Combate alvo)
        {
            double mult = _multiplicador(atacante, alvo);

            // Modificadores de dano do ATACANTE (Piromancer e futuros): dobram o multiplicador ANTES
            // do (int) do Atacar. Varre as duas fontes, igual o ReceberDano faz no lado do defensor.
            foreach (var m in atacante.Personagem.Habilidades.OfType<IModificaDanoCausado>())
                mult *= m.MultiplicadorDeDano(atacante, alvo);
            foreach (var m in atacante.StatusAtivos.OfType<IModificaDanoCausado>())
                mult *= m.MultiplicadorDeDano(atacante, alvo);

            return mult;
        }

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
        {
            eventos.Add(atacante.Atacar(alvo, MultiplicadorEfetivo(atacante, alvo),
                ignorarDefesaPct: _ignorarDefesaPct, forcaCritico: _forcaCritico, ignorarStatus: _ignorarStatus));
        }
    }
}
