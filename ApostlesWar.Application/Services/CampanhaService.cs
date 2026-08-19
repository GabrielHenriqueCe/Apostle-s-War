using System.Collections.Generic;
using System.Linq;
using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;

namespace ApostlesWar.Application.Services
{
    /// <summary>
    /// O META da campanha, FORA da luta: carregar os saves e processar a recompensa da vitória
    /// (desbloqueios + drop + save). Não desenha nada — devolve o resultado, e a casca (o FluxoDoFront)
    /// mostra do seu jeito: a lógica meta mora na Application, nunca no front.
    /// </summary>
    public class CampanhaService
    {
        // ONDE O JOGADOR PAROU — capítulo, fase e time. Mora aqui e não no front porque "onde eu
        // estava" é PROGRESSÃO, não estado de tela: some junto com a conta (o ResetarProgresso apaga
        // esta chave) e sobrevive a fechar o jogo. O front NÃO grava direto na porta: dois donos
        // pra esta chave é um que escreve e outro que apaga.
        private const string ChaveOndeParou = "campanha";

        private readonly ArsenalService _arsenal;
        private readonly ApostolosService _apostolos;
        private readonly CapitulosService _capitulos;
        private readonly PersonagemService _personagens;
        private readonly ProgressaoService _progressao;
        private readonly IRepositorioDeSave _repo;

        public CampanhaService(ArsenalService arsenal, ApostolosService apostolos,
            CapitulosService capitulos, PersonagemService personagens, ProgressaoService progressao,
            IRepositorioDeSave repo)
        {
            _arsenal = arsenal;
            _apostolos = apostolos;
            _capitulos = capitulos;
            _personagens = personagens;
            _progressao = progressao;
            _repo = repo;
        }

        /// <summary>
        /// O marcador do save. Chave ANTIGA guardava só o int da posição; a porta devolve default
        /// silencioso quando não consegue ler (ver SaveLocal), então um save velho custa só o
        /// marcador do mapa voltar pro Reino — apóstolos, itens e fases vivem em outras chaves.
        /// </summary>
        private OndeParou Carregar() => _repo.Carregar<OndeParou>(ChaveOndeParou) ?? new OndeParou();

        private void Salvar(OndeParou onde) => _repo.Salvar(ChaveOndeParou, onde);

        /// <summary>Em que capítulo o marcador do mapa está. Sem save = 0 = o primeiro.</summary>
        public int PosicaoNoMapa() => Carregar().Posicao;

        /// <summary>Guarda o "último lugar" — o marcador reabre onde o jogador deixou.</summary>
        public void SalvarPosicao(int indice) => Salvar(Carregar() with { Posicao = indice });

        /// <summary>
        /// A dificuldade em que o jogador estava. Cai no Fácil se a lembrada não estiver mais aberta —
        /// só acontece depois de um wipe, e é o mesmo tratamento que a fase lembrada recebe.
        /// </summary>
        public Dificuldade DificuldadeAtual()
        {
            Dificuldade salva = Carregar().Dificuldade;
            return _capitulos.DificuldadeDesbloqueada(salva) ? salva : Dificuldade.Facil;
        }

        public void SalvarDificuldade(Dificuldade dificuldade)
            => Salvar(Carregar() with { Dificuldade = dificuldade });

        /// <summary>
        /// A fase que abre selecionada neste capítulo: a última em que o jogador ENTROU, ou a 1 se ele
        /// nunca entrou em nenhuma. "Entrou", não "venceu" — quem apanhou numa fase quer voltar
        /// naquela fase, não na seguinte.
        ///
        /// Por capítulo, e não uma só: o jogador vai e volta entre capítulos, e cada um tem a própria
        /// história. Se a fase lembrada tiver ficado travada (só acontece depois de um wipe), cai na 1.
        /// </summary>
        public Fases UltimaFaseDe(Faccao faccao, Dificuldade dificuldade)
        {
            if (Carregar().UltimaFase.TryGetValue(dificuldade, out var doNivel)
                && doNivel.TryGetValue(faccao, out int numero)
                && Enum.IsDefined(typeof(Fases), numero)
                && _capitulos.EstaDesbloqueado(faccao, (Fases)numero, dificuldade))
                return (Fases)numero;

            return Fases.Fase1;
        }

        /// <summary>
        /// O último time montado, pra a tela de fases já abrir com ele nos slots — em vez de o jogador
        /// remontar os quatro toda vez.
        ///
        /// Guardado por IDENTIDADE (facção + slot), não por posição na lista de desbloqueados. Hoje o
        /// índice funcionaria por acidente — a lista só CRESCE, e sempre pelo fim, então quem já está
        /// nela não muda de lugar. Mas isso é uma propriedade do `DesbloquearApostolos`, não uma
        /// promessa: basta o roster ser reordenado, ou um apóstolo sair, pra um índice salvo passar a
        /// apontar pra outra pessoa — e o sintoma seria o jogador entrar na fase com o time errado,
        /// sem nada dizendo por quê. Identidade não tem esse modo de falhar.
        ///
        /// Filtra quem não está liberado: um wipe no meio do caminho deixaria referência pra ninguém.
        /// </summary>
        public List<Personagem> UltimoTime() => Carregar().UltimoTime
            .Select(c => _personagens.ObterPersonagem(c.Faccao, c.Slot))
            .Where(_apostolos.EstaDesbloqueado)
            .ToList();

        /// <summary>
        /// Registra que o jogador entrou NESTA fase com ESTE time. Um método só porque é um gesto só —
        /// "entrei pra lutar" —, e separar em dois abriria a porta pra salvar metade.
        /// </summary>
        public void SalvarEntradaNaFase(Faccao faccao, Fases fase, Dificuldade dificuldade, List<Personagem> time)
        {
            OndeParou onde = Carregar();

            // A memória é POR DIFICULDADE: quem volta pro Fácil pra farmar tem de cair onde estava lá,
            // e não na fase em que parou no Pesadelo.
            var porDificuldade = new Dictionary<Dificuldade, Dictionary<Faccao, int>>(onde.UltimaFase);
            var doNivel = porDificuldade.TryGetValue(dificuldade, out var atual)
                ? new Dictionary<Faccao, int>(atual) : new Dictionary<Faccao, int>();
            doNivel[faccao] = (int)fase;
            porDificuldade[dificuldade] = doNivel;

            Salvar(onde with
            {
                Dificuldade = dificuldade,
                UltimaFase = porDificuldade,
                // `Personagem.Slot` é int (1..4) e o PersonagemService pede o enum — a conversão é
                // aqui, na fronteira do save, pra o record guardar o tipo com significado.
                UltimoTime = time.Select(p => new ApostoloSalvo(p.Faccao, (Slot)p.Slot)).ToList(),
            });
        }

        /// <summary>
        /// Restaura o progresso na ORDEM que importa: capítulos antes de apostolos/itens — os dois se
        /// derivam do FaseConcluida dos capítulos carregados.
        /// </summary>
        public void CarregarSaves()
        {
            _capitulos.CarregarProgresso();
            _apostolos.CarregarApostolos();
            // DEPOIS dos capítulos, e isso é load-bearing: um save antigo (sem inventário) reconstrói
            // o acervo a partir das fases já concluídas, e antes daqui elas ainda não foram lidas.
            _arsenal.CarregarItensEquipados();
            _progressao.Carregar();   // o nível do roster, por último: ele muta as instâncias já montadas
        }

        /// <summary>
        /// O oposto do <see cref="CarregarSaves"/>: devolve TODO o progresso ao estado de jogo novo.
        /// Chamado pelo "excluir conta" (<see cref="PerfilService.Excluir"/>).
        ///
        /// Fica aqui, e não no PerfilService, pelo mesmo motivo que o CarregarSaves fica: quem sabe
        /// quais são as peças do progresso é este service. E cada peça apaga a PRÓPRIA chave — antes,
        /// o PerfilService carregava uma lista `{ "save", "itens", "campanha" }` de strings que
        /// pertenciam a outros três services, e nada obrigava as duas pontas a concordarem.
        /// </summary>
        public void ResetarProgresso()
        {
            _repo.Excluir(ChaveOndeParou);
            _capitulos.Resetar();
            _apostolos.Resetar();
            _arsenal.Resetar();
            _progressao.Resetar();
        }

        /// <summary>
        /// Tudo que acontece ao VENCER uma fase: desbloqueia a próxima, marca concluída, libera os apóstolos
        /// daquela fase, dropa o item, libera a próxima facção (se completou todas) e salva os dois saves.
        /// A ORDEM é load-bearing (snapshot ANTES pra o diff dos novos). Devolve os apóstolos novos + o item.
        /// </summary>
        public RecompensaDaFase ProcessarVitoria(Faccao faccao, Fases fase, Dificuldade dificuldade)
        {
            var antes = _apostolos.ObterDesbloqueados().ToList();

            _capitulos.DesbloquearFase(faccao, fase, dificuldade);
            _capitulos.ConcluirFase(faccao, fase, dificuldade);
            _apostolos.DesbloquearApostolos(faccao, fase, dificuldade);
            List<Item> itens = _arsenal.DroparItens(faccao, fase);
            _capitulos.DesbloquearFaccao(faccao, fase, dificuldade);
            _capitulos.SalvarProgresso();
            _arsenal.SalvarItens();

            var novos = _apostolos.ObterDesbloqueados().Except(antes).ToList();
            return new RecompensaDaFase(novos, itens);
        }
    }

    /// <summary>
    /// Os apóstolos desbloqueados NESTA vitória + as peças que a fase largou. São
    /// <see cref="ArsenalService.ItensPorFase"/> peças, e a lista nunca é nula: repetir a fase larga
    /// mais quatro, porque a peça agora é instância e não uma casa de catálogo que já estava marcada.
    /// </summary>
    public record RecompensaDaFase(List<Personagem> NovosApostolos, List<Item> Itens);

    /// <summary>
    /// Um apóstolo no save. Facção + slot é a IDENTIDADE dele no jogo (é assim que o
    /// <see cref="ApostolosService.EstaDesbloqueado"/> compara), e é o que sobrevive a um roster que
    /// cresce — ao contrário de um índice, que aponta pra outra pessoa assim que a lista muda.
    /// </summary>
    public record ApostoloSalvo(Faccao Faccao, Slot Slot);

    /// <summary>
    /// Onde o jogador parou na campanha: a dificuldade, o capítulo do marcador, a última fase visitada
    /// de CADA capítulo e o time com que entrou por último. Um record só porque é uma pergunta só —
    /// "de onde eu continuo?" — e quatro chaves de save separadas poderiam discordar entre si.
    /// </summary>
    public record OndeParou
    {
        public int Posicao { get; init; }

        /// <summary>A dificuldade em que ele estava. Jogo novo = Fácil, a única aberta.</summary>
        public Dificuldade Dificuldade { get; init; } = Dificuldade.Facil;

        /// <summary>
        /// A última fase visitada (1..7) em cada capítulo, POR DIFICULDADE. Ausente = nunca entrou.
        /// O aninhamento existe porque voltar pro Fácil pra farmar é jogada legítima, e quem volta tem
        /// de cair onde parou LÁ.
        /// </summary>
        public Dictionary<Dificuldade, Dictionary<Faccao, int>> UltimaFase { get; init; } = new();

        public List<ApostoloSalvo> UltimoTime { get; init; } = new();
    }
}
