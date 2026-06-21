using ApostlesWar;
namespace v1_Apostle_s_War.Skills.Passivas
{
    class PassivaOperario : HabilidadePassiva
    {
        private static readonly Random _random = new Random();
        public PassivaOperario() : base("Instinto do Operário", "🛠️", 0,
            "10% de chance de contra-atacar com Marretada ao receber dano.")
        { }
        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeSerAtacado && ctx.AlvoVivo;
        public override string MensagemSobreviveu(Personagem p) =>
            $"{p.Simbolo} {p.Nome} contra-atacou com Marretada!";
        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo)
        {
            if (_random.NextDouble() >= 0.10) return SemDano();
            var resultado = ctx.Atacante.Atacar(alvo, 1.25);
            Console.WriteLine(MensagemSobreviveu(ctx.Atacante.Personagem));
            Thread.Sleep(1500);
            return new List<EventoDano> { resultado };
        }
    }
}