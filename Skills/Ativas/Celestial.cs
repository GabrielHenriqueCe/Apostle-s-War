using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Remove todos os debuffs dos aliados vivos e cura todos em 30% do HP máximo.
    /// </summary>
    class Celestial : HabilidadeAtiva
    {
        private const double CuraPercentual = 0.30;

        public Celestial() : base("Celestial", "🌟", 3,
            "Limpa debuffs dos aliados e cura 30% HP em todos.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Aliados;
        public override TipoAtaque TipoAtaque => TipoAtaque.NaoAtaque;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (Combate a in ObterListaPrincipal(ctx).Where(c => c.EstaVivo()))
            {
                var debuffs = a.StatusAtivos.OfType<Debuff>().ToList();
                foreach (var d in debuffs)
                    d.Remover(a);

                AplicarCura(a, CuraPercentual);
            }
            return SemDano();
        }
    }
}