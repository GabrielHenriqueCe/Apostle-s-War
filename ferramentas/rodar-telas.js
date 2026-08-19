// Harness das TELAS. O irmão do rodar-tema.js, e existe pelo mesmo motivo: separar arquivo sem
// verificador é mover no escuro.
//
// O que ele faz: carrega o front inteiro num navegador de mentira e DIRIGE o front pelo caminho de
// verdade — publica no `chrome.webview` a mesma mensagem que o C# publicaria, uma por tela, e vê se
// alguma explode. Depois confere que todo `getElementById` citado no JS existe no index.html.
//
// Por que isso pega o que importa numa separação de arquivos:
//   · import faltando em ES module é erro de CARGA — o boot sozinho já derruba.
//   · função que ficou pra trás vira ReferenceError na hora em que a tela é montada.
//   · id trocado no meio do movimento vira tela em branco sem erro; daí a conferência de ids.
//
// ============================================================================================
// O QUE ELE **NÃO** COBRE — leia antes de confiar num verde daqui.
//
// Ele publica as 13 mensagens do C# e vê o que elas montam. **Ele não clica em nada.** Todo
// caminho que só existe a partir de um gesto do jogador está fora:
//
//   duplo-clique, clique em slot/carta/botão · arrastar-e-soltar · teclado (Esc, Enter)
//   e tudo que roda DURANTE a batalha em resposta a isso
//
// Na separação do front, QUATRO bugs saíram exatamente daí — e os quatro foram achados pelo
// Gabriel jogando, nenhum por este arquivo:
//   · o duplo-clique da conquista não abria a ficha (função virou tela e ninguém avisou o chamador)
//   · o clique num slot do arsenal não mostrava item (const esquecida no arquivo antigo)
//   · o 🎲 da campanha não sorteava (helper compartilhado foi morar numa tela só)
//   · a batalha morria no 1º quadro (referência morta dentro do `atualizarBotaoSair`)
//
// Verde aqui quer dizer "as telas MONTAM". Não quer dizer "o jogo FUNCIONA" — isso continua
// sendo teste em jogo, e é do Gabriel.
// ============================================================================================
//
// Uso:  node --experimental-vm-modules ferramentas/rodar-telas.js
'use strict';

const fs = require('fs');
const path = require('path');
const vm = require('vm');

const WWW = path.resolve(__dirname, '../ApostlesWar.Presentation/wwwroot');
const ENTRADA = path.join(WWW, 'jogo.js');
const INDEX = path.join(WWW, 'index.html');

const problemas = [];
const queixar = (m) => { if (!problemas.includes(m)) problemas.push(m); };

// ---------- DOM de mentira ----------
// Só o suficiente pra montar tela: o que interessa aqui não é o pixel, é a chamada não explodir.
const LARGURA = 1500, ALTURA = 940;
const idsPedidos = new Set();
// Conta TOQUES no DOM durante um despacho (um getElementById basta). Existe porque o harness estava lendo "nao explodiu"
// como "montou": uma mensagem que nao esta NEM na tabela NEM na escada nao faz nada, nao lanca, e
// era reportada como OK. Foi assim que a montagem da Arena passou verde sem estar ligada.
let escritas = 0;

function criarElemento(id = '', tag = 'DIV') {
    const filhos = [];
    const el = {
        id, tagName: tag, hidden: false, textContent: '', value: '', className: '', title: '',
        width: LARGURA, height: 700, clientWidth: LARGURA, clientHeight: 700,
        offsetWidth: LARGURA, offsetHeight: 700, scrollTop: 0, scrollHeight: 0, scrollLeft: 0,
        readOnly: false, disabled: false, checked: false, dataset: {},
        style: new Proxy({ setProperty() {}, removeProperty() {}, getPropertyValue: () => '' },
            { set: (t, k, v) => (t[k] = v, true), get: (t, k) => (k in t ? t[k] : '') }),
        classList: {
            _s: new Set(),
            add(...c) { c.forEach(x => this._s.add(x)); }, remove(...c) { c.forEach(x => this._s.delete(x)); },
            toggle(c, f) { const v = f ?? !this._s.has(c); v ? this._s.add(c) : this._s.delete(c); return v; },
            contains(c) { return this._s.has(c); },
        },
        // Os ouvintes ficam GUARDADOS (antes eram descartados) pra o harness poder disparar um
        // clique — ver `clicar`. Continua não havendo varredura automática: quem clica é a tela
        // que pediu, uma por uma.
        _ev: {},
        addEventListener(tipo, fn) { (el._ev[tipo] ||= []).push(fn); },
        removeEventListener() {}, focus() {}, blur() {}, select() {},
        click() { for (const fn of el._ev.click || []) fn({ preventDefault() {}, stopPropagation() {} }); },
        setAttribute(k, v) { if (k.startsWith('data-')) el.dataset[k.slice(5)] = v; },
        removeAttribute(k) { if (k === 'data-tema') delete el.dataset.tema; },
        getAttribute() { return null; }, hasAttribute() { return false; },
        appendChild(c) { filhos.push(c); return c; },
        append(...c) { filhos.push(...c); }, prepend(...c) { filhos.unshift(...c); },
        replaceChildren(...c) { filhos.length = 0; filhos.push(...c); },
        insertBefore(c) { filhos.push(c); return c; }, remove() {},
        // Devolve elemento em vez de null: o alvo e o CAMINHO de montagem rodar inteiro, e null
        // faria a tela parar na primeira consulta em vez de mostrar o que quebra mais adiante.
        querySelector: (sel) => criarElemento(sel), querySelectorAll: () => [], closest: () => null,
        getBoundingClientRect: () => ({ left: 0, top: 0, right: LARGURA, bottom: ALTURA, width: LARGURA, height: ALTURA, x: 0, y: 0 }),
        getContext: () => ctxDeMentira(),
        scrollTo() {}, scrollIntoView() {},
        get children() { return filhos; },
        get firstChild() { return filhos[0] ?? null; },
        get lastChild() { return filhos[filhos.length - 1] ?? null; },
        get parentElement() { return null; },
    };
    return el;
}

const ctxDeMentira = () => new Proxy({
    save() {}, restore() {},
    createLinearGradient: () => ({ addColorStop() {} }),
    createRadialGradient: () => ({ addColorStop() {} }),
    createPattern: () => ({}), measureText: () => ({ width: 10 }),
    setLineDash() {}, getLineDash: () => [],
}, { get: (t, p) => (p in t ? t[p] : () => {}), set: (t, p, v) => (t[p] = v, true) });

const porId = new Map();
const document = {
    body: criarElemento('body', 'BODY'),
    documentElement: criarElemento('html', 'HTML'),
    getElementById(id) {
        escritas++;
        idsPedidos.add(id);
        if (!porId.has(id)) porId.set(id, criarElemento(id));
        return porId.get(id);
    },
    createElement: (tag) => criarElemento('', String(tag).toUpperCase()),
    createElementNS: (ns, tag) => criarElemento('', String(tag).toUpperCase()),
    createDocumentFragment: () => criarElemento(),
    querySelector: (sel) => criarElemento(sel), querySelectorAll: () => [],
    addEventListener() {}, removeEventListener() {},
};

// ---------- a ponte ----------
// Guardamos o ouvinte que o front registra: é por ele que as telas entram.
let ouvinte = null;
const enviados = [];

const janela = {
    innerWidth: LARGURA, innerHeight: ALTURA, devicePixelRatio: 1,
    requestAnimationFrame() { return 1; }, cancelAnimationFrame() {},
    performance: { now: () => 0 },
    getComputedStyle: () => ({ getPropertyValue: () => '' }),
    addEventListener() {}, removeEventListener() {},
    setTimeout: () => 0, clearTimeout() {}, setInterval: () => 0, clearInterval() {},
    chrome: { webview: {
        postMessage: (m) => enviados.push(m),
        addEventListener: (tipo, fn) => { if (tipo === 'message') ouvinte = fn; },
        removeEventListener() {},
    } },
    console,
};
janela.window = janela; janela.document = document; janela.self = janela;

// ---------- carregar ----------
const contexto = vm.createContext(janela);

// DUAS FASES, e a razão é o grafo em DIAMANTE: `jogo.js` importa `nucleo/cena.js` e `nucleo/ar.js`,
// e o `cena.js` também importa o `ar.js`. Ligando recursivamente, o segundo pedido pelo `ar.js`
// devolvia um módulo ainda no meio da própria ligação — "can not be resolved on module that is not
// linked". Criar TODOS primeiro e ligar a raiz uma vez só resolve, e é o que o `import` real faz.
const cache = new Map();

function criar(arquivo) {
    const abs = path.resolve(arquivo);
    if (cache.has(abs)) return cache.get(abs);
    const fonte = fs.readFileSync(abs, 'utf8');
    const m = new vm.SourceTextModule(fonte, { identifier: abs, context: contexto });
    cache.set(abs, m);
    for (const imp of fonte.matchAll(/^\s*import\s[^'"]*['"]([^'"]+)['"]/gm)) {
        criar(path.resolve(path.dirname(abs), imp[1]));
    }
    return m;
}

async function carregar(arquivo) {
    const raiz = criar(arquivo);
    await raiz.link((spec, ref) => cache.get(path.resolve(path.dirname(ref.identifier), spec)));
    return raiz;
}

// ---------- as telas, uma mensagem cada ----------
// As cargas são MÍNIMAS de propósito: o alvo é o caminho de montagem, não o conteúdo. Se a tela lê
// um campo que não veio, isso aparece como exceção — que é exatamente o que se quer saber.
const item = { indice: 0, slot: 0, nome: 'Espada', faccao: 'Reino', simbolo: '🗡️', stat: 'ATK', valor: '+5', valorNum: 5, equipado: true };
const apostolo = { id: 1, nome: 'Teste', simbolo: '🙂', faccao: 'Reino', tipo: 'Guardião', nivel: 1, hp: 100, hpMax: 100, ataque: 10, defesa: 10, velocidade: 85, medidor: 137, precisao: 50, resistencia: 120, taxaCrit: 15, danoCrit: 60, status: [], habilidades: [], liberado: true, vivo: true };
/// Os elementos da árvore com esta classe. O shim não tem querySelectorAll de verdade.
function procurar(raiz, classe) {
    if (!raiz) return [];
    const achados = [];
    (function descer(el) {
        if (typeof el.className === 'string' && el.className.split(' ').includes(classe)) achados.push(el);
        for (const f of el.children || []) descer(f);
    })(raiz);
    return achados;
}

const ALMA = ['Comum', 'Incomum', 'Raro', 'Épico', 'Lendário', 'Mítico']
    .map((nome, i) => ({ raridade: i, nome, quantidade: 100, xpPorUnidade: 5 ** i, max: i < 3 ? 100 : 0, podeFundir: i < 3 }));
const TELAS = [
    ['menu', { titulo: 'Apostle\'s War', subtitulo: '', opcoes: ['Jogar', 'Sair'], raiz: true, perfil: { nome: 'G', avatar: '🧭' } }],
    ['criarPerfil', {}],
    ['edicaoPerfil', { nome: 'G', avatar: '🧭', apostolos: [apostolo] }],
    ['montagemArena', { apostolos: [apostolo, { ...apostolo, id: 2 }] }],
    ['campanhaMapa', { capitulos: [{ nome: 'Reino', simbolo: '👑', liberado: true, fases: 7 }], atual: 0 }],
    ['campanhaFases', { faccao: 'Reino', simbolo: '👑', fases: [{ numero: 1, nome: 'Arma', liberada: true, rodada1: [apostolo], rodada2: [apostolo], item: null }], apostolos: [apostolo], faseSelecionada: 1, time: [], meusApostolos: [apostolo] }],
    // Os GANHOS com dois trechos e stats: um apóstolo que atravessou nível é o caminho longo da
    // animação (encher · zerar · encher), e é o que a carga precisa exercitar.
    ['fimDeFase', {
        venceu: true, titulo: 'Vitória', faccao: 'Reino', fase: 1, recompensa: null,
        temProxima: false, comOpcoes: true, xp: 2870,
        ganhos: [{
            simbolo: '🧙', tipoSimbolo: '🏹', nome: 'Mago', xpGanha: 2870, travou: true,
            trechos: [{ nivel: 8, de: 40, ate: 100 }, { nivel: 9, de: 0, ate: 62 }],
            stats: [{ icone: '❤️', rotulo: 'HP', de: 1240, ate: 1380 }],
        }],
        alma: ALMA.slice(0, 3),
    }],
    ['conquista', apostolo],
    // COM CONTEÚDO, e não vazio: um `slots: []` faz o `.map` não rodar nenhuma vez, e o corpo do
    // laço é justamente onde mora o que quebra. Foi assim que o harness deu verde com o
    // `ARSENAL_AREAS` esquecido no jogo.js — lista vazia não exercita nada.
    ['catedral', {
        slots: [0, 1, 2, 3, 4, 5, 6].map(s => ({ slot: s, nome: `Slot ${s}`, equipado: s === 0 ? item : null })),
        obtidos: [item, { ...item, equipado: false, valorNum: 3 }],
        totais: [{ stat: 'ATK', valor: '+10' }],
        roster: [apostolo, { ...apostolo, id: 2 }],
        selecionado: 0,
        // naParede E podeQueimar ligados ao mesmo tempo NÃO é um estado que o C# produz — é o que
        // faz os DOIS painéis renderizarem o corpo inteiro numa corrida só. Carga de cobertura;
        // quem diz quando cada um vale é o ProgressaoService, e isso tem teste em C#.
        apostolo: {
            ficha: { ...apostolo, nivel: 8 }, estrelas: 2, teto: 9, naParede: true,
            receita: [{ raridade: 1, nome: 'Incomum', quantidade: 150, xpPorUnidade: 5 }],
            faltando: [{ raridade: 2, nome: 'Raro', quantidade: 72, xpPorUnidade: 25 }],
            podeComprarEstrela: false, podeQueimar: true, motivo: 'Falta 72 de Raro.',
            xpAtual: 3600, xpAteAParede: 900,
            limiares: [{ nivel: 9, xp: 3600 }, { nivel: 10, xp: 4500 }],
            porNivel: [8, 9].map(n => ({ nivel: n, hp: 1200 + n * 40, ataque: 200 + n * 4, defesa: 90 + n * 2, velocidade: 85, precisao: 50, resistencia: 120 })),
        },
        alma: ALMA, tetoDeFusao: 2,
    }],
    ['compendio', { faccoes: [{ nome: 'Reino', simbolo: '👑', apostolos: [apostolo] }] }],
    ['compendioApostolo', apostolo],
    ['estado', { turno: 1, fase: 'Assistindo', mensagem: '', equipe1: [apostolo], equipe2: [{ ...apostolo, id: 9 }], quemAge: null, fila: [1, 9, 1, 9, 1], habilidades: [], alvosValidos: [], selecionado: null, auto: false, modo: 'Campanha', tema: 'reino' }],
    ['evento', { tipo: 'dano', alvoId: 1, valor: 10, critico: false, absorvidoPeloEscudo: 0, texto: null }],
];

(async () => {
    let mod;
    try {
        mod = await carregar(ENTRADA);
        await mod.evaluate();
    } catch (e) {
        console.error(`\n  FALHOU AO CARREGAR o front:\n  ${e.stack}\n`);
        process.exit(1);
    }
    console.log(`\n  front carregado · ${enviados.length} mensagem(ns) de boot enviadas ao C#`);

    if (typeof ouvinte !== 'function') {
        console.error('\n  o front não registrou ouvinte de mensagem — o harness não tem por onde entrar.\n');
        process.exit(1);
    }

    console.log(`\n  ${TELAS.length} telas:`);
    for (const [tipo, conteudo] of TELAS) {
        try {
            escritas = 0;
            ouvinte({ data: JSON.stringify({ tipo, conteudo }) });
            // Mensagem que não está NEM na tabela NEM no else-if não faz nada e não lança — e o
            // harness reportava isso como OK. Foi assim que a montagem da Arena passou verde depois
            // de eu tirar o `else if` sem ter posto a tela na tabela. "Não explodiu" ≠ "montou".
            if (escritas === 0) {
                console.log(`  ✗ ${tipo} — não fez NADA (não está ligada em lugar nenhum)`);
                queixar(`${tipo}: mensagem não tratada — nem na tabela TELAS nem no else-if`);
            } else {
                console.log(`  ✓ ${tipo}`);
            }
        } catch (e) {
            console.log(`  ✗ ${tipo} — ${e.message}`);
            queixar(`${tipo}: ${e.stack.split('\n').slice(0, 3).join(' | ')}`);
        }
    }

    // ---------- os painéis que SÓ abrem por clique ----------
    // O harness monta o que a mensagem do C# desenha, e os painéis de aprimorar não estão nisso:
    // eles nascem do clique num botão. Sem esta parte, "Evoluir nível" e "Evoluir estrela" chegam
    // ao jogo sem nunca terem rodado — que é o buraco que este harness existe pra não ter.
    console.log('\n  painéis por clique:');
    for (const b of procurar(porId.get('catedralPortas'), 'afBotao')) {
        const rotulo = (b.children.find(f => f.className === 'abRotulo') || {}).textContent || '?';
        try {
            b.click();
            // E os botões QUE O PAINEL abriu: o Máximo e o confirmar só existem depois do primeiro
            // clique, então são dois níveis de gesto — nenhum deles roda sem isto.
            let dentro = 0;
            for (const c of ['qlBotao', 'acaoConfirmar']) {
                for (const alvo of procurar(porId.get('catedralEstacao'), c)) { alvo.click(); dentro++; }
            }
            console.log(`  ✓ ${rotulo}${dentro ? ` · ${dentro} botão(ões) do painel` : ''}`);
        } catch (e) {
            console.log(`  ✗ ${rotulo} — ${e.message}`);
            queixar(`${rotulo}: ${e.stack.split('\n').slice(0, 3).join(' | ')}`);
        }
    }

    // ---------- os ids ----------
    // Estatico, varrendo o FONTE de todos os .js — e nao so os ids que esta corrida pediu. A
    // primeira versao conferia so os exercitados, e por isso deixou passar um id trocado numa tela
    // que so abre por clique: cobertura de corrida nao e cobertura de codigo.
    const html = fs.readFileSync(INDEX, 'utf8');
    // O `[^-\w]` antes do `id=` é o que separa `id="x"` de `data-id="x"` e de `aria-labelledby`.
    const idsNoHtml = new Set([...html.matchAll(/(?:^|[^-\w])id="([^"]+)"/g)].map(m => m[1]));

    const fontes = [];
    (function varrer(dir) {
        for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
            const p = path.join(dir, e.name);
            if (e.isDirectory()) varrer(p);
            else if (e.name.endsWith('.js')) fontes.push(p);
        }
    })(WWW);

    const idsNoCodigo = new Set();
    for (const f of fontes) {
        const src = fs.readFileSync(f, 'utf8');
        for (const m of src.matchAll(/getElementById\(\s*['"`]([^'"`]+)['"`]/g)) idsNoCodigo.add(m[1]);
    }

    // ---------- terminações de linha ----------
    // Entrou na verificação porque eu quebrei isso duas vezes seguidas e quem acusou foi o Visual
    // Studio, não eu: meus scripts detectam a quebra do arquivo de origem, mas o Edit e o sed nem
    // sempre preservam. Arquivo misto não é cosmético — um `else if` já grudou num comentário e
    // virou código comentado por causa disso.
    const mistos = fontes.filter(f => (fs.readFileSync(f, 'utf8').match(/(^|[^\r])\n/g) || []).length);
    console.log(`  terminações: ${fontes.length} arquivos · ${mistos.length} com CRLF e LF misturados`);
    for (const f of mistos) queixar(`${path.relative(WWW, f)}: terminação de linha MISTA`);

    const faltando = [...idsNoCodigo].filter(i => !idsNoHtml.has(i));
    console.log(`\n  ids: ${idsNoCodigo.size} citados no código (${idsPedidos.size} exercitados nesta corrida)`
        + ` · ${faltando.length} sem elemento no index.html`);
    for (const f of faltando) queixar(`getElementById("${f}") não existe no index.html`);
    if (problemas.length) {
        console.log('\n  ---- problemas ----');
        for (const p of problemas) console.log('  ' + p);
        console.log('\n  RESULTADO: FALHOU\n');
        process.exit(1);
    }
    console.log('\n  RESULTADO: todas as telas montam\n');
})();
