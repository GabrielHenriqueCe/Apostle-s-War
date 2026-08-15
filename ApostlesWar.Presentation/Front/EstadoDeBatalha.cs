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
        /// <summary>
        /// A ORDEM DOS PRÓXIMOS TURNOS, do primeiro ao último, já calculada pelo motor. O front NÃO
        /// deduz ordem nenhuma: quem joga quando sai da <c>FilaDeTurnos</c>, e uma segunda conta aqui
        /// faria o cordão prometer o que a batalha não cumpre.
        /// </summary>
        List<VezVista> Fila,
        List<HabilidadeVista> Habilidades,  // as do QuemAge, quando é a vez de um humano
        List<int> AlvosValidos,             // Ids clicáveis agora (vazio = ninguém)
        string? Mensagem,                   // linha de narração (uso de habilidade, passiva...)
        int LadoVencedor = 0,               // no Fim: 1=esquerda venceu, 2=direita; 0=sem split (campanha)
        bool Auto = false,                  // o automático está ligado? (o botão se desenha daqui)
        int FocoId = 0,                     // inimigo apontado no automático (0 = nenhum)
        /// <summary>
        /// Em que modo esta batalha está rolando. A tela usa pra dizer a verdade no botão de saída:
        /// na CAMPANHA desistir não sai do jogo, conta DERROTA e cai na tela de fim de fase (o
        /// caminho já era esse — `sair` vira `BatalhaAbortada` e a fase vira Perdeu); na ARENA sair é
        /// sair mesmo, sem desfecho. Duas consequências diferentes não podem ter o mesmo rótulo.
        /// </summary>
        string Modo = ModoDeBatalha.Arena,
        /// <summary>
        /// A PELE do campo de batalha — o front põe num `data-tema` e o CSS decide o resto. É um
        /// seam, não uma tela nova: a estrutura da luta (os dois lados, o log, o painel) continua
        /// sendo UMA, e o capítulo só troca cenário, cores e molduras. Assim o 2º tema é CSS, sem
        /// `if` no JS nem cena duplicada — e capítulo sem tema próprio cai no visual padrão sozinho.
        ///
        /// Vazio na Arena: lá não se luta em lugar nenhum, é laboratório.
        /// </summary>
        string Tema = ""
    );

    /// <summary>
    /// Uma vez na fila desenhada. <see cref="Esperou"/> = houve um INTERVALO antes dela (o relógio
    /// teve de andar até alguém cruzar 100). Falso = quem joga aqui já estava pronto e entra na
    /// sequência do anterior, sem espaço pra ninguém se enfiar no meio.
    /// </summary>
    internal record VezVista(int Id, bool Esperou);

    /// <summary>Os modos que o front distingue. String porque é o que atravessa a ponte.</summary>
    internal static class ModoDeBatalha
    {
        public const string Arena = "arena";
        public const string Campanha = "campanha";
    }

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
        // A Velocidade já vem pelo `Combate` — é lá que as camadas (base + itens + buff) vão nascer
        // quando a Bota chegar, e a tela não precisa saber que apareceram. Precisão e Resistência
        // seguem vindo do Personagem: nada no motor as modifica ainda.
        int Velocidade,
        /// <summary>
        /// A barra de turno, de 0 a 100 — e ela PASSA de 100: a sobra é real e carrega, então a tela
        /// tem de saber desenhar acima da linha. Vem crua, sem capar: capar aqui faria três medidores
        /// diferentes virarem três barras iguais, que é exatamente o que o modelo do Raid esconde e
        /// este não quer esconder.
        /// </summary>
        double Medidor,
        int Precisao,
        int Resistencia,
        int TaxaCritPct,
        int DanoCritPct,
        bool Vivo,
        List<StatusVisto> Status,
        /// <summary>
        /// O kit dele, pra LER — de qualquer um no campo, inimigo incluído. Decisão do Gabriel:
        /// esconder o cooldown do inimigo não protege ninguém, porque quem joga bem decora mesmo;
        /// só torna a informação um imposto de memória em vez de uma leitura.
        ///
        /// Vem em TODO combatente e em todo quadro, não só em quem age — é o que permite clicar num
        /// inimigo no meio da luta e ver quantos turnos faltam pro golpe dele voltar. O
        /// <see cref="HabilidadeVista"/> continua existindo à parte porque aquele é pra CLICAR
        /// (índice, disponível, pede alvo) e só faz sentido pra quem está com o turno na mão.
        /// </summary>
        List<HabilidadeDoApostoloVista> Habilidades
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
    ///
    /// <see cref="Marcado"/> != null = a opção é um INTERRUPTOR e este é o estado dele (a tela põe
    /// um ✓ quando ligado). É `bool?` e não `bool` porque a maioria das opções não é interruptor
    /// nenhum — "Campanha" não está ligada nem desligada, e um `false` mudo desenharia um interruptor
    /// apagado em toda linha do menu.
    /// </summary>
    internal record OpcaoMenuVista(string Rotulo, string Icone, bool Habilitado,
        string? Confirmar = null, bool? Marcado = null);

    /// <summary>Um apóstolo na grade de escolha de avatar. `Desbloqueado: false` = aparece em cinza,
    /// não clicável (ainda não conquistado na campanha).
    ///
    /// <see cref="Posicao"/> é a grade 4×4 do perfil de distância dele: <c>Posicao[minhaCasa][casaDoAlvo]</c>,
    /// as duas contadas de 0. Vem PRONTA do C# porque o front não pode ter cópia da tabela — duas
    /// cópias de uma fórmula divergem como duas cópias de um número. É `null` em toda tela que não
    /// põe apóstolo em casa nenhuma (avatar, recompensa, arena).</summary>
    internal record ApostoloVisto(string Simbolo, string Nome, bool Desbloqueado,
        List<List<double>>? Posicao = null);

    /// <summary>
    /// A tela de EDITAR PERFIL: o nome atual (pra pré-preencher), o avatar atual (pra pré-selecionar
    /// na grade) e a lista completa de apóstolos (a ORDEM é o índice que o clique devolve).
    /// </summary>
    internal record EdicaoPerfilVista(string Nome, string Avatar, List<ApostoloVisto> Apostolos);

    /// <summary>
    /// A montagem da Arena que o front devolve: os dois times como ÍNDICES na lista de apóstolos
    /// (mesma ordem do TodosOsApostolos que foi enviada) + quem é bot de cada lado. Vem serializado no
    /// campo Texto da mensagem (a ponte só carrega 1 int/1 string por clique).
    /// </summary>
    internal record ArenaConfig(int[] Time1, int[] Time2, bool Bot1, bool Bot2);

    // ---------- Campanha ----------

    /// <summary>Um nó do mapa = uma facção-capítulo. Índice = posição na lista enviada (Reino..Ascendentes).</summary>
    internal record CapituloVista(string Simbolo, string Nome, bool Desbloqueado, bool Concluido);

    /// <summary>O mapa: as 8 facções em ordem + a posição atual (índice) onde o marcador começa.</summary>
    internal record MapaVista(List<CapituloVista> Capitulos, int Posicao);

    /// <summary>Um item pra mostrar (drop da fase / recompensa).</summary>
    internal record ItemVista(string Simbolo, string Nome, string Stat, string Valor);

    /// <summary>Uma fase: número (1..7), nome (do item), status, inimigos das 2 rodadas e o item que dropa.</summary>
    internal record FaseVista(int Numero, string Nome, bool Desbloqueado, bool Concluido,
        List<ApostoloVisto> Rodada1, List<ApostoloVisto> Rodada2, ItemVista Item);

    /// <summary>
    /// A tela de fases de uma facção: as 7 fases + o pool de apóstolos desbloqueados pra montar o time.
    ///
    /// <see cref="FaseSelecionada"/> e <see cref="TimeMontado"/> são a MEMÓRIA: a tela abre já na
    /// última fase visitada e com o último time nos slots, em vez de exigir que o jogador remonte
    /// tudo a cada visita. O time vem como índices em <see cref="MeusApostolos"/> porque é isso que o
    /// clique devolve; o save guarda identidade (ver CampanhaService.UltimoTime).
    /// </summary>
    internal record FasesVista(string CapituloNome, string CapituloSimbolo, List<FaseVista> Fases,
        List<ApostoloVisto> MeusApostolos, int FaseSelecionada, List<int> TimeMontado);

    /// <summary>Recompensa da vitória: os apóstolos novos desbloqueados + o item dropado (null se já tinha).</summary>
    internal record RecompensaVista(List<ApostoloVisto> Novos, ItemVista? Item);

    /// <summary>
    /// O fim de uma fase — vitória e derrota na MESMA tela, porque a pergunta que vem depois das duas
    /// é a mesma: e agora? Antes eram duas telas que só sabiam dizer "clique pra continuar", e
    /// continuar significava voltar pra lista de fases mesmo quando o jogador só queria tentar de
    /// novo com o mesmo time.
    ///
    /// <see cref="ComOpcoes"/> false = é a passagem da recompensa (o item em destaque, os apóstolos
    /// novos a caminho), que pede um clique e segue pras conquistas; true = é a tela de decisão.
    /// A mesma tela nos dois momentos, pra o jogador não sentir que mudou de lugar.
    ///
    /// <see cref="PodeProxima"/> decide se o botão de continuar existe — falso na derrota (a fase
    /// seguinte não foi liberada) e no fim do último capítulo, sem que a tela precise saber por quê.
    /// <see cref="ProximoECapitulo"/> diz só como CHAMÁ-LO: depois da fase 7 o "continuar" atravessa
    /// pro capítulo seguinte, e prometer "Próxima Fase" quando o que vem é outro capítulo seria
    /// esconder do jogador que ele está mudando de lugar.
    /// </summary>
    internal record FimDeFaseVista(bool Venceu, RecompensaVista? Recompensa, bool PodeProxima,
        bool ProximoECapitulo, bool ComOpcoes);

    /// <summary>
    /// O que o jogador escolheu na tela de fim de fase. Os valores viajam como índice pela ponte, que
    /// carrega um int — então a ORDEM aqui é contrato com o JS.
    /// </summary>
    internal enum DecisaoDeFim
    {
        JogarNovamente = 0,
        EditarEquipe = 1,
        ProximaFase = 2,
        Sair = 3,          // Esc: só o C# produz, o JS nunca manda
    }

    /// <summary>O que o front devolve ao iniciar uma fase: a fase (1..7) e o time como índices dos desbloqueados.</summary>
    internal record FaseConfig(int Fase, int[] Time);

    // ---------- Compêndio ----------

    /// <summary>
    /// Uma habilidade pra LER. Não reusa a <see cref="HabilidadeVista"/> de propósito: aquela existe
    /// pra ser CLICADA (índice, disponível, pede alvo, escopo) e nada disso faz sentido em quem não
    /// está com o turno na mão — metade dos campos viria vazia e alguém, um dia, tentaria preenchê-los.
    ///
    /// Dois clientes, e a diferença entre eles é só o <see cref="CooldownRestante"/>:
    /// - o COMPÊNDIO, onde não há turno correndo, então o restante é 0 e o que importa é a cadência
    ///   DECLARADA (é ela que se compara entre apóstolos);
    /// - o painel da BATALHA, onde clicar em qualquer combatente mostra o kit dele com o cooldown
    ///   andando de verdade.
    ///
    /// <see cref="Passiva"/> separa a passiva das ativas: ela não se usa, e a tela precisa dizer isso
    /// em vez de deixar o jogador procurando o botão.
    /// </summary>
    internal record HabilidadeDoApostoloVista(string Nome, string Simbolo, string Descricao,
        int Cooldown, bool Passiva, int CooldownRestante = 0);

    /// <summary>
    /// O ÚNICO lugar que traduz uma <see cref="Habilidade"/> do domínio pra leitura na tela. Existe
    /// porque os dois clientes (compêndio e painel de batalha) mapeiam os mesmos cinco campos, e
    /// mapeamento duplicado envelhece torto — foi assim que o nome do slot 4 chegou a ser "Acessório"
    /// no front e "Manopla" no back.
    /// </summary>
    internal static class VistaDeHabilidade
    {
        /// <summary>
        /// <paramref name="dono"/> null = fora da luta (compêndio): não há turno correndo, então o
        /// cooldown restante é 0 e o que vale é a cadência declarada. Com dono, o restante é o DELE —
        /// cooldown é do combatente, não do apóstolo: o mesmo Ninja nos dois lados da Arena tem
        /// contagens independentes.
        /// </summary>
        public static HabilidadeDoApostoloVista De(Habilidade h, Combate? dono = null) => new(
            h.Nome, h.Simbolo, h.Descricao, h.Cooldown,
            Passiva: h is HabilidadePassiva,
            CooldownRestante: dono?.Cooldowns.GetValueOrDefault(h)?.CooldownRestante ?? 0);
    }

    /// <summary>
    /// Um apóstolo na grade do compêndio. <see cref="Indice"/> é a posição na lista COMPLETA (a mesma
    /// ordem do TodosOsApostolos) — é o que o clique devolve, e é global às facções justamente pra
    /// não precisar mandar (facção, slot) de volta pela ponte, que carrega um int só.
    /// </summary>
    internal record CompendioApostoloVista(int Indice, string Simbolo, string Nome, bool Desbloqueado);

    /// <summary>Uma facção com os 4 apóstolos dela, na ordem dos slots. É como o catálogo se agrupa.</summary>
    internal record CompendioFaccaoVista(string Nome, string Simbolo, List<CompendioApostoloVista> Apostolos);

    /// <summary>O catálogo inteiro: as 9 facções, cada uma com seus 4 — travados incluídos.</summary>
    internal record CompendioVista(List<CompendioFaccaoVista> Faccoes);

    /// <summary>
    /// A FICHA de um apóstolo. Os números são os de BASE (o que ele é antes de arsenal, buff ou item):
    /// o compêndio é catálogo, não simulador — quem quer saber o efeito do equipamento olha o
    /// Arsenal.
    ///
    /// <see cref="Tipo"/> vem como TEXTO, não como o enum: não há JsonStringEnumConverter na ponte,
    /// então enum viraria número do outro lado.
    ///
    /// <see cref="Desbloqueado"/> só pinta a moldura — a ficha é COMPLETA mesmo travada, porque um
    /// catálogo que esconde o que você ainda não tem não serve pra planejar a campanha.
    /// </summary>
    internal record ApostoloDetalheVista(string Nome, string Simbolo, string Faccao, bool Desbloqueado,
        string Tipo, int Nivel,
        int HP, int Ataque, int Defesa, int Velocidade, int Precisao, int Resistencia,
        int TaxaCritPct, int DanoCritPct,
        List<HabilidadeDoApostoloVista> Habilidades);

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

    /// <summary>
    /// Uma linha do painel de totais: o que o conjunto equipado dá naquele stat, já escrito
    /// (<c>"ATK"</c> / <c>"+240"</c>). Quem soma é o <see cref="ArsenalService.TotaisEquipados"/>;
    /// aqui só chega o número virado texto.
    /// </summary>
    internal record BonusVista(string Stat, string Valor);

    /// <summary>
    /// O arsenal: os 7 slots (equipados), o TOTAL que eles dão e todos os itens obtidos (pra escolher
    /// ao clicar um slot).
    /// </summary>
    internal record ArsenalVista(List<SlotArsenalVista> Slots, List<BonusVista> Totais,
        List<ItemArsenalVista> Obtidos);
}
