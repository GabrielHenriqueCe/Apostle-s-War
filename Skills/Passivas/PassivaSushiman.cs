using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
namespace v1_Apostle_s_War.Skills.Passivas
{
    class PassivaSushiman : HabilidadePassiva
    {
        public PassivaSushiman() : base("Código do Sushi", "🥢", 0,
            "Ao receber crítico, todos os aliados ganham 15% de reflexo de dano por 2 turnos.")
        { }
        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeReceberDano && ctx.AlvoVivo && ctx.FoiCritico;
        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
        // atacante = Sushiman; lista = aliados
        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            foreach (Combate aliado in lista.Where(a => a.EstaVivo()))
                new RefletirDano(turnos: 2, percentual: 0.15).Aplicar(aliado);
            return SemDano();
        }
    }
}