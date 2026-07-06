using ApostlesWar;
namespace v1_Apostle_s_War.Skills.Ativas
{
    // Piloto do motor (ADR-composicao-de-acoes): valida o fragmento de VALOR — cura 30% do HP
    // máximo de cada aliado (Valor.PorHP) sobre o escopo TodosAliados (inclui o próprio).
    class Sushi : HabilidadeAtiva
    {
        public Sushi() : base("Sushi", "🍣", 4, "Cura todos os aliados em 30% do HP máximo.") { }
        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        protected override List<Acao> Acoes => new()
        {
            new Cura(Valor.PorHP(0.30), Escopo.TodosAliados),
        };
    }
}
