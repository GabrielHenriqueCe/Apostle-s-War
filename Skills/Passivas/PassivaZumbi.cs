using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Sempre que recebe dano, aplica 1 stack de Veneno no atacante.
    /// </summary>
    class PassivaZumbi : HabilidadePassiva
    {
        public PassivaZumbi() : base("Putrefação Contagiosa", "🧟", 0,
            "Ao ser atacado, aplica Veneno no atacante.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeReceberDano && ctx.AlvoVivo;

        // atacante = Zumbi (portador da passiva); alvo = quem atacou o Zumbi
        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            new Veneno(stacks: 1).Aplicar(alvo);
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}
