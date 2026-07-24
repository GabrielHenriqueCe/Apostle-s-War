namespace ApostlesWar
{
    /// <summary>
    /// Seleciona QUAIS/QUANTOS status um combatente perde/ganha — eixo compartilhado pelas
    /// operações de manipulação de status (RemoverBuffs/RemoverDebuffs/MoverBuffs/Explodir/
    /// Transformar — ADR-composicao-de-acoes §5.4). Separado do NumeroDeAlvos da habilidade
    /// (que decide QUEM é afetado, não O QUÊ nele é afetado). Três partes: filtro (todos/tipo/
    /// removíveis), quantos (N ou todos) e modo (por ordem ou aleatório).
    ///
    /// 1º cliente: DocesOuTravessuras (Seletor.Removiveis(), RemoverBuffs).
    /// </summary>
    class Seletor
    {
        public Func<StatusEffect, bool> Filtro { get; }
        public int Quantos { get; }
        public bool Aleatorio { get; }

        private Seletor(Func<StatusEffect, bool> filtro, int quantos, bool aleatorio)
        {
            Filtro = filtro;
            Quantos = quantos;
            Aleatorio = aleatorio;
        }

        /// <summary>Todos os status, sem filtro de tipo.</summary>
        public static Seletor Todos(int quantos = int.MaxValue, bool aleatorio = false)
            => new(_ => true, quantos, aleatorio);

        /// <summary>Só os status removíveis (StatusEffect.Removivel).</summary>
        public static Seletor Removiveis(int quantos = int.MaxValue, bool aleatorio = false)
            => new(s => s.Removivel, quantos, aleatorio);

        /// <summary>Só status de um tipo específico (ex: Seletor.Tipo&lt;Queima&gt;() pro Explodir).</summary>
        public static Seletor Tipo<T>(int quantos = int.MaxValue, bool aleatorio = false) where T : StatusEffect
            => new(s => s is T, quantos, aleatorio);
    }
}
