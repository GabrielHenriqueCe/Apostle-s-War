using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Revive todos os aliados mortos (50% HP) e aplica Intocavel 2t em todos os aliados
    /// vivos EXCETO o próprio Palhaço.
    /// </summary>
    class Circo : HabilidadeAtiva
    {
        private const double HPRevivido = 0.50;
        private const int TurnosIntocavel = 2;

        public Circo() : base("Circo", "🎪", 4,
            "Revive aliados (50% HP) e dá Intocável aos demais.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            // Revive todos os aliados mortos (respeita MortePermanente)
            foreach (Combate a in ObterListaPrincipal(ctx))
            {
                if (!a.EstaVivo() && a.PodeReviver)
                    a.Reviver((int)(a.HPMaximo * HPRevivido));
            }

            // Intocavel em todos os aliados vivos EXCETO o Palhaço
            foreach (Combate a in ObterListaPrincipal(ctx).Where(c => c.EstaVivo() && c != ctx.Atacante))
                new Intocavel(turnos: TurnosIntocavel).Aplicar(a);

            return SemDano();
        }
    }
}