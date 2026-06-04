using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Quando o portador é atacado (golpe de natureza Completa), contra-ataca o
    /// agressor com a1 — uma vez por agressor, por turno. Reage via
    /// IReageAoSerAtacado (dispara mesmo com dano 0 — reage ao ato).
    /// 
    /// Só reage a golpes Completa: um Revide (SemContraAtaque) NÃO provoca
    /// contra-ataque -> loop A<->B quebrado pela natureza. O revide é declarado
    /// (RevidarAlvo), não executado aqui — o CombateService o executa com
    /// natureza Revide e propaga as reações do alvo revidado.
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
            // Só reage a golpe Completa: Revide (SemContraAtaque) não gera
            // contra-ataque. (Nenhuma já nem chega aqui — dispatch filtra.)
            if (ctx.Natureza.Reacao != TipoReacao.Completa)
                return new List<ResultadoReacao>();

            if (!ctx.Portador.EstaVivo()) return new List<ResultadoReacao>();
            if (!ctx.Outro.EstaVivo()) return new List<ResultadoReacao>();

            // 1 revide por agressor, por turno.
            if (_jaRevidados.Contains(ctx.Outro)) return new List<ResultadoReacao>();
            _jaRevidados.Add(ctx.Outro);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Portador.Personagem.Nome} contra-ataca! ↩️",
                    RevidarAlvo: ctx.Outro
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