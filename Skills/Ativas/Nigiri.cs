using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Revive todos os aliados mortos com 1 HP e aplica +25% ATK em todos por 2 turnos.
    /// </summary>
    class Nigiri : HabilidadeAtiva
    {
        public Nigiri() : base("Nigiri", "🍙", 4,
            "Revive todos os aliados mortos + +25% ATK em todos por 2 turnos.")
        { }
        public override int NumeroDeAlvos => int.MaxValue;

        public override void Ativar(Combate alvo, List<Combate>? aliados = null)
        {
            var todos = aliados ?? new List<Combate> { alvo };

            foreach (Combate aliado in todos)
            {
                if (!aliado.EstaVivo())
                    aliado.Reviver(1);

                new BuffAtaque(turnos: 2, percentual: 0.25).Aplicar(aliado);
            }
        }
    }
}
