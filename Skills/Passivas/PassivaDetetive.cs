using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;
namespace v1_Apostle_s_War.Skills.Passivas
{
    class PassivaDetetive : HabilidadePassiva
    {
        private readonly double _danoCritBase;
        public PassivaDetetive(double danoCritBase) : base("Olho Clínico", "🚬", 0,
            "+10% dano crítico por debuff ativo no time inimigo. Máx: +100%.")
        {
            _danoCritBase = danoCritBase;
        }
        // Dispara após cada ataque do Detetive
        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeAtacar;
        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
        // atacante = Detetive; lista = inimigos
        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            int totalDebuffs = lista
                .Where(i => i.EstaVivo())
                .Sum(i => i.StatusAtivos.Count(s => s is Debuff));
            double bonus = Math.Min(totalDebuffs * 0.10, 1.00);
            atacante.DefinirDanoCrit(_danoCritBase + bonus);
            return SemDano();
        }
    }
}