# ADR — Seleção de Alvo (separar regra de targeting da UI e do bot)

> **Tipo:** Architecture Decision Record
> **Status:** Aceito (decisões fechadas; implementação proposta abaixo)
> **Contexto:** Apostle's War. A seleção de alvo no CombateService mistura três
>   responsabilidades de naturezas diferentes. Este ADR separa a regra de
>   domínio (que persiste para a web) da UI (que morre) e do bot.
> **Data:** junho/2026

---

## 1. Problema

No CombateService.ExecutarHabilidade, a seleção de alvo são três pedaços
misturados no mesmo arquivo:

- `ResolverListaDeAlvosDisponiveis` — REGRA de domínio: quem pode ser alvo
  (Provocar > sem Bloqueio/Intocável > sem Bloqueio > todos). Zero UI. SOBREVIVE
  à migração web (a API vai precisar da mesma regra).
- `EscolherAlvoDaLista` — UI: Console.Clear, ExibirPartida, ReadKey, cursor. É a
  tela de escolha do jogador. MORRE na web (vira clique no front).
- `EscolherAlvoAleatorio` — BOT: sorteia um alvo. Estratégia de IA do inimigo.

Misturar domínio + apresentação + IA no mesmo service é o cheiro. A regra de
targeting (lógica que persiste) está enterrada perto de Console.ReadKey.

Detalhe menor observado: EscolherAlvoDaLista refiltra `.Where(EstaVivo())` sobre
uma lista que a regra já filtrou — responsabilidade difusa sobre quem garante
"vivos".

---

## 2. Decisões (já fechadas na auditoria)

- **A REGRA vira uma classe própria INJETÁVEL: `SelecaoDeAlvoService`.** Injetável
  (não estática) porque o bot inteligente futuro (AW v2) deve querer estratégias
  de alvo variáveis — o ponto de injeção permite trocar/estender sem reescrever
  o CombateService.
- **A UI (EscolherAlvoDaLista) vai para o MenuService.** É tela; pertence à
  camada de apresentação. Tira do CombateService.
- **O BOT (EscolherAlvoAleatorio) fica simples agora**, marcado como ponto de
  evolução para IA futura (AW v2: "bot inteligente"). Não construir a IA agora
  (YAGNI), só deixar o lugar claro.

---

## 3. Desenho

### 3.1 SelecaoDeAlvoService (novo — Services/)
Dono da REGRA de targeting (domínio puro, sem UI nem input):

```csharp
using ApostlesWar;
using ApostlesWar.Skills.Buffs;

namespace ApostlesWar.Services
{
    /// <summary>
    /// Regra de domínio de targeting: dada uma lista de candidatos, decide quais
    /// são alvos válidos, conforme os status de provocação/bloqueio/intocável.
    /// Domínio puro — sem UI, sem input. Persiste para a versão web (a API usará
    /// a mesma regra). Injetável para permitir estratégias de alvo variáveis no
    /// futuro (ex: bot inteligente do AW v2).
    /// </summary>
    internal class SelecaoDeAlvoService
    {
        /// <summary>
        /// Filtra os candidatos vivos pela regra de prioridade de alvo:
        /// 1. Se há alguém com Provocar, só esses são alváveis.
        /// 2. Senão, quem não tem BloqueioTotal nem Intocável.
        /// 3. Senão, quem não tem BloqueioTotal.
        /// 4. Senão, todos os vivos (último recurso).
        /// </summary>
        public List<Combate> ResolverAlvosDisponiveis(List<Combate> candidatos)
        {
            var vivos = candidatos.Where(c => c.EstaVivo()).ToList();

            var comProvocar = vivos.Where(c => c.StatusAtivos.Any(s => s is Provocar)).ToList();
            if (comProvocar.Count > 0) return comProvocar;

            var semTudo = vivos.Where(c =>
                !c.StatusAtivos.Any(s => s is BloqueioTotal) &&
                !c.StatusAtivos.Any(s => s is Intocavel)).ToList();
            if (semTudo.Count > 0) return semTudo;

            var semBloqueio = vivos.Where(c => !c.StatusAtivos.Any(s => s is BloqueioTotal)).ToList();
            if (semBloqueio.Count > 0) return semBloqueio;

            return vivos;
        }

        /// <summary>
        /// Escolha do bot: alvo aleatório entre os disponíveis. Ponto de evolução
        /// para IA mais inteligente no futuro (AW v2) — focar mais fraco, mais
        /// perigoso, etc. Por ora, aleatório uniforme.
        /// </summary>
        public Combate EscolherAlvoBot(List<Combate> disponiveis)
        {
            var vivos = disponiveis.Where(d => d.EstaVivo()).ToList();
            return vivos[Random.Shared.Next(vivos.Count)];
        }
    }
}
```

Nota: a regra (parametro) deixou de receber `atacante` — ela não o usava (só
filtrava candidatos). Removido o parâmetro morto. (Quando o bot inteligente
precisar do atacante para decidir, entra como parâmetro de EscolherAlvoBot, não
da regra de disponibilidade.)

### 3.2 MenuService — recebe a UI de escolha
Mover EscolherAlvoDaLista para o MenuService como, ex., `EscolherAlvoNaTela`.
É tela (já tem ExibirPartida lá). O CombateService passa a lista já filtrada
pela regra; o Menu só desenha e lê a escolha.

```csharp
// No MenuService:
public Combate EscolherAlvoNaTela(List<Combate> alvosDisponiveis, List<Combate> aliados, List<Combate> defensores)
{
    int idx = 1;
    while (true)
    {
        Console.Clear();
        ExibirPartida(aliados, defensores);
        Console.WriteLine("\nAlvos:");
        for (int i = 0; i < alvosDisponiveis.Count; i++)
        {
            string cursor = idx == i + 1 ? "▶" : " ";
            Console.WriteLine($"{cursor} {i + 1} - {alvosDisponiveis[i].Personagem.Simbolo} {alvosDisponiveis[i].Personagem.Nome} | HP:{alvosDisponiveis[i].HPAtual} ATK:{alvosDisponiveis[i].Ataque} DEF:{alvosDisponiveis[i].Defesa}");
        }

        ConsoleKeyInfo key = Console.ReadKey(true);
        if (key.Key == ConsoleKey.Enter) return alvosDisponiveis[idx - 1];

        idx = ConsoleUtils.SelecionarComCursor(idx, 1, alvosDisponiveis.Count, key.Key);
    }
}
```

Some o refiltro redundante de vivos (a regra já garante). O atacante deixou de
ser parâmetro (não era usado na UI).

### 3.3 CombateService — fica só a orquestração
ExecutarHabilidade passa a:
```csharp
var alvosDisponiveis = _selecaoDeAlvoService.ResolverAlvosDisponiveis(defensores);
alvoInicial = atacante is Jogador
    ? _menuService.EscolherAlvoNaTela(alvosDisponiveis, aliados, defensores)
    : _selecaoDeAlvoService.EscolherAlvoBot(alvosDisponiveis);
```
Remove ResolverListaDeAlvosDisponiveis, EscolherAlvoDaLista e EscolherAlvoAleatorio
do CombateService. Adiciona o campo/injeção do SelecaoDeAlvoService.

### 3.4 Program.cs — montagem (composition root)
```csharp
var selecaoDeAlvoService = new SelecaoDeAlvoService();
// ... e passar ao construtor do CombateService.
```

---

## 4. Onde cada coisa fica (resumo)

| Pedaço                       | Natureza        | Destino               | Sobrevive à web? |
|------------------------------|-----------------|-----------------------|------------------|
| ResolverListaDeAlvosDisponiveis | Regra de domínio | SelecaoDeAlvoService | Sim              |
| EscolherAlvoDaLista          | UI              | MenuService           | Não (vira front) |
| EscolherAlvoAleatorio        | Bot (IA)        | SelecaoDeAlvoService.EscolherAlvoBot | Sim (evolui) |

---

## 5. Fronteiras e futuro

- A regra NÃO conhece UI nem input. O Menu NÃO conhece a regra (recebe a lista
  pronta). O CombateService orquestra: pede a lista à regra, manda o Menu (jogador)
  ou o bot escolher.
- **Bot inteligente (AW v2):** EscolherAlvoBot é o ponto de evolução. Futuro:
  estratégias (focar menor HP, maior ameaça, etc), possivelmente via uma
  interface IEstrategiaDeAlvo injetada no SelecaoDeAlvoService. YAGNI até lá —
  aleatório uniforme por enquanto.
- A regra de targeting é a mesma que a API web consumirá; isolá-la agora paga na
  migração.

---

## 6. Validação

Refactor de realocação (lógica idêntica). Jogar e confirmar:
- Provocar força o alvo (inimigo provocado é o único alvável).
- BloqueioTotal/Intocável removem o alvo da lista (até esgotar o fallback).
- Escolha do jogador (tela) funciona igual.
- Bot ataca alvo válido aleatório.
