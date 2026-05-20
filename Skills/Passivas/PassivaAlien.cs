using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// Ao receber dano, aplica Escudo de 5% do HP máximo por 1 turno em si mesmo.
    /// </summary>
    class PassivaAlien : HabilidadePassiva
    {
        public PassivaAlien() : base("Carapaça Alienígena", "👽", 0,
            "Ao receber dano, ganha Escudo de 5% HP por 1 turno.")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.DepoisDeReceberDano && ctx.AlvoVivo;

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            int pontos = (int)(ctx.Atacante.HPMaximo * 0.05);
            new Escudo(pontos, turnos: 1).Aplicar(ctx.Atacante);
            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}