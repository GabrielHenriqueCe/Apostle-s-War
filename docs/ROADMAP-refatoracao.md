# Roadmap da Refatoração — Apostle's War

> **Tipo:** Backlog técnico vivo (índice mestre)
> **Função:** bússola entre sessões. Lista tudo que falta até o fim da
>   refatoração, priorizado. Aponta para os ADRs de cada tema.
> **Como usar:** risque o que concluir, adicione o que descobrir. Cada tema
>   grande ganha seu ADR próprio em docs/ quando for executado.
> **Atualizado:** junho/2026

---

## Princípios que guiam toda a refatoração

- **Todo método morto na maioria das classes que o herdam vira interface** (ISP).
  Base magra + interfaces por capacidade. A declaração da classe documenta o que faz.
- **Refatore por DOR, não por pureza.** Migra o que incomoda; o resto segue por
  boy scout (quando tocar) ou PR dedicado quando virar dor.
- **Refatore o que PERSISTE; tolere imperfeição no que vai MORRER.** A camada de
  apresentação do console (MenuService, telas) e o save em arquivo morrem na
  migração web (2027). Não investir rigor neles. A LÓGICA de domínio (combate,
  turno, reações, stats) vira a .NET API — é onde o rigor rende.
- **Um PR, um tema.** Não abrir duas frentes grandes ao mesmo tempo.
- **Exception nos LIMITES (I/O, parsing externo), nunca no NÚCLEO** (lógica de
  domínio — se lança lá, é bug, deve estourar pra corrigir).
- **Build verde (Ctrl+Shift+B) antes de todo push.** CI manual do dev solo.

---

## FAZER AGORA / PRÓXIMO

### 1. Conceito de Turno (TurnoDoPersonagem) — PRÓXIMO GRANDE TEMA
**Status:** conceitos fechados, falta ADR + implementação.
**Dor:** o turno está diluído em 5 métodos do CombateService (ExecutarTurnoCompleto,
ExecutarInicioDeTurno, AvancarStatus, AvancarCooldowns, check de Preso), sem dono.
O estado (duração de status, cooldown, "1x por agressor") vive espalhado.

**Conceito fechado:**
- Turno = 3 fases: **Início → Ação → Fim**.
- Início/Fim são "relógio" (mecânico, automático, sem decisão; igual pra jogador e bot).
- Ação é "vontade" (decisão + UI + bot; diferente pra cada um).
- **Escopo (Opção 1.5):** o TurnoDoPersonagem é dono do RELÓGIO — expõe `Iniciar()`
  (tick de status, eventos de início, futuro reset do "1x por agressor") e
  `Finalizar()` (avança duração de status, avança cooldowns). O CombateService chama
  esses EM VOLTA da Ação. A Ação fica no service porque arrasta dependências
  (menu/bot/UI/seleção de alvo) que o Turno NÃO deve conhecer — senão o Turno vira
  outro CombateService (God Object renomeado).

**Fronteira de responsabilidade (por dependências):**
- Início/Fim dependem só de status + cooldown do combatente → vão pro Turno.
- Ação depende de UI/input/bot → fica no CombateService.

**O que migra pro Turno (do StatusEffect/CombateService):**
- A INVOCAÇÃO de `AoIniciarTurno` e `PassarTurno` dos status (hoje no CombateService).
- O `AvancarCooldowns` (hoje percorre Cooldowns no service; SkillCooldown continua
  sendo a classe que encapsula o cooldown — só a orquestração migra).
- O `AvancarStatus` (avança duração + remove expirados).
- (Futuro C5b) o reset do "1x por agressor por turno".

**O que NÃO migra (continua no StatusEffect — é estado/mecanismo do próprio status):**
- `Turnos`, `TurnosRestantes` (duração é dado do status; buffs decidem "maior
  duração prevalece", debuffs stack sincronizam com Stacks).
- `AoPassarTurno` (hook interno do status pra reagir ao avanço — ex: ContraAtaque
  reseta HashSet).

**Próximo passo:** escrever `docs/ADR-conceito-de-turno.md`.

### 2. Seleção de Alvo — separar regra de UI
**Status:** auditado, falta ADR + implementação. Tema próprio (depois do Turno).
**Dor:** no CombateService, mistura regra de domínio com UI e bot:
- `ResolverListaDeAlvosDisponiveis` → REGRA de targeting (Provocar > sem Intocável/
  Bloqueio > sem Bloqueio > todos). Domínio puro. SOBREVIVE à web.
- `EscolherAlvoDaLista` → UI (desenha lista, ReadKey). MORRE na web.
- `EscolherAlvoAleatorio` → lógica de bot.

**Direção:** extrair a REGRA de targeting (domínio) pra perto do Combate ou um
serviço próprio; a UI fica no MenuService; o bot vira estratégia. Benefício extra:
isola o que persiste (regra) do que será descartado (UI).
**Próximo passo:** escrever `docs/ADR-selecao-de-alvo.md`.

### 3. Save defensivo (fix barato, AGORA)
**Status:** fix pequeno e isolado.
**Dor:** `CapitulosService.CarregarProgresso` e `ArsenalService.CarregarItensEquipados/
CarregarItens` desserializam JSON do disco SEM tratamento. Save corrompido (jogador
editou, versão antiga, disco falhou) → JsonSerializer lança → jogo CRASHA na
inicialização.
**Fix:** try-catch no carregamento com fallback ("save inválido → começa do zero").
Exception aqui é correta: é um LIMITE (I/O + parsing externo), não o núcleo. O
conceito de "carregar defensivamente com fallback" SOBREVIVE pro mundo SQL.
**Branch:** fix/carregamento-save-defensivo.

---

## PAUSADO (depende de pré-requisitos)

### 4. C5 — padrão de reações das passivas
**Status:** PAUSADO. Buffs reativos já migrados (PR-C, C2-C6). Passivas continuam no
DeveAtivar/enum (que FUNCIONA). Estado estável.
**Por que pausou:** os casos complexos do C5 dependem de Turno e EventoDano:
- O reset "1x por agressor por turno" (decidido: igual ContraAtaque) precisa do
  conceito de Turno pra morar no lugar certo.
- Sushiman (precisa FoiCritico + Aliados), stat-builders → precisam do ContextoReacao
  rico (EventoDano).
**Ordem de dependência:** Turno → EventoDano → C5.
**O que falta no C5 (quando voltar):**
- Fundação: base HabilidadePassiva (DeveAtivar/Ativar viram virtual); dispatch varre
  StatusAtivos + Personagem.Habilidades por interface (com check de cooldown pro lado
  das Habilidades); coexistência sem dupla execução.
- Migrar passivas simples lado-alvo (1x por agressor): Zumbi, Coco, Palhaco, Ogro,
  PapaiNoel, TRex, Mimico, Cientista.
- Stat-builders (reagem ao ser atacado, ganham stat, têm Estado): Ambicao, CoroaDoSoberano, Diabo.
- DepoisDeReceberDano: Alien, Sushiman (este precisa do contexto rico).
- Lado atacante (IReageAoAtacar, FALTA criar): OlhoClinico, Virus, Sorrateiro, Policial, Robo.
- Interfaces a criar: IReageAoAtacar, IReageAoMorrer, IReageAoMatar, IReageAoInicioDoTurno(?),
  IReageUmaVezPorAgressor (marcadora de frequência).
- Casos especiais: Operario (contra-ataca + Console.WriteLine furando camada; lógica
  central = igual ContraAtaque, nuance = escolher A1/A2/A3); Necromancia (IReageAoMorrer
  + cooldown); Guarda (HACK de revive — intenção real = "impedir morte" via Invencivel,
  Categoria B; consertar se fácil, senão ajustar passiva); Fada/Vilao (IReageAoMatar);
  BonecoDeNeve/Genio (início de turno).
- Aposentar DeveAtivar/enum EventoCombate só quando TODAS migrarem.
- Espinhos/ContraAtaque (já migrados) largam HashSet próprio e usam a política central.
**Decisão firmada:** Espinhos/Zumbi/Coco = "1x por agressor por turno" igual ContraAtaque
(não por-hit). O controle dessa frequência é reusável (não copiado em cada efeito).

### 5. EventoDano — contexto rico das reações
**Status:** FUTURO (depois do Turno, antes/junto do C5).
**O que é:** o ContextoReacao atual (Portador, Outro, DanoCausado, Natureza) é magro.
EventoDano = registro rico (dano bruto/efetivo/absorvido, FoiCritico, atacante, alvo,
natureza, aliados). Evolução natural do ContextoReacao. Alimenta também a versão web
(front consome eventos).
**Fila de eventos (Forma 3):** adiada no C6. Nasce com o EventoDano. O revide hoje usa
Forma 1 (recursão profundidade 1, segura pela natureza Revide) e migra pra fila quando
o EventoDano vier.

### 6. RelógioDoCombate — enrage / limite de turnos
**Status:** FUTURO, YAGNI. Lugar reservado, não implementar agora.
**O que é:** conceito VIZINHO do TurnoDoPersonagem, num nível ACIMA (combate/rodada).
Conta os turnos GLOBAIS do combate e dispara eventos em marcos (turno X → boss mata
todos / enrage; limite de turnos da fase → anti-stall). É o relógio do COMBATE, não o
turno de um personagem — relógios diferentes, níveis diferentes (SRP).
**Por que reservar agora:** players otimizam até quebrar o jogo (combos infinitos);
o jogo VAI querer rédea (enrage timer é padrão clássico). O ADR do Turno só precisa
NÃO IMPEDIR isso — deixar claro que o RelógioDoCombate mora no nível do ExecutarCombate,
não dentro do TurnoDoPersonagem. Implementa quando uma fase concreta pedir.

---

## BOY SCOUT (quando tocar) / FUTURO ARQUITETURAL

### 7. Capacidades — intervenção-dano / stat / bloqueio viram interface
**Status:** ADR já existe (docs/ADR-modelo-de-capacidades.md). Migração incremental.
Reações (Categoria A) em curso. As outras categorias migram por boy scout ou PR dedicado:
- B) Intervenção no dano → IModificaDanoRecebido (Escudo, BloqueioTotal, Invencivel,
  ProtecaoAliado, ReducaoDanoFixo). Hoje é método virtual ModificarDanoRecebido.
- C) Stat sob demanda → IContribuiDefesa / IContribuiAtaque / IContribuiCrit
  (BuffDefesa/ReducaoDefesa já espelhados via ContribuicaoDefesa).
- E) Bloqueio de aplicação → IBloqueiaStatus (ImunidadeDebuffs, ImunidadeEspecifica,
  ImpedirBeneficios). Hoje é método virtual Bloqueia.
- D) Comportamento de turno (Medo/Preso/Irritar) → baixa prioridade, avaliar se vale
  interface ou se a consulta atual basta.

### 8. Persistência — arquivo → SQL (futuro web)
**Status:** FUTURO (versão web 2027). Registrar direção, não fazer agora.
**Problema atual:** os Services acessam arquivo DIRETO (File.ReadAllText + JsonSerializer
dentro de CapitulosService/ArsenalService). Acoplamento à persistência espalhado. Quando
virar SQL, mexe em vários services.
**Direção (para a web):** isolar persistência atrás de um REPOSITÓRIO (interface tipo
IRepositorioProgresso / IRepositorioItens) que hoje lê arquivo, amanhã lê SQL. A troca
arquivo→SQL passa a mexer num lugar só. YAGNI até a web chegar — mas a direção fica
registrada pra não acoplar mais ainda nesse meio tempo.
**Nota:** o save em arquivo é da versão console (morre na web). Só o fix barato do
"save defensivo" (item 3) vale agora; refatorar pra repositório é trabalho da web.

---

## CONCLUÍDO (referência)

- **Sistema de Natureza do Dano** (NaturezaDano + TipoReacao + perfis). Base de tudo.
- **PR-C — reações via interface** (C1-C6): Sedento, Reflexo, Sangramento, Espinhos,
  ContraAtaque migrados pra IReageAo*. Revide orquestrado (Forma 1, profundidade 1).
- **C7 — limpeza:** removidos os 3 hooks mortos do StatusEffect (AoReceberDano/
  AoSerAtacado/AoCausarDano) + EventoCombate.AntesDeReceberDano.
- **fix Veneno tick:** dano do tick é 5% fixo (não × Stacks); acúmulo só recompensa na
  Explosão (Putrefação). Queima já estava correta.
- **ADR modelo de capacidades** salvo em docs/.
- **Stats em Camadas** (Ataque/Defesa/Crit calculados sob demanda, buffs/debuffs/
  permanentes como camadas).

---

## NÃO FAZER (decisões conscientes de NÃO refatorar)

- **Separar mensagens de combate do MenuService.** É camada de apresentação do console
  — MORRE na web. Não investir.
- **Centralizar descrições das habilidades.** A descrição mora na própria habilidade
  (coesão correta). Extrair pra arquivo de strings (100+ entradas) é over-engineering e
  quebra coesão. Separar demais também é exagero.
- **try-catch no núcleo de combate.** Domínio controlado; exceção lá seria bug
  mascarado. Manter o núcleo limpo.
