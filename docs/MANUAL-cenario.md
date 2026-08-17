# Manual do cenário — como uma pele de facção é feita

> **Tipo:** manual de execução (referência viva).
> **Função:** tudo que se lê ANTES de desenhar um cenário novo. Cada linha aqui custou uma rodada
>   de "ficou ruim" em jogo — não invente processo por cima.
> **Como usar:** ler as ARMADILHAS inteiras antes de desenhar. Se for contra alguma, que seja de
>   propósito, e diga por quê.
> **Origem:** era a `## CENÁRIO POR CAPÍTULO` do `ROADMAP-refatoracao.md` — 977 linhas, ~40% de um
>   arquivo que se chama fila. Manual não é fila.

---

## Por onde começar — leia nesta ordem
Oito peles prontas (👑 Reino · 🌑 Lado Sombrio · ⚙️ Tecnológicos · 🪬 Folclore · 🐉 Místicos · ⭐ Especial ·
🔱 Decaídos · ❄️ Ascendentes), falta 1. **A conta das assinaturas está fechada:** dia claro (Reino), lua
(cemitério), estrelas (invasão), âmbar de fogo (Folclore), crepúsculo (praia), interior sem céu
(Especial), luz vinda de baixo (Inferno) e a paisagem vista por um RECORTE (a janela dos Ascendentes).
Pros Humanos não sobra HORA nenhuma — o caminho é o mesmo do ⭐ Especial e dos ❄️ Ascendentes: um LUGAR em
vez de uma hora, ou um enquadramento.
**Não invente processo: já existe manual, e ele é caro — cada linha dele custou uma rodada de "ficou
ruim" em jogo.**
1. **`docs/MANUAL-cenario.md`.** É O manual: as três camadas e o que
   decide em qual entrar, ladrilho × canvas × endereço, o MAESTRO (dado compartilhado), os motores
   já extraídos, a lista de ARMADILHAS (cada uma já custou tempo — leia todas antes de desenhar) e as
   LIÇÕES DE DESENHO. Se for contra alguma delas, seja de propósito e diga por quê.
2. **Uma pele pronta INTEIRA, das duas pontas:** o bloco `body[data-tema="misticos"]` no `estilo.css`
   e a entrada `misticos` do `AR_DO_TEMA` no `jogo.js` (a mais completa: mar, dragão em três
   distâncias, lâmpada, aparições, moldura em canvas). O ⭐ Especial é a segunda melhor referência, e
   a única com INTERIOR, com peças que se ocultam entre si (porta × sentado, anel × cocô) e com o
   `comListras` — o padrão de "monta o caminho UMA vez, usa pra preencher E recortar". O 🔱 Decaídos
   é a mais barata de imitar em peça grande: ela é montada de MEMBROS sorteados num caminho
   só (`tracarMembro`, com o sentido do traço garantido por construção), o maestro sendo LUZ e não
   vento (`inferno.pulso`), e uma HISTÓRIA amarrando as peças umas nas outras. O ❄️ Ascendentes é a mais
   NOVA, e a de imitar quando a cena tiver um ROTEIRO: a noite inteira dele é uma sequência de passos
   (`criarRoteiroDaNoite`) escrita num maestro por uma camada que **não desenha nada**, e lida por
   quatro peças em cantos diferentes da tela. É também a única em que a paisagem é vista por um
   RECORTE, e ela custou TRÊS versões — as duas que morreram estão contadas no ROADMAP, que é onde o
   que não deu certo fica registrado.
3. **`git log --oneline` dos PRs de cenário** (#195, #197, #198, #199, #200, #201, #202, #203) — as
   mensagens contam o que foi tentado e MORREU, que é a parte que o código não mostra.

**A receita, em uma linha:** o tema é `faccao.ToString().ToLowerInvariant()` → `body[data-tema]`, e
custa **zero C#** — um bloco de CSS mais uma entrada de configuração. Tema sem CSS e sem entrada no
`AR_DO_TEMA` simplesmente luta no visual padrão (foi assim que o Folclore saiu inteiro no #199).

**Depois da separação (ago/2026) isso virou:** criar `wwwroot/cenarios/<faccao>/<faccao>.js` com
`export const ar = {...}` e os builders, e pôr **uma linha** no registro `AR_DO_TEMA` do `jogo.js`.
Mais o `<faccao>.css` na mesma pasta (e o `<link>` no index.html). **E rodar
`node --experimental-vm-modules ferramentas/rodar-tema.js` antes de pedir conferência em jogo.**

**As decisões que vêm ANTES de desenhar qualquer coisa:**
- **Que assinatura sobrou.** Dia claro é do Reino, lua do cemitério, estrelas da invasão, âmbar do
  fogo, crepúsculo da praia. Escolha o que SOBROU antes de escolher o que é bonito — é o que faz o
  capítulo ser reconhecível de relance.
- **Os 4 apóstolos entram pelo SINAL, não pela figura.** Nada de corpo humano (fica esquisito em canvas);
  o gênio é a lâmpada, a sereia é a cauda, a fada é o vaga-lume maior. E o gesto tem que ser do sinal:
  cauda sozinha fazendo salto de golfinho lê como pedaço arremessado.
- **Uma peça CENTRAL, uma fonte de luz.** Fogueira, lâmpada. E o que mais acontece na cena responde a
  ela ou ao maestro.
- **Verificação:** `node --check jogo.js`, chaves do `estilo.css` batendo, todo builder do
  `noFundo`/`naFrente` com definição, nenhuma chave de config sem uso, e `dotnet build` limpo. Raio
  negativo num `arc`/`ellipse` LANÇA e mata o `requestAnimationFrame` — a cena congela em silêncio.
  **Pior ainda: NaN em coordenada NÃO lança**, só não desenha (`Math.pow(negativo, fracionário)` é a
  fonte clássica). Vale montar a **bancada headless** do tema — extrair os builders com `eval` + um
  `ctx` de mentira que VALIDA cada argumento (raio ≥ 0, tudo finito, `save`/`restore` batendo) e
  rodar ~900s de `dt` fixo. Ela já pegou bug fatal em três peles seguidas.
- **A conferência em jogo é do Gabriel**, sempre: quase todo acerto deste front veio de ele olhar
  rodando e apontar o defeito exato. E **quando ele descreve um MECANISMO, implementar LITERALMENTE**
  — no ⭐ Especial eu interpretei quatro vezes e errei as quatro; o desenho dele estava certo desde a
  primeira frase.


## Ao renomear uma facção: o que é derivado e o que não é

A chave de tema NASCE do enum (`FluxoDoFront.cs`, `faccao.ToString().ToLowerInvariant()`), então o
`body[data-tema]`, a pasta `wwwroot/cenarios/<tema>/`, os dois arquivos dela, o `<link>` do index e a
entrada do `AR_DO_TEMA` têm de mudar no MESMO commit ou o tema some sem erro nenhum. **O save NÃO
quebra:** não há `JsonStringEnumConverter`, então enum vira NÚMERO no JSON — renomear o membro é
seguro desde que a ORDEM da lista não mude.

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
- Mostrar o SINAL, não a figura. Nenhum dos quatro apóstolos do Folclore é desenhado por inteiro em lugar
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
- **Dois apóstolos podem partilhar o BICHO, desde que não partilhem o COMPORTAMENTO** (jul/2026). 🦇
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
Inferno se abrindo no chão de tempos em tempos) e ❄️ Ascendentes (a SALA de Natal — o segundo interior,
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
❄️ Ascendentes. E há uma pista boa no que a Arena É: ela não é um capítulo da história, é onde se
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
ser propriedade da FACÇÃO — e a facção Humanos existe (são 4 apóstolos), com capítulo ou sem. É esse
recurso que cria a vaga para a 9ª pele, e é por ele que ela vai ser cobrada.

**A ordem é essa, e não é gosto — é dependência:**
1. **Separar o `jogo.js`** — ✅ FEITA em ago/2026. É ela que faz "um cenário" virar coisa que
   se pede por nome em vez de um trecho no meio de 11.920 linhas.
2. **O fundo de facção no compêndio**, que consome a separação: o compêndio vai querer montar o ar de
   um tema fora da batalha, e hoje quem faz isso é o `aplicarTema` chamado de dentro do fluxo de luta.
3. **A pele dos Humanos**, por último, quando já existir a tela que a mostra.

Fazer os Humanos ANTES seria pagar a pele duas vezes: ela nasceria no arquivo monolítico que o passo 1
vai picar, e nasceria sem a tela que decide o enquadramento dela. **E o enquadramento é justamente o
que está em aberto** — não sobra hora nenhuma (dia é do Reino, lua do cemitério, estrelas da invasão,
âmbar do Folclore, crepúsculo da praia, luz-de-baixo do Inferno), então a resposta tem de vir de um
LUGAR ou de um recorte, no caminho do ⭐ Especial e dos ❄️ Ascendentes. Ver a cena no compêndio primeiro
provavelmente responde isso sozinho.

**O que os ❄️ Ascendentes ensinaram**, além do que subiu nas listas acima. Esta pele custou TRÊS versões
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
carregar o apóstolo, a peça pequena que o carregava vira ruído** — e a economia certa é apagar, não
realocar.

**O que os 🐉 Místicos ensinaram** (jul/2026), além do que já subiu nas listas acima: três dos quatro
apóstolos da facção têm CORPO HUMANO (gênio, sereia, fada), e figura humana pequena em canvas fica
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

---

### As duas FERRAMENTAS que a separação deixou (ago/2026)

Nasceram pra este trabalho e ficaram versionadas, porque as duas continuam servindo.

**`ferramentas/rodar-tema.js` — o harness headless.** Carrega o front INTEIRO num navegador de
mentira, chama o `aplicarTema` de VERDADE e roda N segundos por tema com dt fixo (120s × 8 temas em
~1 min). Roda com `node ferramentas/rodar-tema.js`, e com `--experimental-vm-modules` agora que o
front é ES module — ele detecta script × módulo sozinho.

Ele existe porque bancada que monta os builders na mão **não vê exceção dentro do `iniciarAr`** — foi
assim que a colisão de chave `arvore` (Decaídos × árvore de Natal) deixou a cena em branco sem erro
visível. Pega três modos de falha, e os três foram PROVADOS por sabotagem antes de ele ser aceito:

| falha | por que é traiçoeira |
|---|---|
| raio NEGATIVO em `arc`/`ellipse` | LANÇA no canvas real e mata o `requestAnimationFrame` — a cena congela |
| exceção na MONTAGEM | a cena nem nasce, e não há erro na tela |
| **NaN em coordenada** | o pior: **não lança**, só não desenha |

Na separação ele pegou duas cenas em branco em segundos: o `jogo.js` sem reimportar os builders que
tinham acabado de sair, e um módulo novo sem importar o `criarNoHorizonte` do comum.

**`ferramentas/medir-donos.js` — quem é dono de cada função.** Monta o grafo de chamadas, acha as
raízes de cada tema e tira o fecho transitivo. Tem um modo `--porque <tema> <funcao>` que mostra o
CAMINHO, e foi ele que salvou a medição.

**A primeira tabela saiu plausível e completamente errada** — 231 de 235 declarações como
"compartilhadas por 6 temas", incluindo funções de TELA. Quatro formas de aresta falsa, cada uma
capaz de colapsar o grafo sozinha:

1. **sombreamento por local** — `criarNinja` tem um `const sairDaTela` (a fuga dele pela lateral) que
   colide com a função de tela homônima. O ninja "chamava" a interface, a interface chamava o
   `aplicarTema`, e dali TODO builder ficava alcançável de TODO tema.
2. **sombreamento por parâmetro** — `criarNoHorizonte(cfg, canvas, desenhar)`.
3. **chave de objeto** — `criarChifres` entrega `{ acabou, desenhar }`. Nomear campo não é chamar.
4. **fronteira da declaração** — fechar cada uma no início da próxima fazia a ÚLTIMA engolir todo o
   rodapé do arquivo; era por isso que `desenharRena` aparecia falando com o C#.

O script imprime os sombreamentos que ignorou, pra a decisão ser auditável em vez de confiável.
**A moral:** "o grep mente" também vale pra ferramenta que se escreve pra conferir o grep — o jeito de
saber se ela mente é ler o CAMINHO, não o resultado.

---

### A ARMADILHA DO CSS, e as duas provas que a ferramenta faz (ago/2026)

Separar CSS **não é recorte-e-cola**, e é por isso que este passo veio por último e com ferramenta
própria (`ferramentas/separar-css.js`).

**A armadilha:** no arquivo único, a escada de `@media (max-height:...)` que encolhe os ladrilhos vem
DEPOIS do bloco base de cada tema. Arquivos de tema carregados após o `estilo.css` invertem a posição
deles em relação a ~490 linhas de CSS base que hoje vêm **depois** da região de temas. Onde a
ESPECIFICIDADE decide, ordem é irrelevante; onde ela EMPATA, a ordem é quem decide.

**Prova 1 — empates de especificidade.** Para cada propriedade declarada por regra de tema, procura
regra base posterior que declare a MESMA propriedade com especificidade IGUAL. Deu **zero**.

> A 1ª versão da função de especificidade tinha uma **alternativa vazia** numa regex (um `|`
> sobrando), e alternativa vazia casa em TODA posição do texto: a contagem virava o comprimento da
> string, dois seletores nunca empatavam, e o verificador respondia "0 empates" pra qualquer entrada.
> Ela ganhou **auto-teste no arranque** com seis seletores de especificidade conhecida — se mentir, o
> script morre antes de tocar em arquivo. E foi **sabotada** pra provar que pega: um
> `body.sabotagem { --painel: red }` (especificidade 101, a mesma de `body[data-tema="X"]`) fez os 8
> temas acusarem. **Verificador quebrado é pior que verificador nenhum: ele dá PERMISSÃO.**

**Prova 2 — nada perdido nem duplicado.** Conta o multiconjunto de seletores antes e depois. Deu 509
antes e **510** depois — e o seletor sobrando denunciou um bug real e MUDO: o bloco da escada carrega
junto o comentário de 20 linhas que a explica, e a abertura do `@media` estava sendo tomada como "a
primeira linha do bloco". O `@media (max-height: 1000px) {` era trocado pela primeira linha do
comentário, e as regras iam parar **dentro de um comentário nunca fechado**. O CSS seguia válido; o
primeiro degrau da escada simplesmente não existia, e o ladrilho parava de encolher sem nada acusar.

**A lição, que vale pra qualquer recorte de CSS:** um pedaço de CSS pode ficar sintaticamente válido
e semanticamente MORTO. Chave balanceada e arquivo que carrega não provam nada — quem prova é contar
as regras dos dois lados.
