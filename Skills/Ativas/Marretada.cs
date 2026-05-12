using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Golpe poderoso que causa 125% do ATK em um único inimigo.
    /// </summary>
    class Marretada : HabilidadeAtiva
    {
        public Marretada() : base("Marretada", "🔨", 3, "Causa 125% do ATK em 1 inimigo.") { }
        public override int NumeroDeAlvos => 1;

        // Contrato base — não usado diretamente; use AtivarComAtacante
        public override void Ativar(Combate alvo, List<Combate>? aliados = null) { }

        /// <summary>
        /// Versão com atacante explícito — deve ser chamada pelo CombateService.
        /// </summary>
        public void AtivarComAtacante(Combate atacante, Combate alvo)
        {
            bool critico = new Random().NextDouble() < atacante.TaxaCrit;
            int danoBase = (int)(atacante.Ataque * 1.25);
            int dano = critico ? (int)(danoBase * (1 + atacante.DanoCrit)) : danoBase;
            alvo.ReceberDano(dano);
        }
    }
}
