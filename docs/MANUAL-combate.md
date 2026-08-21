# Manual do combate — as regras vivas do motor

> **Tipo:** referência viva do motor de combate.
> **Função:** o que continua valendo DEPOIS de implementado, e que o código sozinho não diz —
>   ordem, contrato entre peças, e a armadilha que quebra em silêncio se alguém "melhorar".
> **Como usar:** ler a seção do assunto antes de mexer no pipeline de dano, nas reações ou na
>   bancada. O DESENHO de cada decisão está nos ADRs; o histórico está no `git log`.
> **Origem:** eram nove seções do `ROADMAP-refatoracao.md`, cada uma misturando a regra que fica
>   com a narrativa do PR que a criou.

---

## A ordem do pipeline de dano — quem reduz de graça antes de quem gasta recurso

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

## Proteção de aliado — a DEF do protetor abate o redirecionado

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

## Ignorar status — uma língua só, a da LISTA

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
  agora tem **1 gate só**: `ignorados` = natureza.Ignora ∪ golpe ∪ apóstolo; o status pergunta "fui
  listado?". Nenhum número de dano mudou (tradução flag→lista fiel; provada por 5 testes de paridade).
- **Anti-StackOverflow de proteção mútua agora ESTRUTURAL:** `DanoIndireto.Ignora ∋ ProtecaoAliado`
  → o redirect (que usa DanoIndireto) não re-redireciona. Numa linha, sem depender de disciplina de
  perfil. (Era o `DeveAgir => Reacao != Nenhuma`.)

### Critério de autoria (o produto pro usuário — no CATALOGO)
De quem é a perfuração? essência do dano → perfil de `NaturezaDano`; só o golpe → `ignorarStatus`
no `Dano`; o apóstolo sempre → passiva `IIgnoraStatusNoAtaque`; % do stat DEF → `ignorarDefesaPct`.

### A defesa é MONTADA limpa, não somada-e-descontada
- **Defesa montada limpa:** a etapa 1 do ReceberDano agora monta `defesaEfetiva = DefesaComStacks +
  soma dos IContribuiDefesa NÃO-ignorados`, em vez de somar tudo (getter `Defesa`) e descontar os
  ignorados. Paridade exata (ContribuicaoDefesa já vem com sinal: BuffDefesa +, ReducaoDefesa −).
  Teste novo cobriu o buraco (nenhum teste exercitava a etapa 1 com ignore): def 400 + BuffDefesa
  furado = 700 de dano vs 550 sem furar.
- **ATENÇÃO (resolvido):** IContribuiDefesa não era "mina dual-source" (usa tipos concretos; passivas
  não entram na lista `ignorados`) — a montagem-limpa lidou com isso sem varrer passivas.

---

## Os 3 contextos — não confundir

- **ContextoCombate** (Atacante, Aliados, Inimigos) — das HABILIDADES.
- **ContextoReacao** (Portador, Contraparte, DanoCausado, Natureza, FoiCritico, Aliados, Inimigos) —
  das REAÇÕES. Aliados/Inimigos são do PORTADOR: inverte no lado do alvo, não inverte no atacante.
- **EventoDano** — descreve o GOLPE (não é contexto de quem reage). O tipo canônico do golpe, o
  "Model" que a apresentação consome; convergiu o antigo `ResultadoAtaque`.

**Por que o EventoDano é rico além do que o combate usa (Propósito B, decisão do Gabriel):**
investir cedo na fundação de EXIBIÇÃO, não só na lógica. Ele é a linguagem entre quem calcula e quem
desenha — uma stream que a camada de animação consome. Por isso `DanoBruto` e `AbsorvidoPeloEscudo`
existem sem nenhum efeito que REAJA a eles hoje: não são código morto, são o contrato do porte.
(Ver `EventoDano por ID` na FILA B.)

---

## A ordem crítica de morte

**Trocar a ordem aqui não quebra o build: muda quem vive.**

**ATUALIZADO (jul/2026 — fix do bug do Guarda):** prevent-death (`IPrevineMorte`, no `ConfirmarMorte`
dentro do `ReceberDano`) → IReageAoMatar (Vilao) → IReageAoMorrer (Necromancia). O Guarda **EVITA a
morte** (não reverte): consultado como CAPACIDADE no instante do golpe fatal, o portador segue Vivo
**com os status intactos** (nunca vira Morto). Se não previne, o Vilão bloqueia o revive antes da
Necromância tentar. **Bug corrigido:** antes o Guarda usava `AplicarRevive` (Vivo novo) e perdia todos
os debuffs/buffs; `IReageAntesDeMorrer` (só o Guarda implementava) foi REMOVIDA junto — código morto.
Ver ADR-estado-de-vida-e-atos §11.

---

## Os dois sabores do lado atacante

- **IReageAoAtacar** = efeito no PRÓPRIO atacante. Segue TipoAtaque: AoE = 1x, Sequencial
  = por hit. Lado a lado com ProcessarPassivasAtacante. [OlhoClinico, Virus]
- **IReagePorAtaque** = efeito POR ALVO atingido. Nx sempre. Dentro do foreach. [Sorrateiro,
  Policial]
ProcessarReacoesAtacante dividido em PorAlvo (dentro do foreach) e PorAtaque (segue
TipoAtaque). Ver "Dívidas" — a repetição do loop vira helper ColetarReacoes<T>.

---

## O cérebro do bot — e o espelho puro da fórmula de dano

O mesmo cérebro joga pelo inimigo e pelo jogador no modo Auto.

### `PreverDano` — a fórmula ganhou um espelho puro

Pra comparar alvos o bot precisa da fórmula REAL. O plano dizia "extrair a parte pura do
`ReceberDano`" — **não havia parte pura**: o `Escudo` consome pontos e se remove, e o
`ProtecaoAliado` chama `Aplicador.ReceberDano(...)`, então prever chamando o modificador **feriria
um aliado de verdade**. A `IModificaDanoRecebido` passou a ter DOIS métodos (`Modificar` aplica,
`Prever` é puro), sem default — capacidade nova é obrigada pelo compilador a responder as duas.
Nasceram `Combate.PreverDanoRecebido`, `PreverAtaque` (crit como VALOR ESPERADO, senão a Kunai com
`forcaCritico` seria subestimada) e **`PreverVidaRemovida`** = `clamp(dano, 0, HPAtual − pisoDeHP)`,
que resolve sozinha "evitar bloqueio de dano" e "evitar Invencível" (ambos dão ~0) e ainda sinaliza
o ABATE (`== HPAtual`). Comportamento idêntico, provado pelos 14 `ReceberDanoTests` intocados.

### A fila absoluta, e por que não é pontuação

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

### O botão Auto — o que é PEDIDO e o que é ESTADO

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

## A bancada de dano — o instrumento do rebalance

A dor que o Gabriel nomeou: pra saber qual habilidade está quebrada, teria que jogar 36 apóstolos × ~4
habilidades à mão. A bancada roda isso sozinha e escreve **`docs/bancada-dano.md`**, VERSIONADO — cada
tweak de número vira um `git diff` legível. A entrega é o RELATÓRIO, não ajustar valores.

**Ela só foi possível porque habilidade é DADO.** O `Detetive.Espionagem()` é uma fábrica PRIVADA — a
bancada nunca a chama e nem poderia. Mas o `Definir()` já executou a fábrica e guardou a instância em
`Personagem.Habilidades`, então varrer os 36 apóstolos é um `foreach` sobre `TodosOsApostolos()`. Se as
habilidades fossem métodos, seria reflection ou uma lista de 144 nomes escrita à mão, que envelheceria
no primeiro apóstolo novo. **O refactor pra dados pagou por si aqui.**

**Colunas** (pedido do Gabriel na 2ª rodada): além de `Usos`/`Dano`/`Dano por uso`, cada linha por
habilidade traz **`Dano (4 alvos)`** — a mesma medição com 4 bonecos, que é o que dá voz às habilidades
de ÁREA (contra alvo único elas ficam indistinguíveis de single-target; agora medem 4,0× exatos,
enquanto as de alvo único seguem 1,0×) — e **`Cura`**, que exigiu uma condição nova: **o apóstolo começa
cada turno com 1 de vida.** Sem isso a coluna seria toda zero (cura não cura quem está cheio). De
quebra é a condição em que aparece quem fica mais FORTE ferido: a Caveira escala `2.0 − HP%` e o
Ossinho dela mede 638 = 200 × **1,99** × 1,6. O apóstolo carrega a mesma prevenção-de-morte do boneco pra
não morrer de auto-dano em 1 de HP. **O HP virou IGUAL nos dois lados** (2.000): cura costuma ser % do
HP máximo, então inflar o apóstolo estouraria a cura pelo mesmo motivo que inflar o boneco estourava o DoT.

**Duas vistas dos mesmos dados:** a tabela agrupada por apóstolo responde "como é o kit deste
personagem?"; os **rankings** no fim (burst, sustentado com área, cura) respondem "quem está fora da
curva?" sem obrigar a varrer 144 linhas com o olho.

**Cinco linhas, variando UM fator por vez** (desenho do Gabriel) — é o que torna as subtrações legíveis:

| # | Modo | DEF do alvo | Recebe malefício? | O que a subtração isola |
|---|---|---|---|---|
| 1 | por habilidade | 0 | não | dano cru |
| 2 | por habilidade | cap | não | **(2)−(1) = o que furar/reduzir DEF vale** |
| 3 | apóstolo inteiro | cap | não | **real − esperado = a SINERGIA do kit** |
| 4 | apóstolo inteiro | cap | **sim** | **(4)−(3) = o que os malefícios valem** |
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
- **Durante o cooldown o apóstolo ESPERA, não usa A1.** Se enchesse o buraco com A1, toda habilidade
  carregaria ~75 ataques básicos junto e todas ficariam parecidas.

**Validação:** o A1 mede 320 = ATK 200 × 1,60 (o `DanoCritBase`), provando que o crítico está cravado.
E a história do Mago sai decomposta: Bola de Fogo isolada com alvo imune = 4000; com malefício = 11500
(7500 são o tick da Queima); o apóstolo inteiro salta de 11000 pra 19750, e o ~1250 que sobra é a passiva
Piromancer — que só rende quando OUTRA habilidade bate no alvo já queimado, e por isso o isolado nunca
a veria.

**Limitação declarada no próprio relatório:** o boneco **nunca age**, então contra-ataque, espinhos,
revide e passivas de apanhar (Herói, Operário, Zumbi, Troll) medem ZERO. É bancada de dano CAUSADO, não
de duelo — apóstolo com número baixo pode ser reativo, não fraco. A coluna **Usos** é diagnóstico do BOT:
habilidade que dispara 0× no apóstolo inteiro mas pontua alto isolada acusa a fila do bot, não o balanço.

**Ela é OPT-IN e o `dotnet test` comum a PULA** — só roda com `$env:BANCADA=1` (ver
`FatoDaBancadaAttribute`). Uma corrida custa ~63 s e reescreve o relatório; sem ela a suíte leva 0,6 s.

---

## O turno do personagem — e os DOIS relógios

O `Combate` POSSUI o seu `Turno` (`Combate.Turno`, criado no ctor, vive o combate todo). Estado
turn-scoped mora nele; duração e cooldown são do COMBATENTE (persistem, o turno só avança).

**Reset "1x por agressor por turno" do CONTRA-ATAQUE — ✅ FEITO.** O registro de quem já foi
contra-atacado saiu dos HashSets privados (ContraAtaque tinha o seu, Operário nem tinha) e virou a
regra única `TentarContraAtacar(agressor, chance)` (chance + "1x por agressor", registra no sucesso),
limpa no Finalizar. Fonte única — buff ContraAtaque, PassivaHeroi e PassivaOperario passam TODOS por
ela, então o gap multi-fonte (Herói com buff do Dragão + passiva) morreu: o primeiro registra, o
segundo vê que já contra-atacou. O hook `StatusEffect.AoPassarTurno` (virtual usado só pelo
ContraAtaque, o único "capaz virtual sem irmã interface") foi REMOVIDO. Herói virou passiva-pura
(IReageAoSerAtacado, sem buff via IPassivaInicial); Operário ganhou o limite 1x/agressor.

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
  o domínio consome. Renasceu por um futuro NOVO que o Gabriel nomeou — o modo **VERSUS**, que virou a Arena. O
  que se fez: `Combat/Batalha.cs` (`Equipe { Membros }` + `Batalha { Equipe1/2, EquipeDe, OponenteDe,
  PerspectivaDe, Combatentes }`), mora no CombateService (rebuild por rodada, como o RelogioDoCombate).
  `PerspectivaDe(portador)` é o "um só caminho": derivada da ESTRUTURA (qual equipe), matou os 3 flips
  manuais (`aliadosDoAlvo = inimigosDoAtacante` + a recursão do revide colapsou numa pergunta só). O
  `is Jogador`/`is Inimigo` do fluxo saiu: **time** = `PerspectivaDe`; **controle** = mapa
  `Dictionary<Equipe, IControladorDeTurno>` (campanha: E1→humano, E2→bot; Versus troca o mapa);
  **apresentação** (UX de preparação) = "controlador é bot". As 5 `ProcessarReacoes*` + `ExecutarAtos`
  perderam os params de perspectiva. Refactor PURO (54 testes). Sobrou o `is Jogador` em `AtaqueBasico`
  (contexto próprio) — não é fluxo, fica.

**NÃO confundir com o `RelogioDoCombate`**, que é o contador GLOBAL de rodadas, um nível acima ("boss
mata todos após X turnos"). São dois relógios em níveis diferentes.

---

## O revide carrega a HABILIDADE

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
