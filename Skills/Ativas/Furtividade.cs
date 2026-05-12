using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;
namespace v1_Apostle_s_War.Skills.Ativas
{
    class Furtividade : HabilidadeAtiva
    {
        public Furtividade() : base("Furtividade", "🕳️", 4,
            "Intocável por 2 turnos. Ao expirar, ataca todos os inimigos com 100% ATK.")
        { }
        public override int NumeroDeAlvos => 0;
        public override TipoAlvo TipoAlvo => TipoAlvo.Explicito;
        public override TipoLista TipoLista => TipoLista.Self;
        public override List<ResultadoAtaque> Ativar(Combate atacante, Combate alvo, List<Combate> lista)
        {
            // lista = inimigos, guardada pelo FurtividadeAtaque para atacar ao expirar
            AplicarBuff(atacante, new Intocavel(turnos: 2));
            AplicarBuff(atacante, new FurtividadeAtaque(lista, turnos: 2));
            return SemDano();
        }
    }
}