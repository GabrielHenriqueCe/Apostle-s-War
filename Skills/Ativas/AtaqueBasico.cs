using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataque básico (A1) como habilidade. Sempre disponível — cooldown 0.
    /// Injetado pelo PersonagemService quando o personagem não tem A1 própria.
    /// TipoAtaque Sequencial (default): 1 alvo, passiva DepoisDeAtacar dispara normalmente.
    /// 
    /// É uma HabilidadeAtiva de primeira classe: pode virar AoE, aleatória, multi-hit
    /// ou ganhar efeitos secundários no futuro — basta ajustar as propriedades e as
    /// Acoes, igual qualquer outra habilidade. Híbrido do motor (ADR §4, Nível 1 +
    /// método custom): o Ativar é declarativo, o contra-ataque continua bespoke abaixo.
    /// </summary>
    class AtaqueBasico : HabilidadeAtiva, IAtaquePrimario, IAtivavelComNatureza
    {
        public AtaqueBasico() : base("Atacar", "⚔️", turnos: 0, "Ataque básico (100% ATK).") { }

        public override int NumeroDeAlvos => 1;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Inimigos;
        public override EstadoAlvo EstadoAlvo => EstadoAlvo.Vivos;

        protected override List<Acao> Acoes => new()
        {
            new Dano(1.0),
        };

        /// <summary>
        /// Entrada usada como contra-ataque (ContraAtaque busca a A1 do portador
        /// via IAtaquePrimario). Alvo já é fixo (o agressor) — não passa por
        /// ResolverAlvos.
        /// </summary>
        public EventoDano AtivarComNatureza(Combate atacante, Combate alvo, NaturezaDano natureza)
            => atacante.Atacar(alvo, 1.0, natureza: natureza);
    }
}