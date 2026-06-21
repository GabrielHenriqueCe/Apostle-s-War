using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataque básico (A1) como habilidade. Sempre disponível — cooldown 0.
    /// Injetado pelo PersonagemService quando o personagem não tem A1 própria.
    /// TipoAtaque Sequencial (default): 1 alvo, passiva DepoisDeAtacar dispara normalmente.
    /// 
    /// É uma HabilidadeAtiva de primeira classe: pode virar AoE, aleatória, multi-hit
    /// ou ganhar efeitos secundários no futuro — basta sobrescrever as propriedades
    /// e ajustar o Ativar, igual qualquer outra habilidade.
    /// </summary>
    class AtaqueBasico : HabilidadeAtiva, IAtaquePrimario
    {
        public AtaqueBasico() : base("Atacar", "⚔️", turnos: 0, "Ataque básico (100% ATK).") { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var resultados = new List<EventoDano>();
            foreach (Combate a in ResolverAlvos(alvo, ObterListaPrincipal(ctx)))
                resultados.Add(ctx.Atacante.Atacar(a));
            return resultados;
        }
    }
}