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

## OS FIOS QUE FALTAM (visão de alto nível)

Resumo do que resta no combate. **C5 COMPLETO** (todas as passivas migradas, sistema
velho aposentado). O que resta:

1. **EventoDano / contexto rico** — Fatia 1 e Fatia 2 FEITAS. Falta só a passiva-conta-mortos
   (1b) como cliente futuro do contexto rico.
2. **Estado de Vida (Vivo/Morto) + Atos do turno** — ✅ **Passos 1-5 FEITOS** (State Pattern,
   status no Morto, Atos, Guarda limpa, seleção de alvo por estado — PR #111). Falta só a
   passiva-conta-mortos (1b), que é do EventoDano.
3. **Turno (resto)** — reset 1x-por-agressor do CONTRA-ATAQUE ✅ FEITO (#112). Falta o reset das
   OUTRAS reações (Espinhos/Zumbi/Coco) + TimeAtualDoTurno (centralizar aliados/inimigos).
4. **Buff-permanente vs passiva-pura** — ✅ **FEITO** (#111/#112): 6 passivas puras + Fantasma
   (Removivel=false). Ver seção própria (marcada concluída).
5. **Composição de Ações + Motor de Habilidades** — habilidade vira DADO (lista de Ações) rodada
   por um interpretador único; **zero `Ativar` override**. Piloto per-alvo FEITO (#115); **MOTOR
   FEITO (#116)** — loop-flip + Escopo/EstadoAlvo por ação + fragmentos de Valor, verificado em
   jogo. Forma-construtor + champ-como-arquivo (Mago piloto) FEITOS. Agora: **sweep por facção**
   (Nível A fundido — cada facção migra direto pra forma final, uma passada só).
   Ver **ADR-composicao-de-acoes.md** (revisado). Predecessor do Rebalanceamento.
6. **Rebalanceamento** — design de jogo (Sereia A3, Morcego→Vampiro, durações). FASE própria,
   pós-composição.

A **unificação dos mecanismos de ignorar** é um fio transversal já começado (DeveAgir = passo 1).

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
  RemoverDebuffs, MoverBuffs, ConcederTurnoExtra.
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
**sweep segue** (unificar-ignorar → Apóstolos [Copiando/`MoverBuffs`, Céu]) → pick do menu (lado UI, §8.2)
quando o `Ambos` morrer (6 dos 7 feitos: Nigiri, DocesDeAbobora, Tecnology, Circo, Atlantis, AnjoCaído —
falta Céu). Quando uma facção ESTREIA um mecanismo, o champ é momento de
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
IReageAntesDeMorrer (Guarda) → IReageAoMatar (Vilao) → IReageAoMorrer (Necromancia).
A Guarda reverte a morte antes do Vilão enxergá-la; se reverteu, os outros dois não disparam.
Se não reverteu, o Vilão bloqueia o revive antes da Necromância tentar.

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

---

## EventoDano — o registro rico do golpe (✅ FATIA 1 + FATIA 2 FEITAS)

**Status:** ✅ COMPLETO. O EventoDano existe e é produzido pelo combate; o ContextoReacao já
foi enriquecido (FoiCritico, Aliados, Inimigos). Só falta a passiva-conta-mortos (1b) como
cliente futuro do contexto rico (seção própria).

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
  BuffDefesa). A unificação precisa cobrir as duas categorias. NOTA: ignorar IContribuiDefesa
  NÃO é igual a pular (continue) como Escudo/ProtecaoAliado — a defesa do buff já foi SOMADA
  ao stat (Defesa), então pra ignorá-la o ReceberDano SUBTRAI a ContribuicaoDefesa (lógica
  invertida vs o continue dos modificadores). Funciona, mas é confuso ("soma tudo, depois
  desconta o ignorado"). Limpeza prevista: o cálculo de quanto descontar deveria morar na
  própria reação/natureza, não no loop do ReceberDano — montar a defesa SEM os ignorados desde
  o início, em vez de incluir e descontar.
- **ATENÇÃO (avaliado jul/2026):** IContribuiDefesa NÃO é a mesma "mina dual-source" que
  IBloqueiaStatus/IModificaDanoRecebido eram (aquelas ganharam a varredura de
  Personagem.Habilidades). Aqui é diferente: o getter `Defesa` usa os tipos CONCRETOS
  BuffDefesa/ReducaoDefesa, não a interface; a interface só serve ao loop de subtração-do-ignorado
  no ReceberDano, e passivas nem entram na lista `ignorados`. Adicionar varredura de passivas
  aqui seria inútil/errado (subtrairia o que nunca foi somado). Resolver só JUNTO desta unificação,
  não como "consistência de dispatch".

---

## Conceito de Turno (TurnoDoPersonagem) — PARCIALMENTE FEITO

**Status:** RELÓGIO FEITO. TurnoDoPersonagem extraído (ADR em docs/ADR-conceito-de-turno.md):
Iniciar() (tick dos status) e Finalizar() (avança duração + remove expirados + avança cooldowns
+ limpa contra-ataques do turno).

**Reset "1x por agressor por turno" do CONTRA-ATAQUE — ✅ FEITO.** O registro de quem já foi
contra-atacado saiu dos HashSets privados (ContraAtaque tinha o seu, Operário nem tinha) e virou
`Combate._jaContraAtacou` + `Combate.TentarContraAtacar(agressor, chance)` (regra única: chance +
"1x por agressor", registra no sucesso) + `Combate.LimparContraAtaques()` (chamado no
Finalizar). Fonte única — buff ContraAtaque, PassivaHeroi e PassivaOperario passam TODOS por ela,
então o gap multi-fonte (Herói com buff do Dragão + passiva) morreu: o primeiro registra, o
segundo vê que já contra-atacou. O hook `StatusEffect.AoPassarTurno` (virtual usado só pelo
ContraAtaque, o único "capaz virtual sem irmã interface") foi REMOVIDO. Herói virou passiva-pura
(IReageAoSerAtacado, sem buff via IPassivaInicial); Operário ganhou o limite 1x/agressor.

**FALTA (Turno resto):**
- **Reset 1x-por-agressor das OUTRAS reações** — Espinhos/Zumbi/Coco ainda são "por hit"; podem
  passar a "1x por agressor" reusando o mesmo padrão do contra-ataque (registro no Combate,
  limpo no Finalizar). Generalizar `TentarContraAtacar` pra um mecanismo de frequência por-reação
  quando doer (hoje só o contra-ataque pediu).
- **TimeAtualDoTurno** — centralizar a regra "quais são os aliados e os inimigos de uma
  perspectiva" numa fonte única que ContextoCombate E ContextoReacao consultam, em vez de cada
  ponto do CombateService recalcular `atacante is Jogador ? jogador : inimigo` e inverter na
  mão. Refinamento puro (o CombateService já calcula os times). Nome a definir.
- O disparo do evento InicioDoTurno das passivas — hoje em DispararEventoInicioDeTurno no
  CombateService. Reavaliar se migra pro Turno.

**EVOLUÇÃO ARQUITETURAL — TurnoDoPersonagem PERSISTENTE (Caminho B, decisão fechada, futuro):**
Hoje o TurnoDoPersonagem é TRANSIENTE (`new` a cada turno, só orquestra; o estado mora no
Combate). O `_jaContraAtacou` foi posto no Combate por pragmatismo (Caminho A) — mas é
conceitualmente estado DE TURNO (nasce e morre no turno), diferente de duração/cooldown que são
do COMBATENTE (persistem, o turno só avança). Quando o "Turno resto" vier, o TurnoDoPersonagem
vira PERSISTENTE (um por combatente, vive o combate todo) e passa a POSSUIR o estado turn-scoped
(contra-ataques hoje; TimeAtualDoTurno depois) — deixa de ser procedimento e vira o MODELO de
turno. Migração barata: o estado só muda de casa (Combate → Turno), a leitura/escrita das
passivas via `TentarContraAtacar` não muda. Habilita ideias de Gabriel: medidor de turno /
velocidade nos stats. NÃO confundir com o RelógioDoCombate (contador GLOBAL de rodadas, nível
acima — "boss mata todos após X turnos") — são dois relógios em níveis diferentes.

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

## Auditoria das habilidades ATIVAS — NÃO FEITA

**Status:** PENDENTE, avaliar uma vez. NÃO refatorar preventivamente.
As ativas usam um modelo data-driven decente (ContextoCombate + metadados + Ativar).
Aparentemente SEM a dor do C5. (O fio do revide-com-habilidade, que dependia parcialmente
disso, já foi resolvido SEM precisar tocar a base de HabilidadeAtiva — IAtivavelComNatureza
é ISP à parte.) Avaliar UMA vez se há dívida; se não houver dor, encerrar como "sem ação".

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

### Modernização e robustez (auditoria jul/2026 — PR próprio pós-sweep)
**Status:** registrado. Sai como PR próprio quando o sweep de facções acabar (Apóstolos é a última).
Tema próprio — NÃO misturar com sweep/unificação ("um PR, um tema"). Nenhum item é bug; são "formas
melhores hoje do que fizemos antigamente" + a única fronteira de confiança real. Guard-clause em código
interno fica DE FORA de propósito (YAGNI — compilador + testes cobrem o miolo).
1. **`Random.Shared`** — mata as 4 instâncias `new Random()` independentes (`Combate`, `AplicarDebuff`,
   `Medo`, `HabilidadeAtiva`). Idioma .NET 6+.
2. **Encapsular coleções mutáveis expostas** — `List<>` público deixa mutação passar por fora das
   regras (`PodeReceber`/bloqueios): `Combate.StatusAtivos`, `Personagem.Habilidades`,
   `Vivo.StatusNoVivo`/`Morto.StatusNoMorto`. Expor `IReadOnlyList<T>`; mutar só pelos métodos.
   Refatorar "quando doer" — hoje ninguém abusa, mas passa a garantia pro TIPO, não pra disciplina.
3. **Save defensivo (a ÚNICA fronteira de confiança real)** — save é JSON lido do disco, pode vir
   corrompido/editado à mão. `CapitulosService`/`ArsenalService` devem envolver a desserialização em
   try/catch com fallback (save inválido → estado limpo, nunca crash). "Segurança" num single-player
   offline = não morrer com arquivo estragado.
4. **"Nulo morre na porta" no resto do código** — o encanamento dos ignorar já foi limpo no PR da
   unificação; varrer os demais `IEnumerable<T>? = null` que vazam nulo pra dentro e normalizar na
   fronteira (`?? []`), deixando o interior não-nulo.

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
**Status:** ADIADO CONSCIENTE (Gabriel, jul/2026 — nunca usou xUnit, prefere não parar o embalo
agora). Não esquecido: é o maior diferencial de portfólio e a rede de segurança dos refactors
(os bugs sérios — StackOverflow de proteção mútua, crítico-exige-dano, dispatch dual-source —
foram achados por raciocínio, não por teste). **On-ramp pra quando quiser:** o 1º teste verde
leva ~15 min guiado, começando pela ordem crítica de morte (Guarda→Vilão→Necromancia) e pelo
ReceberDano — os pontos onde bug já apareceu.

### Persistência — arquivo → SQL (futuro web)
**Status:** FUTURO (web 2027). Isolar persistência atrás de repositório. YAGNI até a web.

### Services-lookup (cosmético, baixa prioridade)
**Status:** observado, sem dor. FaccaoService/CampanhaService são tabelas. Candidatos a virar
dados. Fazer só se incomodar.

### Faxina de nomes (rename do repo/namespace)
**Status:** ✅ **FEITO** o rename do `v1` — namespace `v1_Apostle_s_War` → `ApostlesWar`,
sln/csproj (`v1-Apostle's-War.*` → `ApostlesWar.*`) junto. **Falta:** corrigir a pasta
`Campaingn` (typo de Campaign) e a mistura PT/EN — boy-scout, sem dor conhecida ainda.
Portfólio: recrutador lê o repo.

### EventoDano por ID (desacoplar dos objetos vivos)
**Status:** registrado, sem data. O `EventoDano` carrega hoje `Combate Atacante`/`Combate Alvo`
(objetos vivos). Pra ser um registro limpo do golpe (log/stream desacoplado), referenciaria por
id/nome. Mudança real; fazemos quando for natural. (Não é "web/2027" — é quando der.)

### IModificaDanoCausado (modificador de dano do atacante)
**Status:** follow-on da Composição de Ações. A ação `Dano` passa a consultar modificadores do
atacante automaticamente (a `PassivaPiromancer` para de ser fiada à mão em cada habilidade de
fogo). Espelho do `IModificaDanoRecebido`. Cruza com `FontesDeCapacidade` (dispatch das duas
fontes: StatusAtivos + Personagem.Habilidades).

### Identidade comum (Nome/Simbolo/Descricao) — resíduo do ADR-sistema-de-efeitos (arquivado)
**Status:** boy-scout, cosmético. A "Separação 1" do `sistema-de-efeitos` (uma base comum de
identidade — Nome/Simbolo/Descricao — herdada por `Habilidade` E `StatusEffect`, hoje duplicada)
nunca foi feita; era "a última, cosmética". O ADR foi pra `docs/historico/`; o resíduo fica aqui
pra não se perder. Fazer só se incomodar (puro DRY, sem dor conhecida).

### Faxina de comentários
**Status:** ÚLTIMO da fila. Bisturi: remove ruído, mantém os porquês. Branch própria, depois de
tudo estabilizar.

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
