using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Buffs
{
    /// <summary>
    /// Buff que reduz o dano recebido em um percentual fixo ("Couraça"). A Sereia
    /// migrou pra passiva-pura (Aquagirl implementa IModificaDanoRecebido
    /// direto); este buff fica disponível pra reuso em habilidades ativas futuras
    /// (Rebalanceamento).
    ///
    /// Se aplicado como buff comum (via StatusAtivos), processa junto dos demais
    /// IModificaDanoRecebido no ReceberDano — sem a garantia de ordem "primeiro"
    /// que tinha quando era o buff de contorno da Sereia (essa ordem agora é da
    /// passiva-pura, ver Combate.ReceberDano).
    /// </summary>
    public class ReducaoDanoFixo : Buff, IModificaDanoRecebido
    {
        public ReducaoDanoFixo(double percentual = 0.15)
            : base("Couraça", "🐚", int.MaxValue, percentual,
                $"-{percentual * 100:F0}% dano recebido.")
        { }

        public int ModificarDanoRecebido(Combate portador, int dano)
        {
            return (int)(dano * (1 - Valor));
        }

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}