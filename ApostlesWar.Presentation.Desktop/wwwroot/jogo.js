// Apostle's War — a tela.
//
// Contrato com o C#: recebemos ESTADO (retrato completo: como tudo está agora) e EVENTOS (o que
// acabou de acontecer). O estado redesenha; o evento anima. Essa separação é o que permite a tela
// ser burra — ela nunca calcula regra de jogo, só pinta o que chegou.
//
// Fluxo de clique (desenho do Gabriel): clica na habilidade -> clica no inimigo -> USA.
// Sem habilidade escolhida, clicar num personagem só INSPECIONA (mostra ficha e status).

'use strict';

const ponte = window.chrome.webview;

let estado = null;
let selecionadoId = null;    // quem está aberto no painel de baixo
let habilidadeEscolhida = null;
let mostrarNumeros = true;   // hoje ligado: fase de teste de balance

// ---------- envio ----------
const mandar = (tipo, valor = 0) => ponte.postMessage(JSON.stringify({ tipo, valor }));

// ---------- recepção ----------
ponte.addEventListener('message', e => {
    let msg;
    try { msg = typeof e.data === 'string' ? JSON.parse(e.data) : e.data; }
    catch { return; }

    if (msg.tipo === 'estado') aplicarEstado(msg.conteudo);
    else if (msg.tipo === 'evento') aplicarEvento(msg.conteudo);
});

function aplicarEstado(novo) {
    const fasesDeEscolha = ['EscolhendoAcao', 'EscolhendoAlvo', 1, 2];
    // Ao voltar pra escolha de ação, a habilidade anterior já foi usada (ou cancelada).
    if (nomeDaFase(novo) === 'EscolhendoAcao') habilidadeEscolhida = null;

    estado = novo;

    // Quem age vira o selecionado por padrão — poupa um clique a cada turno.
    if (novo.quemAge != null && nomeDaFase(novo) === 'EscolhendoAcao') selecionadoId = novo.quemAge;

    desenhar();
}

// A fase chega como número (enum serializado) ou string, dependendo do serializador.
const NOMES_FASE = ['Assistindo', 'EscolhendoAcao', 'EscolhendoAlvo', 'Fim'];
const nomeDaFase = e => typeof e.fase === 'number' ? NOMES_FASE[e.fase] : e.fase;

// ---------- desenho ----------
function desenhar() {
    if (!estado) return;

    document.getElementById('turno').textContent = `Turno ${estado.turno}`;

    // A mensagem do retrato entra no log (sem repetir a que já está lá). O fim de batalha ganha
    // destaque próprio.
    if (estado.mensagem) registrar(estado.mensagem, nomeDaFase(estado) === 'Fim' ? 'morte' : '');

    desenharLado('ladoEsquerdo', estado.equipe1);
    desenharLado('ladoDireito', estado.equipe2);
    desenharPainel();
}

// Classes de animação em curso — precisam SOBREVIVER a um redesenho (ver desenharLado).
const ANIMACOES = ['batendo', 'ferido', 'curado'];

// O redesenho REAPROVEITA as caixas existentes (casadas por id) em vez de recriá-las.
//
// Isso não é otimização, é CORREÇÃO: o C# publica o estado logo depois de mandar o evento de
// dano (ver TelaDeCombateWeb.ExibirResultadoAtaque). O `replaceChildren` antigo destruía a caixa
// milissegundos depois da animação começar — o tremor sumia e o número flutuante, que é filho
// dela, nunca chegava a aparecer. Mantendo o nó vivo, a animação roda até o fim.
function desenharLado(idElemento, combatentes) {
    const container = document.getElementById(idElemento);
    const existentes = new Map([...container.children].map(el => [el.dataset.id, el]));

    combatentes.forEach((c, i) => {
        const chave = String(c.id);
        let el = existentes.get(chave);
        if (el) existentes.delete(chave);
        else el = criarCombatente(c);

        atualizarCombatente(el, c);
        // Reposiciona só se a ordem mudou (mover um nó à toa reinicia animação em alguns motores).
        if (container.children[i] !== el) container.insertBefore(el, container.children[i] || null);
    });

    existentes.forEach(el => el.remove());   // sumiu da lista: fecha o caso, hoje não acontece
}

// A casca permanente: o que não muda entre quadros. O `.corpo` é o que será repintado — os
// números flutuantes são IRMÃOS dele, por isso não morrem no repintar.
function criarCombatente(c) {
    const el = document.createElement('div');
    el.dataset.id = c.id;

    const corpo = document.createElement('div');
    corpo.className = 'corpo';
    el.appendChild(corpo);

    // Busca pelo id na hora do clique: o objeto `c` deste quadro fica velho no quadro seguinte.
    el.addEventListener('click', () => clicarEmCombatente(c.id));
    return el;
}

function atualizarCombatente(el, c) {
    const escolhendoAlvo = nomeDaFase(estado) === 'EscolhendoAlvo';
    const ehAlvoValido = (estado.alvosValidos || []).includes(c.id);

    // Preserva as animações em curso ao reescrever as classes de estado.
    const animando = ANIMACOES.filter(k => el.classList.contains(k));
    el.className = 'combatente clicavel';
    if (!c.vivo) el.classList.add('morto');
    if (c.id === estado.quemAge) el.classList.add('agindo');
    if (c.id === selecionadoId) el.classList.add('selecionado');
    if (escolhendoAlvo && ehAlvoValido) el.classList.add('alvo');
    animando.forEach(k => el.classList.add(k));

    const emoji = document.createElement('div');
    emoji.className = 'emoji';
    emoji.textContent = c.simbolo;

    const infos = document.createElement('div');
    infos.className = 'infos';

    const nome = document.createElement('div');
    nome.className = 'nome';
    nome.textContent = c.nome;
    infos.appendChild(nome);

    infos.appendChild(criarBarra(c));

    // Números exatos são muleta de TESTE. Escondidos, sobra só a barra — que é como os jogos do
    // gênero fazem (o Gabriel citou o Raid): você lê a situação, não a planilha.
    if (mostrarNumeros) {
        const hp = document.createElement('div');
        hp.className = 'numeroHP';
        hp.textContent = `${c.hpAtual}/${c.hpMaximo}` + (c.escudo > 0 ? `  🛡️${c.escudo}` : '');
        infos.appendChild(hp);

        const stats = document.createElement('div');
        stats.className = 'statsLinha';
        stats.textContent = `ATK ${c.ataque} · DEF ${c.defesa} · 🎯${c.taxaCritPct}% · 💥${c.danoCritPct}%`;
        infos.appendChild(stats);
    }

    if (c.status.length) infos.appendChild(criarStatus(c.status));

    // Troca só o CORPO: os `.flutuante` são irmãos e seguem animando por cima.
    el.querySelector('.corpo').replaceChildren(emoji, infos);
}

function criarBarra(c) {
    const barra = document.createElement('div');
    barra.className = 'barra';

    const pctVida = c.hpMaximo > 0 ? Math.max(0, c.hpAtual / c.hpMaximo) * 100 : 0;

    const vida = document.createElement('div');
    vida.className = 'barraVida' + (pctVida <= 25 ? ' baixa' : '');
    vida.style.width = `${pctVida}%`;
    barra.appendChild(vida);

    if (c.escudo > 0) {
        // Escudo desenhado em cima, proporcional ao HP máximo (teto de 100%).
        const esc = document.createElement('div');
        esc.className = 'barraEscudo';
        esc.style.width = `${Math.min(100, (c.escudo / c.hpMaximo) * 100)}%`;
        barra.appendChild(esc);
    }
    return barra;
}

function criarStatus(status) {
    const caixa = document.createElement('div');
    caixa.className = 'status';
    for (const s of status) {
        const selo = document.createElement('span');
        selo.className = 'selo ' + (s.ehBuff ? 'buff' : 'debuff');
        selo.textContent = `${s.simbolo}${s.nome} ${s.duracaoRestante}`;
        selo.title = `${s.nome} — ${s.duracaoRestante} turno(s)`;
        caixa.appendChild(selo);
    }
    return caixa;
}

// ---------- painel de baixo ----------
function desenharPainel() {
    const vazio = document.getElementById('painelVazio');
    const conteudo = document.getElementById('painelConteudo');

    const c = acharCombatente(selecionadoId);
    if (!c) { vazio.hidden = false; conteudo.hidden = true; return; }

    vazio.hidden = true;
    conteudo.hidden = false;

    document.getElementById('retratoEmoji').textContent = c.simbolo;
    document.getElementById('retratoNome').textContent = c.nome;

    document.getElementById('painelStats').textContent = mostrarNumeros
        ? `HP ${c.hpAtual}/${c.hpMaximo}${c.escudo ? ` · 🛡️ ${c.escudo}` : ''} · ATK ${c.ataque} · DEF ${c.defesa} · 🎯 ${c.taxaCritPct}% · 💥 ${c.danoCritPct}%`
        : '';

    const caixaStatus = document.getElementById('painelStatus');
    caixaStatus.replaceChildren(...(c.status.length ? [criarStatus(c.status)] : []));

    desenharHabilidades(c);
}

function desenharHabilidades(c) {
    const caixa = document.getElementById('habilidades');

    // As habilidades só existem pra quem está agindo E sendo controlado por você.
    const podeAgir = c.id === estado.quemAge
        && ['EscolhendoAcao', 'EscolhendoAlvo'].includes(nomeDaFase(estado));

    if (!podeAgir) { caixa.replaceChildren(); return; }

    caixa.replaceChildren(...estado.habilidades.map(h => {
        const b = document.createElement('button');
        b.type = 'button';
        b.className = 'habilidade' + (habilidadeEscolhida === h.indice ? ' escolhida' : '');
        b.disabled = !h.disponivel;

        const nome = document.createElement('span');
        nome.className = 'hNome';
        nome.textContent = `${h.simbolo} ${h.nome}` + (h.cooldownRestante > 0 ? ` (${h.cooldownRestante})` : '');

        const desc = document.createElement('span');
        desc.className = 'hDesc';
        desc.textContent = h.descricao;

        b.append(nome, desc);
        b.addEventListener('click', () => escolherHabilidade(h));
        return b;
    }));
}

// ---------- interação ----------
function escolherHabilidade(h) {
    if (!h.disponivel) return;
    habilidadeEscolhida = h.indice;
    desenhar();                       // marca a habilidade como escolhida
    mandar('habilidade', h.indice);   // o C# decide se pede alvo
}

function clicarEmCombatente(id) {
    const escolhendoAlvo = nomeDaFase(estado) === 'EscolhendoAlvo';
    const ehAlvoValido = (estado.alvosValidos || []).includes(id);

    // Com habilidade em curso e alvo legítimo: o clique EXECUTA.
    if (escolhendoAlvo && ehAlvoValido) { mandar('alvo', id); return; }

    // Caso contrário, clicar é só olhar a ficha — inclusive a do inimigo, pra ver os status dele.
    selecionadoId = id;
    desenhar();
}

const acharCombatente = id => id == null ? null
    : [...(estado?.equipe1 || []), ...(estado?.equipe2 || [])].find(c => c.id === id) || null;

// Esc = desistir do alvo e voltar pra escolha de habilidade.
document.addEventListener('keydown', e => {
    if (e.key === 'Escape' && nomeDaFase(estado || {}) === 'EscolhendoAlvo') {
        habilidadeEscolhida = null;
        mandar('cancelar');
    }
});

document.getElementById('alternarNumeros').addEventListener('click', e => {
    mostrarNumeros = !mostrarNumeros;
    e.currentTarget.classList.toggle('ativo', mostrarNumeros);
    desenhar();
});

// ---------- velocidade ----------
// Só encurta a ESPERA entre eventos (o C# divide os 1500ms por este número); as animações em si
// mantêm a duração, senão o dano deixaria de ser visível — que era o ponto de tudo isto.
const VELOCIDADES = [1, 2, 4];
let velocidade = 2;   // começa acelerado: com o log persistente, ninguém precisa esperar pra ler

function aplicarVelocidade() {
    const b = document.getElementById('velocidade');
    b.textContent = `${'▶'.repeat(velocidade === 1 ? 1 : velocidade === 2 ? 2 : 3)} ${velocidade}x`;
    b.classList.toggle('rapido', velocidade > 1);
    mandar('velocidade', velocidade);
}

document.getElementById('velocidade').addEventListener('click', () => {
    velocidade = VELOCIDADES[(VELOCIDADES.indexOf(velocidade) + 1) % VELOCIDADES.length];
    aplicarVelocidade();
});

// ---------- log persistente ----------
// O log substitui a frase solta no meio da tela. Como tudo fica registrado e rolável, a espera
// entre eventos deixou de servir pra "dar tempo de ler" — por isso o botão de velocidade (>>)
// existe, e por isso ele começa em 2x.
let ultimaLinha = null;

// `permitirRepetir`: linhas de dano podem se repetir de verdade (dois golpes iguais seguidos);
// já a mensagem do retrato é eco da que veio no evento, e essa a gente descarta.
function registrar(texto, classe = '', permitirRepetir = false) {
    if (!texto || (!permitirRepetir && texto === ultimaLinha)) return;
    ultimaLinha = texto;

    const log = document.getElementById('log');
    log.querySelector('.linhaLog.atual')?.classList.remove('atual');

    const linha = document.createElement('div');
    linha.className = `linhaLog atual ${classe}`.trim();
    linha.textContent = texto;
    log.appendChild(linha);

    // Só acompanha o fim se o jogador já estava no fim — se ele subiu pra ler, não arrastamos.
    if (coladoNoFim) log.scrollTop = log.scrollHeight;
}

// Enquanto o jogador estiver lendo o histórico, o log não rouba a rolagem dele.
let coladoNoFim = true;
document.getElementById('log').addEventListener('scroll', e => {
    const el = e.currentTarget;
    coladoNoFim = el.scrollHeight - el.scrollTop - el.clientHeight < 24;
});

const nomeDe = id => acharCombatente(id)?.nome ?? '';

// ---------- eventos (animação) ----------
function aplicarEvento(ev) {
    if (ev.tipo === 'narracao') {
        registrar(ev.texto);
        return;
    }

    const el = document.querySelector(`.combatente[data-id="${ev.alvoId}"]`);
    if (!el) return;

    const nome = nomeDe(ev.alvoId);

    if (ev.tipo === 'dano') {
        // Golpe todo aparado pelo escudo: mostra o escudo segurando, não um "0" seco.
        if (ev.valor <= 0 && ev.absorvidoPeloEscudo > 0) {
            flutuar(el, `🛡️ ${ev.absorvidoPeloEscudo}`, 'escudo');
            registrar(`🛡️ ${nome} aparou o golpe (${ev.absorvidoPeloEscudo}).`, '', true);
        } else {
            reanimar(el, 'batendo');
            reanimar(el, 'ferido');
            flutuar(el, `-${ev.valor}`, ev.critico ? 'dano critico' : 'dano');
            if (ev.absorvidoPeloEscudo > 0) flutuar(el, `🛡️ ${ev.absorvidoPeloEscudo}`, 'escudo');

            const escudo = ev.absorvidoPeloEscudo > 0 ? ` (🛡️ ${ev.absorvidoPeloEscudo})` : '';
            registrar(
                ev.critico ? `💥 CRÍTICO! ${nome} levou ${ev.valor}${escudo}.`
                           : `${nome} levou ${ev.valor} de dano${escudo}.`,
                ev.critico ? 'critico' : '', true);
        }
    } else if (ev.tipo === 'cura') {
        reanimar(el, 'curado');
        flutuar(el, `+${ev.valor}`, 'cura');
        registrar(`💚 ${nome} recuperou ${ev.valor}.`, 'cura', true);
    } else if (ev.tipo === 'morte') {
        flutuar(el, '💀', 'dano');
        registrar(`💀 ${nome} caiu!`, 'morte', true);
    }
}

// Reinicia a animação mesmo se a classe já estiver lá (dois golpes seguidos no mesmo alvo).
function reanimar(el, classe) {
    el.classList.remove(classe);
    void el.offsetWidth;
    el.classList.add(classe);
    setTimeout(() => el.classList.remove(classe), 520);   // acompanha o .5s do @keyframes tremer
}

// O número é CUSPIDO pra fora da caixa, na direção do seu próprio lado — a direita é o espelho
// da esquerda. O mesmo champ pode lutar dos dois lados, então a animação não pode ter "lado
// certo": ela deriva de onde a caixa está, não de quem é o personagem.
function flutuar(el, texto, classe) {
    const n = document.createElement('span');
    n.className = `flutuante ${classe}`;
    n.textContent = texto;

    const sentido = el.parentElement?.id === 'ladoDireito' ? 1 : -1;
    // Embaralha distância e giro pra dois golpes seguidos não empilharem no mesmo ponto.
    n.style.setProperty('--dx', `${sentido * (34 + Math.random() * 30)}px`);
    n.style.setProperty('--giro', `${sentido * (4 + Math.random() * 8)}deg`);

    el.appendChild(n);
    setTimeout(() => n.remove(), 1250);
}

// ---------- partida ----------
document.getElementById('alternarNumeros').classList.toggle('ativo', mostrarNumeros);
aplicarVelocidade();   // sincroniza o C# com o 2x inicial
mandar('pronto');      // destrava a thread do jogo no C#
