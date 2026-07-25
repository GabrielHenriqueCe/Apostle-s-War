using System.Collections.Generic;
using System.Linq;
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
        private readonly ArsenalService _arsenal;
        private readonly CampeoesService _campeoes;
        private readonly CapitulosService _capitulos;

        public CampanhaService(ArsenalService arsenal, CampeoesService campeoes, CapitulosService capitulos)
        {
            _arsenal = arsenal;
            _campeoes = campeoes;
            _capitulos = capitulos;
        }

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
