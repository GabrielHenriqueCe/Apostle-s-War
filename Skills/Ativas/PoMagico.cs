using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca todos os inimigos com +50% ATK ignorando TODOS os buffs ativos do alvo.
    /// Detecta os buffs em runtime — qualquer Buff existente no alvo é ignorado nesse hit.
    /// </summary>
    class PoMagico : HabilidadeAtiva
    {
        public PoMagico() : base("Pó Mágico", "✨", 4,
            "Ataca todos ignorando TODOS os benefícios do alvo.")
        { }

        public override int NumeroDeAlvos => int.MaxValue;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override TipoAtaque TipoAtaque => TipoAtaque.AreaDeEfeito;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<ResultadoAtaque>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
            {
                // Lista todos os tipos concretos de Buff ativos no alvo
                var tiposBuff = a.StatusAtivos
                    .OfType<Buff>()
                    .Select(b => b.GetType())
                    .Distinct()
                    .ToArray();

                var r = ctx.Atacante.Atacar(
                    a,
                    multiplicador: 1.5,
                    ignorarStatus: tiposBuff);
                resultados.Add(r);
            }
            return resultados;
        }
    }
}