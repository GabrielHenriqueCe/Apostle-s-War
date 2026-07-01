using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class Lealdade : HabilidadeAtiva
    {
        public Lealdade() : base("Lealdade", "🎖️", 3,
            "Aplica Escudo de 30% do HP máximo em todos os aliados por 2 turnos.")
        { }
        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                int pontos = (int)(a.HPMaximo * 0.30);
                AplicarBuff(a, new Escudo(pontos, turnos: 2));
            }
            return SemDano();
        }
    }
}