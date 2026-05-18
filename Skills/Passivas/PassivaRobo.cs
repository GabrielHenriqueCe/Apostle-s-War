using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao atacar, aplica CuraContinua 1t no aliado vivo com menor HP atual (inclui o Robô).
    /// Em ContextoPassiva DepoisDeAtacar, ctx.Aliados é o time inteiro do atacante.
    /// </summary>
    class PassivaRobo : HabilidadePassiva
    {
        public PassivaRobo() : base("Reparo Automático", "🔧", 0,
            "Ao atacar, aplica Cura Contínua no aliado com menor HP.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeAtacar;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            // `lista` aqui é o time do atacante (vide ProcessarPassivasAtacante no CombateService).
            var aliadoMenorHP = lista
                .Where(a => a.EstaVivo())
                .OrderBy(a => a.HPAtual)
                .FirstOrDefault();

            if (aliadoMenorHP == null) return SemDano();

            new CuraContinua(turnos: 1, percentual: 0.10).Aplicar(aliadoMenorHP);
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}