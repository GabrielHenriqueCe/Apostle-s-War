# Roadmap da Refatoração — Apostle's War

> **Tipo:** Backlog técnico vivo (índice mestre)
> **Função:** bússola entre sessões. Lista tudo que falta até o fim da
>   refatoração, priorizado. Aponta para os ADRs de cada tema.
> **Como usar:** risque o que concluir, adicione o que descobrir. Cada tema
>   grande ganha seu ADR próprio em docs/ quando for executado.
> **Atualizado:** julho/2026
>
> **Sobre timing:** dívida/melhoria aqui é descrita pela NECESSIDADE, sem data nem
>   "versão"/"2027"/"web". O jogo não tem versões; cada coisa é feita quando for o
>   momento ideal (pode ser já). Não rotular nada por era futura.

---

## ESTADO ATUAL (jul/2026) — FRONT CONCLUÍDO, jogável de ponta a ponta

O porte pro **front webview (WebView2)** está FEITO: o jogo é jogável inteiro na pele nova, que agora
é a **ÚNICA** (`ApostlesWar.App.exe` abre a janela direto — o `--front` sumiu com o console).
Mergeado em sequência de PRs:
- **Menu principal + Perfil** (#172): menu data-driven, perfil do jogador (nome + avatar; o avatar
  libera conforme a campanha desbloqueia champs), criar/editar/excluir conta, "sair" confirmado, modal
  reutilizável.
- **Arena com seleção de time** (#173): monta os 2 times e escolhe o controle de cada lado
  (Você×Bot / Bot×Você / hotseat Você×Você / Bot×Bot); fim de batalha por lado.
- **Campanha** (#174): mapa de facções (trilha + marcador que caminha + pan por arrasto), fases
  (inimigos por rodada + item que dropa), monta o time, luta, vitória/derrota, e save/unlock/drop
  reaproveitando o motor do console.
- **Montagem de time** (#175): clique-na-casa + arrastar-e-soltar (grade→slot substitui, slot→slot
  troca, slot→fora remove) — compartilhado por Arena e Campanha.
- **Arsenal** (#176/#177): equipa itens GLOBAIS nos 7 slots (boneco), com comparação de stat.
- **Console REMOVIDO** (#179): a pele de console morreu — era o plano desde sempre (§Princípios).
  Saíram o projeto `ConsoleUI`, o `GerenciadorDeJogoService` (segunda orquestração da meta), as portas
  `ITelaDeMenu` e `IEntrada` (+`Comando`/`Navegacao`, vocabulário de cursor), as versões sem-time do
  `ExecutarFase`/`ExecutarArena` e o `ResultadoFase.Cancelou`. −1.645 linhas.
- **Camadas ajustadas** (#180): auditoria achou 7 decisões na camada errada e as devolveu ao dono —
  ver §CADA DECISÃO NA SUA CAMADA.
- **Rebalanceamento, 1ª passada** (#189): o primeiro ajuste feito **com número na mão** em vez de
  intuição, lendo a bancada (§BANCADA DE DANO). Dois eixos: **cooldown padronizado em 3** (os de 4
  turnos eram penalidade escondida — a habilidade boa aparecia menos, e a coluna `Usos` denunciava) e
  **multiplicadores de dano subindo de faixa** (a maioria de 1.5–3.0 pra 3.0–4.5, porque o boneco com
  DEF no cap engolia os golpes baixos: eles apareciam como se não existissem nas linhas 2 e 5). 36
  champs tocados. De carona vieram o **bug do hit-all** — o guard do `ResolverAlvos` cobrava a
  semente, e como hit-all não tem pick o `CombateService` manda o próprio atacante de placeholder;
  os 5 revive-de-todos (Robô, Sereia, Anjo, Palhaço, Diabo) explodiam com `InvalidOperationException`
  justamente quando havia alguém pra reviver, e sem mortos o early-return salvava antes (por isso
  passava) — e as **colunas de área** nas duas linhas de champ inteiro da bancada: medir contra 1
  boneco SUBESTIMA o champ de área (os malefícios do Detetive valem +9.504 contra 1 alvo e +30.888
  contra 4; com uma coluna só, o instrumento dizia que ele era médio).
- **Save, compêndio e navegação** (#190): (a) **"excluir conta" virou wipe de verdade** — duas causas
  independentes. A semente versionada (`Save/**` copiado pro build "com tudo liberado") repunha o save
  a cada build, e o wipe só limpava o DISCO enquanto o progresso vivia em memória desde o boot, com o
  `CarregarProgresso` só sobrescrevendo quando a porta devolve não-nulo — então save ausente
  PRESERVAVA o que já estava carregado. Cada service ganhou `Resetar()` e virou dono da PRÓPRIA chave
  (const), com o `CampanhaService.ResetarProgresso()` orquestrando. (b) **Clicar fora desarma a
  habilidade**, igual ao Esc: ela ficava armada E invisível (o painel só desenha pra quem age).
  (c) **Compêndio**: o catálogo dos 36 por facção, travados incluídos e clicáveis — é planejando
  contra o que ainda não se tem que a campanha vira escolha.
- **Casca, arsenal e janela** (#191): `--canto-sair` reserva o vértice superior direito (o botão de
  sair estava POR CIMA dos controles do topo na batalha); um idioma único de barra de rolagem pra
  tudo que rola; lista de itens rolável + **totais do arsenal** somados no `ArsenalService`;
  **tela cheia como PADRÃO** (`ConfiguracaoService`, chave `config`, FORA do wipe do excluir-conta —
  apagar a conta zera o progresso, não a preferência); e o **F5 desarmado**, que matava a partida em
  silêncio (o JS recarregava e a thread do jogo seguia parada no `Take()`).
- **Duração permanente** (#192): `AumentarDuracao` estourava o int quando a duração era
  `int.MaxValue`, e `Expirou` lia o negativo como acabado — o Raio-X do Robô, que promete ESTENDER
  benefícios, APAGAVA o permanente (o Intocável do Fantasma). Nasceu `StatusEffect.Permanente`
  (= `int.MaxValue / 2`): "permanente" precisa aguentar que alguém some turnos nele. **Achado pela
  bancada**, por um número esquisito — o Robô media em múltiplos de 30 entre corridas, que é a
  diferença entre um A1 crítico e um não-crítico ali; o crit cravado do instrumento é um buff
  permanente, e ele se apagava sozinho no 1º Raio-X.
- **Batalha: ler e mandar** (#193): o kit de QUALQUER combatente (inimigo incluído) no painel, com o
  cooldown ao vivo — e no MESMO molde do botão de habilidade, não num card de outra tela; e o **foco
  de alvo no automático** (clica no inimigo, o time converge nele), que entra na frente do abate
  porque ordem explícita vale mais que heurística.
- **Ciclo da campanha** (#194): fase pré-selecionada + último time salvo por IDENTIDADE (facção+slot,
  nunca índice), 🎲 na campanha, **tela única de fim de fase** (Jogar Novamente / Editar Equipe /
  Próxima) com o Esc da batalha virando ENCERRAR (derrota + essa tela), continuação atravessando pro
  capítulo seguinte depois da fase 7 — mas **nunca dando a volta**, porque trocar de dificuldade é
  decisão do jogador —, e a **conquista do champ** animada terminando na ficha dele.
- **Cenário por capítulo** (branch `feature/tema-do-reino`): o `EstadoDeBatalha` carrega um `Tema`, o
  JS põe em `body[data-tema]` e a batalha ganha um lugar. A ESTRUTURA da luta não muda em nada — os
  dois lados, o log, o painel, as animações e os tamanhos seguem os mesmos. O quarto tema (🪬 Folclore)
  trouxe o **maestro**: uma CAUSA COMUM que as peças leem, em vez de cada uma só com o seu relógio.
  Ver §CENÁRIO POR CAPÍTULO.

**Arquitetura (detalhe em §FRONT abaixo):** ponte de mensagens LOCAL in-process (JS↔C# pela webview,
sem HTTP). O **motor da luta ficou INTOCADO** — só as telas trocam, pelos seams `ITelaDeCombate`/
`IControladorDeTurno`/`IApresentacao`/`IRepositorioDeSave`. Padrão consolidado: cada modo entra por um
`Executar...ComTime(s)` (a casca pica o time e chama), e a lógica META (recompensa/save da campanha)
mora na Application (`CampanhaService`), nunca no front.

**REBALANCE (#16): EM ITERAÇÃO, não fechado.** A bancada é o instrumento e a 1ª passada (#189) já
entrou; o trabalho agora é o laço `editar número → dotnet test → git diff docs/bancada-dano.md`,
quantas voltas o Gabriel achar que precisa. Fios de combate ainda abertos: ver §OS FIOS QUE FALTAM
(sweep de composição por facção, turno-resto, passiva-conta-mortos).

> As seções §FRONT §Fatiamento e a FILA B abaixo descreviam o front como PLANO — ficam como registro do
> desenho, com o estado real marcado ✅.

---

## CENÁRIO POR CAPÍTULO (jul/2026) — como um tema é feito

O capítulo em que se luta dá o LUGAR da batalha. `EstadoDeBatalha.Tema` → `body[data-tema]` → CSS +
canvas. Capítulo sem tema luta no visual padrão; a Arena nunca tem tema (é laboratório, não lugar).

**As três camadas, e o que decide em qual entrar:**
1. **CSS** — o céu, a paleta e as texturas que REPETEM. A alavanca que barateia tudo é sobrescrever
   as VARIÁVEIS de cor (`--fundo`/`--painel`/`--borda`/`--apagado`) no escopo do tema: tudo que já
   as usava acompanha de graça, sem tocar em regra nenhuma.
2. **Canvas de FUNDO** (`#particulasFundo`, z 0) — o que é MUNDO: paisagem, névoa, bichos no
   horizonte, os exércitos. Fica atrás dos combatentes.
3. **Canvas da FRENTE** (`#particulas`, z 3) — o que está no ar ENTRE o jogador e o mundo: poeira,
   voadores. É essa separação que dá profundidade.

**Ladrilho ou canvas?** Horizonte que é PAISAGEM (mata, cidade) pode ser ladrilho de CSS, porque
paisagem repete sem mentir. Composição NOMEADA ("castelo no meio", "o reator estourado") tem que ser
canvas: nome não sobrevive a ladrilho — o que é "a borda" muda com a largura da tela — e esticar uma
imagem única resolveria a posição estragando a forma. Vale o mesmo pra quem precisa de ENDEREÇO:
"aquela moita ali", de trás da qual o Oni levanta os chifres, não quer dizer nada num ladrilho que
repete, porque a cada repetição existe uma cópia idêntica dela na tela.

**O MAESTRO — quando as peças precisam de uma CAUSA COMUM** (jul/2026, nasceu no Folclore). Até o
terceiro tema cada peça tinha o seu relógio e não sabia de nenhuma outra; dessincronia era a regra e
continua sendo. A exceção que apareceu: o redemoinho ESCREVE num objeto `vento` (`{ forca, x }`) e
quem quiser LÊ — o fogo verga e chega a apagar, a fumaça inclina, as brasas riscam pro lado, os corvos
se abrem. Mesmo formato pro `fogo` (`{ viva }`), escrito pela fogueira e lido pelas brasas no ar.
Por que isso não é o começo de uma bagunça: é DADO, não chamada — ninguém pergunta nada a ninguém,
ninguém sabe quem mais lê, e a camada que ignora o maestro continua correta (é o que os outros três
temas fazem: sem redemoinho, `forca` fica 0 pra sempre e todas as contas viram `+= 0`). Os dois
objetos nascem SEMPRE, no `iniciarAr`, pra quem lê não precisar de dois caminhos.

O **segundo cliente** chegou nos 🐉 Místicos e é a prova de que o maestro não era um enfeite do
Folclore: lá quem sopra é um redemoinho, aqui é o DRAGÃO na passagem de perto — e quem lê (palmeiras,
vapor da lâmpada, vaga-lumes, pólen) não sabe que a fonte mudou. Foi uma linha de escrita num builder
novo, sem tocar em nada do que já lia.

O **terceiro** (🔱 Decaídos, jul/2026) fecha a pergunta que sobrava: o maestro é um formato, e não um
jeito de falar de VENTO. `inferno { pulso, raizes, escorrendo, tremor, jorro, passagem }` é LUZ — a respiração da lava, escrita pela Árvore do
Mundo e lida pelas rachaduras do chão e pela fumaça. Mesmo
formato, mesma regra (nasce sempre, ninguém pergunta nada a ninguém), e o dado agora é uma grandeza de
outra natureza. Quem escreve tem UMA obrigação, e ela vale pra qualquer maestro futuro: **entregar o
número já CLAMPADO na faixa que prometeu**. Do lado de lá, `pulso` vira alfa e vira largura de traço —
e um valor fora de 0..1 chegaria como cor inválida (silenciosa) ou raio negativo (fatal), longe de
quem o produziu.

E é neste terceiro que o maestro deixa de ser um número e vira uma MESA: seis campos e três donos, com
a regra de que **cada campo tem um escritor só**. A árvore escreve `pulso` e `raizes`; a fenda escreve
`escorrendo`, `tremor` e `jorro`; a coluna de morcegos escreve `passagem`. Ninguém escreve no campo de
ninguém, e é isso que impede a mesa de virar variável global — com dois escritores no mesmo campo, o
último a rodar no quadro ganharia, e a ordem das camadas (que é sobre PROFUNDIDADE) viraria também uma
ordem de precedência de regra. Os dois padrões que apareceram aqui valem pra qualquer maestro futuro:
**o gatilho é um CONTADOR, não um flag** (`passagem` só sobe; quem espera guarda o último valor que
viu — um flag precisaria de alguém pra desligá-lo, e aí duas peças seriam donas do mesmo estado), e
**quem publica geometria publica em PIXEL DE TELA** (as raízes vão prontas pras rachaduras correrem em
cima delas; mandar a fração obrigaria os dois lados a ter cada um a sua cópia da conta do tronco).

**Armadilhas que já custaram tempo aqui:**
- `background-position: bottom` vale `50% 100%` — ancora o ladrilho no CENTRO. O JS que posiciona
  coisas no horizonte conta a partir do x=0; use `left bottom` ou as duas contas discordam, e o erro
  muda com a largura da janela.
- `<canvas>` é elemento SUBSTITUÍDO: `inset: 0` NÃO o estica (o `width: auto` vale o tamanho
  intrínseco, 300×150). Precisa de `width/height: 100%`.
- Ordem de pintura: elemento posicionado pinta depois de elemento em fluxo normal, sempre. Foi por
  isso que o cenário passava por cima do log — a solução foi posicionar o `#meio` (z 4), não mexer
  no z do canvas.
- `destination-out` APAGA pixel. Usado pra "detalhe vazado" num escudo, abre buraco de verdade nele.
- **RAIO NEGATIVO LANÇA, e a exceção mata a cena inteira** (jul/2026, custou o primeiro teste em jogo
  dos Místicos). `arc`/`ellipse` com raio < 0 jogam `IndexSizeError`; como o `requestAnimationFrame`
  só é reagendado no FIM do `quadro`, uma exceção lá dentro para o laço pra sempre. O que se vê não é
  um erro: é a cena congelando no meio, com tudo que seria pintado depois da camada que estourou
  simplesmente ausente — e nada no console do jogo. O padrão que produz isso é sempre o mesmo:
  **filtrar a lista ANTES de avançar o relógio** das partículas, e aí desenhar a que já morreu com
  `vida`/`1 − q` negativo. A ordem certa é avançar, desenhar com o valor CLAMPADO, e só então
  descartar. Espelhar coisa com raio negativo (`r * lado`) é a outra fonte — espelho é deslocamento e
  giro, nunca raio.
- **Girar ~180° inverte os DOIS eixos.** Quem desenha em coordenada local (`translate` + `rotate` no
  ângulo de marcha) sai de cabeça pra baixo quando a criatura atravessa pra esquerda — foi o que pôs
  os chifres do dragão pra baixo e as patas dele no dorso. A correção é um espelho vertical
  (`if (Math.cos(ang) < 0) ctx.scale(1, -1)`), a mesma do golfinho. **Quem parte da NORMAL não sofre
  disso**, porque ela é forçada pra cima — e essa é a divisória: crista, crina e barriga nunca
  quebraram; cabeça, patas e escamas quebraram. Arco medido a partir do ângulo de marcha cai no dorso
  num sentido e na BARRIGA no outro; medido a partir da normal, cai sempre no lugar.
- **Cor tem que ser CSS de verdade.** `strokeStyle = '110, 214, 176'` (a tripla crua que serve dentro
  de um `rgba(...)`) é INVÁLIDO: o navegador ignora a atribuição em silêncio e segue pintando com a
  cor anterior. As duas convenções convivem no mesmo objeto de config — hex pra quem vira
  `fillStyle`/`strokeStyle` direto, tripla pra quem entra num `rgba()` com alfa variável — e trocar
  uma pela outra não avisa nada.
- **Velocidade, nunca duração fixa, pra quem atravessa a tela.** A travessia do dragão era em segundos,
  mas o percurso inclui a largura da janela: em tela mais larga ele passava MAIS RÁPIDO, e o mesmo
  bicho virava outro dependendo do tamanho da janela. Medindo em alturas de arena por segundo, a
  duração se ajusta sozinha. (E a velocidade é POR DISTÂNCIA: o que passa perto atravessa a vista mais
  depressa — é a paralaxe que faz o poste voar e a montanha não.)
- **Medida na unidade do MUNDO faz "a mesma coisa" precisar de números diferentes.** A ondulação do
  dragão era contada contra o percurso e contra a altura da arena — duas coisas que mudam de tamanho
  junto com ele —, então cada distância exigia um valor próprio e as três acabaram ondulando
  DIFERENTE sem ninguém ter decidido isso. Medida no corpo dele (ondas por comprimento, amplitude em
  fração do comprimento de onda), um número só serve às três. Quando a mesma intenção precisa de
  números diferentes em cada contexto, quase sempre a unidade está errada — não o desenho.
- **Evento é lista; estado é variável.** A espuma da praia era "quanto tempo faz que a última onda
  chegou", e como a lavagem dura mais que o intervalo entre ondas, cada chegada REINICIAVA a conta: a
  espuma que ainda recuava sumia de uma vez. Uma lista de lavagens vivas resolve sem ajustar tempo
  nenhum, e as gerações se sobrepõem na areia como a água faz.
- **Subcaminhos de sentidos OPOSTOS se ANULAM onde se sobrepõem** (jul/2026, ⭐ Especial). O canvas
  preenche pela regra `nonzero`: dois subcaminhos do mesmo caminho que giram em sentidos contrários
  têm winding 0 na interseção, e ali fica BURACO. A perna do T-Rex é coxa + canela + pé, com
  sobreposição de propósito nas juntas (é assim que emendam sem fresta); enquanto eram três caminhos
  separados, cada um se preenchia sozinho e o sentido não importava. Ao juntá-los num caminho só —
  pra poder recortar as listras — a coxa, que corria ao contrário das outras duas, abriu um rasgo na
  junta e dava pra ver o cenário através da perna. Quem denuncia é a ÁREA COM SINAL (shoelace) de
  cada subcaminho: se os sinais discordam, há buraco.
- **O sentido do traço pode ser garantido por CONSTRUÇÃO, em vez de conferido depois** (jul/2026, 🔱
  Decaídos). A armadilha dos subcaminhos opostos (acima) apareceu de novo na Árvore do Mundo, e por
  uma razão pior que a do T-Rex: ali eram três peças fixas, aqui são vinte e tantos membros — tronco,
  seis raízes, nove galhos e os garfos deles — em ângulos SORTEADOS, e todos no mesmo caminho, porque
  é ele que é preenchido e depois vira o `clip()` da lava. Conferir a área com sinal de cada um seria
  fazer a mesma conta vinte vezes. O que resolve de uma vez é traçar todo membro do mesmo jeito
  RELATIVO ao sentido de marcha — vai pela margem do lado `+n`, volta pela do lado `−n`, onde `n` é a
  normal da direção. Girar o membro gira a forma inteira junto, então o sentido de rotação do contorno
  não muda com o ângulo, e nenhuma sobreposição pode abrir buraco. Um invariante custa menos que uma
  verificação, e não tem como alguém esquecer de rodá-lo.
- **Quem calcula um ponto SOBRE uma curva tem de repetir a MESMA conta, não uma parecida** (jul/2026).
  Os galhos nascem em `noTronco(u)`, que avalia a quadrática do tronco no ponto — e ela nasceu com o
  ponto de controle em `cx − curva·A` enquanto o `tracarMembro` desenhava `cx + curva·A`. Um sinal. O
  defeito seria MUDO: nada lança, nada fica NaN, a bancada passa — os galhos simplesmente ficariam
  pendurados no ar a até 29px do tronco, longe o bastante pra ver e perto o bastante pra parecer que a
  árvore inteira é que está torta. Duas cópias de uma fórmula divergem como duas cópias de um número.
- **Recortar é mais barato e mais certo que acertar a borda à mão.** As primeiras listras do bicho
  tentavam seguir a silhueta por fora, com uma aproximação da forma — e sobrava listra pendurada pra
  fora em todo lugar em que a conta não batia com o bezier. O padrão que resolveu (`comListras`)
  monta o caminho UMA vez e o usa pras duas coisas: preencher e `clip()`. A listra passa folgada da
  borda e a própria forma apara; as duas nunca podem discordar porque são o MESMO caminho.
- **Num recorte de retângulo + elipse, o retângulo tem de descer até a LINHA DO CENTRO da elipse, e
  não até o topo dela.** O arco de cima sobe em curva: parando no topo (medido no centro), sobra uma
  fresta em meia-lua nas laterais — 14px a 90% do raio, num ralo de 119px. Foi o anel do ralo
  "aparecendo por dentro" do cocô que caía. Do centro pra cima o retângulo cobre tudo sem buraco, e
  do centro pra baixo quem fecha é o próprio arco de baixo, que é o único que tem de tapar algo.
- **`Math.pow(base negativa, expoente fracionário)` é NaN, e NaN em coordenada NÃO LANÇA.** Raio
  negativo num `arc` joga exceção e mata o `requestAnimationFrame` — pelo menos é ruidoso. NaN num
  `moveTo`/`lineTo` simplesmente não desenha nada, em silêncio, e a peça some sem erro em lugar
  nenhum. O gatilho é banal: um perseguidor (`atual + (alvo−atual)·k`) pode devolver 1 + 1 ulp. A
  bancada headless deste tema só validava `arc`/`ellipse` e deixou passar — foi corrigida pra
  validar também os traçadores de caminho.
- **A normal do ponto ZERO de uma fita vem do VIZINHO**, e o vizinho já carrega o movimento. Então a
  base "não sai do lugar" (o desvio lateral vale zero em u=0, por construção) e mesmo assim a CORDA
  dela gira um tantinho pra cada lado, abrindo fresta contra o que estiver atrás. Forçar a normal da
  base num valor fixo resolve; puxar as duas seguintes pra ela evita o bico na emenda.
- **Piso na largura da ponta de uma fita deixa CORTE RETO, não ponta.** O tentáculo do Invasor afina
  com `pow(1−p, .75)` até ZERO; a cauda do T-Rex nasceu com `+ .025` de piso e terminava numa
  tesourada atravessando de um lado ao outro.
- **Renomear uma fase e esquecer UM dos lugares não dá erro nenhum.** `descarga` → `caindo` foi
  trocado no relógio e não na linha que avançava o progresso do cocô: o estado ficou cravado em 0, e
  duas animações inteiras (o alçapão e a queda) simplesmente não aconteciam, sem nada no console.
- **Recorte também erra por EXCESSO.** O cocô parado no chão estava sendo cortado pela boca do ralo,
  porque o recorte da queda valia sempre — e ele é bem mais largo que o ralo (98px contra 77), então
  os cantos de baixo dele sumiam o tempo todo e a peça parecia quebrada. Recorte só enquanto desce.
- **Quando duas peças precisam concordar num número, publique o número PRONTO, em pixel de tela.** O
  banheiro publica as bordas da porta (`livre`, `baixo`) e os sentados só se recortam nelas. Mandar a
  fração da abertura obrigaria os dois lados a ter cada um a sua conta da largura da folha, e elas
  divergiriam no meio do gesto — que é a mesma lição do `--mata-passo` e das corujas.
- **O que passa por um buraco tem de ser MENOR que o buraco.** Óbvio dito assim, e mesmo assim o ralo
  (`.055`) ficou menor que o cocô (`.14`) durante várias rodadas: ele descia por uma boca menor que
  ele. Os dois números andam juntos e isso está escrito na config.
- **Ladrilho que não encosta no vizinho deixa parede nua entre os dois.** As cabines tinham largura
  `.148` e passo `~.16`: sobrava uma tira de azulejo entre cada par e cada cabine desenhava só a
  divisória dela, então a fileira lia como seis caixas soltas. Com largura = passo (1/6 da tela), a
  divisória do meio vira UMA só, partilhada.
- Ladrilho de horizonte em px CRUS não encolhe com a janela — e isso QUEBRA: numa arena baixa o
  ladrilho fica mais alto que ela, o `baseY` (`altura do canvas − altura do ladrilho`) vira NEGATIVO e
  o que devia estar no horizonte sobe pra perto do topo. A saída NÃO é `min()`/`clamp()`/`vh`: o JS lê
  esses valores com `parseFloat(getComputedStyle(...))` e propriedade customizada não é resolvida pra
  px — voltaria a string inteira, o `parseFloat` daria NaN e o padrão de emergência assumiria EM
  SILÊNCIO, ancorando tudo no lugar errado sem quebrar nada. Quem encolhe é uma escada de
  `@media (max-height)` em px crus, no fim da seção de temas do `estilo.css`. **Medida que é FRAÇÃO da
  arena não tem esse problema** — as linhas do mar e da areia dos Místicos são números puros usados
  via `calc(var(--mar-linha) * 1%)`, escalam sozinhas e por isso o tema não entra na escada. Quando
  der pra escolher, a fração é a medida barata; ladrilho é imagem e tem tamanho próprio a defender.

**Motores compartilhados** (extraídos quando apareceu o 2º cliente, não antes):
- `criarNoHorizonte` — coisa presa no ladrilho do horizonte, com relógio OPCIONAL: quem declara
  `aceso` pisca (corujas, bobinas de Tesla), quem não declara está sempre à vista (espantalhos).
- `medirDoTema` / `medirLadrilho` — leem do CSS um número que o tema declarou (o par de um ladrilho,
  ou uma linha solta como `--mar-linha`). Existem pra a armadilha do `parseFloat` (acima) estar
  documentada num lugar só, em vez de inline no meio de uma máquina de fases.
- `criarVoadores` + a tabela `VOADORES` — um motor de voo, quatro bichos (morcego, disco, fantasma,
  corvo). A `forma` é do tema. Quem declara `revoada` voa em BANDO — formação, rumo comum e o susto
  que abre o bando quando o vento passa por baixo; quem não declara atravessa sozinho, como antes.
- `criarPo` / `criarNevoa` — partículas e manchas, com a DIREÇÃO saindo do sinal da velocidade. Três
  campos OPCIONAIS no `criarPo` (`sopro`, `cintila`, `doFogo`) viram a mesma poeira em BRASA da
  fogueira, que é arrastada pelo vento, pisca e morre com a chama. Ausentes, nada muda — é o que
  manteve a poeira do Reino, a cinza do cemitério e o pó da invasão intactos.
- `criarAparicaoNaMoita` — o esqueleto de uma APARIÇÃO com endereço: espera, sorteia uma moita,
  a moita treme, a coisa sobe, faz o GESTO que o tema passou, afunda. Dois clientes: os chifres do
  Oni (vigília, com o pisca dos olhos) e a clava do Troll (o vaivém de quem anda atrás do arbusto).

**Lições de desenho que valeram mais que código:**
- A DISTÂNCIA manda no traço: longe, silhueta quebrada; perto, volume e curva.
- Dessincronia é o que dá vida — corujas, labaredas, lâmpadas e fios de veneno têm cada um o seu
  relógio. Tudo junto denuncia que é um efeito só.
- O que se sorteia é a ESPERA, nunca a duração do gesto: é o que separa "bicho que abre o olho de vez
  em quando" de "lâmpada piscando".
- Detalhe demais na escala errada lê como confusão. Duas tentativas de "melhorar" a caverna com boca
  recortada e estalactites ficaram piores que a silhueta simples com uma luz fraca dentro.
- Mostrar o SINAL, não a figura. Nenhum dos quatro champs do Folclore é desenhado por inteiro em lugar
  nenhum — o que se vê é chifre, clava, bico e carta. Figura pequena com anatomia lê como sujeira;
  chifre lê a 20px.
- O AVISO é a parte barata do susto, e a mais eficaz: a moita treme ANTES de a coisa subir (como a
  terra revirando antes de o caixão abrir), e a subida vira consequência. Sem ele, a figura só aparece.
- Uma fonte de luz SÓ por cena. No Folclore não há lua de propósito: lua e fogueira brigariam pelo
  mesmo trabalho e a que perdesse viraria enfeite. Nos Místicos a mesma regra aparece INVERTIDA: cena
  fria e aberta, com um único ponto morno (a lâmpada) — e é ser a única coisa quente que a põe no
  centro, não a posição dela na tela.
- **Não repetir a assinatura de ninguém.** Cada tema tem uma coisa que é só dele — o dia claro do
  Reino, a lua do cemitério, as estrelas da invasão, o âmbar do fogo —, e a pele nova escolhe o que
  SOBROU antes de escolher o que é bonito. Foi assim que os Místicos viraram crepúsculo, e é a regra
  que manda os vaga-lumes deles morarem numa faixa baixa: soltos no céu, virariam estrelas.
- **A DISTÂNCIA pode virar configuração.** O dragão passa três vezes (perto, médio, longe) e o que
  muda entre elas não é só a escala: é o `detalhe` (silhueta chapada · corpo com volume · o bicho
  inteiro com crina, chifres, bigodes, patas e pérola), a `opacidade`, e a COR — de longe ele é da
  `bruma`, azulada, quase a do céu, porque bicho distante não tem a cor dele, tem a cor do ar que está
  no meio do caminho. Desenhar tudo sempre e só escalar dá sujeira ilegível de longe e um bicho de
  papel de perto. O corpo também ESTICA com a distância (`alongar`): longe, comprido e fino lê melhor.
- **Corpo comprido é FITA, nunca fila de elipses.** A primeira versão do dragão empilhava uma elipse
  por anel; como o raio afina até a ponta e o espaço entre os anéis é constante, a metade de trás
  virava linha pontilhada e o bicho lia como duro e picado. Fita (as duas margens pela normal de cada
  anel, um preenchimento só — a mesma técnica do tentáculo) não tem esse problema em resolução
  nenhuma, e é o que libera alongar o bicho à vontade. A normal é forçada pra CIMA, senão o dorso e a
  barriga trocam de lado quando a onda passa da horizontal.
- **Uma frequência é metrônomo; amplitude uniforme é bloco.** O que fez a ondulação virar cobra foram
  duas coisas juntas: somar DUAS senoides fora de compasso, e fazer a amplitude CRESCER em direção à
  cauda. A raiz do movimento é a cabeça, que quase não sai da linha; quem chicoteia é a ponta.
- **Foco é tirar dos outros, não dar ao alvo.** Na passagem de perto o cenário inteiro escurece um
  pouco atrás do dragão, e o olho vai nele sem que nada precise piscar. O efeito nasce e morre com a
  passagem, então não há estado pra alguém esquecer de desligar.
- **A ordem é dramaturgia; o lado é continuidade.** Ele COMEÇA na frente e some, em vez de chegar aos
  poucos com o auge no fim. A DISTÂNCIA da próxima passagem é sorteada entre as três (só não repete a
  mesma duas vezes seguidas — duas idênticas em fila leem como loop), mas o LADO alterna sempre: ele
  reentra por onde saiu, porque deu meia-volta lá fora. Sortear os dois faria ele sumir à direita e
  reaparecer à esquerda, que é a única coisa capaz de quebrar a ilusão de ser um bicho só.
- **Aura feita da própria silhueta MENTE sobre o tamanho.** O halo do dragão era a fita do corpo 1,9×
  mais larga, translúcida — e ele custou três rodadas de "diminui o tamanho" que não resolviam nada,
  porque o halo encolhia junto e o que o olho media continuava sendo ele. Vale como regra geral: se um
  enfeite escala junto com a peça, ajustar a peça nunca conserta o enfeite. Destacar se faz TIRANDO
  dos outros (o véu), não somando volume ao alvo.
- **Contorno é detalhe de escala pequena.** O fio escuro na barriga tinha espessura proporcional ao
  bicho: correto num dragão de 26px, e uma faixa de 40px de verde num de 536. Quando a peça cresce, o
  contorno cresce junto e deixa de ser contorno pra virar mancha. Ou é espessura fixa, ou não existe.
- **A cabeça usa as cores do CORPO.** Ela tinha degradê próprio e era a única parte com luz própria —
  lia como peça de outro bicho encaixada no pescoço. Crânio no tom do dorso, mandíbula no tom da
  barriga, os mesmos dois do resto.
- **A junta cabeça-pescoço é uma CONTA, não um ajuste a olho.** O tamanho da cabeça e a altura da nuca
  andam amarrados (nuca = 1/k do tamanho): assim a nuca vale exatamente o raio do primeiro anel e o
  encaixe é exato. E o pescoço tem que ficar FINO enquanto o crânio o cobre — o corpo engrossava cedo
  demais e ultrapassava a nuca antes de a cabeça acabar, que era o degrau que aparecia atrás do rosto.
- **Folha é SUPERFÍCIE, não fio.** As da palmeira eram nervura + dois riscos por folíolo, e a copa lia
  como um punhado de arames. Lâmina preenchida e serrilhada nas bordas resolve — o serrilhado dá os
  folíolos sem desenhar um a um, porque o que conta é a silhueta e não a contagem.
- **Moldura de cena fica no FUNDO.** As duas tentativas de moldura em primeiro plano (a gruta e o
  pórtico do #197) morreram: coisa grande e perto obriga a acertar o traço, e traço errado em cima da
  luta é pior que cenário nenhum. As palmeiras dos Místicos emolduram de trás dos combatentes — e são
  canvas, e não o `::before`/`::after` do CSS, porque precisam VERGAR quando o dragão passa. Foi o
  cenário pedindo a camada, e não a camada procurando serviço.

- **O ladrilho decide quanto CHÃO a cena tem pra acontecer** (jul/2026, 🔱 Decaídos). A vila élfica
  pousava os pés a 78% da altura do próprio ladrilho, que é onde uma mata costuma encostar no chão — e
  a conta fechava: pé da árvore a 92% da faixa, vila a 78%, tudo dentro. Só que a faixa entre as duas
  linhas é o ÚNICO chão da cena, e é nela que as veias de lava correm: sobravam 17px de profundidade
  contra 1100 de alcance, e as rachaduras saíam quase horizontais, sem perspectiva nenhuma. Subir a
  linha do ladrilho pra 55% resolveu — metade dele virou terra nua, que é justamente onde a luz e as
  rachaduras moram. **A altura de um ladrilho de horizonte não é só o tamanho do desenho: é o quanto
  de piso sobra embaixo dele.** E foi só a cena montada, com os números impressos em pixel, que
  mostrou isso — nenhuma das duas medidas está errada sozinha.
- **Dois champs podem partilhar o BICHO, desde que não partilhem o COMPORTAMENTO** (jul/2026). 🦇
  Morcego e 🧛 Vampiro são o mesmo tema, e desenhar dois morcegos diferentes seria repetição. A saída
  não foi mostrar o sinal de cada um (não há sinal desenhável de vampiro que não seja figura — capa,
  caixão, dentes), foi dar o MESMO bicho a ambos com gestos que não se confundem: os do Morcego
  ATRAVESSAM a tela em bando, os do Vampiro se JUNTAM numa coluna sobre a árvore, seguram e estouram.
  Bando é bicho; coluna é alguém mandando neles. É o contraste-dentro-do-padrão da sereia entre os
  golfinhos, aplicado ao movimento em vez de à forma — e por isso a coluna PRECISA dos morcegos
  comuns existindo antes: sozinha, ela seria só um efeito.
- **Peça central grande fora do centro lê como "uma peça grande num canto"** (jul/2026, 🔱 Decaídos).
  A Árvore do Mundo nasceu em `x: .66`, alta e estreita, e o veredito do Gabriel foi "ficou bem mais ou
  menos": ela era do tamanho certo e mesmo assim não era a cena. O que consertou foram três coisas
  juntas, e nenhuma delas é "aumentar": ir pro MEIO (`x: .5`), engrossar o tronco, e principalmente
  virar o crescimento pros LADOS — galhos quase horizontais e raízes deitadas, ocupando ~60% da
  largura da tela em vez de subir. Uma peça alta e fina divide a tela em dois lados; uma peça larga e
  centrada É a tela, e o resto vira o fundo dela. A lâmpada dos Místicos não precisa disso porque ela
  manda pela LUZ (a única coisa quente da praia) — quando a peça manda pelo TAMANHO, ela tem de estar
  no meio, senão o olho a lê como um objeto do cenário e não como o assunto.
- **A dramaturgia mais barata é a PAUSA antes do susto** (jul/2026, ideia do Gabriel). O ciclo do
  Inferno é: os morcegos passam → a lava PARA de escorrer → um momento de nada → o chão treme → a
  terra se abre e joga lava nova na árvore → treme de novo → fecha. Tirando a pausa, sobra uma fenda
  que abre de vez em quando; com ela, a cena parece ter ACABADO, e é isso que faz o abrir ser "do
  nada". É o mesmo princípio do AVISO (a moita treme antes de o Oni subir), aplicado do lado de fora:
  o aviso prepara, a pausa DESPREPARA, e as duas juntas custam duas fases numa máquina de estados.
  Detalhe que fez a pausa funcionar: a lava não some de uma vez — cada fio termina a descida dele e só
  então deixa de recomeçar, porque a decisão de repetir mora na pausa seca do ciclo de cada um.
- **O que TREMULA não pode morar no ladrilho** (jul/2026). O Gabriel pediu a vila com "pontes e escadas
  de corda, algumas caídas outras QUEIMANDO", e o queimando não cabe numa imagem parada. Os focos são
  canvas, plantados pelo `criarNoHorizonte` em cima das coordenadas do próprio SVG — e aqui eles
  REPETEM por ladrilho de propósito, ao contrário de tudo o que este manual manda pôr em canvas: o
  incêndio não é "aquela casa ali", é "as casas que estão queimando", e o desenho que repete já traz
  uma cópia de cada uma. **Canvas não quer dizer endereço; quer dizer que se mexe.** Eram duas
  perguntas que este manual vinha tratando como uma só.
- **Duas peças que fazem a mesma coisa é uma peça contada duas vezes** (jul/2026, 🔱 Decaídos). A cena
  tinha a poça de lava ao pé da árvore (onde os escorridos caíam) e, alguns metros à frente, uma fenda
  que abria e fechava — duas bocas de inferno na mesma terra, e nenhuma explicando a outra. O Gabriel
  desfez isso numa frase: *"a verdade é que isso aí já É o buraco na terra, não precisávamos criar
  outro, era só acrescentar a erupção direto ali"*. A poça desceu pro lugar da fenda, a fenda morreu, e
  o ciclo inteiro (secar → tremer → escancarar → cuspir → fechar) passou a acontecer no buraco que
  SEMPRE esteve lá — o que ainda deu de graça a ligação que faltava: a lava da árvore agora cai
  DENTRO de alguma coisa (um vertedouro curto do pé do tronco até a boca), em vez de evaporar ao
  encostar na terra. **Antes de animar uma peça nova, perguntar se a cena já não tem uma que só
  precisava do gesto.**
- **Luz que se espalha no CHÃO chapa a terra; luz que sobe dá volume** (jul/2026). Havia duas elipses
  de clarão rente ao solo — uma ao pé da árvore, outra em volta da fenda —, e o pedido foi "remove
  aquela luz que você joga lateralmente, deixa só o fogo pra cima". As duas saíram e no lugar entrou
  uma coluna que nasce larga na boca do buraco e se estreita subindo. A leitura melhora por dois
  motivos: o chão volta a ter textura (elipse translúcida por cima de tudo apaga rachadura, raiz e
  terra de uma vez) e a fonte fica ONDE ELA ESTÁ — luz espalhada no piso não diz de onde veio, coluna
  diz. Vale como regra: fogo tem direção, e desenhar o clarão dele como mancha simétrica é desperdiçar
  a única informação que a peça tem pra dar.
- **Onde a peça grande fica define ONDE SOBRA CENA** (jul/2026, 🔱 Decaídos). A árvore assentada
  exatamente na linha das casas parecia certo e escondia o melhor evento do tema: o terremoto abria no
  pé dela, atrás do tronco e das raízes, que é o único lugar da cena inteira onde ele não podia ser
  visto. O Gabriel resolveu com uma frase — *"era pra árvore ficar mais em cima pro terremoto vir mais
  embaixo e dar pra ver"* — e o que ela descreve não é uma correção de posição, é a **abertura de uma
  faixa de palco**: recuar a árvore 16% da faixa do ladrilho cria ~130px de terra na frente dela, e é
  ali que a fenda passa a rasgar, à vista de todo mundo. Vale como pergunta de composição: cada peça
  grande tem de deixar um lugar VAZIO onde o que acontece possa acontecer.
  E o recuo tem um teto que a arquitetura impõe: a vila é ladrilho do CSS e pinta SEMPRE atrás do
  canvas, então uma árvore muito recuada continuaria sendo desenhada por cima das casas que deveriam
  estar na frente dela. Curto funciona, longo mentiria — e saber por que o limite existe é o que
  impede alguém de "melhorar" isso depois.
- **Peça que serpenteia é LISTA DE NÓS, não arco** (jul/2026, 🔱 Decaídos). As raízes eram um arco
  quadrático cada uma (o `tracarMembro`, o mesmo do tronco e dos galhos), e o pedido — *"saindo de
  BAIXO da árvore, fazendo curvas com emaranhados até chegar nos cantos, umas 3 ou 4 pra cada lado se
  emaranhando em curvas bruscas"* — não cabe num arco: arco tem uma curvatura só, e "brusca" quer
  dizer que a direção MUDA várias vezes. Viraram caminhos de 8 nós, com o desvio de cada nó sorteado
  em cheio (senoide não serve: ela dá ondulação regular, que é o oposto de curva brusca) e com uma
  PROFUNDIDADE própria por raiz — é a profundidade que faz uma passar por cima da outra e o emaranhado
  aparecer, e não a amplitude, porque a faixa de terra tem só ~100px de altura. E como o caminho agora
  é sorteado, publicá-lo no maestro deixou de ser economia e virou obrigatório: refazer o mesmo
  emaranhado do lado das rachaduras seriam duas cópias de um gerador aleatório, que nem com a mesma
  semente dariam o mesmo desenho.
- **Uma peça que "vai até a borda" tem de PASSAR da borda** (jul/2026). Com `alcance` sorteado em
  `[.88, 1.22]` da meia-largura, metade das raízes morria dentro da tela — e raiz que acaba um palmo
  antes do canto não lê como raiz longa, lê como raiz cortada. A faixa virou `[1.04, 1.4]`: todas
  passam, cada uma por um tanto diferente. O que se sorteia é o QUANTO ela sai de cena, nunca SE ela
  sai. (Foi um script que rodou o builder de verdade e mediu o que ele publica que mostrou isso — a
  bancada estava verde, porque nada ali estava errado; estava só feio.)
- **Quando a intenção é chegar na BORDA, a unidade é a borda** (jul/2026). As raízes eram medidas em
  fração da altura da árvore, e o pedido virou "as raízes se esticando pela terra até os cantos do
  mapa" — que é uma medida da TELA. Em fração da árvore, o mesmo número dava raiz curta em tela larga e
  raiz passando do canto em tela estreita; em fração da MEIA-LARGURA (`1` = chega na borda, e algumas
  passam de 1 de propósito, porque raiz que morre um palmo antes do canto lê como raiz cortada), um
  número só serve a todas as janelas. É a mesma lição da ondulação do dragão, do outro lado: lá a
  unidade certa era o corpo do bicho; aqui é a tela. **A pergunta que resolve as duas é "essa medida é
  sobre o quê?"** — e a resposta quase nunca é "sobre o objeto que está desenhando".
- **DUAS LINHAS DE CHÃO na mesma cena é a coisa que o olho pega primeiro** (jul/2026). A árvore ficava
  plantada 37% de faixa ABAIXO dos pés da vila — deliberadamente, pra ser a coisa mais perto —, e o
  veredito foi "como o chão e a árvore estão no mesmo lugar está estranho, coloca a árvore no nível
  das casinhas". Ele está certo: paralaxe se conta por TAMANHO e por COR (o que está longe é menor e
  tem a cor do ar), não por altura na tela; duas alturas de chão diferentes não leem como "uma perto e
  outra longe", leem como duas cenas coladas. A correção apagou um número em vez de acrescentar um: a
  árvore perdeu o `assentada` e passou a usar a linha que o `--vila-chao` declara, que já existia. **A
  segunda cópia de uma medida quase nunca se justifica por "mas esta é diferente".**
- **A bancada headless erra por EXCESSO também, e o falso positivo custa tempo igual** (jul/2026). O
  validador de cor deste tema reprovou 900 mil `rgba(26, 14, 14, 3.9e-17)` — notação científica, que é
  CSS **válido** (Syntax 3 tem expoente) e aparece sozinha quando um alfa sai de um seno perto do
  zero. Um instrumento que grita onde não há defeito ensina a ignorá-lo, que é o pior estado possível
  pra uma bancada. O que ela tem de pegar continua sendo a TRIPLA CRUA (`'110, 214, 176'`) indo parar
  num `fillStyle`. E vale checar o contrário também: um autoteste que planta os dois defeitos de
  propósito e confere que ela reprova, mais um censo de fases — "passou" pode querer dizer "a coluna
  de morcegos nunca chegou a estourar em 900s".
- **A conta de uma composição empilhada tem de fechar em PIXEL, e ela não avisa quando não fecha**
  (ago/2026). Portal + rosácea + lancetas na mesma nave: 83 + 118 + 34px numa faixa de 243. A lanceta
  saía POR DENTRO do vitral e nada quebrava — não lança, não vira NaN, a bancada fica verde, e em jogo
  parece só um vitral sujo. Quem denuncia é somar os números à mão antes de rodar. A lição é que o
  lugar de uma peça pequena é onde SOBRA espaço, não onde ela seria mais bonita.
- **Sentinela de ocioso nunca é zero** (ago/2026). Uma peça que espera `Math.random() * k` antes de
  agir não pode usar `espera = 0` pra dizer "não estou esperando nada": um sorteio que caísse
  exatamente em 0 a deixaria muda PRA SEMPRE — raro demais pra aparecer num teste e permanente quando
  aparecesse. `Infinity` serve (`Infinity − dt` segue infinito) e mantém o estado num campo só, sem um
  segundo booleano pra alguém esquecer de virar.

**Peles prontas:** 👑 Reino (cidade murada, de DIA — o único claro, e é o contraste dele que faz os
outros parecerem escuros de propósito), 🌑 Lado Sombrio (cemitério sob a lua), ⚙️ Tecnológicos (a
noite da invasão), 🪬 Folclore (a clareira com a fogueira, na noite QUENTE — o âmbar era a paleta que
sobrava, e cai bem porque aqui a fonte de luz é fogo), 🐉 Místicos (a praia no CREPÚSCULO, com a
lâmpada na areia e o dragão dando as voltas dele), ⭐ Especial (o banheiro público — o primeiro
INTERIOR), 🔱 Decaídos (a vila élfica vendida, com a Árvore do Mundo no meio escorrendo lava, e o
Inferno se abrindo no chão de tempos em tempos) e ✝️ Apóstolos (a SALA de Natal — o segundo interior,
com a árvore num canto, a lareira no outro e a nevasca vista por uma janela no meio). Falta 1
facção (Humanos) — e ela está **bloqueada de propósito**, pela seção logo abaixo. Barata ela segue
sendo (um bloco de CSS mais um punhado de configuração); o que falta não é esforço, é ONDE ela
aparece.

#### A ARENA vai ter CENÁRIO PRÓPRIO (decisão do Gabriel, ago/2026)

Hoje ela não tem, e não tem **de propósito**: `FluxoDoFront.cs:255` faz `_sessao.Tema = ""` com o
comentário *"laboratório não tem cenário"*. Essa decisão está revogada — a Arena ganha uma pele dela.

**O que isso muda de arquitetura, e é mais do que parece.** Até aqui a chave do tema SEMPRE foi uma
facção: `faccao.ToString().ToLowerInvariant()` → `body[data-tema]`. O vocabulário de temas era o enum
`Faccao`, e por isso "tema" e "capítulo" vinham sendo a mesma palavra. A Arena é o **primeiro tema que
não é facção nenhuma** — o que aposenta essa equivalência e deixa a chave ser só um NOME. Junto com o
fundo de facção no compêndio (logo abaixo), é o segundo sinal de que o tema pertence à CENA, e não ao
capítulo em que se luta.

**O custo continua sendo quase nada:** um `body[data-tema="arena"]` no CSS, uma entrada `arena` no
`AR_DO_TEMA`, e **uma linha de C#** — aquele `""` vira `"arena"`. Nenhuma estrutura nova.

**O que fica em aberto (é desenho, e é do Gabriel):** que cena é essa. A regra da assinatura vale
igual — não sobra HORA nenhuma, então a resposta é um LUGAR, no caminho do ⭐ Especial e dos
✝️ Apóstolos. E há uma pista boa no que a Arena É: ela não é um capítulo da história, é onde se
experimenta time contra time. O antigo comentário chamava isso de *laboratório*; vale decidir se a
cena abraça essa ideia (um lugar de treino/torneio, fora do mundo da campanha) ou se contradiz.

**Ordem:** cabe antes ou depois da separação do `jogo.js` — mas depois é mais barato, porque aí ela
nasce direto como `cenarios/arena/` em vez de virar mais um trecho no monolito.

#### Os HUMANOS ficam por último DE PROPÓSITO — e quem vai cobrar a pele é o COMPÊNDIO (ago/2026)

Decisão do Gabriel, com o motivo e a hora. **Não há capítulo dos Humanos** — a campanha começa no
👑 Reino —, e **está certo assim**: a facção do jogador não é fase pra vencer. Como o tema hoje só é
pedido pela BATALHA (`EstadoDeBatalha.Tema`, que nasce do capítulo em que se luta), uma pele dos
Humanos não teria hoje NENHUM lugar onde aparecer. Fazer agora seria desenhar uma cena que ninguém vê.

**O que muda isso, e é o pedido dele:** no **COMPÊNDIO**, clicar num personagem passa a mostrar, ao
fundo, **o cenário da facção/capítulo DELE**. Aí o tema deixa de ser propriedade da batalha e passa a
ser propriedade da FACÇÃO — e a facção Humanos existe (são 4 champs), com capítulo ou sem. É esse
recurso que cria a vaga para a 9ª pele, e é por ele que ela vai ser cobrada.

**A ordem é essa, e não é gosto — é dependência:**
1. **Separar o `jogo.js`** (a §DÍVIDA ANOTADA logo abaixo). É ela que faz "um cenário" virar coisa que
   se pede por nome em vez de um trecho no meio de 11.920 linhas.
2. **O fundo de facção no compêndio**, que consome a separação: o compêndio vai querer montar o ar de
   um tema fora da batalha, e hoje quem faz isso é o `aplicarTema` chamado de dentro do fluxo de luta.
3. **A pele dos Humanos**, por último, quando já existir a tela que a mostra.

Fazer os Humanos ANTES seria pagar a pele duas vezes: ela nasceria no arquivo monolítico que o passo 1
vai picar, e nasceria sem a tela que decide o enquadramento dela. **E o enquadramento é justamente o
que está em aberto** — não sobra hora nenhuma (dia é do Reino, lua do cemitério, estrelas da invasão,
âmbar do Folclore, crepúsculo da praia, luz-de-baixo do Inferno), então a resposta tem de vir de um
LUGAR ou de um recorte, no caminho do ⭐ Especial e dos ✝️ Apóstolos. Ver a cena no compêndio primeiro
provavelmente responde isso sozinho.

**O que os ✝️ Apóstolos ensinaram**, além do que subiu nas listas acima. Esta pele custou TRÊS versões
inteiras, e as duas que morreram ensinaram mais que a que ficou:

- **1ª, a CATEDRAL na nevasca** — fachada em silhueta, rosácea acesa jogando luz colorida na neve, e um
  SINO regendo a cena inteira num maestro de som. Veredito em jogo: *"o sino ficou ruim, a ideia da
  catedral também"*. O argumento era bom no papel e a peça estava errada assim mesmo: prédio no meio da
  neve lê como arquitetura, não como Natal. Ela deixou duas coisas que sobreviveram a tudo — a
  ILUMINAÇÃO colorida no chão e o céu fechado — e um achado técnico que vale guardar mesmo com a peça
  morta: **luz que ATRAVESSA se desenha ao contrário de luz que EMITE**. Os feixes da rosácea tinham de
  ser pintados ANTES da fachada, porque uma janela acesa não ilumina a parede em que ela está — ela joga
  luz pra FORA. Toda luz anterior deste front emite (fogueira, lâmpada, lava, fluorescente) e por isso
  sempre pôde ser a última camada; a primeira que atravessa um vidro inverte a ordem de pintura.
- **2ª, a ÁRVORE DE NATAL sozinha na nevasca** — *"ficou vazio"*. Uma árvore acesa num descampado não
  tem com o que conversar: a cena vira um objeto e um fundo, e nenhum dos dois explica o outro.
- **3ª, a SALA** — e o que consertou foi o Gabriel virar a coisa do avesso: em vez de pôr o Natal no
  meio da neve, pôr a neve do lado de FORA de uma janela. A paisagem das duas versões mortas continua
  ali, inteira; ela só passou a ser vista por um buraco na parede, e a casa é o que finalmente dá
  assunto à árvore.

- **A assinatura desta pele é um RECORTE.** Com sete peles prontas não sobrava hora do dia nenhuma, e o
  que a distingue não é uma luz nem um lugar: é ser a única em que a paisagem aparece por um vão. Quando
  a lista do que sobrou acabar, o próximo lugar pra procurar não é uma hora — é um ENQUADRAMENTO.
- **A paisagem numa janela é um RECORTE, não uma miniatura.** A primeira versão da vista media a mata, a
  neve, os bonecos e o trenó em fração da JANELA: proporção impecável e leitura errada, porque encolher
  a cena inteira faz o vidro virar um quadro pendurado na parede. Olhar por uma janela não diminui o
  mundo, mostra MENOS dele. Tudo lá dentro é medido na altura da ARENA, igual à pele externa, e o que o
  vidro faz é cortar.
- **Um ROTEIRO que atravessa peças distantes pede um MAESTRO, não uma máquina de estados.** O ciclo do
  Inferno coube dentro da fenda porque todas as fases eram DELA. Aqui a noite passa pela árvore num
  canto, pela janela no meio, pela lareira no outro canto e pelos presentes no chão — e nenhuma delas é
  o lugar certo de guardar a história. O `criarRoteiroDaNoite` é a primeira camada do front que **não
  desenha nada**: ela só escreve o `natal`, e as quatro peças leem. Vindo primeiro na lista, ela também
  garante que todas leiam o mesmo instante no mesmo quadro.
- **Aviso é um PASSO, nunca a cauda de outro.** O tremor da lareira nasceu amarrado à fração final dos
  passos vizinhos, e virou ridículo assim que um deles passou a ter duração VARIÁVEL (o `entalado`
  espera o presente chegar): o "fim" dele durava os três segundos inteiros da espera, e a lareira tremia
  sem parar. Aviso é acontecimento curto — grudá-lo no fim de um passo é pedir que ele dure o que o
  outro durar.
- **Dependência entre peças se DIZ, não se calibra.** O Papai Noel só sobe depois que o presente chega,
  e isso não é "põe o `entalado` maior que o `entregar`": o passo consulta a entrega e espera. Assim
  mexer na duração da viagem do presente não obriga ninguém a lembrar de mexer na outra. Mesma regra no
  `subir`, que não tem duração própria — ele usa a de `descer`, porque o percurso é o mesmo e ele sobe
  na velocidade com que caiu. **Dois números iguais numa config são dois números que um dia vão ficar
  diferentes sem ninguém perceber.**
- **Corpo humano em canvas se resolve ESCOLHENDO O PEDAÇO.** O 🦸 e o 🦹 do ⭐ Especial mostram só a
  canela e o pé atrás do jornal; o 🎅 aqui aparece só da cintura pra baixo, entalado na boca da lareira.
  Nos dois casos a saída não foi desenhar melhor, foi enquadrar. E o que faz a roupa ler não é a forma,
  é a ORDEM: casaco, cinto POR CIMA dele, terminação do casaco embaixo do cinto, e só então as pernas.
- **Peça que entra por um vão se recorta no PRÓPRIO vão, e o recorte acaba onde o vão acaba.** O 🎅 é
  cortado pelo mesmo caminho que preenche a boca da lareira (o padrão do `comListras`: monta uma vez,
  usa pra preencher E pra recortar) — com um retângulo, ele passava por cima da pedra nas quinas do
  arco. Mas o recorte ganha um segundo subcaminho largo abaixo da soleira: as paredes só mandam nele
  enquanto ele está DENTRO delas, e sem isso as botas eram ceifadas por uma linha invisível no meio do
  piso. Os dois subcaminhos correm no mesmo sentido de propósito — em sentidos contrários se anulariam e
  abririam buraco bem na emenda.
- **Tamanho de peça se lê CONTRA o vizinho, nunca sozinho.** Os bonecos nasceram em `.085` da altura da
  arena, que parecia razoável no papel — e davam 79 a 119px numa mata de pinheiros de 85px. Um boneco de
  neve do tamanho de uma árvore inteira: nada quebra, a bancada fica verde, e o que se perde é a
  profundidade, que é a única coisa que um horizonte existe pra dar. A mesma coisa mordeu o casaco do
  🎅 (mais largo que alto, um bloco deitado que não lê como corpo) e as máscaras. Foi o script que roda
  os builders e IMPRIME as caixas em pixel que mostrou os três — **a bancada headless vê ERRO; este vê
  feiura, e os dois são obrigatórios.**
- **Canto de cena se faz por REPETIÇÃO, não por uma linha bem desenhada.** Duas tentativas de amarrar os
  enfeites do canto numa trepadeira única falharam (uma diagonal, depois um S) antes de eu ir ver como
  os outros sete são feitos: as algas dos 🐉 Místicos são TRÊS fitas saindo da borda de cima, os galhos
  dos 🔱 Decaídos são DOIS ramos em pesos de traço diferentes. Nenhum é uma linha só. Uma mecha sozinha
  é um risco; quatro mechas são uma moita entrando pela quina — e nenhuma quantidade de capricho numa
  linha só substitui isso.

**O que os 🔱 Decaídos ensinaram**, além do que subiu nas listas acima: a assinatura foi a última que
ainda estava inteira, e ela não é uma HORA nem um lugar — é a DIREÇÃO da luz. Nos seis anteriores a
luz desce (sol, lua, estrelas, fluorescente) ou fica na altura do chão (a fogueira); aqui ela sai de
dentro da terra. Isso inverte todas as sombras da cena de graça, e prova que o critério "o que sobrou"
não precisa ser sempre um horário do dia — quando os horários acabaram, o que sobra é a física.

A HISTÓRIA veio antes da composição e é o que amarrou as peças, e isso foi ideia do Gabriel: o 🧝 Elfo
vendeu a vila élfica aos demônios. Com ela, nenhuma peça precisa se justificar sozinha — as fendas são
por onde eles subiram, a Árvore está corrompida porque foi por dentro dela que vieram, os 🦇 são os que
chegaram depois. As cinco peças anteriores
eram bonitas e independentes; esta é a primeira cena do front em que uma coisa explica a outra. **Um
cenário com enredo se desenha sozinho** — e a segunda rodada provou isso de novo, porque as correções
dele foram todas de COERÊNCIA, não de gosto: as rachaduras deixaram de ser riscos soltos no chão e
passaram a correr POR CIMA DAS RAÍZES (o que queima por dentro é a árvore, então a terra só racha onde
ela chegou); a lava passou a descer no modelo do veneno da ruína dos ⚙️ Tecnológicos, de vários pontos
e com fios que se sobrepõem (líquido tem física, e ela já estava escrita num builder vizinho); e a
vila virou casas ENTRE as árvores, ligadas por pontes e escadas de corda — porque uma vila élfica não
é um horizonte, é um lugar onde alguém morava. O corolário é a lista de coisas que a história PROÍBE:
sem corpos (figura humana pequena em canvas é sujeira, e caída é pior), sem chama GRANDE — o âmbar
tremulante é do Folclore, então o incêndio aqui já passou: sobrou carvão, cinza CAINDO e uns poucos
focos pequenos ainda ardendo na vila, longe e do tamanho de uma casa de 16px (foi o Gabriel que os
pediu, e eles cabem porque são distantes: o que o Folclore tem é uma fogueira PERTO, que é a fonte de
luz da cena inteira — aqui a fonte é a lava, e o fogo lá atrás é rastro) — e sem uma segunda fonte de
luz perto, que brigaria com a árvore.

**E uma peça inteira MORREU nesta pele, o que também é registro:** a casa do traidor, a única de pé no
meio da ruína, era o 🧝 Elfo contado por uma silhueta que não combinava com as vizinhas — o truque da
sereia entre os golfinhos. Em jogo o Gabriel mandou tirar, e a cena não perdeu nada: a **Árvore do
Mundo é literalmente a habilidade dele**, e depois que ela foi pro meio e virou o assunto da tela, a
casa era um segundo jeito de dizer a mesma coisa, num canto, pequeno. **Quando a peça grande passa a
carregar o champ, a peça pequena que o carregava vira ruído** — e a economia certa é apagar, não
realocar.

**O que os 🐉 Místicos ensinaram** (jul/2026), além do que já subiu nas listas acima: três dos quatro
champs da facção têm CORPO HUMANO (gênio, sereia, fada), e figura humana pequena em canvas fica
esquisita — o Ninja é a única do front inteiro e só passa porque é preta, distante e em movimento. A
regra "mostrar o sinal, não a figura" deixou de ser economia e virou a única saída: o gênio é a
LÂMPADA e o vapor que sai dela, a sereia é uma CAUDA que rompe a água no meio dos golfinhos, e a fada
é o vaga-lume que é maior que os outros e deixa rastro. Nenhum dos três foi desenhado. E o contraste é
o que faz a sereia funcionar: os golfinhos precisam existir ANTES dela — coisa diferente no meio de um
padrão estabelecido lê como acontecimento; sozinha, ela seria só um desenho.

Corolário que só apareceu em jogo: **tirar o corpo também tira os gestos que dependiam dele.** A
sereia começou fazendo o mesmo salto do golfinho e ficou errada na hora (o Gabriel: "ela tá pulando
sem corpo") — sem torso pra explicar o impulso, o arco denuncia o que falta e a cauda lê como pedaço
solto sendo arremessado. O gesto certo é o que a cauda consegue justificar sozinha: a ponta rompe a
água, abana e afunda no mesmo lugar, com o que está abaixo da linha d'água RECORTADO (é o recorte que
dá superfície ao mar). Quando se mostra só o sinal, o movimento tem que ser do sinal — não do corpo
que não está lá.

E o RECORTE na linha d'água acabou valendo pros dois: o golfinho também começa e termina o arco
ABAIXO da superfície, e sai da água em partes — focinho, dorso, cauda — em vez de aparecer inteiro do
nada em cima dela. O respingo passou a sair do CRUZAMENTO da linha, e não do começo e do fim do
relógio do salto: é quase a mesma coisa, e erra justamente onde se está olhando.

Uma técnica que vale reaproveitar: **o corpo do dragão é a cabeça no PASSADO**. Cada anel avalia a
mesma curva num ponto anterior do percurso (`progresso − i * passo`), então a ondulação viaja da
cabeça pra cauda sozinha — sem histórico de posições, sem buffer, e sem depender do `dt` (guardar
posição por quadro quebra quando o framerate varia). É a onda que desce no tentáculo, aplicada a um
corpo que anda.

E o **mar** virou a terceira peça-tipo do front, ao lado do ladrilho de horizonte e da aparição: três
camadas com trabalhos que não se substituem — as ILHAS dão profundidade (são a única referência de
tamanho no fundo), as ONDAS dão movimento (rolando do horizonte pro raso com o avanço em `u²`, que é
perspectiva de graça num expoente), e a ESPUMA dá a BEIRA, que costura o mar ao chão onde a luta
acontece. Sem a terceira, a praia são dois retângulos empilhados. A espuma é DISPARADA pela onda que
encosta — mais um caso de "consequência, não coincidência", como a fogueira que o redemoinho apaga.

**O Folclore foi e VOLTOU**, e a ida e volta é o registro que interessa. A primeira versão (a vila com
o circo, a roda gigante e o torii) saiu por inteiro no #199 — CSS, configuração e os seis desenhos que
só ela usava — **sem tocar em C# nenhum**: o `FluxoDoFront` manda o nome da facção como tema, e tema
sem CSS e sem entrada no `AR_DO_TEMA` simplesmente não tem pele, então o capítulo caiu no visual
padrão. Foi o seam pagando a conta na direção contrária: tirar uma pele é tão barato quanto pôr — e a
volta custou o mesmo, também sem C#. A volta não é a vila melhorada, é outra IDEIA: folclore não é
paisagem, é o que se conta, e o que se conta aqui é a clareira em volta do fogo.

**O que o ⭐ ESPECIAL ensinou** (jul/2026) — o banheiro público, e a pele que mais custou rodada de
ajuste até hoje.

A assinatura que sobrou não foi uma HORA, foi um TETO. Os cinco temas anteriores são paisagem ao ar
livre; este não tem céu nenhum, e é isso que o torna reconhecível de relance. A facção pedia: 💩 Cocô,
🦸 Herói, 🦹 Vilão e 🦖 T-Rex não moram em mundo nenhum em comum, e procurar a paisagem em que os
quatro coubessem ia dar cenário morno. O banheiro não é o mundo deles — é o único lugar onde os
quatro estarem juntos não precisa de explicação.

- **Corpo humano continua sendo o problema, e aqui a saída foi ESCONDER o corpo com o que a cena já
  tinha.** 🦸 e 🦹 estão atrás de um jornal aberto: o que sobra são as pernas com a calça caída, os
  dedos na borda do papel e o topo da cabeça com a máscara. Nenhum dos dois é desenhado. É a regra
  dos Místicos ("mostrar o sinal") levada um passo além — lá o sinal SUBSTITUÍA a figura, aqui um
  objeto de cena a TAPA.
- **A leitura de um jornal aberto:** a borda de cima é um V com a DOBRA NO FUNDO (o meio cede, os
  cantos empinam). Ao contrário — com o meio empinado — lê como placa. E as bordas de fora são
  CÔNCAVAS, porque o papel está virado pra quem lê e foge de nós.
- **A página vira pro lado DE QUEM LÊ**, e a borda de cima do jornal é a linha d'água dela: ela sobe
  do lado de lá, rompe a superfície, atravessa e afunda. É o recorte do golfinho outra vez. Cobrir
  com o jornal desenhado depois quase funciona — mas deixa a ponta escapando PELOS LADOS, além dos
  cantos do V, onde não há papel pra tapar.
- **Máscara se faz por RECORTE no círculo da cabeça.** A borda externa sai exata de graça, e o único
  caminho que sobra pra desenhar é o de BAIXO, que é justamente o que diferencia os dois. Como
  contorno fechado tentando acompanhar o crânio por fora, sobrava um fio de pele nas diagonais.
- **A CABEÇA DO BICHO PODE FICAR FORA DO QUADRO** (ideia do Gabriel). O T-Rex é grande demais pra
  sala e o pescoço sai pelo alto: o que se vê são duas pernas enormes, o tronco cortado em cima e a
  cauda. Um bicho que não cabe na tela lê como MAIOR do que qualquer bicho que coubesse — e some a
  peça mais cara de animar. O preço é que o rugido perde o rosto que o mostrava: quem o mostra passa
  a ser a sala (as portas escancaram, o pó risca, o fluorescente gagueja) e a cauda varrendo pouco.
  É a única coisa da cena que se vê pelo efeito e nunca pela causa.
- **DUAS PEÇAS CONCÊNTRICAS COM A ORDEM TROCANDO substituem qualquer conta de "quanto da face está
  virada".** É a ideia que fechou a cauda, e é do Gabriel. Uma fita de dorso, cheia; uma de barriga,
  menor; a de baixo nasce quando a cauda chega ao zero e passa a sobrepor. Antes disso houve TRÊS
  tentativas minhas de interpolar o giro — largura que abre, alfa que acende, faixa que anda pra
  borda — e as três erraram o mesmo alvo: inventavam um estado que não existe. Um tubo que gira passa
  do "vejo a face de cima" pro "vejo a de baixo" num instante só, quando cruza o perfil.
- **Listra de bicho ENTRA pela borda e AFINA até morrer.** Anel de espessura constante cruzando de
  lado a lado não é pele, é cinta de barril — é o afinar que dá o volume, porque é assim que uma
  marca some quando a superfície vira pra longe. E no tronco elas são VERTICAIS, seguindo uma
  longitude do barril: o x de cada uma é uma fração FIXA da meia-largura do corpo naquela altura,
  então ela abre e fecha junto com o corpo. Horizontal, a listra só atravessa; assim ela envolve.
- **Anel em DUAS METADES pintadas em momentos diferentes resolve oclusão sem recorte.** A metade de
  trás da boca do ralo vai antes de tudo, a da frente depois do cocô — e é ela que tapa o que já
  afundou. Junto numa peça só, ou a borda inteira ficava por baixo, ou por cima.
- **O beat do Papa-Léguas.** O ralo virou alçapão: as folhas abrem pra baixo, o cocô fica um tempo
  PARADO sobre o buraco com um tremeliquezinho, e só então despenca. O que dá graça não é a queda, é
  a pausa antes dela.
- **Consequência, não coincidência, de novo:** os respingos saem do abano CRUZAR O CENTRO, não de
  uma conta de distância entre a cauda e o cocô — o cocô cai no eixo do bicho, e é exatamente ali que
  a ponta da cauda passa. Mesma ideia da espuma disparada pela onda na praia.
- **Abrir num tranco e fechar devagar** é a diferença entre uma porta ARROMBADA e uma manobrada por
  alguém. A assimetria custa um `if` na velocidade do perseguidor e carrega a leitura inteira.
- **LIÇÃO DE COLABORAÇÃO, e é a mais cara desta pele:** quando o Gabriel descreve um MECANISMO
  ("faz dois rabos, o menor por baixo, e a ordem troca"), implementar LITERALMENTE e deixar ele
  corrigir. Eu interpretei várias vezes e errei todas — li "em baixo" como *a metade inferior da
  cauda* quando era *a face de trás*, li "sem girar" como *sem parecer que gira* quando era *sem
  rotação nenhuma*, e li "o rabo maior sobrepõe o círculo menor" ao contrário. Cada leitura minha
  custou uma rodada. O desenho dele estava certo desde a primeira frase.

### 🔴 DÍVIDA ANOTADA — um arquivo por cenário (dor PREVISTA, jul/2026)

Registrada pela regra de "dor prevista se anota e se avisa" (ver §Princípios). **Não está feita, e a
decisão de quando fazer é do Gabriel.**

> **MEDIDO DE NOVO em ago/2026 — a previsão bateu, e por baixo.** Os números abaixo eram de jul/2026;
> os de hoje estão na tabela. A frase "o arquivo caminha pra ~12.000" se cumpriu **com uma facção
> ainda faltando** (Humanos). A dívida parou de ser prevista e passou a ser vencida.
>
> | | jul/2026 | ago/2026 |
> |---|---|---|
> | `jogo.js` | ~5.900 | **11.920** |
> | cenário | ~70% | **~10.240 (86%)** |
> | telas + combate | ~1.700 | ~1.680 (14%) |
>
> O denominador dobrou e o numerador **triplicou**: tudo que entrou desde julho foi cenário. As telas
> não cresceram uma linha. Isso não é acaso — é o que acontece quando um arquivo tem duas razões pra
> mudar e só uma delas está ativa.

O `jogo.js` tem ~5.900 linhas e o cenário já é **~70% delas** — o assunto principal do arquivo virou o
que ele não se propunha a ser (menus, batalha, arsenal e campanha são as outras ~1.700). Quem for
mexer no botão de habilidade rola por 4.000 linhas de árvore, fogueira e corvo pra chegar lá. Cada
facção nova custa ~1.000-1.700 linhas, e faltam 5: o arquivo caminha pra ~12.000.

**O que torna a conta fácil:** a fronteira JÁ existe. O cenário inteiro fala com o resto do jogo por
UMA função — `aplicarTema(tema)`, chamada em dois lugares — mais os dois `<canvas>`. Não é preciso
desenhar seam nenhum; é o disco que não reflete a arquitetura que já está lá. O #199 provou isso
arrancando uma pele inteira sem tocar em C#.

**O corte:** `jogo.js` (jogo) · `cenario/motor.js` (`iniciarAr`, `aplicarTema`, `criarPo`,
`criarNevoa`, `criarVoadores`, `criarNoHorizonte`, `medirLadrilho`/`medirDoTema`, `VOADORES` e os
bichos compartilhados) · um arquivo por tema, cada um levando os desenhos exclusivos **e** a própria
entrada do `AR_DO_TEMA` (`AR_DO_TEMA.folclore = {...}`). Aí "uma facção nova" vira um `.js`, um
`.css` e uma linha no `index.html`.

**A pegadinha:** `AppFront.cs` navega pro caminho do disco, ou seja o front roda em `file://`, e ali
`<script type="module">` NÃO funciona (origem opaca). Então ou se aceita a saída barata — vários
`<script src>` clássicos em ordem, que é recorte-e-cola puro porque tudo já vive no mesmo escopo
global —, ou se paga uma mudança em C# (`SetVirtualHostNameToFolderMapping` + navegar pro host
virtual) pra poder usar módulos de verdade. A saída barata não custa C# nenhum e é reversível.

#### O PLANO, fechado em ago/2026 (o "como", que faltava)

**A decisão entre as duas saídas: PAGAR o C#.** Em jul/2026 as duas empatavam. Não empatam mais: com
9 cenários + núcleo + telas, a saída barata vira ~30 `<script src>` que **um humano tem que manter em
ordem de dependência à mão**, e o erro dela é silencioso (função indefinida em tempo de execução,
cena em branco). O portão inteiro são 3 linhas, em `AppFront.cs:143`:

```csharp
webview.CoreWebView2.SetVirtualHostNameToFolderMapping(
    "apostlesware", Path.Combine(AppContext.BaseDirectory, "wwwroot"),
    CoreWebView2HostResourceAccessKind.Allow);
webview.CoreWebView2.Navigate("https://apostlesware/index.html");
```

Origem `https://` de verdade → `import` nativo, sem bundler, sem npm, sem build step. **Verificação:
o jogo abrir igual já prova.**

**Os critérios (o que decide as pastas, e vale além deste repo):**
1. **Nunca por tipo de arquivo.** `css/`, `js/`, `img/` é o layout que todo mundo aprende e que os
   times abandonaram: pra mexer no Folclore você abre 3 pastas, e nenhuma delas diz que Folclore existe.
2. **O que muda junto fica junto** (Common Closure). O CSS, o JS e a config do Folclore têm a MESMA
   razão pra mudar. É o irmão front do `Champs/<Faccao>/<Champ>/` que o C# já faz.
3. **O nome da pasta grita o DOMÍNIO**, não a tecnologia.
4. **Um arquivo = uma intenção ao abrir.** Tamanho não é critério — mas passou de ~800 linhas, quase
   sempre tem duas razões escondidas. (Este arquivo tinha. Ver a tabela acima.)
5. **A dependência aponta pra dentro**, igual ao C#: `cenarios/` usa `nucleo/`; `nucleo/` **nunca**
   sabe que Folclore existe.

**O destino:**

```
wwwroot/
  index.html
  estilo.css              ← encolhe pra shell + tokens (:root, rolagem, body, a Forja)
  nucleo/  ponte.js (envio/recepção com o C#) · cena.js (roteador de telas) · ar.js (iniciarAr, rAF, maestro)
  ui/      placa.css · barraVida · tooltip          ← as peças compartilhadas
  telas/   menu/ perfil/ campanha/ arena/ arsenal/ compendio/ combate/
  cenarios/ comum/ + reino/ ladosombrio/ tecnologicos/ folclore/ misticos/
            especial/ decaidos/ apostolos/ humanos/
```

Cada facção vira **~700–1.300 linhas** — tamanho que cabe na cabeça. E cada pasta de cenário leva os
três: `<faccao>.js`, `<faccao>.css` (o bloco `body[data-tema=...]`) e a própria entrada do
`AR_DO_TEMA`, que deixa de ser objeto central e vira `export const ar = {...}`.

**A parte difícil, que NÃO é opinião — é medição.** Os **102 builders** de topo da região de cenário
estão **planos e sem dono declarado**, ordenados por ordem histórica de construção, não por facção
(só o 🔱 Decaídos tem cabeçalho de seção). `criarFogueira`, `desenharEspada`, `criarMoitas`,
`medirLadrilho`: alguns são de uma facção só, outros são compartilhados, e **não dá pra saber
olhando**. O critério é claro — *usado por 1 facção vai pra pasta dela; usado por 2+ vai pro
`cenarios/comum/`* — mas a atribuição tem que ser **medida** por um script que lê o `AR_DO_TEMA`,
resolve os nomes que cada tema referencia e monta a tabela de quem-usa-o-quê. Sem isso, você move uma
função e descobre três facções depois que o Místicos usava ela. É o "o grep mente" em forma de
refatoração — e o script é descartável depois.

**A ordem:**
1. As 3 linhas de C# (host virtual). PR minúsculo, destrava tudo, verificável na hora.
2. O script de medição dos 102 builders → a tabela de donos.
3. **Um cenário por PR**, começando pelo 🔱 Decaídos (é o único já com cabeçalho, e é auto-contido).
   Nove PRs pequenos e verificáveis em vez de um monstro.
4. `telas/` e `ui/` por último — são 14% e não estão doendo.

**O que uma empresa faria e aqui NÃO se deve fazer:** Vite, TypeScript, framework de componente, CSS
Modules, ESLint, runner de teste de front. Numa empresa isso se paga com 8 pessoas e deploy. Aqui o
ganho está **na separação, não na ferramenta** — e ES module é nativo, custo zero. A única ferramenta
que vale é o script de medição do passo 2, e ele morre depois de usado.

#### O Claude Design entra nisso? (avaliado ago/2026 — decisão: AINDA NÃO)

Pergunta que vai voltar, então fica respondida por escrito. **O que a ferramenta é:** um agente que
constrói telas **em React**, renderizadas ao vivo no navegador. Por padrão ele monta com componentes
genéricos; a skill `/design-sync` existe pra ensinar a ele os componentes DO PROJETO, e o que ela
sobe é um `_ds_bundle.js` (React compilado a partir do `dist/` do repo), um `.d.ts` com o contrato da
API e cards de preview. Ela pressupõe, portanto: `package.json`, build, e componentes de verdade.

Aplicado a este front, parte em três, com três respostas diferentes:

| pedaço | veredito |
|---|---|
| **Sincronizar este front pra dentro dele** | **Inaplicável.** Não é difícil — não há o que converter. Sem `package.json`, sem React, sem `dist/`. |
| **Usar de prancheta pro chrome** (menus, arsenal, compêndio, fases, arena — os 14%) | **Daria**, e ataca um ponto fraco real: desenhar TELA em vez de PEÇA, e ver antes de escrever. Mas a saída é React e teria que ser **traduzida à mão** pro CSS daqui. Ganha-se na decisão, paga-se na transcrição. |
| **Os 86% de cenário em canvas** | **Fora de escopo.** `requestAnimationFrame` e trigonometria não são composição de componente. Nenhuma ferramenta de UI toca nisso. |

**O gargalo deste front nunca foi capacidade de design — é o LAÇO DE RETORNO.** Todo acerto real da
pele nova (o cercadinho da arena, os 27 seletores que ficaram lisos, o fundo dos menus tapado pelos
overlays) veio do Gabriel rodando o jogo e apontando o defeito exato. Ferramenta nenhuma substitui
isso; o que ela faria é encurtar. E existe substituto barato que pega quase todo o ganho: **artifacts
no nível de TELA** (a tela de fases inteira, a arena inteira), que ele abre no Edge — mesmo benefício
de ver antes, zero React pra traduzir, zero setup.

**O "ainda não" tem prazo, e o prazo é ESTA seção.** No dia em que o front virar biblioteca de
componentes de verdade — que é exatamente o `ui/` + `telas/` do plano acima — a ferramenta passa a
ser aplicável, porque aí existe o que sincronizar. Ou seja: **a separação é o pré-requisito, não uma
tarefa concorrente.** Reavaliar DEPOIS que `ui/` e `telas/` existirem, nunca antes.

---

## Princípios que guiam toda a refatoração

- **Todo método morto na maioria das classes que o herdam vira interface** (ISP).
  Base magra + interfaces por capacidade. A declaração da classe documenta o que faz.
- **Refatore por DOR, não por pureza.** Migra o que incomoda; o resto segue por
  boy scout (quando tocar) ou PR dedicado quando virar dor. **E quando a dor ainda
  não foi SENTIDA mas dá pra PREVER** — o arquivo que vai dobrar de tamanho, o seam
  que já existe e o disco não reflete —, **a regra é ANOTAR aqui e AVISAR o Gabriel
  na hora**, em vez de refatorar por conta própria ou de deixar passar em silêncio.
  A decisão de quando pagar é dele; o que não pode é a previsão morrer na cabeça de
  quem viu.
- **Refatore o que PERSISTE; tolere imperfeição no que vai MORRER.** A camada de
  apresentação do console (telas, render) morre no porte. Não investir rigor nela.
  A LÓGICA de domínio (combate, turno, reações, stats) pluga DIRETO no porte como
  assembly C# — é onde o rigor rende. **Destino (jul/2026): APP NATIVO DESKTOP,
  baixável.** O jogo já é `<OutputType>Exe</OutputType>` — roda NATIVO na máquina, sem
  servidor. Distribuição: `dotnet publish` self-contained → **.exe no GitHub Releases**
  (baixa e joga; sem instalar .NET). A "pele" (View) vira **webview HTML/CSS/JS** (danos
  pulando em CSS; o clique entra pelo `IEntrada`, o C# nativo faz a lógica atrás de uma
  ponte local — sem WASM, sem demora de carregamento, sem servidor). Ferramenta específica
  (Photino / WebView2) decidida na hora do porte. Single-player, **save local** pela porta
  `IRepositorioDeSave`. Repo PÚBLICO + LICENSE. **Sem SQL, sem API/servidor próprio.**
  Descartados: Blazor WASM (o runtime .NET baixa antes de rodar = demora que o Gabriel quer
  evitar); reescrever em JS = 2º motor; C#-backend+API = precisa hospedar servidor. **Unity
  fica como caminho alternativo futuro** (se um dia quiser mobile/console ou animação pesada;
  os mesmos seams reaproveitam).
- **Um PR, um tema.** Não abrir duas frentes grandes ao mesmo tempo.
- **Destravar de forma encadeada.** Escolher a ordem onde cada peça destrava a
  próxima e minimiza retrabalho. Pode-se reabrir algo, mas o caminho limpo é melhor
  que andar em círculos.
- **Exception nos LIMITES (I/O, parsing externo), nunca no NÚCLEO** (lógica de
  domínio — se lança lá, é bug, deve estourar pra corrigir).
- **Build verde (Ctrl+Shift+B) antes de todo push.** CI manual do dev solo.
- **Não desenhar no escuro.** Interface/categoria nasce servindo efeitos reais que
  existem, não casos hipotéticos. YAGNI até o efeito que justifica aparecer.
  EXCEÇÃO consciente: o EventoDano foi investido cedo na fundação de exibição/porte
  (Propósito B) — decisão explícita de aceitar mais trabalho por uma base sólida.

---

## FILA DE EXECUÇÃO (rumo ao porte: app nativo desktop) — ordem mestra

> **Decisão (jul/2026): APP NATIVO DESKTOP BAIXÁVEL.** O jogo já é um executável C#
> (`OutputType Exe`) — roda NATIVO na máquina, sem servidor. Sai do console trocando só a
> View: a "pele" vira **webview HTML/CSS/JS** (o Gabriel quer aprender web e o motor pluga
> atrás de uma ponte local), com o C# rodando nativo (sem WASM → **sem demora de
> carregamento**). Distribuição: `dotnet publish` self-contained → **.exe no GitHub
> Releases** (baixa e joga, não precisa instalar .NET). Ferramenta de webview específica
> (Photino / WebView2) decidida no porte. Single-player, **save local** pela porta
> `IRepositorioDeSave`. Repo PÚBLICO + LICENSE. **Sem SQL, sem API/servidor.** Isso MATOU o
> enquadramento web/RESTful/SQL antigo. (Descartados: **Blazor WASM** — o runtime baixa
> antes de rodar, é a demora que se quer evitar; reescrita em JS = 2º motor;
> C#-backend+API+JS = precisa hospedar servidor. **Unity** fica como alternativa futura para
> mobile/console ou animação pesada — os mesmos seams reaproveitam.)
>
> **Como atacar:** FECHAR a FILA A no console (domínio, serve qualquer plataforma) ANTES do
> porte. Itens 1 por 1 (cada um = 1 PR, 1 mergeado antes do próximo). A FILA B espera o porte.
> A FILA C está descartada.

### 🟢 FILA A — console, agora (numerada)

> **Regra do modo de ataque (Gabriel, jul/2026): nada fica "quando doer" — hoje TUDO dói.**
> Item visto na auditoria = item na fila. O "quando doer" só sobrevive na FILA B, onde o
> gatilho é nomeado (o porte), não adiamento genérico.

1. ✅ **Doc/organização** (#141) — gravou esta fila, matou o enquadramento web, reancorou
   encapsular-coleções, fechou nulo-na-porta, corrigiu drift dos testes.
2. ✅ **`ColetarReacoes<T>` helper** — DRY das varreduras de reação do CombateService. Vive em
   `CombateService.ColetarReacoes<T>` e varre as DUAS fontes (`StatusAtivos` +
   `ColetarPassivasReativas`, esta respeitando cooldown) numa fonte só; usado nas 8 varreduras
   (InicioTurno, ReceberDano, SerAtacado, PorAtaque, CausarDano, AoAtacar, AoMatar, AoMorrer),
   o que já fechou a consistência de dispatch no InicioTurno. *(Estava marcado pendente por
   DRIFT do doc — auditoria jul/2026 achou o helper já implementado e em uso.)*
3. ✅ **Faxina de seams restantes** *(auditoria jul/2026)* — os 2 `Console.Clear` fora da View
   (`CombateService.ExecutarTurno`, `ControladorJogador`) viraram `CombateView.LimparTela()`
   (mora no adapter de console do combate, onde os outros `Console.*` já estavam). Os avisos de
   save com `Console.WriteLine`+`Thread.Sleep` nos services ✅ **morreram no #6** (a porta levou o IO
   e o aviso foi dropado). `Program.cs` `Console.OutputEncoding` deixado de propósito (bootstrap do composition
   root — o ponto que sabe que é app de console; descartado no porte Unity).
4. ✅ **Identidade comum + libertar "Turno"** — base `ElementoDeJogo` (Nome/Símbolo/Descrição)
   herdada por `Habilidade` e `StatusEffect`. **De quebra (pedido do Gabriel): o homônimo `Turnos`
   morreu** — era *cooldown* na Habilidade e *duração* no StatusEffect, poluindo o conceito real de
   "Turno" (a jogada inteira). Agora: `Habilidade.Cooldown`, `SkillCooldown.CooldownRestante`,
   `StatusEffect.DuracaoRestante`; params de status → `duracao`, de habilidade → `cooldown`. Achado:
   `StatusEffect.Turnos` estava MORTO (setado, nunca lido) → deletado. "Turno"/`PassarTurno`/
   `AoIniciarTurno`/`TurnoDoPersonagem` preservados (o turno de verdade).
5. ✅ **Services-lookup** — `FaccaoService`/`CampanhaService` (tabelas puras injetadas) viraram DADO
   estático (`Models/Faccoes.cs`, `Models/Campanha.cs`), fora do grafo de DI. Achado: `ObterNome` era
   MORTO **e** duplicava o `[Description]` do enum `Faccao` (que o `Helper.GetDescricao` já lê) →
   deletado; sobrou só o Símbolo. Program.cs enxugou 2 instâncias + args de 3 ctors.
6. ✅ **Porta de persistência `IRepositorioDeSave` + `SaveLocal`** — o IO de arquivo saiu de
   `CapitulosService`/`ArsenalService` pra trás da porta (`Services/IRepositorioDeSave.cs`). **Corte
   typed:** a porta é dona do JSON + IO + tratamento de corrupção; os services entregam/recebem OBJETO
   (`_repo.Salvar("save", capitulos)` / `_repo.Carregar<List<Capitulo>>("save")`). `chave`→`save.txt`/
   `itens.txt` (compat). **Aviso de save-corrompido DROPADO** (corrompido → default silencioso). Os 2
   `Console.WriteLine`+`Thread.Sleep` dos fallbacks (dívida do #3) morreram → **`Services/` inteiro
   ficou Console-free.** Steam/Play plugam no porte (FILA B) trocando só a impl.
7. **Estatísticas de fim de batalha + cura/DoT visíveis** *(pedido do Gabriel, jul/2026)* —
   **7a ✅ FEITO:** `Combate` acumula `DanoCausado`/`DanoRecebido`/`CuraRecebida` nos funis únicos
   (`ReceberDano` pega ataque+veneno+queima+explosão; `AplicarCura` a cura); tela de **resumo de fim
   de batalha** por personagem do time (`CombateView.ExibirResumoBatalha`); **TaxaCrit/DanoCrit ao
   vivo** na tela de combate (🎯%/💥x — absorve o item #11). Sem tocar no canal de eventos do motor.
   **7b ✅ FEITO:** stream único `EventoCombate` (base) com `EventoDano` e `EventoCura` irmãos;
   `HabilidadeAtiva.Ativar`/`Acao.Executar` devolvem `List<EventoCombate>` (as reações filtram
   `.OfType<EventoDano>()`). **Cura vira evento** (a Ação `Cura` emite `EventoCura`; `Curar`/`AplicarCura`
   retornam o quanto curou); **veneno/queima/cura-contínua ficam VISÍVEIS no início do turno**
   (`AoIniciarTurno` devolve `EventoCombate?`; `TurnoDoPersonagem.Iniciar` coleta — **nulo morre na
   porta**; `CombateService.MostrarTicks` exibe). Views novas: `ExibirDanoDeStatus`/`ExibirCura`.
   A cura mostra mesmo curando 0 ("já está com a vida cheia" — decisão do Gabriel). **Nome da
   habilidade agora aparece ANTES dos resultados** (`ExibirUsoHabilidade` movido pra antes do
   `ExecutarAtos`). **Cura de REAÇÃO corrigida** (Sedento/Sangramento/SedentoDeSangue/Bênção): usavam o
   valor PEDIDO na mensagem; agora usam o RETORNO de `Curar` (o real, capado). E **`ResultadoReacao.Cura`
   virou `EventoCura?`** (simétrico ao `Dano`): a cura de reação agora usa a **MESMA `ExibirCura`** da
   cura de habilidade (mensagem padrão "💚 X recuperou Y", não bespoke por reação — pedido do Gabriel).
   É o embrião do log/stream da FILA B.
8. ✅ **Capacidade C — stat sob demanda (`IContribui*`)** — generalizada pros 4 stats. Cada
   getter (`Ataque`/`Defesa`/`TaxaCrit`/`DanoCrit`) agora SOMA a interface de contribuição com
   sinal (`IContribuiAtaque`/`IContribuiDefesa`/`IContribuiTaxaCrit`/`IContribuiDanoCrit`), não
   tipo concreto — inclusive o getter `Defesa`, que estava meio-migrado (ReceberDano já usava a
   interface). **Matriz de status agora simétrica** (buff + debuff pra todo stat): nasceram
   `ReducaoAtaque`, `ReducaoTaxaCrit`, `BuffDanoCrit`, `ReducaoDanoCrit` (só infra — champs ligam
   no rebalance #16). `AtaqueComStacks` exposto (espelho de `DefesaComStacks`). Refactor puro,
   nenhum número mudou; +6 testes headless (getters são puros).
9. ✅ **Capacidade D — comportamento de turno** — o `CombateService` parou de decidir por tipo
   concreto (`is Preso`/`OfType<Irritar>`/`OfType<Medo>`); cada status carrega a própria
   capacidade. NÃO é uma "família" única (as formas diferem por FASE): `IPulaTurno` (Preso, antes
   da escolha — marcador), `IForcaAcao` (Irritar, na escolha — devolve o alvo forçado),
   `IParalisaAcao` (Medo, após a escolha — rola o dado). O `IPulaTurno` já nasce como a PORTA da
   família de pular-turno (Congelar/Stun/Enraizado/Petrificado plugam sem tocar no fluxo — variantes
   são tema à parte, o Gabriel desenha as diferenças). Migração pura; +3 testes headless.
10. ✅ **`IModificaDanoCausado`** — espelho do Recebido no ATACANTE (Combat/, forma multiplicador),
    consultado pela Ação Dano (o comentário do Dano.cs já previa). O Piromancer virou capacidade
    (implementa a interface), o `static MultExtra` MORREU e as habs do Mago voltaram a `Dano(2.0)`/
    `Dano(1.5)` puras. **Distinção firmada (verify-before-fuse):** "ler status do alvo pra escalar
    dano" tem 2 baldes — fórmula-DA-HAB (`Dano(Func)`: Caveira `2.0-HP%`, Tengu/CorteDeVento `1.0+escudo`)
    vs modificador-DO-ATACANTE (passiva, vale pra todos os ataques: `IModificaDanoCausado`, só o
    Piromancer). O Tengu PARECIA 2º cliente mas NÃO é (só o CorteDeVento escala; subir pra passiva
    vazaria pro Vendaval) → fica no Func. **Única mudança de jogo:** o A1 do Mago (usa `Dano(1.0)`)
    passou a ganhar os +25% vs alvo com Queima (antes só as 2 especiais) — contido, mais correto,
    paridade exata nas especiais. +2 testes.
11. ✅ **Turno (resto) — FECHADO (A+B+C).** **(A)** `TurnoDoPersonagem` PERSISTENTE (Caminho B),
    fundação. **(B)** orçamento de reação por chave (`TentarReagir`), Espinhos/Zumbi/Cocô 1x-por-agressor
    (por-hit segue first-class). **(C) RENASCEU** — a "TimeAtualDoTurno" original (times no combatente)
    morreu (premissa velha: contexto já é a fonte de perspectiva); virou **`Equipe`/`Batalha`** (Combat/):
    a perspectiva nasce num só lugar (`Batalha.PerspectivaDe`) derivada da ESTRUTURA (qual equipe), não
    do tipo `is Jogador`. Matou os 3 flips manuais (incl. a recursão do revide) e desacoplou
    time×controle×classe → **seam do modo VERSUS** (§Versus). Refactor puro, 54 testes. Medidor de
    velocidade = habilitado, fora do #11.
12. ✅ **Passiva-conta-mortos** — FEITO. Capacidade genérica `EscalaComMortos` (Skills/Passivas/):
    `IReageAoInicioTurno` que renova um buff proporcional aos mortos no campo (molde da Ventania;
    valor dinâmico). Generaliza escopo (`ProprioTime`/`TimeInimigo`/`AmbosOsTimes`) × stat (fábrica
    `Func<double,Buff>` da matriz #8) × por-morto. **Zumbi** = 1º cliente: perdeu a `PutrefacaoContagiosa`
    (clone do Fedorento do Cocô — sem identidade) e ganhou **"Horda"** (+10% ATK/morto dos 2 times,
    placeholder pro rebalance). Irmã `EscalaComAbates` (on-kill permanente) DESENHADA, não construída
    (ver §conta-mortos). 4 testes.
13. ✅ **Observabilidade Crit na UI** — absorvido pelo **#7a**: a `CombateView` mostra
    🎯TaxaCrit/💥DanoCrit ao vivo pra todo combatente (time e inimigos) e no resumo de fim de
    batalha. Como os getters somam os bônus permanentes, o acúmulo do OlhoClinico (TaxaCrit) e
    do Vírus (DanoCrit) aparece na tela sozinho. *(Estava marcado pendente por DRIFT do doc.)*
14. ✅ **Ampliar testes xUnit** — **`ReceberDano` ponta-a-ponta** (`Tests/ReceberDanoTests.cs`,
    14 testes): matemática da defesa (fórmula proporcional, cap de 75%, `ignorarDefesaPct`,
    `IgnoraDefesa` da natureza, BuffDefesa com sinal), ordem **passiva-pura ANTES dos status**
    (provada pelo número: se invertesse, o dano efetivo mudaria), escudo absorvendo parcial,
    os **acumuladores do resumo** (#7a: DanoRecebido/DanoCausado/CuraRecebida, incluindo dano
    CHEIO sob piso de HP e cura capada no teto) e a confirmação de morte (prevent-death salva
    1× e **consome cooldown** — o 2º golpe fatal mata). **Guard da mina do `ResolverAlvos`:** a
    semente agora é validada contra os candidatos e **lança `InvalidOperationException`** se não
    for candidata (antes entrava no resultado e o `IndexOf` devolvia -1, desalinhando o sorteio
    dos extras — errado em silêncio). Critério do Gabriel: *mira errada é bug de declaração do
    champ, tem que gritar* — não "consertar" escondido. O guard mora DEPOIS do early-return de
    candidatos vazios, porque "sem ninguém no estado pedido" é caso LEGÍTIMO (Doces de Abóbora
    sem mortos → o CombateService manda o próprio atacante e conta com o vazio). 3 testes cobrem
    os 2 lados + o caso legítimo. 75 testes verdes.
    - **Ordem crítica de morte — DIFERIDA pro FRONT (Fatia 1), de propósito.** A ordem
      (Sentença/`IReageAoMatar` antes de Necromancia/`IReageAoMorrer`, CombateService) é
      load-bearing de verdade: invertida, a Necromancia revive e a Sentença nem dispara (ela tem
      `if (alvoMorto.EstaVivo()) return`). Mas um teste que PEGUE a troca precisa rodar o fluxo, e
      o fluxo chama a View. Descartados na discussão: **wrapper** "só pra criar a ordem" (não
      protege — a ordem continua trocável dentro dele) e **composição à mão** no teste (teatro: o
      teste é que escolheria a ordem, não o fluxo). O teste real nasce quando a Fatia 1 do front
      tornar a apresentação injetável — evita desenhar 2× o seam que o front vai redesenhar.
      Até lá a ordem é guardada pelo comentário no `ProcessarReacoesAoMorrer` + ADR.
15. 🔜 **Faxina de comentários — REORDENADA, não cancelada** (ago/2026). Acontece **DEPOIS da separação
    do `jogo.js`**, e em duas etapas: o cenário que se move já sai reduzido, e então uma passada pelos
    OUTROS arquivos. Ganhou também uma regra de escrita (`CLAUDE.md` §Comentário) pra a vazão não
    repor o que a faxina tirar. Ver §Faxina de comentários — a medição de lá é o BRIEFING dela.
16. 🔄 **REBALANCEAMENTO — EM ITERAÇÃO** (1ª passada mergeada no #189). Não é um item que "termina":
    a bancada (§BANCADA DE DANO) é o instrumento, e cada volta é ler os números e mexer numa
    alavanca. A passada 1 padronizou o cooldown em 3 e subiu os multiplicadores de faixa.
    **Passo 0 (centralizar as constantes) — NÃO é mais item próprio.** Ele foi ABSORVIDO pelo PR de
    DIFICULDADE (decisão do Gabriel, ago/2026): reunir as constantes agora e mexer nelas depois é
    tocar duas vezes nos mesmos arquivos, e a dificuldade é justamente quem precisa da fórmula
    centralizada pra acrescentar o eixo dela. Um movimento só, quando o PR de dificuldade vier.
    **O inventário, medido em ago/2026** (pra não recaçar):

    | onde | constante | forma hoje |
    |---|---|---|
    | `Combate.cs:47-48` | `DefesaPorPontoReducao` 1000 · `ReducaoMaximaPorDefesa` .75 | `private const` |
    | `Personagem.cs:12-13` | `TaxaCritBase` .15 · `DanoCritBase` .60 | `public const` |
    | `CombateService.cs:586-588` | `0.5×capítulo + 0.1×fase` | **escrita 3×** (HP/ATK/DEF) |

    A fórmula é o único item repetido, e é ela que ganha o `1.75×dificuldade`. **Cuidado ao mexer:**
    `ReceberDanoTests.cs:26-27` tem uma CÓPIA das duas constantes de defesa — e ela deve CONTINUAR
    cópia. O teste enuncia a regra por conta própria; apontá-lo pra constante central o tornaria
    tautológico e mudar o balance deixaria de acusar nada.

**Disciplina permanente (NÃO é PR):** varredura de camadas — se cruzar com código fora do
lugar fazendo outra coisa, conserta no mesmo PR; nunca um PR só pra isso.

### 🔵 FILA B — precisa do porte (sair do console)
**Porte = APP NATIVO DESKTOP com pele webview (HTML/CSS/JS) — ✅ FEITO (ver §ESTADO ATUAL).** A View
HTML/CSS/JS, o host WebView2, o `SaveLocal` e a distribuição estão de pé; o front cobre menu, perfil,
arena, campanha e arsenal. O texto abaixo fica como registro do desenho original.
- **View HTML/CSS/JS** — a UI que substitui `CombateView`/`MenuView`. Os seams
  `IApresentacao`/`IEntrada`/`IControladorDeTurno` viram impls que conversam com a webview por
  uma ponte local (`postMessage`): o clique volta pelo `IEntrada`, o C# nativo faz a lógica, o
  resultado sobe pro JS animar (danos pulando em CSS). Personagens = EMOJIS por ora (sem sprites).
- **Host da webview** — Photino (cross-platform) ou WebView2 (Windows); decisão no porte. O C#
  roda NATIVO (não WASM) → zero tempo de carregamento.
- **`SaveLocal` já serve** — a porta `IRepositorioDeSave` grava em arquivo na máquina (impl atual);
  nada a trocar pro desktop. (Steam/Play Cloud entram só se um dia publicar em loja.)
- **Distribuição** — `dotnet publish -c Release -r <rid> --self-contained` → .exe único → GitHub
  Releases. Opcional: GitHub Actions builda o .exe a cada tag.

**Alternativa futura = UNITY (se quiser mobile/console ou animação pesada; reusa o mesmo motor):**
- **Input via Unity Input System** (mouse/gamepad; `Selecionar(N)` já é a ponte).
- **Camada de animação/eventos** (coroutines/UnityEvents/animation events) — consome os eventos.
  Nota: contra-ataque com A1 usa a animação de A1 do PRÓPRIO champ (já roda a habilidade A1 real
  via `AtivarComNatureza`, então o vínculo mapeia natural).
- **`SaveSteam` / `SavePlayGames`** — impls da porta `IRepositorioDeSave` (Steamworks.NET /
  Google Play Games plugin). Steam Auto-Cloud ≈ zero código; Play usa Snapshots.
- **[bônus] champ-como-dado → ScriptableObjects** (o motor de Ações já está pré-moldado pra isso).

**Comum aos dois portes (a "leva de eventos"):**
- **Encapsular coleções → choke-point de evento de status** (gatilho da camada de eventos).
- **`EventoDano` por ID** — desacoplar dos objetos `Combate` vivos → log/stream limpo.
- **#7b — cura/veneno/queima visíveis** (o `EventoCura` + mensagens) alimenta essa camada.

### 🟠 FILA C — DESCARTADA (nenhum servidor SQL/REST próprio)
- Cloud save é via SDK de plataforma (Steam/Play), plugado na porta `IRepositorioDeSave` — não é
  backend seu. SQL/REST só reabrem se um dia quiser contas/ranking/servidor próprio.

---

## FRONT (webview desktop) — ARQUITETURA DECIDIDA (jul/2026)

**Ordem:** o front vem ANTES do REBALANCE #16 (decisão do Gabriel — quer balancear numa interface
amigável, não no console). É uma FASE (várias fatias), não 1 PR.

### Método — NÃO é MVC nem REST
É uma **ponte de mensagens LOCAL, in-process, orientada a eventos.** O JS (tela) e o C# (motor) rodam
no **MESMO processo** (o `.exe`) e trocam mensagens **direto pela webview** — sem HTTP, sem servidor,
sem rede, offline. Analogia: não é "cliente e servidor pela internet", é "duas partes do mesmo programa
passando bilhetes" (padrão de app desktop com webview — VS Code / Discord / Spotify desktop por dentro).
- **REST/servidor foi DESCARTADO** (= FILA C): precisa hospedar, não é grátis/offline.
- **Encaixa nos seams já prontos:** `IEntrada` recebe o clique-do-JS → `Comando`; `IApresentacao`
  empurra o estado/eventos pro JS desenhar. O motor C# **não sabe** que é webview.
- **MVC "de leve" (só a separação natural):** View = HTML/CSS/JS; Model = domínio C#; cola = o host
  webview. NÃO é o framework ASP.NET MVC. Existe um "protocolo" (formato dos bilhetes, provavelmente
  JSON), mas é um **contrato interno do app**, não uma API.

### Ferramenta — **WebView2** (jul/2026: o Photino FOI TENTADO E DESCARTADO)
O **HTML/CSS/JS mora NESTE repo** (sobe pro GitHub junto — não é site/serviço à parte).
Distribuição: `dotnet publish` self-contained → **`.exe` no GitHub Releases**. C# roda NATIVO (não
WASM) → zero carregamento.

**Photino descartado por evidência, não por preferência.** Ele sobe em `net10.0` sem problema
(o NuGet resolve `net8.0` como compatível; os binários nativos são por RID) — mas a **janela abre
PRETA**: o título é setado, o processo não crasha, e nada é renderizado. Reproduzido com
`LoadRawString` (HTML embutido, sem arquivo) E com `Load` de arquivo, com pasta de dados explícita,
na última versão existente (Photino.NET 4.0.16 + Native 4.0.22). O WebView2 Runtime da máquina está
instalado e íntegro (150.0.4078.83) — o **WebView2 direto renderizou de primeira** no mesmo runtime.
Suspeita: shim nativo do Photino velho demais pro runtime atual. **Não vale investigar mais fundo:
o Photino não expõe `CoreWebView2InitializationCompleted`/`NavigationCompleted`, então não dá pra
instrumentar a falha — e essa opacidade é, por si só, o argumento contra ele.**

**Preço do WebView2:** exige host WinForms → TFM `net10.0-windows`. Resolvido SEM contaminar o motor:
o projeto foi partido em **`ApostlesWar` (biblioteca, `net10.0` puro — motor + views de console)** e
**`App/ApostlesWar.App` (Exe, `net10.0-windows` — composition root + front)**. Os Tests seguem
`net10.0` intocados. A separação de camadas que as portas já faziam no código agora aparece também na
estrutura de projetos. *(Gotcha registrado: o pacote do WebView2 referencia as variantes WinForms E
WPF sem condição — `build/Common.targets:133` — e a WPF arrasta outro `WindowsBase`, gerando MSB3277;
removida num Target no csproj do App, já que é referência morta aqui.)*

### Visual — emoji é PLACEHOLDER, não o teto
v1 = emojis + CSS (dano pulando) pra o loop andar rápido sem depender de arte. **Teto real = sprites
pixel ANIMADOS** (sprite sheets + CSS `steps()` ou `<canvas>`; `image-rendering: pixelated` mantém o
pixel nítido; arte do **Pixelab** pluga direto). O motor só EMITE eventos (o stream `EventoCombate`/
`EventoDano` = o gancho de animação); o front decide o quão rico renderiza. **Emoji → sprite = troca de
render no JS, SEM tocar no motor.** Sem susto de performance (é por turnos, não ação 60fps).

### Fatiamento (seam primeiro, sempre)
- **Fatia 0 — casca + ponte: ✅ FEITA.** Janela WebView2 + round-trip JS→C#→JS provado.
- **Fatia 1 — tela de combate: ✅ FEITA** (`--front` cai direto numa Arena com times sorteados).
  - **Descoberta que mudou o desenho:** `IApresentacao` **NÃO era** porta de render — só encapsula a
    ESPERA (`AguardarAnimacao`). O render era `Console.WriteLine` dentro da `CombateView`, concreta
    dentro do `CombateService`. A porta de verdade nasceu agora: **`View/ITelaDeCombate`**.
  - **O contrato é GATILHO, não desenho.** Os nomes são imperativos por herança do console
    (`Exibir...`), mas a impl web traduz cada chamada em (a) um RETRATO do estado serializado ou
    (b) um EVENTO pra animar. A tela se redesenha do estado; nunca recebe ordem de desenho. É isso
    que faz emoji→sprite ser troca só de front.
  - **O laço síncrono foi PRESERVADO INTEIRO** — nada virou async. A UI fica na thread principal
    (`Application.Run`) e o jogo numa thread de fundo; `IEntrada.Ler()` bloqueia num
    `BlockingCollection.Take()` que o clique do JS alimenta. Foi a `IEntrada` bloqueante (que parecia
    um problema pro porte) que salvou o motor de qualquer cirurgia.
  - **Menu de ação e de alvo ficaram FORA da porta:** são navegação por CURSOR, formato do console.
    Quem decide ação/alvo é o `IControladorDeTurno`, e o front tem o seu (`ControladorJogadorWeb`,
    clique-na-habilidade → clique-no-alvo). Botá-los na porta obrigaria a impl web a carregar
    método morto.
  - **Carona ainda em aberto (do #14):** com a apresentação agora injetável, o **teste da ordem
    crítica de morte** (Sentença antes de Necromancia) virou possível com uma tela no-op. Não feito
    neste PR (1 PR, 1 tema) — é o próximo candidato.
- **Fatias seguintes — ✅ FEITAS** (ver §ESTADO ATUAL no topo): menu principal + perfil (#172),
  Arena com seleção de time (#173), montagem de time com arrastar-e-soltar (#175), campanha com mapa
  de facções (#174), arsenal (#176/#177). Configurações (som/tela-cheia) ficaram como placeholder
  "em breve"; falas dos champs e sprites seguem em aberto (o front emite/renderiza, o motor não muda).

---

## BOT INTELIGENTE + MODO AUTO (jul/2026)

O `ControladorBot` só usava A1, o que deixava o inimigo burro e — pior — fazia o **Bot×Bot não
exercitar habilidade nenhuma**, cegando a Arena como laboratório do REBALANCE (#16). Decisão do
Gabriel: **um cérebro só**, o mesmo que joga pelo inimigo joga pelo jogador no **modo Auto
assistido** (botão na batalha, interruptor lido entre turnos, sem mudar o ritmo — é pra assistir).
Simular N batalhas e grindar ficaram nomeados e adiados.

### PR-A ✅ — `PreverDano`: a fórmula de dano ganhou um espelho puro (#181)

Pra comparar alvos o bot precisa da fórmula REAL. O plano dizia "extrair a parte pura do
`ReceberDano`" — **não havia parte pura**: o `Escudo` consome pontos e se remove, e o
`ProtecaoAliado` chama `Aplicador.ReceberDano(...)`, então prever chamando o modificador **feriria
um aliado de verdade**. A `IModificaDanoRecebido` passou a ter DOIS métodos (`Modificar` aplica,
`Prever` é puro), sem default — capacidade nova é obrigada pelo compilador a responder as duas.
Nasceram `Combate.PreverDanoRecebido`, `PreverAtaque` (crit como VALOR ESPERADO, senão a Kunai com
`forcaCritico` seria subestimada) e **`PreverVidaRemovida`** = `clamp(dano, 0, HPAtual − pisoDeHP)`,
que resolve sozinha "evitar bloqueio de dano" e "evitar Invencível" (ambos dão ~0) e ainda sinaliza
o ABATE (`== HPAtual`). Comportamento idêntico, provado pelos 14 `ReceberDanoTests` intocados.

### PR-B ✅ — o cérebro tático

**A habilidade é DADO, então dá pra lê-la sem executá-la.** Cada `Acao` passou a declarar duas
coisas sobre si: a `Utilidade` (que bem faz — FATO) e `TemEfeitoUtil` (tem trabalho a fazer agora?).
O cérebro nunca pergunta `is Dano` — seria reabrir o dispatch por tipo concreto que o #9 fechou.

- **Fila ABSOLUTA** (a única opinião tática, num array só):
  `Reviver > Curar > LimparDebuffs > Reforcar > TirarBuffs > Enfraquecer > TurnoExtra > Ferir`.
  Nada de pontuação: o Gabriel citou jogos onde score emergente virou estratégia degenerada.
  **TurnoExtra fica embaixo de propósito** — com ele no topo, o Copiando do Mímico dispararia sem
  buff nenhum pra roubar, gastando a habilidade pelo turno extra. A habilidade é julgada pelo RESTO.
  `Utilidade.Custo` (AutoDano do Fantasma) nunca conta: é preço, não entrega.
- **Desempates:** área > aleatório > único → mais ações úteis → mais vida removida.
- **Alvo (lexicográfico):** abate → menor punição → mais vida removida. O abate **não fura a fila**
  de habilidade.
- **`IPuneQuemAtaca`** (`Domain/Combat/`) — nova capacidade: `AplicaStatus` (Espinhos) >
  `ContraAtaca` > `RefleteDano`, na ordem de fuga. Não deu pra reusar `IReageAoSerAtacado` porque
  **reagir ≠ punir** (a passiva do Ogro reage se buffando, o que não custa nada a quem atacou). Só
  BUFF conta: passiva é identidade permanente e fugir dela deixaria Herói/Elfo/Zumbi/Cocô
  inatacáveis.
- **Achado durante os testes:** um `OfType<Dano>()` no cérebro lia a explosão como inofensiva (o bot
  preferia o A1 a detonar um alvo envenenado). Virou `Acao.PreverVidaRemovida` virtual, com
  `IStatusComTick.PreverDetonacao` como espelho puro do `Detonar` — mesmo par do PR-A. O dispatch por
  tipo concreto **volta a se infiltrar sempre que se pergunta "quanto isto faz?"** de fora da peça.

+20 testes headless. **É o primeiro pedaço do JOGO que roda sem tela:** decidir é puro, então dá pra
provar comportamento ("não cura quem está inteiro", "prefere o abate"), não só mecanismo.

### PR-C ✅ — o botão Auto no front

`🤖 auto` na barra da batalha, ao lado da velocidade. Espelha o Sair: flag `volatile` na ponte
(thread da UI escreve, thread do jogo lê). **Diferença que importa:** o `_sairPedido` é zerado pelo
`LimparPendentes()` a cada turno porque é um PEDIDO; o auto é ESTADO e tem que atravessar os turnos.

- **Lido no começo de cada decisão** — é isso que faz ligar/desligar valerem "entre turnos": o turno
  que o cérebro já começou termina, e o controle volta na PRÓXIMA pergunta.
- **A mensagem `auto` vira flag E entra na fila.** A fila é o que ACORDA um turno humano já parado
  esperando clique — sem ela, ligar o automático no meio da escolha travaria o jogo pra sempre.
- **O botão se desenha do `estado.auto`**, como todo o resto da tela. É o que o mantém honesto quando
  o C# desliga o modo sozinho — o que acontece a cada batalha nova (`DesligarAuto()` ao lado do
  `SessaoDoFront.Reiniciar()`), pra ninguém entrar numa luta sem o controle que achava que tinha.
- **Duas instâncias de `ControladorBot`**: o adversário e o automático do jogador. Mesmo cérebro,
  instâncias separadas — cada um memoriza o próprio alvo entre escolher-ação e escolher-alvo.
- **Ritmo NÃO muda** (é auto *assistido*, o ponto é ver acontecer) e o **Sair continua funcionando**
  com o automático ligado: ele é detectado na espera entre eventos, caminho independente do controlador.

---

## ✅ ORDEM DO PIPELINE DE DANO — bloqueio total desperdiçava o escudo (achado e CONSERTADO jul/2026)

**Era, reproduzido em jogo** (Rei + Operário): com `Escudo` e `BloqueioTotal` no mesmo alvo, o escudo
era CONSUMIDO mesmo com o dano sendo integralmente bloqueado — e dependia da ordem de APLICAÇÃO (escudo
aplicado antes gastava pontos; na ordem inversa sobrevivia). **Mesmo estado de jogo, resultado
diferente.** De quebra o `absorvidoPeloEscudo` reportava como "absorvido" o que foi bloqueado.

A `NaturezaDano` não resolvia: ela declara **quem participa** (a lista `Ignora`), nunca **em que
ordem**. O conserto **generalizou o princípio que já existia e estava testado** pra passiva-pura
(`PassivaPura_RodaANTESDosStatus_EOEscudoVeODanoJaReduzido`): quem REDUZ de graça roda antes de quem
GASTA recurso.

**Feito:** `IModificaDanoRecebido` ganhou **`OrdemDeMitigacao`** (enum `ReduzDeGraca`/`ConsomeRecurso`,
Combat/) — **sem default**, como os 2 métodos que a interface já tinha: um modificador novo que
herdasse "de graça" em silêncio desperdiçaria recurso alheio sem ninguém notar. A ordem virou
CAPACIDADE DECLARADA, não `OfType<Escudo>()` no meio do cálculo (o dispatch por tipo concreto que o #9
e o #181 tiraram do motor). `ReceberDano` e `PreverDanoRecebido` passaram a ordenar pelo helper
compartilhado `Combate.OrdenarPorMitigacao` — os dois juntos de propósito: se divergirem, o bot mira
errado em silêncio. `OrderBy` é **estável**, então dentro do mesmo balde a ordem de aplicação segue
valendo (Escudo × ProtecaoAliado não têm ordem "certa" entre si, e não foi este PR que inventou uma).

**Classificação:** `BloqueioTotal` + `ReducaoDanoFixo` + `Aquagirl` (passiva) = `ReduzDeGraca`; `Escudo`
(pontos) + `ProtecaoAliado` (HP do aliado) = `ConsomeRecurso`. **`Invencivel` NÃO entrou**: o #151 já o
tinha movido pra `IDefineHPMinimo` (piso de HP ≠ mitigação) — o grep pegava só o comentário histórico.

**Carona:** o `ProtecaoAliado` parou de cobrar do protetor quando o bloqueio já zerou o dano (antes o
aliado comia 30% de um golpe que não ia acontecer).

**Números mudaram, era o esperado** (é o motivo de ter vindo ANTES do rebalance, não junto): escudo +
redução contra 1000 com Escudo 400 e −15% dá 450, não 510. +4 testes (123 → 127): o par de ordens
nos dois sentidos provando que a ordem de aplicação deixou de importar, o Prever amarrado ao Receber, e
o protetor. **Verificados falhando sem o conserto** (3 dos 4 — o 4º é o controle: a ordem que já
funcionava).

---

## ✅ BANCADA DE DANO — o instrumento do REBALANCE (#16) (jul/2026)

A dor que o Gabriel nomeou: pra saber qual habilidade está quebrada, teria que jogar 36 champs × ~4
habilidades à mão. A bancada roda isso sozinha e escreve **`docs/bancada-dano.md`**, VERSIONADO — cada
tweak de número vira um `git diff` legível. A entrega é o RELATÓRIO, não ajustar valores.

**Ela só foi possível porque habilidade é DADO.** O `Detetive.Espionagem()` é uma fábrica PRIVADA — a
bancada nunca a chama e nem poderia. Mas o `Definir()` já executou a fábrica e guardou a instância em
`Personagem.Habilidades`, então varrer os 36 champs é um `foreach` sobre `TodosOsCampeoes()`. Se as
habilidades fossem métodos, seria reflection ou uma lista de 144 nomes escrita à mão, que envelheceria
no primeiro champ novo. **O refactor pra dados pagou por si aqui.**

**Colunas** (pedido do Gabriel na 2ª rodada): além de `Usos`/`Dano`/`Dano por uso`, cada linha por
habilidade traz **`Dano (4 alvos)`** — a mesma medição com 4 bonecos, que é o que dá voz às habilidades
de ÁREA (contra alvo único elas ficam indistinguíveis de single-target; agora medem 4,0× exatos,
enquanto as de alvo único seguem 1,0×) — e **`Cura`**, que exigiu uma condição nova: **o champ começa
cada turno com 1 de vida.** Sem isso a coluna seria toda zero (cura não cura quem está cheio). De
quebra é a condição em que aparece quem fica mais FORTE ferido: a Caveira escala `2.0 − HP%` e o
Ossinho dela mede 638 = 200 × **1,99** × 1,6. O champ carrega a mesma prevenção-de-morte do boneco pra
não morrer de auto-dano em 1 de HP. **O HP virou IGUAL nos dois lados** (2.000): cura costuma ser % do
HP máximo, então inflar o champ estouraria a cura pelo mesmo motivo que inflar o boneco estourava o DoT.

**Duas vistas dos mesmos dados:** a tabela agrupada por champ responde "como é o kit deste
personagem?"; os **rankings** no fim (burst, sustentado com área, cura) respondem "quem está fora da
curva?" sem obrigar a varrer 144 linhas com o olho.

**Cinco linhas, variando UM fator por vez** (desenho do Gabriel) — é o que torna as subtrações legíveis:

| # | Modo | DEF do alvo | Recebe malefício? | O que a subtração isola |
|---|---|---|---|---|
| 1 | por habilidade | 0 | não | dano cru |
| 2 | por habilidade | cap | não | **(2)−(1) = o que furar/reduzir DEF vale** |
| 3 | champ inteiro | cap | não | **real − esperado = a SINERGIA do kit** |
| 4 | champ inteiro | cap | **sim** | **(4)−(3) = o que os malefícios valem** |
| 5 | por habilidade | cap | **sim** | **(5)−(2) = de quem é o mérito do malefício** |

**Zero mudança no motor.** Tudo saiu de seam que já existia: o horizonte de 100 turnos é o controlador
devolvendo `null` na 101ª chamada (cai no `BatalhaAbortada`); a imunidade do boneco é uma passiva
`IBloqueiaStatus` (o MESMO mecanismo da `CascaDura`); o crítico 100% é um `BuffTaxaCrit` de duração
infinita; e o `ExibirInicioArena` da porta de tela é o que entrega os `Combate` construídos lá dentro.

**Decisões que os NÚMEROS forçaram (as primeiras versões estavam erradas — rodar o instrumento É o
que o valida):**
- **Boneco com HP gigante não funciona.** A `Queima` tira **5% do HP máximo** por turno — com 100M de
  HP o tick virava 5 milhões e o boneco se matava (o Mago aparecia com 7 usos em vez de 25). O HP tem
  que ser REALISTA; o boneco volta ao HP cheio entre turnos.
- **Reset entre turnos não salva de quem mata DENTRO de uma ativação.** O Porradeiro do Troll dá 6
  hits de 480 = 2880 num boneco de 2000: matava no 5º e a corrida parava com **1 uso em vez de 25**.
  Solução do Gabriel: dar ao boneco a **prevenção-de-morte do Guarda Real** (`IPrevineMorte`,
  consultada pelo `ConfirmarMorte` dentro do funil), com duas mudanças — restaura o HP **cheio** em
  vez de 1, e **cooldown 0**, que o `SkillCooldown` traduz em sempre-disponível (`Usar()` faz
  `restante = total = 0`), inclusive entre hits da mesma ativação. **Melhor que pôr `Invencivel`:**
  com piso de HP o alvo ficaria em 1 de vida e o próprio bot documenta que "evitar Invencível cai
  sozinho de `PreverVidaRemovida`, que devolve ~0" — ele leria o boneco como inútil de bater. Voltando
  ao HP cheio, a previsão segue honesta e a regra vale pras CINCO linhas, uniforme.
- **O boneco não pode nem ATACAR de mentira.** Dar-lhe ataque 0 não basta: um golpe de dano zero ainda
  dispara `IReageAoSerAtacado`, e a bancada passava a medir a passiva reagindo ao próprio andaime. O
  Troll terminava 25% mais forte (a Ambição conta as pancadas do saco) e a **Parede de Tijolos do
  Operário — que não causa dano nenhum — media 235 por uso**, que era 100% contra-ataque. Agora o
  boneco **se cura** no turno dele (ideia do Gabriel: ação de jogo de verdade em vez de turno oco).
  Depois disso todo número bate com a conta à mão: Porradeiro 2880 = 6 × (200 × 1,5 × 1,6), Pancada
  560 = 200 × 1,75 × 1,6, A1 320 = 200 × 1,6.
- **Sinergia não é "combinado − soma dos isolados".** Cada isolado gastou 100 turnos SÓ naquela
  habilidade; somar N deles e comparar com UMA corrida de 100 turnos é laranja com maçã (dava negativo
  pra todo mundo). O certo é **real − esperado**, com esperado = dano-por-uso isolado × ativações que
  de fato aconteceram.
- **Durante o cooldown o champ ESPERA, não usa A1.** Se enchesse o buraco com A1, toda habilidade
  carregaria ~75 ataques básicos junto e todas ficariam parecidas.

**Validação:** o A1 mede 320 = ATK 200 × 1,60 (o `DanoCritBase`), provando que o crítico está cravado.
E a história do Mago sai decomposta: Bola de Fogo isolada com alvo imune = 4000; com malefício = 11500
(7500 são o tick da Queima); o champ inteiro salta de 11000 pra 19750, e o ~1250 que sobra é a passiva
Piromancer — que só rende quando OUTRA habilidade bate no alvo já queimado, e por isso o isolado nunca
a veria.

**Limitação declarada no próprio relatório:** o boneco **nunca age**, então contra-ataque, espinhos,
revide e passivas de apanhar (Herói, Operário, Zumbi, Troll) medem ZERO. É bancada de dano CAUSADO, não
de duelo — champ com número baixo pode ser reativo, não fraco. A coluna **Usos** é diagnóstico do BOT:
habilidade que dispara 0× no champ inteiro mas pontua alto isolada acusa a fila do bot, não o balanço.

Roda em ~37s dentro do `dotnet test`.

---

## ✅ PROTEÇÃO DE ALIADO — a DEF do protetor abate o redirecionado (jul/2026)

Continuação direta da seção acima: ao perguntar "qual o melhor teste pro `ProtecaoAliado`", a leitura
achou uma **contradição entre doc e implementação**. O doc da classe dizia que os 30% iam pro aplicador
*"(sem defesa)"*; mas o redirecionamento viaja como `NaturezasDano.DanoIndireto`, que tem
`IgnoraDefesa: false` — ou seja, passa pela defesa E pelo escudo do protetor. **Mesmo formato do bug do
Invencível (#151), onde o doc do buff dizia uma coisa e a impl fazia outra.**

**Decisão do Gabriel: a IMPLEMENTAÇÃO é a certa** — "se o tanque tem def alta ele obviamente defende
mais". Proteger sai mais barato pra quem aguenta, que é a fantasia do personagem que se põe na frente.
Corrigido o DOC, fixada a regra em teste, **nenhum número do jogo mudou**. O que o PROTEGIDO desconta
não muda com isso: são sempre os 30%, não importa quem o protege.

**Por que estava invisível:** todo teste de proteção dava **def 0 ao protetor**. A regra existia e
ninguém a exercitava.

**Carona — a conta estava duplicada à mão** no `ModificarDanoRecebido` e no `PreverDanoRecebido`
(`(int)(dano * Valor)` copiado nos dois). Virou um `Redirecionado(dano)` privado compartilhado, pelo
mesmo motivo que fez o `OrdenarPorMitigacao` nascer no PR acima: duas cópias da mesma fórmula divergem
caladas e o bot passa a mirar com um número que o golpe real não reproduz.

**+8 testes (127 → 135):** a DEF do protetor nas 3 faixas (0 → paga 300; 500 → 187; 2000, além do cap →
75); o protetor MORTO (único ramo dos dois métodos que ninguém exercitava — há janela real, o status só
se autoremove na expiração do turno seguinte); e o round-trip Prever×Aplicar.

**MÉTODO, de novo:** o round-trip passa hoje por construção, então ele foi **verificado sabotando** o
`Prever` com `Math.Round` — os 4 casos novos falharam e os **outros 7 testes de proteção passaram
tranquilos**. Isso É o furo que ele fecha: o resto da suíte usa número REDONDO (`1000 × 0,30 = 300`
exato), então um arredondamento trocado passaria verde. Os casos novos usam parte fracionária ≥ 0,5
(`999 × 0,30 = 299,7`).

---

## CADA DECISÃO NA SUA CAMADA (#180, jul/2026)

Com o console fora, uma auditoria do front achou 7 decisões na camada errada. A lição geral: **o
vazamento não é teórico — ele APODRECE.** A prova foi o nome do slot do arsenal, duplicado no front
e no `ArsenalService`, que já tinha divergido (a tela dizia "Acessório", o item que cai nela nasce
"Manopla"); a tela mostrava um nome que o item não tem.

**Front decidindo regra → devolvido ao service:**
- `ArsenalService.NomeDoSlot(fase)` — nomeia o slot E o item que cai nele, uma tabela só (a tela
  precisa nomear slot VAZIO, que é por isso que ela tinha a cópia). Teste trava os dois juntos.
- `CapitulosService.FaccoesDaCampanha()` — o mapa É a lista de capítulos, na ordem. O front deduzia
  ("todas as facções menos Humanos") e acertava por coincidência da ordem do enum.
- `CampanhaService.PosicaoNoMapa()`/`SalvarPosicao()` — o "último lugar" é PROGRESSÃO. O front
  gravava direto na porta de save (única gravação do jogo fora de um service), enquanto o
  `PerfilService` já apagava a mesma chave no wipe de conta: dois donos.
- `ArsenalService.EquiparItem` **persiste sozinho** — quando havia duas cascas, cada uma escolheu
  sua política de quando salvar. Quem manda no dado decide quando ele é durável.
- `PerfilService.AvatarInicial()`/`PodeUsarAvatar()` — a cara do jogador é troféu de campanha. O
  front segue validando o clique, mas como FRONTEIRA, não como fonte da regra.

**Motor decidindo pele → devolvido à tela:**
- `Item.ValorFormatado()`/`NomeStat()` **saíram do Domain**: `:F0` e sufixo `%` são exibição. O Item
  guarda `Valor` + `TipoStat`; cada pele escreve do seu jeito.
- **`IApresentacao.AguardarAnimacao(Momento)`** no lugar de `(int ms)`. O motor mandava `1500` em 10
  lugares e a pele *dividia* o número pra corrigir — sintoma de quem não devia escolher, escolhendo.
  Agora o motor diz a BATIDA (`Tick`/`Narracao`/`Golpe`/`Preparacao`) e a pele dá a duração (todas em
  1500 hoje, de propósito: mudou o dono, não o sentimento). **É o seam do MODO AUTOMÁTICO** — uma
  pele que devolve ~0 e o motor não sabe de nada.

**NÃO mexido, porque está certo:** a validação duplicada (front valida pra UX, back valida porque não
confia na tela) e a `SessaoDoFront` inteira (ids, lado esquerdo/direito, o `_mostrado` que segura a
barra de vida até o número ser narrado) — presentation pura, no lugar certo.

**Dívida registrada, não paga — emoji no Domain.** `Personagem.Simbolo`, `Habilidade.Simbolo`,
`Item.Simbolo` e `Faccoes.simbolos` são RENDER dentro do domínio de regras. Fica como está: pagar
isso hoje custa tocar os 36 champs e todas as skills por uma dor que só chega **quando os sprites
entrarem** — aí o emoji deixa de ser "o visual" e vira "um dos visuais", e o Domain vira o lugar
errado pra ele. Gatilho nomeado, sem data.

---

## OS FIOS QUE FALTAM (visão de alto nível)

Resumo do que resta no combate. **C5 COMPLETO** (todas as passivas migradas, sistema
velho aposentado). O que resta:

1. **EventoDano / contexto rico** — Fatia 1 e Fatia 2 FEITAS. Falta só a passiva-conta-mortos
   (1b) como cliente futuro do contexto rico.
2. **Estado de Vida (Vivo/Morto) + Atos do turno** — ✅ **Passos 1-5 FEITOS** (State Pattern,
   status no Morto, Atos, Guarda limpa, seleção de alvo por estado — PR #111). Falta só a
   passiva-conta-mortos (1b), que é do EventoDano.
3. **Turno (resto)** — ✅ **FEITO** *(verificado no código em ago/2026; o texto abaixo estava velho e
   me fez listar como pendente algo pronto)*. O reset 1x-por-agressor do CONTRA-ATAQUE veio no #112, e
   as OUTRAS reações também já usam o mesmo orçamento: `TurnoDoPersonagem.TentarReagir(chave, agressor,
   chance)`, chamado pelo `EspinhosVenenosos` e pelo `Fedorento` do Cocô, coberto por
   `ReacaoPorAgressorTests.cs` (mesmo agressor não repete no turno · agressor diferente dispara · chave
   diferente dispara · vira o turno e reabre). O **Zumbi saiu da lista** porque não tem mais reação de
   apanhar: a `PutrefacaoContagiosa` morreu no #12 e ele ganhou a Horda (`EscalaComMortos`). E o
   **`TimeAtualDoTurno` não vai existir** — ele foi substituído no #11 pelo `Batalha.PerspectivaDe`,
   que deriva aliados/inimigos da ESTRUTURA (qual equipe) em vez de guardar times no combatente.
4. **Buff-permanente vs passiva-pura** — ✅ **FEITO** (#111/#112): 6 passivas puras + Fantasma
   (Removivel=false). Ver seção própria (marcada concluída).
5. **Composição de Ações + Motor de Habilidades** — ✅ **SWEEP CONCLUÍDO** *(verificado no código em
   ago/2026; o texto que dizia "agora: sweep por facção" era resíduo do meio da migração)*. Habilidade
   é DADO (lista de Ações) rodada por um interpretador único. A conferência: os **76 arquivos de champ
   estão todos em `Champs/<Faccao>/<Champ>/`** (nenhum solto), e os únicos `override Ativar` que
   restam são **8 arquivos `.Passiva.cs`** devolvendo `SemDano()` ou um efeito próprio — que é a forma
   NORMAL de passiva, não sobra de ativa. **Nenhuma habilidade ATIVA tem `Ativar` override.**
   Ver **ADR-composicao-de-acoes.md**. Era predecessor do Rebalanceamento, e por isso o #16 pôde
   começar: mexer em número virou editar dado.
6. **Rebalanceamento** — design de jogo (Sereia A3, Morcego→Vampiro, durações). FASE própria,
   pós-composição.

A **unificação dos mecanismos de ignorar** ✅ CONCLUÍDA (jul/2026) — a natureza virou lista, o
`DeveAgir` morreu, 1 gate só. Ver a seção própria abaixo.

---

## Composição de Ações + Motor de Habilidades (MOTOR FEITO — SWEEP POR FACÇÃO)

> **Índice das Ações (o que já existe pra reusar): `CATALOGO-de-acoes.md`.** Ler antes de criar
> habilidade nova — verbo compartilhado 1º, promover bespoke no 2º cliente, não duplicar.

**Status:** MOTOR IMPLEMENTADO (#116, verificado em jogo: Furtividade/Sushi/Prender + regressão
do Mago) e **forma-construtor + champ-como-arquivo FEITOS** (Mago piloto em `Champs/Reino/Mago/`).
Ver **ADR-composicao-de-acoes.md**. É a "Auditoria das ativas" com dor real: ~70% das ativas
são só dado (loop + lista fixa de efeitos), reinventando boilerplate. Predecessor do
Rebalanceamento (mexer em número/efeito vira editar dado, não 74 classes).

**Decisões novas (jul/2026, pós-motor):**
- **Fusão do Nível A no sweep:** cada PR de facção migra os champs direto pra FORMA FINAL
  (pasta `Champs/<Faccao>/<Champ>/`, habilidades como métodos `static HabilidadeAtiva X() =>
  new(...)`, passiva movida junto, classes velhas deletadas, linha do `PersonagemService` vira
  `Champ.Definir()`). Uma passada por champ em vez de duas — e a VIEW do champ chega facção a
  facção. O `PersonagemService` encolhe até virar o `Roster`.
- **Família do revive mapeada (7 clientes de `Reviver`):** Nigiri, Céu, Tecnology, AnjoCaído,
  DocesDeAbobora (revive SÓ 1), Circo (Intocável exceto self) e Atlantis (Intocável só nos
  revividos — pipeline, segue 1 cliente, bespoke). `Reviver` precisa de percentualHP + quantos.
  Todos os 7 são os únicos usuários de `EstadoAlvo.Ambos` (+ 1 check no CombateService) — o
  `Ambos` morre quando o 7º migrar.
- **`Escopo.OutrosAliados` tem 2 clientes** (OssoDuroDeRoer + Circo) — entra no vocabulário
  quando o 1º migrar.
- **Testes do motor ANTES do sweep — ✅ FEITO** (projeto `Tests/`, xUnit, 10 testes verdes):
  escopo (próprio/todos-inimigos/todos-aliados), EstadoAlvo na execução (recém-morto pulado /
  Mortos pega recém-morto / Ambos sem filtro), agregação+ordem (PorDanoCausado lê o eventos
  completo), fragmento PorHP com cap, Aleatorio com duplicata, Strangler (Democracia override).
  Rede de regressão de cada PR de facção: `dotnet test`. O "xUnit do JOGO" continua pra depois.
- **Regra de processo:** todo PR de código que fecha um marco carrega o bump do ROADMAP/ADR
  NO MESMO DIFF. Chega de PR de docs correndo atrás (drift aconteceu em #115 e #116).

**Núcleo:** um interpretador ÚNICO (`HabilidadeAtiva.Ativar`) roda uma lista de **Ações**;
**nenhuma habilidade sobrescreve `Ativar`**. Três níveis, todos pelo motor: (1) vocabulário
puro, (2) vocabulário + 1 ação custom, (3) ação custom inteira — "único" vira uma `Acao`
especial, não uma habilidade especial. A unidade de reúso é o **FRAGMENTO** (correção: NÃO a
Ação inteira — Cura/Escudo compartilham o fragmento de valor e diferem só no verbo).

**O motor (detalhe no ADR):**
- **Loop-flip:** ação-por-fora; cada ação resolve seu **Escopo** + **EstadoAlvo** no momento
  em que roda. Isso dissolve "escopo próprio" e "condição de estado" — não eram paredes.
- **`AcaoSobreConjunto`:** 2º formato de ação (recebe o conjunto inteiro) pra agregação
  cross-alvo. CONSTRUÍDA E REMOVIDA no sweep LadoSombrio (ADR §3.4) — o único cliente (média
  da Putrefação) morreu no rebalance (cura por dano total = `PorDanoCausado` lê o `eventos`).
  Desenho registrado; reconstrói se agregação real aparecer (candidata: Atlantis §8.1).
- Ações ORDENADAS; cada uma vê o estado da anterior (AnjoCaído: revive→cura os revividos).
- `EstadoAlvo` DESCE pra ação, avaliado NA EXECUÇÃO → o `Ambos` MORRE; a categoria "ao-matar"
  se dissolve no fluxo normal (Sentença = `AplicarDebuff(Mortos)`).
- Eixos da ação: **Operação × Escopo × EstadoAlvo × Valor(fragmento) × Seletor**.
- Vocabulário mapeado: Dano, Cura, AplicarEscudo, AplicarBuff, AplicarDebuff, Reviver,
  RemoverBuffs, RemoverDebuffs, MoverBuffs, ConcederTurnoExtra, **Explodir** (+ `IStatusComTick`,
  `Seletor`). Implementados: Dano/Cura/AplicarEscudo/AplicarBuff/AplicarDebuff/Reviver/
  RemoverBuffs/**Explodir** (genérico, `Seletor` + `IStatusComTick.Detonar → EventoDano`;
  1º cliente Putrefação; Inferno no shim até Decaídos)/IStatusComTick/Seletor. Faltam:
  RemoverDebuffs, MoverBuffs, ConcederTurnoExtra. *(Vocabulário esgotado desde os Apóstolos.)*
- **Toda `Acao` declara `Utilidade` + `TemEfeitoUtil` + `PreverVidaRemovida`** (jul/2026, PR do bot):
  o que ela FAZ, se tem trabalho agora e quanto machuca — tudo respondido pela própria ação. É o que
  permite AVALIAR uma habilidade sem executá-la. `Utilidade` é abstrata: ação nova (inclusive bespoke
  de champ) é obrigada pelo compilador a se classificar, em vez de o avaliador dar `switch` no tipo.
- Disciplina: promove no 2º cliente REAL; verificar-antes-de-fundir (o grep mente — **Copiando
  era Balde 3 e é vocabulário puro**; **Atlantis** revelou o boundary de "pipeline / conjunto
  afetado", 1 cliente, registrado sem construir).
- Invariantes: `TipoAtaque` alimenta dispatch de passivas-atacante; o interpretador agrega os
  `EventoDano` das ações de dano.

**Sequência:** #115 piloto per-alvo ✅ → #116 motor (loop-flip) ✅ → #117 forma-construtor +
Mago champ-arquivo + rename passivas ✅ → #118 testes do motor ✅ → **Humanos ✅** (4 champs na
forma final em `Champs/Humanos/`; `Reviver` nasceu no Nigiri — 1º da família dos 7; Marretada
é a 1ª híbrida `.Ativa.cs`; o Nigiri deixou de usar `Ambos`) → **Reino ✅** (Guarda/Ninja/Rei
migrados em `Champs/Reino/`, ao lado do Mago piloto; `AplicarEscudo` nasceu Ação de
vocabulário — Lealdade, já estava mapeada em §5.1 (como "Escudo") mas sem cliente até agora
(nome `AplicarEscudo`, não `Escudo`, pra não colidir com `Skills.Buffs.Escudo` — o namespace
raiz `ApostlesWar` é envolvente de quase todo o código); `Dano` ganhou
`ignorarDefesaPct`/`forcaCritico` opcionais — Kunai; Shuriken estreou a 1ª Ação bespoke Nível 3,
`GolpeSeguidor`, acoplamento hit-a-hit lido via `eventos`) → **LadoSombrio ✅** (Caveira/
Fantasma/Abóbora/Zumbi migrados em `Champs/LadoSombrio/` — momento de design, estreou 4
mecanismos novos do motor, com duas rodadas de revisão de Gabriel por cima do sweep:
**regra do revive firmada** (ADR §9): `Reviver` per-alvo só com `percentualHP` — revive-de-N
usa o pick do motor (habilidade declara `numeroDeAlvos: N` + `TipoAlvo.Aleatorio` +
`EstadoAlvo.Mortos`, ação herda `AlvosResolvidos`; selecionado + extras sorteados).
**DocesDeAbobora** (2º da família dos 7) é o 1º revive-de-N com pick REAL de morto (a dor do
"primeiro da lista" do ADR-selecao-por-estado morreu); `CombateService` ganhou guard pra pick
sem candidato (revive sem mortos ainda vale pelo Reflexo). **Rebalance da Putrefação** (cura
20% do dano TOTAL, não média — a cura é EXTRA da hab, ação separada): matou o único cliente da
`AcaoSobreConjunto` (construída e removida no mesmo sweep) e fez nascer o **`Explodir`
genérico** (molde único das explosões: `Seletor.Tipo<Veneno>()` hoje, `Seletor.Tipo<Queima>()`
quando o Inferno migrar em Decaídos; `IStatusComTick.Detonar(portador, detonador)` devolve o
`EventoDano` — a explosão aparece na exibição, conta no `PorDanoCausado` e morte-por-explosão
passa pelos Atos de morte, furo antigo fechado; Inferno segue no shim `Queima.Explodir`);
**`Escopo.OutrosAliados`** real, 1º cliente OssoDuroDeRoer (Circo é o 2º, Folclore);
**`RemoverBuffs`/`Seletor`** reais, 1º cliente DocesOuTravessuras. De quebra, `AplicarBuff`
ganhou a sobrecarga `Func&lt;Combate,Buff&gt;` pra buffs com proveniência
(ProtecaoAliado.Aplicador). 14 testes xUnit (3 novos: OutrosAliados, revive-de-N via pick,
Explodir + cura-por-dano)) → **Tecnológicos ✅** (Invasor/Alien/Robô/Cientista — Barata estreou
estado/ao-matar via `Dano`+`AplicarDebuff(Mortos)`, sem condicional; Tecnology 3º do Reviver;
`EstenderBuffs` bespoke-local no Robô/RaioX, espelho do `RemoverBuffs` (§9); Galáxia = novo
cliente de `OutrosAliados`; `EstenderTurno`→`AumentarDuracao` consolidado; princípio DECOMPOR
firmado — ADR §3.3) → **Folclore ✅** (Ogro/Tengu/Palhaço/Troll — `RemoverDebuffs` nasceu [Coringa,
gêmeo do RemoverBuffs]; `Dano`+`ignorarStatus` [CorteDeVento/Vendaval]; `AplicarDebuff`+`chance`
[Pancada] + overload de proveniência [Irritar/Quebrar]; Circo 4º do revive + cliente de
`OutrosAliados`; Porradeiro = molde do Tiroteio + cura do Zumbi; ZERO bespoke) → **Místicos ✅**
(Gênio/Sereia/Fada/Dragão — pipeline §8.1 DISSOLVIDO: `Reviver` ganhou `buffNoRevivido` [Intocável só
nos revividos], fez o Atlantis (5º do revive) e CONSERTOU o Circo (bug: pegava todos os outros vivos);
PoMágico = vocabulário puro [`ignorarStatus` casa por tipo-BASE, `typeof(Buff)`=todos os buffs];
`RestaurarHPMaximo` bespoke no Dragão; unificar-ignorar fica pra tema próprio no Vampiro/Decaídos) →
**Especial ✅** (Cocô/Herói/Vilão/T-Rex — 100% vocabulário puro, ZERO bespoke, 1ª facção totalmente
mecânica; DestruindoDia = 2º cliente do `RemoverDebuffs`, SalvandoDia = mais um de `OutrosAliados`) →
**Decaídos ✅** (Morcego/Vampiro/Elfo/Diabo — 100% vocabulário puro, ZERO bespoke; `ConcederTurnoExtra`
construído [1º cliente = Rato Voador, não o Copiando]; Inferno migrou pro `Explodir` genérico e o shim
`Queima.Explodir` morreu [explosão agora entra no pipeline]; Anjo Caído = 6º do revive
[`RemoverDebuffs`(Sentença,Mortos)+`Reviver`+`Cura`, a ordem quebra a Sentença antes de reviver];
renomes do Vampiro: "Controle de Sangue" 🩸 + "Vampiro Primordial" 🌙; colisão "Espinhos" resolvida
[passiva do Elfo → `EspinhosCorrompidos`]; unificar-ignorar NÃO feito aqui — vira PR próprio a seguir) →
**Apóstolos ✅ — SWEEP DAS 9 FACÇÕES COMPLETO** (Boneco de Neve/Mímico/Anjo/Papai Noel — 100%
vocabulário puro, ZERO bespoke; `MoverBuffs` construído [gêmeo do RemoverBuffs, cliente Copiando] e
com ele o vocabulário mapeado esgotou; Imitação = `Dano(Func)` [molde Tengu]; Céu = 7º/último do revive
e último champ com `Ambos` → agora NENHUM champ usa `Ambos`; fio §9 fechado com Repetindo deixada como
está [3ª de 3, igual AnáliseCrítica/Policial]) → **sweep segue** (unificar-ignorar → pick do menu/§8.2
quando o `Ambos` morrer). Revive 7/7 (Nigiri, DocesDeAbobora, Tecnology, Circo, Atlantis, AnjoCaído,
Céu). Quando uma facção ESTREIA um mecanismo, o champ é momento de
design (verificar em jogo com cuidado extra), não sweep mecânico.

---

## C5 — padrão de reações das passivas (✅ COMPLETO)

**Status:** ✅ COMPLETO. Todas as 36 passivas migradas do DeveAtivar/enum para o modelo
de interfaces (IReageAo*), ao lado dos buffs reativos (PR-C). Strangler Fig concluído:
nenhuma passiva usa mais o sistema velho. A Guarda foi a última (migrada pro IReageAoMorrer
com hack provisório — ver "Estado morto" / [sistema-morte-como-estado]).

**PRÓXIMO (consequência direta):** aposentar o sistema velho — agora ÓRFÃO. Remover
ExecutarPassivasReativas, HabilidadePassiva.Revive()/MensagemSobreviveu/MensagemMorreu/
DeveAtivar, o enum EventoCombate, DispararEvento/DispararEventoInicioDeTurno (a parte velha).
Confirmar que tudo está sem uso antes de remover. Branch de limpeza (junto da exclusão de
documentação/ADRs mortos).

### As 36 passivas — mapa de status

**JÁ MIGRADAS pro modelo de reação (17):**
- Lado "ao ser atacado" (IReageAoSerAtacado): Zumbi, Coco, Palhaco, Cientista,
  Mimico, Ogro, PapaiNoel, TRex, CoroaDoSoberano, Ambicao, Diabo.
- Lado atacante: OlhoClinico, Virus (IReageAoAtacar — 1x por ataque, segue TipoAtaque);
  Sorrateiro, Policial (IReagePorAtaque — Nx por alvo atingido).
- Ao matar (IReageAoMatar): Fada, Vilao.

**Fora do C5 — outro padrão, JÁ PRONTAS, NÃO migram (10):**
- IPassivaInicial (aplicam buff no IniciarCombate, o buff é que reage): Fantasma,
  Tengu, Heroi, Morcego, Sereia, Dragao, Elfo, Anjo.
- Interface própria não-reativa (consulta direta, sem evento): Piromancer (MultExtra
  no cálculo de dano), Vampiro (IIgnoraStatusNoAtaque no Atacar).

**FALTAM migrar: NENHUMA (C5 completo).** A Guarda foi migrada pra IReageAntesDeMorrer
(hack provisório removido — ver Passo 4 no CONCLUÍDO). O Operario perdeu o provisório
do revide (ver "Fio do revide" no CONCLUÍDO) — hoje declara Revide igual ao ContraAtaque.

### Interfaces de reação — estado
- IReageAoSerAtacado, IReageAoReceberDano, IReageAoCausarDano — existem (PR-C).
- IReageAoAtacar (1x por ataque, segue TipoAtaque), IReagePorAtaque (Nx por alvo),
  IReageAoMatar — CRIADAS.
- IReageAoMorrer (pós-morte, Necromancia), IReageAoInicioTurno (início de turno, recebe
  ContextoCombate — Genio/BonecoDeNeve/Tengu/Elfo) — CRIADAS.
- IReageAntesDeMorrer (pré-morte, gancho de morte-iminente) — CRIADA (Passo 4).

### Dois sabores do lado atacante (decisão firmada)
- **IReageAoAtacar** = efeito no PRÓPRIO atacante. Segue TipoAtaque: AoE = 1x, Sequencial
  = por hit. Lado a lado com ProcessarPassivasAtacante. [OlhoClinico, Virus]
- **IReagePorAtaque** = efeito POR ALVO atingido. Nx sempre. Dentro do foreach. [Sorrateiro,
  Policial]
ProcessarReacoesAtacante dividido em PorAlvo (dentro do foreach) e PorAtaque (segue
TipoAtaque). Ver "Dívidas" — a repetição do loop vira helper ColetarReacoes<T>.

### Ordem crítica preservada (morte/revive)
**ATUALIZADO (jul/2026 — fix do bug do Guarda):** prevent-death (`IPrevineMorte`, no `ConfirmarMorte`
dentro do `ReceberDano`) → IReageAoMatar (Vilao) → IReageAoMorrer (Necromancia). O Guarda **EVITA a
morte** (não reverte): consultado como CAPACIDADE no instante do golpe fatal, o portador segue Vivo
**com os status intactos** (nunca vira Morto). Se não previne, o Vilão bloqueia o revive antes da
Necromância tentar. **Bug corrigido:** antes o Guarda usava `AplicarRevive` (Vivo novo) e perdia todos
os debuffs/buffs; `IReageAntesDeMorrer` (só o Guarda implementava) foi REMOVIDA junto — código morto.
Ver ADR-estado-de-vida-e-atos §11.

### Aposentar o sistema velho
DeveAtivar/Ativar virtual e o enum EventoCombate só saem quando a ÚLTIMA passiva migrar.

---

## Buff-permanente vs passiva-pura vs buff-não-removível (FIO NOVO — descoberto ao migrar início-de-turno)

**Status:** ✅ **FEITO** (PRs #111/#112). Os 6 viraram passiva-pura (capacidade direta) e o
Fantasma ganhou `Removivel = false` (segue buff, só protegido). Herói veio junto no #112. O
texto abaixo fica como registro da decisão. Gabriel
identificou: vários personagens aplicam um BUFF PERMANENTE (int.MaxValue) via IPassivaInicial
"pra contornar" — quando o certo seria a passiva SER a capacidade diretamente. Foi a gambiarra
que originou a refatoração. Três categorias distintas (não "buff vs passiva"):

1. **Buff permanente NÃO-REMOVÍVEL** — só o **Fantasma** (Intocável permanente que NÃO deve
   ser dispelável). Hoje é buff comum (removível). Falta o conceito "buff não-removível" —
   provavelmente uma flag `Removivel = false` no StatusEffect (pequena, geral), em vez de
   converter pra passiva pura. O Fantasma continua com buff, só protegido de remoção.
2. **Passiva PURA** (a passiva É a capacidade, sem buff intermediário) — **Abóbora**
   (IBloqueiaStatus, hoje ImunidadeDebuffs), **Dragão** (bloqueio Veneno/Queima, hoje
   ImunidadeEspecifica — pode deletar o buff depois), **Herói** (IReageAoSerAtacado /
   contra-ataque, hoje ContraAtaque), **Morcego** (IReageAoCausarDano / cura 15%, hoje
   Sedento), **Anjo** (IReageAoInicioTurno / cura 5%, hoje CuraContinua), **Sereia**
   (IModificaDanoRecebido / -15%, hoje ReducaoDanoFixo). Cada um passa a implementar a
   interface da sua capacidade direto — é o modelo de capacidades do ADR. É DOR (o buff é
   contorno), não pureza. Os buffs cuja MECÂNICA Gabriel gostou (ReducaoDanoFixo "Couraça",
   Sedento) ficam pra reuso em habilidades ativas (ver Rebalanceamento), só somem de serem o
   meio da passiva.
3. **Reaplica buff no início do turno** — Tengu, Elfo (FEITO — viraram IReageAoInicioTurno
   2t/turno), Genio (já era). Categoria resolvida.

Agrupar B (passivas puras) numa branch; o Fantasma (C) pode ir junto ou virar detalhe.

---

## Rebalanceamento de personagens (FASE PRÓPRIA, pós-estrutura)

**Status:** BACKLOG DE DESIGN (não refactor). Só DEPOIS do C5/estrutura estabilizar — mudar
comportamento e estrutura juntos esconde bugs. Ideias de Gabriel registradas pra não perder:
- **Sereia A3** — mudar pra reviver aliados + aplicar Intocável (já faz) + acrescentar a
  Couraça (ReducaoDanoFixo) nos aliados, dando utilidade ao buff.
- **Morcego** — o buff Sedento sai dele (vira passiva pura) e vai pra **A2 do Vampiro**.
- **Durações/quando inicia** — revisar buffs (quais 2t, quais permanentes, quando aplicam).
- Trocas de habilidade entre personagens em geral. É DESIGN DE JOGO, não arquitetura.

### Bugs de dano a VERIFICAR no rebalance (relatados jogando o front, jul/2026)
Gabriel viu no front: **o log/número diz que causou dano, mas o HP do inimigo não desce** (ou só
desce depois). NÃO reproduzido isoladamente — anotado pra investigar com o front na mão, no
rebalance. Suspeitos, do mais provável ao menos:
1. **Escudo absorvendo** — o golpe é registrado, mas o dano bate no Escudo (buff azul) e não no HP.
   O front já diferencia (`🛡️ N aparou`), mas confirmar que o número mostrado bate com o que foi
   pro HP vs pro escudo.
2. **Piso de HP do Invencível** (`IDefineHPMinimo` = 1) — alvo em 1 HP leva dano CHEIO no
   DanoEfetivo (pro lifesteal enxergar) mas o HP não passa de 1. Log diz "levou X", HP fica igual.
   Comportamento CORRETO, só mal comunicado.
3. **Redução de dano** (ReducaoDanoFixo/Couraça, IModificaDanoRecebido) — dano exibido pode ser o
   bruto e o efetivo outro. Conferir se o número no log é o EFETIVO (o que de fato tirou HP).
4. **Ordem de narração** — havia um bug (corrigido no PR do log persistente) em que a barra descia
   1,5s antes do número; se sobrar sintoma parecido, é a mesma classe de problema (retrato x evento).
Ação: quando pegar, decidir se é bug real ou só clareza de UI — e se for UI, o log já é o lugar de
explicar ("aparou", "imune", "reduziu de X pra Y").

---

## EventoDano — o registro rico do golpe (✅ FATIA 1 + FATIA 2 FEITAS)

**Status:** ✅ COMPLETO. O EventoDano existe e é produzido pelo combate; o ContextoReacao já
foi enriquecido (FoiCritico, Aliados, Inimigos). Só falta a passiva-conta-mortos (1b) como
cliente futuro do contexto rico (seção própria).

**O que é:** o tipo canônico que descreve um golpe — o "Model do golpe" que a apresentação
consome (console hoje, **Unity amanhã**). Convergiu o antigo ResultadoAtaque.

**Propósito B (decisão de Gabriel):** investir cedo na fundação de exibição, não só na lógica.
No porte **Unity**, o EventoDano é a linguagem entre a lógica (calcula) e a apresentação
(desenha) — a camada de eventos consome uma stream de EventoDano e a transforma em animação
(UnityEvents/coroutines). Mesmo princípio das reações (declaram, o orquestrador exibe), levado
ao combate inteiro. Aceito refatorar depois. (Ver `EventoDano por ID` na FILA B.)

### Fatia 1 — FEITA (record + produção)
- **EventoDano** (record em Combate.cs): Atacante, Alvo, DanoBruto, DanoEfetivo,
  AbsorvidoPeloEscudo, Critico, HPRestante, Natureza. Renomeou ResultadoAtaque; campo Dano
  virou DanoEfetivo. Rename propagou pra habilidades, ResultadoReacao.Dano e exibição.
- **ReceberDano** retorna (Efetivo, AbsorvidoPeloEscudo) em vez de int. Captura o
  escudo-absorvido medindo antes/depois de cada modificador (sem tocar a classe Escudo).
- **Atacar** monta o EventoDano com bruto + efetivo/absorvido + crítico.
- **RefletirDano** ajustado (desestrutura a tupla, monta EventoDano).
- Descartado conscientemente: "aparado pela defesa" (nunca tem cliente). DanoBruto e
  AbsorvidoPeloEscudo entram pela exibição/porte (Propósito B), sem efeito que REAJA a eles hoje.

### Fatia 2 — FEITA (enriquecer o ContextoReacao)
O ContextoReacao era magro (Portador, Contraparte, DanoCausado, Natureza). A Fatia 2 levou pras
reações a visão de times que o ContextoCombate já dava pras habilidades:
- ContextoReacao ganhou: **FoiCritico** (de r.Critico), **Aliados**, **Inimigos** (do Portador).
- Os métodos de reação (ProcessarReacoesAlvo e afins) recebem os times (que o CombateService já
  calcula nos call sites) e montam o contexto rico. Aliados/Inimigos = do PORTADOR (inverte no
  lado do alvo, como ProcessarPassivasAlvo já faz; não inverte no lado atacante).
- **Robo** (lê ctx.Aliados, cura o de menor HP) e **Sushiman** (lê ctx.Aliados + ctx.FoiCritico,
  reflexo a todos) migrados. Destravou também a **passiva-conta-mortos** (ver seção, ainda não
  implementada).
ContextoReacao atual: (Portador, Contraparte, DanoCausado, Natureza, FoiCritico, Aliados, Inimigos).

### Os 3 contextos — não confundir (esclarecido)
- **ContextoCombate** (Atacante, Aliados, Inimigos) — das HABILIDADES.
- **ContextoReacao** (Portador, Contraparte, ...) — das REAÇÕES. Enriquecido na Fatia 2.
- **EventoDano** — descreve o GOLPE (não é contexto de quem reage).

---

## Unificar os 3 mecanismos de ignorar status (✅ CONCLUÍDO — jul/2026)

**Status:** ✅ FEITO. Uma língua só de ignorar; o `DeveAgir` morreu.

**A descoberta (original):** "ignorar um status" tinha TRÊS caminhos sem padrão único:
1. **Por natureza** — flags no NaturezaDano (IgnoraEscudo, IgnoraBloqueio, IgnoraDefesa).
2. **Por lista na chamada** — `ignorarStatus: Type[]` (PóMágico, CorteDeVento, Vendaval).
3. **Por passiva permanente** — `IIgnoraStatusNoAtaque` (Drenagem/Vampiro), unida no Atacar.

A inconsistência: a MESMA coisa (furar Escudo) podia ser natureza (Queima) OU lista (CorteDeVento),
sem critério — o "monte de ignorar que ninguém sabia qual usar".

### A resolução — a natureza passou a falar a língua da LISTA
- `NaturezaDano` trocou os bools de STATUS (`IgnoraEscudo`/`IgnoraBloqueio`) por
  `Ignora: IReadOnlyCollection<Type>` (default `[]`, molde da Drenagem). `IgnoraDefesa` FICOU bool
  (defesa é STAT, não status). `NaturezasDano.Direto` foi DELETADO (zero clientes + doc mentirosa).
- Perfis: QueimaDano `Ignora = [Escudo, ProtecaoAliado]`; Veneno/DanoIndireto `= [ProtecaoAliado]`;
  Ataque `= []`.
- `IModificaDanoRecebido.DeveAgir` **DELETADO** (interface + as 6 implementações). O ReceberDano
  agora tem **1 gate só**: `ignorados` = natureza.Ignora ∪ golpe ∪ champ; o status pergunta "fui
  listado?". Nenhum número de dano mudou (tradução flag→lista fiel; provada por 5 testes de paridade).
- **Anti-StackOverflow de proteção mútua agora ESTRUTURAL:** `DanoIndireto.Ignora ∋ ProtecaoAliado`
  → o redirect (que usa DanoIndireto) não re-redireciona. Numa linha, sem depender de disciplina de
  perfil. (Era o `DeveAgir => Reacao != Nenhuma`.)

### Critério de autoria (o produto pro usuário — no CATALOGO)
De quem é a perfuração? essência do dano → perfil de `NaturezaDano`; só o golpe → `ignorarStatus`
no `Dano`; o champ sempre → passiva `IIgnoraStatusNoAtaque`; % do stat DEF → `ignorarDefesaPct`.

### Resíduo — ✅ FEITO (PR limpeza-e-robustez pós-sweep, jul/2026)
- **Defesa montada limpa:** a etapa 1 do ReceberDano agora monta `defesaEfetiva = DefesaComStacks +
  soma dos IContribuiDefesa NÃO-ignorados`, em vez de somar tudo (getter `Defesa`) e descontar os
  ignorados. Paridade exata (ContribuicaoDefesa já vem com sinal: BuffDefesa +, ReducaoDefesa −).
  Teste novo cobriu o buraco (nenhum teste exercitava a etapa 1 com ignore): def 400 + BuffDefesa
  furado = 700 de dano vs 550 sem furar.
- **ATENÇÃO (resolvido):** IContribuiDefesa não era "mina dual-source" (usa tipos concretos; passivas
  não entram na lista `ignorados`) — a montagem-limpa lidou com isso sem varrer passivas.

---

## Conceito de Turno (TurnoDoPersonagem) — PARCIALMENTE FEITO

**Status:** RELÓGIO FEITO. TurnoDoPersonagem extraído (ADR em docs/ADR-conceito-de-turno.md):
Iniciar() (tick dos status) e Finalizar() (avança duração + remove expirados + avança cooldowns
+ limpa contra-ataques do turno).

**Reset "1x por agressor por turno" do CONTRA-ATAQUE — ✅ FEITO.** O registro de quem já foi
contra-atacado saiu dos HashSets privados (ContraAtaque tinha o seu, Operário nem tinha) e virou a
regra única `TentarContraAtacar(agressor, chance)` (chance + "1x por agressor", registra no sucesso),
limpa no Finalizar. Fonte única — buff ContraAtaque, PassivaHeroi e PassivaOperario passam TODOS por
ela, então o gap multi-fonte (Herói com buff do Dragão + passiva) morreu: o primeiro registra, o
segundo vê que já contra-atacou. O hook `StatusEffect.AoPassarTurno` (virtual usado só pelo
ContraAtaque, o único "capaz virtual sem irmã interface") foi REMOVIDO. Herói virou passiva-pura
(IReageAoSerAtacado, sem buff via IPassivaInicial); Operário ganhou o limite 1x/agressor.

**Caminho B — TurnoDoPersonagem PERSISTENTE — ✅ FEITO (FILA A #11 Fatia A, jul/2026).** O
TurnoDoPersonagem deixou de ser TRANSIENTE (`new` a cada turno): o `Combate` agora POSSUI o seu
`Turno` (`Combate.Turno`, criado no ctor, vive o combate todo). O estado turn-scoped
`_jaContraAtacou` + `TentarContraAtacar` MUDOU DE CASA (Combate → Turno); o `Combate.TentarContraAtacar`
virou FACHADA que delega ao Turno (as passivas chamam `ctx.Portador.TentarContraAtacar` — não mudaram
nada). O `Finalizar` limpa o próprio set (o `Combate.LimparContraAtaques` sumiu). Foi feito PRIMEIRO,
como fundação, pra as fatias B/C nascerem na casa certa. Refactor puro: 45/45 testes, zero mudança de
comportamento. Habilita ideias de Gabriel: medidor de turno / velocidade nos stats (feature à parte).

**FALTA (Turno resto):**
- **(Fatia B) Reset 1x-por-agressor das OUTRAS reações — ✅ FEITO (jul/2026).** O `_jaContraAtacou`
  (HashSet) virou `_jaReagiu` (`Dictionary<object, HashSet<Combate>>` POR CHAVE) no Turno +
  `TentarReagir(chave, agressor, chance)`; contra-ataque usa uma CHAVE compartilhada (sentinel), cada
  reação de veneno keya por `GetType()`. `EspinhosVenenosos`/`PutrefacaoContagiosa`/`Fedorento` migradas
  (gate no início do `AoSerAtacado`) — de por-hit → 1x por agressor por turno. **As duas frequências são
  first-class:** `TentarReagir` é OPT-IN (por-agressor); por-hit dispara direto (sem orçamento) — regra
  "só cria método quando há estado" (documentado no `TentarReagir`). 5 testes headless do mecanismo.
- **(Fatia C) — ✅ FEITO (RENASCIDA como `Equipe`/`Batalha`, jul/2026).** A ideia original
  ("times no combatente / TimeAtualDoTurno") MORREU na investigação: a premissa ("cada ponto recalcula
  `is Jogador`") era drift (é 1×, threaded como param) e os CONTEXTOS já são a fonte de perspectiva que
  o domínio consome. Renasceu por um futuro NOVO que o Gabriel nomeou — o modo **VERSUS** (§Versus). O
  que se fez: `Combat/Batalha.cs` (`Equipe { Membros }` + `Batalha { Equipe1/2, EquipeDe, OponenteDe,
  PerspectivaDe, Combatentes }`), mora no CombateService (rebuild por rodada, como o RelogioDoCombate).
  `PerspectivaDe(portador)` é o "um só caminho": derivada da ESTRUTURA (qual equipe), matou os 3 flips
  manuais (`aliadosDoAlvo = inimigosDoAtacante` + a recursão do revide colapsou numa pergunta só). O
  `is Jogador`/`is Inimigo` do fluxo saiu: **time** = `PerspectivaDe`; **controle** = mapa
  `Dictionary<Equipe, IControladorDeTurno>` (campanha: E1→humano, E2→bot; Versus troca o mapa);
  **apresentação** (UX de preparação) = "controlador é bot". As 5 `ProcessarReacoes*` + `ExecutarAtos`
  perderam os params de perspectiva. Refactor PURO (54 testes). Sobrou o `is Jogador` em `AtaqueBasico`
  (contexto próprio) — não é fluxo, fica.
- O disparo do evento InicioDoTurno das passivas — hoje em DispararEventoInicioDeTurno no
  CombateService. Reavaliar se migra pro Turno (boy-scout futuro).

**(histórico) Motivação do Caminho B:** o `_jaContraAtacou` foi posto no Combate por pragmatismo
(Caminho A) — mas é conceitualmente estado DE TURNO (nasce e morre no turno), diferente de
duração/cooldown que são do COMBATENTE (persistem, o turno só avança). Habilita ideias de Gabriel:
medidor de turno /
velocidade nos stats. NÃO confundir com o RelógioDoCombate (contador GLOBAL de rodadas, nível
acima — "boss mata todos após X turnos") — são dois relógios em níveis diferentes.

---

## Modo ARENA (SEAM na Fatia C; MODO ✅ FEITO) — instrumento do REBALANCE #16

**Motivação (Gabriel):** "pro rebalanceamento vou PRECISAR desse Versus". ✅ **FEITO (jul/2026):**
"⚔️ Arena" (3ª opção do menu principal) → menu de 4 modos de controle (**Você×Bot, Bot×Você,
Você×Você hotseat, Bot×Bot**) → monta os 2 times com **TODOS os 36 campeões** (pool independente do
progresso — qualquer matchup) → `CombateService.ExecutarArena(bot1, bot2)` (irmão do `ExecutarFase`:
monta Equipes + o mapa `Dictionary<Equipe, IControladorDeTurno>` conforme o modo; o loop de combate
NÃO muda — o seam faz tudo funcionar). Ambos os times são classe `Jogador` (a ESTRUTURA define quem é
inimigo). SEM mult de fase, itens, recompensa ou save. `CombateView.ExibirResumoArena` mostra os 2
times + vencedor (#7a mede dano/cura por personagem = o instrumento).
- **CAVEAT do rebalance (importante):** o `ControladorBot` **só usa A1** (não as habilidades) — é o
  bot da campanha, class-agnostic mas burro. Então **Bot×Bot NÃO exercita as skills**; pra testar
  matchups de habilidades, o **hotseat (Você×Você)** é o modo forte (humano dirige os 2 lados). Bot
  inteligente (escolher entre habilidades) = melhoria própria do ControladorBot, desacoplada.
- **B×B headless** (simular N batalhas sem TTY, coletar números) segue na FILA B (precisa do seam de
  View concreto) e depende também do bot inteligente pra ser representativo.
- **Descobertas:** hotseat J×J sai quase de graça (a View já mostra "Seu time" da perspectiva de quem
  age, estilo xadrez); a vitória-bool já serve ("Equipe1 sobreviveu?").
- **B×B headless** (simular N batalhas e coletar números sem TTY) → depende do seam de View concreto →
  **FILA B** (conecta com #14 xUnit e o próprio #16).

---

## Fio do revide — revide carrega a HABILIDADE (✅ FEITO)

**Status:** ✅ COMPLETO. `ResultadoReacao.RevidarAlvo: Combate?` era uma "request" disfarçada
de "declaration" — o executor decidia sozinho o HOW (1.0x hardcoded, qual natureza). Virou
`Revide? Revide` onde:

```csharp
record Revide(IAtivavelComNatureza Habilidade, Combate Alvo);
interface IAtivavelComNatureza { EventoDano AtivarComNatureza(Combate atacante, Combate alvo, NaturezaDano natureza); }
```

`IAtivavelComNatureza` é ISP — só A1 (AtaqueBasico) e Marretada implementam. O executor chama
`Revide.Habilidade.AtivarComNatureza(alvo, Revide.Alvo, natureza)` polimorficamente, sem saber
qual skill é. **Sem ContextoCombate na assinatura** (desvio da ideia original) — o revide não
precisa de Aliados/Inimigos, só do atacante fixo; carregar `ctx` seria parâmetro sem uso.

Cada reação que declara revide busca a skill do portador via `IAtaquePrimario`/tipo concreto
(não hardcoda `AtaqueBasico` cru — pensando em A1 customizada futura, que pode ser AoE ou
aleatória):
- ContraAtaque: `portador.Personagem.Habilidades.OfType<IAtaquePrimario>().OfType<IAtivavelComNatureza>().First()`
- Operário: `portador.Personagem.Habilidades.OfType<Marretada>().First()`

**EspinhosVenenosos NÃO é cliente** (correção: o ROADMAP antigo listava errado — Espinhos só
aplica Veneno+Queima no atacante, nunca revidou com dano).

**Quebra do loop A↔B: profundidade, não Natureza (mudou do desenho original).** A ideia inicial
usava `NaturezasDano.Revide` com `TipoReacao.SemContraAtaque` só pra sinalizar "não gera outro
contra-ataque" — auditoria mostrou que `SemContraAtaque` só era lido em UM lugar (dentro do
próprio ContraAtaque), um enum value inteiro existindo só pra carregar controle de fluxo
disfarçado de tipo de dano. Trocado por **profundidade explícita** em `ProcessarReacoesAlvo`
(`int profundidade = 0`, incrementado na recursão): só processa `res.Revide` quando
`profundidade == 0`. `TipoReacao.SemContraAtaque` e `NaturezasDano.Revide` foram REMOVIDOS —
`TipoReacao` agora é só `{ Completa, Nenhuma }`; o revide usa `NaturezasDano.Ataque` (mecanicamente
é um ataque igual qualquer outro). Se um dia o revide precisar de comportamento distinto de um
ataque normal, o lugar certo é metadado no `EventoDano` (ver "Proveniência de status"), não uma
Natureza nova.

**Operário:** aceita o gap de multi-fonte conscientemente. Se o mesmo personagem tiver, ao mesmo
tempo, o buff genérico ContraAtaque (ex: aplicado por DragaoProtetor) E a passiva própria (10%
Marretada), as duas podem contra-atacar no mesmo golpe — cada uma com seu próprio limite "1x por
agressor" independente, sem tracker compartilhado. Resolver isso de vez é o "reset 1x-por-agressor
reutilizável" já registrado em Turno (resto); não vale puxar pra cá sem um caso real doendo.

**Ideia futura registrada:** um personagem cuja habilidade é ativa-e-passiva ("eu sempre
contra-ataco com ESTA habilidade", sem RNG) já é suportado de graça pelo desenho atual — só
precisa declarar `Revide(suaHabilidade, contraparte)` como o Operário faz. A interação desse
personagem com o ContraAtaque genérico do Dragão (qual prevalece?) fica em aberto pro dia que
existir.

**Depois:** quando um personagem novo quiser revidar com outra skill, basta implementar
`IAtivavelComNatureza` nela. Zero mudança no executor.

---

## Auditoria das habilidades ATIVAS — ✅ ENCERRADA SEM AÇÃO (ago/2026)

**Status:** FECHADA. Era "avaliar UMA vez se há dívida; se não houver dor, encerrar como sem ação" —
e a avaliação foi feita pelos fatos, não por opinião: a **Composição de Ações** (fio 5) passou por
cima deste item e resolveu a dúvida por construção. As ativas não são mais "modelo data-driven
decente com `Ativar`" — elas são DADO puro, lista de `Acao` rodada pelo interpretador, com **zero
`Ativar` override** entre elas. Não há o que auditar: a pergunta "essas classes escondem dívida?"
deixou de ter sujeito.

O fio do revide-com-habilidade, que dependia parcialmente disto, já tinha se resolvido sozinho sem
tocar a base de `HabilidadeAtiva` (`IAtivavelComNatureza` é ISP à parte).

---

## Proveniência de status — quem criou o status (FIO NOVO)

**Status:** REGISTRADO, futuro. Implementar quando o primeiro efeito que precisa aparecer.

**O que é:** todo StatusEffect carregar quem o criou (Aplicador/Origem), pra passivas filtrarem
"os status que EU criei". NÃO é o EventoDano (que descreve o golpe) — é a proveniência (de quem
é o status).

**Exemplo motivador (Gabriel):** escudo que reflete X% do dano de CADA escudo que ESTE campeão
colocou. Se o campeão põe escudo em vários aliados, reflete só dos escudos DELE; se outro
substituiu, a ligação quebra. Combo de time: A põe escudo, B aumenta valor+duração → escudo
sempre cresce e nunca acaba → dano refletido cresce junto. Precisa de: (1) Escudo carrega
Aplicador, (2) passiva filtra por origem, (3) regra "maior prevalece" (já existe).

**Precedente:** ProtecaoAliado.Aplicador, Irritar.Aplicador — alguns status JÁ rastreiam origem.

---

## Estado de Vida (Vivo/Morto) + Atos do turno — Passos 1-4 FEITOS, falta seleção por estado

**Status:** ✅ Passos 1-**5** IMPLEMENTADOS (State Pattern Vivo/Morto, status separados no
Morto, Atos do Turno, Guarda limpa, **seleção de alvo por estado — PR #111**; ver
ADR-selecao-por-estado.md). Só a passiva-conta-mortos (1b) segue pendente, e é do EventoDano.
Ver **ADR-estado-de-vida-e-atos.md**
(conceitos fechados). Este foi o fio que UNIFICOU o que antes eram dois: "estado morto" e
"separação de fases do turno" — o Ato de Morte é a transição de estado. **Falta só o Passo
5 (seleção de alvo por estado)**, detalhado abaixo.

**Resumo da decisão (detalhe no ADR):**
- **Estado de vida como objeto (State Pattern):** Combate tem um `EstadoVida` interno
  (Vivo/Morto), delega interações que dependem de estado (Reviver/Curar/etc) — sem
  `if (estaVivo)` espalhado. O Combate mantém a identidade; troca o objeto de estado na
  transição.
- **Status de morto vivem no objeto Morto** (lista separada da do vivo). Isola dos
  cleanses/bloqueios do vivo. O bloquear-revive volta a ser DEBUFF (símbolo, removível
  pelo Diabo) — agora possível porque (1) ganha removedor e (2) vive no Morto, fora do
  alcance da ImunidadeDebuffs. Resolve o histórico do MortePermanente.
- **Atos do turno:** ExecutarHabilidade vira 5 Atos nomeados (AtoExecucao,
  AtoReacaoDoAlvo, AtoMorte, AtoReacaoDoAtacante, AtoEncerramento). "Ato" e não "Fase"
  (Fase = campanha). O AtoMorte é onde o modelo de estado encaixa.
- **Cálculo vs fluxo entre classes JÁ está certo** (Combate=domínio calcula,
  CombateService=orquestra). NÃO se move cálculo nem se cria classe de cálculo. O
  refactor é dar Atos ao fluxo + o modelo de estado fornecendo o AtoMorte.

**Já destravado (Passos 1-4 feitos):** a Guarda limpa (saiu do hack, vira prevenção no
AtoMorte) e o bloquear-revive com dono (ImpedirRessurreicao, debuff no Morto). **Ainda
falta consumir a última destrava:** seleção de alvo por estado (reviver→morto, curar→vivo).

**Sequência de implementação (no ADR §10):** ✅ modelo de estado → ✅ status no morto →
✅ Atos → ✅ Guarda limpa → **seleção por estado (falta, é o Passo 5)**. Cada passo
buildável, Strangler-friendly — a sequência entregou exatamente nessa ordem.

**REFATORAÇÃO DAS ATIVAS — seleção de alvo por estado (dor REAL detectada, fio próprio):**
Hoje cada ativa checa EstaVivo() de forma INCONSISTENTE — umas filtram `.Where(EstaVivo())`,
o AnjoCaido filtra `!EstaVivo()`, as de dano não checam. Não há padrão; cada Ativar decide na
mão se mira vivo ou morto. Agora que Morto é estado de primeira classe, a bagunça fica
explícita. SOLUÇÃO (ideia de Gabriel, análoga ao que fizemos com NaturezaDano): uma INTERFACE
declarativa onde a habilidade DECLARA quem mira (VIVOS / MORTOS / AMBOS), e a seleção de alvo
respeita automaticamente — em vez do filtro manual em cada Ativar. "Make illegal states
unrepresentable" (uma cura não consegue nem mirar um morto). Conecta com o Passo 5 do ADR
("seleção de alvo por estado") — agora com FORMA (interface declarativa). É refactor das
ativas, dói de verdade (não pureza), GRANDE — fio próprio, não cabe no Passo 2.

EXEMPLOS pra desenhar a interface (mapeados): a maioria mira UM estado (cura→vivos,
revive→mortos, ataque→vivos). Casos ricos que miram OS DOIS: **AnjoCaido (Diabo)** revive
mortos E cura vivos — duas seleções de listas diferentes na mesma habilidade. **Barata
(Glitch)** é ativa que mira VIVO (ataque) e, como CONSEQUÊNCIA do golpe (matou→virou morto),
aplica bloqueio no morto resultante — uma seleção (vivo) + consequência no morto, não duas
seleções. A interface precisa distinguir "mira os dois" (Diabo) de "mira um, consequência no
outro" (Glitch). NOTA: bloqueadores de revive são de DOIS tipos — PassivaVilao é REAÇÃO
(IReageAoMatar), Barata/Glitch é ATIVA (seleção). Os dois migram no Passo 2 (aplicam o debuff),
mas só o Glitch é cliente da interface de seleção.

**SEMENTE FUTURA — mecânica de "Vida de Alma" (lore + mecânica grande, NÃO desenvolver agora):**
ideias cruas de Gabriel pra maturar: o morto teria uma "Vida de Alma" (a vida REAL pós-morte);
atacar a alma e zerá-la = morte PERMANENTE de verdade (nem o Diabo revive — matou a alma).
Possível atacar o morto direto na Vida de Alma. Almas atacando almas (um morto inimigo ataca
as almas vivas, afetando o vivo?) — Gabriel inclinou que talvez não generalize, mas uma facção/
personagem TEMÁTICO poderia ter "sobrevida" e atacar como alma (vantagem temporária vs a
desvantagem de decompor). Questão em aberto: a decomposição TIRA a Vida de Alma (uma coisa só)
ou são dois sistemas (provavelmente uma só). Status no morto poderiam mirar a alma (reduzir
"defesa da alma" pra matá-la mais rápido?).

**SACADA ARQUITETURAL (Gabriel) — Alma como TERCEIRO ESTADO:** se a Alma precisar de
comportamento próprio (atacar, ser atacada na vida-de-alma, interagir), ela vira uma TERCEIRA
filha de EstadoVida (Vivo / Morto / Alma) — o State Pattern já suporta, é só criar a classe sem
mexer nas outras. Transição possível Vivo→Morto→Alma (decompôs vira alma) ou Vivo→Alma (facções
que morrem direto pra alma). Isso resolveria as dúvidas (morto passivo não ataca; Alma é estado
ATIVO que ataca/é atacada). O modelo NÃO fecha portas — a Alma entra como estado quando a
mecânica amadurecer. TUDO futuro — Gabriel vai maturar e trazer depois.

**DECISÃO (Passo 2):** ImpedirRessurreicao fica como **Debuff** por ora (não tipo próprio). É
seguro — vive na lista do Morto, e os cleanses genéricos filtram EstaVivo() (só vivos), então
nunca alcançam ele. O isolamento vem da SEPARAÇÃO DE LISTAS (Forma 2), não do tipo. Só vira
decisão se/quando a Alma mudar como cleanses rodam em mortos — aí Gabriel reavalia.

**Gameplay futuro que o modelo HABILITA (NÃO no refactor — design de balanceamento, matura
JOGANDO):** o estado Morto vira um SISTEMA tático rico. Visão de Gabriel (registrada pra não
perder; números NÃO cravados — afinar em playtest, são alavancas interdependentes):

- **Decomposição (penalidade por não reviver):** a cada turno morto, acumula um tick de
  decomposição que tira % PERMANENTE (na partida) dos stats TOTAIS — vida, def, atk (ex:
  ~5%/tick). Incide sobre o total: passivas que alteram o total vão junto; buffs de atk somam
  SOBRE o novo total já penalizado. Debuff NÃO-removível e VISÍVEL (o jogador vê quantos ticks
  de penalidade acumulou).
- **Explosão (clímax da decomposição):** ao atingir N ticks (ex: ~10), o corpo EXPLODE —
  causa dano no PRÓPRIO time (penalidade por abandonar o morto) e CONTAMINA os vivos (aplica
  ~2 ticks de um debuff de contaminação NELES — o mesmo debuff transicionando vivo↔morto).
  Após explodir, é MORTE PERMANENTE DE VERDADE: nem o Diabo revive. ESTA é a "morte
  permanente" real — reservar o nome pra ela.
- **Renomeação (decisão de AGORA, afeta o Passo 2):** a "morte permanente" do Vilão NÃO é a
  permanente de verdade — é só um BLOQUEIO removível. Vira **ImpedirRessurreicao** (debuff do
  Vilão, removível pelo Diabo). "Morte permanente" fica reservado pra explosão-da-decomposição.
- **Diabo com penalidade (ponderar):** pra reviver alguém com ImpedirRessurreicao, o Diabo
  paga um preço — duas opções a ponderar: (a) ADICIONA ~2 ticks de decomposição ao reviver
  (mais agressivo — pode empurrar pra explosão), ou (b) ROUBA metade dos ticks pra si (menos
  agressivo, mas ainda custoso). Decidir jogando.
- **Limpeza de ticks (ponderar):** formas de reduzir decomposição — a cada cura recebida, a
  cada ~2 turnos vivo, ou ao matar um inimigo. Decidir jogando.
- **fraqueza-por-revive:** caso mais simples do mesmo princípio — cada morte+revive deixa uma
  marca acumulativa. Pode ser a própria decomposição ou um efeito à parte.

Tudo são status de MORTO (e contaminação que transiciona vivo↔morto) que rodam sobre o
modelo via a view StatusAtivos — REÚSO dos mesmos mecanismos de tick/processamento, sem
duplicação. O refactor (Passo 2) entrega só o ImpedirRessurreicao (Vilão aplica, Diabo
remove). A mecânica completa valida que o desenho não fecha portas, mas só vira código na
fase de balanceamento.

**Identidade / lore (semente):** esta mecânica de morte-como-sistema é um DIFERENCIAL — Gabriel
não conhece jogo com algo assim (Void Hunters tem penalidades, mas natureza diferente). Dá
identidade própria ao Apostle's War. Possível resgate da lore criada no Campo Minado (a Deusa
e os apóstolos) pra justificar a mecânica na ficção — por que mortos decompõem/explodem/
contaminam. Fio de NARRATIVA, futuro.

**Conexão com Arena (design, ver GDD §6):** a decomposição serve como **enrage timer natural**
do Modo Arena — quando dois times entram em loop e ninguém morre, os ticks de decomposição
forçam resolução sem timer externo artificial. O mesmo sistema que pune negligenciar mortos na
campanha resolve o anti-stall da Arena. Um fio técnico, dois problemas de design.

**Comportamento-BASE (já decidido):** status de vivo SOMEM ao morrer (Opção X); as
consequências de morte/revive entram depois como status de morto, sem retrabalho estrutural.

**1b) Passiva-conta-mortos** (passiva do VIVO que conta mortos pra ganhar força) — NÃO é
estado morto, consulta o tabuleiro. Depende do contexto rico (Fatia 2). Seção própria.

---

## Passiva-conta-mortos — ✅ FEITO (jul/2026) + IRMÃ desenhada

**`EscalaComMortos` (Skills/Passivas/EscalaComMortos.cs):** passiva genérica config-driven —
`IReageAoInicioTurno` (molde da Ventania/Tengu) que RENOVA todo turno um buff cujo valor é
proporcional aos MORTOS no campo. Lê o tabuleiro pelo `ContextoCombate` (Aliados/Inimigos, da Fatia
2 — não é estado de morto). Generaliza 3 eixos: **escopo** (`EscopoMortos.ProprioTime`/`TimeInimigo`/
`AmbosOsTimes`) × **stat** (fábrica `Func<double,Buff>` → BuffAtaque/BuffDefesa/… da matriz de stats)
× **por-morto**. Cliente: **Zumbi "Horda"** (ambos os times → +10% ATK/morto, placeholder). Os escopos
só-aliados/só-inimigos nascem prontos p/ passivas futuras. (O Zumbi perdeu a `PutrefacaoContagiosa`,
que era clone do Fedorento do Cocô.)

**IRMÃ — `EscalaComAbates` (DESIGN PRONTO, NÃO construída; Gabriel implementa depois do front + mais
champs):** mesmo TEMA, gatilho/persistência diferentes.
- Gatilho: `IReageAoMatar` (reage quando ESTE champ mata; `ctx.Portador` = matador), não tabuleiro.
- Efeito: bônus **PERMANENTE** que ACUMULA por abate (molde da **Ambição** do Troll — `AdicionarBonus
  XPermanente`), **não** é buff.
- Generaliza **stat × por-abate** via closure `Action<Combate>` (os bônus permanentes são heterogêneos:
  ATK/DEF = %-sobre-base int; DanoCrit/TaxaCrit = pontos absolutos double — closure captura tudo).
  Ex.: `c => c.AdicionarBonusAtaquePermanente((int)(c.AtaqueComItens * 0.10))`.
- **GAP a resolver na impl:** HP/vida NÃO tem `AdicionarBonusHPPermanente` (HP é HPMaximo, mexido só
  no IniciarCombate/itens) — escalar HP por abate precisa de método novo (mexe no HPMaximo e/ou HPAtual?).
  ATK/DEF/DanoCrit/TaxaCrit já têm método.
- Aberto: cap (Ambição tem 25% via `ObterEstado`) vs sem cap (abates são finitos). Default sugerido: sem cap.

---

## RelógioDoCombate — enrage / limite de turnos

**Status:** EMBRIÃO ✅ FEITO (jul/2026, a pedido do Gabriel: contador de turnos na tela de combate).
`Combat/RelogioDoCombate.cs` — `NumeroDoTurno`/`Avancar`/`Reiniciar`. Injetado no CombateService
(avança no início de cada `ExecutarTurnoCompleto` — inclui turno-extra e Preso; reinicia no
`ExecutarCombate`) e no CombateView (cabeçalho `═══ Turno N ═══` no `ExibirPartida`). Conceito
VIZINHO do TurnoDoPersonagem, num nível ACIMA (combate/rodada, turnos GLOBAIS). **FALTA (futuro,
YAGNI):** disparar eventos em marcos (enrage / limite de turnos / anti-stall) — cresce daqui quando
uma fase concreta pedir.

---

## BOY SCOUT (quando tocar) / FUTURO ARQUITETURAL

### Modernização e robustez (auditoria jul/2026)
**Status:** PARCIALMENTE FEITO. Os itens pequenos saíram no PR "limpeza e robustez pós-sweep"
(jul/2026, junto do enum `Ambos` e da defesa-montada-limpa). Nenhum item é bug; são "formas melhores
hoje do que fizemos antigamente". Guard-clause em código interno fica DE FORA de propósito (YAGNI).
1. **`Random.Shared`** ✅ FEITO — matou as 8 instâncias `new Random()` (eram 8, não 4: Combate,
   HabilidadeAtiva, Medo, AplicarDebuff, Repetindo, Surpresa, PeleGrossa, Intimidador). Idioma .NET 6+.
2. **Encapsular coleções mutáveis → choke-point de evento (FILA B, gatilho do porte)** — REANCORADO
   (jul/2026). O valor NÃO é pureza de encapsulação — é ser o **único ponto onde disparar "status X
   aplicado/removido em Y"** pra camada de eventos/animação do Unity. **Descoberta ao ler o código:**
   os "46 lugares" são enganosos — quase todos são **auto-mutação de dentro do próprio `StatusEffect`**
   (`Aplicar` base faz `alvo.StatusAtivos.Add(this)` em `StatusEffect.cs:72`; cada `Remover` faz
   `.Remove(this)`). É **1 padrão conceitual** repetido ~44×, não 46 callers caóticos; externo real só
   `MoverBuffs` e `Escudo`. **Forma alvo:** `List` privada + `IReadOnlyList` público +
   `AplicarStatus`/`RemoverStatus` no `Combate` (onde o evento dispara). Também `Personagem.Habilidades`,
   `Vivo.StatusNoVivo`/`Morto.StatusNoMorto`. **Gatilho:** fazer JUNTO da camada de eventos no porte —
   agora seria seam vazio custando 44 arquivos. Converge com `EventoDano por ID` e `IModificaDanoCausado`.
3. **Save defensivo** ✅ JÁ ESTAVA FEITO — `CapitulosService.CarregarProgresso` e
   `ArsenalService.CarregarItensEquipados` já têm try/catch com fallback (JsonException/IOException/
   UnauthorizedAccess → mantém default, não crasha). A auditoria assumiu que faltava; o código já fazia.
4. **"Nulo morre na porta" no resto do código** — ✅ **PRATICAMENTE RESOLVIDO** (verificado jul/2026).
   Varrido: as ÚNICAS coleções anuláveis são a família `ignorarStatus` (`Combate.ReceberDano`/`Atacar`,
   `Dano.cs`). O `= null` NÃO é desleixo — é **imposto pelo C#** (valor default de parâmetro tem que ser
   constante; `[]`/`new List()` não são → `IEnumerable<Type>? x = null` é o único jeito de ter param de
   coleção opcional). E o nulo já morre na fronteira: `ReceberDano` faz `ignorarStatus?.ToHashSet() ??
   new HashSet<Type>()`. Resíduo só cosmético: `ComporListaIgnorar` (`Combate.cs`) retorna anulável e
   passa o nulo um pulo, mas nunca é iterado enquanto nulo. Nada a fazer de substancial.

### Capacidades — stat sob demanda e comportamento de turno
**Status:** ADR em docs/ADR-modelo-de-capacidades.md. Migração incremental.
- A) Reação após evento → IReageAo* — buffs FEITOS; passivas no C5 (quase fim).
- B) Intervenção no dano → IModificaDanoRecebido FEITO (o `DeveAgir` foi REMOVIDO na unificação
  do ignorar — jul/2026; a decisão virou 1 gate de lista no ReceberDano). **Lado ATACANTE ✅ FEITO
  (FILA A #10):** `IModificaDanoCausado` (forma multiplicador, consultado pela Ação Dano) — o irmão
  do defensor. Cliente: Piromancer. Fórmula-de-hab (Caveira/Tengu) fica no `Dano(Func)`, é outro balde.
- E) Bloqueio de aplicação → IBloqueiaStatus FEITO.
- C) Stat sob demanda → IContribui* ✅ **FEITO (FILA A #8, jul/2026).** Generalizado pros 4 stats
  (`IContribuiAtaque`/`IContribuiDefesa`/`IContribuiTaxaCrit`/`IContribuiDanoCrit`); todo getter soma
  a interface com sinal. Matriz simétrica: cada stat tem buff (+) e debuff (−).
- D) Comportamento de turno → ✅ **FEITO (FILA A #9, jul/2026).** Três capacidades por FASE, cada
  status dono do seu comportamento (o fluxo não decide mais por tipo concreto): `IPulaTurno` (Preso),
  `IForcaAcao` (Irritar), `IParalisaAcao` (Medo). Descoberta ao investigar: NÃO eram uma "família"
  única — hookam em fases diferentes (antes/na/depois da escolha) com formas diferentes (marcador/
  alvo/bool), então ISP por fase, não interface-Deus. A "família" real é o pular-turno: `IPulaTurno`
  é a porta pra Congelar/Stun/Enraizado/Petrificado (variantes = tema à parte, Gabriel desenha as
  diferenças). A nota antiga "Irritar fica de fora" foi revista — ele entrou (o objetivo é tirar TODA
  decisão de fluxo do combate).

### Helper ColetarReacoes<T> (dívida de repetição)
**Status:** ✅ **FEITO (jul/2026, FILA A #2).** O padrão "varre StatusAtivos.OfType<T> +
ColetarPassivasReativas<T>" se repetia em **7** métodos do CombateService (Alvo, AntesDeMorrer,
AtacanteMorte, AoMorrer, PorAlvo, PorAtaque, InicioTurno). Virou UM helper
`ColetarReacoes<T>(portador, invocar)` — o `invocar` é lambda porque cada interface tem seu verbo
(AoReceberDano, AoMatar...); o helper unifica a VARREDURA, não o verbo. Ordem preservada
(status → passivas, mesma dos métodos à mão). Bônus de consistência: o InicioTurno passou a varrer
as DUAS fontes (antes só passivas; hoje nenhum status implementa IReageAoInicioTurno — idêntico em
comportamento, mas o dia que um buff quiser reagir ao início do turno, já funciona).

### Auditoria de código (jul/2026 — olhar fresco, pós-fila)
Leitura crítica do núcleo (CombateService, Combate, HabilidadeAtiva, Acao, TurnoDoPersonagem,
Program.cs, champs na forma final) procurando o que discordar. **Veredito: a arquitetura está
sólida** — motor de 8 linhas, composition root limpo, cadeia de Atos da morte robusta (cada ato
re-checa EstaVivo — a Guarda reverter a morte e os posteriores não dispararem é design, não sorte).
**4 achados, todos encaixados na fila:**
1. **Seams violados (→ itens 3 e 6):** sobraram 4 `Console.*` fora da View — `Console.Clear` no
   `CombateService.ExecutarTurno` (o único no coração do combate) e no `ControladorJogador`;
   `Console.WriteLine`+`Thread.Sleep` nos fallbacks de save do `ArsenalService`/`CapitulosService`.
2. **Mina latente no `ResolverAlvos` (→ item 14):** `resultado.Add(alvoSelecionado)` confia que a
   semente está nos `candidatos` filtrados; se não estiver (ex.: hab `Mortos` + hit-all → semente =
   atacante vivo), o vivo entra nos resolvidos e `IndexOf` devolve -1. VERIFICADO: nenhum champ
   ativa esse caminho hoje (só DocesDeAbobora usa `Mortos`, com pick real que trata o vazio). Não é
   bug vivo — é contrato sem guarda. Guard de 1 linha + teste que o documente.
   - **EPÍLOGO (jul/2026): o "nenhum champ ativa" ENVELHECEU e o guard virou crash.** O sweep dos
     champs pra forma-construtor trouxe 5 revive-de-todos com exatamente essa forma (hit-all +
     `Mortos`): Robô/Technology, Sereia/Atlantis, Anjo/Céu, Palhaço/Circo, Diabo/Anjo Caído. Todos
     explodiam ao reviver — só quando havia alguém morto pra reviver (sem mortos, o early-return
     de candidatos vazios salvava antes). **Correção:** o guard só vale onde existe PICK; hit-all
     (`NumeroDeAlvos == int.MaxValue`) não tem — a semente é placeholder do `CombateService`, e o
     `ResolverAlvos` agora devolve os candidatos inteiros antes de cobrá-la. Lição de doc: um
     "VERIFICADO: nenhum champ faz isso" é foto, não regra — quem adiciona champs não lê o
     ROADMAP. O que faz a regra valer é o teste (`HitAllDeMortos_ComSementeViva_...`).
3. **Constantes de balance espalhadas (→ item 16 passo 0).**
4. **Contrato traiçoeiro no `ColetarPassivasReativas`:** consome o cooldown AO COLETAR, antes de
   saber se a passiva vai agir. Hoje inofensivo (reativas têm cooldown 0). **REGRA ao criar passiva
   reativa com cooldown E condição interna:** mover o consumo pra DEPOIS da decisão de agir — senão
   ela queima cooldown sem fazer nada, silenciosamente.
**Avaliado e deixado como está (de propósito):** `EstadoHabilidades: Dictionary<Habilidade, object>`
(object-typed, mas 2 usos contidos via `is not T` — o custo de tipar não paga); o TODO de exibição
de cura no `ExibirResultadosReacao` (conferir no item 7 se reações que curam já existem e o TODO
envelheceu).

### Observabilidade — exibir TaxaCrit/DanoCrit na UI de combate
**Status:** ✅ **FEITO (absorvido pela FILA A #7a, jul/2026).** TaxaCrit/DanoCrit agora aparecem na
`CombateView.ExibirPartida` (🎯%/💥x) e no resumo de fim de batalha. OlhoClinico/Virus (que mexem
nesses stats) viraram observáveis ao vivo.

### Testes automatizados (xUnit na lógica de domínio)
**Status:** ✅ **xUnit JÁ RODA** — o projeto `Tests/` existe com `MotorDeHabilidadesTests.cs` +
`NavegacaoTests.cs` (~30 testes verdes, somados facção a facção durante o sweep). A antiga nota
"nunca usou xUnit / adiado" ficou desatualizada (drift corrigido jul/2026). O que segue **PENDENTE
(FILA A #12) é AMPLIAR a cobertura** pra além do motor/navegação: a **ordem crítica de morte**
(Guarda→Vilão→Necromancia) e o `ReceberDano` ponta-a-ponta — os pontos onde bug sério já apareceu
(StackOverflow de proteção mútua, crítico-exige-dano, dispatch dual-source). É a rede de segurança
antes do Rebalanceamento e diferencial de portfólio.

### Persistência — porta `IRepositorioDeSave` (Steam/Play cloud save, NÃO SQL)
**Status:** ✅ **PORTA FEITA (FILA A #6, jul/2026)** — `Services/IRepositorioDeSave.cs` (corte typed:
dona do JSON+IO+corrupção) + `SaveLocal`. O SQL/servidor-próprio foi **DESCARTADO** (Unity
single-player). O save fica **local** (JSON em `Save/`), mas precisa sincronizar com **Steam Cloud** e
**Google Play Saved Games** — que são **SDK de plataforma, não backend seu** (Steam Auto-Cloud =
arquivos locais sincronizados sozinhos; Play = API de Snapshots). Falta só:
`SaveSteam`/`SavePlayGames` plugam no porte (`FILA B`, precisam dos plugins Unity). SQL/REST só
reabrem se um dia quiser contas/ranking/servidor próprio.

### Services-lookup (cosmético, baixa prioridade)
**Status:** ✅ **FEITO (FILA A #5, jul/2026).** `FaccaoService`→`Models/Faccoes.cs` (estático, só o
Símbolo — o Nome já vive no `[Description]` do enum); `CampanhaService`→`Models/Campanha.cs` (estático).
Saíram da DI; `ObterNome` morto+duplicado deletado. Base natural pra virar ScriptableObject no porte.

### Faxina de nomes (rename do repo/namespace) + organização de camadas
**Status:** ✅ **FEITO** o rename do `v1` — namespace `v1_Apostle_s_War` → `ApostlesWar`,
sln/csproj junto. ✅ **FEITO (jul/2026) a organização de camadas:** pastas `View/`
(`ApostlesWar.View`) e `Controllers/` (`ApostlesWar.Controllers`) criadas fora de `Services/`; o
god-object `MenuService` foi QUEBRADO em **`MenuView`** (telas de menu: principal/capítulos/fases/
inventário/seleção-de-time) + **`CombateView`** (render da partida + menu de alvo); `IApresentacao`
foi pro View, os 3 controladores pro Controllers. **Convenção FIXADA:** pasta/namespace em inglês
(`View`/`Controllers`/`Services`/`Skills`/`Combat`), sufixo de camada em inglês (`View`/`Controller`/
`Service`), raiz de domínio em português (`Menu`, `Combate`, `Controlador`) — "domínio na língua local,
andaime em inglês", padrão comum em time não-anglófono. ✅ **FEITO (jul/2026) a porta de ENTRADA:**
`IEntrada`/`EntradaConsole` + `Comando(Tipo, Numero)` + `Navegacao.MoverCursor` (em `View/`) — o par
simétrico do `IApresentacao` (saída). TODO `Console.ReadKey` (9 sites) passa por ela; agora o único
`ReadKey` do código está na `EntradaConsole`. `ConsoleUtils.SelecionarComCursor` do GHUtils aposentado.
O comando `Selecionar(N)` capturou o atalho numérico do teclado (apertar "3" pula pra opção 3) — que
é a MESMA forma de um clique de mouse, então mouse-futuro entra por aí sem reescrever o seam. Mouse
NÃO feito de propósito (modelo diferente — navegação vs seleção direta; desenhar antes de saber
web-vs-Unity = generalidade especulativa; o `Selecionar` já deixa a ponte). ✅ **FEITO (jul/2026) a
faxina do `GerenciadorDeJogo`:** a renderização de saída (confirmar-saída, créditos, tela de vitória,
aviso de inventário vazio) migrou pra `MenuView` (`ExibirConfirmacaoSaida`/`ExibirCreditos`/
`ExibirTelaVitoria`/`ExibirAviso`); os `Thread.Sleep` passaram pelo `IApresentacao` (injetado na
MenuView). O Gerenciador ficou com **ZERO `Console.*`/`Thread.*`** — orquestração pura (decide QUANDO,
a View faz COMO; o cálculo de domínio "quem é novo" fica no Gerenciador). ✅ **FEITO (jul/2026) a
faxina de modelos/nomes:** auditadas `Campaingn/` e `Combat/` — NADA fora de lugar (tudo domínio, zero
Console/IO, zero dependência das camadas de cima). `Capitulos.cs`+`Fase.cs` migraram pra `Models/`
(gêmeos de Item/Personagem, namespace-raiz → zero mudança de using) e a pasta typo `Campaingn/` SUMIU.
Classe `Capitulos`→`Capitulo` (era plural num modelo singular; save intacto, JSON é por propriedade).
Enum morto `OpcoesMenu` deletado; `using System.Text.Json` morto do Capitulo removido. **Deixados de
propósito:** `Combat/` fica (subsistema, não modelo solto); `Fases` enum fica (25 refs + colide com
`class Fase` — churn > ganho); `NaturezasDano` fica (catálogo estático, plural ok); `CapitulosService`
mantém o nome (serviço da área-de-domínio no plural é ok, o MODELO é que ficou singular). **Falta:**
quando o alvo (web/Unity) for escolhido, o input de MOUSE. Portfólio: recrutador lê o repo.

### Cancelamento de batalha (Esc → encerra → perde) ✅ FEITO (jul/2026) — o payoff dos seams
O jogador ficava preso no turno do inimigo/animações sem poder sair. Agora Esc durante a batalha →
"Encerrar? Você perde a fase" → Sim → a fase vira derrota (sem recompensa). **Forma 1 (espera
interrompível), decidida PROVANDO** contra a async: no console, a Forma 2 (async+CancellationToken)
COLAPSA no mesmo poll da Forma 1 (o console tem UM consumidor de input — um listener de Esc em
background brigaria com o `ReadKey` dos menus), pagando cascata async por ~20 métodos por ZERO ganho;
e o porte web/Unity re-arquiteta o loop de qualquer jeito e reaproveita os SEAMS, não a async-ness.
Mecânica: `IApresentacao.AguardarAnimacao` retorna `bool` (Esc na espera; poll em fatias no
`ApresentacaoConsole`); `CombateView.ConfirmarEncerramento()` (diálogo auto-contido); `CombateService.
Aguardar(ms)` reage e lança `BatalhaAbortada`, capturada em `ExecutarFase` → `false` (desenrola a
cadeia profunda sem threading de flag). **Escopo:** só nas ESPERAS (a dor real); Esc-nos-menus-de-ação
fica como follow-up trivial. Não testável headless → verificação em jogo do Gabriel.

### EventoDano por ID (desacoplar dos objetos vivos) — FILA B
**Status:** registrado (FILA B, junto da camada de eventos do porte). O `EventoDano` carrega hoje
`Combate Atacante`/`Combate Alvo` (objetos vivos). Pra ser um registro limpo do golpe (log/stream
desacoplado que a apresentação Unity consome), referenciaria por id/nome. Converge com
`Encapsular coleções → choke-point de evento`. Fazemos no porte.

### IModificaDanoCausado (modificador de dano do atacante)
**Status:** follow-on da Composição de Ações. A ação `Dano` passa a consultar modificadores do
atacante automaticamente (a `PassivaPiromancer` para de ser fiada à mão em cada habilidade de
fogo). Espelho do `IModificaDanoRecebido`. Cruza com `FontesDeCapacidade` (dispatch das duas
fontes: StatusAtivos + Personagem.Habilidades).

### Identidade comum (Nome/Simbolo/Descricao) — resíduo do ADR-sistema-de-efeitos (arquivado)
**Status:** ✅ **FEITO (FILA A #4, jul/2026).** A "Separação 1" do `sistema-de-efeitos` (base comum de
identidade — Nome/Simbolo/Descricao — herdada por `Habilidade` E `StatusEffect`) virou a classe
`ElementoDeJogo` (`Skills/ElementoDeJogo.cs`). No mesmo PR morreu o homônimo `Turnos`
(cooldown/duração viraram `Cooldown`/`DuracaoRestante`, libertando "Turno").

### Faxina de comentários — 🔜 VAI ACONTECER, e agora tem ORDEM e BRIEFING (ago/2026)

Era "último da fila, bisturi, branch própria", sem data e sem números. **Continua valendo — o Gabriel
foi explícito: *"vai ser feito isso primeiro e depois voltamos nos outros arquivos e vamos ajustar
SIM"*.** O que mudou é que ela deixou de ser um item vago e ganhou ordem, medição e uma regra que
impede a vazão de repor o que ela tirar.

**A ORDEM (decisão do Gabriel):**
1. **Separar o `jogo.js`** (§DÍVIDA ANOTADA). Cada cenário que se move **já sai reduzido** — a faxina
   dos 29% pega carona no movimento, em vez de ser um segundo passe pelos mesmos arquivos.
2. **Depois, os OUTROS arquivos** — o C# e o que sobrar do front. Aí sim como trabalho próprio.

Fazer o C# antes seria trabalhar contra a ordem: 29% dos comentários vivem no `jogo.js`, que vai ser
picado de qualquer jeito, e limpá-los antes do movimento é fazer duas vezes.

**A REGRA DE ESCRITA, que é a outra metade** (`CLAUDE.md` §Comentário): sem ela a faxina se refaz
sozinha. A conta que provou: no PR da cura em área (#207) eu escrevi **28 linhas de comentário pra 11
de código** (70%, quase 3× a média do repo) e expliquei a MESMA armadilha três vezes — comentário,
commit e ROADMAP —, o que pela regra da casa de que duas cópias divergem fabrica comentário mentiroso
futuro. O piloto (#208) cortou aquele bloco de 28 pra 14 sem perder nada acionável: saiu a HISTÓRIA
(que o git guarda melhor) e a ÊNFASE de defender-o-PR; ficou a regra e a mina.

**O BRIEFING — o estoque, medido em ago/2026:**

| | linhas | comentário | código |
|---|---|---|---|
| C# (209 arquivos) | 15.024 | **3.959 (26%)** | 9.117 (60%) |
| `jogo.js` | 11.920 | **3.563 (29%)** | — |

Dos 3.959 do C#, **3.440 são `///`** e só 519 são `//` inline. Ou seja 87% não é "comentário" no
sentido que incomoda — é documentação de API, o contrato entre camadas.

**A razão comentário/código é métrica MENTIROSA aqui.** O topo do ranking é `IPulaTurno.cs` (14
linhas: 11 de comentário, 2 de código), `IAtaquePrimario.cs`, `ICapacidadesStatus.cs` — interfaces de
capacidade cujo valor inteiro é a prosa que diz quando disparam e por que não são a mesma família da
vizinha. **Faxinar por ranking destrói o melhor primeiro.**

**COMO fatiar, quando chegar a vez** — o risco desta faxina não é o corte, é o TAMANHO do diff: 209
arquivos, ZERO verificação possível (nenhum teste pega comentário apagado errado) e perda
irrecuperável se a mão pesar, porque os becos sem saída são o conhecimento mais caro daqui e os que
mais parecem supérfluos pra quem não os viveu. Então: **por camada ou por pasta, um PR por fatia**,
nunca tudo de uma vez — e o critério é a tabela do `CLAUDE.md`, não o olho.

**As três categorias de gordura, em ordem de custo** — a primeira dá pra atacar a qualquer momento,
inclusive antes da separação, porque é verificável:
1. **Comentário que MENTE.** Custa mais que cinquenta verbosos: em ago/2026 o §OS FIOS QUE FALTAM
   dizia que 3 fios estavam abertos, e eu apresentei ao Gabriel trabalho pronto como pendência.
   Um PR de caça a MENTIRA (comentário citando `#NNN` já mergeado, nome de classe/método que não
   existe mais, TODO de coisa feita) é barato e **verificável** — o nome existe ou não —, ao
   contrário de "esse comentário é supérfluo", que é gosto e vira discussão infinita.
2. **Narração histórica** — 138 linhas no C# + 178 no `jogo.js`. Destino: mensagem de commit.
3. **Narrar o óbvio** — a menor das três.

---

## CONCLUÍDO (referência)

- **Revide-com-habilidade:** `ResultadoReacao.Revide` (Habilidade + Alvo) substitui
  `RevidarAlvo: Combate?`. `IAtivavelComNatureza` (A1, Marretada) executa o revide
  polimorficamente. Loop A↔B quebrado por profundidade explícita no executor (não mais por
  `NaturezasDano.Revide`/`TipoReacao.SemContraAtaque`, removidos — enum virou só
  `{ Completa, Nenhuma }`). Operário unificado com ContraAtaque (mesmo fluxo, troca A1 por
  Marretada); gap de multi-fonte (buff + passiva simultâneos) aceito conscientemente.
- **Sistema de Natureza do Dano** (NaturezaDano + TipoReacao + perfis). Base de tudo.
- **ContextoCombate** (Atacante/Aliados/Inimigos) — habilidades recebem o contexto rico.
- **PR-C — reações via interface** (C1-C6): Sedento, Reflexo, Sangramento, Espinhos, ContraAtaque
  migrados pra IReageAo*. Revide orquestrado (Forma 1, profundidade 1).
- **C7 — limpeza:** removidos os 3 hooks mortos do StatusEffect + EventoCombate.AntesDeReceberDano.
- **C5 completo — todas as passivas migradas:** lado "ao ser atacado"; lado atacante (OlhoClinico,
  Virus, Sorrateiro, Policial); ao matar (Fada, Vilao); Robo + Sushiman; Necromancia (IReageAoMorrer);
  Genio, BonecoDeNeve, Tengu, Elfo (IReageAoInicioTurno); Guarda (IReageAntesDeMorrer — Passo 4).
  Operario migrado (revide unificado com ContraAtaque, ver "Revide-com-habilidade"). Sistema
  velho aposentado.
- **Atos do Turno [Passo 3]:** ExecutarAtos centraliza o fluxo pós-Ativar. Ordem: AtoReacaoDoAlvo
  → IReageAntesDeMorrer → AtoMorte (IReageAoMatar + IReageAoMorrer) → AtoReacaoDoAtacante →
  AtoEncerramento. Irritar unificado (passava só AtoMorte, agora passa todos os Atos).
- **Guarda limpa [Passo 4]:** IReageAntesDeMorrer criada; Guarda migrada do hack IReageAoMorrer;
  ProcessarReacoesAntesDeMorrer inserido em ExecutarAtos antes do Vilão. Bug Vilao+Guarda corrigido.
  ContextoReacao.Outro renomeado para Contraparte (19 arquivos).
- **Crítico exige dano:** golpe cujo dano efetivo total (HP + escudo) deu 0 não é crítico no
  EventoDano — foi negado (bloqueio total). Escudo consumido conta (é vida). Beneficia todos os
  consumidores de FoiCritico na fonte.
- **EventoDano (Fatia 2):** ContextoReacao enriquecido (FoiCritico, Aliados, Inimigos do portador).
  Os 4 métodos de reação recebem os times; ProcessarReacoesAlvo inverte a perspectiva.
- **EventoDano (Fatia 1):** ResultadoAtaque convergiu em EventoDano (record rico do golpe).
  ReceberDano retorna (Efetivo, AbsorvidoPeloEscudo). Atacar monta o evento. Base da exibição
  rica no porte (Propósito B).
- **Unificação do ignorar (✅ jul/2026):** a natureza virou lista (`NaturezaDano.Ignora`), o
  `DeveAgir` foi REMOVIDO (interface + 6 impl), o ReceberDano ficou com 1 gate de lista só
  (natureza ∪ golpe ∪ champ). Anti-StackOverflow de proteção mútua agora estrutural
  (`DanoIndireto.Ignora ∋ ProtecaoAliado`). `NaturezasDano.Direto` deletado (órfão). Paridade
  provada por 5 testes. (Antes: o passo 1 introduziu o `DeveAgir`; agora ele morreu no passo final.)
- **Capacidade C (IContribui*, generalizada FILA A #8):** os 4 stats (Ataque/Defesa/TaxaCrit/
  DanoCrit) somam a interface de contribuição com sinal; matriz de status simétrica (buff+debuff
  por stat). Sem inconsistência (camadas distintas; `Sum` == `FirstOrDefault` pois não-empilham).
- **fix Veneno tick:** dano do tick é 5% fixo (não × Stacks); acúmulo só na Explosão.
- **fix Save defensivo:** trata JSON corrompido com fallback, no limite de I/O.
- **TurnoDoPersonagem (relógio)** extraído. ADR em docs/. Falta reset 1x-por-agressor + evento de
  início + TimeAtualDoTurno (cruzam C5).
- **Seleção de Alvo:** regra → SelecaoDeAlvoService; UI → MenuService; bot → EscolherAlvoBot. ADR
  em docs/.
- **Capacidades B + E:** IModificaDanoRecebido e IBloqueiaStatus.
- **Stats em Camadas** (Ataque/Defesa/Crit sob demanda).
- **Bloquear-revive promovido a flag** (PodeReviver/BloquearRevive). Vilao migrado pra
  IReageAoMatar. (Ver "Estado morto" — a modelagem evolui no rebalanceamento.)
- **Limpeza de branches remotas** (GitHub) + auto-delete ativado.

---

## NÃO FAZER (decisões conscientes de NÃO refatorar)

- ~~**Separar mensagens de combate do MenuService.**~~ REVERTIDO — foi FEITO (jul/2026): o
  `MenuService` virou `MenuView` + `CombateView`. A razão antiga ("morre no porte") caiu porque a
  camada View é justamente o que se troca no porte Unity; separar deu organização + o seam do front.
- **Centralizar descrições das habilidades.** A descrição mora na habilidade (coesão correta).
- **try-catch no núcleo de combate.** Domínio controlado; exceção lá seria bug mascarado.
- **Refatorar as ativas preventivamente.** Só se a auditoria achar dor real.
