// FORJA — a oficina da PEÇA, e a irmã da Catedral: lá o centro é o apóstolo e a moeda é alma, aqui
// o centro é a peça e a moeda é pó. Quatro colunas: o acervo do slot, a peça, as bancadas e a
// bancada aberta.
//
// As bancadas são as etapas de forjar uma lâmina, e cada uma é um eixo do item:
//   ⚒️ Bigorna     malha pó em pontos de nível — golpe a golpe, é a que mais se usa
//   💧 Têmpera     o mergulho que fixa a dureza: paga a estrela e abre a dezena seguinte
//   🏺 Caldeamento caldear é unir metais aquecidos num só — 10 de uma faixa viram 1 da seguinte
//   ⚙️ Esmeril     o rebolo que desfaz a peça em limalha: ela vira pó e deixa de existir
//   ⚗️ Amálgama    (em breve) várias peças viram uma de raridade acima, e as subs delas viram o pool
//
// As duas máquinas da oficina são a Bigorna e o Esmeril; as duas de fogo são a Têmpera e o
// Caldeamento. O Esmeril é a ÚNICA que destrói, e por isso a única com confirmação.
//
// Entra-se pela Catedral (o ⚒️ Melhorar de um slot) e o Esc volta pra lá. Não há estado de jogo
// aqui: o que mora neste arquivo é só qual bancada está aberta e o que está sendo montado nela.

import { mandar } from '../nucleo/ponte.js';
import { marcaDeEstrelas, barraDeNivel } from '../ui/marcas.js';
import { poIcone } from '../ui/po.js';
import { barraDeQuantidade } from '../ui/quantidade.js';
import { blocoDeDelta } from '../ui/delta.js';
import { cardDePeca } from '../ui/peca.js';
import { navegador } from '../ui/navegador.js';
import { filtroLimpo, aplicar, agrupar, painelDeFiltro } from '../ui/filtro.js';
import { confirmar } from '../ui/modal.js';
import { saldoDeAlma, saldoDePo } from '../ui/saldo.js';

let dados = null;
let bancada = 'nivel';   // 'nivel' = Bigorna · 'estrela' = Têmpera · 'fundir' = Caldeamento

// O MESMO filtro do acervo da Catedral (ui/filtro.js) — com o acervo passando de mil peças, achar a
// que vai pra bigorna é problema de BUSCA aqui também.
//
// SEM o eixo "Vestidas", e com ele ligado de saída: na Catedral esconder as peças de aliado protege
// de um roubo sem querer, mas aqui toda peça do slot é forjável — e a primeira que o filtro
// esconderia seria justamente a peça vestida por onde se ENTROU na tela.
let filtro = filtroLimpo({ deAliados: true });

// O que está sendo MONTADO na bancada, lido pela peça do meio: as duas colunas são partes do mesmo
// gesto — a direita é o quanto, o meio é o que isso vira.
let previsao = null;   // { nivel, pct, ganho }

export const forja = {
    cena: 'forja',
    montar(f, anterior) {
        // A Bigorna é o repouso: é a bancada que se usa toda hora, e a têmpera só uma vez por dezena.
        if (anterior !== 'forja') bancada = 'nivel';
        dados = f;

        // O mesmo par de saldos da Catedral, nas mesmas pontas: as duas telas mostram o mesmo par de
        // abas, e trocar de tela não pode trocar o saldo de lugar.
        saldoDeAlma('forjaSaldoAlma', f.alma);
        saldoDePo('forjaSaldoPo', f.po);
        previsao = null;   // toda volta da ponte apaga o que estava sendo montado
        desenharAcervo();
        desenharPeca();
        desenharPortas();
        desenharBancada();
    },
};

// A aba do título. Ela NÃO é o "voltar" do Esc: entrando pelo menu, voltar devolve ao menu, e um
// botão escrito "Catedral" que leva pro menu é rótulo mentindo. O C# distingue os dois.
document.getElementById('forjaIrCatedral').addEventListener('click', () => mandar('irParaCatedral', 0));

// ---------- esquerda: as peças DAQUELE slot ----------
//
// Trocar quem está na bigorna não equipa nada: melhorar uma peça guardada é legítimo, e é aqui que
// isso acontece. Só o slot da peça aberta — a bigorna não é um segundo lugar de escolher arma.
function desenharAcervo() {
    const col = document.getElementById('forjaAcervo');
    const itens = aplicar(dados.acervo, filtro);

    const topo = document.createElement('div');
    topo.className = 'acervoTopo';
    const titulo = document.createElement('span');
    titulo.className = 'acervoTitulo';
    // Com filtro ligado, o número é o que PASSOU dele — dizer o total escondendo metade da lista
    // faria o jogador procurar uma peça que a tela sabe estar filtrada.
    titulo.textContent = `${dados.slotNome} · ${itens.length}`;
    topo.append(titulo);

    // O MESMO cartão e a MESMA grade do acervo da Catedral (ui/peca.js): trocar de tela não pode
    // trocar o jeito de a peça se parecer. O que muda é só o que a marca quer dizer — lá é a peça
    // comparada, aqui é a que está na bigorna — e o que o clique manda.
    const carta = (o) => {
        const card = cardDePeca(o, {
            marcada: o.indice === dados.peca.indice,
            aoClicar: () => mandar('escolherPeca', o.indice),
        });
        if (o.equipado) card.classList.add('vestida');   // a que está mexendo em ficha agora
        return card;
    };

    const lista = document.createElement('div');
    lista.className = 'acervoLista';

    if (!itens.length) {
        const v = document.createElement('div');
        v.className = 'catedralVazio';
        v.textContent = 'Nenhuma peça passa neste filtro.';
        lista.append(v);
    } else {
        for (const g of agrupar(itens, filtro)) {
            if (g.cabecalho) {
                const cab = document.createElement('div');
                cab.className = 'acervoGrupo';
                cab.textContent = g.cabecalho;
                lista.append(cab);
            }
            lista.append(...g.itens.map(carta));
        }
    }

    col.replaceChildren(topo, painelDeFiltro(dados.acervo, filtro, desenharAcervo, { comVestidas: false }), lista);
}

// ---------- centro: a peça, e o que a bancada faria com ela ----------

function desenharPeca() {
    const alvo = document.getElementById('forjaPeca');
    const p = dados.peca;

    const arte = document.createElement('div'); arte.id = 'forjaArte'; arte.textContent = p.simbolo;
    const nome = document.createElement('div'); nome.className = 'afNome'; nome.textContent = p.nome;

    const ident = document.createElement('div');
    ident.className = 'afFaccao';
    ident.textContent = `${p.faccaoSimbolo} ${p.faccao}`;

    // O TIPO com as setas, e ele fica no rótulo do slot de propósito — ver ui/navegador.js. Sem
    // isto, forjar o elmo depois da arma custava sair da Forja, clicar no slot e voltar.
    const tipo = navegador(dados.slotNome, {
        ha: dados.slotsComPeca > 1,
        aoAnterior: () => mandar('trocarSlot', -1),
        aoProximo: () => mandar('trocarSlot', 1),
        classe: 'navSlot',
    });

    // ATRAVESSANDO NÍVEL A BARRA ZERA: ela passa a ser a barra do nível de DESTINO, e o que já
    // estava preenchido era do nível anterior. Quem conta a travessia é o rótulo.
    const virou = previsao && previsao.nivel > p.nivel;

    const nivel = document.createElement('div');
    nivel.className = 'afNivel';
    const numero = document.createElement('span');
    numero.className = 'afNivelNumero' + (dados.naParede ? ' travado' : '') + (previsao ? ' previsto' : '');
    numero.textContent = virou
        ? `nv ${p.nivel} → ${previsao.nivel} / ${dados.teto}`
        : `nv ${p.nivel} / ${dados.teto}`;
    nivel.append(marcaDeEstrelas(p.estrelas),
        barraDeNivel(virou ? previsao.nivel : p.nivel, virou ? 0 : p.pct, previsao ? previsao.pct : -1),
        numero);

    const filhos = [arte, nome, ident, tipo, nivel];

    if (dados.motivo) {
        const m = document.createElement('div'); m.className = 'afMotivo'; m.textContent = dados.motivo;
        filhos.push(m);
    }

    // O QUE A PEÇA DÁ, que é o número pelo qual ela existe. Montando uma malhada, vira `de → para`.
    const destino = virou ? dados.porNivel.find(n => n.nivel === previsao.nivel) : null;
    const stat = document.createElement('div');
    stat.className = 'forjaStat' + (destino ? ' subindo' : '');
    const rot = document.createElement('span'); rot.className = 'fsStatRotulo'; rot.textContent = p.stat;
    const val = document.createElement('span'); val.className = 'fsStatValor';
    val.textContent = destino ? `${p.valor} → ${destino.valor}` : p.valor;
    stat.append(rot, val);
    filhos.push(stat);

    // O REFLEXO no apóstolo que a veste. Ele é a resposta à pergunta que se faz de verdade — "vale
    // gastar o pó nisto?" —, e some quando a peça está no baú: sem portador não há ficha pra mexer.
    if (dados.portadorNome) {
        const quem = document.createElement('div');
        quem.className = 'acaoRotulo';
        // O emoji junto do nome: é o MESMO sinal que marca a peça no acervo, e ver os dois iguais é
        // o que liga "esta é a peça do Ninja" lá com "estou forjando a peça do Ninja" aqui.
        quem.textContent = `em ${dados.peca.portadorSimbolo} ${dados.portadorNome}`.trim();
        filhos.push(quem, blocoDeDelta(destino ? destino.noApostolo : [],
            'Arraste o pó pra ver o que muda na ficha dele.'));
    } else {
        const guardada = document.createElement('div');
        guardada.className = 'afMotivo';
        guardada.textContent = 'Peça guardada — equipe-a na Armaria pra ver o efeito numa ficha.';
        filhos.push(guardada);
    }

    alvo.replaceChildren(...filhos);
}

// ---------- as BANCADAS: as portas da 3ª coluna ----------

function desenharPortas() {
    document.getElementById('forjaPortas').replaceChildren(
        botao('⚒️', 'Bigorna', 'nivel', dados.podeQueimar),
        botao('💧', 'Têmpera', 'estrela', dados.naParede),
        botao('🏺', 'Caldeamento', 'fundir', true),
        botao('⚙️', 'Esmeril', 'esmeril', dados.podeEsmerilhar),
        botao('⚗️', 'Amálgama', null, false, 'em breve'));
}

// `acao` null = a bancada que ainda não existe. `habilitado` false NÃO desabilita: a bancada é onde
// o jogador descobre o que falta, e botão morto não ensina nada — só o ♻️ é de fato inerte.
function botao(icone, rotulo, acao, habilitado, nota) {
    const b = document.createElement('button');
    b.type = 'button';
    b.className = 'afBotao' + (bancada === acao ? ' aberto' : '');
    b.disabled = acao === null;

    const ic = document.createElement('span'); ic.className = 'abIcone'; ic.textContent = icone;
    const rot = document.createElement('span'); rot.className = 'abRotulo'; rot.textContent = rotulo;
    b.append(ic, rot);

    if (nota) {
        const n = document.createElement('span'); n.className = 'abNota'; n.textContent = nota;
        b.append(n);
    }
    if (!habilitado && !b.disabled) b.classList.add('semSaldo');

    if (!b.disabled) b.addEventListener('click', () => {
        bancada = acao;
        previsao = null;
        desenharPortas(); desenharPeca(); desenharBancada();
    });
    return b;
}

function desenharBancada() {
    const painel = document.getElementById('forjaBancada');
    const nomes = { nivel: '⚒️ Bigorna', estrela: '💧 Têmpera', fundir: '🏺 Caldeamento', esmeril: '⚙️ Esmeril' };

    const titulo = document.createElement('h2');
    titulo.className = 'apostoloSecao';
    titulo.textContent = nomes[bancada];

    const corpo = bancada === 'estrela' ? painelDaTempera()
        : bancada === 'fundir' ? painelDoCaldeamento()
            : bancada === 'esmeril' ? painelDoEsmeril()
                : painelDaBigorna();

    painel.replaceChildren(titulo, ...corpo);
}

// A BIGORNA: pó vira ponto de nível, uma barra por faixa. O `max` de cada uma é quanto dela cabe
// ANTES da parede — quem diz é o C#, e é o que impede torrar pó mítico num nível não destravado.
function painelDaBigorna() {
    if (!dados.podeQueimar) return [aviso(dados.motivo || 'Não há nível a subir agora.')];

    const escolha = dados.po.map(() => 0);
    const barras = [];

    const resumo = document.createElement('div'); resumo.className = 'queimaResumo';
    const confirmar = document.createElement('button');
    confirmar.type = 'button';
    confirmar.className = 'acaoConfirmar';

    const linhas = dados.po.map((p, raridade) => {
        const barra = barraDeQuantidade({
            max: p.max,
            rotuloMax: p.max < p.quantidade
                ? `você tem ${p.quantidade}, mas só ${p.max} cabem até a parede`
                : 'tudo o que você tem desta faixa',
            aoMudar: (v) => { escolha[raridade] = v; atualizar(); },
        });
        barras.push(barra);
        return linhaDeFaixa(p, `+${p.pontosPorUnidade} pontos cada`, barra.el);
    });

    // MÁXIMO enche até a parede gastando da faixa MAIS BARATA pra mais cara: se o comum resolve, ele
    // não encosta no raro — que é justamente a faixa que a têmpera vai cobrar.
    const max = document.createElement('button');
    max.type = 'button';
    max.className = 'qlBotao qlMaximo';
    max.textContent = 'Máximo';
    max.addEventListener('click', () => {
        let falta = dados.pontosAteAParede;
        dados.po.forEach((p, i) => {
            const q = falta <= 0 ? 0 : Math.min(p.max, Math.ceil(falta / p.pontosPorUnidade));
            escolha[i] = q;
            falta -= q * p.pontosPorUnidade;
            barras[i].definir(q);
        });
        atualizar();
    });

    function atualizar() {
        const ganho = escolha.reduce((s, q, i) => s + q * dados.po[i].pontosPorUnidade, 0);
        resumo.textContent = ganho > 0
            ? `+${ganho.toLocaleString('pt-BR')} pontos`
            : 'Arraste uma barra pra escolher quanto pó malhar.';
        confirmar.textContent = ganho > 0 ? `Malhar (+${ganho.toLocaleString('pt-BR')})` : 'Malhar';
        confirmar.disabled = ganho <= 0;

        // O RESULTADO é desenhado no MEIO, na peça: o nível de destino, a barra em brasa e o
        // `57,5 → 71,3` na linha do stat. Aqui à direita fica só quanto se está gastando.
        previsao = ganho > 0 ? projetar(ganho) : null;
        desenharPeca();
    }

    confirmar.addEventListener('click', () => {
        const faixas = escolha
            .map((quantidade, raridade) => ({ raridade, quantidade }))
            .filter(f => f.quantidade > 0);
        if (faixas.length) mandar('queimarPo', 0, JSON.stringify({ faixas }));
    });

    atualizar();
    return [rotulo('Malhar pó em nível'), ...linhas, max, resumo, confirmar];
}

// A TÊMPERA: o pedágio da dezena. Só na parede, e só com o preço inteiro no bolso.
function painelDaTempera() {
    const partes = [];
    const p = dados.peca;

    partes.push(rotulo(`★ ${p.estrelas + 1} — encha a barra no nv ${dados.teto}`));

    const onde = document.createElement('div');
    onde.className = 'acaoAviso';
    onde.textContent = dados.naParede
        ? `O nv ${dados.teto} está cheio. A têmpera abre até o nv ${Math.min(dados.teto + 10, 60)}.`
        : `Ela está no nv ${p.nivel}. Malhe até o nv ${dados.teto} pra poder temperar.`;
    partes.push(onde);

    // UMA linha por faixa, no formato `tenho/preciso`: duas listas (o preço e o que falta) diziam a
    // mesma coisa com números diferentes, e o jogador tinha de subtrair de cabeça.
    const receita = document.createElement('div');
    receita.className = 'acaoReceita';
    receita.replaceChildren(...dados.receita.map(r => {
        const tenho = (dados.po[r.raridade] || {}).quantidade ?? 0;
        const linha = linhaDePo(r, `${Math.min(tenho, r.quantidade).toLocaleString('pt-BR')}/${r.quantidade.toLocaleString('pt-BR')}`);
        if (tenho < r.quantidade) linha.classList.add('faltando');
        return linha;
    }));
    partes.push(rotulo('Custo'), receita);

    const b = document.createElement('button');
    b.type = 'button';
    b.className = 'acaoConfirmar';
    b.textContent = `Temperar (★ ${p.estrelas + 1})`;
    b.disabled = !dados.podeComprarEstrela;
    b.addEventListener('click', () => mandar('comprarEstrelaItem', 0));
    partes.push(b);

    return partes;
}

// O CALDEAMENTO: 10 de uma faixa viram 1 da seguinte. A barra conta GRUPOS de 10, não unidades — é
// o grupo que produz alguma coisa, e arrastar em unidades deixaria o jogador parar num resto.
// O ESMERIL: a peça vira pó e DEIXA DE EXISTIR. É a única bancada que destrói, e por isso é a única
// com confirmação — as outras três gastam material, esta gasta a peça.
//
// Peça vestida não entra no esmeril: a bancada diz de quem tirar primeiro, em vez de desnudar o
// apóstolo por conta própria. Quem tira é o ✕ Remover da Armaria.
function painelDoEsmeril() {
    const p = dados.peca;
    const partes = [rotulo('Moer a peça em pó')];

    if (!dados.podeEsmerilhar) {
        partes.push(aviso(`Está vestida em ${dados.portadorNome}. Remova na Armaria pra poder moer.`));
        return partes;
    }

    partes.push(aviso(`${p.simbolo} ${p.nome} nv ${p.nivel} deixa de existir. O nível dela não volta.`));

    const ganho = document.createElement('div');
    ganho.className = 'acaoReceita';
    ganho.append(linhaDePo(dados.esmeril, `+${dados.esmeril.quantidade.toLocaleString('pt-BR')}`));
    partes.push(rotulo('Devolve'), ganho);

    const b = document.createElement('button');
    b.type = 'button';
    b.className = 'acaoConfirmar';
    b.textContent = 'Esmerilhar';
    b.addEventListener('click', () => confirmar(
        `Moer ${p.nome} nv ${p.nivel}? A peça deixa de existir.`,
        () => mandar('esmerilharPeca', 0)));
    partes.push(b);

    return partes;
}

function painelDoCaldeamento() {
    const escolha = dados.po.map(() => 0);

    const resumo = document.createElement('div'); resumo.className = 'queimaResumo';
    const confirmar = document.createElement('button');
    confirmar.type = 'button';
    confirmar.className = 'acaoConfirmar';

    const linhas = dados.po.map((p, raridade) => {
        const grupos = p.podeFundir ? Math.floor(p.quantidade / 10) : 0;
        const seguinte = dados.po[raridade + 1];

        const barra = barraDeQuantidade({
            max: grupos,
            rotuloMax: p.raridade >= dados.tetoDeFusao
                ? 'vença uma dificuldade mais alta pra caldear até aqui'
                : `cada grupo consome 10 de ${p.nome}`,
            aoMudar: (v) => { escolha[raridade] = v; atualizar(); },
            // As PONTAS contam a troca: à esquerda o que SAI, à direita o que NASCE. A ponta
            // esquerda não repete o pozinho da linha de cima — dois iguais empilhados liam como
            // duas coisas; a direita mantém, porque ali a faixa é OUTRA.
            pontas: seguinte ? {
                esquerda: (v) => soNumero(v * 10),
                direita: (v) => poContado(seguinte, v),
            } : null,
        });

        const nota = p.raridade >= dados.tetoDeFusao ? '🔒 travado'
            : seguinte ? `10 → 1 ${seguinte.nome}` : 'não sobe mais';
        return linhaDeFaixa(p, nota, barra.el);
    });

    function atualizar() {
        const total = escolha.reduce((s, g) => s + g, 0);
        resumo.textContent = total > 0
            ? `${escolha.reduce((s, g) => s + g * 10, 0)} de pó viram ${total}`
            : 'Arraste uma barra pra escolher quantos grupos caldear.';
        confirmar.textContent = total > 0 ? `Caldear (${total})` : 'Caldear';
        confirmar.disabled = total <= 0;
    }

    confirmar.addEventListener('click', () => {
        const faixas = escolha
            .map((grupos, raridade) => ({ raridade, quantidade: grupos * 10 }))
            .filter(f => f.quantidade > 0);
        if (faixas.length) mandar('fundirPo', 0, JSON.stringify({ faixas }));
    });

    const nota = document.createElement('div');
    nota.className = 'acaoAviso';
    nota.textContent = `O caldeamento para na faixa ${dados.po[dados.tetoDeFusao].nome}: `
        + 'é até onde a dificuldade mais alta que você abriu derruba pó.';

    atualizar();
    return [rotulo('Caldear — 10 viram 1'), ...linhas, resumo, confirmar, nota];
}

// ---------- as peças pequenas ----------

// Onde este ganho para: o nível de destino e o quanto a barra DAQUELE nível encheria. Os dois saem
// da tabela de patamares que o C# manda — é busca numa lista, não a curva do pó copiada pra cá.
function projetar(ganho) {
    const total = dados.pontos + ganho;

    let nivel = dados.peca.nivel;
    for (const p of dados.patamares) if (total >= p.pontos && p.nivel <= dados.teto) nivel = p.nivel;

    const pontosDe = (n) => (dados.patamares.find(p => p.nivel === n) || {}).pontos;
    const piso = pontosDe(nivel);
    const teto = pontosDe(nivel + 1);
    if (piso == null || teto == null || teto <= piso) return { nivel, pct: 100, ganho };

    return { nivel, pct: Math.max(0, Math.min(100, ((total - piso) / (teto - piso)) * 100)), ganho };
}

// A linha de UMA faixa: o pozinho, o nome, o saldo, uma nota curta e a barra embaixo. As três
// bancadas usam a mesma forma de propósito — é o mesmo gesto.
function linhaDeFaixa(p, nota, barraEl) {
    const linha = document.createElement('div');
    linha.className = 'faixaLinha' + (p.quantidade > 0 ? '' : ' vazia');

    const topo = document.createElement('div'); topo.className = 'flTopo';
    const nome = document.createElement('span'); nome.className = 'flNome'; nome.textContent = p.nome;
    const qtd = document.createElement('span'); qtd.className = 'flQtd';
    qtd.textContent = p.quantidade.toLocaleString('pt-BR');
    const obs = document.createElement('span'); obs.className = 'flNota'; obs.textContent = nota;

    topo.append(poIcone(p.raridade, 22), nome, qtd, obs);
    linha.append(topo, barraEl);
    return linha;
}

function linhaDePo(p, valor) {
    const linha = document.createElement('div');
    linha.className = 'almaLinha';
    const n = document.createElement('span'); n.className = 'alNome'; n.textContent = p.nome;
    const v = document.createElement('span'); v.className = 'alValor'; v.textContent = valor;
    linha.append(poIcone(p.raridade), n, v);
    return linha;
}

// Quanto SAI: só o número, porque o pozinho dessa faixa já está na linha de cima.
function soNumero(quantidade) {
    const n = document.createElement('span');
    n.className = 'fcQtd';
    n.textContent = `−${Math.max(0, quantidade).toLocaleString('pt-BR')}`;
    return n;
}

// O pozinho com o número embaixo — é a ponta direita da barra do caldeamento.
function poContado(p, quantidade) {
    const cont = document.createElement('span');
    cont.className = 'fogoContado';
    const n = document.createElement('span');
    n.className = 'fcQtd';
    n.textContent = `+${Math.max(0, quantidade).toLocaleString('pt-BR')}`;
    cont.append(poIcone(p.raridade, 20), n);
    return cont;
}

function rotulo(texto) {
    const d = document.createElement('div');
    d.className = 'acaoRotulo';
    d.textContent = texto;
    return d;
}

function aviso(texto) {
    const d = document.createElement('div');
    d.className = 'acaoAviso';
    d.textContent = texto;
    return d;
}
