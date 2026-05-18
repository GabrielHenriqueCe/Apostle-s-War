using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao receber ataque que zeraria o HP, restaura para 1 e aplica Invencível por 1 turno.
    /// CD: 4 turnos. Não age se houver bloqueio de ressurreição.
    /// </summary>
    class PassivaGuarda : HabilidadePassiva
    {
        public PassivaGuarda() : base("Invencível", "⚜️", 4,
            "Ao receber ataque fatal, sobrevive com 1 HP e ganha invencibilidade por 1 turno.")
        { }

        public override bool Revive() => true;

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeReceberDano && !ctx.AlvoVivo;

        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            if (atacante.TemBloqueioRessurreicao()) return SemDano();
            atacante.Reviver(1);
            new Invencivel(turnos: 1).Aplicar(atacante);
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) =>
            $"{p.Simbolo} {p.Nome} sobreviveu pela passiva! Invencível por 1 turno!";
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}