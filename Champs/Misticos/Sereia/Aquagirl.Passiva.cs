using ApostlesWar;

namespace ApostlesWar.Champs.Misticos
{
    /// <summary>
    /// Recebe 15% menos dano durante todo o combate. Capacidade direta
    /// (IModificaDanoRecebido) — não usa mais buff de contorno (ReducaoDanoFixo).
    /// Processa antes do Escudo/BloqueioTotal (ver Combate.ReceberDano).
    /// </summary>
    class Aquagirl : HabilidadePassiva, IModificaDanoRecebido
    {
        private const double PercentualReducao = 0.15;

        public Aquagirl() : base("Aquagirl", "🧜‍♀️", 0,
            "Recebe 15% menos dano.")
        { }

        public int ModificarDanoRecebido(Combate portador, int dano) =>
            (int)(dano * (1 - PercentualReducao));

        public override List<EventoDano> Ativar(ContextoCombate ctx, Combate alvo) => SemDano();
    }
}
