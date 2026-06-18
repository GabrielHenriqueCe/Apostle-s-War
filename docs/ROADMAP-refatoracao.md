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

### 1. Conceito de Turno (TurnoDoPersonagem) — PARCIALMENTE FEITO
**Status:** RELÓGIO FEITO. TurnoDoPersonagem extraído do CombateService (ADR em
docs/ADR-conceito-de-turno.md): expõe Iniciar() (tick dos status) e Finalizar()
(avança duração de status + remove expirados + avança cooldowns). O CombateService
chama os dois em volta da Ação.
**FALTA (cruza o C5 — "o inevitável adiado"):**
- O reset "1x por agressor por turno" — mora no Turno, mas serve as reações do C5.
- O disparo do evento InicioDoTurno das passivas — hoje ficou no CombateService
  (DispararEventoInicioDeTurno) porque depende do sistema de passivas DeveAtivar/
  enum, que o C5 vai migrar.
Ambos serão feitos na branch que mexe em Turno + C5 JUNTOS (o ponto onde os dois
temas se cruzam, que decidimos não forçar a separar).

**Conceito fechado (referência):**
- Turno = 3 fases: Início → Ação → Fim. Início/Fim = "relógio" (mecânico); Ação =
  "vontade" (decisão/UI/bot, fica no service).
- O que NÃO migrou (continua no StatusEffect): Turnos, TurnosRestantes (duração é
  dado do status), AoPassarTurno (hook interno de reação ao avanço).

### 2. RelógioDoCombate — enrage / limite de turnos
**Status:** FUTURO, YAGNI. Lugar reservado, não implementar agora.
**O que é:** conceito VIZINHO do TurnoDoPersonagem, num nível ACIMA (combate/rodada).
Conta os turnos GLOBAIS do combate e dispara eventos em marcos (turno X → boss mata
todos / enrage; limite de turnos da fase → anti-stall). É o relógio do COMBATE, não o
turno de um personagem — relógios diferentes, níveis diferentes (SRP).
**Por que reservar:** players otimizam até quebrar o jogo (combos infinitos); o jogo
VAI querer rédea (enrage timer é padrão clássico). Mora no nível do ExecutarCombate,
não dentro do TurnoDoPersonagem. Implementa quando uma fase concreta pedir.

---

## PAUSADO (depende de pré-requisitos)

### 3. C5 — padrão de reações das passivas
**Status:** PAUSADO. Buffs reativos já migrados (PR-C, C2-C6). Passivas continuam no
DeveAtivar/enum (que FUNCIONA). Estado estável.
**Por que pausou:** os casos complexos do C5 dependem de Turno e EventoDano:
- O reset "1x por agressor por turno" (decidido: igual ContraAtaque) precisa do
  conceito de Turno pra morar no lugar certo. (Turno-relógio já existe; falta o reset.)
- Sushiman (precisa FoiCritico + Aliados), stat-builders → precisam do ContextoReacao
  rico (EventoDano).
**Ordem de dependência:** Turno → EventoDano → C5. (Turno-relógio feito; falta o
reset + EventoDano antes do grosso do C5.)
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
- Disparo do evento InicioDoTurno das passivas (hoje em DispararEventoInicioDeTurno no
  CombateService) — reavaliar se vai pro Turno quando as passivas migrarem.
- Aposentar DeveAtivar/enum EventoCombate só quando TODAS migrarem.
- Espinhos/ContraAtaque (já migrados) largam HashSet próprio e usam a política central.
**Decisão firmada:** Espinhos/Zumbi/Coco = "1x por agressor por turno" igual ContraAtaque
(não por-hit). O controle dessa frequência é reusável (não copiado em cada efeito).

### 4. EventoDano — contexto rico das reações
**Status:** FUTURO (depois do Turno, antes/junto do C5).
**O que é:** o ContextoReacao atual (Portador, Outro, DanoCausado, Natureza) é magro.
EventoDano = registro rico (dano bruto/efetivo/absorvido, FoiCritico, atacante, alvo,
natureza, aliados). Evolução natural do ContextoReacao. Alimenta também a versão web
(front consome eventos).
**Fila de eventos (Forma 3):** adiada no C6. Nasce com o EventoDano. O revide hoje usa
Forma 1 (recursão profundidade 1, segura pela natureza Revide) e migra pra fila quando
o EventoDano vier.

---

## BOY SCOUT (quando tocar) / FUTURO ARQUITETURAL

### 5. Capacidades — intervenção-dano / stat / bloqueio viram interface
**Status:** ADR em docs/ADR-modelo-de-capacidades.md. Migração incremental.
- A) Reação após evento → interfaces IReageAo* — buffs FEITOS (PR-C); passivas no C5.
- B) Intervenção no dano → IModificaDanoRecebido ✅ FEITO (Escudo, BloqueioTotal,
  Invencivel, ProtecaoAliado, ReducaoDanoFixo). Método virtual morto removido da base.
- E) Bloqueio de aplicação → IBloqueiaStatus ✅ FEITO (ImunidadeDebuffs,
  ImunidadeEspecifica, ImpedirBeneficios). Método virtual morto removido da base.
- C) Stat sob demanda → IContribuiDefesa / IContribuiAtaque / IContribuiCrit
  (BuffDefesa/ReducaoDefesa já espelhados via ContribuicaoDefesa). PENDENTE (boy scout).
- D) Comportamento de turno (Medo/Preso/Irritar) → PENDENTE, baixa prioridade,
  avaliar se vale interface ou se a consulta atual basta.

### 6. Persistência — arquivo → SQL (futuro web)
**Status:** FUTURO (versão web 2027). Registrar direção, não fazer agora.
**Problema atual:** os Services acessam arquivo DIRETO (File.ReadAllText + JsonSerializer
dentro de CapitulosService/ArsenalService). Acoplamento à persistência espalhado. Quando
virar SQL, mexe em vários services.
**Direção (para a web):** isolar persistência atrás de um REPOSITÓRIO (interface tipo
IRepositorioProgresso / IRepositorioItens) que hoje lê arquivo, amanhã lê SQL. A troca
arquivo→SQL passa a mexer num lugar só. YAGNI até a web chegar — mas a direção fica
registrada pra não acoplar mais ainda nesse meio tempo.
**Nota:** o save em arquivo é da versão console (morre na web). O fix do "save
defensivo" (já feito) cobre o risco imediato; refatorar pra repositório é trabalho da web.

### 7. Services-lookup (cosmético, baixa prioridade)
**Status:** observado na auditoria, sem dor. FaccaoService e CampanhaService são
basicamente tabelas (dicionário/lista + getters, sem comportamento real). Candidatos a
virar dados em vez de service. Cosmético — fazer só se incomodar. YAGNI.

### 8. Bloquear-revive temporário / "status do estado morto" (FUTURO, não desenhar agora)
**Status:** ideia registrada, YAGNI. Não construir até os efeitos pedirem.
**Hoje:** "impedir revive" é a flag `PodeReviver` + `BloquearRevive()` (permanente,
irreversível). O Vilao bloqueia; o AnjoCaido (Diabo) ignora via exceção pontual na
própria habilidade. Dois casos — a flag basta.
**A ideia (Gabriel):** se o jogo quiser bloqueio de revive TEMPORÁRIO/removível
(estilo Raid: duração, cleanse, resistência), a flag não basta. Modelar como um
"status do ESTADO MORTO" — uma categoria de status separada da lista do vivo, pra
NÃO ser pega pela ImunidadeDebuffs (que protege o vivo; foi o bug original que tirou
MortePermanente de Debuff). Essa categoria também acomodaria cenários complexos
(contra-revive, ignorar-bloqueio seletivo).
**Por que não agora:** só 2 efeitos hoje (Vilao bloqueia, AnjoCaido ignora). Construir
um sub-sistema de "status da morte" pra 2 casos é desenhar no escuro. A interface/
categoria nasce quando os efeitos que a justificam existirem.

---

## CONCLUÍDO (referência)

- **Sistema de Natureza do Dano** (NaturezaDano + TipoReacao + perfis). Base de tudo.
- **PR-C — reações via interface** (C1-C6): Sedento, Reflexo, Sangramento, Espinhos,
  ContraAtaque migrados pra IReageAo*. Revide orquestrado (Forma 1, profundidade 1).
- **C7 — limpeza:** removidos os 3 hooks mortos do StatusEffect (AoReceberDano/
  AoSerAtacado/AoCausarDano) + EventoCombate.AntesDeReceberDano.
- **fix Veneno tick:** dano do tick é 5% fixo (não × Stacks); acúmulo só recompensa na
  Explosão (Putrefação). Queima já estava correta.
- **fix Save defensivo:** CarregarProgresso/CarregarItensEquipados tratam JSON corrompido/
  ilegível (JsonException/IOException) com fallback (começa do zero + aviso), em vez de
  crashar. Tratamento no limite de I/O, não no núcleo.
- **TurnoDoPersonagem (relógio)** extraído do CombateService (Iniciar/Finalizar). ADR em
  docs/ADR-conceito-de-turno.md. Falta o reset 1x-por-agressor + evento de início (cruzam C5).
- **Seleção de Alvo:** regra de targeting → SelecaoDeAlvoService (injetável, domínio puro);
  UI → MenuService.EscolherAlvoNaTela; bot → SelecaoDeAlvoService.EscolherAlvoBot (ponto de
  evolução p/ IA do AW v2). CombateService só orquestra. ADR em docs/ADR-selecao-de-alvo.md.
- **Capacidades B + E:** IModificaDanoRecebido (Escudo/BloqueioTotal/Invencivel/
  ProtecaoAliado/ReducaoDanoFixo) e IBloqueiaStatus (ImunidadeDebuffs/ImunidadeEspecifica/
  ImpedirBeneficios). Métodos virtuais mortos removidos da base StatusEffect.
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
