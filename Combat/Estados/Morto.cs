using System.Linq;

namespace ApostlesWar
{
    /// <summary>
    /// Estado morto: carrega os status do morto (na Fatia 2, o ImpedirRessurreicao
    /// e futuros status de morto). Ignora cura; revive transiciona de volta pra Vivo.
    /// Começa com a lista limpa — os status do vivo somem na transição.
    /// </summary>
    class Morto : EstadoVida
    {
        public List<StatusEffect> StatusNoMorto { get; } = new List<StatusEffect>();

        public override List<StatusEffect> Status => StatusNoMorto;

        public override bool EstaVivo() => false;

        public override void Curar(Combate dono, int valor)
        {
            // Morto não cura.
        }

        public override void Reviver(Combate dono, int hp)
        {
            // Checagem CENTRALIZADA do bloqueio: se o morto tem a Sentença
            // (ImpedirRessurreicao), não revive. Toda tentativa de revive passa por
            // aqui, então nenhuma habilidade precisa checar — é à prova de esquecimento.
            // O Diabo (AnjoCaido) remove o debuff ANTES de chamar Reviver, daí passa.
            if (StatusNoMorto.OfType<v1_Apostle_s_War.Skills.Debuffs.ImpedirRessurreicao>().Any())
                return;

            dono.AplicarRevive(hp);
        }
    }
}