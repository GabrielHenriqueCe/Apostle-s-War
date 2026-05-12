using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Cura todos os aliados vivos em 30% do HP máximo de cada um.
    /// </summary>
    class Sushi : HabilidadeAtiva
    {
        public Sushi() : base("Sushi", "🍣", 4, "Cura todos os aliados em 30% do HP máximo.") { }
        public override int NumeroDeAlvos => int.MaxValue;

        public override void Ativar(Combate alvo, List<Combate>? aliados = null)
        {
            var alvos = aliados ?? new List<Combate> { alvo };
            foreach (Combate aliado in alvos.Where(a => a.EstaVivo()))
                aliado.Curar((int)(aliado.HPMaximo * 0.30));
        }
    }
}
