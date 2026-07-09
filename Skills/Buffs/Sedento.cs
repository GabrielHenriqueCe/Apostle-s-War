using ApostlesWar;

namespace ApostlesWar.Skills.Buffs
{
    /// <summary>
    /// Buff: cada vez que o portador causa dano, recupera um percentual desse
    /// dano como HP. Reage via IReageAoCausarDano — orquestrado pelo
    /// CombateService (declara a cura; o serviço exibe). O Morcego migrou pra
    /// passiva-pura (SedentoDeSangue implementa IReageAoCausarDano direto); este
    /// buff fica disponível pra reuso em habilidades ativas futuras (Rebalanceamento).
    /// </summary>
    class Sedento : Buff, IReageAoCausarDano
    {
        public Sedento(int turnos = int.MaxValue, double percentual = 0.15)
            : base("Sedento", "🩸", turnos, percentual,
                $"Cura {percentual * 100:F0}% do dano causado.")
        { }

        public List<ResultadoReacao> AoCausarDano(ContextoReacao ctx)
        {
            if (ctx.DanoCausado <= 0)
                return new List<ResultadoReacao>();

            int cura = (int)(ctx.DanoCausado * Valor);
            ctx.Portador.Curar(cura);

            return new List<ResultadoReacao>
            {
                new ResultadoReacao(
                    Mensagem: $"{ctx.Portador.Personagem.Nome} se cura em {cura} com Sedento! 🩸",
                    Cura: cura
                )
            };
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}