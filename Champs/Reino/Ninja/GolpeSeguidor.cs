namespace ApostlesWar.Champs.Reino
{
    /// <summary>
    /// Ataque cujo bônus de ignorar-DEF depende do golpe anterior NESTA MESMA ativação
    /// (Shuriken: se o 1º hit foi crítico, o 2º ignora 25% da DEF). Bespoke local — só a
    /// Shuriken tem esse acoplamento hit-a-hit hoje (ADR-composicao-de-acoes §9, Nível 3:
    /// "ação custom inteira", a habilidade especial vira uma Acao especial, não uma
    /// subclasse com Ativar override).
    ///
    /// Escopo.AlvosResolvidos (default) faz o interpretador chamar Executar 1x por alvo
    /// resolvido, NA ORDEM (ADR §3.3): a 1ª chamada vê `eventos` vazio (hit normal), a 2ª vê o
    /// EventoDano do hit anterior já anexado — é aí que o ignorar condicional entra.
    /// </summary>
    class GolpeSeguidor : Acao
    {
        private readonly double _multiplicador;
        private readonly double _ignorarDefesaPctSeAnteriorCritico;

        public GolpeSeguidor(double multiplicador, double ignorarDefesaPctSeAnteriorCritico,
            Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo)
        {
            _multiplicador = multiplicador;
            _ignorarDefesaPctSeAnteriorCritico = ignorarDefesaPctSeAnteriorCritico;
        }

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
        {
            var anterior = eventos.OfType<EventoDano>().LastOrDefault();
            double ignorar = anterior?.Critico == true
                ? _ignorarDefesaPctSeAnteriorCritico : 0.0;
            eventos.Add(atacante.Atacar(alvo, _multiplicador, ignorarDefesaPct: ignorar));
        }
    }
}
