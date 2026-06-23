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
- **Destravar de forma encadeada.** Escolher a ordem onde cada peça destrava a
  próxima e minimiza retrabalho. Pode-se reabrir algo, mas o caminho limpo é melhor
  que andar em círculos.
- **Exception nos LIMITES (I/O, parsing externo), nunca no NÚCLEO** (lógica de
  domínio — se lança lá, é bug, deve estourar pra corrigir).
- **Build verde (Ctrl+Shift+B) antes de todo push.** CI manual do dev solo.
- **Não desenhar no escuro.** Interface/categoria nasce servindo efeitos reais que
  existem, não casos hipotéticos. YAGNI até o efeito que justifica aparecer.
  EXCEÇÃO consciente: o EventoDano foi investido cedo na fundação de exibição/web
  (Propósito B) — decisão explícita de aceitar mais trabalho por uma base sólida.

---

## OS 5 FIOS QUE FALTAM (visão de alto nível)

Resumo do que resta no combate, na ordem encadeada que destrava o máximo:

1. **EventoDano / contexto rico** — o nó central. NASCEU (record do golpe pronto,
   Fatia 1). Falta a Fatia 2 (enriquecer o ContextoReacao) pra destravar Robo,
   Sushiman e a passiva-conta-mortos.
2. **Turno (resto)** — reset 1x-por-agressor + evento de início. Cruza o C5. Junto
   do TimeAtualDoTurno (centralizar a regra de aliados/inimigos).
3. **Refactor das ativas → revide-com-habilidade** — destrava Operario certo +
   contra-ataque de múltiplas fontes (buff/passiva/item).
4. **Estado morto (1a)** — dívida de modelagem ATUAL (não YAGNI): bloquear-revive já
   tem clientes reais. Refatorar no rebalanceamento.
5. **Terminar o C5 → aposentar o sistema velho** — consequência de 1, 2, 3.

Dois eixos quase independentes: o do **contexto** (1 → 2) e o do **revide** (3). A
**unificação dos mecanismos de ignorar** (abaixo) é um fio transversal que já começou
(DeveAgir = passo 1).

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
  passiva) pra limpar. Ver "Fio do revide" — a migração esbarra na decisão de como o
  revide carrega a habilidade. Decisão pro AGORA: Opção C provisória.
- **Necromancia + Guarda** — IReageAoMorrer (A CRIAR). Revive (capacidade vs efetivo),
  cooldown real (6 e 4), interação com o bloqueio do Vilao (já migrado), mensagens
  sobreviveu/morreu. Guarda é um HACK de revive — intenção real é "impedir morte" via
  Invencivel (Categoria B). NÃO depende do EventoDano — pode ser feito "solta".
- **BonecoDeNeve + Genio** — IReageAoInicioTurno (A CRIAR). Cruza com o
  DispararEventoInicioDeTurno (evento de início que ficou no CombateService) — fio do Turno.
- **Robo + Sushiman** — BLOQUEADOS, esperam a Fatia 2 do EventoDano. Robo precisa de
  Aliados (curar o de menor HP). Sushiman precisa de Aliados (reflexo a todos) + FoiCritico.
  O Combate NÃO conhece o próprio time — vem via contexto montado pelo CombateService.

### Interfaces de reação — estado
- IReageAoSerAtacado, IReageAoReceberDano, IReageAoCausarDano — existem (PR-C).
- IReageAoAtacar (1x por ataque, segue TipoAtaque), IReagePorAtaque (Nx por alvo),
  IReageAoMatar — CRIADAS.
- IReageAoMorrer, IReageAoInicioTurno — A CRIAR.

### Dois sabores do lado atacante (decisão firmada)
- **IReageAoAtacar** = efeito no PRÓPRIO atacante. Segue TipoAtaque: AoE = 1x, Sequencial
  = por hit. Lado a lado com ProcessarPassivasAtacante. [OlhoClinico, Virus]
- **IReagePorAtaque** = efeito POR ALVO atingido. Nx sempre. Dentro do foreach. [Sorrateiro,
  Policial]
ProcessarReacoesAtacante dividido em PorAlvo (dentro do foreach) e PorAtaque (segue
TipoAtaque). Ver "Dívidas" — a repetição do loop vira helper ColetarReacoes<T>.

### Ordem crítica preservada (morte/revive)
"Ao matar" (IReageAoMatar) roda ANTES de "ao morrer" (Necromancia/Guarda). O Vilao bloqueia
o revive antes da tentativa. Dispatch novo chamado no mesmo ponto que o velho, preservando a
ordem.

### Aposentar o sistema velho
DeveAtivar/Ativar virtual e o enum EventoCombate só saem quando a ÚLTIMA passiva migrar.

---

## EventoDano — o registro rico do golpe (NASCEU — Fatia 1 FEITA)

**Status:** FATIA 1 CONCLUÍDA. O EventoDano existe e é produzido pelo combate. Falta a
Fatia 2 (enriquecer o ContextoReacao das reações).

**O que é:** o tipo canônico que descreve um golpe — o "Model do golpe" (conceito MVC) que
a apresentação consome (console hoje, React/web amanhã via JSON). Convergiu o antigo
ResultadoAtaque.

**Propósito B (decisão de Gabriel):** investir cedo na fundação de exibição/web, não só na
lógica. Na versão web, o EventoDano é a linguagem entre back (.NET API, calcula) e front
(React, desenha) — o front consome uma stream de eventos e os transforma em animação. Mesmo
princípio das reações (declaram, o orquestrador exibe), levado ao combate inteiro. Aceito
refatorar depois.

### Fatia 1 — FEITA (record + produção)
- **EventoDano** (record em Combate.cs): Atacante, Alvo, DanoBruto, DanoEfetivo,
  AbsorvidoPeloEscudo, Critico, HPRestante, Natureza. Renomeou ResultadoAtaque; campo Dano
  virou DanoEfetivo. Rename propagou pra habilidades, ResultadoReacao.Dano e exibição.
- **ReceberDano** retorna (Efetivo, AbsorvidoPeloEscudo) em vez de int. Captura o
  escudo-absorvido medindo antes/depois de cada modificador (sem tocar a classe Escudo).
- **Atacar** monta o EventoDano com bruto + efetivo/absorvido + crítico.
- **RefletirDano** ajustado (desestrutura a tupla, monta EventoDano).
- Descartado conscientemente: "aparado pela defesa" (nunca tem cliente). DanoBruto e
  AbsorvidoPeloEscudo entram pela exibição/web (Propósito B), sem efeito que REAJA a eles hoje.

### Fatia 2 — PRÓXIMA (enriquecer o ContextoReacao)
O ContextoReacao hoje é magro (Portador, Outro, DanoCausado, Natureza). A Fatia 2 leva pras
reações a visão de times que o ContextoCombate já dá pras habilidades:
- ContextoReacao ganha: **FoiCritico** (de r.Critico), **Aliados**, **Inimigos** (do Portador).
- Os 4 métodos de reação (ProcessarReacoesAlvo, PorAlvo, PorAtaque, Morte) recebem os times
  (que o CombateService já calcula nos call sites) e montam o contexto rico. Aliados/Inimigos =
  do PORTADOR (inverte no lado do alvo, como ProcessarPassivasAlvo já faz; não inverte no lado
  atacante).
- **Migra Robo** (lê ctx.Aliados, cura o de menor HP) e **Sushiman** (lê ctx.Aliados +
  ctx.FoiCritico, reflexo a todos). Destrava também a **passiva-conta-mortos** (ver seção).
ContextoReacao alvo: (Portador, Outro, DanoCausado, Natureza, FoiCritico, Aliados, Inimigos).

### Os 3 contextos — não confundir (esclarecido)
- **ContextoCombate** (Atacante, Aliados, Inimigos) — das HABILIDADES. JÁ EXISTE.
- **ContextoReacao** — das REAÇÕES. Magro hoje; a Fatia 2 enriquece.
- **EventoDano** — descreve o GOLPE (não é contexto de quem reage). Feito.

---

## Unificar os 3 mecanismos de ignorar status (FIO NOVO — passo 1 FEITO)

**Status:** EM CURSO. O DeveAgir foi o passo 1. Faltam os passos 2+.

**A descoberta:** "ignorar um status" no cálculo de dano tem TRÊS caminhos sem padrão único:
1. **Por natureza** — flags no NaturezaDano (IgnoraEscudo, IgnoraBloqueio, IgnoraDefesa).
   Ex: QueimaDano ignora defesa+escudo; Veneno ignora só defesa.
2. **Por lista na chamada** — parâmetro ignorarStatus: Type[]. Ex: PoMagico (todos os buffs),
   CorteDeVento (Escudo), Vendaval (ProtecaoAliado + BuffDefesa). Atravessa
   IModificaDanoRecebido E IContribuiDefesa.
3. **Por passiva permanente** — IIgnoraStatusNoAtaque. Ex: Vampiro (Invencivel + BloqueioTotal).
   Consultada no Combate.Atacar via ComporListaIgnorar.

Os mecanismos 2 e 3 convergem na lista `ignorados`; o 1 era um `if natureza.IgnoraX` no
ReceberDano. A inconsistência: a MESMA coisa (ignorar Escudo) pode ser expressa por natureza
(Queima) OU por lista (CorteDeVento), sem critério. Foi a limpeza que revelou — bug latente,
não criado agora.

**Meta:** UM caminho canônico de ignorar. Cada status responde "devo agir neste golpe?".

### Passo 1 — FEITO: DeveAgir(natureza) no IModificaDanoRecebido
- A interface ganhou `bool DeveAgir(NaturezaDano natureza)`. Cada status decide:
  - Escudo: `!natureza.IgnoraEscudo`
  - BloqueioTotal: `!natureza.IgnoraBloqueio`
  - Invencivel, ReducaoDanoFixo: `true`
  - ProtecaoAliado: `natureza.Reacao != TipoReacao.Nenhuma`
- ReceberDano perdeu os guardas de natureza por-tipo; consulta DeveAgir. Mantém o
  `ignorados.Contains` (lista — genérico).
- **De quebra, corrigiu o StackOverflow de proteção mútua** (A protege B, B protege A): o
  redirecionamento usa DanoIndireto (Reacao = Nenhuma) → DeveAgir do segundo ProtecaoAliado
  retorna false → não re-redireciona → loop morto. Bug PRÉ-EXISTENTE, exposto ao testar 2
  aliados com proteção.

### Passos futuros (2+)
- **Lista (mecanismo 2) entra no DeveAgir** — o status pergunta "fui listado pra ignorar?" em
  vez do ReceberDano fazer `ignorados.Contains`. Some o critério separado.
- **IIgnoraStatusNoAtaque (mecanismo 3) entra no DeveAgir** — provavelmente o
  IIgnoraStatusNoAtaque some ou se transforma; o Vampiro contribui pro DeveAgir de outra forma.
- Cruza com IContribuiDefesa (o ignorarStatus também afeta a defesa — Vendaval ignora
  BuffDefesa). A unificação precisa cobrir as duas categorias.

---

## TimeAtualDoTurno — fonte única de aliados/inimigos (FIO NOVO)

**Status:** REGISTRADO, fazer junto do "resto do Turno". NÃO é pré-requisito da Fatia 2.

**A ideia (Gabriel):** centralizar a regra "quais são os aliados e os inimigos de uma
perspectiva" numa fonte única, que ContextoCombate E ContextoReacao consultam, em vez de cada
ponto do CombateService recalcular `atacante is Jogador ? jogador : inimigo` e inverter na mão.

**Por que não é pré-requisito:** o CombateService JÁ calcula os times a cada turno; a Fatia 2
só encaminha o que ele já tem. O TimeAtualDoTurno é refinamento — quando vier, o ContextoReacao
LÊ da fonte em vez de receber por parâmetro (mudança pequena, não retrabalho). Fazê-lo agora
abriria terceira frente no meio da Fatia 2.

**Por que junto do Turno:** "de quem é a vez" e "quais os times dessa vez" são parentes.
Agrupar com o resto do Turno (reset 1x-por-agressor + evento de início). Nome a definir
(TimeAtualDoTurno / RelaçãoDeTimes / PerspectivaDeCombate).

---

## Conceito de Turno (TurnoDoPersonagem) — PARCIALMENTE FEITO

**Status:** RELÓGIO FEITO. TurnoDoPersonagem extraído (ADR em docs/ADR-conceito-de-turno.md):
Iniciar() (tick dos status) e Finalizar() (avança duração + remove expirados + avança cooldowns).
**FALTA (cruza o C5):**
- O reset "1x por agressor por turno" — mora no Turno, serve as reações do C5 (Espinhos/Zumbi/
  Coco passam de "por hit" pra "1x por agressor", igual ContraAtaque: HashSet de já-revidados,
  limpo no AoPassarTurno). Controle de frequência reusável, não copiado.
- O disparo do evento InicioDoTurno das passivas — hoje em DispararEventoInicioDeTurno no
  CombateService. Reavaliar quando BonecoDeNeve/Genio (IReageAoInicioTurno) forem migrados.
- **TimeAtualDoTurno** (ver acima) — centralizar a regra de times. Vai junto.
Tudo na branch que mexe em Turno + C5 JUNTOS.

---

## Fio do revide — revide carrega a HABILIDADE (decisão de design pendente)

**Status:** decisão tomada na direção, EXECUÇÃO depende do refactor das ativas.

**O problema:** o revide hoje (RevidarAlvo no ResultadoReacao) carrega só "quem revidar" e o
executor faz Atacar(alvo, 1.0) — multiplicador 1.0 hardcoded. O ContraAtaque usa isso. Mas o
Operario quer revidar com Marretada (1.25x).

**A decisão (Gabriel):** o revide deve carregar a HABILIDADE, não um multiplicador. Cada
habilidade tem identidade (animação, nome, símbolo). O AtaqueBasico já é habilidade — "revidar
com a1" e "revidar com Marretada" são o MESMO conceito.

**Cenário rico (Gabriel):** o contra-ataque pode vir de MÚLTIPLAS fontes — passiva fixa
("sempre contra-ataca com Marretada"), buff temporário, item com chance — cada uma revidando
com habilidade diferente, podendo coexistir. Confirma que o revide tem que carregar a habilidade.

**O obstáculo:** a força da habilidade está ENTERRADA no Ativar de cada ativa (Marretada faz
AplicarDano(..., 1.25) lá dentro). "Revide carrega a habilidade" DEPENDE do refactor das ativas:
expor a força + aceitar natureza.

**Decisão pro Operario AGORA:** Opção C provisória — executa Atacar(ctx.Outro, 1.25, natureza:
Revide) e DECLARA via ResultadoReacao (Mensagem + Dano), em vez de Console.WriteLine. Corrige o
furo de camada sem mexer no RevidarAlvo. Depois unifica com o revide-com-habilidade.

**Entrelaçamento:** cruza com "Auditoria das ativas".

---

## Auditoria das habilidades ATIVAS — NÃO FEITA

**Status:** PENDENTE, avaliar uma vez. NÃO refatorar preventivamente.
As ativas usam um modelo data-driven decente (ContextoCombate + metadados + Ativar).
Aparentemente SEM a dor do C5. O fio com o revide: se expusessem força + aceitassem natureza,
o revide-com-habilidade ficaria possível. Avaliar UMA vez se há dívida; se não houver dor,
encerrar como "sem ação".

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

## Estado morto — DÍVIDA DE MODELAGEM ATUAL (não YAGNI puro)

**Status:** RECLASSIFICADO. Tem clientes reais HOJE — refatorar no rebalanceamento.

**Distinção 1a vs 1b:**
- **1a) Estado morto** = status que agem sobre o morto / regras de revive. Dívida atual.
- **1b) Passiva-conta-mortos** = passiva do VIVO que conta mortos pra ganhar força. NÃO é
  estado morto — consulta o tabuleiro. Depende do contexto rico (Fatia 2). Ver seção própria.

**Por que 1a não é YAGNI (correção):** "impedir revive" JÁ tem clientes — Vilao bloqueia (flag
PodeReviver), Diabo/AnjoCaido ignora (exceção pontual). Anjo talvez ignore no futuro (céu/
inferno). Todo revive checa PodeReviver; só o Diabo ignora. A forma atual (flag + exceção)
funciona pros 2 casos, mas a regra "quem revive / bloqueia / ignora o bloqueio" está espalhada,
sem dono.

**Gatilho concreto:** quando adicionar o Anjo-que-ignora OU mexer no rebalanceamento de revives,
modelar o "estado morto" como categoria própria (dono claro), no MESMO trabalho. Boy scout com
gatilho, não YAGNI.

**A parte genuinamente YAGNI (separada):** bloquear-revive TEMPORÁRIO/removível estilo Raid
(duração, cleanse, resistência) — categoria de status separada da lista do vivo, pra NÃO ser
pega pela ImunidadeDebuffs. ESSA só quando os efeitos pedirem.

---

## Passiva-conta-mortos (1b) — depende do contexto rico (FIO NOVO)

**Status:** REGISTRADO, junto de Robo/Sushiman (Fatia 2 do EventoDano).
**O que é:** passiva do vivo que fica mais forte por quantos inimigos/aliados estão mortos (ex:
+10% ATK por inimigo morto). Efeitos de item pra isso no futuro. Precisa contar mortos nos times
→ precisa de Aliados + Inimigos no ContextoReacao (mesma dependência do Robo/Sushiman). Por isso
a Fatia 2 destrava TRÊS coisas.

---

## RelógioDoCombate — enrage / limite de turnos

**Status:** FUTURO, YAGNI. Conceito VIZINHO do TurnoDoPersonagem, num nível ACIMA (combate/
rodada). Conta turnos GLOBAIS e dispara eventos em marcos (enrage; anti-stall). Mora no nível do
ExecutarCombate. Implementa quando uma fase concreta pedir.

---

## BOY SCOUT (quando tocar) / FUTURO ARQUITETURAL

### Capacidades — stat sob demanda e comportamento de turno
**Status:** ADR em docs/ADR-modelo-de-capacidades.md. Migração incremental.
- A) Reação após evento → IReageAo* — buffs FEITOS; passivas no C5 (quase fim).
- B) Intervenção no dano → IModificaDanoRecebido FEITO (+ DeveAgir agora).
- E) Bloqueio de aplicação → IBloqueiaStatus FEITO.
- C) Stat sob demanda → IContribui* (BuffDefesa/ReducaoDefesa já espelhados). PENDENTE.
- D) Comportamento de turno (Medo/Preso/Irritar) → PENDENTE, baixa prioridade. Medo com grau,
  Preso como família futura, Irritar fica de fora. NÃO desenhar até os efeitos pedirem.

### Helper ColetarReacoes<T> (dívida de repetição)
**Status:** dívida registrada. O padrão "varre StatusAtivos.OfType<T> + ColetarPassivasReativas<T>"
se repete em ProcessarReacoesAlvo, PorAlvo e PorAtaque. Extrair helper genérico. A separação dos
métodos está correta; só o loop interno é repetido.

### Observabilidade — exibir TaxaCrit/DanoCrit na UI de combate
**Status:** dívida, pré-requisito de teste. OlhoClinico/Virus mexem em TaxaCrit/DanoCrit, que NÃO
aparecem na tela — não-testáveis hoje. Conecta com o test bed (modo Versus).

### Testes automatizados (xUnit na lógica de domínio)
**Status:** futuro, quando a estrutura estabilizar. Diferencial de portfólio. Timing: DEPOIS de o
C5/EventoDano estabilizarem.

### Persistência — arquivo → SQL (futuro web)
**Status:** FUTURO (web 2027). Isolar persistência atrás de repositório. YAGNI até a web.

### Services-lookup (cosmético, baixa prioridade)
**Status:** observado, sem dor. FaccaoService/CampanhaService são tabelas. Candidatos a virar
dados. Fazer só se incomodar.

### Faxina de comentários
**Status:** ÚLTIMO da fila. Bisturi: remove ruído, mantém os porquês. Branch própria, depois de
tudo estabilizar.

---

## CONCLUÍDO (referência)

- **Sistema de Natureza do Dano** (NaturezaDano + TipoReacao + perfis). Base de tudo.
- **ContextoCombate** (Atacante/Aliados/Inimigos) — habilidades recebem o contexto rico.
- **PR-C — reações via interface** (C1-C6): Sedento, Reflexo, Sangramento, Espinhos, ContraAtaque
  migrados pra IReageAo*. Revide orquestrado (Forma 1, profundidade 1).
- **C7 — limpeza:** removidos os 3 hooks mortos do StatusEffect + EventoCombate.AntesDeReceberDano.
- **C5 (em curso) — 17 passivas migradas:** lado "ao ser atacado" completo; lado atacante
  (OlhoClinico, Virus; Sorrateiro, Policial); ao matar (Fada, Vilao). Interfaces
  IReageAoAtacar/IReagePorAtaque/IReageAoMatar criadas.
- **EventoDano (Fatia 1):** ResultadoAtaque convergiu em EventoDano (record rico do golpe).
  ReceberDano retorna (Efetivo, AbsorvidoPeloEscudo). Atacar monta o evento. Base da exibição
  rica/web (Propósito B).
- **DeveAgir (IModificaDanoRecebido):** cada status decide se age via DeveAgir(natureza). Removeu
  os guardas de natureza por-tipo do ReceberDano. Corrigiu o StackOverflow de proteção mútua.
  Passo 1 da unificação dos mecanismos de ignorar.
- **Capacidade C (IContribuiDefesa):** BuffDefesa/ReducaoDefesa sobre DefesaComStacks; sem
  inconsistência (camadas distintas).
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

- **Separar mensagens de combate do MenuService.** Apresentação do console — MORRE na web.
- **Centralizar descrições das habilidades.** A descrição mora na habilidade (coesão correta).
- **try-catch no núcleo de combate.** Domínio controlado; exceção lá seria bug mascarado.
- **Refatorar as ativas preventivamente.** Só se a auditoria achar dor real.
- **Importar a complexidade de revive do Raid** sem ter os efeitos. (Mas o estado-morto ATUAL —
  Vilao/Diabo — é dívida real, ver seção própria.)
- **Criar TimeAtualDoTurno agora, no meio da Fatia 2.** É refinamento, vai junto do Turno.