using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Aplica Provocar 2t + RefletirDano 2t em si mesmo e Medo 1t em todos os inimigos.
    /// </summary>
    class Rugido : HabilidadeAtiva
    {
        public Rugido() : base("Rugido", "🦖", 3,
            "Provocar + Refletir Dano em si (2t) e Medo nos inimigos (1t).")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Self;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            new Provocar(turnos: 2).Aplicar(ctx.Atacante);
            new RefletirDano(turnos: 2).Aplicar(ctx.Atacante);

            foreach (Combate i in ctx.Inimigos.Where(c => c.EstaVivo()))
                new Medo(turnos: 1).Aplicar(i);

            return SemDano();
        }
    }
}