using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Revive todos os aliados mortos com 50% HP (respeita PodeReviver)
    /// e aplica BuffAtaque 25% por 2 turnos em todos os aliados vivos
    /// (incluindo os recém-revividos).
    /// </summary>
    class Nigiri : HabilidadeAtiva
    {
        private const double HPRevivido = 0.50;
        private const int TurnosBuffAtaque = 2;
        private const double PercentualBuffAtaque = 0.25;

        public Nigiri() : base("Nigiri", "🍙", 4,
            "Revive aliados (50% HP) e +25% ATK em todos por 2 turnos.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var lista = ObterListaPrincipal(ctx);

            foreach (Combate aliado in lista.Where(a => !a.EstaVivo() && a.PodeReviver))
                aliado.Reviver((int)(aliado.HPMaximo * HPRevivido));

            foreach (Combate a in ResolverAlvos(alvo, lista))
                AplicarBuff(a, new BuffAtaque(turnos: TurnosBuffAtaque, percentual: PercentualBuffAtaque));

            return SemDano();
        }
    }
}