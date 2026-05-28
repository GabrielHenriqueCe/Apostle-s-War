using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica Provocar + RefletirDano + ContraAtaque em si mesmo, todos 2t.
    /// Tanque puro: força inimigos a atacar o Elfo, que reflete dano e contra-ataca.
    /// </summary>
    class ArvoreDoMundo : HabilidadeAtiva
    {
        public ArvoreDoMundo() : base("Árvore do Mundo", "🌳", 3,
            "Provocar + Refletir Dano + Contra-Ataque em si mesmo (2t).")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Self;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            new Provocar(turnos: 2).Aplicar(ctx.Atacante);
            new RefletirDano(turnos: 2).Aplicar(ctx.Atacante);
            new ContraAtaque(turnos: 2).Aplicar(ctx.Atacante);
            return SemDano();
        }
    }
}