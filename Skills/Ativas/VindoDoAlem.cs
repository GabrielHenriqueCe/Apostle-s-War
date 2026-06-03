using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca 1 inimigo ignorando 100% DEF, sempre crítico (mult 1.5x ATK).
    /// Causa 20% do dano causado como auto-dano no Fantasma.
    /// </summary>
    class VindoDoAlem : HabilidadeAtiva
    {
        public VindoDoAlem() : base("Vindo do Além", "💀", 3,
            "Ataque crítico ignorando DEF. Sofre 20% do dano causado.")
        { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                var r = ctx.Atacante.Atacar(a, 1.5,
                    ignorarDefesaPct: 1.0, forcaCritico: true);
                resultados.Add(r);
                ctx.Atacante.ReceberDano((int)(r.Dano * 0.20), NaturezasDano.DanoIndireto);
            }
            return resultados;
        }
    }
}