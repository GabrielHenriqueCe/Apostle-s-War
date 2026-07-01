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
        private readonly HashSet<Combate> _jaRevidados = new();

        public ContraAtaque(int turnos = 2)
            : base("Contra-Ataque", "↩️", turnos, 0,
                "Contra-ataca ao ser atacado (1x por agressor, por turno).")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Portador.EstaVivo()) return new List<ResultadoReacao>();
            if (!ctx.Contraparte.EstaVivo()) return new List<ResultadoReacao>();

            // 1 revide por agressor, por turno.
            if (_jaRevidados.Contains(ctx.Contraparte)) return new List<ResultadoReacao>();
            _jaRevidados.Add(ctx.Contraparte);

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

        protected override void AoPassarTurno()
        {
            _jaRevidados.Clear();
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}