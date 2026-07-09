using ApostlesWar;
using ApostlesWar.Skills.Debuffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos com +75% ATK. Cada alvo tem 50% de chance de receber Medo 1t.
    /// </summary>
    class Pancada : HabilidadeAtiva
    {
        private const double MultiplicadorAtaque = 1.75;
        private const double ChanceMedo = 0.50;

        private static readonly Random _random = new Random();

        public Pancada() : base("Pancada", "🤜", 3,
            "Ataca todos +75% ATK e 50% chance de Medo 1t em cada.")
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
                resultados.Add(AplicarDano(ctx.Atacante, a, MultiplicadorAtaque));

                if (a.EstaVivo() && _random.NextDouble() < ChanceMedo)
                    new Medo(turnos: 1).Aplicar(a);
            }
            return resultados;
        }
    }
}