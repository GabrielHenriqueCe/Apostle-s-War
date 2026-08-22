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
// O QUE ELE COBRE, E O QUE NÃO — leia antes de confiar num verde daqui.
//
// Publica as mensagens do C#, monta cada tela e DISPARA os gestos: todo elemento com ouvinte
// registrado leva o evento do tipo dele, em rondas, até parar de aparecer ouvinte novo (clique
// abre painel, painel registra mais ouvintes). Arrastar e mouse-arrasto vão como SEQUÊNCIA
// (dragstart→dragover→drop→dragend, mousedown→mousemove→mouseup) porque os handlers guardam
// estado entre um e outro — disparados soltos, saem pelo `return` da primeira linha.
//
// FORA, e são duas coisas:
//   · tudo que roda contra o motor C# — a batalha depois do primeiro quadro;
//   · o markup ESTÁTICO do index.html. O DOM daqui nasce do que o JS pede: `getElementById` e
//     `createElement`. Elemento que só existe escrito no HTML e é achado por CLASSE nunca é
//     materializado, então `querySelectorAll('.setupJog')` (arena.js, no carregamento) continua
//     devolvendo vazio e aqueles ouvintes nem chegam a ser registrados. Fechar isso é construir a
//     árvore estática a partir do index.html — outro trabalho.
//
// Verde aqui quer dizer "as telas montam e os gestos não explodem", nunca "o jogo funciona" —
// isso continua sendo teste em jogo, e é do Gabriel.
//
// A ARMADILHA: disparar ouvinte fora da ordem que um jogador produziria gera estado que o C# nunca
// manda. Exceção daqui é PISTA, não veredito, e por isso o relatório nomeia o gesto e o elemento.
// E o ∅ (não tocou no DOM) pesa tanto quanto o ✗: handler que depende de estado não montado sai
// daqui calado, e calado já deixou tela passar verde estando desligada.
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

// ---------- seletores ----------
// Um seletor é uma sequência de PASSOS separados por espaço: o último casa o elemento, os
// anteriores têm de casar algum ancestral. Entende só o que o front usa — `tag`, `#id`, `.classe`
// (podendo repetir) e `[data-x="v"]`. Passo que ele NÃO entende vira queixa, não `false` calado:
// seletor mudo devolveria lista vazia e a tela passaria verde com o laço nunca tendo rodado, que é
// exatamente o buraco que a lista vazia do `querySelectorAll` antigo abria.
const passosMemo = new Map();

function analisarPasso(passo) {
    const cond = { tag: null, id: null, classes: [], attrs: [] };
    let resto = passo;
    const tag = resto.match(/^[a-zA-Z][\w-]*/);
    if (tag) { cond.tag = tag[0].toUpperCase(); resto = resto.slice(tag[0].length); }
    const fichas = resto.match(/[.#][\w-]+|\[[^\]]+\]/g) || [];
    if (fichas.join('').length !== resto.length) return null;   // sobrou sintaxe desconhecida
    for (const f of fichas) {
        if (f[0] === '.') cond.classes.push(f.slice(1));
        else if (f[0] === '#') cond.id = f.slice(1);
        else {
            const m = f.slice(1, -1).match(/^([\w-]+)(?:=(?:"([^"]*)"|'([^']*)'|(.*)))?$/);
            if (!m) return null;
            cond.attrs.push([m[1], m[2] ?? m[3] ?? m[4] ?? null]);
        }
    }
    return cond;
}

function analisar(sel) {
    if (passosMemo.has(sel)) return passosMemo.get(sel);
    const passos = String(sel).trim().split(/\s+/).map(analisarPasso);
    const bom = passos.length && passos.every(Boolean) ? passos : null;
    if (!bom) queixar(`seletor não suportado pelo shim: "${sel}" — o harness devolveria vazio calado`);
    passosMemo.set(sel, bom);
    return bom;
}

// `className` e `classList` são independentes neste shim, e o front usa os dois — conferir só um
// faria `.linhaLog.atual` (posta por classList) nunca casar.
const temClasse = (el, c) =>
    (typeof el.className === 'string' && el.className.split(/\s+/).includes(c)) || el.classList.contains(c);

function casaPasso(el, cond) {
    if (cond.tag && el.tagName !== cond.tag) return false;
    if (cond.id && el.id !== cond.id) return false;
    for (const c of cond.classes) if (!temClasse(el, c)) return false;
    for (const [k, v] of cond.attrs) {
        const atual = el.getAttribute(k);
        if (atual === null || (v !== null && atual !== v)) return false;
    }
    return true;
}

function casaSeletor(el, sel) {
    const passos = analisar(sel);
    if (!passos || !casaPasso(el, passos[passos.length - 1])) return false;
    let i = passos.length - 2, pai = el._pai;
    while (i >= 0) {
        if (!pai) return false;
        if (casaPasso(pai, passos[i])) i--;
        pai = pai._pai;
    }
    return true;
}

/// Descendentes de `raiz` que casam `sel`. `parar` corta no primeiro (é o querySelector).
function buscar(raiz, sel, parar) {
    escritas++;
    if (!analisar(sel)) return [];
    const achados = [];
    (function descer(el) {
        for (const f of el.children || []) {
            if (casaSeletor(f, sel)) { achados.push(f); if (parar) return; }
            descer(f);
            if (parar && achados.length) return;
        }
    })(raiz);
    return achados;
}

function adotar(pai, filho) { escritas++; if (filho && typeof filho === 'object') filho._pai = pai; }

// ---------- o evento de mentira ----------
// Precisa ter FORMA: handler que lê `e.key`, `e.clientX` ou `e.dataTransfer` explodiria por falta
// de campo, e a exceção seria do harness, não do front.
// O `dataTransfer` é ÚNICO no módulo de propósito — o par dragstart→drop existe pra transportar
// coisa entre os dois, e um objeto novo por evento quebraria o transporte que se quer testar.
const TRANSFERENCIA = {
    _d: new Map(), effectAllowed: '', dropEffect: '',
    setData(t, v) { this._d.set(String(t), String(v)); },
    getData(t) { return this._d.get(String(t)) ?? ''; },
    clearData() { this._d.clear(); }, setDragImage() {},
    get types() { return [...this._d.keys()]; },
};

function criarEvento(tipo, alvo, extra = {}) {
    return {
        type: tipo, target: alvo, currentTarget: alvo, srcElement: alvo,
        bubbles: true, cancelable: true, defaultPrevented: false,
        preventDefault() { this.defaultPrevented = true; }, stopPropagation() {},
        stopImmediatePropagation() {},
        button: 0, buttons: 1, detail: 1,
        clientX: 0, clientY: 0, offsetX: 0, offsetY: 0, pageX: 0, pageY: 0, deltaY: 0,
        key: '', code: '', shiftKey: false, ctrlKey: false, altKey: false, metaKey: false,
        dataTransfer: TRANSFERENCIA, animationName: '', propertyName: '',
        ...extra,
    };
}

function criarElemento(id = '', tag = 'DIV') {
    escritas++;
    const filhos = [];
    const el = {
        _pai: null,
        id, tagName: tag, hidden: false, textContent: '', value: '', className: '', title: '',
        width: LARGURA, height: 700, clientWidth: LARGURA, clientHeight: 700,
        offsetWidth: LARGURA, offsetHeight: 700, scrollTop: 0, scrollHeight: 0, scrollLeft: 0,
        readOnly: false, disabled: false, checked: false, dataset: {},
        style: new Proxy({ setProperty() {}, removeProperty() {}, getPropertyValue: () => '' },
            { set: (t, k, v) => (t[k] = v, true), get: (t, k) => (k in t ? t[k] : '') }),
        classList: {
            _s: new Set(),
            add(...c) { escritas++; c.forEach(x => this._s.add(x)); },
            remove(...c) { escritas++; c.forEach(x => this._s.delete(x)); },
            toggle(c, f) { escritas++; const v = f ?? !this._s.has(c); v ? this._s.add(c) : this._s.delete(c); return v; },
            contains(c) { return this._s.has(c); },
        },
        // Os ouvintes ficam GUARDADOS: é deles que a varredura de gestos tira o que disparar.
        _ev: {},
        addEventListener(tipo, fn) { (el._ev[tipo] ||= []).push(fn); },
        removeEventListener(tipo, fn) {
            const l = el._ev[tipo];
            if (l) el._ev[tipo] = l.filter(f => f !== fn);
        },
        focus() {}, blur() {}, select() {},
        disparar(tipo, extra) { for (const fn of (el._ev[tipo] || []).slice()) fn(criarEvento(tipo, el, extra)); },
        click() { el.disparar('click'); },
        setAttribute(k, v) { if (k.startsWith('data-')) el.dataset[k.slice(5)] = v; },
        removeAttribute(k) { if (k.startsWith('data-')) delete el.dataset[k.slice(5)]; },
        // Só data-*: é o único atributo que o shim guarda, e é o que os seletores do front leem
        // (`.combatente[data-id="…"]`). Atributo fora disso responde "não tenho", não `null` mudo.
        getAttribute(k) { return k.startsWith('data-') ? (el.dataset[k.slice(5)] ?? null) : null; },
        hasAttribute(k) { return el.getAttribute(k) !== null; },
        matches(sel) { return casaSeletor(el, sel); },
        appendChild(c) { adotar(el, c); filhos.push(c); return c; },
        append(...c) { for (const f of c) adotar(el, f); filhos.push(...c); },
        prepend(...c) { for (const f of c) adotar(el, f); filhos.unshift(...c); },
        replaceChildren(...c) {
            for (const f of filhos) if (f._pai === el) f._pai = null;
            filhos.length = 0;
            for (const f of c) adotar(el, f);
            filhos.push(...c);
        },
        insertBefore(c) { adotar(el, c); filhos.push(c); return c; },
        // `remove()` era no-op, e isso mentia pra varredura: elemento tirado da tela continuava na
        // árvore e levaria gesto que o jogador não tem como dar.
        remove() {
            escritas++;
            const p = el._pai;
            if (!p) return;
            const i = p.children.indexOf(el);
            if (i >= 0) p.children.splice(i, 1);
            el._pai = null;
        },
        querySelectorAll: (sel) => buscar(el, sel, false),
        // Devolve elemento novo quando NÃO acha, em vez de null: o alvo é o CAMINHO de montagem
        // rodar inteiro, e um null faria a tela parar na primeira consulta em vez de mostrar o que
        // quebra mais adiante. Achando de verdade, devolve o de verdade.
        querySelector: (sel) => buscar(el, sel, true)[0] || criarElemento(sel),
        closest(sel) {
            for (let n = el; n; n = n._pai) if (casaSeletor(n, sel)) return n;
            return null;
        },
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
    querySelectorAll: (sel) => buscarNoDoc(sel, false),
    querySelector: (sel) => buscarNoDoc(sel, true)[0] || criarElemento(sel),
    // O documento também GUARDA ouvinte: o `keydown` que mata o F5 e leva o Enter das telas de
    // passagem mora aqui (jogo.js), e enquanto isto era no-op ele nunca rodava no harness.
    _ev: {},
    addEventListener(tipo, fn) { (document._ev[tipo] ||= []).push(fn); },
    removeEventListener(tipo, fn) {
        const l = document._ev[tipo];
        if (l) document._ev[tipo] = l.filter(f => f !== fn);
    },
    disparar(tipo, extra) { for (const fn of (document._ev[tipo] || []).slice()) fn(criarEvento(tipo, document, extra)); },
};

// A árvore aqui é uma FLORESTA: `getElementById` cria elemento solto, sem pai, e é nele que a tela
// pendura os filhos. Procurar só a partir do `body` acharia quase nada.
const raizesDoDoc = () => [document.body, document.documentElement, ...porId.values()];

function buscarNoDoc(sel, parar) {
    if (!analisar(sel)) return [];
    const achados = [];
    for (const r of raizesDoDoc()) {
        if (casaSeletor(r, sel) && !achados.includes(r)) achados.push(r);
        if (parar && achados.length) return achados;
        for (const f of buscar(r, sel, parar)) if (!achados.includes(f)) achados.push(f);
        if (parar && achados.length) return achados;
    }
    return achados;
}

// ---------- a ponte ----------
// Guardamos o ouvinte que o front registra: é por ele que as telas entram.
let ouvinte = null;
const enviados = [];

const janela = {
    innerWidth: LARGURA, innerHeight: ALTURA, devicePixelRatio: 1,
    requestAnimationFrame() { return 1; }, cancelAnimationFrame() {},
    performance: { now: () => 0 },
    getComputedStyle: () => ({ getPropertyValue: () => '' }),
    // A janela guarda ouvinte pelo mesmo motivo do documento: o `mousemove`/`mouseup` do pan do
    // mapa da campanha é registrado nela, e sem isto o arrasto do mapa nunca era exercitado.
    _ev: {},
    addEventListener(tipo, fn) { (janela._ev[tipo] ||= []).push(fn); },
    removeEventListener(tipo, fn) {
        const l = janela._ev[tipo];
        if (l) janela._ev[tipo] = l.filter(f => f !== fn);
    },
    disparar(tipo, extra) { for (const fn of (janela._ev[tipo] || []).slice()) fn(criarEvento(tipo, janela, extra)); },
    setTimeout: () => 0, clearTimeout() {}, setInterval: () => 0, clearInterval() {},
    chrome: { webview: {
        // Conta como TOQUE: metade dos botões do jogo não mexe na tela, só avisa o C# (`mandar`).
        // Sem isto eles caíam no ∅ — "não fez nada" — quando o que fizeram é exatamente o esperado.
        postMessage: (m) => { escritas++; enviados.push(m); },
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
const item = { indice: 0, slot: 0, nome: 'Espada', faccao: 'Reino', faccaoSimbolo: '👑', simbolo: '🗡️', stat: 'ATK', statChave: 'ataque', valor: '+5', valorNum: 5, equipado: true, portadorSimbolo: '', nivel: 3, estrelas: 1, pct: 40 };
// Um só objeto pros dois contratos que o front recebe — o `ApostoloVisto` das telas de menu e o
// combatente do combate. Chave a mais é ignorada pelo JS; chave a MENOS é caminho que não roda.
// O `bonus` vem SEMPRE do C# (é a tela que decide se mostra a conta aberta), e a ficha lê
// `c.bonus` — sem ele o painel de equipamento nunca desenha no harness.
const BONUS = { hp: 40, ataque: 6, defesa: 3, velocidade: 0, precisao: 2, resistencia: 0, taxaCritPct: 1, danoCritPct: 5 };
// A vida do COMBATE é `hpAtual`/`hpMaximo`/`escudo` (CombatenteVisto) — não `hp`, que é o HP de
// BASE da ficha (ApostoloDetalheVista). Mandar só `hp` deixava o `hpMaximo` undefined e a barra de
// vida calculava 0% sem erro nenhum na tela.
const apostolo = { id: 1, nome: 'Teste', simbolo: '🙂', tipoSimbolo: '🏹', faccao: 'Reino', tipo: 'Guardião', desbloqueado: true, estrelas: 2, nivel: 1, xpPct: 45, posicao: [[0.3, 0.5], [0.7, 0.5]], hp: 100, hpAtual: 72, hpMaximo: 100, escudo: 15, ataque: 10, defesa: 10, velocidade: 85, medidor: 137, precisao: 50, resistencia: 120, taxaCritPct: 15, danoCritPct: 60, status: [], habilidades: [], bonus: BONUS, vivo: true };
// `DificuldadeVista`: as quatro, com duas trancadas — a barra desenha cadeado e requisito, e uma
// lista toda liberada não exercita esse ramo.
const DIFICULDADES = ['Normal', 'Difícil', 'Insano', 'Apocalipse'].map((nome, i) => ({
    nome, valor: i, desbloqueada: i < 2, requisito: i < 2 ? null : `Conclua ${['Normal', 'Difícil', 'Insano'][i - 1]}`,
}));
// ---------- o que o jogador ALCANÇA ----------
// O index.html carrega TODAS as telas ao mesmo tempo e o `mostrarCena` esconde as que não são a
// atual pondo `hidden` no contêiner. Quem varre gesto tem de respeitar isso, senão clica em tela
// escondida — e clique em tela escondida não é bug do jogo, é bug do harness.
//
// O `_pai` sozinho não resolve: `getElementById` devolve elemento SOLTO, então `#catedralPortas`
// não sabe que mora dentro de `#catedral`. O esqueleto estático vem do próprio index.html.
const paiDeId = (() => {
    const pai = new Map();
    const html = fs.readFileSync(INDEX, 'utf8').replace(/<!--[\s\S]*?-->/g, '');
    const VAZIA = new Set(['br', 'hr', 'img', 'input', 'meta', 'link', 'source', 'col', 'area', 'embed']);
    const pilha = [];
    for (const [, fecha, tag, attrs] of html.matchAll(/<(\/?)([a-zA-Z][\w-]*)([^>]*)>/g)) {
        const t = tag.toLowerCase();
        if (fecha) {
            for (let i = pilha.length - 1; i >= 0; i--) if (pilha[i].tag === t) { pilha.length = i; break; }
            continue;
        }
        const id = (attrs.match(/(?:^|[^-\w])id="([^"]+)"/) || [])[1] || null;
        if (id) {
            const p = [...pilha].reverse().find(x => x.id);
            if (p) pai.set(id, p.id);
        }
        if (!VAZIA.has(t) && !attrs.trim().endsWith('/')) pilha.push({ tag: t, id });
    }
    return pai;
})();

/// Nem o elemento nem nenhum ancestral dele — vivo ou só no esqueleto do HTML — está escondido.
function alcancavel(el) {
    for (let n = el; n; n = n._pai) {
        if (n.hidden === true) return false;
        if (!n._pai && n.id) {
            for (let id = paiDeId.get(n.id); id; id = paiDeId.get(id)) {
                if (porId.get(id)?.hidden === true) return false;
            }
        }
    }
    return true;
}

// ---------- a varredura de gestos ----------
// Estes NÃO vão soltos: os handlers guardam estado entre um evento e o outro (`arrastando` no
// ui/time.js, `mapaArrastando` na campanha), então disparados avulsos saem pelo `return` da
// primeira linha sem exercitar nada. Vão em sequência, mais abaixo. O `message` fica de fora
// porque é a ponte do C#, não gesto de jogador.
const SEQUENCIA = new Set(['dragstart', 'dragenter', 'dragover', 'dragleave', 'drop', 'dragend',
                           'mousedown', 'mousemove', 'mouseup', 'message']);
// As teclas que o front lê: Enter e Escape movem tela; F5 e Ctrl+R são o guarda que impede
// recarregar a página (recarregar mata a partida — ver jogo.js).
const TECLAS = [{ key: 'Enter' }, { key: 'Escape' }, { key: 'F5' }, { key: 'r', ctrlKey: true }];
const RONDAS = 3;          // clique abre painel, painel registra ouvinte novo, e por aí
const MAX_PARES = 40;      // teto das combinações origem×alvo do arrasto

// Elemento+tipo já disparado, GLOBAL e não por tela: os elementos do shim sobrevivem à troca de
// tela, e sem isto a varredura re-disparava tudo a cada uma das 14. Assim cada ouvinte roda uma
// vez, logo depois da tela que o registrou.
const jaFoi = new Map();
const marcado = (el, tipo) => {
    let s = jaFoi.get(el);
    if (!s) jaFoi.set(el, s = new Set());
    if (s.has(tipo)) return true;
    s.add(tipo);
    return false;
};

const temTexto = (s) => typeof s === 'string' && s.trim() !== '';
const nomear = (el) =>
    el === document ? 'document'
        : el === janela ? 'window'
            : el.id ? `#${el.id}`
                : temTexto(el.className) ? `.${String(el.className).trim().split(/\s+/)[0]}`
                    : String(el.tagName || '?').toLowerCase();

/// Tudo que tem ouvinte, varrendo a floresta inteira (document e window entram por fora).
function comOuvinte() {
    const vistos = new Set(), achados = [];
    const descer = (el) => {
        if (!el || typeof el !== 'object' || vistos.has(el) || el.hidden === true) return;
        vistos.add(el);
        if (el._ev && Object.values(el._ev).some(l => l.length)) achados.push(el);
        for (const f of el.children || []) descer(f);
    };
    for (const r of raizesDoDoc()) if (alcancavel(r)) descer(r);
    return achados;
}

/// Roda o gesto; devolve a queixa se explodiu, ou null. Quem julga é o `contabilizar`.
function rodar(el, tipo, rotulo, extras = [undefined]) {
    try {
        for (const extra of extras) el.disparar(tipo, extra);
        return null;
    } catch (e) {
        return `${rotulo} · ${nomear(el)} · ${tipo}: ${e.stack.split('\n').slice(0, 3).join(' | ')}`;
    }
}

/// Uma linha do placar: ✓ mexeu no DOM (ou avisou o C#) · ∅ não mexeu · ✗ explodiu.
/// Uma SEQUÊNCIA inteira vale uma linha só: os passos do meio de um arrasto (`dragstart` guarda o
/// que está sendo arrastado, `dragend` limpa) não mexem em nada por definição, e contá-los um a um
/// enchia o ∅ de ruído que escondia o ∅ que importa.
function contabilizar(nome, conta, corpo) {
    escritas = 0;
    const erro = corpo();
    if (erro) { conta.erro++; queixar(erro); return; }
    if (escritas) conta.ok++;
    else { conta.vazio++; conta.mudos.push(nome); }
}

function varrerGestos(rotulo, conta) {
    for (let ronda = 1; ronda <= RONDAS; ronda++) {
        let disparados = 0;
        for (const el of [document, janela, ...comOuvinte()]) {
            for (const [tipo, ouvintes] of Object.entries(el._ev || {})) {
                if (!ouvintes.length || SEQUENCIA.has(tipo) || marcado(el, tipo)) continue;
                contabilizar(`${nomear(el)}·${tipo}`, conta,
                    () => rodar(el, tipo, rotulo, tipo === 'keydown' ? TECLAS : [undefined]));
                disparados++;
            }
        }
        if (!disparados) break;   // ponto fixo: ninguém novo apareceu
    }
    sequenciasDeArrasto(rotulo, conta);
    sequenciasDeMouse(rotulo, conta);
}

// O protocolo do arrastar, na ordem que o navegador emite. Fora dela o `drop` vê `arrastando`
// nulo e volta na primeira linha — resultado ∅, que parece cobertura e não é.
function sequenciasDeArrasto(rotulo, conta) {
    const todos = comOuvinte();
    const origens = todos.filter(e => (e._ev.dragstart || []).length);
    const alvos = todos.filter(e => (e._ev.drop || []).length);
    let pares = 0;
    for (const o of origens) {
        for (let i = 0; i < alvos.length; i++) {
            const a = alvos[i];
            if (pares >= MAX_PARES) return;
            if (marcado(o, `arrasto→${i}`)) continue;
            pares++;
            TRANSFERENCIA.clearData();
            contabilizar(`${nomear(o)}⇢${nomear(a)}·arrasto`, conta, () =>
                rodar(o, 'dragstart', rotulo)
                || ['dragenter', 'dragover', 'drop', 'dragleave'].reduce((e, t) => e || rodar(a, t, rotulo), null)
                || rodar(o, 'dragend', rotulo));
        }
    }
}

// mousedown no elemento → mousemove/mouseup na JANELA (é onde o pan do mapa registra), com clientX
// andando mais que o limiar de 4px que a campanha usa pra separar arrasto de clique.
function sequenciasDeMouse(rotulo, conta) {
    for (const el of comOuvinte()) {
        if (!(el._ev.mousedown || []).length || marcado(el, 'mouse-arrasto')) continue;
        contabilizar(`${nomear(el)}·mouse-arrasto`, conta, () =>
            rodar(el, 'mousedown', rotulo, [{ clientX: 100, clientY: 100 }])
            || rodar(janela, 'mousemove', rotulo, [{ clientX: 160, clientY: 100 }])
            || rodar(janela, 'mouseup', rotulo, [{ clientX: 160, clientY: 100 }]));
    }
}

const ALMA = ['Comum', 'Incomum', 'Raro', 'Épico', 'Lendário', 'Mítico']
    .map((nome, i) => ({ raridade: i, nome, quantidade: 100, xpPorUnidade: 5 ** i, max: i < 3 ? 100 : 0, podeFundir: i < 3 }));
// O PÓ com as três primeiras faixas cheias e as outras ZERADAS: a faixa de saldo só desenha as que
// têm, então uma lista toda cheia não exercitaria o filtro que decide isso.
const PO = ALMA.map((a, i) => ({ raridade: i, nome: a.nome, quantidade: i < 3 ? 60 : 0, pontosPorUnidade: 5 ** i, max: i < 3 ? 100 : 0, podeFundir: i < 3 }));
const TELAS = [
    // `MenuVisto`: avatar e nome vêm SOLTOS, não embrulhados num `perfil` — o menu desenha o canto
    // do jogador a partir do `m.avatar`, e o mapa da campanha usa esse mesmo avatar como marcador.
    ['menu', {
        titulo: 'Apostle\'s War', subtitulo: '', raiz: true, avatar: '🧭', nome: 'G',
        opcoes: [{ rotulo: 'Jogar', icone: '⚔️', habilitado: true, confirmar: null, marcado: null },
                 { rotulo: 'Excluir conta', icone: '🗑️', habilitado: true, confirmar: 'Apagar tudo?', marcado: null }],
    }],
    ['criarPerfil', {}],
    ['edicaoPerfil', { nome: 'G', avatar: '🧭', apostolos: [apostolo] }],
    ['montagemArena', { apostolos: [apostolo, { ...apostolo, id: 2 }] }],
    // `MapaVista` — dois capítulos, um trancado: o nó bloqueado não ganha o botão de entrar, e é
    // outro ramo do laço.
    ['campanhaMapa', {
        capitulos: [{ simbolo: '👑', nome: 'Reino', desbloqueado: true, concluido: true },
                    { simbolo: '😈', nome: 'Decaídos', desbloqueado: false, concluido: false }],
        posicao: 0, dificuldades: DIFICULDADES, dificuldade: 0,
    }],
    // `FasesVista`. O `faseSelecionada` TEM de casar o `numero` de uma fase `desbloqueado` — é o
    // que abre o `#faseDetalhe`, e com ele fechado o 🎲, o picker e os quatro slots do time ficam
    // escondidos e gesto nenhum os alcança.
    ['campanhaFases', {
        capituloNome: 'Reino', capituloSimbolo: '👑',
        fases: [
            { numero: 1, nome: 'Arma', desbloqueado: true, concluido: true, rodada1: [apostolo], rodada2: [{ ...apostolo, id: 2 }], nivelDoInimigo: 3, drop: { simbolo: '🗡️', nome: 'Arma', principais: 'ATK · HP', quantidade: 2 } },
            { numero: 2, nome: 'Elmo', desbloqueado: false, concluido: false, rodada1: [], rodada2: [], nivelDoInimigo: 4, drop: { simbolo: '🪖', nome: 'Elmo', principais: 'DEF', quantidade: 1 } },
        ],
        meusApostolos: [apostolo, { ...apostolo, id: 2, nome: 'Outro', simbolo: '🧙' }],
        faseSelecionada: 1, timeMontado: [0],
        dificuldades: DIFICULDADES, dificuldade: 0,
    }],
    // Os GANHOS com dois trechos e stats: um apóstolo que atravessou nível é o caminho longo da
    // animação (encher · zerar · encher), e é o que a carga precisa exercitar.
    // `FimDeFaseVista`. `podeProxima` ligado é o que faz existir o botão de continuar — com ele
    // falso, um dos três botões da tela de decisão nunca é criado nem clicado.
    ['fimDeFase', {
        venceu: true, recompensa: null, podeProxima: true, proximoECapitulo: false,
        comOpcoes: true, xp: 2870,
        ganhos: [{
            simbolo: '🧙', tipoSimbolo: '🏹', nome: 'Mago', xpGanha: 2870, travou: true,
            trechos: [{ nivel: 8, de: 40, ate: 100 }, { nivel: 9, de: 0, ate: 62 }],
            stats: [{ icone: '❤️', rotulo: 'HP', de: 1240, ate: 1380 }],
        }],
        alma: ALMA.slice(0, 3), po: PO.slice(0, 3),
    }],
    ['conquista', apostolo],
    // COM CONTEÚDO, e não vazio: um `slots: []` faz o `.map` não rodar nenhuma vez, e o corpo do
    // laço é justamente onde mora o que quebra. Foi assim que o harness deu verde com o
    // `ARSENAL_AREAS` esquecido no jogo.js — lista vazia não exercita nada.
    ['catedral', {
        slots: [0, 1, 2, 3, 4, 5, 6].map(s => ({ slot: s, nome: `Slot ${s}`, equipado: s === 0 ? item : null })),
        // A terceira está VESTIDA NUM ALIADO: é ela que faz o emoji do portador e o chip "Vestidas"
        // rodarem. Sem uma peça assim na carga, os dois caminhos ficam mortos no harness.
        obtidos: [item, { ...item, equipado: false, valorNum: 3 },
            { ...item, equipado: false, valorNum: 4, portadorSimbolo: '🥷' }],
        // A prévia LIGADA: é ela que marca a peça em comparação no acervo e desenha as linhas que
        // mudariam. Com `null` (o estado mais comum) esses dois trechos não rodam.
        previa: { indice: 0, deltas: [{ rotulo: 'Ataque', antes: 200, depois: 214, delta: 14, sufixo: '' }] },
        roster: [apostolo, { ...apostolo, id: 2 }],
        selecionado: 0,
        // naParede E podeQueimar ligados ao mesmo tempo NÃO é um estado que o C# produz — é o que
        // faz os DOIS painéis renderizarem o corpo inteiro numa corrida só. Carga de cobertura;
        // quem diz quando cada um vale é o ProgressaoService, e isso tem teste em C#.
        apostolo: {
            ficha: { ...apostolo, nivel: 8 }, estrelas: 2, teto: 9, naParede: true,
            receita: [{ raridade: 1, nome: 'Incomum', quantidade: 150, xpPorUnidade: 5, max: 100, podeFundir: true }],
            faltando: [{ raridade: 2, nome: 'Raro', quantidade: 72, xpPorUnidade: 25, max: 0, podeFundir: false }],
            podeComprarEstrela: false, podeQueimar: true, motivo: 'Falta 72 de Raro.',
            xpAtual: 3600, xpAteAParede: 900,
            limiares: [{ nivel: 9, xp: 3600 }, { nivel: 10, xp: 4500 }],
            porNivel: [8, 9].map(n => ({ nivel: n, hp: 1200 + n * 40, ataque: 200 + n * 4, defesa: 90 + n * 2, velocidade: 85, precisao: 50, resistencia: 120 })),
        },
        alma: ALMA, tetoDeFusao: 2,
    }],
    // A FORJA com naParede e podeQueimar LIGADOS ao mesmo tempo — estado que o C# não produz, mas
    // que faz as três bancadas renderizarem o corpo inteiro numa corrida só. Mesma carga de
    // cobertura da Catedral.
    ['forja', {
        peca: { ...item, indice: 0, nivel: 9, estrelas: 1, pct: 100, valor: '+57,5', faccaoSimbolo: '👑' },
        slotNome: 'Arma', slotsComPeca: 3,
        acervo: [{ ...item, indice: 0, nivel: 9, estrelas: 1, pct: 100 }, { ...item, indice: 1, equipado: false, nivel: 3, estrelas: 0, pct: 40 }],
        teto: 9, naParede: true, pontos: 780, pontosAteAParede: 120,
        po: PO, tetoDeFusao: 2,
        receita: [{ raridade: 0, nome: 'Comum', quantidade: 50, pontosPorUnidade: 1, max: 100, podeFundir: true }],
        faltando: [{ raridade: 1, nome: 'Incomum', quantidade: 30, pontosPorUnidade: 5, max: 0, podeFundir: false }],
        podeComprarEstrela: false, podeQueimar: true, motivo: 'Falta 30 de Incomum.',
        patamares: [9, 10].map(n => ({ nivel: n, pontos: n * 100 })),
        porNivel: [9, 10].map(n => ({ nivel: n, valor: '+' + n, noApostolo: [{ rotulo: 'Ataque', antes: 200, depois: 214, delta: 14, sufixo: '' }] })),
        portadorNome: 'Teste',
    }],
    // `CompendioVista`: a peça é `CompendioApostoloVista` (índice + travado), não o apóstolo
    // inteiro. Um travado junto porque a peça bloqueada é outro ramo — e é ela que NÃO abre ficha.
    ['compendio', {
        faccoes: [{
            nome: 'Reino', simbolo: '👑', apostolos: [
                { indice: 0, simbolo: '🙂', nome: 'Teste', desbloqueado: true },
                { indice: 1, simbolo: '❔', nome: '???', desbloqueado: false },
            ],
        }],
    }],
    ['compendioApostolo', apostolo],
    ['estado', { turno: 1, fase: 'Assistindo', mensagem: '', equipe1: [apostolo], equipe2: [{ ...apostolo, id: 9 }], quemAge: null, fila: [1, 9, 1, 9, 1], habilidades: [], alvosValidos: [], ladoVencedor: 0, auto: false, focoId: 0, modo: 'Campanha', tema: 'reino' }],
    ['evento', { tipo: 'dano', alvoId: 1, valor: 10, critico: false, absorvidoPeloEscudo: 0, texto: null }],
];

// ---------- ler os DTOs do C# ----------
const PONTE = path.resolve(__dirname, '../ApostlesWar.Presentation/Front/PonteWebView2.cs');
const DTOS = path.resolve(__dirname, '../ApostlesWar.Presentation/Front/EstadoDeBatalha.cs');

// O camelCase do System.Text.Json (`JsonNamingPolicy.CamelCase`, ligado no PonteWebView2): minuscula
// a corrida inicial de MAIÚSCULAS, parando antes da última se ela vier seguida de minúscula.
// `HP`→`hp` · `HPMax`→`hpMax` · `XpPct`→`xpPct` · `TaxaCritPct`→`taxaCritPct`.
function camel(nome) {
    const c = [...nome];
    for (let i = 0; i < c.length && c[i] === c[i].toUpperCase() && /[A-Z]/.test(c[i]); i++) {
        if (i > 0 && i + 1 < c.length && !/[A-Z]/.test(c[i + 1])) break;
        c[i] = c[i].toLowerCase();
    }
    return c.join('');
}

/// Os parâmetros de um `internal record Nome(...)`, já em camelCase. Todos os DTOs do front são
/// record POSICIONAL, então a lista de parâmetros É a lista de propriedades.
function propsDoRecord(fonte, tipo) {
    const inicio = fonte.indexOf(`internal record ${tipo}(`);
    if (inicio < 0) return null;
    let i = fonte.indexOf('(', inicio), profundidade = 0, fim = i;
    for (; fim < fonte.length; fim++) {
        if (fonte[fim] === '(') profundidade++;
        else if (fonte[fim] === ')' && --profundidade === 0) break;
    }
    // Comentário some ANTES de qualquer coisa: o `///` aparece no meio da lista (o
    // ApostoloDetalheVista e o EstadoDeBatalha documentam parâmetros assim) e o `//` de fim de linha
    // carrega parêntese no texto (`// índice global (Id) de quem está agindo`), que desalinharia a
    // contagem de profundidade e viraria "propriedade" chamada `(Id`.
    // O `\r?` do split não é decoração: com `split('\n')` sobra um `\r` no fim de cada linha, e o
    // `$` do regex de comentário deixa de casar — o `//` passava inteiro e virava "propriedade".
    const corpo = fonte.slice(i + 1, fim).split(/\r?\n/)
        .filter(l => !l.trim().startsWith('///'))
        .map(l => l.replace(/\/\/.*$/, ''))
        .join('\n');

    const partes = [];
    let atual = '', nivel = 0;
    for (const ch of corpo) {
        if ('<([' .includes(ch)) nivel++;
        else if ('>)]'.includes(ch)) nivel--;
        if (ch === ',' && nivel === 0) { partes.push(atual); atual = ''; continue; }
        atual += ch;
    }
    partes.push(atual);

    return partes.map(p => {
        const semPadrao = p.split('=')[0].trim();          // tira o valor default
        const tokens = semPadrao.split(/\s+/).filter(Boolean);
        if (!tokens.length) return null;
        // O TIPO importa tanto quanto o nome: é por ele que a conferência DESCE nos aninhados, e o
        // erro que motivou tudo isto (`liberada` no lugar de `desbloqueado`) mora dentro de um
        // `List<FaseVista>` — no topo ele passa despercebido.
        const bruto = tokens.slice(0, -1).join(' ');
        const interno = bruto.match(/^(?:List|IReadOnlyList|IEnumerable)<(.+)>\??$/);
        return { nome: camel(tokens[tokens.length - 1]), tipo: (interno ? interno[1] : bruto).replace(/[?\[\]]/g, '').trim() };
    }).filter(Boolean);
}

/// tipo da mensagem → as propriedades que o C# manda nela.
function contratosDaPonte() {
    const ponte = fs.readFileSync(PONTE, 'utf8');
    const dtos = fs.readFileSync(DTOS, 'utf8');
    const mapa = new Map();
    for (const m of ponte.matchAll(/public void \w+\(([^)]*)\)\s*=>\s*Enviar\("(\w+)",\s*([^;]+)\);/g)) {
        const [, params, tipo, carga] = m;
        const anonimo = carga.trim().match(/^new\s*\{([^}]*)\}$/);
        if (anonimo) {
            // `new { apostolos }` — as propriedades do anônimo são os nomes das variáveis, e o tipo
            // de cada uma vem da assinatura (`List<ApostoloVisto> apostolos` → `ApostoloVisto`).
            const props = anonimo[1].split(',').map(s => s.trim()).filter(Boolean).map(nome => {
                const par = params.split(',').map(p => p.trim()).find(p => p.endsWith(' ' + nome));
                const interno = par && par.match(/<(.+)>\??\s+\w+$/);
                return { nome, tipo: interno ? interno[1] : '' };
            });
            mapa.set(tipo, { nome: tipo, props });
            continue;
        }
        const nomeVar = carga.trim();
        const par = params.split(',').map(p => p.trim().split(/\s+/)).find(p => p[p.length - 1] === nomeVar);
        const nomeRecord = par && par.slice(0, -1).join(' ').replace(/\?$/, '');
        const props = nomeRecord && propsDoRecord(dtos, nomeRecord);
        if (props) mapa.set(tipo, { nome: nomeRecord, props });
    }
    return mapa;
}

function conferirCargas() {
    const dtos = fs.readFileSync(DTOS, 'utf8');
    const contratos = contratosDaPonte();
    const extras = [];
    let conferidas = 0, faltando = 0, sobrando = 0;

    // Agrupa por RECORD, não por caminho: o mesmo `apostolo` aparece em cinco lugares da carga, e
    // sem isto uma propriedade faltando vira cinco queixas idênticas com endereço diferente.
    const vistos = new Set();

    function descer(caminho, valor, record, profundidade) {
        if (Array.isArray(valor)) {
            valor.forEach((v, i) => descer(`${caminho}[${i}]`, v, record, profundidade));
            return;
        }
        if (!valor || typeof valor !== 'object') return;
        const tem = new Set(Object.keys(valor));
        for (const { nome, tipo } of record.props) {
            if (!tem.has(nome)) {
                const chave = `${record.nome}.${nome}`;
                if (!vistos.has(chave)) {
                    vistos.add(chave);
                    faltando++;
                    queixar(`${record.nome}: a carga não manda \`${nome}\` (ex.: ${caminho}) — o caminho que a lê não roda`);
                }
                continue;
            }
            // Aninhado: só desce quando o tipo declarado é um record DESTE arquivo. Enum, int e
            // string param aqui, e é o que impede a recursão de virar passeio.
            const filhos = profundidade < 4 && tipo ? propsDoRecord(dtos, tipo) : null;
            if (filhos && filhos.length) {
                descer(`${caminho}.${nome}`, valor[nome], { nome: tipo, props: filhos }, profundidade + 1);
            }
        }
        const aMais = [...tem].filter(k => !record.props.some(p => p.nome === k));
        const chaveExtra = `${record.nome}:${aMais.join(',')}`;
        if (aMais.length && !vistos.has(chaveExtra)) {
            vistos.add(chaveExtra);
            sobrando += aMais.length;
            extras.push(`${record.nome} recebe ${aMais.length} chave(s) que o C# não manda: ${aMais.join(', ')}`);
        }
    }

    for (const [tipo, conteudo] of TELAS) {
        const contrato = contratos.get(tipo);
        if (!contrato || !contrato.props.length) continue;   // mensagem sem corpo (`criarPerfil`)
        conferidas++;
        descer(`"${tipo}"`, conteudo, contrato, 0);
    }
    return { conferidas, faltando, sobrando, extras };
}

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

    console.log(`\n  ${TELAS.length} telas · gestos disparados logo depois de cada uma:`);
    const total = { ok: 0, vazio: 0, erro: 0, mudos: [] };
    for (const [tipo, conteudo] of TELAS) {
        try {
            escritas = 0;
            ouvinte({ data: JSON.stringify({ tipo, conteudo }) });
            // Mensagem que não está NEM na tabela NEM no else-if não faz nada e não lança — e o
            // harness reportava isso como OK. "Não explodiu" ≠ "montou".
            if (escritas === 0) {
                console.log(`  ✗ ${tipo} — não fez NADA (não está ligada em lugar nenhum)`);
                queixar(`${tipo}: mensagem não tratada — nem na tabela TELAS nem no else-if`);
                continue;
            }
        } catch (e) {
            console.log(`  ✗ ${tipo} — ${e.message}`);
            queixar(`${tipo}: ${e.stack.split('\n').slice(0, 3).join(' | ')}`);
            continue;
        }
        // A varredura vem AQUI, e não no fim: os ouvintes que esta tela acabou de registrar rodam
        // contra o estado que ela montou. No fim de tudo eles rodariam contra a última tela.
        const conta = { ok: 0, vazio: 0, erro: 0, mudos: [], alcancaveis: comOuvinte().length };
        varrerGestos(tipo, conta);
        for (const k of ['ok', 'vazio', 'erro']) total[k] += conta[k];
        total.mudos.push(...conta.mudos.map(m => `${tipo} · ${m}`));
        const gestos = conta.ok + conta.vazio + conta.erro;
        // `alcançáveis` é o CENSO da tela: quantos elementos com ouvinte o jogador consegue tocar
        // com ela aberta. Tela rica em botão que mostra um número baixo aqui montou pela metade —
        // foi assim que a carga velha da campanha (mandava `liberada`, o código lê `desbloqueado`)
        // apareceu: a lista de fases nascia toda desabilitada e o detalhe nunca abria.
        console.log(`  ${conta.erro ? '✗' : '✓'} ${tipo}  ${conta.alcancaveis} alcançáveis`
            + (gestos ? ` · ${conta.ok} ✓${conta.vazio ? ` · ${conta.vazio} ∅` : ''}${conta.erro ? ` · ${conta.erro} ✗` : ''}` : ''));
    }
    console.log(`\n  gestos: ${total.ok + total.vazio + total.erro} disparados`
        + ` · ${total.ok} mexeram no DOM · ${total.vazio} não mexeram (∅) · ${total.erro} explodiram`);
    // O ∅ não é falha: handler guardado por estado que esta tela não tem sai calado, e isso é
    // legítimo. Vai listado porque ∅ em massa numa tela é sinal de que ela montou pela metade.
    if (total.mudos.length) {
        console.log('  ∅ (não tocaram no DOM): ' + total.mudos.slice(0, 12).join(', ')
            + (total.mudos.length > 12 ? ` … +${total.mudos.length - 12}` : ''));
    }

    // ---------- as cargas contra os DTOs do C# ----------
    // A carga é CONTRATO, e contrato só vale se alguém confere. Chave errada não explode: ela
    // desliga o caminho em silêncio e a tela passa verde pela metade. Já mordeu duas vezes — o
    // `taxaCrit` que a ficha lê como `taxaCritPct` (#254) e o `liberada` que o código lê como
    // `desbloqueado`, que deixou a campanha inteira sem UM gesto alcançável.
    //
    // O que DERRUBA é a propriedade do record que a carga NÃO manda: é ela que apaga um caminho.
    // Chave a mais é só ruído (o `apostolo` daqui serve a dois contratos de propósito), então vai
    // listada e não derruba.
    const props = conferirCargas();
    console.log(`\n  cargas: ${props.conferidas} conferidas contra o C# · ${props.faltando} propriedade(s) faltando`
        + `${props.sobrando ? ` · ${props.sobrando} chave(s) a mais` : ''}`);
    for (const l of props.extras) console.log('  · ' + l);

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
