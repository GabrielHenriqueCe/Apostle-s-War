using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// 1. Aplica ContraAtaque 2t em todos os aliados
    /// 2. Restaura HP máximo perdido em todos os aliados (cap próprio de 25% do HPMaximoInicial)
    /// 3. Cura 25% do HP máximo em todos os aliados
    /// </summary>
    class DragaoProtetor : HabilidadeAtiva
    {
        private const double CapPropioRestauracao = 0.25;
        private const double CuraPercentual = 0.25;

        public DragaoProtetor() : base("Dragão Protetor", "🐲", 3,
            "Contra-ataque, restaura HP máx perdido (até 25%) e cura 25% HP em todos.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                new ContraAtaque(turnos: 2).Aplicar(a);

                int maxRestauravel = (int)(a.HPMaximoInicial * CapPropioRestauracao);
                a.RestaurarHPMaximo(maxRestauravel);

                AplicarCura(a, CuraPercentual);
            }
            return SemDano();
        }
    }
}