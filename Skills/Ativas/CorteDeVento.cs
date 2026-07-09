using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos ignorando Escudo (não consome, dano direto no HP).
    /// O multiplicador de ATK aumenta proporcional ao Escudo do alvo: cada ponto
    /// de escudo adiciona dano até um cap de +100% do ATK base.
    /// Anti-tanque especializado.
    /// </summary>
    class CorteDeVento : HabilidadeAtiva
    {
        private const double MultiplicadorBase = 1.0;
        private const double BonusMaximo = 1.0;  // +100% no cap

        private static readonly Type[] _ignorar = new[] { typeof(Escudo) };

        public CorteDeVento() : base("Corte de Vento", "🌬️", 3,
            "Ataca todos ignorando Escudo. Dano aumenta com escudo do alvo.")
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
                // Calcula o bônus proporcional ao escudo do alvo
                int escudoTotal = a.StatusAtivos.OfType<Escudo>().Sum(e => e.PontosRestantes);
                double proporcao = escudoTotal > 0
                    ? Math.Min((double)escudoTotal / a.HPMaximoInicial, 1.0)
                    : 0;
                double multiplicador = MultiplicadorBase + (BonusMaximo * proporcao);

                var r = ctx.Atacante.Atacar(
                    a,
                    multiplicador,
                    ignorarStatus: _ignorar);
                resultados.Add(r);
            }
            return resultados;
        }
    }
}