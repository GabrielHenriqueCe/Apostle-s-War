using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class Kunai : HabilidadeAtiva
    {
        public Kunai() : base("Kunai", "🗡️", 4,
            "Intocável 2t. Ataca 1 inimigo +50% ATK, sempre crítico, ignora 75% DEF.")
        { }
        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            AplicarBuff(ctx.Atacante, new Intocavel(turnos: 2));

            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(ctx.Atacante.Atacar(a, 1.5,
                    ignorarDefesaPct: 0.75, forcaCritico: true));
            return resultados;
        }
    }
}