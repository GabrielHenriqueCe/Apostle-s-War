namespace ApostlesWar.Domain
{
    /// <summary>
    /// Cura o alvo por um fragmento de Valor (ex: Valor.PorHP(0.30) = 30% do HP máximo do alvo).
    /// Operação separada do AplicarBuff — cura mexe direto no HP, não aplica status. A cura no
    /// morto é no-op (o estado Morto ignora Curar), mas o interpretador já filtra por EstadoAlvo.
    /// </summary>
    public class Cura : Acao
    {
        private readonly ValorFn _valor;

        public Cura(ValorFn valor, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) => _valor = valor;

        public override Utilidade Utilidade => Utilidade.Curar;

        /// <summary>Abaixo de quanto da vida cheia vale a pena gastar um turno curando.</summary>
        private const double MargemParaValerCura = 0.10;

        /// <summary>Curar quem está (quase) com a vida cheia joga a cura fora.</summary>
        public override bool TemEfeitoUtil(Combate atacante, IReadOnlyList<Combate> alvos)
            => alvos.Any(a => a.HPAtual <= a.HPMaximo * (1 - MargemParaValerCura));

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
        {
            int curado = alvo.Curar(_valor(atacante, alvo, eventos));
            // Emite mesmo curando 0 (alvo já cheio) — o combate MOSTRA que a cura rodou.
            eventos.Add(new EventoCura(atacante, alvo, curado, alvo.HPAtual));
        }
    }
}
