// Quem é dono de cada função do jogo.js — a tabela que decide as pastas da separação.
//
// O ROADMAP avisou que isto NÃO dá pra saber olhando: os builders estão planos, ordenados por
// ordem histórica de construção, e `criarFogueira`/`desenharEspada`/`criarMoitas` podem ser de uma
// facção só ou compartilhados. Mover no olho é descobrir três facções depois que o Místicos usava
// aquilo — "o grep mente" em forma de refatoração.
//
// COMO: monta o grafo de chamadas entre as declarações de topo, acha as RAÍZES de cada tema (os
// builders que o `noFundo`/`naFrente` liga quando a config daquele tema tem a chave), e tira o
// fecho transitivo. Usado por 1 tema → pasta do tema. Por 2+ → cenarios/comum/. Por nenhum tema
// mas alcançável pelas telas → é do jogo, não do cenário.
//
// Uso:  node ferramentas/medir-donos.js
'use strict';

const fs = require('fs');
const path = require('path');

const argLivre = process.argv.slice(2).filter(a => !a.startsWith('--'));
const ARQ = (process.argv.includes('--porque') ? null : argLivre[0])
    || path.resolve(__dirname, '../ApostlesWar.Presentation/wwwroot/jogo.js');
const fonte = fs.readFileSync(ARQ, 'utf8');
const linhas = fonte.split(/\r?\n/);

// ---------- as declarações de topo ----------
// Coluna 0 é o que define "topo" — dentro de função tudo vem indentado. Pega `function nome(`,
// `const nome = `, `let nome = ` e `class Nome`.
// O `export ` opcional na frente: a partir da separação as declarações que atravessam módulo o
// carregam, e sem isto o medidor deixa de enxergar justamente o que já foi separado.
const RE_DECL = /^(?:export\s+)?(?:async\s+)?function\s+([A-Za-z_$][\w$]*)|^(?:export\s+)?(?:const|let|var)\s+([A-Za-z_$][\w$]*)\s*=|^(?:export\s+)?class\s+([A-Za-z_$][\w$]*)/;

const decls = [];
linhas.forEach((linha, i) => {
    const m = RE_DECL.exec(linha);
    if (m) decls.push({ nome: m[1] || m[2] || m[3], inicio: i });
});
// O FIM de cada declaração é o fechamento na COLUNA 0 (`}`, `};`, `];`), não o início da próxima.
// Fechar na próxima parecia equivalente e não é: a ÚLTIMA declaração engolia todo o rodapé do
// arquivo — os `mandar('pronto')` e `addEventListener` soltos no fim —, e assim a `desenharRena`
// aparecia chamando a ponte com o C#. Aresta falsa que jogava os Apóstolos dentro das telas.
const RE_FECHA = /^[}\])]+\s*;?\s*,?\s*$/;
for (let i = 0; i < decls.length; i++) {
    const limite = (i + 1 < decls.length ? decls[i + 1].inicio : linhas.length) - 1;
    let fim = limite;
    for (let l = decls[i].inicio; l <= limite; l++) {
        if (l > decls[i].inicio && RE_FECHA.test(linhas[l])) { fim = l; break; }
        // Declaração de uma linha só (`const entre = ([min,max]) => ...;`).
        if (l === decls[i].inicio && /;\s*$/.test(linhas[l]) && !/[{[(]\s*$/.test(linhas[l])) { fim = l; break; }
    }
    decls[i].fim = fim;
    decls[i].linhas = fim - decls[i].inicio + 1;
}

const porNome = new Map(decls.map(d => [d.nome, d]));

// ---------- o grafo de chamadas ----------
// Comentário fora: um nome citado só na prosa não é dependência. Foi por isso que o ROADMAP
// insistiu em MEDIR — a prosa deste arquivo cita builders o tempo todo.
function semComentario(txt) {
    return txt
        .replace(/\/\*[\s\S]*?\*\//g, ' ')
        .split('\n').map(l => l.replace(/(^|[^:])\/\/.*$/, '$1')).join('\n')
        .replace(/`(?:[^`\\]|\\.)*`/g, '``')
        .replace(/'(?:[^'\\]|\\.)*'/g, "''")
        .replace(/"(?:[^"\\]|\\.)*"/g, '""');
}

// SOMBREAMENTO. Um nome declarado DENTRO do corpo não é referência ao homônimo de topo, e ignorar
// isso não é detalhe: o `criarNinja` tem um `const sairDaTela` local (a fuga dele pela lateral) que
// colide com a função de tela `sairDaTela`. Sem esta subtração, o ninja "chamava" a tela, a tela
// chamava o `aplicarTema`, e o grafo inteiro colapsava — TODO builder aparecia como compartilhado
// por todos os temas. A tabela ficava plausível e completamente errada.
const RE_LOCAL = /(?:const|let|var|function)\s+([A-Za-z_$][\w$]*)/g;

// Os PARÂMETROS também sombreiam, e este repo usa isso de propósito: `criarNoHorizonte(cfg, canvas,
// desenhar)` recebe a função de desenho por parâmetro, e `desenhar` é também o nome da função que
// redesenha a TELA. Sem subtrair parâmetro, as corujas "chamavam" a interface inteira.
function parametrosDe(corpo) {
    const abre = corpo.indexOf('(');
    if (abre === -1) return [];
    let nivel = 0, fecha = -1;
    for (let i = abre; i < corpo.length; i++) {
        if (corpo[i] === '(') nivel++;
        else if (corpo[i] === ')') { nivel--; if (nivel === 0) { fecha = i; break; } }
    }
    if (fecha === -1) return [];
    return [...corpo.slice(abre + 1, fecha).matchAll(/([A-Za-z_$][\w$]*)\s*(?=[,)=\]}]|$)/g)].map(m => m[1]);
}

const sombreados = [];
for (const d of decls) {
    const corpo = semComentario(linhas.slice(d.inicio, d.fim + 1).join('\n'));
    const locais = new Set(parametrosDe(corpo));
    // A partir da 2ª linha: a 1ª É a declaração deste próprio nome.
    const miolo = corpo.slice(corpo.indexOf('\n') + 1);
    for (const m of miolo.matchAll(RE_LOCAL)) locais.add(m[1]);

    d.usa = new Set();
    for (const m of corpo.matchAll(/\b([A-Za-z_$][\w$]*)\b/g)) {
        if (m[1] === d.nome || !porNome.has(m[1])) continue;
        if (locais.has(m[1])) { sombreados.push(`${d.nome} → ${m[1]} (local/param)`); continue; }

        const antes = corpo.slice(0, m.index);
        // ACESSO POR PONTO: `algo.desenhar` é membro de outro objeto, não a função de topo.
        if (/\.\s*$/.test(antes)) { sombreados.push(`${d.nome} → ${m[1]} (.membro)`); continue; }

        // CHAVE DE OBJETO: `{ desenhar: ... }` nomeia um campo, não chama nada. O `criarChifres`
        // entrega `{ acabou, desenhar }` ao `criarAparicaoNaMoita` — e `desenhar` também é o nome da
        // função que redesenha a tela. Sem esta exclusão, os chifres "chamavam" a interface.
        // O guarda do `?` é pra não confundir com o dois-pontos de um ternário.
        const depois = corpo.slice(m.index + m[1].length);
        const linhaAntes = antes.slice(antes.lastIndexOf('\n') + 1);
        const ternarioNaLinha = /\?(?!\.)/.test(linhaAntes);   // `?.` é opcional-chaining, não ternário
        if (/^\s*:/.test(depois) && /(?:[{,]|^)\s*$/.test(antes) && !ternarioNaLinha) {
            sombreados.push(`${d.nome} → ${m[1]} (chave de objeto)`);
            continue;
        }

        d.usa.add(m[1]);
    }
}

// O DESPACHANTE não é aresta. `aplicarTema`/`iniciarAr` conhecem TODOS os builders por construção —
// é o trabalho deles. Atravessá-los faria qualquer caminho chegar em qualquer lugar, que é a mesma
// forma de colapso do sombreamento, só que legítima. Eles são a FRONTEIRA da medição.
const DESPACHANTES = new Set(['aplicarTema', 'iniciarAr', 'AR_DO_TEMA']);

// ---------- as raízes de cada tema ----------
// O `noFundo`/`naFrente` é uma lista de `config.CHAVE && criarAlgo(...)`. Cada linha amarra uma
// chave de config a um builder: se o tema declara a chave, aquele builder entra em cena.
const iniciarAr = porNome.get('iniciarAr');
if (!iniciarAr) { console.error('iniciarAr não encontrado'); process.exit(1); }
const corpoAr = semComentario(linhas.slice(iniciarAr.inicio, iniciarAr.fim + 1).join('\n'));

const gatilhos = [];   // { chaves: [...], builder }
for (const m of corpoAr.matchAll(/((?:config\.[\w$]+\s*&&\s*)+)([A-Za-z_$][\w$]*)\s*\(/g)) {
    const chaves = [...m[1].matchAll(/config\.([\w$]+)/g)].map(x => x[1]);
    if (porNome.has(m[2])) gatilhos.push({ chaves, builder: m[2] });
}

// O AR_DO_TEMA: quais chaves cada tema declara. Lido do próprio arquivo, no nível de indentação
// dos temas (4 espaços) e das chaves de config (8).
const arDoTema = porNome.get('AR_DO_TEMA');
const temas = {};
{
    let atual = null;
    for (let i = arDoTema.inicio; i <= arDoTema.fim; i++) {
        const l = linhas[i];
        const t = /^ {4}([\w$]+):\s*\{/.exec(l);
        if (t) { atual = t[1]; temas[atual] = new Set(); continue; }
        if (/^ {4}\},?\s*$/.test(l)) { atual = null; continue; }
        const c = /^ {8}([\w$]+):/.exec(l);
        if (c && atual) temas[atual].add(c[1]);
    }
}

// ---------- fecho transitivo ----------
const alcance = (raizes) => {
    const vistos = new Set();
    const fila = [...raizes];
    while (fila.length) {
        const n = fila.pop();
        if (vistos.has(n) || !porNome.has(n)) continue;
        vistos.add(n);
        if (DESPACHANTES.has(n)) continue;   // entra na conta, mas não se atravessa
        for (const v of porNome.get(n).usa) fila.push(v);
    }
    return vistos;
};

const raizesDoTema = (tema) =>
    gatilhos.filter(g => g.chaves.every(c => temas[tema].has(c))).map(g => g.builder);

const usoPorTema = {};
for (const tema of Object.keys(temas)) usoPorTema[tema] = alcance(raizesDoTema(tema));

// MODO RASTREIO: `node ferramentas/medir-donos.js --porque <tema> <funcao>` mostra POR ONDE aquele
// tema chega naquela função. Existe porque a primeira tabela saiu plausível e errada, e ler o
// CAMINHO foi o que revelou a aresta falsa (um `const sairDaTela` local dentro do criarNinja).
const iPorque = process.argv.indexOf('--porque');
if (iPorque !== -1) {
    const [tema, alvo] = process.argv.slice(iPorque + 1);
    const raizes = raizesDoTema(tema);
    if (!raizes.length) { console.log(`tema '${tema}' não tem raízes — nomes válidos: ${Object.keys(temas).join(' ')}`); process.exit(1); }
    const pai = new Map(raizes.map(r => [r, null]));
    const fila = [...raizes];
    while (fila.length) {
        const n = fila.shift();
        if (DESPACHANTES.has(n)) continue;
        for (const v of porNome.get(n)?.usa ?? []) if (!pai.has(v)) { pai.set(v, n); fila.push(v); }
    }
    if (!pai.has(alvo)) { console.log(`\n  ${tema} NÃO alcança ${alvo}\n`); process.exit(0); }
    const caminho = []; for (let x = alvo; x; x = pai.get(x)) caminho.unshift(x);
    console.log(`\n  ${tema} → ${alvo}:\n    ${caminho.join('\n    → ')}\n`);
    process.exit(0);
}

// O que as TELAS alcançam (tudo que não é cenário): raízes = o que sobra de topo e não é builder de
// tema nem o próprio AR_DO_TEMA/iniciarAr.
const doCenario = new Set(['iniciarAr', 'AR_DO_TEMA', 'aplicarTema', ...gatilhos.map(g => g.builder)]);
const raizesDeTela = decls.map(d => d.nome).filter(n => !doCenario.has(n));
const alcanceTela = alcance(raizesDeTela);

// ---------- a tabela ----------
const nomesTemas = Object.keys(temas);
const linhasTabela = [];
for (const d of decls) {
    const donos = nomesTemas.filter(t => usoPorTema[t].has(d.nome));
    let destino;
    // Os despachantes são a FRONTEIRA da medição, não órfãos: o `iniciarAr` conhece todo builder por
    // ofício (vai pro nucleo/) e o `AR_DO_TEMA` se PARTE, uma entrada por pasta de tema.
    if (DESPACHANTES.has(d.nome)) destino = d.nome === 'AR_DO_TEMA' ? 'FRONTEIRA (parte por tema)' : 'FRONTEIRA (nucleo/ar.js)';
    else if (donos.length === 1) destino = `cenarios/${donos[0]}/`;
    else if (donos.length > 1) destino = 'cenarios/comum/';
    else destino = alcanceTela.has(d.nome) ? 'jogo (telas/nucleo)' : '?? ORFAO';
    linhasTabela.push({ nome: d.nome, linhas: d.linhas, donos, destino });
}

const porDestino = {};
for (const l of linhasTabela) (porDestino[l.destino] ??= []).push(l);

const soma = (arr) => arr.reduce((a, b) => a + b.linhas, 0);

console.log(`\n  ${path.basename(ARQ)} — ${decls.length} declarações de topo, ${linhas.length} linhas\n`);
console.log('  DESTINO                       decls   linhas');
console.log('  ' + '-'.repeat(46));
for (const [dest, itens] of Object.entries(porDestino).sort((a, b) => soma(b[1]) - soma(a[1]))) {
    console.log(`  ${dest.padEnd(28)} ${String(itens.length).padStart(5)}   ${String(soma(itens)).padStart(6)}`);
}

console.log('\n  ---- COMPARTILHADOS (2+ temas) → cenarios/comum/ ----');
for (const l of (porDestino['cenarios/comum/'] ?? []).sort((a, b) => b.linhas - a.linhas)) {
    console.log(`  ${l.nome.padEnd(28)} ${String(l.linhas).padStart(5)}  ${l.donos.join(' ')}`);
}

console.log('\n  ---- EXCLUSIVOS, por tema ----');
for (const t of nomesTemas) {
    const meus = linhasTabela.filter(l => l.destino === `cenarios/${t}/`).sort((a, b) => b.linhas - a.linhas);
    console.log(`\n  ${t}  (${meus.length} decls, ${soma(meus)} linhas)`);
    for (const l of meus) console.log(`     ${l.nome.padEnd(30)} ${String(l.linhas).padStart(5)}`);
}

if (sombreados.length) {
    console.log('\n  ---- SOMBREAMENTOS ignorados (nome local com o mesmo nome de uma função de topo) ----');
    console.log('  Cada um destes seria uma aresta FALSA. O `criarNinja → sairDaTela` sozinho colapsava o grafo.');
    for (const s of [...new Set(sombreados)]) console.log(`  ${s}`);
}

const orfaos = porDestino['?? ORFAO'] ?? [];
if (orfaos.length) {
    console.log('\n  ---- ÓRFÃOS (ninguém alcança — conferir à mão) ----');
    for (const l of orfaos) console.log(`  ${l.nome.padEnd(28)} ${String(l.linhas).padStart(5)}`);
}

fs.writeFileSync(path.resolve(__dirname, 'donos.json'), JSON.stringify({ linhasTabela, gatilhos, temas: Object.fromEntries(Object.entries(temas).map(([k, v]) => [k, [...v]])) }, null, 2));
console.log(`\n  tabela completa em ferramentas/donos.json\n`);
