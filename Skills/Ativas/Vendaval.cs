using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca 1 inimigo com +150% ATK ignorando ProtecaoAliado e BuffDefesa,
    /// além de ignorar 50% da DEF base do alvo.
    /// </summary>
    class Vendaval : HabilidadeAtiva
    {
        public Vendaval() : base("Vendaval", "🌪️", 4,
            "+150% ATK ignorando Proteção, BuffDefesa e 50% DEF.")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;

        private static readonly Type[] _ignorar = new[]
        {
            typeof(ProtecaoAliado),
            typeof(BuffDefesa)
        };

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                var r = ctx.Atacante.Atacar(
                    a,
                    multiplicador: 2.5,             // +150%
                    ignorarDefesaPct: 0.50,
                    ignorarStatus: _ignorar);
                resultados.Add(r);
            }
            return resultados;
        }
    }
}