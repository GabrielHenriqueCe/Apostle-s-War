using ApostlesWar.Application.Portas;

namespace ApostlesWar.Application.Services
{
    /// <summary>
    /// As PREFERÊNCIAS do jogador — como ele gosta de jogar, não o que ele conquistou.
    ///
    /// Slot de save próprio (<c>"config"</c>) e FORA do wipe do "excluir conta": apagar a conta zera
    /// o progresso, não o gosto de quem está sentado na cadeira. Quem some junto com a conta é o que
    /// o <see cref="CampanhaService.ResetarProgresso"/> lista; esta chave não está lá de propósito.
    ///
    /// Um campo só (tela cheia) por enquanto — som e as outras opções "em breve" do menu caem aqui
    /// quando chegarem, e é pra isso que o dado é um record e não um bool solto na porta.
    /// </summary>
    public class ConfiguracaoService
    {
        private const string ChaveConfig = "config";

        private readonly IRepositorioDeSave _repositorio;

        public ConfiguracaoService(IRepositorioDeSave repositorio) => _repositorio = repositorio;

        /// <summary>
        /// O que está valendo. Sem save (instalação nova) → o <see cref="Configuracao"/> padrão, que
        /// abre em TELA CHEIA: é assim que um jogo abre, e quem prefere janela desmarca uma vez e a
        /// escolha fica.
        /// </summary>
        public Configuracao Carregar()
            => _repositorio.Carregar<Configuracao>(ChaveConfig) ?? new Configuracao();

        public void Salvar(Configuracao config) => _repositorio.Salvar(ChaveConfig, config);

        /// <summary>Vira a tela cheia e devolve como ela ficou — o chamador aplica na janela.</summary>
        public bool AlternarTelaCheia()
        {
            Configuracao atual = Carregar();
            var nova = atual with { TelaCheia = !atual.TelaCheia };
            Salvar(nova);
            return nova.TelaCheia;
        }
    }

    /// <summary>As preferências, como um dado só — é o que vai e volta do slot de save.</summary>
    public record Configuracao(bool TelaCheia = true);
}
