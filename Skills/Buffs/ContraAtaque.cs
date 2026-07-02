using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Quando o portador é atacado, contra-ataca o agressor com a A1 — uma vez
    /// por agressor, por turno. Reage via IReageAoSerAtacado (dispara mesmo com
    /// dano 0 — reage ao ato).
    ///
    /// Declara o revide (Revide: Habilidade + Alvo), não executa aqui — o
    /// CombateService executa via IAtivavelComNatureza e propaga as reações do
    /// alvo revidado. O loop A↔B é quebrado por profundidade (o executor não
    /// processa Revide de um revide), não pela Natureza do golpe.
    /// </summary>
    class ContraAtaque : Buff, IReageAoSerAtacado
    {
        public ContraAtaque(int turnos = 2)
            : base("Contra-Ataque", "↩️", turnos, 0,
                "Contra-ataca ao ser atacado (1x por agressor, por turno).")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Portador.EstaVivo()) return new List<ResultadoReacao>();
            if (!ctx.Contraparte.EstaVivo()) return new List<ResultadoReacao>();

            // Regra "1x por agressor, por turno" mora no Combate (fonte única —
            // compartilhada com as passivas Herói/Operário). chance 1.0 = sempre.
            if (!ctx.Portador.TentarContraAtacar(ctx.Contraparte, 1.0)) return new List<ResultadoReacao>();

            var a1 = ctx.Portador.Personagem.Habilidades
                .OfType<IAtaquePrimario>()
                .OfType<IAtivavelComNatureza>()
                .First();

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Portador.Personagem.Nome} contra-ataca! ↩️",
                    Revide: new Revide(a1, ctx.Contraparte)
                )
            };
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}