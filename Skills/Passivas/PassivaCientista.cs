using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Sempre que recebe dano, reduz a duração de TODOS os buffs do atacante em 1 turno.
    /// </summary>
    class PassivaCientista : HabilidadePassiva
    {
        public PassivaCientista() : base("Análise Crítica", "🔬", 0,
            "Ao ser atacado, reduz em 1t a duração dos benefícios do atacante.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeSerAtacado && ctx.AlvoVivo;

        // ctx.Atacante = Cientista (portador); alvo = quem atacou o Cientista
        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            foreach (var buff in alvo.StatusAtivos.OfType<Buff>().ToList())
            {
                buff.ReduzirDuracao(1);
                if (buff.Expirou)
                    buff.Remover(alvo);
            }
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}