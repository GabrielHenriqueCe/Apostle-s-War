using System.Collections.Generic;
using System.Linq;
using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;

namespace ApostlesWar.Application.Services
{
    /// <summary>
    /// O META da campanha, FORA da luta: carregar os saves e processar a recompensa da vitória
    /// (desbloqueios + drop + save). Não desenha nada — devolve o resultado, e a casca (o FluxoDoFront)
    /// mostra do seu jeito. Nasceu porque HAVIA duas cascas duplicando isto; a lição sobrevive à
    /// segunda: a lógica meta mora na Application, nunca no front.
    /// </summary>
    public class CampanhaService
    {
        // Onde o jogador parou no mapa. Mora aqui e não no front porque "último lugar" é PROGRESSÃO,
        // não estado de tela: some junto com a conta (o PerfilService já apaga esta chave no wipe) e
        // sobrevive a fechar o jogo. Enquanto o front gravava direto na porta, esta chave tinha dois
        // donos — um que a escrevia e outro que a apagava.
        private const string ChavePosicao = "campanha";

        private readonly ArsenalService _arsenal;
        private readonly CampeoesService _campeoes;
        private readonly CapitulosService _capitulos;
        private readonly IRepositorioDeSave _repo;

        public CampanhaService(ArsenalService arsenal, CampeoesService campeoes,
            CapitulosService capitulos, IRepositorioDeSave repo)
        {
            _arsenal = arsenal;
            _campeoes = campeoes;
            _capitulos = capitulos;
            _repo = repo;
        }

        /// <summary>Em que capítulo o marcador do mapa está. Sem save = 0 = o primeiro.</summary>
        public int PosicaoNoMapa() => _repo.Carregar<int>(ChavePosicao);

        /// <summary>Guarda o "último lugar" — o marcador reabre onde o jogador deixou.</summary>
        public void SalvarPosicao(int indice) => _repo.Salvar(ChavePosicao, indice);

        /// <summary>
        /// Restaura o progresso na ORDEM que importa: capítulos antes de champs/itens — os dois se
        /// derivam do FaseConcluida dos capítulos carregados.
        /// </summary>
        public void CarregarSaves()
        {
            _arsenal.CarregarItensEquipados();
            _capitulos.CarregarProgresso();
            _campeoes.CarregarCampeoes();
            _arsenal.CarregarItens();
        }

        /// <summary>
        /// Tudo que acontece ao VENCER uma fase: desbloqueia a próxima, marca concluída, libera os champs
        /// daquela fase, dropa o item, libera a próxima facção (se completou todas) e salva os dois saves.
        /// A ORDEM é load-bearing (snapshot ANTES pra o diff dos novos). Devolve os champs novos + o item.
        /// </summary>
        public RecompensaDaFase ProcessarVitoria(Faccao faccao, Fases fase)
        {
            var antes = _campeoes.ObterDesbloqueados().ToList();

            _capitulos.DesbloquearFase(faccao, fase);
            _capitulos.ConcluirFase(faccao, fase);
            _campeoes.DesbloquearCampeoes(faccao, fase);
            Item? item = _arsenal.DroparItem(faccao, fase);
            _capitulos.DesbloquearFaccao(faccao, fase);
            _capitulos.SalvarProgresso();
            _arsenal.SalvarItens();

            var novos = _campeoes.ObterDesbloqueados().Except(antes).ToList();
            return new RecompensaDaFase(novos, item);
        }
    }

    /// <summary>Os champs desbloqueados NESTA vitória + o item dropado (null se a fase já tinha caído).</summary>
    public record RecompensaDaFase(List<Personagem> NovosCampeoes, Item? Item);
}
