using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Sempre que o Elfo recebe dano, aplica 1 stack de Veneno e 1 stack de Queima
    /// no atacante. Sem cooldown — dispara em cada hit.
    /// </summary>
    class PassivaElfo : HabilidadePassiva
    {
        public PassivaElfo() : base("Espinhos", "🌿", 0,
            "Ao ser atacado, aplica Veneno e Queima no atacante.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeReceberDano && ctx.AlvoVivo;

        // ctx.Atacante = Elfo (portador); alvo = quem atacou o Elfo
        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            if (!alvo.EstaVivo()) return SemDano();

            new Veneno(stacks: 1).Aplicar(alvo);
            new Queima(stacks: 1).Aplicar(alvo);
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}