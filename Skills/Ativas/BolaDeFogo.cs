using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;
using v1_Apostle_s_War.Skills.Passivas;

namespace v1_Apostle_s_War.Skills.Ativas
{
    // Piloto da composição de Ações (ver ADR-composicao-de-acoes.md): era Balde 1 puro
    // (loop + Dano + Queima), agora declara Acoes em vez de sobrescrever Ativar.
    class BolaDeFogo : HabilidadeAtiva
    {
        public BolaDeFogo() : base("Bola de Fogo", "🔥", 4,
            "Causa +100% ATK em 1 inimigo e aplica Queima (2t).")
        { }
        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;

        protected override List<Acao> Acoes => new()
        {
            new Dano((atk, alvo) => 2.0 * PassivaPiromancer.MultExtra(atk, alvo)),
            new AplicarDebuff(() => new Queima(2)),
        };
    }
}
