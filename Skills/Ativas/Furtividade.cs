using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
namespace v1_Apostle_s_War.Skills.Ativas
{
    // Piloto do motor (ADR-composicao-de-acoes): valida ESCOPO PRÓPRIO — aplica Intocável em si
    // (ProprioAtacante) e depois ataca todos os inimigos. A ordem da lista preserva "buff → ataca".
    class Furtividade : HabilidadeAtiva
    {
        public Furtividade() : base("Furtividade", "🕳️", 4,
            "Intocável por 2 turnos. Ataca todos os inimigos com 100% ATK.")
        { }
        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        protected override List<Acao> Acoes => new()
        {
            new AplicarBuff(() => new Intocavel(turnos: 2), Escopo.ProprioAtacante),
            new Dano(1.0),
        };
    }
}
