namespace ApostlesWar.Domain
{
    #region Item

    /// <summary>
    /// Uma PEÇA de equipamento — e a palavra é instância, não catálogo.
    ///
    /// <b>Até ago/2026 o item era uma entrada de tabela:</b> identidade `(Facção, Fase)`, valor
    /// derivado do capítulo, uma peça possível por par e nada de duplicata. Agora cada peça que cai
    /// é um objeto próprio com <see cref="Id"/>, e duas Manoplas do Reino são coisas diferentes —
    /// níveis diferentes, principais diferentes. É o que o drop de 4 por fase e a forja exigem.
    ///
    /// <b>DOIS EIXOS, e eles são independentes</b> (docs/GDD-itens.md §Os dois eixos): o NÍVEL diz
    /// QUANTO cada número vale e sobe jogando; a raridade diz QUANTAS subestatísticas e vem da forja.
    /// A raridade e as subs ainda não existem — este arquivo é só o eixo do nível.
    ///
    /// <b>A facção aqui é o CONJUNTO, não a magnitude.</b> Uma arma do Reino e uma dos Ascendentes,
    /// no mesmo nível, dão exatamente o mesmo ATK; o que a facção vai decidir é o bônus de conjunto
    /// (3/6/9 peças). Antes ela multiplicava o valor pelo número do capítulo, e era isso que fazia o
    /// item do capítulo 8 valer oito vezes o do 1.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// A identidade da PEÇA. Sem ela duas cópias do mesmo slot seriam indistinguíveis no save e
        /// no inventário — e é justamente a cópia que a forja consome e o drop multiplica.
        /// </summary>
        public Guid Id { get; init; } = Guid.NewGuid();

        public string Nome { get; init; }
        public string Simbolo { get; init; }

        /// <summary>O CONJUNTO a que a peça pertence. Não mexe em número nenhum — ver o resumo.</summary>
        public Faccao Faccao { get; init; }

        /// <summary>O SLOT, que é a fase de onde a peça cai. Cada fase dropa só o slot dela.</summary>
        public Fases Fase { get; init; }

        /// <summary>
        /// O principal. Fixo nos três slots cheios, SORTEADO no drop nos quatro de percentual —
        /// ver <see cref="Equipamento.OpcoesDoSlot"/>.
        /// </summary>
        public TipoStat TipoStat { get; init; }

        /// <summary>
        /// Os pontos de uso acumulados, a moeda do nível. É o análogo exato da XP do apóstolo, e pelo
        /// mesmo motivo é o que se GRAVA: o nível é conta (<see cref="Nivel"/>), não dado. Guardar o
        /// nível à mão foi o defeito que produziu a divergência da Velocidade no #247.
        /// </summary>
        public int Pontos { get; set; }

        /// <summary>Quantas estrelas a peça já teve o pedágio pago. Cada uma abre a dezena seguinte.</summary>
        public int Estrelas { get; set; }

        /// <summary>
        /// O SEGUNDO eixo: quantas subestatísticas a peça tem. <b>Hoje toda peça é Comum</b>, e isso
        /// não é lacuna disfarçada — é o estado de verdade do jogo: as subs não existem, então não há
        /// faixa a sortear no drop. O campo já existe porque o ⚙️ Esmeril paga POR FAIXA
        /// (<see cref="Po.Esmerilhar"/>), e a regra dele é a mesma no dia em que a raridade chegar.
        ///
        /// Quem passa a mexer aqui é o passo 10-b2 (GDD-progressao §7): o drop sorteia a faixa dentro
        /// do teto da dificuldade, e a ⚗️ Amálgama sobe um degrau consumindo peças iguais.
        /// </summary>
        public Raridade Raridade { get; set; } = Raridade.Comum;

        /// <summary>
        /// O nível de hoje: o que os <see cref="Pontos"/> pagam, preso ao teto que as
        /// <see cref="Estrelas"/> abriram. Os pontos continuam ACUMULANDO na parede — é o que faz a
        /// estrela comprada mover o nível de uma vez, sem estado novo.
        /// </summary>
        public int Nivel => Po.NivelPorPontos(Pontos, Progressao.TetoPorEstrelas(Estrelas));

        /// <summary>
        /// Quanto a peça dá do <see cref="TipoStat"/> dela HOJE: o teto do slot vezes o que o nível
        /// liberou. É a única fonte do número — nada mais multiplica item.
        /// </summary>
        public double Valor => Equipamento.Maximo(TipoStat) * Equipamento.FatorNivel(Nivel);

        public Item(string nome, string simbolo, Faccao faccao, Fases fase, TipoStat tipoStat)
        {
            Nome = nome;
            Simbolo = simbolo;
            Faccao = faccao;
            Fase = fase;
            TipoStat = tipoStat;
        }

        public Item()
        {
            Nome = null!;
            Simbolo = null!;
        }

        // COMO o valor é ESCRITO na tela ("5%" vs "0,05", o rótulo "Dano Crit") não mora aqui: o Item
        // guarda o número e o tipo, e cada pele decide a apresentação. Formatar aqui dentro põe `:F0`
        // e sufixo `%` no domínio de regras.
    }

    #endregion
}
