using ApostlesWar;

namespace v1_Apostle_s_War.Skills.Passivas
{
    class Necromancia : HabilidadePassiva
    {
        public Necromancia() : base("Necromancia", "🪦", 6,
            "Revive o personagem com 50% do HP máximo ao morrer.")
        { }

        public override bool Revive() => true;

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeReceberDano && !ctx.AlvoVivo;

        public override string MensagemSobreviveu(Personagem p) =>
            $"{p.Simbolo} {p.Nome} foi ressuscitado pela Necromancia!";
        public override string MensagemMorreu(Personagem p) =>
            $"{p.Simbolo} {p.Nome} caiu em batalha e não pode ser ressuscitado.";

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            if (!ctx.Atacante.PodeReviver) return SemDano();
            if (ctx.Atacante.HPAtual <= 0)
                ctx.Atacante.Reviver(ctx.Atacante.HPMaximo / 2);
            return SemDano();
        }
    }
}