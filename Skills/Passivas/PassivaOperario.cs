using ApostlesWar;
using v1_Apostle_s_War.Skills.Ativas;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao receber dano, tem 10% de chance de contra-atacar com Marretada.
    /// Não consome nem afeta o cooldown da Marretada.
    /// </summary>
    class PassivaOperario : HabilidadePassiva
    {
        private static readonly Random random = new Random();
        private readonly Marretada _marretada = new();

        public PassivaOperario() : base("Instinto do Operário", "🛠️", 0,
            "10% de chance de contra-atacar com Marretada ao receber dano.")
        { }

        public override bool DeveAtivar(EventoCombate evento) =>
            evento == EventoCombate.DepoisDeReceberDano;

        // Revive() herdado já retorna false — sem override necessário

        public override void Ativar(Combate alvo, List<Combate>? aliados = null)
        {
            // alvo aqui é o próprio Operário (quem recebeu dano)
            // aliados[0] será o atacante inimigo — convenção a seguir no CombateService
            if (aliados == null || aliados.Count == 0) return;
            if (random.NextDouble() >= 0.10) return;

            Combate atacanteOriginal = aliados[0];
            _marretada.AtivarComAtacante(alvo, atacanteOriginal);
        }

        public override string MensagemSobreviveu(Personagem personagem) =>
            $"{personagem.Simbolo} {personagem.Nome} contra-atacou com Marretada!";

        public override string MensagemMorreu(Personagem personagem) => string.Empty;
    }
}
