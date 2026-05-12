using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao receber um ataque crítico, aplica 15% de reflexo de dano em todos os aliados por 2 turnos.
    /// O CombateService deve detectar o crítico e chamar Ativar com a lista de aliados.
    /// </summary>
    class PassivaSushiman : HabilidadePassiva
    {
        public PassivaSushiman() : base("Código do Sushi", "🥢", 0,
            "Ao receber crítico, todos os aliados ganham 15% de reflexo de dano por 2 turnos.")
        { }

        public override bool DeveAtivar(EventoCombate evento) =>
            evento == EventoCombate.DepoisDeReceberDano;

        /// <summary>
        /// alvo = o próprio Sushiman; aliados = time do jogador completo (incluindo o Sushiman).
        /// </summary>
        public override void Ativar(Combate alvo, List<Combate>? aliados = null)
        {
            var todos = aliados ?? new List<Combate> { alvo };
            foreach (Combate aliado in todos.Where(a => a.EstaVivo()))
            {
                var buff = new RefletirDano(turnos: 2, percentual: 0.15);
                buff.Aplicar(aliado);
            }
        }

        public override string MensagemSobreviveu(Personagem personagem) => string.Empty;
        public override string MensagemMorreu(Personagem personagem) => string.Empty;
    }
}
