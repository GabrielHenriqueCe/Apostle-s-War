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
quantas voltas o Gabriel achar que precisa. Os fios de combate que aquele parágrafo listava como
abertos (sweep de composição, turno-resto, passiva-conta-mortos) estão TODOS fechados — ver §OS FIOS
QUE FALTAM, que é o índice do que resta.

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
   é DADO (lista de Ações) rodada por um interpretador único. A conferência: os **76 arquivos de apóstolo
   estão todos em `Apostolos/<Faccao>/<Apostolo>/`** (nenhum solto), e os únicos `override Ativar` que
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
do Mago) e **forma-construtor + apóstolo-como-arquivo FEITOS** (Mago piloto em `Apostolos/Reino/Mago/`).
Ver **ADR-composicao-de-acoes.md**. É a "Auditoria das ativas" com dor real: ~70% das ativas
são só dado (loop + lista fixa de efeitos), reinventando boilerplate. Predecessor do
Rebalanceamento (mexer em número/efeito vira editar dado, não 74 classes).

**Decisões novas (jul/2026, pós-motor):**
- **Fusão do Nível A no sweep:** cada PR de facção migra os apóstolos direto pra FORMA FINAL
  (pasta `Apostolos/<Faccao>/<Apostolo>/`, habilidades como métodos `static HabilidadeAtiva X() =>
  new(...)`, passiva movida junto, classes velhas deletadas, linha do `PersonagemService` vira
  `Apostolo.Definir()`). Uma passada por apóstolo em vez de duas — e a VIEW do apóstolo chega facção a
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
  RemoverDebuffs, MoverBuffs, ConcederTurnoExtra. *(Vocabulário esgotado desde os Ascendentes.)*
- **Toda `Acao` declara `Utilidade` + `TemEfeitoUtil` + `PreverVidaRemovida`** (jul/2026, PR do bot):
  o que ela FAZ, se tem trabalho agora e quanto machuca — tudo respondido pela própria ação. É o que
  permite AVALIAR uma habilidade sem executá-la. `Utilidade` é abstrata: ação nova (inclusive bespoke
  de apóstolo) é obrigada pelo compilador a se classificar, em vez de o avaliador dar `switch` no tipo.
- Disciplina: promove no 2º cliente REAL; verificar-antes-de-fundir (o grep mente — **Copiando
  era Balde 3 e é vocabulário puro**; **Atlantis** revelou o boundary de "pipeline / conjunto
  afetado", 1 cliente, registrado sem construir).
- Invariantes: `TipoAtaque` alimenta dispatch de passivas-atacante; o interpretador agrega os
  `EventoDano` das ações de dano.

**Sequência:** #115 piloto per-alvo ✅ → #116 motor (loop-flip) ✅ → #117 forma-construtor +
Mago apóstolo-arquivo + rename passivas ✅ → #118 testes do motor ✅ → **Humanos ✅** (4 apóstolos na
forma final em `Apostolos/Humanos/`; `Reviver` nasceu no Nigiri — 1º da família dos 7; Marretada
é a 1ª híbrida `.Ativa.cs`; o Nigiri deixou de usar `Ambos`) → **Reino ✅** (Guarda/Ninja/Rei
migrados em `Apostolos/Reino/`, ao lado do Mago piloto; `AplicarEscudo` nasceu Ação de
vocabulário — Lealdade, já estava mapeada em §5.1 (como "Escudo") mas sem cliente até agora
(nome `AplicarEscudo`, não `Escudo`, pra não colidir com `Skills.Buffs.Escudo` — o namespace
raiz `ApostlesWar` é envolvente de quase todo o código); `Dano` ganhou
`ignorarDefesaPct`/`forcaCritico` opcionais — Kunai; Shuriken estreou a 1ª Ação bespoke Nível 3,
`GolpeSeguidor`, acoplamento hit-a-hit lido via `eventos`) → **LadoSombrio ✅** (Caveira/
Fantasma/Abóbora/Zumbi migrados em `Apostolos/LadoSombrio/` — momento de design, estreou 4
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
**Ascendentes ✅ — SWEEP DAS 9 FACÇÕES COMPLETO** (Boneco de Neve/Mímico/Anjo/Papai Noel — 100%
vocabulário puro, ZERO bespoke; `MoverBuffs` construído [gêmeo do RemoverBuffs, cliente Copiando] e
com ele o vocabulário mapeado esgotou; Imitação = `Dano(Func)` [molde Tengu]; Céu = 7º/último do revive
e último apóstolo com `Ambos` → agora NENHUM apóstolo usa `Ambos`; fio §9 fechado com Repetindo deixada como
está [3ª de 3, igual AnáliseCrítica/Policial]) → **sweep segue** (unificar-ignorar → pick do menu/§8.2
quando o `Ambos` morrer). Revive 7/7 (Nigiri, DocesDeAbobora, Tecnology, Circo, Atlantis, AnjoCaído,
Céu). Quando uma facção ESTREIA um mecanismo, o apóstolo é momento de
design (verificar em jogo com cuidado extra), não sweep mecânico.

---

## C5 — padrão de reações das passivas (✅ COMPLETO)

As 36 passivas migradas pro modelo de interfaces `IReageAo*`, sistema velho aposentado. A ordem
crítica de morte e os dois sabores do lado atacante estão em `docs/MANUAL-combate.md`.
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

O record do golpe existe e o `ContextoReacao` já é rico (FoiCritico, Aliados, Inimigos). A distinção
dos 3 contextos está em `docs/MANUAL-combate.md`; o `EventoDano` por ID segue na FILA B.

---

## Unificar os 3 mecanismos de ignorar status — ✅ CONCLUÍDO (jul/2026)

A regra que ficou está em `docs/MANUAL-combate.md`.

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
Você×Você hotseat, Bot×Bot**) → monta os 2 times com **TODOS os 36 apóstolos** (pool independente do
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

**Exemplo motivador (Gabriel):** escudo que reflete X% do dano de CADA escudo que ESTE apóstolo
colocou. Se o apóstolo põe escudo em vários aliados, reflete só dos escudos DELE; se outro
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
  (natureza ∪ golpe ∪ apóstolo). Anti-StackOverflow de proteção mútua agora estrutural
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
