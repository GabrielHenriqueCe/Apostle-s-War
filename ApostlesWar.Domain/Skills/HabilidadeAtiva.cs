using ApostlesWar.Domain.Skills;

namespace ApostlesWar.Domain
{
    #region HabilidadeAtiva

    /// <summary>
    /// Concreta e construível por DADO (forma-construtor — ver ADR-composicao-de-acoes §3.1):
    /// a forma final de uma habilidade é `new HabilidadeAtiva(..., acoes: [...])`, montada no
    /// arquivo do apóstolo. Durante a migração (Strangler), as habilidades antigas continuam como
    /// subclasses usando o construtor curto + override das propriedades virtuais.
    /// </summary>
    public class HabilidadeAtiva : Habilidade
    {

        private readonly int _numeroDeAlvos;
        private readonly TipoAlvo _tipoAlvo;
        private readonly TipoLista _tipoLista;
        private readonly EstadoAlvo _estadoAlvo;
        private readonly TipoAtaque _tipoAtaque;
        private readonly List<Acao> _acoes;

        /// <summary>
        /// Construtor de SUBCLASSE (Strangler): a subclasse sobrescreve as propriedades
        /// virtuais — os backings ficam em default e nunca são lidos. Some quando a última
        /// subclasse migrar pra forma-construtor.
        /// </summary>
        public HabilidadeAtiva(string nome, string simbolo, int cooldown, string descricao = "")
            : base(nome, simbolo, cooldown, descricao)
        {
            _tipoAtaque = TipoAtaque.Sequencial;
            _acoes = new List<Acao>();
        }

        /// <summary>
        /// Construtor de DADO (forma final): a habilidade inteira é config. A lista de Acoes é
        /// criada UMA vez e reusada a cada ativação — por isso Acao só pode carregar config
        /// (multiplicador, fábrica), nunca estado por-ativação (invariante do ADR §3.1).
        /// </summary>
        public HabilidadeAtiva(string nome, string simbolo, int cooldown, string descricao,
            int numeroDeAlvos, TipoAlvo tipoAlvo, TipoLista tipoLista, EstadoAlvo estadoAlvo,
            List<Acao> acoes, TipoAtaque tipoAtaque = TipoAtaque.Sequencial)
            : base(nome, simbolo, cooldown, descricao)
        {
            _numeroDeAlvos = numeroDeAlvos;
            _tipoAlvo = tipoAlvo;
            _tipoLista = tipoLista;
            _estadoAlvo = estadoAlvo;
            _tipoAtaque = tipoAtaque;
            _acoes = acoes;
        }

        public virtual int NumeroDeAlvos => _numeroDeAlvos;
        public virtual TipoAlvo TipoAlvo => _tipoAlvo;

        /// <summary>
        /// Define qual lista a habilidade considera como "principal" pra selecionar alvos.
        /// O CombateService usa isso pra mostrar a lista certa de alvos pro jogador.
        /// A habilidade ainda tem acesso às duas listas via ContextoCombate.
        /// </summary>
        public virtual TipoLista TipoLista => _tipoLista;

        /// <summary>
        /// Define qual estado de vida a habilidade mira dentro da TipoLista, alimentando a
        /// resolução de alvos e o menu (ver ADR-selecao-por-estado.md). Cada AÇÃO re-filtra
        /// pelo EstadoAlvo dela na execução (ver ADR-composicao-de-acoes §5.3).
        /// </summary>
        public virtual EstadoAlvo EstadoAlvo => _estadoAlvo;

        /// <summary>
        /// Esta habilidade PEDE um alvo ao jogador (abre o passo de escolha)? Regra ESTÁTICA — só olha
        /// a config, não o tabuleiro: inimigos sempre pedem; aliados só quando o número de alvos é
        /// finito; Self e hit-all resolvem sozinhos. É a FONTE ÚNICA da regra: o
        /// <c>CombateService.ResolverAlvoInicial</c> e o front (que monta o menu de habilidade) leem
        /// daqui em vez de reescrever a expressão. A parte que DEPENDE do tabuleiro (aliado finito sem
        /// candidato vivo → não abre o pick) segue no service, que é quem tem as listas.
        /// </summary>
        public bool PedeAlvoDoJogador =>
            TipoLista == TipoLista.Inimigos
            || (TipoLista == TipoLista.Aliados && NumeroDeAlvos != int.MaxValue);

        /// <summary>
        /// Ações que a habilidade executa (Balde 1: só aplica uma lista fixa de efeitos).
        /// Na forma-construtor vêm do ctor; subclasses Strangler sobrescrevem (ou sobrescrevem
        /// Ativar direto, se ainda bespoke). Ver ADR-composicao-de-acoes.md.
        ///
        /// PÚBLICA porque a habilidade é DADO, e dado se lê: quem precisa raciocinar sobre o que ela
        /// faz — sem executá-la — lê esta lista. É o que o controlador automático usa pra decidir se
        /// vale a pena usar a habilidade, perguntando a cada ação o que ela entrega. Uma habilidade
        /// que só se revela executando não poderia ser avaliada.
        /// </summary>
        public virtual List<Acao> Acoes => _acoes;

        /// <summary>
        /// A chance de esta habilidade COLAR neste alvo — o número do 🎲 na tela (GDD-combate §1).
        /// 1,0 quando ela não impõe nada a ele, e é aí que a tela some com a linha: habilidade sem
        /// malefício, alvo sem Resistência e alvo fora do escopo do malefício caem todos no mesmo
        /// 100%, sem que ninguém precise perguntar qual dos três é.
        ///
        /// <b>O MENOR entre as ações que alcançam o alvo</b>, e não o produto nem a média: são
        /// tentativas INDEPENDENTES (cada malefício rola o seu dado), então nenhum número único as
        /// descreve — e o menor é o único que nunca promete mais do que a parte mais frágil entrega.
        /// Hoje é exato, porque nenhuma habilidade tem dois malefícios de chances diferentes; quando
        /// a RARIDADE começar a mexer no `chance` de cada ação isso deixa de ser verdade, e aí a
        /// escolha entre um número e uma linha por malefício volta à mesa (o GDD crava UM número).
        ///
        /// <paramref name="alvoEhInimigo"/> vem de fora porque quem sabe de que lado alguém está é a
        /// estrutura da batalha, não a habilidade. Sem ele o Desejo do Gênio (buff nos aliados,
        /// Maldição nos inimigos) mostraria a chance da Maldição também nos aliados.
        /// </summary>
        public double ChanceDeAplicarEm(Combate atacante, Combate alvo, bool alvoEhInimigo)
        {
            double menor = 1.0;
            foreach (Acao acao in Acoes)
                if (Alcanca(acao.Escopo, atacante, alvo, alvoEhInimigo))
                    menor = Math.Min(menor, acao.PreverChanceDeAplicar(atacante, alvo));
            return menor;
        }

        /// <summary>
        /// Este escopo cai neste alvo? É a mesma pergunta que o <see cref="Ativar"/> responde
        /// montando LISTAS; aqui ela é feita de um alvo só, porque quem pergunta já tem o campo
        /// inteiro na mão e quer a resposta por combatente.
        /// </summary>
        private bool Alcanca(Escopo escopo, Combate atacante, Combate alvo, bool alvoEhInimigo) => escopo switch
        {
            Escopo.ProprioAtacante => alvo == atacante,
            Escopo.TodosAliados => !alvoEhInimigo,
            Escopo.OutrosAliados => !alvoEhInimigo && alvo != atacante,
            Escopo.TodosInimigos => alvoEhInimigo,
            // AlvosResolvidos herda a mira da habilidade: é o escopo que diz "onde ela já estava mirando".
            _ => TipoLista switch
            {
                TipoLista.Inimigos => alvoEhInimigo,
                TipoLista.Aliados => !alvoEhInimigo,
                _ => alvo == atacante,
            },
        };

        /// <summary>
        /// Interpretador default (o MOTOR — ver ADR-composicao-de-acoes §3): roda cada Acao na
        /// ordem declarada, resolvendo o Escopo de cada uma e filtrando pelo EstadoAlvo NO
        /// MOMENTO em que ela executa. É "ação-por-fora": uma ação termina toda a sua passada
        /// antes da próxima começar — é isso que faz o dano agregar (o eventos já está completo
        /// quando uma ação seguinte o lê) e que faz uma ação pegar o estado que a anterior
        /// deixou (revive → cura os revividos). Habilidades bespoke sobrescrevem este Ativar; as
        /// duas formas convivem durante a migração (Strangler).
        ///
        /// A resolução dos AlvosResolvidos (o pick + extras) ainda usa o EstadoAlvo da HABILIDADE
        /// — é o que alimenta o menu de alvo. Cada ação re-filtra o seu conjunto pelo EstadoAlvo
        /// DELA na execução. A derivação do pick a partir das ações (§8.2) fica pra depois.
        /// </summary>
        public override List<EventoCombate> Ativar(ContextoCombate ctx, Combate alvo)
        {
            var eventos = new List<EventoCombate>();
            var alvosResolvidos = ResolverAlvos(alvo, ObterListaPrincipal(ctx));
            foreach (var acao in Acoes)
                foreach (Combate combatente in ResolverEscopo(acao, alvosResolvidos, ctx))
                    acao.Executar(ctx.Atacante, combatente, eventos);
            return eventos;
        }

        /// <summary>
        /// Quem esta ação PODERIA atingir, do ponto de vista de quem ainda está DECIDINDO usar a
        /// habilidade — antes, portanto, de existir um alvo escolhido.
        ///
        /// Difere do que roda na ativação em um ponto, e de propósito: onde o `Ativar` usa os alvos
        /// já resolvidos (o pick + os extras, limitados por `NumeroDeAlvos`), aqui entra a lista
        /// principal inteira. É a leitura certa pra pergunta "vale a pena?" — uma cura só faz sentido
        /// se ALGUÉM do lado está ferido, independente de qual deles o pick vai sortear depois.
        /// O resto do caminho é o mesmo (`FiltrarPorEstado` da habilidade → `ResolverEscopo` da ação),
        /// então escopo e estado respondem uma vez só, aqui e lá.
        /// </summary>
        public List<Combate> AlvosPossiveis(Acao acao, ContextoCombate ctx)
            => ResolverEscopo(acao, FiltrarPorEstado(ObterListaPrincipal(ctx)), ctx);

        /// <summary>
        /// Monta o conjunto de combatentes que uma ação atinge: pega a lista do Escopo dela e
        /// filtra pelo EstadoAlvo dela, avaliado AGORA (não na resolução). Snapshot (ToList) pra
        /// não iterar uma coleção que a própria ação pode alterar (ex: reviver muda quem é vivo).
        ///
        /// PÚBLICO porque quem AVALIA a habilidade antes de usá-la (o bot) precisa da mesma resposta
        /// que o interpretador vai dar — "quem esta ação atinge?" tem que ser uma pergunta só. Uma
        /// segunda leitura das regras de escopo divergiria da primeira.
        /// </summary>
        public List<Combate> ResolverEscopo(Acao acao, List<Combate> resolvidos, ContextoCombate ctx)
        {
            IEnumerable<Combate> conjunto = acao.Escopo switch
            {
                Escopo.AlvosResolvidos => resolvidos,
                Escopo.TodosAliados => ctx.Aliados,
                Escopo.TodosInimigos => ctx.Inimigos,
                Escopo.ProprioAtacante => new List<Combate> { ctx.Atacante },
                Escopo.OutrosAliados => ctx.Aliados.Where(c => c != ctx.Atacante),
                _ => resolvidos,
            };
            return conjunto
                .Where(c => acao.EstadoAlvo == EstadoAlvo.Mortos ? !c.EstaVivo() : c.EstaVivo())
                .ToList();
        }

        /// <summary>
        /// Retorna a lista correspondente ao TipoLista da habilidade.
        /// Conveniência pra resolver alvos.
        /// </summary>
        protected List<Combate> ObterListaPrincipal(ContextoCombate ctx) => TipoLista switch
        {
            TipoLista.Aliados => ctx.Aliados,
            TipoLista.Inimigos => ctx.Inimigos,
            TipoLista.Self => new List<Combate> { ctx.Atacante },
            _ => ctx.Inimigos
        };

        /// <summary>
        /// Filtra a lista pelo EstadoAlvo declarado pela habilidade (Vivos ou Mortos).
        /// </summary>
        private List<Combate> FiltrarPorEstado(List<Combate> lista) => EstadoAlvo switch
        {
            EstadoAlvo.Mortos => lista.Where(c => !c.EstaVivo()).ToList(),
            _ => lista.Where(c => c.EstaVivo()).ToList(),
        };

        /// <summary>
        /// Monta a lista de alvos com base em TipoAlvo, NumeroDeAlvos e EstadoAlvo.
        ///
        /// CONTRATO da semente (`alvoSelecionado`): ela tem que ser um dos `candidatos` — ou seja,
        /// já ter passado pelo MESMO EstadoAlvo que esta habilidade declara. Quem escolhe o alvo
        /// (`CombateService.ResolverAlvoInicial` → menu do jogador ou bot) filtra por
        /// `hab.EstadoAlvo` antes de escolher, então o contrato vale hoje pra todos os apóstolos.
        /// Se um apóstolo futuro violar (ex: pedir alvo Vivo e mandar um morto), a semente entraria
        /// no resultado sem ser candidata E o `IndexOf` abaixo devolveria -1, desalinhando o `%`
        /// dos extras — erro silencioso. Por isso o guard EXPLODE em vez de "consertar":
        /// mira errada é bug de declaração do apóstolo, tem que gritar na primeira execução.
        ///
        /// O guard vem DEPOIS do early-return de lista vazia de propósito: "não há candidato no
        /// estado pedido" é caso LEGÍTIMO (Doces de Abóbora sem nenhum aliado morto — o
        /// CombateService manda o próprio atacante como semente e conta com o vazio aqui, ver
        /// CombateService.ResolverAlvoInicial). Vazio = habilidade sem alvo, não é violação.
        ///
        /// E vem DEPOIS do hit-all pelo mesmo motivo: onde não há PICK, não há semente pra validar.
        /// Numa habilidade de todos os alvos o CombateService nem pergunta (PedeAlvoDoJogador é
        /// false pro aliado hit-all) — ele manda o próprio atacante como placeholder, e a semente
        /// é ficção. O contrato acima existe pra manter o `IndexOf` alinhado com o sorteio dos
        /// EXTRAS; sem extras a sortear, não há o que proteger. Cobrar a semente aqui explodia os
        /// revive-de-todos (Technology, Atlantis, Céu, Circo, Anjo Caído: hit-all + Mortos, semente
        /// = atacante vivo) justamente quando havia alguém pra reviver.
        /// </summary>
        protected List<Combate> ResolverAlvos(Combate alvoSelecionado, List<Combate> lista)
        {
            var candidatos = FiltrarPorEstado(lista);
            var resultado = new List<Combate>();

            if (candidatos.Count == 0) return resultado;

            // Hit-all: o conjunto É a lista de candidatos, na ordem do tabuleiro. Sem pick, sem extras.
            if (NumeroDeAlvos == int.MaxValue) return candidatos;

            if (!candidatos.Contains(alvoSelecionado))
                throw new InvalidOperationException(
                    $"'{Nome}': o alvo selecionado ({alvoSelecionado.Personagem.Nome}) não é candidato " +
                    $"válido pro EstadoAlvo.{EstadoAlvo} da habilidade. A seleção de alvo tem que " +
                    $"filtrar pelo MESMO estado que a habilidade declara.");

            resultado.Add(alvoSelecionado);

            int extras = NumeroDeAlvos - 1;   // hit-all já saiu acima

            if (extras <= 0) return resultado;

            if (TipoAlvo == TipoAlvo.Explicito)
            {
                int inicio = candidatos.IndexOf(alvoSelecionado);
                for (int i = 1; i <= extras; i++)
                {
                    int idx = (inicio + i) % candidatos.Count;
                    Combate proximo = candidatos[idx];
                    if (!resultado.Contains(proximo))
                        resultado.Add(proximo);
                }
            }
            else
            {
                for (int i = 0; i < extras; i++)
                    resultado.Add(candidatos[Random.Shared.Next(candidatos.Count)]);
            }

            return resultado;
        }

        protected EventoDano AplicarDano(Combate atacante, Combate alvo, double multiplicador = 1.0)
            => atacante.Atacar(alvo, multiplicador);

        protected void AplicarCura(Combate alvo, double percentual)
            => alvo.Curar((int)(alvo.HPMaximo * percentual));

        protected void AplicarBuff(Combate alvo, Buff buff)
            => buff.Aplicar(alvo);

        protected void AplicarDebuff(Combate alvo, Debuff debuff)
            => debuff.Aplicar(alvo);

        protected List<EventoCombate> SemDano() => new List<EventoCombate>();

        /// <summary>
        /// Semântica do evento de ataque. Default Sequencial (a maioria das habilidades de dano
        /// single-target ou multi-hit). Na forma-construtor vem do ctor; subclasses AoE e
        /// não-atacantes sobrescrevem.
        /// O CombateService usa isso pra decidir quantas vezes dispara passivas DepoisDeAtacar.
        /// </summary>
        public virtual TipoAtaque TipoAtaque => _tipoAtaque;
    }

    #endregion
}