using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Buffs
{
    /// <summary>
    /// Buff: reflete uma % do dano efetivamente recebido de volta ao atacante.
    /// Reage via IReageAoReceberDano (só dispara com dano > 0): se Escudo/
    /// BloqueioTotal cobriram tudo, não reflete — proporcional ao sofrimento real.
    /// 
    /// O dano refletido usa NaturezasDano.DanoIndireto: passa por defesa/escudo/
    /// bloqueio do atacante, mas NÃO dispara reação (não reflete de volta — loop
    /// quebrado). Reflete qualquer golpe que tenha causado dano recebido.
    /// </summary>
    class RefletirDano : Buff, IReageAoReceberDano
    {
        public RefletirDano(int turnos = 2, double percentual = 0.15)
            : base("Reflexo", "🥢", turnos, percentual,
                $"Reflete {percentual * 100:F0}% do dano recebido.")
        { }

        public List<ResultadoReacao> AoReceberDano(ContextoReacao ctx)
        {
            if (!ctx.Outro.EstaVivo())
                return new List<ResultadoReacao>();

            int danoRefletido = (int)(ctx.DanoCausado * Valor);
            if (danoRefletido <= 0)
                return new List<ResultadoReacao>();

            var (real, absorvido) = ctx.Outro.ReceberDano(danoRefletido, NaturezasDano.DanoIndireto);

            var resultado = new EventoDano(
                Atacante: ctx.Portador,
                Alvo: ctx.Outro,
                DanoBruto: danoRefletido,
                DanoEfetivo: real,
                AbsorvidoPeloEscudo: absorvido,
                Critico: false,
                HPRestante: Math.Max(0, ctx.Outro.HPAtual),
                Natureza: NaturezasDano.DanoIndireto
            );

            return new List<ResultadoReacao>
    {
        new ResultadoReacao(
            Mensagem: $"{ctx.Portador.Personagem.Nome} refletiu {real} de dano! 🥢",
            Dano: resultado
        )
    };
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}