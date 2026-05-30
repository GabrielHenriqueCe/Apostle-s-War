using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;
namespace v1_Apostle_s_War.Skills.Passivas
{
    class PassivaPolicial : HabilidadePassiva
    {
        public PassivaPolicial() : base("Algemas Reforçadas", "🔗", 0,
            "Atacar um inimigo Preso adiciona +1 turno ao debuff.")
        { }
        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeAtacar;
        // ctx.Atacante = Policial; alvo = inimigo atacado
        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            alvo.StatusAtivos.OfType<Preso>().FirstOrDefault()?.EstenderTurno();
            return SemDano();
        }
    }
}