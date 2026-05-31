using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Revive todos os aliados mortos com 50% HP e aplica Intocavel 2t nos revividos.
    /// Respeita TemBloqueioRessurreicao (MortePermanente).
    /// </summary>
    class Atlantis : HabilidadeAtiva
    {
        private const double HPRevivido = 0.50;
        private const int TurnosIntocavel = 2;

        public Atlantis() : base("Atlantis", "🌊", 4,
            "Revive aliados mortos (50% HP) e aplica Intocável.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ObterListaPrincipal(ctx))
            {
                if (a.EstaVivo()) continue;
                if (!a.PodeReviver) continue;

                a.Reviver((int)(a.HPMaximo * HPRevivido));
                new Intocavel(turnos: TurnosIntocavel).Aplicar(a);
            }

            return SemDano();
        }
    }
}