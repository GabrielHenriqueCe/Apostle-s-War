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
- **Não desenhar no escuro.** Interface/categoria nasce servindo efeitos reais que
  existem, não casos hipotéticos. YAGNI até o efeito que justifica aparecer.

---

## C5 — padrão de reações das passivas (EM CURSO, quase no fim)

**Status:** AVANÇADO. As passivas reativas estão migrando do DeveAtivar/enum para o
modelo de interfaces (IReageAo*), ao lado dos buffs reativos (PR-C). Strangler Fig:
sistema velho coexiste com o novo; cada passiva migrada deixa de sobrescrever
DeveAtivar e some do sistema velho. Sem dupla execução (confirmado).

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

**FALTAM migrar (9):**
- **Operario** — IReageAoSerAtacado (interface existe). Contra-ataque 10% com
  Marretada 1.25x. Tem o furo de camada (Console.WriteLine + Thread.Sleep dentro da
  passiva) pra limpar. Ver "Fio do revide" abaixo — a migração esbarra na decisão de
  como o revide carrega a habilidade.
- **Necromancia + Guarda** — IReageAoMorrer (A CRIAR). Revive (capacidade vs efetivo),
  cooldown real (6 e 4, não 0), interação com o bloqueio do Vilao (já migrado), mensagens
  sobreviveu/morreu. Guarda é um HACK de revive — intenção real é "impedir morte" via
  Invencivel (Categoria B). Consertar se fácil; senão ajustar o conceito da passiva.
- **BonecoDeNeve + Genio** — IReageAoInicioTurno (A CRIAR). Cruza com o
  DispararEventoInicioDeTurno (evento de início que ficou no CombateService).
- **Robo + Sushiman** — BLOQUEADOS, esperam o EventoDano. Robo precisa de Aliados
  (ContextoReacao não tem; o Combate NÃO conhece o próprio time — confirmado, vem via
  ContextoCombate). Sushiman precisa de FoiCritico (ContextoReacao não tem).

### Interfaces de reação — estado
- IReageAoSerAtacado, IReageAoReceberDano, IReageAoCausarDano — existem (PR-C).
- IReageAoAtacar (1x por ataque, segue TipoAtaque), IReagePorAtaque (Nx por alvo),
  IReageAoMatar — CRIADAS nestas sessões.
- IReageAoMorrer, IReageAoInicioTurno — A CRIAR (Necromancia/Guarda e BonecoDeNeve/Genio).

### Dois sabores do lado atacante (decisão firmada)
"Reagir ao atacar" tem dois sabores distintos, que disparam diferente:
- **IReageAoAtacar** = efeito no PRÓPRIO atacante. Segue TipoAtaque: AoE = 1x (um golpe
  é um ato), Sequencial = por hit. Roda lado a lado com ProcessarPassivasAtacante
  (DepoisDeAtacar). [OlhoClinico, Virus]
- **IReagePorAtaque** = efeito POR ALVO atingido. Nx sempre (cada inimigo do golpe).
  Roda dentro do foreach de alvos. [Sorrateiro, Policial]
ProcessarReacoesAtacante foi dividido em PorAlvo (dentro do foreach) e PorAtaque (segue
TipoAtaque). A separação em dois métodos é correta (chamados em lugares diferentes do
fluxo). Ver "Dívidas" — a repetição do loop "varre StatusAtivos + ColetarPassivasReativas"
deve virar helper ColetarReacoes<T>.

### Ordem crítica preservada (morte/revive)
"Ao matar" (IReageAoMatar / ProcessarReacoesAtacanteMorte) roda ANTES de "ao morrer"
(Necromancia/Guarda). O Vilao bloqueia o revive antes da tentativa de reviver. O dispatch
novo foi chamado no mesmo ponto que o ProcessarPassivasAtacanteMorte (velho), preservando
a ordem.

### Aposentar o sistema velho
DeveAtivar/Ativar virtual e o enum EventoCombate só saem quando a ÚLTIMA passiva migrar.

---

## Fio do revide — revide carrega a HABILIDADE (decisão de design pendente)

**Status:** decisão tomada na direção, EXECUÇÃO depende do refactor das ativas. Registrado
pra não migrar o Operario de um jeito que conflite depois.

**O problema:** o revide hoje (RevidarAlvo no ResultadoReacao) carrega só "quem revidar" e
o executor faz Atacar(alvo, 1.0) — multiplicador 1.0 hardcoded. O ContraAtaque usa isso
(revide básico). Mas o Operario quer revidar com Marretada (1.25x), e no futuro outros
podem revidar com habilidades específicas.

**A decisão (Gabriel):** o revide deve carregar a HABILIDADE, não um multiplicador. Razão:
cada habilidade tem identidade própria (animação, nome, símbolo, eventual efeito). Carregar
só "1.25" perde a identidade — o jogo não saberia que foi uma Marretada (não mostra o nome/
animação certa). O AtaqueBasico já é uma habilidade, então "revidar com a1" e "revidar com
Marretada" são o MESMO conceito: revidar com uma habilidade.

**O obstáculo:** invocar a habilidade exige o multiplicador/efeito dela, que hoje está
ENTERRADO dentro do Ativar de cada ativa (a Marretada faz AplicarDano(..., 1.25) lá dentro,
sem expor). E o Ativar re-seleciona alvos (ResolverAlvos) + ataca com natureza normal — não
serve pra "dar um golpe específico neste alvo com natureza Revide". Então "revide carrega a
habilidade" DEPENDE de um refactor das ativas: expor a força (multiplicador) e aceitar uma
natureza no ataque, separando "qual o efeito" de "rodar no turno".

**Decisão para o Operario AGORA:** como o refactor das ativas é grande e o Operario é 1 caso,
fazer a Opção C provisória — o Operario executa Atacar(ctx.Outro, 1.25, natureza: Revide) e
DECLARA o resultado via ResultadoReacao(Mensagem + Dano), em vez de Console.WriteLine. Isso
já corrige o furo de camada (a passiva declara, o orquestrador exibe), sem mexer no contrato
do RevidarAlvo. Quando o refactor das ativas acontecer, o revide passa a carregar a
HabilidadeAtiva e o Operario/ContraAtaque se unificam nesse mecanismo.

**Entrelaçamento:** este fio cruza com "Auditoria das ativas" (abaixo) — é a auditoria das
ativas que destravaria o revide-com-habilidade.

---

## EventoDano — contexto rico das reações (o nó central dos pendentes)

**Status:** FUTURO (depois do Turno-reset, destrava o grosso do C5 restante).
**O que é:** o ContextoReacao atual (Portador, Outro, DanoCausado, Natureza) é magro.
EventoDano = registro rico (dano bruto/efetivo/absorvido, FoiCritico, atacante, alvo,
natureza, aliados). Evolução natural do ContextoReacao. Alimenta também a versão web
(front consome eventos).
**O que destrava:**
- **Robo** (precisa de Aliados no contexto — no mesmo molde do TipoLista.Aliados que o
  Tecnology usa via ObterListaPrincipal).
- **Sushiman** (precisa de FoiCritico).
- O **reset "1x por agressor por turno"** (mora no Turno, serve o C5): Espinhos/Zumbi/Coco
  passam de "por hit" pra "1x por agressor". Igual ContraAtaque (HashSet de já-revidados,
  limpo no AoPassarTurno). O controle de frequência deve ser reusável, não copiado.
**Fila de eventos (Forma 3):** adiada no C6. Nasce com o EventoDano. O revide hoje usa
Forma 1 (recursão profundidade 1, segura pela natureza Revide) e migra pra fila quando o
EventoDano vier.

---

## Conceito de Turno (TurnoDoPersonagem) — PARCIALMENTE FEITO

**Status:** RELÓGIO FEITO. TurnoDoPersonagem extraído do CombateService (ADR em
docs/ADR-conceito-de-turno.md): Iniciar() (tick dos status) e Finalizar() (avança duração
de status + remove expirados + avança cooldowns). O CombateService chama os dois em volta
da Ação.
**FALTA (cruza o C5):**
- O reset "1x por agressor por turno" — mora no Turno, serve as reações do C5 (ver EventoDano).
- O disparo do evento InicioDoTurno das passivas — hoje em DispararEventoInicioDeTurno no
  CombateService, porque depende do sistema de passivas que o C5 migra. Reavaliar quando
  BonecoDeNeve/Genio (IReageAoInicioTurno) forem migrados.
Ambos na branch que mexe em Turno + C5 JUNTOS (o ponto onde os temas se cruzam).

---

## Auditoria das habilidades ATIVAS — NÃO FEITA

**Status:** PENDENTE, avaliar uma vez. NÃO refatorar preventivamente.
**O que é:** as ativas (Tecnology, Marretada, etc) usam um modelo data-driven decente
(ContextoCombate + metadados TipoAlvo/TipoLista/TipoAtaque + Ativar). Aparentemente SEM a
dor que motivou o C5 — a responsabilidade já está razoavelmente dividida (ativa = efeito,
service = orquestração). Mas nunca foram auditadas a fundo.
**O fio com o C5:** o único ponto de contato é TipoAtaque → contagem de DepoisDeAtacar
(AoE 1x / Sequencial por hit). Design correto, sobrevive à migração.
**O fio com o revide:** se as ativas expusessem força (multiplicador) + aceitassem natureza,
o revide-com-habilidade ficaria possível (ver "Fio do revide"). Hoje a força está enterrada
no Ativar de cada uma.
**Decisão:** avaliar UMA vez se há dívida (duplicação entre ativas, responsabilidade trocada,
força não-exposta). Se não houver dor real, encerrar como "sem ação". Não fazer simetria com
as passivas só por pureza.

---

## RelógioDoCombate — enrage / limite de turnos

**Status:** FUTURO, YAGNI. Lugar reservado, não implementar agora.
**O que é:** conceito VIZINHO do TurnoDoPersonagem, num nível ACIMA (combate/rodada). Conta
os turnos GLOBAIS do combate e dispara eventos em marcos (turno X → enrage / boss mata todos;
limite de turnos → anti-stall). Relógio do COMBATE, não de um personagem — níveis diferentes
(SRP). Mora no nível do ExecutarCombate. Implementa quando uma fase concreta pedir.

---

## BOY SCOUT (quando tocar) / FUTURO ARQUITETURAL

### Capacidades — stat sob demanda e comportamento de turno
**Status:** ADR em docs/ADR-modelo-de-capacidades.md. Migração incremental.
- A) Reação após evento → IReageAo* — buffs FEITOS (PR-C); passivas no C5 (quase fim).
- B) Intervenção no dano → IModificaDanoRecebido ✅ FEITO.
- E) Bloqueio de aplicação → IBloqueiaStatus ✅ FEITO.
- C) Stat sob demanda → IContribuiDefesa / IContribuiAtaque / IContribuiCrit
  (BuffDefesa/ReducaoDefesa já espelhados via ContribuicaoDefesa). PENDENTE (boy scout).
- D) Comportamento de turno (Medo/Preso/Irritar) → PENDENTE, baixa prioridade. Refinamento
  registrado: Medo com grau (fraco = só paralisa; forte = paralisa + cooldown), Preso como
  família futura (congelar/atordoar/petrificar), Irritar fica de fora. NÃO desenhar até os
  efeitos que justificam existirem.

### Helper ColetarReacoes<T> (dívida de repetição)
**Status:** dívida registrada, branch de limpeza futura. O padrão "varre StatusAtivos.OfType<T>
+ ColetarPassivasReativas<T> e junta resultados" se repete em ProcessarReacoesAlvo,
ProcessarReacoesAtacantePorAlvo e PorAtaque. Extrair um helper genérico
ColetarReacoes<T>(combatente, ctx, acc, chamar) reduz a repetição. A separação dos métodos em
si está correta (chamados em lugares diferentes); só o loop interno é repetido.

### Observabilidade — exibir TaxaCrit/DanoCrit na UI de combate
**Status:** dívida registrada, pré-requisito de teste. OlhoClinico/Virus mexem em TaxaCrit/
DanoCrit, que NÃO aparecem na tela — passivas não-testáveis hoje (ponto cego). Exibir esses
stats na UI de combate é pré-requisito pra validar essas passivas de verdade. Conecta com o
test bed (modo Versus controlável).

### Bloquear-revive temporário / "status do estado morto"
**Status:** ideia registrada, YAGNI. Não construir até os efeitos pedirem.
**Hoje:** "impedir revive" é a flag PodeReviver + BloquearRevive() (permanente, irreversível).
Vilao bloqueia; AnjoCaido ignora via exceção pontual. Dois casos — a flag basta.
**A ideia (Gabriel):** se o jogo quiser bloqueio TEMPORÁRIO/removível (estilo Raid: duração,
cleanse, resistência), a flag não basta. Modelar como "status do ESTADO MORTO" — categoria de
status separada da lista do vivo, pra NÃO ser pega pela ImunidadeDebuffs (que protege o vivo;
foi o bug original que tirou MortePermanente de Debuff). Acomodaria contra-revive e
ignorar-bloqueio seletivo. Construir quando os efeitos que a justificam existirem.

### Testes automatizados (xUnit na lógica de domínio)
**Status:** futuro, quando a estrutura estabilizar. A lógica separada da UI torna os testes
fáceis (dano, veneno, reações, caps). Seria diferencial de portfólio. Timing certo: DEPOIS de
o C5/EventoDano estabilizarem (testar estrutura que ainda muda é retrabalho).

### Persistência — arquivo → SQL (futuro web)
**Status:** FUTURO (versão web 2027). Isolar persistência atrás de repositório
(IRepositorioProgresso / IRepositorioItens) — hoje lê arquivo, amanhã SQL. YAGNI até a web,
mas não acoplar mais nesse meio tempo. O save em arquivo é da versão console (morre na web).

### Services-lookup (cosmético, baixa prioridade)
**Status:** observado, sem dor. FaccaoService e CampanhaService são basicamente tabelas.
Candidatos a virar dados em vez de service. Fazer só se incomodar. YAGNI.

### Faxina de comentários
**Status:** ÚLTIMO da fila. Combate/CombateService têm mais comentário que código em alguns
trechos. Bisturi: remove ruído, mantém os porquês. Branch própria, depois de tudo estabilizar.

---

## CONCLUÍDO (referência)

- **Sistema de Natureza do Dano** (NaturezaDano + TipoReacao + perfis). Base de tudo.
- **PR-C — reações via interface** (C1-C6): Sedento, Reflexo, Sangramento, Espinhos,
  ContraAtaque migrados pra IReageAo*. Revide orquestrado (Forma 1, profundidade 1).
- **C7 — limpeza:** removidos os 3 hooks mortos do StatusEffect + EventoCombate.AntesDeReceberDano.
- **C5 (em curso) — 17 passivas migradas:** lado "ao ser atacado" completo (Zumbi, Coco,
  Palhaco, Cientista, Mimico, Ogro, PapaiNoel, TRex, Coroa, Ambicao, Diabo); lado atacante
  (OlhoClinico, Virus via IReageAoAtacar; Sorrateiro, Policial via IReagePorAtaque); ao matar
  (Fada, Vilao via IReageAoMatar). Interfaces IReageAoAtacar/IReagePorAtaque/IReageAoMatar criadas.
- **Capacidade C (IContribuiDefesa):** BuffDefesa/ReducaoDefesa sobre DefesaComStacks; confirmado
  sem inconsistência com stat-builders permanentes (camadas distintas).
- **fix Veneno tick:** dano do tick é 5% fixo (não × Stacks); acúmulo só na Explosão.
- **fix Save defensivo:** trata JSON corrompido com fallback, no limite de I/O.
- **TurnoDoPersonagem (relógio)** extraído (Iniciar/Finalizar). ADR em docs/. Falta reset
  1x-por-agressor + evento de início (cruzam C5).
- **Seleção de Alvo:** regra → SelecaoDeAlvoService (injetável); UI → MenuService; bot →
  EscolherAlvoBot. ADR em docs/.
- **Capacidades B + E:** IModificaDanoRecebido e IBloqueiaStatus. Métodos virtuais mortos
  removidos da base.
- **Stats em Camadas** (Ataque/Defesa/Crit calculados sob demanda).
- **Bloquear-revive promovido a flag** (PodeReviver/BloquearRevive) — saiu de Debuff
  (MortePermanente fingia ser debuff e era pego por ImunidadeDebuffs). Vilao migrado pra
  IReageAoMatar usa BloquearRevive().
- **Limpeza de branches remotas** (GitHub) + auto-delete de head branches ativado.

---

## NÃO FAZER (decisões conscientes de NÃO refatorar)

- **Separar mensagens de combate do MenuService.** Camada de apresentação do console — MORRE
  na web. Não investir.
- **Centralizar descrições das habilidades.** A descrição mora na própria habilidade (coesão
  correta). Extrair pra arquivo de strings é over-engineering. Separar demais é exagero.
- **try-catch no núcleo de combate.** Domínio controlado; exceção lá seria bug mascarado.
- **Refatorar as ativas preventivamente** (por simetria com as passivas). Só se a auditoria
  achar dor real. Elas já têm modelo declarativo decente.
- **Importar a complexidade de revive do Raid** sem ter os efeitos dele. Modelar pro que o
  jogo faz, não pro que outro jogo faz.