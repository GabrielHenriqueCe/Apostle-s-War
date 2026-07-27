using ApostlesWar.Application.Portas;
using ApostlesWar.Domain;
namespace ApostlesWar.Presentation.Front
{
    /// <summary>
    /// O RETRATO da partida que vai pro JS desenhar. É o coração do contrato da ponte: o front recebe
    /// ESTADO (o que é verdade agora) e desenha do zero, em vez de receber ordens de desenho. Por isso
    /// trocar emoji por sprite depois não toca no motor — muda só quem lê este retrato.
    ///
    /// São records simples de propósito: viram JSON direto (System.Text.Json) e nenhum objeto vivo do
    /// domínio (Combate, Habilidade) atravessa a ponte. Isso é o mesmo espírito do "EventoDano por ID"
    /// registrado no ROADMAP — o front não segura referência pro motor, só um espelho.
    /// </summary>
    internal record EstadoDeBatalha(
        int Turno,
        FaseDaTela Fase,
        List<CombatenteVisto> Equipe1,
        List<CombatenteVisto> Equipe2,
        int? QuemAge,                       // índice global (Id) de quem está agindo — o contorno verde
        List<HabilidadeVista> Habilidades,  // as do QuemAge, quando é a vez de um humano
        List<int> AlvosValidos,             // Ids clicáveis agora (vazio = ninguém)
        string? Mensagem,                   // linha de narração (uso de habilidade, passiva...)
        int LadoVencedor = 0,               // no Fim: 1=esquerda venceu, 2=direita; 0=sem split (campanha)
        bool Auto = false                   // o automático está ligado? (o botão se desenha daqui)
    );

    /// <summary>Em que ponto do turno a tela está — o JS usa pra saber o que é clicável.</summary>
    internal enum FaseDaTela
    {
        Assistindo,        // bot agindo / animação rolando: nada clicável
        EscolhendoAcao,    // humano escolhe habilidade
        EscolhendoAlvo,    // habilidade escolhida, falta o alvo
        Fim                // batalha acabou
    }

    /// <summary>
    /// Um combatente como a tela o vê. Traz os números TODOS (ATK/DEF/crit) porque hoje o Gabriel
    /// está testando balance e precisa enxergar tudo; o botão de esconder vive no front, não aqui —
    /// mandar o dado e deixar a tela decidir o que mostrar é mais barato que ter dois formatos.
    /// </summary>
    internal record CombatenteVisto(
        int Id,
        string Nome,
        string Simbolo,
        int HPAtual,
        int HPMaximo,
        int Escudo,
        int Ataque,
        int Defesa,
        int TaxaCritPct,
        int DanoCritPct,
        bool Vivo,
        List<StatusVisto> Status
    );

    internal record StatusVisto(string Nome, string Simbolo, int DuracaoRestante, bool EhBuff);

    internal record HabilidadeVista(
        int Indice,
        string Nome,
        string Simbolo,
        string Descricao,
        int CooldownRestante,
        bool Disponivel,
        /// <summary>
        /// Se o motor vai abrir um passo de ESCOLHA DE ALVO depois deste clique. A tela usa isto
        /// pra decidir onde pedir a confirmação: quem pede alvo já tem um segundo passo natural
        /// (clicar no alvo), quem não pede precisa de um segundo clique na própria habilidade —
        /// senão o primeiro clique gastaria o turno sem direito a mudar de ideia.
        ///
        /// É um PALPITE da tela, espelhando `CombateService.ResolverAlvoInicial`. Errar é barato:
        /// palpitou "pede" e não pedia → dispara no 1º clique (como era antes); palpitou "não pede"
        /// e pedia → um clique a mais. Nenhum dos dois corrompe estado.
        /// </summary>
        bool PedeAlvo,
        /// <summary>
        /// Em quem a habilidade age: "Inimigos", "Aliados" ou "Self" (espelha a TipoLista). Só a tela
        /// usa, e só pras que NÃO pedem alvo (Self, buff em todos os aliados): armada a habilidade, os
        /// combatentes válidos brilham e clicar num deles confirma — clicar em si mesmo pro buff
        /// próprio, num aliado pro buff de aliado. O duplo-clique na habilidade segue valendo.
        /// </summary>
        string Escopo
    );

    /// <summary>
    /// Um acontecimento pra ANIMAR (dano, cura, morte). Vai num canal separado do estado de propósito:
    /// o estado diz "como as coisas estão", o evento diz "o que acabou de acontecer" — é o segundo que
    /// faz o número pular e o alvo tremer. O motor já emite EventoDano/EventoCura desde o #7b; aqui só
    /// viram formato de tela.
    /// </summary>
    internal record EventoVisto(
        string Tipo,        // "dano" | "cura" | "morte" | "narracao"
        int? AlvoId,
        int Valor,
        bool Critico,
        int AbsorvidoPeloEscudo,
        string? Texto
    );

    /// <summary>
    /// Um menu pra tela desenhar. Mesma ideia do <see cref="EstadoDeBatalha"/>, do lado dos menus: o
    /// C# manda "o que mostrar" e o JS pinta botões — o clique volta como o ÍNDICE da opção. Menu com
    /// cursor é formato de console; no front cada opção é um botão (ver ITelaDeCombate sobre o mesmo
    /// princípio pro combate).
    ///
    /// <see cref="Raiz"/> = é o menu PRINCIPAL (não um submenu): decide o que o Esc faz na tela —
    /// na raiz confirma sair do jogo; num submenu, volta um nível. <see cref="Avatar"/>/<see cref="Nome"/>
    /// != null = mostra a moldura do jogador (avatar + nome + botão ✏️ de editar); só o menu principal manda.
    /// </summary>
    internal record MenuVisto(string Titulo, string? Subtitulo, List<OpcaoMenuVista> Opcoes,
        bool Raiz = false, string? Avatar = null, string? Nome = null);

    /// <summary>
    /// Uma opção clicável. `Habilitado: false` = aparece apagada com "em breve" (fatia futura).
    /// <see cref="Confirmar"/> != null = ação destrutiva: o clique abre um modal com esse texto e só
    /// dispara a escolha se o jogador confirmar (ex.: excluir conta).
    /// </summary>
    internal record OpcaoMenuVista(string Rotulo, string Icone, bool Habilitado, string? Confirmar = null);

    /// <summary>Um campeão na grade de escolha de avatar. `Desbloqueado: false` = aparece em cinza,
    /// não clicável (ainda não conquistado na campanha).</summary>
    internal record CampeaoVisto(string Simbolo, string Nome, bool Desbloqueado);

    /// <summary>
    /// A tela de EDITAR PERFIL: o nome atual (pra pré-preencher), o avatar atual (pra pré-selecionar
    /// na grade) e a lista completa de campeões (a ORDEM é o índice que o clique devolve).
    /// </summary>
    internal record EdicaoPerfilVista(string Nome, string Avatar, List<CampeaoVisto> Campeoes);

    /// <summary>
    /// A montagem da Arena que o front devolve: os dois times como ÍNDICES na lista de campeões
    /// (mesma ordem do TodosOsCampeoes que foi enviada) + quem é bot de cada lado. Vem serializado no
    /// campo Texto da mensagem (a ponte só carrega 1 int/1 string por clique).
    /// </summary>
    internal record ArenaConfig(int[] Time1, int[] Time2, bool Bot1, bool Bot2);

    // ---------- Campanha ----------

    /// <summary>Um nó do mapa = uma facção-capítulo. Índice = posição na lista enviada (Reino..Apóstolos).</summary>
    internal record CapituloVista(string Simbolo, string Nome, bool Desbloqueado, bool Concluido);

    /// <summary>O mapa: as 8 facções em ordem + a posição atual (índice) onde o marcador começa.</summary>
    internal record MapaVista(List<CapituloVista> Capitulos, int Posicao);

    /// <summary>Um item pra mostrar (drop da fase / recompensa).</summary>
    internal record ItemVista(string Simbolo, string Nome, string Stat, string Valor);

    /// <summary>Uma fase: número (1..7), nome (do item), status, inimigos das 2 rodadas e o item que dropa.</summary>
    internal record FaseVista(int Numero, string Nome, bool Desbloqueado, bool Concluido,
        List<CampeaoVisto> Rodada1, List<CampeaoVisto> Rodada2, ItemVista Item);

    /// <summary>A tela de fases de uma facção: as 7 fases + o pool de champs desbloqueados pra montar o time.</summary>
    internal record FasesVista(string CapituloNome, string CapituloSimbolo, List<FaseVista> Fases,
        List<CampeaoVisto> MeusCampeoes);

    /// <summary>Recompensa da vitória: os champs novos desbloqueados + o item dropado (null se já tinha).</summary>
    internal record RecompensaVista(List<CampeaoVisto> Novos, ItemVista? Item);

    /// <summary>O que o front devolve ao iniciar uma fase: a fase (1..7) e o time como índices dos desbloqueados.</summary>
    internal record FaseConfig(int Fase, int[] Time);

    // ---------- Compêndio ----------

    /// <summary>
    /// Uma habilidade como o COMPÊNDIO a mostra — só o que se lê fora da luta. Não reusa a
    /// <see cref="HabilidadeVista"/> de propósito: aquela existe pra ser CLICADA (índice, cooldown
    /// restante, disponível, pede alvo) e nada disso faz sentido num catálogo, onde não há turno nem
    /// dono. Metade dos campos viriam vazios e alguém, um dia, tentaria preenchê-los.
    ///
    /// <see cref="Passiva"/> separa a passiva das duas ativas: ela não se usa, e a tela precisa dizer
    /// isso em vez de deixar o jogador procurando o botão.
    /// </summary>
    internal record HabilidadeDoChampVista(string Nome, string Simbolo, string Descricao,
        int Cooldown, bool Passiva);

    /// <summary>
    /// Um champ na grade do compêndio. <see cref="Indice"/> é a posição na lista COMPLETA (a mesma
    /// ordem do TodosOsCampeoes) — é o que o clique devolve, e é global às facções justamente pra
    /// não precisar mandar (facção, slot) de volta pela ponte, que carrega um int só.
    /// </summary>
    internal record CompendioChampVista(int Indice, string Simbolo, string Nome, bool Desbloqueado);

    /// <summary>Uma facção com os 4 champs dela, na ordem dos slots. É como o catálogo se agrupa.</summary>
    internal record CompendioFaccaoVista(string Nome, string Simbolo, List<CompendioChampVista> Champs);

    /// <summary>O catálogo inteiro: as 9 facções, cada uma com seus 4 — travados incluídos.</summary>
    internal record CompendioVista(List<CompendioFaccaoVista> Faccoes);

    /// <summary>
    /// A FICHA de um champ. Os números são os de BASE (o que ele é antes de arsenal, buff ou item):
    /// o compêndio é catálogo, não simulador — quem quer saber o efeito do equipamento olha o
    /// Arsenal. Crit não vem do <see cref="Personagem"/> porque lá ele não existe por champ: é o
    /// <c>TaxaCritBase</c>/<c>DanoCritBase</c>, global a todo mundo.
    ///
    /// <see cref="Desbloqueado"/> só pinta a moldura — a ficha é COMPLETA mesmo travada, porque um
    /// catálogo que esconde o que você ainda não tem não serve pra planejar a campanha.
    /// </summary>
    internal record ChampDetalheVista(string Nome, string Simbolo, string Faccao, bool Desbloqueado,
        int HP, int Ataque, int Defesa, int TaxaCritPct, int DanoCritPct,
        List<HabilidadeDoChampVista> Habilidades);

    // ---------- Arsenal ----------

    /// <summary>
    /// Um item obtido, pra o arsenal. <see cref="Indice"/> = posição na lista de obtidos (é o que o
    /// clique devolve pra equipar). <see cref="Slot"/> = qual dos 7 slots ele ocupa (0..6 = Fase-1).
    /// <see cref="ValorNum"/> = valor cru pra a diferença (equipado × novo) ser calculada no front.
    /// </summary>
    internal record ItemArsenalVista(int Indice, string Simbolo, string Nome, string Faccao, int Slot,
        string Stat, string Valor, double ValorNum, bool Equipado);

    /// <summary>Um dos 7 slots do boneco: nome do tipo + o item equipado (null = vazio).</summary>
    internal record SlotArsenalVista(int Slot, string Nome, ItemArsenalVista? Equipado);

    /// <summary>O arsenal: os 7 slots (equipados) + todos os itens obtidos (pra escolher ao clicar um slot).</summary>
    internal record ArsenalVista(List<SlotArsenalVista> Slots, List<ItemArsenalVista> Obtidos);
}
