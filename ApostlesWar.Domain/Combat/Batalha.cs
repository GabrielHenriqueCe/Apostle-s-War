namespace ApostlesWar.Domain
{
    /// <summary>
    /// Uma EQUIPE: quem joga junto (aliados entre si). Só a estrutura — quem CONTROLA a equipe
    /// (humano/bot) é decisão da orquestração (CombateService mapeia Equipe→IControladorDeTurno),
    /// não mora aqui, pra manter o domínio livre da camada de controle.
    /// </summary>
    public class Equipe
    {
        public List<Combate> Membros { get; }

        public Equipe(List<Combate> membros) => Membros = membros;

        public bool TemVivos() => Membros.Any(m => m.EstaVivo());
    }

    /// <summary>
    /// A estrutura de uma BATALHA: duas equipes e a perspectiva de cada combatente. É o dono da
    /// pergunta "quem é meu aliado / meu inimigo?", derivada da ESTRUTURA (qual equipe o combatente
    /// integra) — não do TIPO (Jogador/Inimigo). Isso desacopla time × controle × classe: no modo
    /// campanha a Equipe1 é o jogador e a Equipe2 os inimigos; no Versus qualquer equipe pode ser
    /// humana ou bot, e este código não muda.
    ///
    /// Fronteira de conceitos: a Batalha é dona da ESTRUTURA (times/perspectiva); o TurnoDoPersonagem
    /// é dono do TEMPO per-combatente (estado turn-scoped); o RelogioDoCombate do tempo GLOBAL.
    /// Mora no nível do CombateService (rebuild por rodada, como o RelogioDoCombate) — não no domínio
    /// do combatente, então não há referência velha entre rodadas.
    /// </summary>
    public class Batalha
    {
        public Equipe Equipe1 { get; }
        public Equipe Equipe2 { get; }

        public Batalha(Equipe equipe1, Equipe equipe2)
        {
            Equipe1 = equipe1;
            Equipe2 = equipe2;
        }

        /// <summary>A equipe que o combatente integra.</summary>
        public Equipe EquipeDe(Combate c) => Equipe1.Membros.Contains(c) ? Equipe1 : Equipe2;

        /// <summary>A equipe adversária de uma equipe.</summary>
        public Equipe OponenteDe(Equipe e) => e == Equipe1 ? Equipe2 : Equipe1;

        /// <summary>
        /// A perspectiva do portador: (seus aliados, seus inimigos). O "um só caminho" — toda
        /// perspectiva do combate nasce aqui, em vez de cada ponto inverter times na mão.
        /// </summary>
        public (List<Combate> Aliados, List<Combate> Inimigos) PerspectivaDe(Combate portador)
        {
            var minha = EquipeDe(portador);
            return (minha.Membros, OponenteDe(minha).Membros);
        }

        /// <summary>Todos os combatentes na ordem de turno (Equipe1 depois Equipe2).</summary>
        public List<Combate> Combatentes => Equipe1.Membros.Concat(Equipe2.Membros).ToList();
    }
}
