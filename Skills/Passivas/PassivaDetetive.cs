using ApostlesWar;
using v1_Apostle_s_War.Skills;

class PassivaDetetive : HabilidadePassiva
{
    private double? _danoCritBase = null;

    public PassivaDetetive() : base("Olho Clínico", "🚬", 0,
        "+10% dano crítico por debuff ativo no time inimigo. Máx: +100%.")
    { }

    public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
        evento == EventoCombate.DepoisDeAtacar;

    public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
    {
        // Captura o valor base na primeira ativação (após itens)
        _danoCritBase ??= atacante.DanoCrit;

        int totalDebuffs = lista
            .Where(i => i.EstaVivo())
            .Sum(i => i.StatusAtivos.Count(s => s is Debuff));

        double bonus = Math.Min(totalDebuffs * 0.10, 1.00);
        atacante.DefinirDanoCrit(_danoCritBase.Value + bonus);
        return SemDano();
    }

    public override string MensagemSobreviveu(Personagem p) => string.Empty;
    public override string MensagemMorreu(Personagem p) => string.Empty;
}