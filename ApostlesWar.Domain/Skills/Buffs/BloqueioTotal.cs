using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Skills.Buffs
{
    /// <summary>
    /// Bloqueia 100% do dano recebido enquanto ativo.
    /// </summary>
    public class BloqueioTotal : Buff, IModificaDanoRecebido
    {
        public BloqueioTotal(int duracao = 1) : base("Bloqueio Total", "🧱", duracao, 1,
            "Bloqueia todo o dano recebido.")
        { }

        // Zerar o dano não custa nada — então roda antes de quem paga pra reduzir, senão o Escudo
        // gasta pontos aparando um golpe que este bloqueio ia anular de graça.
        public OrdemDeMitigacao OrdemDeMitigacao => OrdemDeMitigacao.ReduzDeGraca;

        public int ModificarDanoRecebido(Combate portador, int dano) => 0;

        public int PreverDanoRecebido(Combate portador, int dano) => 0;

        public override void Remover(Combate alvo)
        {
            alvo.StatusAtivos.Remove(this);
        }
    }
}
