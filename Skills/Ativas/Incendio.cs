using ApostlesWar;
using v1_Apostle_s_War.Skills.Passivas;

namespace v1_Apostle_s_War.Skills.Ativas
{
    // Piloto da composição de Ações (ver ADR-composicao-de-acoes.md): AoE de dano puro,
    // agora declara Acoes em vez de sobrescrever Ativar.
    class Incendio : HabilidadeAtiva
    {
        public Incendio() : base("Incêndio", "🌋", 4,
            "Ataca todos os inimigos com +50% ATK.")
        { }
        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        protected override List<Acao> Acoes => new()
        {
            new Dano((atk, alvo) => 1.5 * PassivaPiromancer.MultExtra(atk, alvo)),
        };
    }
}
