using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;

namespace ApostlesWar.Application.Services
{
    /// <summary>
    /// O saldo de alma do jogador — um pote só, compartilhado por todos os apóstolos. Quem diz
    /// QUANTO cai, QUANTO custa e QUANTO vale é o <see cref="Alma"/>; aqui só se guarda e se gasta.
    ///
    /// O saldo é um vetor indexado pela <see cref="Raridade"/> (seis posições) e não um dicionário,
    /// pelo mesmo motivo do `Item?[]` do <see cref="ArsenalService"/>: é o que atravessa o save sem
    /// depender de como o serializador escreve chave de enum.
    /// </summary>
    public class AlmaService
    {
        // O slot de save deste service. Const porque salvar, carregar e o wipe do Resetar o citam.
        private const string ChaveAlma = "alma";

        private readonly IRepositorioDeSave _repo;

        private int[] saldo = NovoSaldo();

        public AlmaService(IRepositorioDeSave repo) => _repo = repo;

        private static int[] NovoSaldo() => new int[Enum.GetValues<Raridade>().Length];

        /// <summary>Lê o save. Ausente ou de tamanho errado = pote vazio, que é o jogo novo.</summary>
        public void Carregar()
        {
            int[]? salvo = _repo.Carregar<int[]>(ChaveAlma);
            saldo = salvo?.Length == NovoSaldo().Length ? salvo : NovoSaldo();
        }

        public int SaldoDe(Raridade raridade) => saldo[(int)raridade];

        /// <summary>O pote inteiro, na ordem da <see cref="Raridade"/>, pra a tela desenhar.</summary>
        public IReadOnlyList<int> Saldo() => saldo;

        /// <summary>
        /// A alma dos inimigos derrubados nesta fase. Cai por INIMIGO, não por vitória — quem matou
        /// dois e perdeu leva os dois, igual à XP.
        /// </summary>
        public void Creditar(Dificuldade dificuldade, int mortos)
        {
            if (mortos <= 0) return;

            foreach (Custo c in Alma.QuedaPorInimigo(dificuldade))
                saldo[(int)c.Raridade] += c.Quantidade * mortos;

            _repo.Salvar(ChaveAlma, saldo);
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

            _repo.Salvar(ChaveAlma, saldo);
            return true;
        }

        /// <summary>
        /// Desce alma uma faixa (ver <see cref="Alma.Diluir"/>). É o único caminho pra quem precisa de
        /// faixa BAIXA numa dificuldade alta: o Pesadelo não derruba Comum e a 1ª estrela é Comum puro.
        /// </summary>
        public bool Diluir(Raridade raridade, int quantidade)
        {
            Custo? virou = Alma.Diluir(raridade, quantidade);
            if (virou == null || SaldoDe(raridade) < quantidade) return false;

            saldo[(int)raridade] -= quantidade;
            saldo[(int)virou.Raridade] += virou.Quantidade;

            _repo.Salvar(ChaveAlma, saldo);
            return true;
        }

        /// <summary>
        /// Sobe alma uma faixa (ver <see cref="Alma.Fundir"/>), travada pelo
        /// <see cref="Alma.TetoDeFusao"/> da dificuldade mais alta já aberta — quem passa a
        /// dificuldade é o <c>CapitulosService</c>, e ela chega aqui como fato, não como consulta.
        /// </summary>
        public bool Fundir(Raridade raridade, int quantidade, Dificuldade maisAlta)
        {
            Custo? virou = Alma.Fundir(raridade, quantidade);
            if (virou == null || virou.Raridade > Alma.TetoDeFusao(maisAlta)) return false;

            // Só o que fecha grupo é cobrado: pedir 25 funde 20 e devolve 5 no bolso.
            int gasto = virou.Quantidade * Alma.PorFusao;
            if (SaldoDe(raridade) < gasto) return false;

            saldo[(int)raridade] -= gasto;
            saldo[(int)virou.Raridade] += virou.Quantidade;

            _repo.Salvar(ChaveAlma, saldo);
            return true;
        }

        /// <summary>Zera disco E memória, como os outros services fazem no "excluir conta".</summary>
        public void Resetar()
        {
            _repo.Excluir(ChaveAlma);
            saldo = NovoSaldo();
        }
    }
}
