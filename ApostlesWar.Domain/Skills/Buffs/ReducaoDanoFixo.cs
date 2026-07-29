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
    /// IModificaDanoRecebido no ReceberDano — antes de quem gasta recurso (é
    /// ReduzDeGraca), mas depois da passiva-pura, que roda fora do laço de status
    /// (ver Combate.ReceberDano).
    /// </summary>
    public class ReducaoDanoFixo : Buff, IModificaDanoRecebido
    {
        public ReducaoDanoFixo(double percentual = 0.15)
            : base("Couraça", "🐚", Permanente, percentual,
                $"-{percentual * 100:F0}% dano recebido.")
        { }

        // Percentual não custa nada pra quem reduz: entra antes do escudo gastar pontos.
        public OrdemDeMitigacao OrdemDeMitigacao => OrdemDeMitigacao.ReduzDeGraca;

        public int ModificarDanoRecebido(Combate portador, int dano)
        {
            return (int)(dano * (1 - Valor));
        }

        // Já era puro: prever é a mesma conta.
        public int PreverDanoRecebido(Combate portador, int dano) => (int)(dano * (1 - Valor));

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}