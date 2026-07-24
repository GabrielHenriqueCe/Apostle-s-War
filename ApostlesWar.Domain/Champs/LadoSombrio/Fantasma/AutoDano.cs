namespace ApostlesWar.Domain.Champs.LadoSombrio
{
    /// <summary>
    /// Dano ao PRÓPRIO atacante, por um fragmento de Valor — reusa Valor.PorDanoCausado, o
    /// mesmo fragmento de Cura/AplicarEscudo, só troca o verbo final pra dano-a-si-mesmo
    /// (ADR-composicao-de-acoes §5.5). Bespoke local — só VindoDoAlem tem esse padrão hoje
    /// (§9, Nível 2: promove no 2º cliente real).
    /// </summary>
    public class AutoDano : Acao
    {
        private readonly ValorFn _valor;

        public AutoDano(ValorFn valor, Escopo escopo = Escopo.ProprioAtacante, EstadoAlvo estadoAlvo = EstadoAlvo.Vivos)
            : base(escopo, estadoAlvo) => _valor = valor;

        public override void Executar(Combate atacante, Combate alvo, List<EventoCombate> eventos)
            => alvo.ReceberDano(_valor(atacante, alvo, eventos), NaturezasDano.DanoIndireto);
    }
}
