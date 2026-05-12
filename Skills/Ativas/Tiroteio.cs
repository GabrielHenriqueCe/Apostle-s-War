using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Ataca 2 inimigos aleatórios causando 75% do ATK em cada um.
    /// </summary>
    class Tiroteio : HabilidadeAtiva
    {
        public Tiroteio() : base("Tiroteio", "🔫", 4, "Ataca 2 inimigos aleatórios com 75% ATK.") { }
        public override int NumeroDeAlvos => 2;

        // Ativar base não é usado para Tiroteio — use AtivarComAtacante
        public override void Ativar(Combate alvo, List<Combate>? aliados = null) { }

        /// <summary>
        /// Deve ser chamado pelo CombateService para garantir escalonamento com ATK do atacante.
        /// aliados = lista completa de inimigos vivos para sorteio.
        /// </summary>
        public void AtivarComAtacante(Combate atacante, List<Combate> inimigos)
        {
            var alvos = inimigos
                .Where(i => i.EstaVivo())
                .OrderBy(_ => Guid.NewGuid())
                .Take(NumeroDeAlvos);

            foreach (Combate alvo in alvos)
                atacante.AtacarComMultiplicador(alvo, 0.75);
        }
    }
}
