using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Recalcula DanoCrit do Detetive: +10% por debuff ativo no time inimigo, máximo +100%.
    /// Deve ser chamada pelo CombateService no início do turno do Detetive.
    /// </summary>
    class PassivaDetetive : HabilidadePassiva
    {
        private readonly double _danoCritBase;

        public PassivaDetetive(double danoCritBase) : base("Olho Clínico", "🚬", 0,
            "+10% dano crítico por debuff ativo no time inimigo. Máx: +100%.")
        {
            _danoCritBase = danoCritBase;
        }

        public override bool DeveAtivar(EventoCombate evento) => false; // ativada manualmente

        /// <summary>
        /// Recalcula e aplica o bônus de DanoCrit com base nos debuffs inimigos.
        /// Chamar no início do turno do Detetive, passando a lista de inimigos em aliados.
        /// </summary>
        public override void Ativar(Combate portador, List<Combate>? aliados = null)
        {
            if (aliados == null) return;

            int totalDebuffs = aliados
                .Where(i => i.EstaVivo())
                .Sum(i => i.StatusAtivos.Count(s => s is Debuff));

            double bonus = Math.Min(totalDebuffs * 0.10, 1.00);
            portador.DefinirDanoCrit(_danoCritBase + bonus);
        }

        public override string MensagemSobreviveu(Personagem personagem) => string.Empty;
        public override string MensagemMorreu(Personagem personagem) => string.Empty;
    }
}
