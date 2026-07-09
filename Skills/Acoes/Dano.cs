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
    /// </summary>
    class Dano : Acao
    {
        private readonly Func<Combate, Combate, double> _multiplicador;

        public Dano(double multiplicador, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : this((_, _) => multiplicador, escopo, estadoAlvo) { }

        public Dano(Func<Combate, Combate, double> multiplicador, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) => _multiplicador = multiplicador;

        public override void Executar(Combate atacante, Combate alvo, List<EventoDano> eventos)
            => eventos.Add(atacante.Atacar(alvo, _multiplicador(atacante, alvo)));
    }
}
