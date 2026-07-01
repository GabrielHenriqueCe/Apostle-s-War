# ADR — Seleção de Alvo por Estado (Vivo/Morto)

> **Tipo:** Architecture Decision Record
> **Status:** Aceito (decisões fechadas; implementação a seguir)
> **Contexto:** Apostle's War. Passo 5 do ADR-estado-de-vida-e-atos.md — a última
>   destrava do fio de Estado de Vida. Consolida junto o fio "buff-permanente
>   vs passiva-pura" (mesmo PR, decisão de Gabriel).
> **Data:** julho/2026

---

## 1. Problema

Hoje `EstaVivo()` é checado de forma **inconsistente e duplicada** em 3 formas
diferentes, sem nenhuma delas ser uma decisão declarada pela habilidade:

1. **`HabilidadeAtiva.ResolverAlvos`** hardcoda `lista.Where(c => c.EstaVivo())`
   — usado por ataques (`AtaqueBasico`, `Marretada`, `Barata`) e por buffs-de-time
   com escolha explícita (`Lealdade`, `ParedeDeTijolos`). Sempre foi Vivos porque
   nunca existiu outro caso — não é bug, mas está hardcoded no lugar errado
   (deveria ser uma decisão da habilidade, não do método utilitário).
2. **Filtro manual duplicado**: `SalvandoDia` e `Celestial` reimplementam a
   MESMA coisa que `ResolverAlvos` já faz (`.Where(c => c.EstaVivo())` direto em
   `ObterListaPrincipal`), porque não passam por `ResolverAlvos` — código
   repetido sem motivo.
3. **Família revive** (`Ceu`, `Circo`, `Atlantis`, `AnjoCaido`, `DocesDeAbobora`):
   todas fazem "reviver morto(s) E buffar/curar vivos" na MESMA habilidade —
   duas ações independentes sobre dois estados diferentes, cada uma com seu
   próprio loop manual.

**Descoberta adicional (não estava no ROADMAP original):** `CombateService.
ExecutarHabilidade` só pede escolha explícita de alvo (`SelecaoDeAlvoService` +
`MenuService.EscolherAlvoNaTela`) quando `TipoLista.Inimigos`. Para
`TipoLista.Aliados`/`Self`, o código sempre faz `alvoInicial = atacante` —
**nunca existiu escolha real de alvo aliado**. Isso nunca foi um bug porque as
20 habilidades que miram Aliados são todas `NumeroDeAlvos => int.MaxValue`
(atingem o time inteiro, o parâmetro `alvo` é decorativo). Mas `DocesDeAbobora`
já mostra a dor: `lista.FirstOrDefault(a => !a.EstaVivo())` não é uma escolha,
é ordem de lista — se uma habilidade futura precisar reviver só 1 ou 2 aliados
com intenção real, falta o mecanismo de escolher QUEM.

**Boa notícia:** o mecanismo de "1 escolhido + N extras" já existe em
`ResolverAlvos` (usado pelas ativas com dano) — escolha explícita +
adjacentes (`TipoAlvo.Explicito`) ou aleatório (`TipoAlvo.Aleatorio`). Não
precisa inventar nada novo, só generalizar pra aceitar Mortos e ligar o
mesmo mecanismo pra Aliados.

---

## 2. Decisões

### 2.1 `EstadoAlvo` — nova propriedade abstrata em `HabilidadeAtiva`

```csharp
enum EstadoAlvo { Vivos, Mortos, Ambos }
```

Toda `HabilidadeAtiva` declara `EstadoAlvo EstadoAlvo { get; }` — **abstract,
sem default**, igual `NumeroDeAlvos`/`TipoAlvo`/`TipoLista` já são. Força toda
habilidade (existente e futura) a fazer a escolha conscientemente — "make
illegal states unrepresentable" (uma cura não consegue nem mirar um morto
porque nem compila sem declarar).

- **Vivos**: maioria (ataques, curas, buffs). `ResolverAlvos` filtra Vivos.
- **Mortos**: revive puro de N-fixo (não existe hoje, mas a Abóbora
  rebalanceada pode virar isso). `ResolverAlvos` filtra Mortos.
- **Ambos**: a habilidade faz mais de uma coisa (família revive) — **opta por
  fora do pick automático** (ver 2.3). Não é "pureza quebrada", é uma categoria
  legítima (confirmado por Gabriel: "não tem problema as habilidades fazerem
  mais de uma coisa").

### 2.2 `ResolverAlvos` generalizado (sem mudar assinatura)

`ResolverAlvos` é método de instância de `HabilidadeAtiva` — já tem acesso a
`EstadoAlvo` da própria habilidade. Troca o hardcode:

```csharp
// Antes: var vivos = lista.Where(c => c.EstaVivo()).ToList();
var candidatos = EstadoAlvo switch
{
    EstadoAlvo.Vivos  => lista.Where(c => c.EstaVivo()).ToList(),
    EstadoAlvo.Mortos => lista.Where(c => !c.EstaVivo()).ToList(),
    _                 => lista.Where(c => c.EstaVivo()).ToList(), // Ambos não usa este caminho
};
```

Resto do método (explícito+adjacente / aleatório) não muda. `SalvandoDia` e
`Celestial` passam a usar `ResolverAlvos` em vez do loop manual duplicado —
apagam código, não adicionam.

### 2.3 `SelecaoDeAlvoService` — overload novo, sem regras de combate

O overload existente (`ResolverAlvosDisponiveis(candidatos)`) fica **intocado**
— só serve pra Inimigos, com Provocar/BloqueioTotal/Intocável (regras que só
fazem sentido mirando o time adversário). Overload novo pra Aliados, sem essas
regras:

```csharp
public List<Combate> ResolverAlvosDisponiveis(List<Combate> candidatos, EstadoAlvo estado) =>
    estado switch
    {
        EstadoAlvo.Vivos  => candidatos.Where(c => c.EstaVivo()).ToList(),
        EstadoAlvo.Mortos => candidatos.Where(c => !c.EstaVivo()).ToList(),
        _                 => candidatos,
    };
```

### 2.4 `CombateService.ExecutarHabilidade` — regra de setup

```csharp
Combate alvoInicial;
if (hab.TipoLista == TipoLista.Inimigos)
{
    var disponiveis = _selecaoDeAlvoService.ResolverAlvosDisponiveis(defensores);
    alvoInicial = atacante is Jogador
        ? _menuService.EscolherAlvoNaTela(disponiveis, aliados, defensores)
        : _selecaoDeAlvoService.EscolherAlvoBot(disponiveis);
}
else if (hab.TipoLista == TipoLista.Aliados && hab.NumeroDeAlvos != int.MaxValue && hab.EstadoAlvo != EstadoAlvo.Ambos)
{
    var disponiveis = _selecaoDeAlvoService.ResolverAlvosDisponiveis(aliados, hab.EstadoAlvo);
    alvoInicial = atacante is Jogador
        ? _menuService.EscolherAlvoNaTela(disponiveis, aliados, defensores)
        : _selecaoDeAlvoService.EscolherAlvoBot(disponiveis);
}
else alvoInicial = atacante; // hit-all (NumeroDeAlvos=MaxValue) ou Ambos — a própria habilidade resolve
```

`MenuService.EscolherAlvoNaTela` não muda — já é agnóstico a aliado/inimigo
(só desenha uma lista de `Combate`).

### 2.5 Caso especial: Barata NÃO é cliente do `EstadoAlvo.Ambos`

`Barata` mira Vivos (ataque normal) e, como CONSEQUÊNCIA do golpe (matou →
virou morto), aplica `ImpedirRessurreicao` no morto resultante. Isso é uma
seleção (Vivos) + efeito colateral no `Ativar`, não duas seleções — declara
`EstadoAlvo.Vivos` normalmente. `Ambos` é só para habilidades que fazem
**duas seleções independentes de listas diferentes** na mesma chamada
(Diabo/AnjoCaido: revive mortos E cura vivos, dois loops, dois efeitos).

---

## 3. Buff-permanente → passiva-pura (mesmo PR, decisão de Gabriel)

Já mapeado no ROADMAP ("Buff-permanente vs passiva-pura"), sem redesenho —
só referência de execução, mesma branch:

| Personagem | Buff de contorno hoje | Vira capacidade direta |
|---|---|---|
| Abóbora | ImunidadeDebuffs | `IBloqueiaStatus` |
| Dragão | ImunidadeEspecifica(Veneno, Queima) | interface de bloqueio específico |
| Herói | ContraAtaque | `IReageAoSerAtacado` (já usa o Revide novo) |
| Morcego | Sedento 15% | `IReageAoCausarDano` |
| Anjo | CuraContinua 5%/turno | `IReageAoInicioTurno` |
| Sereia | ReducaoDanoFixo 15% | `IModificaDanoRecebido` |
| Fantasma | Intocavel (removível, não deveria ser) | flag `Removivel = false` no StatusEffect (fica buff, só protegido) |

Cada um implementa a interface da própria capacidade em vez de aplicar um
buff em si mesmo no `IniciarCombate` — remove o `IPassivaInicial` desses 6
(Fantasma mantém, só ganha a flag de não-removível).

---

## 4. Validação

- Cada uma das 74 ativas compila só depois de declarar `EstadoAlvo` (garante
  cobertura total, nenhuma esquecida).
- Jogar: Abóbora (ou equivalente rebalanceado) revive com pick real quando
  `NumeroDeAlvos` finito; família revive continua funcionando igual (Ambos,
  sem pick automático, comportamento inalterado).
- `SalvandoDia`/`Celestial` continuam curando só vivos (agora via
  `ResolverAlvos`, não loop manual).
- Os 6 personagens de passiva-pura mantêm o MESMO efeito de jogo, só mudam a
  implementação interna (sem buff de contorno).
