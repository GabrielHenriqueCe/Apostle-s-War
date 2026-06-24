using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// 10% de chance de contra-atacar com Marretada (1.25x) ao ser atacado.
    /// Reage via IReageAoSerAtacado (dispara mesmo se o dano foi absorvido — reage
    /// ao ATO de ser atacado, não ao dano). Declara o revide (Dano + Mensagem); o
    /// CombateService exibe.
    /// </summary>
    class PassivaOperario : HabilidadePassiva, IReageAoSerAtacado
    {
        private static readonly Random _random = new Random();

        public PassivaOperario() : base("Instinto do Operário", "🛠️", 0,
            "10% de chance de contra-atacar com Marretada ao ser atacado.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (_random.NextDouble() >= 0.10) return new List<ResultadoReacao>();
            if (!ctx.Outro.EstaVivo()) return new List<ResultadoReacao>();

            // PROVISÓRIO [revide-com-habilidade]: o contra-ataque usa multiplicador
            // hardcoded (1.25, força da Marretada) + natureza Revide. Quando o refactor
            // das ativas expuser a força das habilidades, isto vira "revidar carregando
            // a HabilidadeAtiva Marretada" (nome/animação/efeito próprios), unificando
            // com o ContraAtaque. Ver "Fio do revide" no roadmap.
            var revide = ctx.Portador.Atacar(ctx.Outro, 1.25, natureza: NaturezasDano.Revide);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Portador.Personagem.Simbolo} {ctx.Portador.Personagem.Nome} contra-atacou com Marretada! 🛠️",
                    Dano: revide)
            };
        }
    }
}