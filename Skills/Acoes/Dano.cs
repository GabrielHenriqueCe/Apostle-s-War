namespace ApostlesWar
{
    /// <summary>
    /// Causa dano ao alvo com um multiplicador de ATK. O multiplicador pode depender do
    /// atacante e do alvo (ex: bônus da Piromancer contra alvo com Queima) — por isso
    /// aceita uma função. Habilidades de multiplicador fixo usam a sobrecarga com double.
    ///
    /// NOTA (ADR-composicao-de-acoes §12): o modificador do atacante (Piromancer) é passado à
    /// mão aqui por enquanto. Quando existir IModificaDanoCausado, a Dano consultará os
    /// modificadores do atacante sozinha e o multiplicador volta a ser só o número da habilidade.
    ///
    /// ignorarDefesaPct/forcaCritico/ignorarStatus espelham os parâmetros homônimos de
    /// Combate.Atacar — nascem parâmetro opcional da Ação (não bespoke). Clientes: Kunai
    /// (ignorarDefesaPct/forcaCritico); CorteDeVento (ignorarStatus: Escudo) e Vendaval
    /// (ignorarStatus: ProtecaoAliado+BuffDefesa + ignorarDefesaPct). O ignorarStatus é a
    /// LISTA de tipos de status que o golpe pula no cálculo (mecanismo 2 do fio "unificar
    /// ignorar" do ROADMAP — aqui só EXPOSTO no motor, sem unificação).
    /// </summary>
    class Dano : Acao
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

        public override void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos)
            => eventos.Add(atacante.Atacar(alvo, _multiplicador(atacante, alvo),
                ignorarDefesaPct: _ignorarDefesaPct, forcaCritico: _forcaCritico, ignorarStatus: _ignorarStatus));
    }
}
