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

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
        {
            double mult = _multiplicador(atacante, alvo);

            // Modificadores de dano do ATACANTE (Piromancer e futuros): dobram o multiplicador ANTES
            // do (int) do Atacar. Varre as duas fontes, igual o ReceberDano faz no lado do defensor.
            foreach (var m in atacante.Personagem.Habilidades.OfType<IModificaDanoCausado>())
                mult *= m.MultiplicadorDeDano(atacante, alvo);
            foreach (var m in atacante.StatusAtivos.OfType<IModificaDanoCausado>())
                mult *= m.MultiplicadorDeDano(atacante, alvo);

            eventos.Add(atacante.Atacar(alvo, mult,
                ignorarDefesaPct: _ignorarDefesaPct, forcaCritico: _forcaCritico, ignorarStatus: _ignorarStatus));
        }
    }
}
