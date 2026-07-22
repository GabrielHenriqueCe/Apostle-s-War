using ApostlesWar;

namespace ApostlesWar.Skills.Buffs
{
    /// <summary>
    /// O portador recebe o dano CHEIO (o DanoEfetivo é integral — lifesteal, reflexo etc. enxergam o
    /// valor real), mas o HP nunca cai abaixo de 1. O piso é aplicado pelo ReceberDano via IDefineHPMinimo,
    /// separado da mitigação de dano — por isso não zera o dano (o bug antigo: era IModificaDanoRecebido
    /// e capava o dano em HPAtual-1, o que zerava o DanoEfetivo com o portador em 1 HP).
    /// </summary>
    class Invencivel : Buff, IDefineHPMinimo
    {
        public Invencivel(int duracao = 1) : base("Invencível", "⚜️", duracao, 0,
            "Não pode morrer. HP mínimo de 1.")
        { }

        public int HPMinimo() => 1;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
