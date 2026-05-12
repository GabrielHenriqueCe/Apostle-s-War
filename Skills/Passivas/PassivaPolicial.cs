using ApostlesWar;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao atacar um inimigo que já está Preso, garante +1 turno de duração ao debuff.
    /// Deve ser verificada pelo CombateService após cada ataque do Policial.
    /// </summary>
    class PassivaPolicial : HabilidadePassiva
    {
        public PassivaPolicial() : base("Algemas Reforçadas", "🔗", 0,
            "Atacar um inimigo Preso adiciona +1 turno ao debuff.")
        { }

        public override bool DeveAtivar(EventoCombate evento) => false; // ativada manualmente

        /// <summary>
        /// Verifica se o alvo está Preso e estende o debuff em +1 turno.
        /// Chamar no CombateService após o ataque do Policial.
        /// </summary>
        public override void Ativar(Combate alvo, List<Combate>? aliados = null)
        {
            var preso = alvo.StatusAtivos.OfType<Preso>().FirstOrDefault();
            if (preso != null)
                preso.EstenderTurno();
        }

        public override string MensagemSobreviveu(Personagem personagem) => string.Empty;
        public override string MensagemMorreu(Personagem personagem) => string.Empty;
    }
}
