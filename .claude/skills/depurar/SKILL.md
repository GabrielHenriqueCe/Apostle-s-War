---
name: depurar
description: Ler ANTES de caçar a causa de um defeito no Apostle's War — "não funciona", "quebrou", "está errado", tela em branco, número que não bate, teste vermelho herdado, comportamento que some quando se olha. Também vale quando o trabalho é escrever a ferramenta ou o teste que prende um defeito.
---

# Depurar aqui

Quatro fases, e a ordem é o método. Pular a 2 pra ir direto na 4 é o erro que este projeto já pagou
várias vezes — a correção "óbvia" conserta um sintoma vizinho e o defeito volta de outra forma.

## 1. REPRODUZIR — pôr o defeito num lugar que dispara à vontade

Um defeito que só aparece jogando não é depurável: é anedota. Antes de qualquer coisa, achar o
gatilho que roda em 1 segundo.

- **Combate RODA headless** — o molde é `Tests/OrdemDeMorteTests.cs`: tela no-op sobre
  `ITelaDeCombate` + controlador que fecha o horizonte em N golpes. Determinismo sem semear `Random`:
  dano muito acima do HP do alvo.
- **Front:** `node --experimental-vm-modules ferramentas/rodar-telas.js` monta as 13 telas e dispara
  os gestos. **Rodar os três harnesses ANTES de mexer** — é o que evita herdar vermelho de outra
  sessão como se fosse seu (o `rodar-telas` já esteve vermelho na `main` sem ninguém ver).
- **O que NÃO reproduz aqui:** o PIXEL. Layout, cor, animação e "ficou feio" são do Gabriel, em jogo.
  Não abrir a janela sem avisar — e o jogo ABERTO trava o build.

## 2. INSTRUMENTAR — medir o caminho, nunca adivinhar

**A hipótese bonita perde pra uma sonda feia.** Antes de propor causa, imprimir o estado no ponto
exato: o que entrou, o que saiu, quem é `null`.

- **Ler o CAMINHO, não o resultado.** Verde pode ser verde por não ter rodado. Já aconteceu com a
  montagem da Arena (passou sem estar ligada) e com o `querySelectorAll` devolvendo lista vazia — o
  laço nunca rodou e ninguém viu.
- **O grep mente.** Verificar antes de fundir: o nome pode estar em comentário, em doc morto, ou ser
  outro símbolo com a mesma grafia.
- Sonda é temporária. Sai antes do commit — o que sobra do aprendizado vai pra mensagem de commit.

## 3. PRENDER — a rede que falha AGORA

Corrigir sem rede é trocar um defeito por uma data de validade. **A ferramenta vem antes de mover a
primeira linha**, e ela só vale depois de PROVAR que pega.

- **Prova por SABOTAGEM:** quebrar de propósito o que a rede deveria pegar e ver a rede acusar — e
  acusar **pelo motivo certo**. Uma rede que fica vermelha pelo motivo errado é vermelha por acaso.
- **RESTAURAR a sabotagem por substituição INVERSA**, nunca `git checkout --`: o checkout restaura
  pro HEAD e apaga junto todo o trabalho não commitado do working tree. Depois, `git diff` do arquivo
  sabotado tem de vir VAZIO.
- **Rodar a FIAÇÃO, não só as camadas.** Bancada que monta o objeto na mão não vê a exceção que só
  acontece no caminho real de montagem — a cena ficou em branco e as camadas estavam todas verdes.

## 4. CORRIGIR e VARRER

- Consertar a CAUSA. Se a correção precisa de comentário defendendo por que é assim, o comentário é
  curto e diz **o que quebra se mudar** — a história vai pra mensagem de commit (`CLAUDE.md`
  §Comentário).
- **Armadilha ATIVA nunca se corta**: o que quebra em silêncio se alguém "melhorar" o código.
- **Varrer no MESMO PR.** Renomeou, moveu, apagou ou concluiu? `grep` o nome VELHO em `docs/`,
  `CLAUDE.md`, `README.md`, `.claude/skills/`, `ferramentas/` e a memória. Quem descreve no PRESENTE
  vira erro; quem descreve no PASSADO é história e FICA.
- Fechar com os três: `dotnet build` · `dotnet test` · os três harnesses (`CLAUDE.md` §Comandos).

## As armadilhas que já morderam, e não se deduzem do código

| sintoma | causa |
|---|---|
| tela monta e o gesto não existe | chave errada na carga do harness — desliga o caminho **em silêncio** |
| `else if` grudado num comentário | arquivo com CRLF e LF misturados; arquivo novo de ferramenta nasce LF |
| 5.600 linhas somem e o `node --check` passa | splice em arquivo CRLF — quem denuncia é `git diff --numstat` |
| build falha sem motivo | o jogo está ABERTO travando o `.exe`/`.dll` |
| doc "certa" apontando pro lugar errado | deriva de rename/move — `node ferramentas/conferir-docs.js` |
| trocar a ordem não quebra o build | é a ordem crítica de morte: muda quem VIVE (`MANUAL-combate.md`) |

**`sed -i` reescreve CRLF→LF em TODO arquivo que toca** — preferir `node -e` com
`replace(/\r?\n/g,'\r\n')` no fim. E nada de crase dentro de `node -e "..."` no bash.
