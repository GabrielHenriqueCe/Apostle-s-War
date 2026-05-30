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
            evento == EventoCombate.DepoisDeSerAtacado && ctx.AlvoVivo;

        // ctx.Atacante = Zumbi (portador); alvo = quem atacou o Zumbi
        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            new Veneno(stacks: 1).Aplicar(alvo);
            return SemDano();
        }
    }
}