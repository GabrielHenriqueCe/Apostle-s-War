using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;
namespace v1_Apostle_s_War.Skills.Ativas
{
    // Piloto do motor (ADR-composicao-de-acoes): valida AplicarDebuff fora do Mago —
    // aplica Preso (2t) no inimigo selecionado.
    class Prender : HabilidadeAtiva
    {
        public Prender() : base("Prender", "⛓️", 4, "Inimigo pula os próximos 2 turnos.") { }
        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        protected override List<Acao> Acoes => new()
        {
            new AplicarDebuff(() => new Preso(turnos: 2)),
        };
    }
}
