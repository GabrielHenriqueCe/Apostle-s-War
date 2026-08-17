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
        // esta chave) e sobrevive a fechar o jogo. Enquanto o front gravava direto na porta, esta
        // chave tinha dois donos — um que a escrevia e outro que a apagava.
        private const string ChaveOndeParou = "campanha";

        private readonly ArsenalService _arsenal;
        private readonly ApostolosService _apostolos;
        private readonly CapitulosService _capitulos;
        private readonly PersonagemService _personagens;
        private readonly IRepositorioDeSave _repo;

        public CampanhaService(ArsenalService arsenal, ApostolosService apostolos,
            CapitulosService capitulos, PersonagemService personagens, IRepositorioDeSave repo)
        {
            _arsenal = arsenal;
            _apostolos = apostolos;
            _capitulos = capitulos;
            _personagens = personagens;
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
        /// A fase que abre selecionada neste capítulo: a última em que o jogador ENTROU, ou a 1 se ele
        /// nunca entrou em nenhuma. "Entrou", não "venceu" — quem apanhou numa fase quer voltar
        /// naquela fase, não na seguinte.
        ///
        /// Por capítulo, e não uma só: o jogador vai e volta entre capítulos, e cada um tem a própria
        /// história. Se a fase lembrada tiver ficado travada (só acontece depois de um wipe), cai na 1.
        /// </summary>
        public Fases UltimaFaseDe(Faccao faccao)
        {
            if (Carregar().UltimaFase.TryGetValue(faccao, out int numero)
                && Enum.IsDefined(typeof(Fases), numero)
                && _capitulos.EstaDesbloqueado(faccao, (Fases)numero))
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
        public void SalvarEntradaNaFase(Faccao faccao, Fases fase, List<Personagem> time)
        {
            OndeParou onde = Carregar();
            var fases = new Dictionary<Faccao, int>(onde.UltimaFase) { [faccao] = (int)fase };
            Salvar(onde with
            {
                UltimaFase = fases,
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
            _arsenal.CarregarItensEquipados();
            _capitulos.CarregarProgresso();
            _apostolos.CarregarApostolos();
            _arsenal.CarregarItens();
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
        }

        /// <summary>
        /// Tudo que acontece ao VENCER uma fase: desbloqueia a próxima, marca concluída, libera os apóstolos
        /// daquela fase, dropa o item, libera a próxima facção (se completou todas) e salva os dois saves.
        /// A ORDEM é load-bearing (snapshot ANTES pra o diff dos novos). Devolve os apóstolos novos + o item.
        /// </summary>
        public RecompensaDaFase ProcessarVitoria(Faccao faccao, Fases fase)
        {
            var antes = _apostolos.ObterDesbloqueados().ToList();

            _capitulos.DesbloquearFase(faccao, fase);
            _capitulos.ConcluirFase(faccao, fase);
            _apostolos.DesbloquearApostolos(faccao, fase);
            Item? item = _arsenal.DroparItem(faccao, fase);
            _capitulos.DesbloquearFaccao(faccao, fase);
            _capitulos.SalvarProgresso();
            _arsenal.SalvarItens();

            var novos = _apostolos.ObterDesbloqueados().Except(antes).ToList();
            return new RecompensaDaFase(novos, item);
        }
    }

    /// <summary>Os apóstolos desbloqueados NESTA vitória + o item dropado (null se a fase já tinha caído).</summary>
    public record RecompensaDaFase(List<Personagem> NovosApostolos, Item? Item);

    /// <summary>
    /// Um apóstolo no save. Facção + slot é a IDENTIDADE dele no jogo (é assim que o
    /// <see cref="ApostolosService.EstaDesbloqueado"/> compara), e é o que sobrevive a um roster que
    /// cresce — ao contrário de um índice, que aponta pra outra pessoa assim que a lista muda.
    /// </summary>
    public record ApostoloSalvo(Faccao Faccao, Slot Slot);

    /// <summary>
    /// Onde o jogador parou na campanha: o capítulo do marcador, a última fase visitada de CADA
    /// capítulo, e o time com que entrou por último. Um record só porque é uma pergunta só — "de onde
    /// eu continuo?" — e três chaves de save separadas poderiam discordar entre si.
    /// </summary>
    public record OndeParou
    {
        public int Posicao { get; init; }

        /// <summary>A última fase visitada em cada capítulo (1..7). Capítulo ausente = nunca entrou.</summary>
        public Dictionary<Faccao, int> UltimaFase { get; init; } = new();

        public List<ApostoloSalvo> UltimoTime { get; init; } = new();
    }
}
