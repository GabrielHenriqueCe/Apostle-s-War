using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    class PassivaGuarda : HabilidadePassiva
    {
        public PassivaGuarda() : base("Invencível", "⚜️", 4,
            "Ao receber ataque fatal, sobrevive com 1 HP e ganha invencibilidade por 1 turno.")
        { }

        public override bool Revive() => true;

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeReceberDano && !ctx.AlvoVivo;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            if (ctx.Atacante.TemBloqueioRessurreicao()) return SemDano();
            ctx.Atacante.Reviver(1);
            new Invencivel(turnos: 1).Aplicar(ctx.Atacante);
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) =>
            $"{p.Simbolo} {p.Nome} sobreviveu pela passiva! Invencível por 1 turno!";
    }
}