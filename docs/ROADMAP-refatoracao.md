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
  libera conforme a campanha desbloqueia apóstolos), criar/editar/excluir conta, "sair" confirmado, modal
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
  ver `docs/MANUAL-front.md`.
- **Rebalanceamento, 1ª passada** (#189): o primeiro ajuste feito **com número na mão** em vez de
  intuição, lendo a bancada (`docs/MANUAL-combate.md`). Dois eixos: **cooldown padronizado em 3** (os de 4
  turnos eram penalidade escondida — a habilidade boa aparecia menos, e a coluna `Usos` denunciava) e
  **multiplicadores de dano subindo de faixa** (a maioria de 1.5–3.0 pra 3.0–4.5, porque o boneco com
  DEF no cap engolia os golpes baixos: eles apareciam como se não existissem nas linhas 2 e 5). 36
  apóstolos tocados. De carona vieram o **bug do hit-all** — o guard do `ResolverAlvos` cobrava a
  semente, e como hit-all não tem pick o `CombateService` manda o próprio atacante de placeholder;
  os 5 revive-de-todos (Robô, Sereia, Anjo, Palhaço, Diabo) explodiam com `InvalidOperationException`
  justamente quando havia alguém pra reviver, e sem mortos o early-return salvava antes (por isso
  passava) — e as **colunas de área** nas duas linhas de apóstolo inteiro da bancada: medir contra 1
  boneco SUBESTIMA o apóstolo de área (os malefícios do Detetive valem +9.504 contra 1 alvo e +30.888
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
  decisão do jogador —, e a **conquista do apóstolo** animada terminando na ficha dele.
- **Cenário por capítulo** (branch `feature/tema-do-reino`): o `EstadoDeBatalha` carrega um `Tema`, o
  JS põe em `body[data-tema]` e a batalha ganha um lugar. A ESTRUTURA da luta não muda em nada — os
  dois lados, o log, o painel, as animações e os tamanhos seguem os mesmos. O quarto tema (🪬 Folclore)
  trouxe o **maestro**: uma CAUSA COMUM que as peças leem, em vez de cada uma só com o seu relógio.
  Ver `docs/MANUAL-cenario.md`.

**Arquitetura (detalhe em `docs/MANUAL-front.md`):** ponte de mensagens LOCAL in-process (JS↔C# pela webview,
sem HTTP). O **motor da luta ficou INTOCADO** — só as telas trocam, pelos seams `ITelaDeCombate`/
`IControladorDeTurno`/`IApresentacao`/`IRepositorioDeSave`. Padrão consolidado: cada modo entra por um
`Executar...ComTime(s)` (a casca pica o time e chama), e a lógica META (recompensa/save da campanha)
mora na Application (`CampanhaService`), nunca no front.

**REBALANCE (#16): EM ITERAÇÃO, não fechado.** A bancada é o instrumento e a 1ª passada (#189) já
entrou; o trabalho agora é o laço `editar número → dotnet test → git diff docs/bancada-dano.md`,
quantas voltas o Gabriel achar que precisa. Os fios de combate que este parágrafo listava como
abertos (sweep de composição, turno-resto, passiva-conta-mortos) estão TODOS fechados — ver
§ONDE ESTÁ CADA COISA, que é o índice do que resta.

---

## CENÁRIO POR CAPÍTULO → mudou de casa: `docs/MANUAL-cenario.md`

O manual de como se faz uma pele de facção (as três camadas, ladrilho × canvas, o maestro, as
armadilhas, as lições de desenho e o que cada pele ensinou) saiu daqui: eram 977 linhas — ~40% de um
arquivo que se chama FILA — e manual não é fila. Foram junto as duas ferramentas que a separação
deixou (`rodar-tema.js`, `medir-donos.js`) e a armadilha do CSS por tema.

O que ainda é TRABALHO está na FILA A: a pele da Arena (o 1º tema que não é facção) e a 9ª pele
(Humanos), bloqueada até o fundo de facção no compêndio.

A história da separação do `jogo.js` (11.921 → 192 linhas) saiu inteira — ela está nas mensagens dos
PRs #204–#213, que guardam melhor e datado. O mapa e o contrato finais do front estão no `CLAUDE.md`.

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
   `ReducaoAtaque`, `ReducaoTaxaCrit`, `BuffDanoCrit`, `ReducaoDanoCrit` (só infra — apóstolos ligam
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
    time×controle×classe → **seam do modo VERSUS** (hoje a Arena). Refactor puro, 54 testes. Medidor de
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
    apóstolo, tem que gritar* — não "consertar" escondido. O guard mora DEPOIS do early-return de
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
15. ✅ **Faxina de comentários — FECHADA PELA MEDIÇÃO** (ago/2026), e não pelo corte. A separação
    terminou; a medição de novo, arquivo por arquivo, diz que **não há gordura pra tirar**:

    | | comentário |
    |---|---|
    | `telas/` (7 arquivos) | **3% a 22%** |
    | `nucleo/` + `ui/` | 7% a 24% (os altos são arquivos de 11–80 linhas, onde o doc É o produto) |
    | `cenarios/` (8 facções) | 21% a 36% — e são MECANISMO, não história |

    As telas nunca foram gordas; a densidade que assustava vinha do cenário, e lá o comentário é a
    única coisa que diz QUE FORMA está sendo desenhada (código de canvas é trigonometria). Fui atrás
    de narração histórica com filtro e achei **25 linhas em 9.700**; abri os doze maiores blocos
    contíguos e todos eram mecanismo ou MINA. **Cortar ali não enxuga, cega.**

    O que de fato saiu foi o que a MUDANÇA introduziu — o cabeçalho duplicado do `ladrilho.js` e um
    parágrafo de história — mais os três comentários que a separação transformou em MENTIRA no
    `jogo.js`. O que fica valendo é a regra de escrita (`CLAUDE.md` §Comentário), que é a vazão.

    **A lição, e ela vale além daqui:** razão comentário/código é métrica mentirosa. Ela é alta
    justamente onde o comentário é o produto — as interfaces de capacidade no C#, os builders de
    canvas no front. Faxinar por ranking destrói o melhor primeiro.
16. 🔄 **REBALANCEAMENTO — EM ITERAÇÃO** (1ª passada mergeada no #189). Não é um item que "termina":
    a bancada (`docs/MANUAL-combate.md`) é o instrumento, e cada volta é ler os números e mexer numa
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

17. ✅ **RENOMEAR A FACÇÃO: `Apostolos` → `Ascendentes`, símbolo 🌬️ → ❄️** *(ago/2026, vem da §LORE;
    feito junto com as correções da lore)*. **Era o PRIMEIRO dos dois** — ver o item 18.
    **Por que:** o jogo se chama *Apostle's War* e **todo herói é um apóstolo**, então não pode
    existir uma facção com esse nome. E o 🌬️ era o símbolo da **Cindy**, a deusa — uma facção estava
    usando o símbolo dela como se fosse próprio. Detalhe em `docs/LORE.md`.
    **O que mudou (zero lógica):** o valor do enum `Faccao` + o `[Description]` · o símbolo em
    `Faccoes.cs` · a chave de tema `body[data-tema]` · a pasta `wwwroot/cenarios/ascendentes/` (o
    `.js`, o `.css` e o `<link>` do index) · a entrada do `AR_DO_TEMA` · a pasta
    `ApostlesWar.Domain/Apostolos/Ascendentes/` (8 arquivos, namespace junto) · `ArsenalService`,
    `CapitulosService`, `PersonagemService` e 2 testes · o `TEMAS` e o cabeçalho do
    `ferramentas/separar-css.js` · e o ✝️ que **nunca existiu no jogo** (invenção minha em comentário
    no #204, espalhada por 17 lugares de prosa) virou ❄️ junto.
    **DUAS COISAS QUE VALE SABER ANTES DE RENOMEAR OUTRA FACÇÃO:**
    - **A chave de tema é DERIVADA do enum** (`FluxoDoFront.cs:431`,
      `faccao.ToString().ToLowerInvariant()`). Renomear o membro troca a chave sozinho, então pasta,
      CSS, `<link>` e registro têm de acompanhar no MESMO commit — senão o tema some **sem erro
      nenhum**, só cai no visual padrão.
    - **O save NÃO quebra.** Não há `JsonStringEnumConverter` em lugar nenhum, então o
      `System.Text.Json` grava enum como NÚMERO. Renomear o membro é seguro; **mudar a ORDEM da
      lista não seria.**
    **Escolha do ❄️:** é **tema de Natal**, que é a estética da facção (decisão do Gabriel) — e o
    anjo cabe nela pelo caminho curto: anjo → Natal → Jesus. Também não colide com os 8 símbolos em
    uso. (🪽 estava fora: já é o item de bota da própria facção; ⛄ é apóstolo; ⭐ é o Especial.)

18. ✅ **O NOME ANTIGO DO HERÓI JOGÁVEL VIROU `apóstolo` EM TODO O REPO** *(ago/2026, vem da
    §LORE)*. **904 substituições em 136 arquivos**, mais `Domain/Apostolos/` (78 arquivos movidos
    um a um) e o `ApostolosService.cs`. Zero lógica. Precisava vir DEPOIS do 17, senão a pasta
    nasceria dentro de uma homônima. **Os nomes antigos estão na mensagem do commit** — aqui não,
    porque um doc que ainda os escreve é um doc que ainda os ensina.

    **AS TRÊS COISAS QUE EU CHEQUEI ANTES DE TROCAR A PRIMEIRA LETRA** — é o que transforma um
    replace global de arriscado em mecânico, e vale repetir em qualquer renomeação grande:
    1. **Falso positivo.** Listei toda palavra que CONTÉM cada radical (`git grep -ohE` + `sort
       -u`): 26 compostos de um, 19 do outro, e **nenhum** era outra coisa. Se houvesse um, o
       replace o teria corrompido em silêncio.
    2. **O que vai pro SAVE.** JSON grava **nome de propriedade**, não nome de tipo. O record que é
       salvo tem `Faccao`/`Slot` como propriedades, então renomear o TIPO é invisível pro save. Uma
       `public int X { get; }` com o nome antigo num modelo salvo teria quebrado o save do jogador
       sem aviso nenhum.
    3. **O que atravessa a PONTE.** O `tipo` das mensagens, os ids do DOM e os records de View
       existem dos DOIS lados. Como o replace pega C#, JS, CSS e HTML na mesma passada, eles mudam
       em lockstep — e o `rodar-telas.js` confere os ids contra o `index.html`, que é a rede.

    **E O ACENTO É PARTE DO TRABALHO, não detalhe.** O replace escreve o identificador (sem acento)
    e isso vaza pra PROSA, que em português leva acento. Foram duas passadas extras: nos `.md`
    (pulando crase e bloco de código) e nos comentários do código (com um scanner que rastreia
    string e `/* */`, porque no código a mesma palavra solta é ora prosa, ora nome de método).
    **Strings que viram documento gerado contam como prosa** — as da bancada foram acentuadas à mão
    e o `docs/bancada-dano.md` regerado.

    **O 🦸 Herói NÃO entrou** (decisão do Gabriel: *"o Heroi é um personagem apenas ELE é Heroi"*).
    Ele é nome próprio, par do 🦹 Vilão, e vive em `Apostolos/Especial/Heroi/`. As duas "heróis"
    que trocaram são a fala do ritual no `LORE.md`, que é o conceito e não o personagem.

19. 🔄 **A PROGRESSÃO EM CÓDIGO** *(ago/2026 — em andamento)*. O desenho inteiro está no
    **`docs/GDD-progressao.md`**; aqui fica só a EXECUÇÃO. **Ler o GDD §2 antes de tocar em posição
    e o §7 pra ordem — não re-deduzir o modelo daqui.**

    **Feito e mergeado:**
    - ✅ **#228 — tipos + status base do tipo.** Os 108 números soltos viraram `Arquetipos`
      (4 fichas + 9 torções + a curva `1 + 29(nv−1)/59`). O crítico saiu de constante global.
      A ficha inteira (Tipo, Nível, Velocidade, Precisão, Resistência) chega na tela.
    - ✅ **#229 — o modelo da posição no GDD §2.** O portão do DD (`posicoesDeUso`/`posicoesAlvo`)
      morreu; a posição MODULA o dano.
    - ✅ **#230 — o campo virou fileira** com as frentes se olhando, casa sempre de ¼, stats
      empilhados, montagem espelhando o campo, casas numeradas.
    - ✅ **#231 — a preparação da fase virou tabuleiro** (seu time × rodada 1, rodada 2 embaixo),
      casas do inimigo com a mesma peça, e o tamanho do time virou parâmetro (`--casa`,
      `--casa-gap`, `--fileira`, `--tabuleiro`).

    - ✅ **O PERFIL DE DISTÂNCIA NO MOTOR** *(branch `feature/perfil-de-distancia`)*. A regra do GDD
      §2 virou código, e só motor — a pintura é o PR seguinte.
      - **A tabela mora no `Arquetipos`** (o d\* é do TIPO, é ficha): `DistanciaIdeal`,
        `DistanciaEntreCasas` e a função pura `MultiplicadorDePosicao(tipo, distancia)`. Ela conta
        em CENTÉSIMOS de propósito — `1.30 - 0.10 * 3` em double dá 0,9999999999999999, e o `(int)`
        do dano transformaria 200 em 199.
      - **`Combate.Casa`** (1 = frente … 4 = fundo) é preenchida pelo `IniciarCombate(casa)`, que
        agora exige o argumento. Quem posiciona é o `CombateService.Posicionar` — um lugar só, o
        índice em `Membros` + 1 —, chamado pelos três pontos de entrada (fase, rodada de inimigos,
        arena). **`Casa == ForaDoTabuleiro` (0) = sem geometria**, multiplicador 1,00: é o caso dos
        bonecos de teste, e é explícito em vez de um 1 chutado.
      - **Entra no `Atacar` e no `PreverAtaque`, no MESMO ponto** — dentro do `(int)` do `danoBase`,
        do lado do atacante, antes da mitigação. A `OrdemDeMitigacao` (#185) não foi tocada.
      - **O bot ficou esperto sem uma linha de bot**, como previsto: ele passa a mirar por
        geometria, e isso aparece no relatório (o ×1,30 das colunas de 4 alvos).
      - **A bancada regenerou, e a variação BATE com o multiplicador** — conferido célula a célula:
        **toda** mudança caiu em coluna de 4 alvos, dentro da faixa [1,00 … 1,30], com o grosso
        exatamente em **1,15** (a média das quatro casas). As colunas de 1 alvo não mudaram uma
        casa decimal, porque ali os dois estão na casa 1 e a distância é 1. **O que passou a mentir
        um pouco:** parte da `Sinergia (4)` agora é geometria e não composição — está avisado no
        cabeçalho do próprio relatório.

    **➡️ ESTE PR — O MAPA DE CALOR DA POSIÇÃO** (só pintura; nenhuma regra nova). Na PREPARAÇÃO da
    fase, que é onde a casa ainda é decisão: passar o mouse numa casa sua acende as casas inimigas
    com o multiplicador dele ali (cor + o número escrito, `×1,20`, escala divergindo no 1,00), e o
    **arraste** troca a leitura ao vivo — é o instante em que a decisão está sendo tomada.
    - **O front NÃO tem cópia da tabela:** o C# manda a **grade 4×4 por apóstolo**
      (`ApostoloVisto.Posicao[minhaCasa][casaDoAlvo]`), e o arraste vira troca de LINHA numa grade
      que já está na mão. *"Duas cópias de uma fórmula divergem como duas cópias de um número"* —
      essa já custou um defeito mudo nos Decaídos.
    - **Casa vazia não recebe leitura**: ninguém está lá pra apanhar, e um `×1,30` sem dono leria
      como oportunidade que não existe. Por isso as duas rodadas podem acender diferente.
    - **A leitura FICA no ar** depois que o mouse sai (só troca quando outra é pedida): comparar
      duas casas exige olhar o tabuleiro, e apagar no `mouseleave` levaria o número embora justo na
      hora de ler.
    - **🚫 NUNCA um NÚMERO de dano previsto** (decisão do Gabriel). Multiplicador é ficha e pode
      aparecer; dano previsto é simulação.

    **⚰️ O 2º BRILHO ("onde ele VAI bater mais forte") MORREU ANTES DE NASCER — decisão do Gabriel.**
    O desenho estava fechado e era barato: no passo de alvo, a **aura do CURSOR** ficava mais forte
    sobre quem sofreria mais, lendo o `VidaRemovidaPor` que o bot já calcula (DEF, escudo, bloqueio,
    Invencível e a geometria, tudo junto), normalizado no C# pra nenhum número de dano cruzar a
    ponte. **O que o matou foi o TOQUE:** aura de cursor não existe em tela sensível ao toque, e um
    clique ali significaria outra coisa. Se um dia o jogo for pra toque, o efeito volta como
    destaque nas CAIXAS dos alvos — e aí ele precisa se distinguir deste mapa de calor, que é
    exatamente a confusão que a versão do cursor evitava.

    - ✅ **O MEDIDOR DE TURNO — a fila única** *(branch `feature/medidor-de-turno`)*. O `for` sobre
      `Equipe1.Membros ++ Equipe2.Membros` morreu, e com ele a vantagem de time que ninguém tinha
      desenhado. **Não existe mais RODADA:** quem é rápido joga de novo antes de o lento jogar a
      primeira vez.
      - **A maquete VALIDADA foi a especificação**, e implementei literalmente
        ([artefato](https://claude.ai/code/artifact/8ab74f17-3630-4cfa-80f7-974a7de2c9eb)): salto
        EXATO até o próximo cruzamento (ninguém passa de 100 por avanço natural, então sobra só vem
        de empurrão ou do custo), o mais CHEIO age, desempate posição → lado do jogador.
      - **`FilaDeTurnos` (Domain)** guarda a regra inteira e as constantes; **`Combate.Medidor`** é a
        barra, gravável só por `AcumularMedidor`/`DescontarUmTurnoDoMedidor`. A **fração é
        adimensional** (10% do ciclo de referência) — o `0,05` seria o número certo na unidade errada.
      - **O TURNO EXTRA não passa pela fila**: não desconta medidor nem paga o custo. Se pagasse, o
        prêmio de jogar de novo encheria a barra de todo mundo — inclusive a dos inimigos.
      - **A BANCADA É A PROVA de que a troca é conservadora onde tem de ser:** com Velocidades
        iguais o desempate reproduz exatamente a ordem antiga, e o relatório só mexeu nas 5 linhas
        que já oscilavam por sorteio de alvo. A fila só morde quando as Velocidades diferem — que é
        o ponto.
      - ✅ **O CORDÃO DE TURNOS** *(branch `feature/fila-na-tela`)*: a ordem dos 8 próximos turnos,
        centralizada no alto do campo, com as fichas pequenas — leitura de canto de olho, não a
        atração da tela. As duas primeiras inteiras; da 3ª em diante cada uma entra POR BAIXO da
        anterior, metade escondida (a fila vem de trás), com o z-index descendo pela posição. O
        `Prever` roda as MESMAS funções do combate sobre um retrato copiado (`Passo`), e o teste que
        justifica a peça existir é o que compara a previsão com o que a batalha entrega de verdade.
        - **ARMADILHA QUE JÁ MORDEU:** o `body` é um grid de TRÊS linhas (topo · arena · painel).
          O cordão entrou como 4º item, virou a linha do `1fr` e empurrou a arena pra `auto` — o
          campo encolheu, o canvas do cenário ficou com o tamanho velho e a cena aparecia **cortada
          e subindo** a cada mudança de altura da fila. Peça nova entre o topo e o painel é
          **absoluta dentro da arena**, ou o cenário paga.
        - **O sinal de espera entre as fichas foi CORTADO** (pedido do Gabriel). O `Vez.Esperou`
          continua no motor e testado — é a regra dizendo onde a ordem é frágil, que é onde um
          empurrão de medidor vai valer —, mas não atravessa mais a ponte.
      - ✅ **A onda nova começa empatada** (decisão do Gabriel): o time do jogador tem o medidor
        zerado no início de cada rodada, como os inimigos — que já nasciam em 0 por serem
        posicionados de novo. Sem isso ele abria a 2ª onda com meio ciclo de vantagem.

    - ✅ **A DEF EM CURVA + PRECISÃO × RESISTÊNCIA** *(branch `feature/def-e-precisao`, o passo 4 do
      GDD §7)*. Os dois stats que estavam inertes passaram a valer.
      - **`DEF/(DEF+5000)`** no lugar de `min(DEF/1000 × 0,75; 0,75)`. A curva nunca satura e nunca
        chega a 100%, então o ponto de DEF não vira lixo depois de um número — com o item valendo
        `55 × capítulo`, DOIS deles saturavam a fórmula velha. `Combate.DefesaDeMeiaReducao` é o
        joelho, e é **o único número a calibrar** no balanço de defesa.
      - **DOIS PORTÕES no malefício, e são coisas diferentes** (decisão do Gabriel): a `chance` da
        própria habilidade continua existindo — é o incremento diferencial do rebalanceamento, o
        freio de habilidade roubada e o eixo por onde a RARIDADE vai diferenciar kits — e ela
        MULTIPLICA a disputa `min(100%, Precisão ÷ (Resistência × 2))`. Auto-malefício não rola nada.
      - **A segunda rolagem** apara 1 turno com `(1 − colar) ÷ 2`, com piso de 1 turno; ela morre
        junto com a primeira, então quem chega a 100% de Precisão cola sempre E cola cheio.
      - **A BANCADA MUDOU INTEIRA, e desta vez é sinal:** o boneco de defesa saiu do "cap" (1000, que
        na curva nova vale 16,7%) pro **joelho** (5000 = 50%), senão a linha de defesa pesada deixava
        de ser pesada. Os bonecos ganharam **Resistência 0** — mesmo motivo do crítico 100%: a
        bancada mede o KIT, e chance de aplicar entre 0 e 1 mediria o dado.
      - **Falta na TELA:** o `chance de aplicar: 75%` ao mirar, que **some** quando chega a 100%
        (GDD §1). Nenhum número de dano previsto — só a chance.

20. 🔜 **A pele da ARENA** — o 1º tema que NÃO é facção, e é isso que ele custa: a chave do tema
    deixa de ser `faccao.ToString()` e passa a ser só um NOME. Uma linha de C# (`FluxoDoFront.cs`, o
    `_sessao.Tema = ""` do *"laboratório não tem cenário"*, decisão REVOGADA pelo Gabriel), um bloco
    de CSS e uma entrada no `AR_DO_TEMA`. **Que cena é essa continua em aberto — é desenho, e é
    dele.** O manual e a regra da assinatura: `docs/MANUAL-cenario.md`.

21. 🔒 **A 9ª pele (HUMANOS) — BLOQUEADA de propósito**, e quem destrava é o **fundo de facção no
    COMPÊNDIO**: clicar num personagem passa a mostrar o cenário da facção DELE. Sem isso o tema só é
    pedido pela batalha, e não há capítulo dos Humanos — a pele nasceria sem lugar onde aparecer.
    Ordem (é dependência, não gosto): compêndio primeiro, pele depois. Ver `docs/MANUAL-cenario.md`.

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
  Nota: contra-ataque com A1 usa a animação de A1 do PRÓPRIO apóstolo (já roda a habilidade A1 real
  via `AtivarComNatureza`, então o vínculo mapeia natural).
- **`SaveSteam` / `SavePlayGames`** — impls da porta `IRepositorioDeSave` (Steamworks.NET /
  Google Play Games plugin). Steam Auto-Cloud ≈ zero código; Play usa Snapshots.
- **[bônus] apóstolo-como-dado → ScriptableObjects** (o motor de Ações já está pré-moldado pra isso).

**Comum aos dois portes (a "leva de eventos"):**
- **Encapsular coleções → choke-point de evento de status** (gatilho da camada de eventos).
- **`EventoDano` por ID** — desacoplar dos objetos `Combate` vivos → log/stream limpo.
- **#7b — cura/veneno/queima visíveis** (o `EventoCura` + mensagens) alimenta essa camada.

### 🟠 FILA C — DESCARTADA (nenhum servidor SQL/REST próprio)
- Cloud save é via SDK de plataforma (Steam/Play), plugado na porta `IRepositorioDeSave` — não é
  backend seu. SQL/REST só reabrem se um dia quiser contas/ranking/servidor próprio.

---

## As regras do motor → `docs/MANUAL-combate.md`

Saíram daqui, porque não eram fila — eram regra que continua valendo: a ordem do pipeline de dano
(`OrdemDeMitigacao`), a DEF do protetor no `ProtecaoAliado`, a língua única do ignorar-status, os 3
contextos, a ordem crítica de morte, os dois sabores do lado atacante, o cérebro do bot e a bancada
de dano. O DESENHO de cada uma está nos ADRs; o histórico dos PRs, no `git log`.

---

## A ponte e as camadas do front → `docs/MANUAL-front.md`

A arquitetura da pele saiu daqui: a ponte de mensagens local (não é MVC nem REST), o WebView2 e o
preço dele, o contrato de tela que é GATILHO e não desenho, e as 7 decisões que o #180 devolveu à
camada dona — inclusive a dívida do emoji no Domain, que segue registrada e NÃO paga (o gatilho é a
entrada dos sprites).

---

## ONDE ESTÁ CADA COISA (o índice, depois da arrumação de ago/2026)

| assunto | onde |
|---|---|
| o que fazer a seguir | a **FILA DE EXECUÇÃO** acima |
| as regras vivas do motor (dano, reações, turno, bot, bancada) | `MANUAL-combate.md` |
| a ponte, o contrato de tela e as camadas | `MANUAL-front.md` |
| como se faz uma pele de facção | `MANUAL-cenario.md` |
| o modelo do jogo (números, curvas, progressão) | `GDD-progressao.md` |
| o que ainda é ideia (itens, arena, morte-como-sistema) | `GDD-expansao.md` |
| o DESENHO de cada decisão grande | os `ADR-*.md` |
| o que foi tentado e MORREU, e quando | `git log` |

**O que resta no combate:** a passiva-conta-mortos tem a IRMÃ `EscalaComAbates` desenhada e não
construída (seção própria), a proveniência de status está registrada sem cliente, e o rebalance
(#16) segue em iteração. O resto dos fios fechou.
## Composição de Ações, C5 e capacidades — ✅ FEITOS

O sweep das 9 facções está COMPLETO: habilidade é DADO (lista de `Acao` rodada por um interpretador
único), **nenhuma ativa sobrescreve `Ativar`** (os 8 `override` que restam são `.Passiva.cs`, que é a
forma normal de passiva), o vocabulário mapeado esgotou e o `EstadoAlvo.Ambos` morreu. As 36 passivas
migraram pro modelo `IReageAo*` e o sistema velho foi aposentado; os 6 buffs-permanentes-por-contorno
viraram passiva pura e o Fantasma ganhou `Removivel = false`.

O desenho mora nos ADRs (`ADR-composicao-de-acoes.md`, `ADR-modelo-de-capacidades.md`); o
**vocabulário de Ações pra reusar** está no `CATALOGO-de-acoes.md` — ler antes de criar habilidade
nova, verbo compartilhado primeiro, bespoke só no 2º cliente.

**A disciplina que sobrevive ao sweep:** promover no 2º cliente REAL, e verificar antes de fundir
(o grep mente). Quando uma facção ESTREIA um mecanismo, aquele apóstolo é momento de DESIGN —
conferir em jogo com cuidado extra —, não sweep mecânico.
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

## EventoDano e a unificação do ignorar — ✅ FEITOS

O record rico do golpe, o `ContextoReacao` enriquecido, a natureza falando a língua da LISTA e o
`DeveAgir` morto. As regras estão em `docs/MANUAL-combate.md`; o `EventoDano` por ID segue na FILA B.
## Conceito de Turno — ✅ FEITO

`TurnoDoPersonagem` persistente, orçamento de reação por chave/agressor, `Equipe`/`Batalha` com
`PerspectivaDe`. A regra que ficou está em `docs/MANUAL-combate.md`; o desenho, no
`ADR-conceito-de-turno.md`. **Resta um boy-scout:** o disparo do evento InicioDoTurno das passivas
ainda mora no `DispararEventoInicioDeTurno` do CombateService — reavaliar se migra pro Turno.
## Proveniência de status — quem criou o status (FIO NOVO)

**Status:** REGISTRADO, futuro. Implementar quando o primeiro efeito que precisa aparecer.

**O que é:** todo StatusEffect carregar quem o criou (Aplicador/Origem), pra passivas filtrarem
"os status que EU criei". NÃO é o EventoDano (que descreve o golpe) — é a proveniência (de quem
é o status).

**Exemplo motivador (Gabriel):** escudo que reflete X% do dano de CADA escudo que ESTE apóstolo
colocou. Se o apóstolo põe escudo em vários aliados, reflete só dos escudos DELE; se outro
substituiu, a ligação quebra. Combo de time: A põe escudo, B aumenta valor+duração → escudo
sempre cresce e nunca acaba → dano refletido cresce junto. Precisa de: (1) Escudo carrega
Aplicador, (2) passiva filtra por origem, (3) regra "maior prevalece" (já existe).

**Precedente:** ProtecaoAliado.Aplicador, Irritar.Aplicador — alguns status JÁ rastreiam origem.

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
apóstolos):** mesmo TEMA, gatilho/persistência diferentes.
- Gatilho: `IReageAoMatar` (reage quando ESTE apóstolo mata; `ctx.Portador` = matador), não tabuleiro.
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
Program.cs, apóstolos na forma final) procurando o que discordar. **Veredito: a arquitetura está
sólida** — motor de 8 linhas, composition root limpo, cadeia de Atos da morte robusta (cada ato
re-checa EstaVivo — a Guarda reverter a morte e os posteriores não dispararem é design, não sorte).
**4 achados, todos encaixados na fila:**
1. **Seams violados (→ itens 3 e 6):** sobraram 4 `Console.*` fora da View — `Console.Clear` no
   `CombateService.ExecutarTurno` (o único no coração do combate) e no `ControladorJogador`;
   `Console.WriteLine`+`Thread.Sleep` nos fallbacks de save do `ArsenalService`/`CapitulosService`.
2. **Mina latente no `ResolverAlvos` (→ item 14):** `resultado.Add(alvoSelecionado)` confia que a
   semente está nos `candidatos` filtrados; se não estiver (ex.: hab `Mortos` + hit-all → semente =
   atacante vivo), o vivo entra nos resolvidos e `IndexOf` devolve -1. VERIFICADO: nenhum apóstolo
   ativa esse caminho hoje (só DocesDeAbobora usa `Mortos`, com pick real que trata o vazio). Não é
   bug vivo — é contrato sem guarda. Guard de 1 linha + teste que o documente.
   - **EPÍLOGO (jul/2026): o "nenhum apóstolo ativa" ENVELHECEU e o guard virou crash.** O sweep dos
     apóstolos pra forma-construtor trouxe 5 revive-de-todos com exatamente essa forma (hit-all +
     `Mortos`): Robô/Technology, Sereia/Atlantis, Anjo/Céu, Palhaço/Circo, Diabo/Anjo Caído. Todos
     explodiam ao reviver — só quando havia alguém morto pra reviver (sem mortos, o early-return
     de candidatos vazios salvava antes). **Correção:** o guard só vale onde existe PICK; hit-all
     (`NumeroDeAlvos == int.MaxValue`) não tem — a semente é placeholder do `CombateService`, e o
     `ResolverAlvos` agora devolve os candidatos inteiros antes de cobrá-la. Lição de doc: um
     "VERIFICADO: nenhum apóstolo faz isso" é foto, não regra — quem adiciona apóstolos não lê o
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
1. **Separar o `jogo.js`** (✅ feita em ago/2026). Cada cenário que se move **já sai reduzido** — a faxina
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
1. **Comentário que MENTE.** Custa mais que cinquenta verbosos: em ago/2026 o índice de fios deste
   arquivo dizia que 3 fios estavam abertos, e eu apresentei ao Gabriel trabalho pronto como
   pendência.
   Um PR de caça a MENTIRA (comentário citando `#NNN` já mergeado, nome de classe/método que não
   existe mais, TODO de coisa feita) é barato e **verificável** — o nome existe ou não —, ao
   contrário de "esse comentário é supérfluo", que é gosto e vira discussão infinita.
2. **Narração histórica** — 138 linhas no C# + 178 no `jogo.js`. Destino: mensagem de commit.
3. **Narrar o óbvio** — a menor das três.

---

## NÃO FAZER (decisões conscientes de NÃO refatorar)

- ~~**Separar mensagens de combate do MenuService.**~~ REVERTIDO — foi FEITO (jul/2026): o
  `MenuService` virou `MenuView` + `CombateView`. A razão antiga ("morre no porte") caiu porque a
  camada View é justamente o que se troca no porte Unity; separar deu organização + o seam do front.
- **Centralizar descrições das habilidades.** A descrição mora na habilidade (coesão correta).
- **try-catch no núcleo de combate.** Domínio controlado; exceção lá seria bug mascarado.
- **Refatorar as ativas preventivamente.** Só se a auditoria achar dor real.
