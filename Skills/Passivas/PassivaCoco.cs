using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, aplica 2 stacks de Veneno no atacante.
    /// Mais agressivo que PassivaZumbi (1 stack).
    /// </summary>
    class PassivaCoco : HabilidadePassiva
    {
        public PassivaCoco() : base("Fedorento", "💩", 0,
            "Ao ser atacado, aplica 2 stacks de Veneno no atacante.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeSerAtacado && ctx.AlvoVivo;

        // ctx.Atacante = Coco (portador); alvo = quem atacou
        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            if (!alvo.EstaVivo()) return SemDano();
            new Veneno(stacks: 2).Aplicar(alvo);
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}