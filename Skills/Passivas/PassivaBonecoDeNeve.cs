using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
using v1_Apostle_s_War.Skills.Debuffs;

namespace v1_Apostle_s_War.Skills.Passivas
{
    /// <summary>
    /// No início do turno, se o Boneco de Neve tiver Queima, remove e aplica CuraContinua 2t.
    /// Tematicamente: "o calor da queima vira o gelo derretendo e curando".
    /// </summary>
    class PassivaBonecoDeNeve : HabilidadePassiva
    {
        private const double CuraPercentual = 0.10;
        private const int TurnosCura = 2;

        public PassivaBonecoDeNeve() : base("Derretendo", "❄️", 0,
            "Início do turno: se tem Queima, remove e aplica Cura Contínua (2t).")
        { }

        public override bool DeveAtivar(EventoCombate evento, ContextoPassiva ctx) =>
            evento == EventoCombate.InicioDoTurno && ctx.AlvoVivo
            && ctx.Atacante.StatusAtivos.OfType<Queima>().Any();

        public override List<ResultadoAtaque> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var queimas = ctx.Atacante.StatusAtivos.OfType<Queima>().ToList();
            foreach (var q in queimas)
                q.Remover(ctx.Atacante);

            new CuraContinua(turnos: TurnosCura, percentual: CuraPercentual).Aplicar(ctx.Atacante);

            return SemDano();
        }

        public override string MensagemSobreviveu(Personagem p) => string.Empty;
        public override string MensagemMorreu(Personagem p) => string.Empty;
    }
}