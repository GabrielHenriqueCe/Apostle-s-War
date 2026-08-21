// Confere se os DOCUMENTOS ainda descrevem o repositório que existe.
//
// Doc que envelhece MENTINDO é pior que doc ausente: quem lê age em cima. Este arquivo pega a
// metade MECÂNICA disso — caminho que não existe mais, PR citado que nunca foi mergeado, símbolo
// que sumiu do código, número de orçamento que ficou para trás. A outra metade, a semântica
// (o texto que descreve comportamento que o código não tem mais), continua precisando de leitor:
// o cabeçalho do `rodar-telas.js` dizia "não clica em nada" enquanto clicava, e nada aqui pegaria.
//
// Ele NÃO reescreve nada. O que fazer com a deriva é decisão de quem lê — script que edita doc é
// como o `dotnet test` reescrevendo o `bancada-dano.md`.
//
// ============================================================================================
// O QUE DERRUBA E O QUE NÃO. Só as conferências 1, 2 e 4 mexem no código de saída. A 3 (símbolos)
// sai numa seção de LEITURA porque ela não sabe separar "citado como atual" de "citado como
// história", e doc que conta o que morreu está CERTO — o ROADMAP faz isso de propósito. Um
// verificador que grita cinquenta vezes é um verificador que ninguém roda.
// ============================================================================================
//
// Uso:  node ferramentas/conferir-docs.js
'use strict';

const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const RAIZ = path.resolve(__dirname, '..');
process.chdir(RAIZ);

const ALVOS = [
    ...fs.readdirSync('docs').filter(f => f.endsWith('.md')).map(f => 'docs/' + f),
    'CLAUDE.md',
    ...fs.readdirSync('.claude/skills').map(d => `.claude/skills/${d}/SKILL.md`),
];

// Nomes que NÃO são deste repo: BCL, API de navegador, ferramenta, e o que foi dissolvido e o doc
// cita justamente pra dizer que não existe mais. Sem esta lista a conferência 3 acusa a si mesma.
const DE_FORA = new Set([
    'CancellationToken', 'WindowsBase', 'LoadRawString', 'ReadKey', 'InternalsVisibleTo',
    'Grep', 'GHUtils', 'System', 'Task', 'List', 'Dictionary', 'Console',
]);

const problemas = [];       // derrubam
const paraLer = [];         // seção de leitura
const queixar = (m) => { if (!problemas.includes(m)) problemas.push(m); };

// ---------- o índice do repositório ----------
const caminhos = [], fontes = [];
(function varrer(d) {
    for (const e of fs.readdirSync(d, { withFileTypes: true })) {
        if (['bin', 'obj', '.git', 'node_modules', '.vs'].includes(e.name)) continue;
        const p = path.join(d, e.name).split(path.sep).join('/');
        const comBarra = p + (e.isDirectory() ? '/' : '');
        caminhos.push(comBarra);
        // APELIDO: no disco o projeto é `ApostlesWar.Domain/`, e os documentos o chamam de
        // `Domain/`. Sem indexar as duas formas, `Domain/Combat/` viraria "sumiu" — e o remédio
        // seria reescrever os docs pra uma verbosidade que ninguém quer.
        if (comBarra.startsWith('ApostlesWar.')) caminhos.push(comBarra.slice('ApostlesWar.'.length));
        if (e.isDirectory()) varrer(p);
        else if (/\.(cs|js|css|html)$/.test(e.name)) fontes.push(p);
    }
})('.');
const corpus = fontes.map(f => fs.readFileSync(f, 'utf8')).join('\n');

/// Uma linha por vez, sabendo se ela está sob um heading `###` ou mais fundo.
function linhasComProfundidade(texto) {
    let fundo = false;
    return texto.split(/\r?\n/).map((linha) => {
        const h = linha.match(/^(#{1,6})\s/);
        if (h) fundo = h[1].length >= 3;
        return { linha, fundo };
    });
}

const emCrase = (linha) => [...linha.matchAll(/`([^`\n]+)`/g)].map(m => m[1].trim());

// ---------- 1. o caminho citado ainda existe? ----------
// Os docs citam caminho relativo ao PROJETO (`Skills/Acoes/Explodir.cs`), não à raiz do repo —
// por isso casa por SUFIXO contra o índice, e não por `existsSync`.
//
// Só entra o que tem cara de caminho: extensão minúscula conhecida, ou barra no fim. Sem essa
// trava, `nivel/10`, `morto/morreu` e `feat/forja-e-po` viram "arquivo sumido".
const EXT = /\.(cs|md|js|css|html|json|csproj|sln)$/;

// Caminho que o doc cita SEM que ele deva existir no disco, com o motivo de cada um. É a lista de
// vocabulário desta conferência: sem ela o verde vira mentira e ninguém roda o verificador de novo.
// O `###` não serve aqui como serve nos símbolos — a FILA A, que é a parte VIVA do ROADMAP, mora
// sob `###`, e cortá-la silenciaria justamente a deriva que importa.
const CITADO_DE_PROPOSITO = new Map([
    ['MEMORY.md', 'mora em ~/.claude, fora do repo'],
    ['memory/', 'mora em ~/.claude, fora do repo'],
    ['~/.claude/statusline.js', 'a statusline do Claude Code, fora do repo'],
    ['Save/', 'pasta de RUNTIME (onde o save cai), não caminho de código'],
    ['cenarios/arena/', 'caminho PLANEJADO — a pele da Arena ainda não existe'],
    ['View/', 'estrutura da pele de console, removida no #179; o doc conta a história'],
    ['Campaingn/', 'a pasta com typo que SUMIU — citada justamente pra dizer isso'],
    ['NavegacaoTests.cs', 'testava a navegação por teclado; morreu com a pele de console no #179'],
]);

function conferirCaminhos(arquivo, linhas) {
    for (const { linha } of linhas) {
        for (const t of emCrase(linha)) {
            if (t.includes(' ') || t.includes('*') || t.includes('<') || t.startsWith('.')) continue;
            if (!/[A-Za-z]/.test(t)) continue;                     // `///` e `//` não são caminho
            if (!EXT.test(t) && !t.endsWith('/')) continue;
            if (CITADO_DE_PROPOSITO.has(t)) continue;
            const q = '/' + t.replace(/^\.\//, '').replace(/\/$/, '');
            const achou = caminhos.some(p => ('/' + p).endsWith(q) || ('/' + p).endsWith(q + '/'));
            if (!achou) queixar(`${arquivo}: caminho citado não existe — \`${t}\``);
        }
    }
}

// ---------- 2. o #NNN citado foi mergeado? ----------
// `#NNN` é AMBÍGUO neste repo: `#15` é item do ROADMAP, `#254` é PR. Os PRs passaram de 100 há
// muito tempo e os itens do ROADMAP não chegam a 20, então o corte em 100 separa os dois sem
// precisar de lista. Abaixo disso, não dá pra saber e não se acusa.
const PRIMEIRO_PR_SEGURO = 100;

function conferirPRs(arquivo, linhas, mergeados) {
    for (const { linha } of linhas) {
        for (const m of linha.matchAll(/#(\d{1,4})\b/g)) {
            const n = Number(m[1]);
            if (n < PRIMEIRO_PR_SEGURO || mergeados.has(m[1])) continue;
            queixar(`${arquivo}: #${m[1]} citado como PR, mas não há commit \`(#${m[1]})\` no log`);
        }
    }
}

// ---------- 3. o símbolo citado ainda existe? (LEITURA, não derruba) ----------
// Sob `###` ou mais fundo o ROADMAP guarda backlog e histórico — é onde ele conta o que morreu, e
// citar coisa morta ali é o doc funcionando. Fora disso, símbolo sem uma ocorrência sequer no
// código é candidato a nome fantasma: foi assim que o `IReageAntesDeMorrer` apareceu, um nome que
// nunca existiu (o real é `IPrevineMorte`) e que estava em dois documentos.
function conferirSimbolos(arquivo, linhas) {
    for (const { linha, fundo } of linhas) {
        if (fundo) continue;
        for (const t of emCrase(linha)) {
            if (!/^[A-Z][A-Za-z0-9]{2,}$/.test(t) || DE_FORA.has(t)) continue;
            if (corpus.includes(t)) continue;
            if (!paraLer.some(p => p.nome === t && p.doc === arquivo)) paraLer.push({ nome: t, doc: arquivo });
        }
    }
}

// ---------- 4. a tabela de orçamento do CLAUDE.md ainda vale? ----------
// A régua é `chars/4`, que é o que reproduz os números que já estavam na tabela. Não é tokenizador
// de verdade — é ordem de grandeza, e ordem de grandeza é pra que a tabela serve.
const TOLERANCIA = 0.2;
const emK = (arquivo) => fs.readFileSync(arquivo, 'utf8').length / 4000;

function conferirOrcamento() {
    for (const linha of fs.readFileSync('CLAUDE.md', 'utf8').split(/\r?\n/)) {
        const col = linha.split('|').map(c => c.trim());
        if (col.length < 4) continue;
        const arquivos = [...col[1].matchAll(/`([^`]+\.md)`/g)].map(m => m[1]);
        const nums = [...col[2].matchAll(/(\d+(?:[.,]\d+)?)k/g)].map(m => Number(m[1].replace(',', '.')));
        if (!arquivos.length || arquivos.length !== nums.length) continue;
        arquivos.forEach((nome, i) => {
            const alvo = ALVOS.find(a => a.endsWith('/' + nome) || a === nome);
            if (!alvo) return queixar(`CLAUDE.md: a tabela de orçamento cita \`${nome}\`, que não existe`);
            const real = emK(alvo);
            const desvio = Math.abs(real - nums[i]) / nums[i];
            if (desvio > TOLERANCIA) {
                queixar(`CLAUDE.md: orçamento de \`${nome}\` diz ${nums[i]}k e o arquivo está em `
                    + `${real.toFixed(0)}k (${(desvio * 100).toFixed(0)}% fora)`);
            }
        });
    }
}

// ---------- corrida ----------
const mergeados = new Set(
    [...cp.execSync('git log --format=%s', { maxBuffer: 1e8 }).toString().matchAll(/\(#(\d+)\)/g)]
        .map(m => m[1]));

console.log(`\n  ${ALVOS.length} documentos · ${caminhos.length} caminhos e ${fontes.length} fontes no índice`
    + ` · ${mergeados.size} PRs no log`);

for (const arquivo of ALVOS) {
    const linhas = linhasComProfundidade(fs.readFileSync(arquivo, 'utf8'));
    conferirCaminhos(arquivo, linhas);
    conferirPRs(arquivo, linhas, mergeados);
    conferirSimbolos(arquivo, linhas);
}
conferirOrcamento();

// O `git log -S` diz se aquele nome ALGUM DIA foi código. Quem nunca foi é o caso interessante
// (nome proposto e não construído, ou typo); quem foi e morreu é quase sempre o documento contando
// a história, que é o trabalho dele.
//
// Ordena e ENCOLHE — não filtra. Filtrar por isso silenciaria a deriva de verdade: o
// `IReageAntesDeMorrer` do GDD-combate também "foi código e morreu", e o que o tornava erro era o
// TEMPO VERBAL ("o motor JÁ TEM o gancho"), que script nenhum lê.
// Um `git log -S` por nome custa um processo cada, e no Windows isso é ~0,4s — vinte nomes viram
// oito segundos. O histórico INTEIRO das fontes sai numa chamada só (~6 MB, ~1,5s) e responde a
// todos os nomes de uma vez.
let historico = null;
function foiCodigoAlgumDia(nome) {
    if (historico === null) {
        try {
            historico = cp.execSync('git log -p --unified=0 --format="" -- "*.cs" "*.js"',
                { encoding: 'utf8', maxBuffer: 1e9, stdio: ['ignore', 'pipe', 'ignore'] });
        } catch {
            historico = '';   // sem histórico não dá pra afirmar nada; ver o `|| !historico` abaixo
        }
    }
    return !historico || historico.includes(nome);
}

if (paraLer.length) {
    const novos = paraLer.filter(p => !foiCodigoAlgumDia(p.nome));
    const velhos = paraLer.filter(p => foiCodigoAlgumDia(p.nome));

    console.log(`\n  ---- pra CONFERIR à mão (${paraLer.length}) — não derruba ----`);
    if (novos.length) {
        console.log('  NUNCA foram código — nome proposto e não construído, ou typo:');
        for (const p of novos) console.log(`  · ${p.doc}: \`${p.nome}\``);
    }
    if (velhos.length) {
        const nomes = [...new Set(velhos.map(p => p.nome))];
        console.log(`  +${velhos.length} citações de ${nomes.length} nomes que FORAM código e morreram`
            + ' — história provável. Confirmar um com `git log -S <nome>`:');
        console.log('    ' + nomes.join(' · '));
    }
}

if (problemas.length) {
    console.log(`\n  ---- problemas (${problemas.length}) ----`);
    for (const p of problemas) console.log('  ✗ ' + p);
    console.log('\n  RESULTADO: FALHOU\n');
    process.exit(1);
}
console.log('\n  RESULTADO: os documentos batem com o repositório\n');
