using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;

namespace ApostlesWar.Application.Services
{
    /// <summary>
    /// A XP e as ESTRELAS de cada apóstolo, e o nível que as duas juntas pagam. Guarda a XP crua e a
    /// estrela comprada — o nível é derivado pelo <see cref="Progressao.NivelPorXp"/> na hora de
    /// aplicar. Guardar o nível à mão foi o defeito que produziu a divergência da Velocidade (#247).
    ///
    /// <b>A estrela é a COMPRA e o nível é que deriva dela</b> (docs/GDD-progressao.md §A ESTRELA):
    /// sem nenhuma, o apóstolo trava no 9. Guardar a XP crua mesmo travado é o que faz ela ACUMULAR
    /// na parede — comprar a estrela reaplica o nível e ele salta de uma vez, sem estado novo.
    ///
    /// Quem MUDA de verdade é o roster do <see cref="PersonagemService"/>: um apóstolo é uma
    /// instância só no jogo inteiro, e é ela que sobe de nível. O inimigo da campanha por isso é
    /// CÓPIA (`Personagem.ComNivel`) — nivelar a instância compartilhada vazaria pro time do jogador.
    /// </summary>
    public class ProgressaoService
    {
        private const string ChaveProgressao = "progressao";

        private readonly PersonagemService _personagens;
        private readonly AlmaService _alma;
        private readonly IRepositorioDeSave _repo;

        private List<ApostoloProgredido> progresso = new();

        public ProgressaoService(PersonagemService personagens, AlmaService alma, IRepositorioDeSave repo)
        {
            _personagens = personagens;
            _alma = alma;
            _repo = repo;
        }

        /// <summary>
        /// Lê o save e põe cada apóstolo no nível que a XP dele paga, preso ao teto das estrelas.
        ///
        /// <b>Save sem `Estrelas` é save ANTIGO</b> (de antes da estrela virar compra) e a estrela
        /// dele se deduz do nível que a XP pagava — a identidade do <see cref="Progressao.Estrelas"/>.
        /// Sem essa dedução, todo apóstolo já nivelado despencaria pro teto 9 na primeira abertura.
        /// </summary>
        public void Carregar()
        {
            // A alma entra junto, e por aqui: ela é a moeda DESTE service (compra a estrela, queima em
            // XP), e o CarregarSaves da campanha não a conhece. O Resetar desce pelo mesmo caminho.
            _alma.Carregar();

            var lido = _repo.Carregar<List<ApostoloProgredido>>(ChaveProgressao) ?? new();

            progresso = lido
                .Select(p => p with { Estrelas = p.Estrelas ?? Progressao.Estrelas(Progressao.NivelPorXp(p.Xp)) })
                .ToList();

            foreach (ApostoloProgredido p in progresso)
                _personagens.ObterPersonagem(p.Faccao, p.Slot).AplicarNivel(NivelDe(p));
        }

        /// <summary>
        /// O pote da fase repartido entre QUEM ESTAVA EM CAMPO, igual pra todo mundo e independente de
        /// quem bateu. Sozinho leva tudo; em quatro, cada um leva um quarto — é isso que faz do solo o
        /// jeito rápido de subir UM e do time cheio o jeito de subir quatro, sem regra nova.
        ///
        /// A XP que passa do teto simplesmente se perde: quem concentra troca alcance por velocidade e
        /// paga em desperdício.
        /// </summary>
        public void Creditar(List<Personagem> time, int pote)
        {
            if (time.Count == 0 || pote <= 0) return;

            int porApostolo = pote / time.Count;
            foreach (Personagem p in time)
                Gravar(p, XpDe(p) + porApostolo, EstrelasDe(p));

            _repo.Salvar(ChaveProgressao, progresso);
        }

        /// <summary>A XP acumulada deste apóstolo. Sem registro = 0, que é o nível 1.</summary>
        public int XpDe(Personagem apostolo) => Registro(apostolo)?.Xp ?? 0;

        /// <summary>Quantas estrelas ele COMPROU. Sem registro = nenhuma, e o teto é o 9.</summary>
        public int EstrelasDe(Personagem apostolo) => Registro(apostolo)?.Estrelas ?? 0;

        /// <summary>Até onde ele pode subir hoje. Ver <see cref="Progressao.TetoPorEstrelas"/>.</summary>
        public int TetoDe(Personagem apostolo) => Progressao.TetoPorEstrelas(EstrelasDe(apostolo));

        /// <summary>
        /// Ele está na parede: a XP dele JÁ PAGA um nível acima do teto, e ainda há estrela a comprar.
        ///
        /// <b>O critério é a XP, não o nível</b>, e a diferença é o bug que isso conserta: chegar ao
        /// nível 9 com a barra pela metade não é estar travado — é estar subindo. Comprar a estrela
        /// ali não movia o nível (a XP não estava lá), então o jogador pagava o pedágio e não via
        /// nada acontecer. A parede é quando a barra ENCHE e o nível não passa.
        ///
        /// É também o único momento em que a compra é permitida: comprar adiantado quebraria a
        /// identidade `nivel / 10 == estrelas`, que é o que deixa a ficha e a
        /// <see cref="Arquetipos.Velocidade"/> lerem a estrela de fontes diferentes sem divergir.
        /// </summary>
        public bool NaParede(Personagem apostolo)
            => EstrelasDe(apostolo) < Material.EstrelaMaxima
            && Progressao.NivelPorXp(XpDe(apostolo)) > TetoDe(apostolo);

        /// <summary>
        /// O quanto falta pro próximo nível, pra a barra da ficha: (o que já entrou nesta faixa, o que
        /// a faixa inteira custa).
        ///
        /// Na parede a barra continua sendo a barra de verdade e só PARA de encher quando a XP chega —
        /// é ela que diz ao jogador que a compra está liberada. Daí o clamp em vez de um atalho pra
        /// (1,1): a XP acumulada passa do topo da faixa, e sem o clamp a barra vazaria pra fora.
        /// </summary>
        public (int Feito, int Total) FaixaDoNivel(Personagem apostolo)
        {
            int nivel = apostolo.Nivel;
            if (nivel >= Arquetipos.NivelMaximo) return (1, 1);

            int piso = Progressao.XpParaNivel(nivel);
            int total = Progressao.XpParaNivel(nivel + 1) - piso;
            return (Math.Min(XpDe(apostolo) - piso, total), total);
        }

        /// <summary>
        /// Compra a próxima estrela com alma, e com ela a dezena seguinte. Só na parede, e só com o
        /// preço inteiro no bolso (<see cref="Alma.Receita"/>).
        /// </summary>
        public MotivoRecusa ComprarEstrela(Personagem apostolo)
        {
            if (EstrelasDe(apostolo) >= Material.EstrelaMaxima) return MotivoRecusa.NoTetoFinal;
            if (!NaParede(apostolo)) return MotivoRecusa.ForaDaParede;

            var receita = Alma.Receita(EstrelasDe(apostolo) + 1);
            if (!_alma.Debitar(receita)) return MotivoRecusa.SemSaldo;

            Gravar(apostolo, XpDe(apostolo), EstrelasDe(apostolo) + 1);
            _repo.Salvar(ChaveProgressao, progresso);
            return MotivoRecusa.Nenhum;
        }

        /// <summary>
        /// Queima alma como XP (<see cref="Alma.XpPorAlma"/>).
        ///
        /// <b>Recusada na parede</b>, e isso não é conforto de UI: ali a XP não move nível nenhum, e a
        /// alma queimada é a MESMA que a estrela cobra. Deixar passar seria o jogador destruir a moeda
        /// que abre a parede em troca de nada.
        /// </summary>
        public MotivoRecusa QueimarAlma(Personagem apostolo, Raridade raridade, int quantidade)
            => QueimarAlma(apostolo, new[] { new Custo(raridade, quantidade) });

        /// <summary>
        /// A mesma queima com VÁRIAS faixas de uma vez — é como o painel do Arsenal manda, e o débito
        /// é tudo-ou-nada: metade cobrada com a outra metade recusada seria alma sumindo.
        /// </summary>
        public MotivoRecusa QueimarAlma(Personagem apostolo, IReadOnlyList<Custo> faixas)
        {
            if (faixas.Count == 0 || faixas.Any(f => f.Quantidade <= 0)) return MotivoRecusa.SemSaldo;
            if (apostolo.Nivel >= Arquetipos.NivelMaximo) return MotivoRecusa.NoTetoFinal;
            if (NaParede(apostolo)) return MotivoRecusa.NaParede;

            if (!_alma.Debitar(faixas)) return MotivoRecusa.SemSaldo;

            int ganho = faixas.Sum(f => Alma.XpPorAlma(f.Raridade) * f.Quantidade);
            Gravar(apostolo, XpDe(apostolo) + ganho, EstrelasDe(apostolo));
            _repo.Salvar(ChaveProgressao, progresso);
            return MotivoRecusa.Nenhum;
        }

        private ApostoloProgredido? Registro(Personagem apostolo)
            => progresso.FirstOrDefault(a => a.Faccao == apostolo.Faccao && (int)a.Slot == apostolo.Slot);

        /// <summary>Reescreve o registro e reaplica o nível — o único lugar que mexe nos dois.</summary>
        private void Gravar(Personagem apostolo, int xp, int estrelas)
        {
            progresso.RemoveAll(a => a.Faccao == apostolo.Faccao && (int)a.Slot == apostolo.Slot);
            var novo = new ApostoloProgredido(apostolo.Faccao, (Slot)apostolo.Slot, xp, estrelas);
            progresso.Add(novo);
            apostolo.AplicarNivel(NivelDe(novo));
        }

        private static int NivelDe(ApostoloProgredido p)
            => Progressao.NivelPorXp(p.Xp, Progressao.TetoPorEstrelas(p.Estrelas ?? 0));

        /// <summary>
        /// Volta todo mundo pro nível 1 — disco E memória, como o <see cref="CapitulosService.Resetar"/>:
        /// as instâncias do roster são compartilhadas e sobrevivem ao "excluir conta", então apagar só
        /// o arquivo deixaria o jogador novo com o elenco nivelado do anterior.
        /// </summary>
        public void Resetar()
        {
            _repo.Excluir(ChaveProgressao);
            progresso = new();
            _alma.Resetar();
            foreach (Personagem p in _personagens.Todos())
                p.AplicarNivel(Arquetipos.NivelMinimo);
        }
    }

    /// <summary>
    /// Um apóstolo no save da progressão: a identidade (facção + slot, a mesma do
    /// <see cref="ApostoloSalvo"/>), a XP e as estrelas compradas. O nível NÃO entra aqui de propósito
    /// — ele é conta, não dado.
    ///
    /// <b><c>Estrelas</c> é anulável só pra reconhecer o save ANTIGO</b>, que não tinha o campo; quem
    /// carrega preenche na hora (<see cref="ProgressaoService.Carregar"/>). Trocar por <c>int</c> faz
    /// todo save existente virar "zero estrelas", e o elenco inteiro cai pro nível 9.
    /// </summary>
    public record ApostoloProgredido(Faccao Faccao, Slot Slot, int Xp, int? Estrelas = null);

    /// <summary>Por que um aprimoramento não aconteceu. A tela usa isto pra dizer o motivo.</summary>
    public enum MotivoRecusa
    {
        Nenhum,
        /// <summary>Ainda não chegou na parede — a estrela só se compra travado.</summary>
        ForaDaParede,
        /// <summary>Está travado, e queimar alma aqui seria jogá-la fora.</summary>
        NaParede,
        /// <summary>Nível 60 ou 6 estrelas: não há mais o que comprar.</summary>
        NoTetoFinal,
        SemSaldo,
        /// <summary>A peça está vestida em alguém — o esmeril não desnuda apóstolo por conta própria.</summary>
        Vestida,
        /// <summary>A peça não está no acervo (já foi moída, ou o índice envelheceu).</summary>
        ForaDoAcervo
    }
}
