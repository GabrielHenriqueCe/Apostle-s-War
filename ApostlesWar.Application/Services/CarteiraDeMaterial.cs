using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;

namespace ApostlesWar.Application.Services
{
    /// <summary>
    /// O BOLSO de uma moeda de material — guardar, cobrar, fundir, diluir. Quem diz QUANTO cai,
    /// QUANTO custa e QUANTO vale é o <see cref="Material"/> e a moeda; aqui só se guarda e se gasta.
    ///
    /// É a metade compartilhada entre o <see cref="AlmaService"/> e o <see cref="PoService"/>: as
    /// duas moedas são as mesmas seis faixas com a mesma fusão e a mesma diluição, e a única coisa
    /// que cada uma traz de própria é a CHAVE de save e a torneira (de onde cai). Ver
    /// <see cref="Material"/> pro porquê de a escada morar num lugar só.
    ///
    /// O saldo é um vetor indexado pela <see cref="Raridade"/> (seis posições) e não um dicionário,
    /// pelo mesmo motivo do `Item?[]` do <see cref="ArsenalService"/>: é o que atravessa o save sem
    /// depender de como o serializador escreve chave de enum.
    /// </summary>
    public abstract class CarteiraDeMaterial
    {
        private readonly IRepositorioDeSave _repo;
        private readonly string _chave;

        private int[] saldo = NovoSaldo();

        protected CarteiraDeMaterial(IRepositorioDeSave repo, string chave)
        {
            _repo = repo;
            _chave = chave;
        }

        private static int[] NovoSaldo() => new int[Enum.GetValues<Raridade>().Length];

        /// <summary>Lê o save. Ausente ou de tamanho errado = pote vazio, que é o jogo novo.</summary>
        public void Carregar()
        {
            int[]? salvo = _repo.Carregar<int[]>(_chave);
            saldo = salvo?.Length == NovoSaldo().Length ? salvo : NovoSaldo();
        }

        public int SaldoDe(Raridade raridade) => saldo[(int)raridade];

        /// <summary>O pote inteiro, na ordem da <see cref="Raridade"/>, pra a tela desenhar.</summary>
        public IReadOnlyList<int> Saldo() => saldo;

        /// <summary>
        /// Deposita <paramref name="vezes"/> aplicações de uma queda. Quem decide a queda é a moeda —
        /// a alma multiplica por inimigo morto, o pó por fase concluída.
        /// </summary>
        protected void Depositar(IReadOnlyList<Custo> queda, int vezes)
        {
            if (vezes <= 0) return;

            foreach (Custo c in queda) saldo[(int)c.Raridade] += c.Quantidade * vezes;

            _repo.Salvar(_chave, saldo);
        }

        public bool TemPara(IReadOnlyList<Custo> preco) => Faltando(preco).Count == 0;

        /// <summary>
        /// O que falta pra pagar <paramref name="preco"/>, faixa por faixa. A tela precisa da LISTA e
        /// não de um bool: "falta 72 de Raro" é o que diz ao jogador o que ir fazer.
        /// </summary>
        public IReadOnlyList<Custo> Faltando(IReadOnlyList<Custo> preco)
            => preco
                .Where(c => SaldoDe(c.Raridade) < c.Quantidade)
                .Select(c => new Custo(c.Raridade, c.Quantidade - SaldoDe(c.Raridade)))
                .ToList();

        /// <summary>Cobra o preço inteiro, ou nada. Sem saldo devolve false e não toca no pote.</summary>
        public bool Debitar(IReadOnlyList<Custo> preco)
        {
            if (!TemPara(preco)) return false;

            foreach (Custo c in preco) saldo[(int)c.Raridade] -= c.Quantidade;

            _repo.Salvar(_chave, saldo);
            return true;
        }

        /// <summary>
        /// Desce uma faixa (ver <see cref="Material.Diluir"/>). É o único caminho pra quem precisa de
        /// faixa BAIXA numa dificuldade alta: o Pesadelo não derruba Comum e a 1ª estrela é Comum puro.
        /// </summary>
        public bool Diluir(Raridade raridade, int quantidade)
        {
            Custo? virou = Material.Diluir(raridade, quantidade);
            if (virou == null || SaldoDe(raridade) < quantidade) return false;

            saldo[(int)raridade] -= quantidade;
            saldo[(int)virou.Raridade] += virou.Quantidade;

            _repo.Salvar(_chave, saldo);
            return true;
        }

        /// <summary>
        /// Sobe uma faixa (ver <see cref="Material.Fundir"/>), travada pelo
        /// <see cref="Material.TetoDeFusao"/> da dificuldade mais alta já aberta — quem passa a
        /// dificuldade é o <c>CapitulosService</c>, e ela chega aqui como fato, não como consulta.
        /// </summary>
        public bool Fundir(Raridade raridade, int quantidade, Dificuldade maisAlta)
        {
            Custo? virou = Material.Fundir(raridade, quantidade);
            if (virou == null || virou.Raridade > Material.TetoDeFusao(maisAlta)) return false;

            // Só o que fecha grupo é cobrado: pedir 25 funde 20 e devolve 5 no bolso.
            int gasto = virou.Quantidade * Material.PorFusao;
            if (SaldoDe(raridade) < gasto) return false;

            saldo[(int)raridade] -= gasto;
            saldo[(int)virou.Raridade] += virou.Quantidade;

            _repo.Salvar(_chave, saldo);
            return true;
        }

        /// <summary>Zera disco E memória, como os outros services fazem no "excluir conta".</summary>
        public void Resetar()
        {
            _repo.Excluir(_chave);
            saldo = NovoSaldo();
        }
    }
}
