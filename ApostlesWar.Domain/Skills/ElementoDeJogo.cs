namespace ApostlesWar
{
    /// <summary>
    /// Base de IDENTIDADE dos elementos que o jogo exibe: nome, símbolo e descrição.
    /// Herdada por Habilidade e StatusEffect (antes cada uma duplicava esses 3 campos).
    /// Carrega SÓ a identidade — o cooldown (da Habilidade) e a duração (do StatusEffect)
    /// são conceitos próprios de cada uma e ficam nas subclasses.
    /// </summary>
    abstract class ElementoDeJogo
    {
        public string Nome { get; }
        public string Simbolo { get; }
        public string Descricao { get; }

        protected ElementoDeJogo(string nome, string simbolo, string descricao = "")
        {
            Nome = nome;
            Simbolo = simbolo;
            Descricao = descricao;
        }
    }
}
