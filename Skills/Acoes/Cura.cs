namespace ApostlesWar
{
    /// <summary>
    /// Cura o alvo por um fragmento de Valor (ex: Valor.PorHP(0.30) = 30% do HP máximo do alvo).
    /// Operação separada do AplicarBuff — cura mexe direto no HP, não aplica status. A cura no
    /// morto é no-op (o estado Morto ignora Curar), mas o interpretador já filtra por EstadoAlvo.
    /// </summary>
    class Cura : Acao
    {
        private readonly ValorFn _valor;

        public Cura(ValorFn valor, Escopo escopo = Escopo.AlvosResolvidos, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) => _valor = valor;

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
        {
            int curado = alvo.Curar(_valor(atacante, alvo, eventos));
            // Emite mesmo curando 0 (alvo já cheio) — o combate MOSTRA que a cura rodou.
            eventos.Add(new EventoCura(atacante, alvo, curado, alvo.HPAtual));
        }
    }
}
