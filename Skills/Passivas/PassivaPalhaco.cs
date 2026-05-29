using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao ser atacado, aplica 1 stack de Maldição no atacante.
    /// </summary>
    class PassivaPalhaco : HabilidadePassiva
    {
        public PassivaPalhaco() : base("Piada de Mau Gosto", "🤡", 0,
            "Ao ser atacado, amaldiçoa o atacante.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeSerAtacado && ctx.AlvoVivo;

        // ctx.Atacante = Palhaço (portador); alvo = quem atacou o Palhaço
        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            if (!alvo.EstaVivo()) return SemDano();
            new Maldicao(stacks: 1).Aplicar(alvo);
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}