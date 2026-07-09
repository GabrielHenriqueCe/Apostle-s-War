using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Aplica Preso 1t em 2 inimigos aleatórios.
    /// </summary>
    class Abduzir : HabilidadeAtiva
    {
        public Abduzir() : base("Abduzir", "🛸", 4,
            "Incapacita 2 inimigos aleatórios por 1 turno.")
        { }

        public override int NumeroDeAlvos => 2;
        public override TipoAlvo TipoAlvo => TipoAlvo.Aleatorio;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                new Preso(turnos: 1).Aplicar(a);
            return SemDano();
        }
    }
}