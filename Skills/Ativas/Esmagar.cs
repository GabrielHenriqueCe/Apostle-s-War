using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Skills.Ativas
{
    /// <summary>
    /// Cura o Ogro em 50% do HP máximo e aplica ProtecaoAliado 30% (2t) nos demais aliados.
    /// </summary>
    class Esmagar : HabilidadeAtiva
    {
        private const double CuraPercentual = 0.50;
        private const double ProtecaoPercentual = 0.30;
        private const int TurnosProtecao = 2;

        public Esmagar() : base("Esmagar", "👊", 3,
            "Cura 50% HP em si e protege os aliados (2t).")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Self;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            AplicarCura(ctx.Atacante, CuraPercentual);

            foreach (Combate a in ctx.Aliados.Where(c => c != ctx.Atacante && c.EstaVivo()))
                new ProtecaoAliado(ctx.Atacante, turnos: TurnosProtecao, percentual: ProtecaoPercentual).Aplicar(a);

            return SemDano();
        }
    }
}