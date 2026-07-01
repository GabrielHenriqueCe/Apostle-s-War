using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos com +50% ATK e aplica 2 stacks de Veneno em cada.
    /// </summary>
    class Desentupidor : HabilidadeAtiva
    {
        public Desentupidor() : base("Desentupidor", "🪠", 3,
            "Ataca todos +50% ATK e aplica 2 stacks de Veneno.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                resultados.Add(AplicarDano(ctx.Atacante, a, 1.5));
                if (a.EstaVivo())
                    new Veneno(stacks: 2).Aplicar(a);
            }
            return resultados;
        }
    }
}