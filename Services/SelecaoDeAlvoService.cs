using ApostlesWar;
using v1_Apostle_s_War.Skills.Buffs;

namespace v1_Apostle_s_War.Services
{
    /// <summary>
    /// Regra de domínio de targeting: dada uma lista de candidatos, decide quais
    /// são alvos válidos, conforme os status de provocação/bloqueio/intocável.
    /// Domínio puro — sem UI, sem input. Persiste para a versão web (a API usará
    /// a mesma regra). Injetável para permitir estratégias de alvo variáveis no
    /// futuro (ex: bot inteligente do AW v2).
    /// </summary>
    internal class SelecaoDeAlvoService
    {
        /// <summary>
        /// Filtra os candidatos vivos pela regra de prioridade de alvo:
        /// 1. Se há alguém com Provocar, só esses são alváveis.
        /// 2. Senão, quem não tem BloqueioTotal nem Intocável.
        /// 3. Senão, quem não tem BloqueioTotal.
        /// 4. Senão, todos os vivos (último recurso).
        /// </summary>
        public List<Combate> ResolverAlvosDisponiveis(List<Combate> candidatos)
        {
            var vivos = candidatos.Where(c => c.EstaVivo()).ToList();

            var comProvocar = vivos.Where(c => c.StatusAtivos.Any(s => s is Provocar)).ToList();
            if (comProvocar.Count > 0) return comProvocar;

            var semTudo = vivos.Where(c =>
                !c.StatusAtivos.Any(s => s is BloqueioTotal) &&
                !c.StatusAtivos.Any(s => s is Intocavel)).ToList();
            if (semTudo.Count > 0) return semTudo;

            var semBloqueio = vivos.Where(c => !c.StatusAtivos.Any(s => s is BloqueioTotal)).ToList();
            if (semBloqueio.Count > 0) return semBloqueio;

            return vivos;
        }

        /// <summary>
        /// Escolha do bot: alvo aleatório entre os disponíveis. Ponto de evolução
        /// para IA mais inteligente no futuro (AW v2) — focar mais fraco, mais
        /// perigoso, etc. Por ora, aleatório uniforme. Confia que `disponiveis` já
        /// veio filtrado (por ResolverAlvosDisponiveis) — não refiltra por EstaVivo,
        /// senão quebra quando o estado alvo é Mortos (ex: bot revivendo aliado).
        /// </summary>
        public Combate EscolherAlvoBot(List<Combate> disponiveis) =>
            disponiveis[Random.Shared.Next(disponiveis.Count)];

        /// <summary>
        /// Overload pra alvo ALIADO: filtra só pelo EstadoAlvo declarado pela
        /// habilidade (Vivos/Mortos), SEM as regras de Provocar/BloqueioTotal/
        /// Intocável — essas são mecânica de combate contra o time adversário,
        /// não fazem sentido escolhendo quem curar/reviver no próprio time.
        /// </summary>
        public List<Combate> ResolverAlvosDisponiveis(List<Combate> candidatos, EstadoAlvo estado) =>
            estado switch
            {
                EstadoAlvo.Mortos => candidatos.Where(c => !c.EstaVivo()).ToList(),
                _ => candidatos.Where(c => c.EstaVivo()).ToList(),
            };
    }
}