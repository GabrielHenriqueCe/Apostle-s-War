using ApostlesWar;

namespace v1_Apostle_s_War.Champs.Humanos
{
    /// <summary>
    /// 10% de chance de contra-atacar com Marretada ao ser atacado. Reage via
    /// IReageAoSerAtacado (dispara mesmo se o dano foi absorvido — reage ao ATO
    /// de ser atacado, não ao dano). Declara o revide (Revide: Marretada +
    /// Contraparte); o CombateService executa via IAtivavelComNatureza e exibe —
    /// mesmo fluxo do ContraAtaque, só troca A1 por Marretada.
    /// </summary>
    class InstintoDoOperario : HabilidadePassiva, IReageAoSerAtacado
    {
        private const double Chance = 0.10;

        public InstintoDoOperario() : base("Instinto do Operário", "🛠️", 0,
            "10% de chance de contra-atacar com Marretada ao ser atacado.")
        { }

        public List<ResultadoReacao> AoSerAtacado(ContextoReacao ctx)
        {
            if (!ctx.Portador.EstaVivo()) return new List<ResultadoReacao>();
            if (!ctx.Contraparte.EstaVivo()) return new List<ResultadoReacao>();

            // Mesma regra do ContraAtaque/Herói, só muda a chance (10%) e a habilidade
            // (Marretada). Agora ganha o limite "1x por agressor por turno" que não tinha —
            // a rolagem de 10% e o registro vivem no Combate (fonte única).
            if (!ctx.Portador.TentarContraAtacar(ctx.Contraparte, Chance)) return new List<ResultadoReacao>();

            var marretada = ctx.Portador.Personagem.Habilidades.OfType<Marretada>().First();

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Portador.Personagem.Simbolo} {ctx.Portador.Personagem.Nome} contra-atacou com Marretada! 🛠️",
                    Revide: new Revide(marretada, ctx.Contraparte))
            };
        }
    }
}