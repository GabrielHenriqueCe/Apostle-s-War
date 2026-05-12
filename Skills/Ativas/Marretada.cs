using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Ativas
{
    class Marretada : HabilidadeAtiva
    {
        public Marretada() : base("Marretada", "🔨", 3, "Causa 125% do ATK em 1 inimigo.") { }
        public override int NumeroDeAlvos => 1;

        public override void Ativar(Combate alvo, List<Combate>? aliados = null) { }

        public ResultadoAtaque AtivarComAtacante(Combate atacante, Combate alvo)
        {
            return atacante.AtacarComMultiplicador(alvo, 1.25);
        }
    }
}
