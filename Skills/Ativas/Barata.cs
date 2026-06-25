using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Intocavel 2t em si, ataca 1 inimigo com +100% ATK (2.0x).
    /// Se este golpe matou o inimigo, aplica a Sentença (ImpedirRessurreicao).
    /// </summary>
    class Barata : HabilidadeAtiva
    {
        public Barata() : base("Barata", "🪳", 3,
            "Intocável 2t, ataca com +100% ATK. Matou? Não pode reviver.")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            new Intocavel(turnos: 2).Aplicar(ctx.Atacante);

            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                bool vivoAntes = a.EstaVivo();
                var r = AplicarDano(ctx.Atacante, a, 2.0);
                resultados.Add(r);

                if (vivoAntes && !a.EstaVivo())
                    new ImpedirRessurreicao().Aplicar(a);
            }
            return resultados;
        }
    }
}