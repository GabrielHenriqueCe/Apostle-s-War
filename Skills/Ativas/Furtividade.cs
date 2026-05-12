using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Ativas
{
    /// <summary>
    /// Torna o Detetive intocável por 2 turnos. Ao expirar, ataca todos os inimigos com 100% ATK.
    /// </summary>
    class Furtividade : HabilidadeAtiva
    {
        public Furtividade() : base("Furtividade", "🕳️", 4,
            "Intocável por 2 turnos. Ao expirar, ataca todos os inimigos com 100% ATK.")
        { }
        public override int NumeroDeAlvos => 0; // self-buff, não seleciona alvo

        public override void Ativar(Combate alvo, List<Combate>? aliados = null)
        {
            // alvo = o próprio Detetive; aliados = lista de inimigos para o ataque posterior
            var inimigos = aliados ?? new List<Combate>();
            alvo.StatusAtivos.Add(new Intocavel(turnos: 2));
            alvo.StatusAtivos.Add(new FurtividadeAtaque(inimigos, turnos: 2));
        }
    }
}
