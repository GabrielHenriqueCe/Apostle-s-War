using ApostlesWar.Domain;

namespace ApostlesWar.Domain.Apostolos.Misticos
{
    /// <summary>
    /// Recebe 15% menos dano durante todo o combate. Capacidade direta
    /// (IModificaDanoRecebido) — não usa mais buff de contorno (ReducaoDanoFixo).
    /// Processa antes do Escudo/BloqueioTotal (ver Combate.ReceberDano).
    /// </summary>
    public class Aquagirl : HabilidadePassiva, IModificaDanoRecebido
    {
        private const double PercentualReducao = 0.15;

        public Aquagirl() : base("Aquagirl", "🧜‍♀️", 0,
            "Recebe 15% menos dano.")
        { }

        // Declara por contrato, mas não é o que a coloca na frente: passiva-pura roda fora do laço de
        // status, antes de todos eles (Combate.ReceberDano). Aqui o valor é a resposta honesta —
        // reduzir 15% não gasta recurso nenhum.
        public OrdemDeMitigacao OrdemDeMitigacao => OrdemDeMitigacao.ReduzDeGraca;

        public int ModificarDanoRecebido(Combate portador, int dano) =>
            (int)(dano * (1 - PercentualReducao));

        // Já era puro: prever é a mesma conta.
        public int PreverDanoRecebido(Combate portador, int dano) =>
            (int)(dano * (1 - PercentualReducao));

        public override List<EventoCombate> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}
