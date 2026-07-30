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
let mostrarEstatisticas = true;   // hoje ligado: fase de teste de balance
let cenaAtual = 'menu';      // cena atual (menu, combate, criarPerfil, arenaSetup, campanha*) — o Esc depende disto
let menuRaiz = true;         // o menu na tela é o PRINCIPAL? (decide o Esc: sair do jogo × voltar)
let perfilAvatar = '🧭';     // avatar do jogador; vira o marcador que caminha no mapa

// ---------- envio ----------
// `texto` só é usado quando o valor é uma string (ex: o nome do perfil); o resto manda só o índice.
const mandar = (tipo, valor = 0, texto = null) => ponte.postMessage(JSON.stringify({ tipo, valor, texto }));

// ---------- recepção ----------
ponte.addEventListener('message', e => {
    let msg;
    try { msg = typeof e.data === 'string' ? JSON.parse(e.data) : e.data; }
    catch { return; }

    if (msg.tipo === 'estado') aplicarEstado(msg.conteudo);
    else if (msg.tipo === 'evento') aplicarEvento(msg.conteudo);
    else if (msg.tipo === 'menu') aplicarMenu(msg.conteudo);
    else if (msg.tipo === 'criarPerfil') mostrarCriarPerfil();
    else if (msg.tipo === 'edicaoPerfil') mostrarEditarPerfil(msg.conteudo);
    else if (msg.tipo === 'montagemArena') mostrarMontagemArena(msg.conteudo.campeoes);
    else if (msg.tipo === 'campanhaMapa') mostrarMapa(msg.conteudo);
    else if (msg.tipo === 'campanhaFases') mostrarFasesCampanha(msg.conteudo);
    else if (msg.tipo === 'fimDeFase') mostrarFimDeFase(msg.conteudo);
    else if (msg.tipo === 'conquista') mostrarConquista(msg.conteudo);
    else if (msg.tipo === 'arsenal') mostrarArsenal(msg.conteudo);
    else if (msg.tipo === 'compendio') mostrarCompendio(msg.conteudo);
    else if (msg.tipo === 'compendioChamp') mostrarChampDetalhe(msg.conteudo);
});

// ---------- cenas (menu × combate × criar/editar perfil) ----------
function mostrarCena(cena) {
    cenaAtual = cena;
    const emCombate = cena === 'combate';
    document.getElementById('menu').hidden = cena !== 'menu';
    document.getElementById('criarPerfil').hidden = cena !== 'criarPerfil';
    document.getElementById('editarPerfil').hidden = cena !== 'editarPerfil';
    document.getElementById('arenaSetup').hidden = cena !== 'arenaSetup';
    document.getElementById('campanhaMapa').hidden = cena !== 'campanhaMapa';
    document.getElementById('campanhaFases').hidden = cena !== 'campanhaFases';
    document.getElementById('fimDeFase').hidden = cena !== 'fimDeFase';
    document.getElementById('conquista').hidden = cena !== 'conquista';
    document.getElementById('arsenal').hidden = cena !== 'arsenal';
    document.getElementById('compendio').hidden = cena !== 'compendio';
    // A ficha do champ tem UMA seção e dois donos: o compêndio e a conquista (o champ recém-ganho
    // termina na própria ficha). Copiar o HTML pra ter duas telas iguais seria duas telas pra manter.
    document.getElementById('compendioChamp').hidden = !['compendioChamp', 'conquistaChamp'].includes(cena);
    document.getElementById('arena').hidden = !emCombate;
    document.getElementById('painel').hidden = !emCombate;
    // Os controles de combate só fazem sentido na batalha.
    document.getElementById('botoesTopo').style.visibility = emCombate ? 'visible' : 'hidden';
    document.getElementById('turno').style.visibility = emCombate ? 'visible' : 'hidden';
    // O overlay de fim só existe em combate; ao trocar de cena garante que sumiu.
    if (!emCombate) document.getElementById('fimBatalha').hidden = true;
    // O tema é do CAMPO DE BATALHA: fora dele, nenhum. Quem o LIGA é o desenhar (precisa do estado
    // novo); aqui só se garante que ele não vaze pras telas de menu.
    if (!emCombate) aplicarTema('');
    atualizarBotaoSair();
}

function aplicarMenu(m) {
    mostrarCena('menu');
    menuRaiz = !!m.raiz;
    document.getElementById('menuTitulo').textContent = m.titulo;
    document.getElementById('menuSubtitulo').textContent = m.subtitulo || '';

    // Perfil no canto (avatar + nome): só o menu principal manda. Editar é pelo botão ✏️, não pelo avatar.
    const perfil = document.getElementById('menuPerfil');
    perfil.hidden = !m.avatar;
    if (m.avatar) {
        perfilAvatar = m.avatar;
        document.getElementById('menuAvatar').textContent = m.avatar;
        document.getElementById('menuNome').textContent = m.nome || '';
    }

    const cont = document.getElementById('menuOpcoes');
    cont.replaceChildren(...m.opcoes.map((o, i) => {
        const b = document.createElement('button');
        b.type = 'button';
        b.className = 'opcaoMenu';
        b.disabled = !o.habilitado;

        const ic = document.createElement('span'); ic.className = 'opIcone'; ic.textContent = o.icone;
        const rot = document.createElement('span'); rot.className = 'opRotulo'; rot.textContent = o.rotulo;
        b.append(ic, rot);
        if (!o.habilitado) {
            const em = document.createElement('span'); em.className = 'opEmBreve'; em.textContent = 'em breve';
            b.appendChild(em);
        }
        // Opção-INTERRUPTOR (marcado != null): mostra em que estado ela está. O C# manda o estado e
        // redesenha o menu depois de alternar — a tela não guarda nada, como o botão do auto.
        if (o.marcado != null) {
            const marca = document.createElement('span');
            marca.className = 'opMarca' + (o.marcado ? ' ligado' : '');
            marca.textContent = o.marcado ? '✓' : '—';
            b.appendChild(marca);
        }

        // Opção destrutiva (ex: excluir conta): confirma no modal ANTES de mandar a escolha.
        b.addEventListener('click', () => o.confirmar
            ? confirmar(o.confirmar, () => mandar('menuEscolha', i))
            : mandar('menuEscolha', i));
        return b;
    }));
}

// ---------- criar perfil (1ª vez) ----------
function mostrarCriarPerfil() {
    mostrarCena('criarPerfil');
    const input = document.getElementById('nomePerfil');
    input.value = '';
    input.focus();
}

function enviarPerfil() {
    const nome = document.getElementById('nomePerfil').value.trim();
    if (!nome) return;   // sem nome, não começa
    mandar('criarPerfil', 0, nome);
}

document.getElementById('confirmarPerfil').addEventListener('click', enviarPerfil);
document.getElementById('nomePerfil').addEventListener('keydown', e => {
    if (e.key === 'Enter') enviarPerfil();
});

// ---------- editar perfil (nome + avatar) ----------
// A entidade avatar+nome no menu é o gatilho inteiro.
document.getElementById('menuPerfil').addEventListener('click', () => mandar('editarPerfil'));

let avatarSelecionado = -1;   // índice na grade (= índice na lista completa que o C# mandou)

function mostrarEditarPerfil(dados) {
    mostrarCena('editarPerfil');
    // O nome começa TRAVADO (display); só o botão ✏️ destrava pra editar.
    const nomeInput = document.getElementById('nomeEditar');
    nomeInput.value = dados.nome || '';
    nomeInput.readOnly = true;

    const grade = document.getElementById('avatarGrade');
    avatarSelecionado = -1;

    grade.replaceChildren(...dados.campeoes.map((c, i) => {
        const cel = document.createElement('div');
        cel.className = 'avatarCelula' + (c.desbloqueado ? '' : ' bloqueado');

        const em = document.createElement('span'); em.className = 'aEmoji'; em.textContent = c.simbolo;
        const nm = document.createElement('span'); nm.className = 'aNome'; nm.textContent = c.nome;
        cel.append(em, nm);

        // Pré-seleciona o avatar atual (se desbloqueado).
        if (c.desbloqueado && c.simbolo === dados.avatar && avatarSelecionado === -1) {
            avatarSelecionado = i;
            cel.classList.add('selecionado');
        }
        if (c.desbloqueado) cel.addEventListener('click', () => selecionarAvatar(i));
        return cel;
    }));

    // Avatar atual não encontrado entre os desbloqueados: cai no 1º desbloqueado.
    if (avatarSelecionado === -1) {
        const i = dados.campeoes.findIndex(c => c.desbloqueado);
        if (i >= 0) selecionarAvatar(i);
    }
}

// ✏️ ao lado do nome: destrava a edição (não editável ao abrir).
document.getElementById('editarNomeBtn').addEventListener('click', () => {
    const nomeInput = document.getElementById('nomeEditar');
    nomeInput.readOnly = false;
    nomeInput.focus();
    nomeInput.select();
});

function selecionarAvatar(i) {
    avatarSelecionado = i;
    const celulas = document.getElementById('avatarGrade').children;
    for (let k = 0; k < celulas.length; k++) celulas[k].classList.toggle('selecionado', k === i);
}

function salvarEdicao() {
    const nome = document.getElementById('nomeEditar').value.trim();
    if (!nome || avatarSelecionado < 0) return;
    mandar('salvarPerfil', avatarSelecionado, nome);
}

document.getElementById('salvarEditar').addEventListener('click', salvarEdicao);
document.getElementById('nomeEditar').addEventListener('keydown', e => {
    if (e.key === 'Enter') salvarEdicao();
});

// ---------- fim de batalha (overlay por lado) ----------
document.getElementById('fimBatalha').addEventListener('click', () => mandar('voltarMenu'));

function mostrarFim(lado, mensagem) {
    const esq = document.querySelector('#fimEsq .fimMsg');
    const dir = document.querySelector('#fimDir .fimMsg');
    if (lado === 1 || lado === 2) {
        const venceuEsq = lado === 1;
        esq.textContent = venceuEsq ? '🏆 Vitória!' : '☠️ Derrota!';
        dir.textContent = venceuEsq ? '☠️ Derrota!' : '🏆 Vitória!';
        esq.className = 'fimMsg ' + (venceuEsq ? 'venceu' : 'perdeu');
        dir.className = 'fimMsg ' + (venceuEsq ? 'perdeu' : 'venceu');
    } else {
        // Campanha (futuro): mensagem única, guardando o molde.
        esq.textContent = mensagem || 'Fim da batalha!';
        esq.className = 'fimMsg venceu';
        dir.textContent = '';
        dir.className = 'fimMsg';
    }
}

// ---------- montagem da Arena ----------
let arenaCampeoes = [];                                            // pool [{simbolo, nome}] (índice = id)
let arenaTimes = { esq: [null, null, null, null], dir: [null, null, null, null] };  // índices ou null
let arenaControle = { esq: 'jogador', dir: 'bot' };               // padrão: esquerda joga, direita bot
let arenaSlotSel = null;                                           // { lado, i } ou null

function mostrarMontagemArena(campeoes) {
    mostrarCena('arenaSetup');
    arenaCampeoes = campeoes;
    arenaTimes = { esq: [null, null, null, null], dir: [null, null, null, null] };
    arenaControle = { esq: 'jogador', dir: 'bot' };
    arenaSlotSel = null;
    aplicarToggleArena('esq', 'jogador');
    aplicarToggleArena('dir', 'bot');
    montarPickerArena();
    desenharSlotsArena();
}

// ---------- montagem de time: helpers compartilhados (arena e campanha) ----------
// picker = a grade de champs (de onde escolhe); slot = as casas do time montado.
// Cliques: picker → adiciona na casa selecionada/1ª vazia; casa vazia → seleciona; casa cheia → remove.
// Arrastar: picker→casa substitui; casa→casa troca de posição; casa→fora dos slots remove.
let arrastando = null;   // { tipo:'picker', idx } | { tipo:'slot', arr, i }

function criarCelulaPicker(c) {
    const cel = document.createElement('div');
    cel.className = 'avatarCelula';
    const em = document.createElement('span'); em.className = 'aEmoji'; em.textContent = c.simbolo;
    const nm = document.createElement('span'); nm.className = 'aNome'; nm.textContent = c.nome;
    cel.append(em, nm);
    return cel;
}

function tornarPickerArrastavel(cel, idx) {
    cel.draggable = true;
    cel.addEventListener('dragstart', e => { arrastando = { tipo: 'picker', idx }; e.dataTransfer.setData('text', ''); });
    cel.addEventListener('dragend', () => { arrastando = null; });
}

function criarSlot(campeoes, idx, selecionado) {
    const slot = document.createElement('div');
    slot.className = 'slot' + (idx != null ? ' preenchido' : '') + (selecionado ? ' selecionado' : '');
    if (idx != null) {
        const c = campeoes[idx];
        const em = document.createElement('span'); em.className = 'slotEmoji'; em.textContent = c.simbolo;
        const nm = document.createElement('span'); nm.className = 'slotNome'; nm.textContent = c.nome;
        slot.append(em, nm);
    } else {
        const v = document.createElement('span'); v.className = 'slotVazio'; v.textContent = 'clique e escolha';
        slot.append(v);
    }
    return slot;
}

// `arr` = array do time (o lado, na arena); `i` = índice da casa; `redesenhar` re-renderiza.
function configurarSlotDnD(slot, arr, i, redesenhar) {
    if (arr[i] != null) {
        slot.draggable = true;
        slot.addEventListener('dragstart', e => { arrastando = { tipo: 'slot', arr, i }; e.dataTransfer.setData('text', ''); });
        slot.addEventListener('dragend', () => {   // soltou FORA de qualquer slot → remove
            if (arrastando && arrastando.tipo === 'slot') { arrastando.arr[arrastando.i] = null; arrastando = null; redesenhar(); }
        });
    }
    slot.addEventListener('dragover', e => { e.preventDefault(); slot.classList.add('dropAlvo'); });
    slot.addEventListener('dragleave', () => slot.classList.remove('dropAlvo'));
    slot.addEventListener('drop', e => {
        e.preventDefault();
        slot.classList.remove('dropAlvo');
        if (!arrastando) return;
        if (arrastando.tipo === 'picker') {
            const k = arr.indexOf(arrastando.idx);   // dedup só NESTE time (o outro lado pode repetir)
            if (k >= 0) arr[k] = null;
            arr[i] = arrastando.idx;                 // substitui o que estava na casa
        } else {                                     // casa → casa: troca de posição
            const s = arrastando;
            const tmp = arr[i]; arr[i] = s.arr[s.i]; s.arr[s.i] = tmp;
        }
        arrastando = null;
        redesenhar();
    });
}

// ---------- montagem: Arena ----------
function montarPickerArena() {
    document.getElementById('setupPicker').replaceChildren(...arenaCampeoes.map((c, i) => {
        const cel = criarCelulaPicker(c);
        cel.addEventListener('click', () => escolherCampeaoArena(i));
        tornarPickerArrastavel(cel, i);
        return cel;
    }));
}

function desenharSlotsArena() {
    for (const lado of ['esq', 'dir']) {
        const cont = document.getElementById(lado === 'esq' ? 'slotsEsq' : 'slotsDir');
        cont.replaceChildren(...arenaTimes[lado].map((idx, i) => {
            const slot = criarSlot(arenaCampeoes, idx, arenaSlotSel && arenaSlotSel.lado === lado && arenaSlotSel.i === i);
            slot.addEventListener('click', () => {
                if (arenaTimes[lado][i] != null) arenaTimes[lado][i] = null;   // casa cheia = remove
                else arenaSlotSel = { lado, i };                                // casa vazia = seleciona (foca o lado)
                desenharSlotsArena();
            });
            configurarSlotDnD(slot, arenaTimes[lado], i, desenharSlotsArena);
            return slot;
        }));
    }
    // Basta 1 de cada lado (dá pra montar 1x1 pra testar algo).
    const podeLutar = ['esq', 'dir'].every(l => arenaTimes[l].some(v => v != null));
    document.getElementById('setupLutar').disabled = !podeLutar;
}

// Clique no picker = adiciona na casa selecionada (ou 1ª vazia do lado em foco). Não duplica NO MESMO
// lado — mas o mesmo champ PODE estar nos dois times (espelho no versus).
function escolherCampeaoArena(idx) {
    const lado = arenaSlotSel ? arenaSlotSel.lado : 'esq';
    if (arenaTimes[lado].includes(idx)) return;
    const i = (arenaSlotSel && arenaTimes[lado][arenaSlotSel.i] == null) ? arenaSlotSel.i : arenaTimes[lado].indexOf(null);
    if (i < 0) return;
    arenaTimes[lado][i] = idx;
    const prox = arenaTimes[lado].indexOf(null);
    arenaSlotSel = prox >= 0 ? { lado, i: prox } : null;
    desenharSlotsArena();
}

// Quatro índices DISTINTOS de um pool — o time sorteado. Embaralho de Fisher-Yates e corta em 4;
// com menos de 4 no pool, as casas que sobram ficam vazias em vez de repetir alguém.
function sortearTime(total) {
    const ids = [...Array(total).keys()];
    for (let k = ids.length - 1; k > 0; k--) {
        const j = Math.floor(Math.random() * (k + 1));
        [ids[k], ids[j]] = [ids[j], ids[k]];
    }
    const time = ids.slice(0, 4);
    while (time.length < 4) time.push(null);
    return time;
}

function sortearLadoArena(lado) {
    arenaTimes[lado] = sortearTime(arenaCampeoes.length);
    if (arenaSlotSel && arenaSlotSel.lado === lado) arenaSlotSel = null;
    desenharSlotsArena();
}

function aplicarToggleArena(lado, tipo) {
    arenaControle[lado] = tipo;
    document.querySelectorAll(`.setupJog[data-lado="${lado}"]`).forEach(b => b.classList.toggle('ativo', tipo === 'jogador'));
    document.querySelectorAll(`.setupBot[data-lado="${lado}"]`).forEach(b => b.classList.toggle('ativo', tipo === 'bot'));
}

document.querySelectorAll('.setupSortear').forEach(b => b.addEventListener('click', () => sortearLadoArena(b.dataset.lado)));
document.querySelectorAll('.setupJog').forEach(b => b.addEventListener('click', () => aplicarToggleArena(b.dataset.lado, 'jogador')));
document.querySelectorAll('.setupBot').forEach(b => b.addEventListener('click', () => aplicarToggleArena(b.dataset.lado, 'bot')));
document.getElementById('setupLutar').addEventListener('click', () => {
    const time1 = arenaTimes.esq.filter(v => v != null);
    const time2 = arenaTimes.dir.filter(v => v != null);
    if (!time1.length || !time2.length) return;   // pelo menos 1 de cada lado
    mandar('iniciarArena', 0, JSON.stringify({
        time1,
        time2,
        bot1: arenaControle.esq === 'bot',
        bot2: arenaControle.dir === 'bot',
    }));
});

// ---------- Campanha: mapa ----------
const ESPACO_NO = 210;   // distância horizontal entre facções
let mapaNodeX = [];      // x de cada nó (o marcador anda até um deles)
let mapaSelecionado = -1;// facção com o marcador em cima e o botão "Entrar" visível; -1 = nenhuma

function mostrarMapa(m) {
    mostrarCena('campanhaMapa');
    mapaSelecionado = -1;
    const trilha = document.getElementById('mapaTrilha');
    mapaNodeX = m.capitulos.map((_, i) => 110 + i * ESPACO_NO);

    const nos = m.capitulos.map((c, i) => {
        const no = document.createElement('div');
        no.className = 'mapaNo' + (c.desbloqueado ? '' : ' bloqueado') + (c.concluido ? ' concluido' : '');
        no.style.left = mapaNodeX[i] + 'px';
        no.dataset.idx = i;
        const circ = document.createElement('div'); circ.className = 'noCirculo'; circ.textContent = c.desbloqueado ? c.simbolo : '🔒';
        const nome = document.createElement('div'); nome.className = 'noNome'; nome.textContent = c.nome;
        no.append(circ, nome);
        if (c.desbloqueado) {
            const entrar = document.createElement('button');
            entrar.type = 'button'; entrar.className = 'noEntrar'; entrar.textContent = 'Entrar'; entrar.hidden = true;
            entrar.addEventListener('click', e => { e.stopPropagation(); mandar('selecionarCapitulo', i); });
            no.appendChild(entrar);
            no.addEventListener('click', () => clicarNo(i));
        }
        return no;
    });

    trilha.style.width = (220 + (m.capitulos.length - 1) * ESPACO_NO) + 'px';
    const marcador = document.getElementById('mapaMarcador');
    marcador.textContent = perfilAvatar;
    trilha.replaceChildren(marcador, ...nos);

    // posiciona SEM animar (troca de cena); zera o pan
    marcador.style.transition = 'none';
    marcador.style.left = mapaNodeX[Math.max(0, Math.min(m.posicao, mapaNodeX.length - 1))] + 'px';
    mapaOffset = 0; mapaOffsetBase = 0; trilha.style.transform = 'translateX(0px)';
    requestAnimationFrame(() => { marcador.style.transition = ''; });
}

// 1º clique numa facção: o marcador caminha até ela e o botão "Entrar" aparece. 2º clique na MESMA
// (ou o botão): entra. Assim dá pra só andar pelo mapa sem entrar.
function clicarNo(i) {
    if (mapaMoveu) return;   // foi arrasto, não clique
    if (i === mapaSelecionado) { mandar('selecionarCapitulo', i); return; }
    mapaSelecionado = i;
    document.getElementById('mapaMarcador').style.left = mapaNodeX[i] + 'px';   // caminha (~1.4s CSS)
    atualizarEntrarBotoes();
}

function atualizarEntrarBotoes() {
    document.querySelectorAll('#mapaTrilha .mapaNo').forEach(no => {
        const btn = no.querySelector('.noEntrar');
        if (btn) btn.hidden = Number(no.dataset.idx) !== mapaSelecionado;
    });
}

// pan por arrasto (sem barra de rolagem)
let mapaArrastando = false, mapaStartX = 0, mapaOffset = 0, mapaOffsetBase = 0, mapaMoveu = false;
(() => {
    const vp = document.getElementById('mapaViewport');
    vp.addEventListener('mousedown', e => { mapaArrastando = true; mapaMoveu = false; mapaStartX = e.clientX; vp.classList.add('arrastando'); });
    window.addEventListener('mousemove', e => {
        if (!mapaArrastando) return;
        const dx = e.clientX - mapaStartX;
        if (Math.abs(dx) > 4) mapaMoveu = true;
        const trilha = document.getElementById('mapaTrilha');
        const limite = Math.min(0, vp.clientWidth - trilha.offsetWidth);
        mapaOffset = Math.max(limite, Math.min(0, mapaOffsetBase + dx));
        trilha.style.transform = `translateX(${mapaOffset}px)`;
    });
    window.addEventListener('mouseup', () => {
        if (!mapaArrastando) return;
        mapaArrastando = false; mapaOffsetBase = mapaOffset;
        document.getElementById('mapaViewport').classList.remove('arrastando');
    });
})();


// ---------- Campanha: fases ----------
let campFases = null, campFaseSel = null, campSlotSel = null;
let campTime = [null, null, null, null];

function mostrarFasesCampanha(f) {
    mostrarCena('campanhaFases');
    campFases = f;
    campFaseSel = null;
    campSlotSel = null;

    // O time volta MONTADO da última vez (o C# manda os índices; quem guarda identidade é o save).
    // Sem isto, repetir uma fase custava quatro cliques de remontagem antes de qualquer coisa.
    campTime = [null, null, null, null];
    (f.timeMontado || []).slice(0, 4).forEach((idx, i) => { campTime[i] = idx; });

    document.getElementById('fasesTitulo').textContent = `${f.capituloSimbolo} ${f.capituloNome}`;
    document.getElementById('faseDetalhe').hidden = true;
    document.getElementById('fasesLutar').disabled = true;

    let botaoDaSelecionada = null, faseDaSelecionada = null;

    document.getElementById('fasesLista').replaceChildren(...f.fases.map(fase => {
        const b = document.createElement('button');
        b.type = 'button';
        b.className = 'faseBtn';
        b.disabled = !fase.desbloqueado;
        const st = document.createElement('span'); st.className = 'faseStatus';
        st.textContent = !fase.desbloqueado ? '🔒' : fase.concluido ? '✅' : '⚔️';
        const nm = document.createElement('span'); nm.className = 'faseNome'; nm.textContent = `${fase.numero}. ${fase.nome}`;
        b.append(st, nm);
        if (fase.desbloqueado) {
            b.addEventListener('click', () => selecionarFaseCampanha(fase, b));
            if (fase.numero === f.faseSelecionada) { botaoDaSelecionada = b; faseDaSelecionada = fase; }
        }
        return b;
    }));

    montarPickerFase();

    // SEMPRE há uma fase selecionada: a última visitada, ou a 1. Quem decide é o C# (a memória é
    // progressão, não estado de tela) — aqui só se abre o detalhe dela.
    if (botaoDaSelecionada) selecionarFaseCampanha(faseDaSelecionada, botaoDaSelecionada);
}

function selecionarFaseCampanha(fase, btn) {
    campFaseSel = fase;
    [...document.getElementById('fasesLista').children].forEach(b => b.classList.remove('selecionada'));
    btn.classList.add('selecionada');
    document.getElementById('faseDetalhe').hidden = false;

    document.getElementById('faseInimigos').replaceChildren(
        grupoRodada('Rodada 1', fase.rodada1), grupoRodada('Rodada 2', fase.rodada2));

    const it = fase.item;
    const item = document.getElementById('faseItem');
    item.replaceChildren();
    const emo = document.createElement('span'); emo.className = 'fiEmoji'; emo.textContent = it.simbolo;
    const txt = document.createElement('span'); txt.textContent = `${it.nome} · `;
    const stat = document.createElement('span'); stat.className = 'fiStat'; stat.textContent = `${it.stat} ${it.valor}`;
    item.append(emo, txt, stat);

    atualizarLutarFase();
}

function grupoRodada(titulo, champs) {
    const g = document.createElement('div'); g.className = 'rodadaGrupo';
    const t = document.createElement('div'); t.className = 'rodadaTitulo'; t.textContent = titulo;
    const cs = document.createElement('div'); cs.className = 'rodadaChamps';
    cs.replaceChildren(...champs.map(c => {
        const m = document.createElement('div'); m.className = 'miniChamp';
        const e = document.createElement('span'); e.className = 'mcEmoji'; e.textContent = c.simbolo;
        const n = document.createElement('span'); n.className = 'mcNome'; n.textContent = c.nome;
        m.append(e, n);
        return m;
    }));
    g.append(t, cs);
    return g;
}

function montarPickerFase() {
    document.getElementById('fasePicker').replaceChildren(...campFases.meusCampeoes.map((c, i) => {
        const cel = criarCelulaPicker(c);
        cel.addEventListener('click', () => escolherChampFase(i));
        tornarPickerArrastavel(cel, i);
        return cel;
    }));
    desenharSlotsFase();
}

function desenharSlotsFase() {
    document.getElementById('faseSlots').replaceChildren(...campTime.map((idx, i) => {
        const slot = criarSlot(campFases.meusCampeoes, idx, campSlotSel === i);
        slot.addEventListener('click', () => {
            if (campTime[i] != null) campTime[i] = null;   // casa cheia = remove
            else campSlotSel = i;                           // casa vazia = seleciona
            desenharSlotsFase();
        });
        configurarSlotDnD(slot, campTime, i, desenharSlotsFase);
        return slot;
    }));
    atualizarLutarFase();
}

// Clique no picker = adiciona na casa selecionada (ou 1ª vazia). Não duplica.
function escolherChampFase(idx) {
    if (campTime.includes(idx)) return;
    const i = (campSlotSel != null && campTime[campSlotSel] == null) ? campSlotSel : campTime.indexOf(null);
    if (i < 0) return;
    campTime[i] = idx;
    const prox = campTime.indexOf(null);
    campSlotSel = prox >= 0 ? prox : null;
    desenharSlotsFase();
}

function atualizarLutarFase() {
    const temTime = campTime.some(v => v != null);
    document.getElementById('fasesLutar').disabled = !(campFaseSel && temTime);
}

document.getElementById('fasesLutar').addEventListener('click', () => {
    if (!campFaseSel) return;
    const time = campTime.filter(v => v != null);
    if (!time.length) return;
    mandar('iniciarFase', 0, JSON.stringify({ fase: campFaseSel.numero, time }));
});

document.getElementById('faseSortear').addEventListener('click', () => {
    if (!campFases) return;
    campTime = sortearTime(campFases.meusCampeoes.length);
    campSlotSel = null;
    desenharSlotsFase();
});

// ---------- Campanha: fim de fase ----------
// Uma tela pros dois desfechos. Em dois momentos: sem opções (é a passagem da recompensa, um clique
// segue pras conquistas) e com opções (é a hora de decidir). Mesma tela nos dois pra o jogador não
// sentir que mudou de lugar entre ganhar o item e escolher o que fazer.
let fimComOpcoes = false;

function mostrarFimDeFase(f) {
    mostrarCena('fimDeFase');
    fimComOpcoes = !!f.comOpcoes;

    document.getElementById('fimFaseTitulo').textContent = f.venceu ? '🏆 Fase concluída!' : '🕯️ Derrota';

    const cont = document.getElementById('fimFaseConteudo');
    cont.replaceChildren();

    if (!f.venceu) {
        const p = document.createElement('div');
        p.className = 'fimFaseTexto';
        p.textContent = 'A Deusa observa em silêncio. Levante e tente de novo.';
        cont.append(p);
    }

    const r = f.recompensa;
    if (r && r.item) {
        // O item ganha CARD, não uma linha de texto: é a recompensa que o jogador veio buscar, e
        // antes ela passava despercebida no meio da tela.
        const bloco = document.createElement('div'); bloco.className = 'recompensaBloco';
        const t = document.createElement('div'); t.className = 'recompensaTitulo'; t.textContent = 'Novo item';

        const card = document.createElement('div'); card.className = 'itemPremio';
        const em = document.createElement('span'); em.className = 'ipEmoji'; em.textContent = r.item.simbolo;
        const info = document.createElement('div'); info.className = 'ipInfo';
        const nm = document.createElement('div'); nm.className = 'ipNome'; nm.textContent = r.item.nome;
        const st = document.createElement('div'); st.className = 'ipStat'; st.textContent = `${r.item.stat} +${r.item.valor}`;
        info.append(nm, st);
        card.append(em, info);

        bloco.append(t, card); cont.append(bloco);
    }
    if (r && r.novos && r.novos.length) {
        const bloco = document.createElement('div'); bloco.className = 'recompensaBloco';
        const t = document.createElement('div'); t.className = 'recompensaTitulo'; t.textContent = 'Novos campeões';
        const cs = document.createElement('div'); cs.className = 'recompensaChamps';
        cs.replaceChildren(...r.novos.map(c => {
            const m = document.createElement('div'); m.className = 'miniChamp';
            const e = document.createElement('span'); e.className = 'mcEmoji'; e.textContent = c.simbolo;
            const n = document.createElement('span'); n.className = 'mcNome'; n.textContent = c.nome;
            m.append(e, n); return m;
        }));
        bloco.append(t, cs); cont.append(bloco);
    }

    document.getElementById('fimFaseOpcoes').hidden = !fimComOpcoes;
    document.getElementById('fimFaseDica').hidden = fimComOpcoes;

    // Depois da fase 7 o "continuar" atravessa pro capítulo seguinte — e o botão diz isso. Prometer
    // "Próxima Fase" quando o que vem é outro capítulo esconderia do jogador que ele mudou de lugar.
    const proximo = document.getElementById('fimProximaFase');
    proximo.hidden = !f.podeProxima;
    proximo.textContent = f.proximoECapitulo ? 'Próximo Capítulo »' : 'Próxima Fase »';
}

// Índices casados com o enum DecisaoDeFim do C# — a ponte carrega um int, então a ordem é contrato.
const DECISAO = { jogarNovamente: 0, editarEquipe: 1, proximaFase: 2 };

document.getElementById('fimJogarNovamente').addEventListener('click', () => mandar('fimDeFase', DECISAO.jogarNovamente));
document.getElementById('fimEditarEquipe').addEventListener('click', () => mandar('fimDeFase', DECISAO.editarEquipe));
document.getElementById('fimProximaFase').addEventListener('click', () => mandar('fimDeFase', DECISAO.proximaFase));

// Clicar na tela só serve na passagem da recompensa. Com as opções à mostra, o clique no vazio não
// pode escolher por ninguém — cada botão diz o que faz.
document.getElementById('fimDeFase').addEventListener('click', e => {
    if (fimComOpcoes || e.target.closest('#fimFaseOpcoes')) return;
    mandar('continuar');
});

// ---------- Campanha: a conquista de um champ ----------
// Ele vem do fundo, pequeno e fora de foco, e cresce até o centro; ao chegar, brilha e some o
// brilho. Terminada a animação a tela vira a FICHA dele — a mesma seção do compêndio. Dois cliques
// pulam pro fim (quem já viu não precisa ver de novo).
let conquistaEmCurso = null;   // o champ que está chegando (guardado pra virar ficha no fim)

function mostrarConquista(champ) {
    mostrarCena('conquista');
    conquistaEmCurso = champ;

    document.getElementById('conquistaEmoji').textContent = champ.simbolo;
    document.getElementById('conquistaNome').textContent = champ.nome;
    document.getElementById('conquistaFaccao').textContent = champ.faccao;

    // UMA classe rege a cena inteira (champ, aura, anéis, raios) — assim as peças compartilham a
    // mesma linha do tempo sem cada uma precisar ser ligada à mão.
    const cena = document.getElementById('conquista');
    cena.classList.remove('chegando');
    void cena.offsetWidth;          // reinicia a animação mesmo com dois champs seguidos
    cena.classList.add('chegando');

    // Quem manda abrir a ficha é o fim da animação DO CORPO, não de qualquer uma da cena: o
    // `animationend` borbulha, e as outras peças terminam em tempos diferentes. Sem `once`, porque
    // `once` somado a um filtro se auto-desarma no primeiro evento ALHEIO — e aí a ficha nunca abre.
    const corpo = document.getElementById('conquistaCorpo');
    const aoTerminar = e => {
        if (e.target !== corpo) return;
        corpo.removeEventListener('animationend', aoTerminar);
        abrirFichaDaConquista();
    };
    corpo.addEventListener('animationend', aoTerminar);
}

// Fim da animação (ou pulo): a mesma cena vira a ficha do champ. O C# continua achando que estamos
// na conquista — o que ele espera é um "continuar", e é o Esc/Sair daqui que vai mandá-lo.
function abrirFichaDaConquista() {
    if (!conquistaEmCurso) return;
    mostrarChampDetalhe(conquistaEmCurso);
    mostrarCena('conquistaChamp');
    conquistaEmCurso = null;
}

document.getElementById('conquista').addEventListener('dblclick', abrirFichaDaConquista);

// A ficha do champ sai por CLIQUE ou Enter, além do Esc/X — vale nos dois donos da seção (o
// compêndio e a conquista), porque é a mesma tela e não deve ter dois jeitos de fechar.
document.getElementById('compendioChamp').addEventListener('click', sairDaTela);

// ---------- Arsenal ----------
const ARSENAL_AREAS = ['arma', 'elmo', 'escudo', 'acess', 'peito', 'calca', 'bota'];   // slot índice → grid-area
const ARSENAL_ICONES = ['🗡️', '⛑️', '🛡️', '📿', '🎽', '👖', '👢'];   // ícone do tipo quando o slot está vazio
let arsenalDados = null;
let arsenalSlotSel = -1;

function mostrarArsenal(a) {
    if (cenaAtual !== 'arsenal') arsenalSlotSel = -1;   // entrada fresca → nenhum slot aberto
    mostrarCena('arsenal');
    arsenalDados = a;
    desenharBoneco();
    desenharTotais();
    if (arsenalSlotSel >= 0) mostrarItensSlot(arsenalSlotSel);
    else document.getElementById('arsenalDetalhe').hidden = true;
}

// O que o conjunto equipado dá, somado. Quem soma e quem escreve o número é o C#
// (ArsenalService.TotaisEquipados + ValorFormatado) — aqui só chega texto pronto, pelo mesmo motivo
// de sempre: "0.05" virar "5%" é exibição, mas QUANTO é regra de item.
function desenharTotais() {
    const cont = document.getElementById('arsenalTotais');
    const titulo = document.createElement('div');
    titulo.className = 'atTitulo';
    titulo.textContent = 'Bônus do arsenal';

    if (!arsenalDados.totais.length) {
        const v = document.createElement('div');
        v.className = 'atVazio';
        v.textContent = 'Nada equipado ainda.';
        cont.replaceChildren(titulo, v);
        return;
    }

    cont.replaceChildren(titulo, ...arsenalDados.totais.map(b => {
        const linha = document.createElement('div'); linha.className = 'atLinha';
        const rot = document.createElement('span'); rot.className = 'atStat'; rot.textContent = b.stat;
        const val = document.createElement('span'); val.className = 'atValor'; val.textContent = b.valor;
        linha.append(rot, val);
        return linha;
    }));
}

function desenharBoneco() {
    document.getElementById('boneco').replaceChildren(...arsenalDados.slots.map(s => {
        const div = document.createElement('div');
        div.className = 'bonecoSlot' + (s.slot === arsenalSlotSel ? ' selecionado' : '') + (s.equipado ? ' preenchido' : '');
        div.style.gridArea = ARSENAL_AREAS[s.slot];
        const emoji = document.createElement('div'); emoji.className = 'bsEmoji';
        emoji.textContent = s.equipado ? s.equipado.simbolo : ARSENAL_ICONES[s.slot];
        const nome = document.createElement('div'); nome.className = 'bsNome'; nome.textContent = s.nome;
        div.append(emoji, nome);
        div.addEventListener('click', () => mostrarItensSlot(s.slot));
        return div;
    }));
}

function mostrarItensSlot(slot) {
    arsenalSlotSel = slot;
    desenharBoneco();
    document.getElementById('arsenalDetalhe').hidden = false;
    document.getElementById('arsenalSlotNome').textContent = arsenalDados.slots[slot].nome;

    const itens = arsenalDados.obtidos.filter(o => o.slot === slot);
    const equipado = itens.find(o => o.equipado);
    const cont = document.getElementById('arsenalItens');

    if (!itens.length) {
        const v = document.createElement('div'); v.className = 'arsenalVazio'; v.textContent = 'Nenhum item deste tipo ainda.';
        cont.replaceChildren(v);
        return;
    }

    cont.replaceChildren(...itens.map(o => {
        const card = document.createElement('div');
        card.className = 'itemCard' + (o.equipado ? ' equipado' : '');

        const em = document.createElement('span'); em.className = 'icEmoji'; em.textContent = o.simbolo;
        const info = document.createElement('div'); info.className = 'icInfo';
        const nm = document.createElement('div'); nm.className = 'icNome'; nm.textContent = `${o.nome} · ${o.faccao}`;
        const st = document.createElement('div'); st.className = 'icStat'; st.textContent = `${o.stat} +${o.valor}`;
        info.append(nm, st);
        card.append(em, info);

        if (equipado && !o.equipado) {   // seta de diferença vs o equipado
            const diff = o.valorNum - equipado.valorNum;
            const d = document.createElement('div');
            d.className = 'icDiff ' + (diff > 0 ? 'sobe' : diff < 0 ? 'desce' : '');
            d.textContent = diff > 0 ? '▲' : diff < 0 ? '▼' : '=';
            card.append(d);
        }
        if (o.equipado) {
            const tag = document.createElement('div'); tag.className = 'icTag'; tag.textContent = 'equipado';
            card.append(tag);
        } else {
            card.addEventListener('click', () => mandar('equiparItem', o.indice));
        }
        return card;
    }));
}

// ---------- compêndio ----------
// Catálogo, só leitura: nenhum clique daqui muda progresso. Por isso o champ TRAVADO é clicável
// igual ao liberado — o cadeado diz "ainda não é seu", não "não é da sua conta". Quem decide o que
// está travado é o C# (CampeoesService.EstaDesbloqueado); aqui só se pinta a resposta.
function mostrarCompendio(c) {
    mostrarCena('compendio');

    document.getElementById('compendioFaccoes').replaceChildren(...c.faccoes.map(f => {
        const bloco = document.createElement('section');
        bloco.className = 'compFaccao';

        const titulo = document.createElement('h2');
        titulo.className = 'compFaccaoNome';
        titulo.textContent = `${f.simbolo} ${f.nome}`;

        const grade = document.createElement('div');
        grade.className = 'compGrade';
        grade.replaceChildren(...f.champs.map(ch => {
            const card = document.createElement('button');
            card.type = 'button';
            card.className = 'compChamp' + (ch.desbloqueado ? '' : ' travado');

            const em = document.createElement('span');
            em.className = 'ccEmoji';
            em.textContent = ch.simbolo;

            const nm = document.createElement('span');
            nm.className = 'ccNome';
            nm.textContent = ch.nome;

            card.append(em, nm);
            if (!ch.desbloqueado) {
                const cad = document.createElement('span');
                cad.className = 'ccCadeado';
                cad.textContent = '🔒';
                card.appendChild(cad);
            }

            // O índice é GLOBAL (posição na lista completa), não o da facção: a ponte carrega um int
            // só por clique, e mandar (facção, slot) exigiria dois.
            card.addEventListener('click', () => mandar('verChamp', ch.indice));
            return card;
        }));

        bloco.append(titulo, grade);
        return bloco;
    }));
}

function mostrarChampDetalhe(c) {
    mostrarCena('compendioChamp');

    // Hoje o emoji gigante; o #champArte é o SLOT que recebe a arte do personagem inteiro depois.
    document.getElementById('champArte').textContent = c.simbolo;
    document.getElementById('champNome').textContent = c.nome;
    document.getElementById('champFaccao').textContent =
        c.desbloqueado ? c.faccao : `${c.faccao} · 🔒 ainda não conquistado`;

    // Números de BASE: catálogo, não simulador — arsenal, itens e buffs não entram aqui.
    const stats = [
        ['❤️', 'HP', c.hp],
        ['⚔️', 'Ataque', c.ataque],
        ['🛡️', 'Defesa', c.defesa],
        ['🎯', 'Taxa de crítico', `${c.taxaCritPct}%`],
        ['💥', 'Dano crítico', `${c.danoCritPct}%`],
    ];

    const painelStats = document.getElementById('champStats');
    const tituloStats = document.createElement('h2');
    tituloStats.className = 'champSecao';
    tituloStats.textContent = 'Estatísticas';

    painelStats.replaceChildren(tituloStats, ...stats.map(([icone, rotulo, valor]) => {
        const linha = document.createElement('div');
        linha.className = 'champStat';

        const ic = document.createElement('span'); ic.className = 'csIcone'; ic.textContent = icone;
        const rot = document.createElement('span'); rot.className = 'csRotulo'; rot.textContent = rotulo;
        const val = document.createElement('span'); val.className = 'csValor'; val.textContent = valor;

        linha.append(ic, rot, val);
        return linha;
    }));

    const painelHabs = document.getElementById('champHabilidades');
    const tituloHabs = document.createElement('h2');
    tituloHabs.className = 'champSecao';
    tituloHabs.textContent = 'Habilidades';

    painelHabs.replaceChildren(tituloHabs, ...c.habilidades.map(h => {
        const card = document.createElement('div');
        card.className = 'champHab' + (h.passiva ? ' passiva' : '');

        const topo = document.createElement('div');
        topo.className = 'chTopo';

        const nome = document.createElement('span');
        nome.className = 'chNome';
        nome.textContent = `${h.simbolo} ${h.nome}`;

        // A passiva não se usa: dizer isso evita o jogador procurar o botão dela. As ativas mostram a
        // cadência DECLARADA — fora da luta não há turno correndo, e é a cadência que se compara.
        const marca = document.createElement('span');
        marca.className = 'chMarca';
        marca.textContent = h.passiva ? 'passiva' : `${h.cooldown} turnos`;

        topo.append(nome, marca);

        const desc = document.createElement('p');
        desc.className = 'chDesc';
        desc.textContent = h.descricao;

        card.append(topo, desc);
        return card;
    }));
}


// ---------- modal de confirmação ----------
let modalAberto = false;
let modalAoConfirmar = null;

function confirmar(texto, aoConfirmar) {
    modalAberto = true;
    modalAoConfirmar = aoConfirmar;
    document.getElementById('modalTexto').textContent = texto;
    document.getElementById('modal').hidden = false;
}

function fecharModal() {
    modalAberto = false;
    modalAoConfirmar = null;
    document.getElementById('modal').hidden = true;
}

document.getElementById('modalConfirmar').addEventListener('click', () => {
    const cb = modalAoConfirmar;
    fecharModal();
    if (cb) cb();
});
document.getElementById('modalCancelar').addEventListener('click', fecharModal);

function aplicarEstado(novo) {
    // Batalha nova (entrando no combate de outra cena) → log limpo. O log não persiste entre fases/
    // arenas: acabou a luta, morre; ao entrar de novo (mesma fase inclusive) nasce um log novo. Entre
    // as 2 rodadas de uma fase a cena continua 'combate', então o log dessa fase é preservado.
    if (cenaAtual !== 'combate') limparLog();
    mostrarCena('combate');   // chegou estado de batalha → sai do menu

    // Ao voltar pra escolha de ação, a habilidade anterior já foi usada (ou cancelada).
    if (nomeDaFase(novo) === 'EscolhendoAcao') habilidadeEscolhida = null;

    // Buff não vem como evento (só dano e cura vêm) — então a piscada de buff nasce da DIFERENÇA
    // entre o estado anterior e o novo: quem GANHOU escudo/buff desde o último quadro, brilha.
    const brilhos = detectarBuffsGanhos(estado, novo);

    estado = novo;

    // Quem age vira o selecionado por padrão — poupa um clique a cada turno.
    if (novo.quemAge != null && nomeDaFase(novo) === 'EscolhendoAcao') selecionadoId = novo.quemAge;

    desenhar();

    // Depois do desenho: os elementos já existem pra receber a animação.
    for (const b of brilhos) {
        const el = document.querySelector(`.combatente[data-id="${b.id}"]`);
        if (el) reanimar(el, b.classe);
    }
}

// Compara os status de cada combatente entre dois estados e devolve os ganhos a animar. Escudo
// (campo numérico próprio) → azul; outro buff novo → dourado; debuff novo → roxo. Vermelho não
// entra: é a cor do DANO (ferido), não de status.
function detectarBuffsGanhos(anterior, novo) {
    if (!anterior) return [];   // primeiro quadro: nada "ganhou", é o estado inicial

    const antes = new Map(
        [...(anterior.equipe1 || []), ...(anterior.equipe2 || [])].map(c => [c.id, c]));

    const brilhos = [];
    for (const c of [...(novo.equipe1 || []), ...(novo.equipe2 || [])]) {
        const velho = antes.get(c.id);
        if (!velho) continue;

        const ganhouEscudo = (c.escudo || 0) > (velho.escudo || 0);
        if (ganhouEscudo) brilhos.push({ id: c.id, classe: 'ganhouEscudo' });

        // Status cujo nome não existia antes, separados por sinal. O escudo também é um buff na
        // lista, mas já foi tratado pelo número — daí o `> (ganhouEscudo ? 1 : 0)`: só dispara o
        // dourado quando o buff novo NÃO é (só) o escudo.
        const nomesAntes = new Set((velho.status || []).map(s => s.nome));
        const novos = (c.status || []).filter(s => !nomesAntes.has(s.nome));

        if (novos.filter(s => s.ehBuff).length > (ganhouEscudo ? 1 : 0))
            brilhos.push({ id: c.id, classe: 'ganhouBuff' });
        if (novos.some(s => !s.ehBuff))
            brilhos.push({ id: c.id, classe: 'ganhouDebuff' });
    }
    return brilhos;
}

// A fase chega como número (enum serializado) ou string, dependendo do serializador.
const NOMES_FASE = ['Assistindo', 'EscolhendoAcao', 'EscolhendoAlvo', 'Fim'];
const nomeDaFase = e => typeof e.fase === 'number' ? NOMES_FASE[e.fase] : e.fase;

// A habilidade atualmente ARMADA (destacada, esperando confirmação), ou null.
const habArmada = () => habilidadeEscolhida == null ? null
    : (estado?.habilidades || []).find(h => h.indice === habilidadeEscolhida) || null;

// O time (lista de combatentes) a que um id pertence.
function ladoDe(id) {
    const e1 = estado?.equipe1 || [], e2 = estado?.equipe2 || [];
    return e1.some(c => c.id === id) ? e1 : e2;
}

// A equipe1 é SEMPRE o seu lado — quem garante isso é o C# (ver SessaoDoFront.LadoDe), então a tela
// pode perguntar "é inimigo?" olhando só de que lista o id veio.
const ehInimigo = id => (estado?.equipe2 || []).some(c => c.id === id);

// Quem pode ser clicado pra CONFIRMAR uma habilidade armada que não pede alvo (Self / buff em
// aliados): clicar em si mesmo confirma o buff próprio, clicar num aliado confirma o de aliado.
// Vazio quando não há habilidade armada, quando ela pede alvo (aí o C# dirige a fase de alvo), ou
// fora da escolha de ação.
function alvosDeConfirmacao() {
    const h = habArmada();
    if (!h || h.pedeAlvo || nomeDaFase(estado) !== 'EscolhendoAcao') return new Set();

    if (h.escopo === 'Self') return new Set([estado.quemAge]);
    // Sem filtro de vivo/morto: o clique aqui não ESCOLHE alvo (o C# já resolve sozinho quem é
    // atingido, ver PedeAlvoDoJogador), só confirma o disparo — então qualquer um do time serve,
    // vivo ou morto (ex.: Robô mirando Mortos pro revive-de-todos).
    if (h.escopo === 'Aliados')
        return new Set(ladoDe(estado.quemAge).map(c => c.id));
    return new Set();
}

// ---------- desenho ----------
let confirmarAtuais = new Set();   // ids que podem confirmar a habilidade armada (recalc por quadro)

function desenhar() {
    if (!estado) return;

    document.getElementById('turno').textContent = `Turno ${estado.turno}`;
    confirmarAtuais = alvosDeConfirmacao();

    // De novo aqui, e não só no mostrarCena: o rótulo do 🚪 depende do MODO, que vem no estado — e o
    // mostrarCena roda antes de `estado` receber o quadro novo, então lá ele ainda leria o anterior.
    atualizarBotaoSair();
    aplicarTema(estado.tema || '');   // idem: o cenário também vem no estado

    // O botão do automático se desenha do estado: é o C# que manda, e ele desliga o modo sozinho a
    // cada batalha nova. Sem isto o botão continuaria dizendo ON numa luta em que o controle já
    // voltou pro jogador.
    if (!!estado.auto !== autoLigado) aplicarAuto(!!estado.auto);

    // Fim de batalha: mensagem POR LADO (vitória/derrota) por cima de tudo (clique/Enter/Esc = sair).
    // O "🚪 sair" fica embaixo do overlay — não precisa escondê-lo, qualquer clique cai na tela de fim.
    const acabou = nomeDaFase(estado) === 'Fim';
    const fim = document.getElementById('fimBatalha');
    fim.hidden = !acabou;
    if (acabou) mostrarFim(estado.ladoVencedor || 0, estado.mensagem);

    // A mensagem do retrato entra no log (sem repetir a que já está lá). O fim de batalha ganha
    // destaque próprio.
    if (estado.mensagem) registrar(estado.mensagem, nomeDaFase(estado) === 'Fim' ? 'morte' : '');

    desenharLado('ladoEsquerdo', estado.equipe1);
    desenharLado('ladoDireito', estado.equipe2);
    desenharPainel();
}

// Classes de animação em curso — precisam SOBREVIVER a um redesenho (ver desenharLado). O `foco`
// NÃO entra aqui: ele não é animação, é estado, e vem do retrato a cada quadro.
const ANIMACOES = ['batendo', 'ferido', 'curado', 'ganhouEscudo', 'ganhouBuff', 'ganhouDebuff'];

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
    // Alvo AMIGO de uma habilidade armada sem passo de alvo (buff próprio/de aliado): clicar confirma.
    if (confirmarAtuais.has(c.id)) el.classList.add('alvoAmigo');
    // O inimigo que você apontou no automático.
    if (c.id === estado.focoId) el.classList.add('foco');
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
    if (mostrarEstatisticas) {
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

// Duração "permanente" no motor é int.MaxValue (ex: a Sentença do Vilão) — mostrar o número cru
// dava "⚰️Sentença 2147483647". Acima de um turno qualquer plausível, vira ∞.
const PERMANENTE = 9000;
const duracaoTexto = t => t >= PERMANENTE ? '∞' : t;

function criarStatus(status) {
    const caixa = document.createElement('div');
    caixa.className = 'status';
    for (const s of status) {
        const selo = document.createElement('span');
        selo.className = 'selo ' + (s.ehBuff ? 'buff' : 'debuff');
        const dur = duracaoTexto(s.duracaoRestante);
        selo.textContent = `${s.simbolo}${s.nome} ${dur}`;
        selo.title = s.duracaoRestante >= PERMANENTE
            ? `${s.nome} — permanente`
            : `${s.nome} — ${s.duracaoRestante} turno(s)`;
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

    document.getElementById('painelStats').textContent = mostrarEstatisticas
        ? `HP ${c.hpAtual}/${c.hpMaximo}${c.escudo ? ` · 🛡️ ${c.escudo}` : ''} · ATK ${c.ataque} · DEF ${c.defesa} · 🎯 ${c.taxaCritPct}% · 💥 ${c.danoCritPct}%`
        : '';

    const caixaStatus = document.getElementById('painelStatus');
    caixaStatus.replaceChildren(...(c.status.length ? [criarStatus(c.status)] : []));

    desenharHabilidades(c);
}

// O painel de habilidades tem UM molde só — o botão `.habilidade` de sempre. O que muda entre "é seu
// turno" e "só estou olhando" é se ele RESPONDE ao clique, não a cara dele: o jogador não deveria ter
// que reaprender a ler a mesma informação porque mudou de quem está olhando.
function desenharHabilidades(c) {
    const caixa = document.getElementById('habilidades');

    // Clicável só pra quem está agindo E sob seu controle. Todo o resto — aliado parado, inimigo,
    // qualquer um — mostra o mesmo painel, inerte. Esconder o cooldown do inimigo não protegeria
    // nada (quem joga bem decora), só viraria imposto de memória.
    const podeAgir = c.id === estado.quemAge
        && ['EscolhendoAcao', 'EscolhendoAlvo'].includes(nomeDaFase(estado));

    if (!podeAgir) {
        caixa.replaceChildren(...(c.habilidades || []).map(criarBotaoInerte));
        return;
    }

    caixa.replaceChildren(...estado.habilidades.map(h => {
        const armada = habilidadeEscolhida === h.indice;
        // Armada e sem passo de alvo: falta o clique de confirmação — a tela precisa dizer isso.
        const aguardandoConfirmar = armada && !h.pedeAlvo;

        const b = criarBotaoHabilidade(h, aguardandoConfirmar
            ? 'clique de novo ou num alvo pra usar · Esc/fora cancela'
            : h.descricao);
        if (armada) b.classList.add('escolhida');
        if (aguardandoConfirmar) b.classList.add('confirmar');
        b.disabled = !h.disponivel;

        b.addEventListener('click', () => escolherHabilidade(h));
        return b;
    }));
}

// A caixa de uma habilidade no painel. Os dois chamadores mandam formatos diferentes de dado
// (HabilidadeVista pra clicar, HabilidadeDoChampVista pra ler), mas o que a tela desenha vem só do
// que os dois têm em comum: símbolo, nome, cooldown restante e descrição.
function criarBotaoHabilidade(h, descricao) {
    const b = document.createElement('button');
    b.type = 'button';
    b.className = 'habilidade';

    const nome = document.createElement('span');
    nome.className = 'hNome';
    nome.textContent = `${h.simbolo} ${h.nome}` + (h.cooldownRestante > 0 ? ` (${h.cooldownRestante})` : '');

    const desc = document.createElement('span');
    desc.className = 'hDesc';
    desc.textContent = descricao;

    b.append(nome, desc);
    return b;
}

// Mesma caixa, sem clique: é o kit de quem você está OLHANDO. A passiva entra junto — ela é parte do
// que se precisa saber sobre o inimigo — marcada como passiva, porque botão de passiva não existe.
function criarBotaoInerte(h) {
    const b = criarBotaoHabilidade(h, h.descricao);
    b.classList.add('inerte');
    if (h.passiva) {
        b.classList.add('passiva');
        b.querySelector('.hNome').textContent = `${h.simbolo} ${h.nome} · passiva`;
    }
    b.disabled = true;   // inerte de verdade: nem foca pelo teclado
    return b;
}

// ---------- interação ----------
// Clicar numa habilidade NUNCA deve gastar o turno de primeira — o Gabriel quer poder clicar só
// pra olhar e mudar de ideia.
//
// Quem PEDE ALVO já tem esse direito de graça: o clique manda pro C#, que abre a escolha de alvo,
// e o Esc volta atrás. Quem NÃO pede alvo (as Self) disparava na hora, sem volta — então aqui o
// primeiro clique só ARMA (destaca e mostra a descrição) e o segundo é que usa.
function escolherHabilidade(h) {
    if (!h.disponivel) return;

    if (h.pedeAlvo) {
        habilidadeEscolhida = h.indice;
        desenhar();
        mandar('habilidade', h.indice);   // o C# abre a escolha de alvo
        return;
    }

    // Segundo clique na MESMA habilidade = confirmar.
    if (habilidadeEscolhida === h.indice) {
        mandar('habilidade', h.indice);
        return;
    }

    // Primeiro clique (ou troca de ideia pra outra habilidade): só arma.
    habilidadeEscolhida = h.indice;
    desenhar();
}

function clicarEmCombatente(id) {
    const escolhendoAlvo = nomeDaFase(estado) === 'EscolhendoAlvo';
    const ehAlvoValido = (estado.alvosValidos || []).includes(id);

    // Com habilidade em curso e alvo legítimo: o clique EXECUTA.
    if (escolhendoAlvo && ehAlvoValido) { mandar('alvo', id); return; }

    // No AUTOMÁTICO, clicar num inimigo o aponta: o cérebro passa a mirar nele. Clicar no mesmo de
    // novo desfaz. Só faz sentido com o auto ligado — fora dele quem escolhe alvo é você, no passo
    // de alvo. E não substitui a inspeção: a ficha dele abre igual, logo abaixo.
    if (autoLigado && ehInimigo(id)) mandar('foco', estado.focoId === id ? 0 : id);

    // Habilidade armada sem passo de alvo (buff próprio / em aliados): clicar num alvo destacado
    // CONFIRMA — o equivalente ao 2º clique na habilidade, só que apontando quem recebe.
    if (confirmarAtuais.has(id)) { mandar('habilidade', habilidadeEscolhida); return; }

    // Clique fora de qualquer alvo válido: DESARMA (a mesma coisa que o Esc) e mostra a ficha.
    desarmar();
    selecionadoId = id;
    desenhar();
}

// Desfaz a habilidade em curso, seja qual for o lado que a está segurando. É o corpo do Esc, extraído
// porque agora o CLIQUE FORA faz o mesmo — uma regra só pros dois gestos.
//
// Existir foi correção, não gosto: sem isto, clicar noutro combatente com uma habilidade armada
// deixava `habilidadeEscolhida` setada e o painel de habilidades some (ele só desenha pra quem está
// agindo). A habilidade ficava ARMADA E INVISÍVEL, e só o Esc — que ninguém tinha motivo pra apertar,
// já que nada na tela dizia que havia algo armado — desfazia.
//
// Os dois lados desarmam diferente de propósito: no EscolhendoAlvo quem segura o estado é o C# (a
// thread do combate está parada esperando alvo), então precisa do 'cancelar'; no EscolhendoAcao a
// arma é só da tela, o C# nem soube dela — some sozinha.
function desarmar() {
    if (nomeDaFase(estado) === 'EscolhendoAlvo') {
        habilidadeEscolhida = null;
        mandar('cancelar');
        return true;
    }
    if (habilidadeEscolhida !== null) {
        habilidadeEscolhida = null;
        return true;
    }
    return false;
}

const acharCombatente = id => id == null ? null
    : [...(estado?.equipe1 || []), ...(estado?.equipe2 || [])].find(c => c.id === id) || null;

// ---------- sair da tela ----------
// UMA função pra "voltar um nível", e os dois gestos que a disparam: a tecla Esc e o botão 🚪 Sair
// do canto superior direito. Antes cada tela tinha o próprio "Voltar" no rodapé, cada um num lugar
// diferente, e a tecla era a única coisa consistente — agora o botão é o espelho VISÍVEL dela.
//
// O que "um nível" quer dizer muda com a cena, e é aqui que a tabela mora:
//  - modal aberto → cancela o modal (ele é o nível mais raso)
//  - criar perfil → nada (não se sai no meio de digitar o nome; o botão fica escondido)
//  - fim de batalha / vitória / derrota → segue pro menu
//  - submenu e telas de conteúdo → 'voltar' (o C# desempilha; ver LerEscolha)
//  - menu raiz → confirma sair do JOGO, que é o único nível acima
//  - batalha → desarma o que estiver armado; sem nada armado, confirma sair da luta
function sairDaTela() {
    if (modalAberto) { fecharModal(); return; }
    if (cenaAtual === 'criarPerfil') return;

    if (cenaAtual === 'combate' && nomeDaFase(estado || {}) === 'Fim') { mandar('voltarMenu'); return; }

    // Fim de fase: com as opções à mostra, sair é a decisão "Sair" (que faz o mesmo que Editar
    // Equipe — sair desta tela É voltar pra montagem). Na passagem da recompensa, sair é seguir.
    if (cenaAtual === 'fimDeFase') { mandar(fimComOpcoes ? 'voltar' : 'continuar'); return; }

    // A conquista e a ficha dela: sair fecha o champ e devolve o comando ao C#, que segue pro
    // próximo champ novo ou pra tela de vitória.
    if (cenaAtual === 'conquista' || cenaAtual === 'conquistaChamp') { mandar('continuar'); return; }

    if (cenaAtual === 'menu') {
        if (menuRaiz) confirmar('Sair do jogo?', () => mandar('sairDoJogo'));
        else mandar('voltar');
        return;
    }

    if (cenaAtual !== 'combate') { mandar('voltar'); return; }

    if (desarmar()) { desenhar(); return; }

    // Na campanha, desistir NÃO é sair do jogo: conta derrota e cai na tela de fim de fase, de onde
    // dá pra tentar de novo. Na Arena não há desfecho nenhum, então sair é sair. O rótulo e o texto
    // seguem a consequência — duas coisas diferentes não podem ter o mesmo nome.
    if (estado?.modo === 'campanha')
        confirmar('Encerrar a batalha? Conta como DERROTA nesta fase.', () => mandar('sair'));
    else
        confirmar('Sair da batalha? O progresso desta luta será perdido.', () => mandar('sair'));
}

// O botão só some onde sair não é opção (criar perfil). Nas demais ele existe SEMPRE no mesmo pixel —
// é isso que o torna aprendível.
// O X é sempre o mesmo desenho no mesmo pixel — o que muda é o que ele PROMETE, e isso vive no
// title (e no texto do modal). Na batalha da campanha ele encerra a luta em derrota e a tela de fim
// aparece: o jogo continua, então "sair" seria mentira.
function atualizarBotaoSair() {
    const b = document.getElementById('sairTela');
    b.hidden = cenaAtual === 'criarPerfil';

    const encerrando = cenaAtual === 'combate' && estado?.modo === 'campanha'
        && nomeDaFase(estado) !== 'Fim';
    b.title = encerrando ? 'Encerrar a batalha (Esc)' : 'Sair desta tela (Esc)';
}

document.getElementById('sairTela').addEventListener('click', sairDaTela);

document.addEventListener('keydown', e => {
    // Recarregar a página MATA a partida: o JS volta do zero, mas a thread do jogo no C# continua
    // parada esperando um clique que nunca vem. O WebView2 já está com os atalhos de navegador
    // desligados (ver AppFront); isto aqui é o cinto além do suspensório — a tecla não pode passar
    // por caminho nenhum.
    if (e.key === 'F5' || ((e.ctrlKey || e.metaKey) && (e.key === 'r' || e.key === 'R'))) {
        e.preventDefault();
        return;
    }

    // Nas telas de PASSAGEM o Enter também segue em frente (é o gesto natural de "ok, continuar").
    // A tela de fim de fase COM opções fica de fora de propósito: ali cada botão faz uma coisa
    // diferente, e o Enter escolheria uma delas por conta própria.
    const passagem = (cenaAtual === 'combate' && nomeDaFase(estado || {}) === 'Fim')
        || (cenaAtual === 'fimDeFase' && !fimComOpcoes)
        || cenaAtual === 'conquista' || cenaAtual === 'conquistaChamp'
        || cenaAtual === 'compendioChamp';

    if (e.key === 'Escape' || (passagem && e.key === 'Enter')) sairDaTela();
});

// Clique no VAZIO da arena (fora de qualquer combatente) — o outro "clicar em outro lugar". Sem
// isto, só o clique em cima de outro personagem desarmava, e o espaço entre os times não fazia nada.
document.getElementById('arena').addEventListener('click', e => {
    if (e.target.closest('.combatente')) return;   // esse clique já tem dono (clicarEmCombatente)
    if (desarmar()) desenhar();
});

document.getElementById('alternarEstatisticas').addEventListener('click', e => {
    mostrarEstatisticas = !mostrarEstatisticas;
    e.currentTarget.classList.toggle('ativo', mostrarEstatisticas);
    desenhar();
});

// ---------- mostrar/esconder o log ----------
// Escondido, sobra só a arena: dá pra assistir as animações e os números sem ler nada. O log
// SEGUE SENDO ALIMENTADO por trás, então ao reabrir o histórico está inteiro.
let mostrarLog = true;

document.getElementById('alternarLog').addEventListener('click', e => {
    mostrarLog = !mostrarLog;
    e.currentTarget.classList.toggle('ativo', mostrarLog);
    document.getElementById('meio').classList.toggle('oculto', !mostrarLog);

    // Ao reabrir, cai no fim do log (senão volta na posição de quando escondeu).
    if (mostrarLog && coladoNoFim) {
        const log = document.getElementById('log');
        log.scrollTop = log.scrollHeight;
    }
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

// ---------- automático ----------
// Interruptor: liga e o jogo escolhe as jogadas por você; clica de novo e o controle volta. O C#
// lê o estado no começo de cada decisão, então desligar no meio de um turno que o cérebro já
// começou só devolve o controle na PRÓXIMA jogada — igual ao 🚪 sair, que também é lido em ponto
// de espera e não no meio da animação.
//
// O botão NÃO guarda o estado: ele se desenha do `estado.auto` que vem do C# (ver desenhar()). Isso
// é o que mantém os dois em acordo quando o C# desliga o modo sozinho — o que acontece a cada
// batalha nova, pra ninguém entrar numa luta sem o controle que achava que tinha.
let autoLigado = false;

document.getElementById('auto').addEventListener('click', () => {
    autoLigado = !autoLigado;
    aplicarAuto(autoLigado);          // pinta na hora: esperar o próximo estado dá sensação de travado
    mandar('auto', autoLigado ? 1 : 0);
});

function aplicarAuto(ligado) {
    autoLigado = ligado;
    const b = document.getElementById('auto');
    b.classList.toggle('ligado', ligado);
    b.textContent = ligado ? '🤖 auto ON' : '🤖 auto';
}

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

// Zera o log (nova batalha). O histórico não atravessa fases/arenas.
function limparLog() {
    document.getElementById('log').replaceChildren();
    ultimaLinha = null;
    coladoNoFim = true;
}

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

// ---------- tema do campo de batalha ----------
// Cada capítulo pode ter o próprio cenário. O tema entra como `data-tema` no <body> e o CSS faz o
// resto — a ESTRUTURA da luta não muda, só a pele. É o mesmo princípio do painel de habilidades:
// uma tela, uma forma; o que varia é a roupa.
//
// Aqui em cima mora só o que o CSS não sabe fazer: as partículas do ar. Um capítulo sem entrada
// nesta tabela fica sem partículas e ainda assim ganha a pele do CSS, se ela existir — as duas
// metades do tema são independentes de propósito, pra nenhuma delas exigir a outra.
// O AR de cada cenário, em três camadas OPCIONAIS e independentes — um tema usa as que fizerem
// sentido pra ele e ignora o resto:
//   pó      · partículas pequenas. `subida` em px/s: positivo SOBE, negativo CAI. A direção é o
//             SINAL da velocidade, não um campo à parte que se pode esquecer de casar com ela.
//   névoa   · manchas grandes e translúcidas passeando devagar, coladas no chão.
//   voadores· bichos atravessando a tela, com asas.
const AR_DO_TEMA = {
    // A cidade murada sob cerco: a muralha e o castelo ao fundo, os dois exércitos trocando tiro nas
    // bordas, e a poeira dourada das tochas subindo.
    reino: {
        // A PAISAGEM é a coisa mais distante — por isso é a primeira do fundo (ver iniciarAr): os
        // exércitos atiram na frente dela, não por cima.
        castelo: {
            pedra: '#bcc2d2', sombra: '#8e95ad', telhado: '#b0453c', telhadoAlt: '#3f6ea8',
            grama: '#5d8c46', gramaSombra: '#4a7539', janela: '#2c3452', bandeira: '#d9b45b',
            morro: '#7fa4a8', nuvem: '255, 255, 255',
            muro: .3, torre: .46, casas: 11, nuvens: 6, vento: 5,
        },
        po: { cor: '217, 180, 91', quantas: 46, subida: [4, 14], raio: [0.6, 2.2], opacidade: [.12, .5] },
        // O 🥷 correndo pelos telhados. O corpo NÃO é preto: contra o céu de dia o preto chapado lê
        // como recorte de papel (a mesma lição que tirou o preto dos exércitos). Este azul quase
        // preto lê como preto e ainda fica dentro do quadro.
        ninja: {
            corpo: '#1a1f33', faixa: '26, 31, 51', fumaca: '240, 244, 250',
            // Em FRAÇÃO da altura da arena, e não em px: o castelo inteiro escala com ela, e ele
            // precisa escalar junto ou deixa de caber em cima da torre. Ver o `remontar`.
            tamanho: .025, velocidade: 1, arco: .11, fumacaRaio: .062,
            // Ele passa MUITO mais tempo FORA do que em cena — cerca de três por um. É o que o
            // mantém sendo uma aparição: um ninja sempre visível correndo no telhado vira mascote
            // do castelo, e o castelo é o cenário, não ele. `emCena` é quanto ele dura por visita
            // (quando zera, some na primeira borda que encontrar); `fora` é o tempo sumido.
            emCena: [4, 9], sumir: .28, fora: [12, 26], surgir: .3,
            // Com que frequência a visita termina em FUGA pela lateral (o resto termina na bomba de
            // fumaça). Metade e metade: as duas saídas são boas, e alternar impede que qualquer uma
            // vire tique. A entrada é sempre pela fumaça — chegar tem que ter um só jeito.
            sairPelaBorda: .5,
            // Com ele menor e mais rápido, o RASTRO passa a ser o que se lê de longe — é ele que
            // ocupa a tela, não o boneco. Por isso ficou longo e grosso: a leitura agora é "uma
            // sombra correndo no telhado", e a figura só confirma de perto.
            rastro: 18, bolhas: 9, fumacaDura: .9,
        },
        exercitos: {
            // De dia a silhueta preta não serve mais: contra o céu claro ela lia como recorte de
            // papel. Agora cada coisa tem o MATERIAL dela — o aço reflete (por isso é gradiente,
            // não cor chapada), a madeira é fosca, o couro é escuro e o bronze é o detalhe caro.
            aco: '#dfe6f2', acoSombra: '#7c8aa6', madeira: '#7a5231', couro: '#3c2a1c',
            bronze: '#c9a227', flecha: '230, 218, 184',
            espada: 168, escudo: 96, escudos: 3, lanca: 196, lancas: 4, tamanhoFlecha: 2,
            // O 2º número: magia. Mesma coreografia (gesto → voo → defesa), outro vocabulário.
            cajado: 176, bola: 30, esfera: 230, explosao: .85,
            // A esfera era AZUL-CLARA e sumia contra o céu de dia — agora o tema é diurno, e violeta
            // é a cor que mais se afasta de um céu azul sem virar outra coisa.
            fogo: '255, 168, 66', brasa: '255, 242, 208', magia: '198, 130, 255',
            arco: .6,                               // altura do voo, em fração da tela
            volei: [5, 8], voo: 1.75, intervalo: .09,
            espera: [2.4, 5.2], gesto: .9, guarda: .6, recolher: .9,
        },
    },
    // Cemitério: cinza caindo, névoa no chão, morcegos cruzando o céu e corujas nas árvores.
    ladosombrio: {
        po: { cor: '178, 205, 186', quantas: 34, subida: [-9, -3], raio: [0.5, 2.0], opacidade: [.08, .3] },
        nevoa: { cor: '150, 190, 170', quantas: 7, deriva: [6, 20], raio: [140, 320], opacidade: [.03, .08] },
        // Aqui NÃO há `voadores`. Os fantasmas do cemitério passeiam pela tela como sempre, mas
        // agora eles NASCEM do caixão em vez de entrarem pela borda — ver `caixao` mais abaixo.
        // Entrar pela borda dizia que vinham de fora; saindo da cova, o cemitério passa a ter uma
        // origem, e a peça do meio deixa de ser um enfeite pra virar a causa do resto.
        // (O morcego segue guardado pros 🔱 Decaídos, que é onde ele pertence.)
        // As corujas nas árvores. `poleiros` são posições DENTRO do ladrilho da mata, em fração —
        // assim elas seguem o ladrilho quando ele muda de tamanho, e cada árvore desenhada no SVG
        // ganha a sua sem ninguém recontar pixel. `lado` encosta a coruja num flanco do tronco: uma
        // à esquerda, a outra à direita, pra as duas árvores não ficarem iguais.
        //
        // O olho fica APAGADO a maior parte do tempo, e o tempo ACESO é sempre o mesmo — é o que faz
        // parecer bicho olhando de vez em quando, e não lâmpada piscando. O que varia é QUANDO cada
        // uma abre: o tempo apagado é sorteado a cada ciclo, então elas nunca casam.
        corujas: {
            corpo: '#060f0b', olho: '236, 246, 205', tamanho: 11,
            ladrilho: ['--mata-passo', '--mata-altura'],
            // Tirados do próprio SVG da mata (ladrilho 320×190): o tronco grande fica em x≈47 e o
            // menor em x≈234, daí .148 e .733. Antes eram valores ajustados a olho, que estavam
            // compensando o ladrilho ancorado no centro — com a âncora consertada, a conta fecha.
            pontos: [{ x: .148, y: .24, lado: -1 }, { x: .733, y: .30, lado: 1 }],
            aceso: 1.7, apagado: [5, 17], acordar: [0, 13],
        },
        // Os espantalhos fincados no meio das lápides. Mesmo motor das corujas — posição vinda do
        // ladrilho — só que SEM relógio: eles não piscam, estão sempre lá. É a ausência de `aceso`
        // que diz isso; nenhuma configuração extra foi precisa.
        espantalhos: {
            // O pano é PANO, não sombra: um marrom-ferrugem gasto. Estava quase preto e sumia contra
            // o mato — e um espantalho que não se vê não assusta ninguém. Ele e a abóbora são as
            // únicas coisas quentes do cemitério, e conversam entre si sem competir: o pano é escuro
            // e terroso, a cabeça é clara e acesa.
            poste: '#050b08', pano: '#5c3623', tamanho: 40,
            // A abóbora é o único ponto de COR do cemitério inteiro. Baixa saturação de propósito:
            // laranja aceso aqui brigaria com a lua e roubaria a cena.
            abobora: '148, 84, 32', cara: '255, 172, 66',
            ladrilho: ['--mata-passo', '--mata-altura'],
            // UM por ladrilho, fincado no chão (y ≈ .95 é quase o rodapé), no vão entre a cruz e a
            // árvore menor (x≈196) — onde o SVG não desenhou nada. Dois por ladrilho enchiam demais:
            // espantalho é figura solitária, e repetido de perto vira plantação.
            pontos: [{ x: .613, y: .95 }],
        },
        // O 💀 saindo da cova, no meio do cemitério — a única peça do tema que ACONTECE em vez de só
        // estar lá. O que sai dele são fantasmas; ver o doc do `criarCaixao`.
        caixao: {
            madeira: '#2a1d16', ferro: '#12100e', dentro: '#040706', terra: '#0b120e',
            // O mesmo verde espectral do pó, da névoa e dos fantasmas que já cruzam o céu: a luz
            // nova tinha que pertencer ao cemitério que já existe, não trazer uma paleta própria.
            brilho: '178, 255, 214',
            // `cor` (e não um nome próprio) porque é o que o `desenharFantasma` já espera receber —
            // é o mesmo desenho dos voadores, reusado inteiro.
            cor: '208, 238, 222',
            // Fração da altura da arena. Encolheu (era .72) pra abrir espaço pros fantasmas: com o
            // caixão menor, o que ocupa a cena passa a ser o que SAI dele, que é o ponto.
            // Metade do que era (.30 × .54). Com os fantasmas passeando pela tela o tempo todo, o
            // caixão não precisa mais carregar a cena sozinho — ele virou a FONTE deles, e fonte
            // grande demais rouba a atenção do que sai dela.
            largura: .15, altura: .27, canto: 4,
            // Os caixõezinhos em volta: `x` em múltiplos da largura do grande, `y` em fração da
            // altura dele, `giro` em radianos e `atraso` no quanto cada um demora a romper o chão.
            // Tortos e em alturas diferentes de propósito — enfileirados e retos, virariam cerca.
            menorTamanho: .095,
            menores: [
                { x: -2.6, y: .06, giro: -.38, atraso: .5 },
                { x: -1.5, y: .10, giro: .26, atraso: 0 },
                { x: 1.6, y: .09, giro: -.3, atraso: .25 },
                { x: 2.7, y: .05, giro: .44, atraso: .7 },
            ],
            // O raio do clarão, em MÚLTIPLOS da largura do caixão. É a única alavanca do quanto a
            // luz vaza pelos lados do log (~300px de coluna): com o caixão menor, `2.4` deixava a
            // luz morrer justo na borda dele. Aumentar isto é o ajuste se ainda parecer escondido.
            clarao: 5.5,
            espera: [10, 20], revirar: .8, subir: 2.4, abrir: 1.1,
            fechar: .9, descer: 2.2, assentar: 1.3,
            // A leva. `subida` e `tamanho` são fração da altura da arena, como todo o resto.
            // A LEVA. Três por abertura, e cada fantasma dura DOIS ciclos do caixão: a leva 1 sai, a
            // leva 2 sai (seis na tela), e quando a 3 vem a 1 já está se apagando. É o rodízio que
            // segura o teto em 6 sem ninguém contar nada — o `maximo` é só rede de segurança.
            porLeva: 3, maximo: 6, intervalo: .5,
            acender: .6, apagar: 2.2,
            // `subida` e `deriva` são FAIXAS e não valores únicos: fantasmas todos na mesma
            // velocidade leem como um efeito só. `assumir` é em quanto tempo a deriva lateral toma
            // o lugar do impulso de saída — é o que liga "saiu do caixão" a "passeia pela tela".
            subida: [.05, .11], deriva: [.02, .05], assumir: 2.4, tamanho: .058,
            // A faixa de altitude que eles percorrem, em fração da altura da arena: de quase
            // encostado no topo (.06) até a metade de baixo (.66). `mudarAltura` é de quanto em
            // quanto tempo cada um escolhe outra, e `buscarAltura` o quanto ele demora a chegar lá
            // — lento de propósito, senão a troca vira teleporte vertical em vez de voo.
            //
            // `altitude`, e NÃO `altura`: esta config já tem uma `altura`, que é a do caixão. Chamar
            // as duas de `altura` no mesmo objeto não dá erro nenhum em JS — a segunda simplesmente
            // apaga a primeira, e o caixão inteiro vira NaN.
            altitude: [.06, .66], mudarAltura: [4, 10], buscarAltura: 5,
        },
    },
    // A noite da invasão, montada em cima de quem luta aqui: 👽/👾 nos discos que cruzam o céu
    // varrendo o chão com o feixe, 🧑‍🔬 nas bobinas do laboratório soltando faísca, 🤖 nas fagulhas
    // de solda subindo. Nenhuma peça é mecanismo novo — são as MESMAS camadas dos outros dois com
    // outro vocabulário, que era o teste de verdade do seam.
    tecnologicos: {
        po: { cor: '126, 255, 190', quantas: 38, subida: [12, 34], raio: [0.5, 1.7], opacidade: [.14, .55] },
        voadores: {
            forma: 'disco', cor: '#0a1622', luz: '126, 255, 190',
            quantos: 2, velocidade: [26, 52], tamanho: [26, 40], intervalo: [3, 14],
        },

        // As bobinas de Tesla do horizonte. Mesmo motor das corujas — posição vinda do ladrilho,
        // relógio próprio, muito tempo apagada — só que o que acende é um raio, não um olho.
        bobinas: {
            cor: '170, 245, 255', tamanho: 15,
            ladrilho: ['--lab-passo', '--lab-altura'],
            pontos: [{ x: .115, y: .22 }, { x: .858, y: .315 }],
            aceso: .5, apagado: [3, 11], acordar: [0, 7],
        },
        // O reator estourado no meio da cidade, queimando e vazando. Fica no canvas e NÃO no
        // ladrilho da cidade porque desastre não se repete a cada 340px — repetido, ele viraria
        // padrão de papel de parede em vez de acontecimento.
        ruina: {
            silhueta: '#050b14', fogo: '255, 146, 56', brasa: '255, 236, 190',
            // O veneno tem os DOIS tons pelo mesmo motivo que o fogo tem: um claro pro núcleo e um
            // saturado pro corpo. É o par que faz a coisa parecer luminosa em vez de pintada.
            veneno: '138, 255, 150', venenoClaro: '226, 255, 228',
            // Em FRAÇÃO da altura da arena, não mais em px fixos (era 230×128). Passou a ser fração
            // pelo mesmo motivo do caixão e do ninja: crescendo, ela deixa de ser baixa o bastante
            // pra escapar — 218px fixos numa arena de 260 seriam quase a tela inteira.
            //
            // Cresceu SÓ EM ALTURA — a largura até ENCOLHEU um pouco (era .548 em fração). A
            // proporção foi de 1.80 pra 0.94, e é ela que importa aqui: o reator estava lendo como
            // entulho espalhado no chão, e o que conserta isso é ele ficar de pé, não ficar grande.
            // Alargar junto foi um erro — largo e alto ao mesmo tempo, ele virou um paredão.
            largura: .35, altura: .5, labaredas: 7,
        },
        // O 👽/👾 INVASOR baixando do céu. Roxo, que é a cor dele — e é a única coisa fora do verde
        // num tema que até aqui era todo verde. O contraste é o ponto: quando ele desce, a cena troca
        // de cor, e isso anuncia a chegada antes de qualquer forma ficar legível.
        tentaculos: {
            corpo: '#2e1145', corpoClaro: '#4d2073', escuro: '30, 8, 46',
            brilho: '198, 130, 255',
            // Nove braços cobrindo 78% da largura, o do meio o mais longo e o mais grosso. `talo` é
            // fração da LARGURA (a grossura na raiz) e `alcance` fração da ALTURA (até onde a ponta
            // desce). Os dois em fração pelo mesmo motivo do resto: a cena escala com a janela.
            quantos: 9, largura: .78, alcance: .66, talo: .022,
            // O quanto da altura o véu escuro do topo cobre. É o corpo — o único jeito em que ele
            // aparece, e sem forma nenhuma de propósito (ver o desenho).
            sombra: .3,
            // `atraso` é o quanto do evento os braços das PONTAS esperam antes de começar a descer.
            // Zero faria os nove entrarem em bloco, que lê como cortina em vez de bicho.
            espera: [16, 30], descer: 4.5, pairar: [4, 8], subir: 3.6, atraso: .38,
            ondular: [.6, 1.3], onda: [12, 30],
        },
    },
};

let temaAtual = '';

function aplicarTema(tema) {
    if (tema === temaAtual) return;   // o estado chega dezenas de vezes por turno; só reage à TROCA
    temaAtual = tema;

    if (tema) document.body.dataset.tema = tema;
    else document.body.removeAttribute('data-tema');

    iniciarAr(AR_DO_TEMA[tema]);
}

// ---------- o ar do cenário (canvas) ----------
// Canvas atrás de tudo (z -1), rodando SÓ enquanto há tema com ar. Sem tema o laço é cancelado e o
// canvas escondido: cenário nenhum não pode custar quadro nenhum.
//
// Tudo aqui anda por DELTA DE TEMPO (`* dt`), nunca por quadro — assim a cena tem a mesma velocidade
// num monitor de 60Hz e num de 144Hz.
let arFrame = null;

const entre = ([min, max]) => min + Math.random() * (max - min);

function iniciarAr(config) {
    const telas = [document.getElementById('particulasFundo'), document.getElementById('particulas')];

    if (arFrame !== null) { cancelAnimationFrame(arFrame); arFrame = null; }
    for (const t of telas) t.hidden = !config;
    if (!config) return;

    const [fundo, frente] = telas;
    const ctxFundo = fundo.getContext('2d');
    const ctxFrente = frente.getContext('2d');

    const dimensionar = () => {
        for (const t of telas) { t.width = t.clientWidth; t.height = t.clientHeight; }
    };
    dimensionar();

    // Cada camada declara em QUE MUNDO ela vive, não em que canvas — o roteamento é consequência.
    // A ORDEM É A PROFUNDIDADE. A paisagem vem primeiro por ser a coisa mais distante: o que
    // ACONTECE (bichos, incêndio, os exércitos atirando) passa na frente dela, nunca por baixo — foi
    // o que o Gabriel pediu com "a paisagem deve ficar entre as animações". E a névoa fecha a fila,
    // porque ela é ar: some tudo que está atrás dela, um pouco.
    const noFundo = [
        config.castelo && criarCastelo(config.castelo, fundo),
        // O ninja vem logo DEPOIS do castelo: ele anda em cima dos telhados que o castelo acabou de
        // desenhar, e é do castelo que ele tira a geometria — por isso recebe as duas configurações.
        config.ninja && config.castelo && criarNinja(config.ninja, fundo, config.castelo),
        config.corujas && criarCorujas(config.corujas, fundo),
        config.espantalhos && criarEspantalhos(config.espantalhos, fundo),
        config.caixao && criarCaixao(config.caixao, fundo),
        config.bobinas && criarBobinas(config.bobinas, fundo),
        config.ruina && criarRuina(config.ruina, fundo),
        // O Invasor vem DEPOIS da ruína e da cidade: ele desce por cima delas, que é o lugar certo
        // — chegou depois. E fica no fundo, atrás dos combatentes, porque um bicho desse tamanho na
        // tela da frente taparia a luta.
        config.tentaculos && criarTentaculos(config.tentaculos, fundo),
        config.exercitos && criarExercitos(config.exercitos, fundo),
        config.nevoa && criarNevoa(config.nevoa, fundo),
    ].filter(Boolean);

    const naFrente = [
        config.po && criarPo(config.po, frente),
        config.voadores && criarVoadores(config.voadores, frente),
    ].filter(Boolean);

    let anterior = performance.now();
    const quadro = (agora) => {
        const dt = Math.min((agora - anterior) / 1000, .05);   // janela minimizada não deve dar salto
        anterior = agora;

        if (fundo.width !== fundo.clientWidth || fundo.height !== fundo.clientHeight) dimensionar();
        ctxFundo.clearRect(0, 0, fundo.width, fundo.height);
        ctxFrente.clearRect(0, 0, frente.width, frente.height);

        for (const camada of noFundo) camada(ctxFundo, dt);
        for (const camada of naFrente) camada(ctxFrente, dt);

        arFrame = requestAnimationFrame(quadro);
    };
    arFrame = requestAnimationFrame(quadro);
}

/// Partículas pequenas subindo ou caindo, com vaivém horizontal pra não andarem em linha reta.
function criarPo(cfg, canvas) {
    // Sobe ou cai? Sai do SINAL da velocidade, e daí saem também a borda em que a partícula nasce e
    // aquela em que ela é reciclada — três coisas que teriam que concordar se fossem configuradas
    // separado, e que assim não têm como discordar.
    const sobe = cfg.subida[1] > 0;

    const nova = (yInicial) => ({
        x: Math.random() * canvas.width,
        y: yInicial ?? (sobe ? canvas.height + 10 : -10),
        r: entre(cfg.raio),
        vy: entre(cfg.subida),
        deriva: (Math.random() - .5) * 12,
        fase: Math.random() * Math.PI * 2,
        alfa: entre(cfg.opacidade),
    });

    // Na PRIMEIRA leva nasce espalhada pela tela inteira, e não na borda: senão o ar começa
    // parecendo um enxame entrando de uma vez.
    let grãos = Array.from({ length: cfg.quantas }, () => nova(Math.random() * canvas.height));

    return (ctx, dt) => {
        for (let i = 0; i < grãos.length; i++) {
            const p = grãos[i];
            p.y -= p.vy * dt;
            p.fase += dt;
            if (sobe ? p.y < -10 : p.y > canvas.height + 10) grãos[i] = nova();

            ctx.beginPath();
            ctx.arc(p.x + Math.sin(p.fase) * p.deriva, p.y, p.r, 0, Math.PI * 2);
            ctx.fillStyle = `rgba(${cfg.cor}, ${p.alfa})`;
            ctx.fill();
        }
    };
}

/// Manchas grandes e translúcidas passeando de lado, agarradas ao chão. São poucas e enormes de
/// propósito: névoa é uma massa que se move, não um monte de bolinhas.
function criarNevoa(cfg, canvas) {
    const nova = (xInicial) => ({
        x: xInicial ?? -entre(cfg.raio),
        y: canvas.height * (0.72 + Math.random() * 0.3),   // rente ao chão
        r: entre(cfg.raio),
        vx: entre(cfg.deriva),
        alfa: entre(cfg.opacidade),
    });

    let bancos = Array.from({ length: cfg.quantas }, () => nova(Math.random() * canvas.width));

    return (ctx, dt) => {
        for (let i = 0; i < bancos.length; i++) {
            const n = bancos[i];
            n.x += n.vx * dt;
            if (n.x - n.r > canvas.width) bancos[i] = nova();

            const g = ctx.createRadialGradient(n.x, n.y, 0, n.x, n.y, n.r);
            g.addColorStop(0, `rgba(${cfg.cor}, ${n.alfa})`);
            g.addColorStop(1, `rgba(${cfg.cor}, 0)`);
            ctx.fillStyle = g;
            ctx.beginPath();
            // achatada: névoa se espalha no chão, não sobe em bola
            ctx.ellipse(n.x, n.y, n.r, n.r * 0.42, 0, 0, Math.PI * 2);
            ctx.fill();
        }
    };
}

/// Bichos atravessando a tela. Aparecem em levas, não em fila: cada um espera um tempo sorteado
/// antes de entrar, e sorteia de novo ao sair — é o que faz parecer que passou um bando, em vez de
/// uma esteira de morcegos saindo de um cano.
function criarVoadores(cfg, canvas) {
    const novo = (primeiraVez) => {
        const paraDireita = Math.random() < .5;
        return {
            paraDireita,
            x: paraDireita ? -60 : canvas.width + 60,
            y: canvas.height * (0.08 + Math.random() * 0.42),   // voam na parte de cima
            vx: entre(cfg.velocidade) * (paraDireita ? 1 : -1),
            tamanho: entre(cfg.tamanho),
            bobo: entre([14, 34]),                 // amplitude do sobe-e-desce
            fase: Math.random() * Math.PI * 2,
            asa: Math.random() * Math.PI * 2,
            velocidadeDaAsa: entre([9, 15]),
            // A primeira leva espera pouco, pra a cena não começar vazia por 10 segundos.
            espera: primeiraVez ? Math.random() * 3 : entre(cfg.intervalo),
        };
    };

    let bando = Array.from({ length: cfg.quantos }, () => novo(true));

    return (ctx, dt) => {
        for (let i = 0; i < bando.length; i++) {
            const m = bando[i];
            if (m.espera > 0) { m.espera -= dt; continue; }

            m.x += m.vx * dt;
            m.fase += dt * 2.2;
            m.asa += dt * m.velocidadeDaAsa;

            const saiu = m.vx > 0 ? m.x > canvas.width + 60 : m.x < -60;
            if (saiu) { bando[i] = novo(false); continue; }

            // A FORMA é do tema: um motor de voo só, e o desenho é que muda. O fantasma do
            // cemitério, o disco da invasão e o morcego (guardado pros 🔱 Decaídos) atravessam a
            // tela pela mesma conta.
            const y = m.y + Math.sin(m.fase) * m.bobo;
            VOADORES[cfg.forma ?? 'morcego'](ctx, m.x, y, m.tamanho, m.asa, m.paraDireita, canvas, cfg);
        }
    };
}

/// Os DOIS EXÉRCITOS ao longe, trocando rajadas de flecha por cima do campo.
///
/// A cena é um DIRETOR com uma fase por vez, e um lado atacando por vez (ideia do Gabriel: "não
/// precisa necessariamente jogar as flechas ao mesmo tempo"). Alternar sai de graça e é o que dá a
/// leitura de conversa — ação e resposta — em vez de dois efeitos rodando lado a lado:
///
///   espera → o líder do atacante ERGUE A ESPADA → solta a rajada → o defensor LEVANTA OS ESCUDOS
///   pouco antes de as flechas chegarem → elas batem e caem → escudos abaixam → troca o lado.
///
/// Nenhuma fase tem duração adivinhada: cada uma termina quando a anterior acabou de fato (a
/// última flecha bater, por exemplo). Cronometrar "mais ou menos" daria dessincronizar em máquina
/// lenta — e o escudo subiria depois da flecha chegar.
function criarExercitos(cfg, canvas) {
    // 1 = a esquerda ataca; -1 = a direita ataca. Quem defende é sempre o outro.
    let atacante = Math.random() < .5 ? 1 : -1;

    // O NÚMERO desta rodada: 'aço' (espada → flechas → escudos e lanças) ou 'magia' (cajado → bola
    // de fogo → esfera). A coreografia é a MESMA — gesto, voo, defesa; o que troca é o vocabulário
    // desenhado em cada momento. Foi por isso que o 2º ataque não pediu um segundo diretor: as
    // fases já falavam de "o gesto" e "a defesa", não de espada e escudo.
    let numero = Math.random() < .5 ? 'aço' : 'magia';
    let fase = 'espera';
    // A PRIMEIRA espera é curta e fixa, não sorteada: a batalha tem que abrir mostrando a cena
    // inteira — espada, flecha, escudo, nessa ordem. Com a espera normal (até 5s), quem entrasse na
    // luta veria um campo parado e só depois entenderia que há algo acontecendo ali.
    let relogio = .5;
    let flechas = [];
    let explosoes = [];

    // Onde o voo ACABA. A flecha some além da borda (some no meio do exército que não se vê); a bola
    // de fogo para ANTES, em cima da esfera — ela tem que estourar onde o defensor está, não fora da
    // tela. É por isso que "bateu" é um flag e não `p >= 1`: cada número bate num lugar.
    const impacto = () => numero === 'magia' ? .93 : 1;

    // 0..1, o quanto cada gesto está completo. Ficam FORA da fase pra poderem descer suavemente
    // enquanto a fase seguinte já corre — é o que evita o gesto "sumir" no corte.
    let espada = 0, escudo = 0;

    const chao = () => canvas.height + 4;                       // um fio abaixo da borda
    const bordaDe = (lado) => lado > 0 ? 0 : canvas.width;      // de que lado da tela o exército está

    // A parábola: sai de FORA da tela, sobe até `arco` da altura no meio do caminho, e cai fora da
    // tela do outro lado. Começar e terminar além da borda é o que faz parecer que há um exército
    // ali, em vez de flecha nascendo do nada num ponto visível.
    const posicao = (p) => {
        const x0 = bordaDe(atacante) - atacante * 40;
        const x1 = bordaDe(-atacante) + atacante * 40;
        const y0 = canvas.height * .82;
        const altura = canvas.height * cfg.arco;
        return {
            x: x0 + (x1 - x0) * p,
            y: y0 - altura * 4 * p * (1 - p),
        };
    };

    const soltarRajada = () => {
        // MAGIA é um tiro só, grande e lento; FLECHA é uma nuvem deles. A diferença de contagem é o
        // que separa os dois números sem precisar de dois diretores.
        const quantas = numero === 'magia' ? 1 : Math.round(entre(cfg.volei));
        flechas = Array.from({ length: quantas }, (_, i) => ({
            atraso: i * cfg.intervalo,
            p: 0,
            desvio: numero === 'magia' ? 0 : (Math.random() - .5) * 26,   // nenhuma flecha sai igual
            ritmo: numero === 'magia' ? .72 : 1 + (Math.random() - .5) * .12,
        }));
    };

    /// Quanto falta pra a PRIMEIRA flecha chegar. É o que diz a hora de levantar o escudo — o
    /// defensor reage ao que vê, não a um cronômetro paralelo.
    const faltaPraChegar = () => {
        let menor = Infinity;
        for (const f of flechas) {
            if (f.bateu) continue;
            menor = Math.min(menor, f.atraso + (impacto() - f.p) * cfg.voo / f.ritmo);
        }
        return menor;
    };

    return (ctx, dt) => {
        relogio -= dt;

        switch (fase) {
            case 'espera':
                if (relogio <= 0) { fase = 'espada'; relogio = cfg.gesto; }
                break;

            case 'espada':
                espada = Math.min(1, espada + dt / (cfg.gesto * .5));
                if (relogio <= 0) { soltarRajada(); fase = 'voo'; }
                break;

            case 'voo':
                espada = Math.max(0, espada - dt / (cfg.gesto * .5));   // a espada baixa junto do disparo
                if (faltaPraChegar() < cfg.guarda) escudo = Math.min(1, escudo + dt / (cfg.guarda * .7));
                if (flechas.every(f => f.bateu)) { fase = 'recolher'; relogio = cfg.recolher; }
                break;

            case 'recolher':
                if (relogio <= 0) {
                    escudo = Math.max(0, escudo - dt / (cfg.recolher * .5));
                    if (escudo === 0) {
                        atacante = -atacante;
                        numero = Math.random() < .5 ? 'aço' : 'magia';   // cada troca sorteia o seu
                        fase = 'espera';
                        relogio = entre(cfg.espera);
                    }
                }
                break;
        }

        // --- o que está voando ---
        for (const f of flechas) {
            if (f.atraso > 0) { f.atraso -= dt; continue; }
            if (f.bateu) continue;

            const fim = impacto();
            f.p = Math.min(fim, f.p + dt / cfg.voo * f.ritmo);

            const aqui = posicao(f.p);
            const y = aqui.y + f.desvio * (1 - Math.abs(f.p - .5) * 2);

            if (f.p >= fim) {
                f.bateu = true;
                // A bola de fogo não some: ela vira a explosão, no ponto exato em que parou.
                if (numero === 'magia') explosoes.push(criarExplosao(aqui.x, y));
                continue;
            }

            if (numero === 'magia') {
                desenharBolaDeFogo(ctx, aqui.x, y, f.p, posicao, cfg);
            } else {
                // A inclinação sai da própria trajetória (olha um passo à frente), então a flecha
                // aponta pra onde vai: sobe de bico pra cima, desce de bico pra baixo.
                const adiante = posicao(Math.min(1, f.p + .02));
                desenharFlecha(ctx, aqui.x, y,
                    Math.atan2(adiante.y - aqui.y, adiante.x - aqui.x), cfg);
            }
        }

        // --- o estouro ---
        // Vive fora do laço das flechas de propósito: ele COMEÇA quando uma acaba, e precisa seguir
        // queimando enquanto a esfera já está baixando.
        for (const e of explosoes) e.t += dt / cfg.explosao;
        explosoes = explosoes.filter(e => e.t < 1);
        for (const e of explosoes) desenharExplosao(ctx, e, cfg);

        // --- o gesto de cada lado, entrando pela borda ---
        // Ninguém aparece: o que se vê é só o que ENTRA em cena, como se o exército estivesse logo
        // fora do quadro. Desenhar soldados obrigaria a desenhá-los bem, e um boneco mal resolvido
        // no canto rouba mais atenção do que uma silhueta que sugere.
        if (espada > 0) {
            const erguer = numero === 'magia' ? desenharCajado : desenharEspada;
            erguer(ctx, bordaDe(atacante), chao(), atacante, espada, cfg);
        }
        if (escudo > 0) {
            const defender = numero === 'magia' ? desenharEsfera : desenharDefesa;
            defender(ctx, bordaDe(-atacante), chao(), -atacante, escudo, cfg);
        }
    };
}

/// A inclinação de tudo que é erguido: espada e lanças partilham este ângulo, e é o que faz os dois
/// lados parecerem o mesmo exército em vez de dois efeitos separados.
const INCLINACAO = .22;

/// A espada erguida pelo atacante: sobe da borda de baixo, inclinada pra dentro do campo. `subida`
/// vai de 0 a 1 e é o quanto dela já entrou em cena.
function desenharEspada(ctx, borda, chao, lado, subida, cfg) {
    const h = cfg.espada;

    ctx.save();
    // Entra POR BAIXO da borda: em subida 0 ela está inteira fora, em 1 está no alto.
    ctx.translate(borda + lado * h * .38, chao + h * (1 - subida));
    ctx.rotate(lado * INCLINACAO);
    ctx.scale(lado, 1);

    // A LÂMINA em gradiente: aço não é uma cor, é um reflexo. Claro na aresta que pega o sol e
    // escuro na outra metade — é o degradê ATRAVESSADO que faz um retângulo virar metal.
    const lamina = ctx.createLinearGradient(-h * .035, 0, h * .035, 0);
    lamina.addColorStop(0, cfg.acoSombra);
    lamina.addColorStop(.42, cfg.aco);
    lamina.addColorStop(.55, '#ffffff');
    lamina.addColorStop(1, cfg.acoSombra);
    ctx.fillStyle = lamina;
    ctx.fillRect(-h * .035, -h, h * .07, h * .78);
    ctx.beginPath();                                         // ponta
    ctx.moveTo(-h * .035, -h); ctx.lineTo(0, -h * 1.09); ctx.lineTo(h * .035, -h);
    ctx.closePath(); ctx.fill();

    ctx.fillStyle = cfg.bronze;
    ctx.fillRect(-h * .17, -h * .24, h * .34, h * .055);    // guarda
    ctx.fillStyle = cfg.couro;
    ctx.fillRect(-h * .028, -h * .19, h * .056, h * .17);   // punho
    ctx.fillStyle = cfg.bronze;
    ctx.beginPath();                                         // pomo
    ctx.arc(0, -h * .015, h * .045, 0, Math.PI * 2);
    ctx.fill();

    ctx.restore();
}

/// A DEFESA do outro lado: as lanças sobem primeiro (elas são o fundo) e os escudos por cima,
/// tapando o pé delas — é a ordem de desenho que monta a falange, sem ninguém precisar existir.
///
/// Cada peça começa a subir um tiquinho depois da anterior, então a fileira levanta em ONDA e não
/// em bloco: é o escalonamento que faz ler como vários, e não como uma parede só.
function desenharDefesa(ctx, borda, chao, lado, subida, cfg) {
    // Um escalonamento só pros dois, pra a lança e o escudo da mesma posição subirem juntos.
    const naVez = (i) => Math.max(0, Math.min(1, subida * 1.3 - i * .13));

    // O `-.1` (era `.2`) tira a lança mais de DENTRO e joga a fileira um passo pra trás — a nova
    // entra na borda, meio saindo de cena, que é onde o resto do exército estaria.
    const hl = cfg.lanca;
    for (let i = 0; i < cfg.lancas; i++) {
        desenharLanca(ctx, borda + lado * (hl * -.1 + i * hl * .3), chao + hl * (1 - naVez(i)),
            hl, lado, i, cfg);
    }

    const he = cfg.escudo;
    for (let i = 0; i < cfg.escudos; i++) {
        // Sem `globalAlpha` aqui: escudo de metal semitransparente parece vidro. A profundidade da
        // fileira vem do escalonamento e da sobreposição, não de apagar os de trás.
        desenharEscudo(ctx, borda + lado * (he * .38 + i * he * .62), chao + he * (1 - naVez(i)),
            he, cfg);
    }

    ctx.globalAlpha = 1;
}

/// Uma lança: haste comprida e ponta em folha, na MESMA inclinação da espada — com uma variação
/// mínima por lança, senão as quatro viram uma listra só.
function desenharLanca(ctx, x, base, h, lado, indice, cfg) {
    ctx.save();
    ctx.translate(x, base);
    ctx.rotate(lado * (INCLINACAO + (indice - 1.5) * .035));

    // haste de MADEIRA: fosca, com um fio mais claro de um lado só (a luz batendo no cilindro)
    ctx.fillStyle = cfg.madeira;
    ctx.fillRect(-h * .013, -h, h * .026, h);
    ctx.fillStyle = 'rgba(255, 226, 180, .22)';
    ctx.fillRect(-h * .013, -h, h * .008, h);

    // ponta de AÇO
    const ponta = ctx.createLinearGradient(-h * .05, 0, h * .05, 0);
    ponta.addColorStop(0, cfg.acoSombra);
    ponta.addColorStop(.5, cfg.aco);
    ponta.addColorStop(1, cfg.acoSombra);
    ctx.fillStyle = ponta;
    ctx.beginPath();
    ctx.moveTo(0, -h * 1.11);
    ctx.quadraticCurveTo(h * .05, -h * 1.0, 0, -h * .93);
    ctx.quadraticCurveTo(-h * .05, -h * 1.0, 0, -h * 1.11);
    ctx.fill();

    // o anel de bronze que prende a ponta na haste
    ctx.fillStyle = cfg.bronze;
    ctx.fillRect(-h * .019, -h * .95, h * .038, h * .022);

    ctx.restore();
}

/// Um escudo tipo "gota", em METAL: reto em cima, afinando até a ponta embaixo.
///
/// Era silhueta com o umbo e a travessa RECORTADOS em `destination-out` — e recorte apaga pixel, ou
/// seja, abria buracos de verdade no escudo. Era essa a "transparência" que aparecia em jogo. Agora
/// os detalhes são PINTADOS por cima, e o corpo ganhou o mesmo degradê atravessado da espada.
function desenharEscudo(ctx, x, base, h, cfg) {
    const l = h * .74;

    ctx.save();
    ctx.translate(x, base);

    const contorno = () => {
        ctx.beginPath();
        ctx.moveTo(-l / 2, -h);
        ctx.lineTo(l / 2, -h);
        ctx.lineTo(l / 2, -h * .42);
        ctx.quadraticCurveTo(l / 2, -h * .06, 0, 0);
        ctx.quadraticCurveTo(-l / 2, -h * .06, -l / 2, -h * .42);
        ctx.closePath();
    };

    const face = ctx.createLinearGradient(-l / 2, -h, l / 2, 0);
    face.addColorStop(0, cfg.aco);
    face.addColorStop(.45, cfg.acoSombra);
    face.addColorStop(1, '#4c5872');
    ctx.fillStyle = face;
    contorno();
    ctx.fill();

    // a borda rebitada
    ctx.strokeStyle = cfg.bronze;
    ctx.lineWidth = h * .045;
    contorno();
    ctx.stroke();

    // travessa e umbo, PINTADOS (não recortados)
    ctx.fillStyle = cfg.bronze;
    ctx.fillRect(-l * .46, -h * .8, l * .92, h * .05);
    ctx.beginPath();
    ctx.arc(0, -h * .56, h * .11, 0, Math.PI * 2);
    ctx.fill();
    ctx.fillStyle = cfg.aco;
    ctx.beginPath();
    ctx.arc(-h * .025, -h * .585, h * .05, 0, Math.PI * 2);   // o brilho no alto do umbo
    ctx.fill();

    ctx.restore();
}

/// O cajado do conjurador: sobe da borda como a espada, na mesma inclinação, mas termina numa pedra
/// acesa em vez de numa lâmina. É o gesto que anuncia a magia — o mesmo papel da espada, e por isso
/// a mesma entrada: o exército é um só, o que muda é quem está na frente hoje.
function desenharCajado(ctx, borda, chao, lado, subida, cfg) {
    const h = cfg.cajado;

    ctx.save();
    ctx.translate(borda + lado * h * .38, chao + h * (1 - subida));
    ctx.rotate(lado * INCLINACAO);
    ctx.scale(lado, 1);

    // vara de madeira, com o mesmo fio de luz da lança
    ctx.fillStyle = cfg.madeira;
    ctx.fillRect(-h * .022, -h * .92, h * .044, h * .92);
    ctx.fillStyle = 'rgba(255, 226, 180, .2)';
    ctx.fillRect(-h * .022, -h * .92, h * .014, h * .92);
    ctx.fillStyle = cfg.bronze;

    // as garras que seguram a pedra
    ctx.beginPath();
    ctx.moveTo(-h * .07, -h * .88); ctx.lineTo(0, -h * 1.0); ctx.lineTo(h * .07, -h * .88);
    ctx.lineTo(h * .04, -h * .84); ctx.lineTo(-h * .04, -h * .84);
    ctx.closePath();
    ctx.fill();

    // A pedra acesa: o único ponto de luz da silhueta, e o que diz "magia" sem escrever nada.
    const raio = h * .1;
    const luz = ctx.createRadialGradient(0, -h * 1.02, 0, 0, -h * 1.02, raio * 3);
    luz.addColorStop(0, `rgba(${cfg.brasa}, 1)`);
    luz.addColorStop(.28, `rgba(${cfg.magia}, .85)`);
    luz.addColorStop(1, `rgba(${cfg.magia}, 0)`);
    ctx.fillStyle = luz;
    ctx.beginPath();
    ctx.arc(0, -h * 1.02, raio * 3, 0, Math.PI * 2);
    ctx.fill();

    ctx.restore();
}

/// A ESFERA DE MAGIA: a defesa do outro número. Sobe da borda como os escudos, mas é uma cúpula
/// translúcida — o golpe não é aparado, é contido. Anéis concêntricos dão a leitura de campo de
/// força; um disco chapado leria como bolha de sabão.
function desenharEsfera(ctx, borda, chao, lado, subida, cfg) {
    const r = cfg.esfera;
    // Ancorada NO CANTO, como o ✕ da saída: o centro fica quase em cima do vértice, e o que se vê é
    // o arco invadindo o campo. Uma bola inteira no meio da tela leria como objeto flutuando; um
    // arco saindo do canto lê como algo grande que está ali fora, protegendo.
    const cx = borda - lado * r * .08;   // centro JÁ do lado de fora: só o arco entra
    const cy = chao - r * subida * .22;

    ctx.save();

    const corpo = ctx.createRadialGradient(cx, cy, r * .1, cx, cy, r);
    corpo.addColorStop(0, `rgba(${cfg.magia}, .07)`);
    corpo.addColorStop(.72, `rgba(${cfg.magia}, .2)`);
    corpo.addColorStop(.93, `rgba(${cfg.magia}, .55)`);
    corpo.addColorStop(1, `rgba(${cfg.magia}, 0)`);
    ctx.fillStyle = corpo;
    ctx.beginPath();
    ctx.arc(cx, cy, r, 0, Math.PI * 2);
    ctx.fill();

    ctx.strokeStyle = `rgba(${cfg.magia}, .5)`;
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.arc(cx, cy, r * .97, 0, Math.PI * 2);
    ctx.stroke();

    ctx.strokeStyle = `rgba(${cfg.magia}, .22)`;
    ctx.lineWidth = 1.2;
    for (const fatia of [.72, .46]) {
        ctx.beginPath();
        ctx.ellipse(cx, cy, r * fatia, r * .96, 0, 0, Math.PI * 2);
        ctx.stroke();
    }

    ctx.restore();
}

/// A BOLA DE FOGO: núcleo claro, corpo alaranjado e um rastro que sai da própria trajetória — as
/// posições de trás vêm de amostrar a mesma parábola em `p` menores, então o rastro acompanha a
/// curva de verdade em vez de ser uma linha reta pendurada atrás.
function desenharBolaDeFogo(ctx, x, y, p, posicao, cfg) {
    const r = cfg.bola;

    ctx.save();

    for (let k = 6; k >= 1; k--) {
        const atras = posicao(Math.max(0, p - k * .016));
        const escala = 1 - k * .11;
        const alfa = .16 * (1 - k / 7);
        const g = ctx.createRadialGradient(atras.x, atras.y, 0, atras.x, atras.y, r * escala);
        g.addColorStop(0, `rgba(${cfg.fogo}, ${alfa * 3})`);
        g.addColorStop(1, `rgba(${cfg.fogo}, 0)`);
        ctx.fillStyle = g;
        ctx.beginPath();
        ctx.arc(atras.x, atras.y, r * escala, 0, Math.PI * 2);
        ctx.fill();
    }

    const bola = ctx.createRadialGradient(x, y, 0, x, y, r);
    bola.addColorStop(0, `rgba(${cfg.brasa}, 1)`);
    bola.addColorStop(.3, `rgba(${cfg.fogo}, .95)`);
    bola.addColorStop(.62, `rgba(${cfg.fogo}, .45)`);
    bola.addColorStop(1, `rgba(${cfg.fogo}, 0)`);
    ctx.fillStyle = bola;
    ctx.beginPath();
    ctx.arc(x, y, r, 0, Math.PI * 2);
    ctx.fill();

    ctx.restore();
}

/// O estouro da bola de fogo contra a esfera. As brasas são sorteadas UMA VEZ, no nascimento, e não
/// a cada quadro: sorteadas por quadro elas piscariam em lugares diferentes em vez de voar.
function criarExplosao(x, y) {
    return {
        x, y, t: 0,
        brasas: Array.from({ length: 11 }, () => ({
            angulo: Math.random() * Math.PI * 2,
            alcance: .7 + Math.random() * .9,
            tamanho: 1.4 + Math.random() * 2.4,
        })),
    };
}

/// Três coisas ao mesmo tempo, e é a soma que lê como explosão: um CLARÃO que nasce grande e morre
/// rápido, um ANEL de choque que abre e afina, e BRASAS cuspidas pra fora. Só o clarão seria um
/// borrão; só o anel seria um efeito de interface.
function desenharExplosao(ctx, e, cfg) {
    const t = e.t;
    const restante = 1 - t;
    const r = cfg.bola * (1 + t * 4.2);

    ctx.save();

    // clarão — desaparece mais rápido que o resto (curva ao quadrado)
    const clarao = ctx.createRadialGradient(e.x, e.y, 0, e.x, e.y, r);
    clarao.addColorStop(0, `rgba(${cfg.brasa}, ${restante * restante})`);
    clarao.addColorStop(.3, `rgba(${cfg.fogo}, ${restante * restante * .85})`);
    clarao.addColorStop(1, `rgba(${cfg.fogo}, 0)`);
    ctx.fillStyle = clarao;
    ctx.beginPath();
    ctx.arc(e.x, e.y, r, 0, Math.PI * 2);
    ctx.fill();

    // anel de choque
    ctx.strokeStyle = `rgba(${cfg.brasa}, ${restante * .8})`;
    ctx.lineWidth = Math.max(.6, 6 * restante);
    ctx.beginPath();
    ctx.arc(e.x, e.y, r * .88, 0, Math.PI * 2);
    ctx.stroke();

    // brasas
    ctx.fillStyle = `rgba(${cfg.fogo}, ${restante})`;
    for (const b of e.brasas) {
        const d = r * 1.15 * b.alcance;
        ctx.beginPath();
        ctx.arc(e.x + Math.cos(b.angulo) * d,
                e.y + Math.sin(b.angulo) * d + t * t * 26,   // a gravidade puxando as brasas
                b.tamanho * restante, 0, Math.PI * 2);
        ctx.fill();
    }

    ctx.restore();
}

/// Uma flecha: haste, ponta e penas, deitada na direção do voo.
function desenharFlecha(ctx, x, y, angulo, cfg) {
    const s = cfg.tamanhoFlecha;

    ctx.save();
    ctx.translate(x, y);
    ctx.rotate(angulo);
    ctx.scale(s, s);

    ctx.strokeStyle = `rgba(${cfg.flecha}, .85)`;
    ctx.lineWidth = 1.4;
    ctx.beginPath();
    ctx.moveTo(-9, 0); ctx.lineTo(5, 0);
    ctx.stroke();

    ctx.fillStyle = `rgba(${cfg.flecha}, .95)`;
    ctx.beginPath();
    ctx.moveTo(9, 0); ctx.lineTo(4, -2.2); ctx.lineTo(4, 2.2);
    ctx.closePath();
    ctx.fill();

    ctx.beginPath();
    ctx.moveTo(-9, 0); ctx.lineTo(-12, -2.4); ctx.lineTo(-7, 0); ctx.lineTo(-12, 2.4);
    ctx.closePath();
    ctx.fill();

    ctx.restore();
}

/// As corujas empoleiradas nos troncos da mata, cada uma com o PRÓPRIO relógio.
///
/// Fora de sincronia é o ponto (ideia do Gabriel). Um piscar coletivo denunciaria que é um efeito
/// só; relógios independentes fazem parecer que há bichos ali, cada um na sua.
///
/// O olho fica APAGADO a maior parte do tempo, e o tempo ACESO é fixo. Essa assimetria é o que
/// separa "bicho que abre o olho de vez em quando" de "lâmpada piscando": o que se sorteia é a
/// ESPERA, nunca a duração do olhar. Como o sorteio se repete a cada ciclo, duas corujas que por
/// acaso acendam juntas se desencontram na volta seguinte.
///
/// A posição sai do ladrilho REAL da mata (as vars `--mata-*` do CSS, lidas do #arena), então os
/// olhos caem em cima das árvores desenhadas e acompanham qualquer mudança no ladrilho — em vez de
/// haver duas cópias do número pra divergirem.
///
/// As contas são em coordenadas do CANVAS, que É a arena (ele é filho dela e a preenche): a mata
/// fica ancorada no rodapé, então o topo do ladrilho é `altura do canvas − altura do ladrilho`.

/// O CAIXÃO que sobe no meio do cemitério — a referência do 💀 no cenário dele.
///
/// Fica no CENTRO, como a ruína dos Tecnológicos e o castelo do Reino: coisa única e nomeada mora no
/// meio, e o log (uma coluna de ~300px em z 4) passa na frente dela nos três casos sem que isso tenha
/// atrapalhado nenhum. O que resolve não é fugir do log — é o CLARÃO, que tem raio bem maior que a
/// peça e vaza pelos dois lados dele. A luz é o que anuncia o acontecimento; a silhueta só confirma.
///
/// EM PÉ, e não deitado: caixão deitado visto de frente é um retângulo, e o que sobe do chão precisa
/// ter uma direção.
///
/// O que sai de dentro são FANTASMAS, e não uma caveira. A caveira falhava por escala — a 70px o
/// crânio já é quase só silhueta, e detalhe nessa medida lê como borrão — a lição que todo cenário
/// aqui já cobrou. Os fantasmas resolvem os dois problemas de uma vez: não dependem de detalhe (são vulto
/// e halo), o desenho JÁ EXISTE (`desenharFantasma`, o mesmo dos que cruzam o céu), e eles dão à cena
/// um ANTES e um DEPOIS — o caixão passa a ser de onde a assombração vem, e não um objeto que abre.
///
/// A TAMPA abre em DUAS METADES, cada uma girando pra fora na dobradiça do próprio lado. Uma folha
/// só, girando pra um lado, jogava o peso da figura pro outro e brigava com uma peça que é simétrica
/// em tudo mais. Com duas, o vão nasce no meio — que é justamente de onde os fantasmas saem.
///
/// Diretor de uma fase por vez, como os exércitos e o ninja:
///   enterrado → terra → subindo → abrindo → soltando → fechando → descendo → assentando → enterrado
///
/// A TERRA é a primeira a aparecer e a última a sumir, das três opções possíveis (sempre visível,
/// nunca, ou acompanhando). Ela revirar ANTES de qualquer coisa aparecer é o que transforma a subida
/// em consequência: o chão racha, e só então sai o que estava embaixo. Deixá-la fixa faria dela mais
/// uma lápide do cenário — perderia a antecipação, que é a parte barata e mais eficaz do susto.
///
/// Só a ESPERA enterrado é sorteada. Os gestos têm duração fixa: sortear quanto tempo uma tampa leva
/// pra abrir faria a mesma tampa parecer pesada numa vez e leve na outra.
function criarCaixao(cfg, canvas) {
    let fase = 'enterrado';
    let relogio = entre(cfg.espera) * .35;   // a primeira espera é curta: a cena não pode abrir vazia
    let terra = 0;       // 0 = chão intacto, 1 = monte revirado
    let emerso = 0;      // 0 = enterrado, 1 = fora
    let abertura = 0;    // 0 = fechado, 1 = tampa escancarada
    let aSoltar = 0;     // quantos fantasmas ainda faltam sair nesta leva
    let proximo = 0;     // relógio até o próximo sair
    let leva = 0;        // quantas aberturas já houve — é o relógio de vida dos fantasmas
    let fantasmas = [];
    let t = 0;

    // Os caixões PEQUENOS em volta, sorteados uma vez: eles não têm ciclo nenhum, só acompanham o
    // grande subindo e descendo. Sorteados por quadro, tremeriam.
    const menores = cfg.menores.map(m => ({ ...m, ondula: Math.random() * Math.PI * 2 }));

    return (ctx, dt) => {
        t += dt;
        relogio -= dt;

        const l = canvas.height * cfg.largura, h = canvas.height * cfg.altura;
        const cx = canvas.width / 2;
        const base = canvas.height;
        // O pé do caixão fica sempre ABAIXO da borda: ele sai do chão, não desliza de uma fresta.
        const topo = base - h * emerso;
        // A BOCA: de onde os fantasmas saem. É o vão que se abre no meio, no alto do caixão.
        const bocaY = topo + h * .22;

        switch (fase) {
            case 'enterrado':
                if (relogio <= 0) fase = 'terra';
                break;
            case 'terra':
                terra = Math.min(1, terra + dt / cfg.revirar);
                if (terra === 1) {
                    fase = 'subindo';
                    // A LEVA vira aqui, no instante em que o chão termina de revirar — e é aqui que
                    // os fantasmas velhos recebem a ordem de apagar. Eles duram DOIS ciclos: a leva
                    // 1 sai, a leva 2 sai (seis na tela), e quando a 3 vem a 1 já está se apagando.
                    //
                    // Marcar AQUI, e não quando a tampa abre, é o que faz a conta fechar: daqui até
                    // o primeiro fantasma novo há a subida inteira mais a abertura (~3,5s), e a
                    // fade leva 2,2s. Marcando mais tarde, os velhos ainda estariam se apagando na
                    // hora de os novos saírem, e o teto de 6 barraria a leva nova pela metade —
                    // o caixão abriria pra soltar dois. Também é melhor de ver: eles se dissolvem
                    // ENQUANTO o caixão sobe, então a troca de guarda tem o gesto certo.
                    leva++;
                    for (const f of fantasmas) if (f.leva <= leva - 2) f.apagando = true;
                }
                break;
            case 'subindo':
                emerso = Math.min(1, emerso + dt / cfg.subir);
                if (emerso === 1) fase = 'abrindo';
                break;
            case 'abrindo':
                abertura = Math.min(1, abertura + dt / cfg.abrir);
                if (abertura === 1) { fase = 'soltando'; aSoltar = cfg.porLeva; proximo = 0; }
                break;
            case 'soltando':
                proximo -= dt;
                if (aSoltar > 0 && proximo <= 0) {
                    // O teto é rede de segurança, não a regra: com o rodízio das levas a conta já
                    // fecha em 6. Ele existe pra o caso de um ciclo atropelar o outro.
                    if (fantasmas.length < cfg.maximo) {
                        fantasmas.push({
                            leva,
                            x: cx + (Math.random() - .5) * l * .5,
                            y: bocaY,
                            // Nasce SUBINDO e sem deriva; a deriva entra depois (ver o passeio). Sair
                            // já andando de lado leria como "passava por ali", não como "saiu daí".
                            vy: -entre(cfg.subida) * canvas.height,
                            vx: 0,
                            deriva: (Math.random() < .5 ? -1 : 1) * entre(cfg.deriva) * canvas.width,
                            // A ALTITUDE que ele persegue, e quando vai trocar por outra. É isto que
                            // espalha o bando pela tela inteira: sem alvo, todos ficariam na faixa
                            // em que o impulso de saída os largou.
                            alvoY: canvas.height * entre(cfg.altitude),
                            trocar: entre(cfg.mudarAltura),
                            s: entre([.72, 1.25]),
                            bobo: entre([14, 34]),
                            fase: Math.random() * Math.PI * 2,
                            asa: Math.random() * Math.PI * 2,
                            paraDireita: Math.random() < .5,
                            t: 0,
                            alfa: 0,
                        });
                    }
                    aSoltar--;
                    proximo = cfg.intervalo;
                }
                // Fecha logo depois do ÚLTIMO SAIR — e não depois de ele sumir. Os fantasmas agora
                // vivem dois ciclos inteiros passeando pela tela; esperar o fim deles deixaria o
                // caixão escancarado o tempo todo, e ele voltou a ser um acontecimento breve.
                if (aSoltar <= 0) fase = 'fechando';
                break;
            case 'fechando':
                abertura = Math.max(0, abertura - dt / cfg.fechar);
                if (abertura === 0) fase = 'descendo';
                break;
            case 'descendo':
                emerso = Math.max(0, emerso - dt / cfg.descer);
                if (emerso === 0) fase = 'assentando';
                break;
            case 'assentando':
                terra = Math.max(0, terra - dt / cfg.assentar);
                if (terra === 0) { fase = 'enterrado'; relogio = entre(cfg.espera); }
                break;
        }

        // --- o PASSEIO dos fantasmas, fora da máquina de fases: eles vivem dois ciclos do caixão, e
        //     têm que seguir andando enquanto a tampa fecha, o caixão desce e o próximo sobe.
        for (let k = fantasmas.length - 1; k >= 0; k--) {
            const f = fantasmas[k];
            f.t += dt;

            // Sai de dentro subindo e vai TROCANDO o impulso vertical pela deriva lateral: é essa
            // troca que transforma "saiu do caixão" em "está passeando pela tela", sem os dois
            // momentos precisarem de dois sistemas.
            f.vy *= .988;
            f.vx += (f.deriva - f.vx) * Math.min(1, dt / cfg.assumir);
            f.x += f.vx * dt;
            f.y += f.vy * dt;

            // Troca de altitude de vez em quando, cada um no seu tempo. A perseguição do alvo está
            // SEMPRE ligada, mas é lenta: enquanto o impulso de saída é forte ela mal se nota, e
            // quando ele se esgota é ela que passa a mandar. Um só mecanismo cobre os dois momentos.
            f.trocar -= dt;
            if (f.trocar <= 0) {
                f.alvoY = canvas.height * entre(cfg.altitude);
                f.trocar = entre(cfg.mudarAltura);
            }
            f.y += (f.alvoY - f.y) * Math.min(1, dt / cfg.buscarAltura);

            f.fase += dt * 1.6;
            f.asa += dt * 2.2;
            f.paraDireita = f.vx >= 0;

            // Dá a volta pelas bordas em vez de morrer nelas: são eles que povoam a tela entre uma
            // abertura e outra, e um fantasma que some ao encostar na borda deixaria o cemitério
            // vazio na metade do ciclo.
            const folga = canvas.height * cfg.tamanho * 2;
            if (f.x < -folga) f.x = canvas.width + folga;
            else if (f.x > canvas.width + folga) f.x = -folga;
            if (f.y < -folga) f.y = -folga;

            // Acende ao sair e apaga só quando MANDAM apagar (a leva dele venceu). Nada de morrer
            // por cronômetro próprio: quem manda no fim deles é o caixão.
            const alvo = f.apagando ? 0 : 1;
            const passo = dt / (f.apagando ? cfg.apagar : cfg.acender);
            f.alfa += Math.sign(alvo - f.alfa) * Math.min(passo, Math.abs(alvo - f.alfa));
            if (f.apagando && f.alfa <= 0) fantasmas.splice(k, 1);
        }

        if (terra <= 0 && emerso <= 0 && fantasmas.length === 0) return;

        ctx.save();

        // --- o clarão, ATADO À ABERTURA: com a tampa fechada não há luz nenhuma, e é isso que faz a
        //     abertura ser um acontecimento em vez de um objeto sempre aceso.
        if (abertura > 0) {
            const pulso = .8 + Math.sin(t * 2.6) * .12 + Math.sin(t * 6.3) * .06;
            const raio = l * cfg.clarao * abertura * pulso;
            const clarao = ctx.createRadialGradient(cx, bocaY, 0, cx, bocaY, raio);
            clarao.addColorStop(0, `rgba(${cfg.brilho}, ${.3 * abertura})`);
            clarao.addColorStop(.45, `rgba(${cfg.brilho}, ${.12 * abertura})`);
            clarao.addColorStop(1, `rgba(${cfg.brilho}, 0)`);
            ctx.fillStyle = clarao;
            ctx.beginPath();
            ctx.arc(cx, bocaY, raio, 0, Math.PI * 2);
            ctx.fill();
        }

        // --- os caixões PEQUENOS, em diagonal, brotando na terra em volta. São enfeite: não abrem,
        //     não soltam nada, só acompanham o grande. Ficam ANTES dele no desenho porque são a
        //     companhia, não o assunto — o grande passa na frente quando se cruzam.
        for (const m of menores) {
            const mh = canvas.height * cfg.menorTamanho;
            const ml = mh * .42;
            // Brotam um pouco atrasados em relação ao grande (`emerso` elevado), como se a terra os
            // empurrasse junto. O expoente é o atraso: sem ele, todos rompem o chão no mesmo quadro.
            const brota = Math.pow(emerso, 1.5 + m.atraso);
            if (brota <= .01) continue;

            ctx.save();
            ctx.translate(cx + l * m.x, base - h * m.y * brota);
            ctx.rotate(m.giro);
            ctx.globalAlpha = Math.min(1, brota * 1.4);
            ctx.fillStyle = cfg.madeira;
            contornoDoCaixao(ctx, 0, -mh * brota, 0, ml, mh, cfg.canto * .5);
            ctx.fill();
            // uma cruz só, bem simples: nessa escala é o que separa "caixão" de "tábua espetada"
            ctx.fillStyle = cfg.ferro;
            ctx.fillRect(-ml * .06, -mh * brota * .78, ml * .12, mh * brota * .4);
            ctx.fillRect(-ml * .22, -mh * brota * .68, ml * .44, mh * brota * .1);
            ctx.restore();
        }
        ctx.globalAlpha = 1;

        if (emerso > 0) {
            // --- a MADEIRA inteira, fechada. A tampa não é desenhada em peças: é este corpo, e o
            //     que abre é um VÃO cavado nele (logo abaixo).
            ctx.fillStyle = cfg.madeira;
            contornoDoCaixao(ctx, cx, topo, base + 8, l, h, cfg.canto);
            ctx.fill();

            // as tábuas e a cruz de ferro, na madeira fechada
            ctx.strokeStyle = 'rgba(0, 0, 0, .32)';
            ctx.lineWidth = 1.5;
            for (const lado of [-1, 1]) {
                ctx.beginPath();
                ctx.moveTo(cx + lado * l * .22, topo + h * .06);
                ctx.lineTo(cx + lado * l * .12, base);
                ctx.stroke();
            }
            ctx.fillStyle = cfg.ferro;
            ctx.fillRect(cx - l * .05, topo + h * .1, l * .1, h * .38);
            ctx.fillRect(cx - l * .2, topo + h * .19, l * .4, h * .09);

            // --- a ABERTURA: um vão escuro que cresce do MEIO pra fora, recortado no contorno do
            //     caixão. É a tampa partindo ao meio e as duas metades sumindo pros lados.
            //
            //     Era duas folhas transformadas por `scale` em direção às dobradiças. Aquilo não
            //     funcionava: cada folha carregava o contorno INTEIRO do caixão espremido, então o
            //     que se via eram dois caixõezinhos deformados nas laterais em vez de duas metades
            //     de tampa — e nas aberturas parciais elas se sobrepunham no meio. Cavar o vão é o
            //     inverso e não tem como errar: o que não é vão É tampa, e o meio abre primeiro
            //     porque o vão nasce no eixo.
            if (abertura > 0) {
                ctx.save();
                contornoDoCaixao(ctx, cx, topo, base + 8, l, h, cfg.canto);
                ctx.clip();
                ctx.fillStyle = cfg.dentro;
                const meio = l * .5 * abertura;
                ctx.fillRect(cx - meio, topo - 2, meio * 2, h + 12);
                ctx.restore();
            }
        }

        // --- o MONTE DE TERRA revirada. Vem DEPOIS do caixão pra cobrir a junta com o chão, e tem
        //     vida própria: sobe antes de tudo e assenta depois de tudo.
        if (terra > 0) {
            ctx.fillStyle = cfg.terra;
            ctx.beginPath();
            ctx.ellipse(cx, base - 2, l * (1.9 + emerso * .8) * terra, h * .14 * terra, 0, Math.PI, 0);
            ctx.fill();
        }

        // --- os fantasmas por ÚLTIMO: eles saem de dentro e passam na frente da madeira.
        for (const f of fantasmas) {
            ctx.globalAlpha = f.alfa;
            desenharFantasma(ctx, f.x, f.y + Math.sin(f.fase) * f.bobo,
                canvas.height * cfg.tamanho * f.s, f.asa, f.paraDireita, cfg);
        }
        ctx.globalAlpha = 1;

        ctx.restore();
    };
}

/// A silhueta hexagonal do caixão, com os cantos LEVEMENTE arredondados — madeira velha não tem
/// quina viva, e o arredondado é o que tira a leitura de "polígono desenhado".
///
/// São os OMBROS (o ponto mais largo, a 26% do topo) que fazem a forma ler como caixão; sem eles é
/// uma caixa comprida. O `pe` é a altura do PÉ e vem de fora: o caixão grande passa da borda de
/// baixo (caixão saindo do chão não tem fundo à vista), e os pequenos, que são desenhados girados
/// em volta do próprio centro, terminam no zero.
function contornoDoCaixao(ctx, cx, topo, pe, l, h, canto) {
    const pontos = [
        [cx - l * .30, topo],
        [cx + l * .30, topo],
        [cx + l * .50, topo + h * .26],
        [cx + l * .22, pe],
        [cx - l * .22, pe],
        [cx - l * .50, topo + h * .26],
    ];
    const meio = (a, b) => [(a[0] + b[0]) / 2, (a[1] + b[1]) / 2];

    // O idioma do polígono arredondado: começa no MEIO de um lado (pra o primeiro arcTo ter de onde
    // partir) e vai ligando meio-de-lado a meio-de-lado, curvando em cada vértice.
    ctx.beginPath();
    const inicio = meio(pontos[0], pontos[1]);
    ctx.moveTo(inicio[0], inicio[1]);
    for (let k = 1; k <= pontos.length; k++) {
        const v = pontos[k % pontos.length];
        const seguinte = meio(v, pontos[(k + 1) % pontos.length]);
        ctx.arcTo(v[0], v[1], seguinte[0], seguinte[1], canto);
    }
    ctx.closePath();
}

/// O INVASOR descendo do céu — e dele só se vê o que ENTRA na tela: tentáculos roxos baixando do
/// alto, o do meio maior. É o 👽/👾 chegando de verdade; os discos já cruzavam o céu, mas quem os
/// mandava nunca aparecia.
///
/// O corpo NÃO é desenhado. Nem cortado pela moldura, nem sugerido em silhueta: ele simplesmente não
/// está lá. O que existe é uma escuridão roxa sangrando pela borda de cima e as partes que descem
/// dela. Desenhar a massa entregaria o tamanho do bicho, e o tamanho é justamente o que não deve ser
/// resolvido — o que a cabeça monta a partir de sete braços vindos do escuro é maior que qualquer
/// monstro que eu desenhasse ali. Foi a mesma escolha do sorriso do palhaço, que também não tem rosto.
///
/// Vive no canvas de FUNDO (z 0), atrás dos combatentes: um bicho desse tamanho na tela da frente
/// taparia a luta, e cenário nenhum vale atrapalhar o jogo. Passa por cima da cidade e do reator,
/// que é o lugar certo — ele chegou depois.
///
/// O EVENTO tem uma fase por vez, como os exércitos, o ninja e o caixão:
///   escondido → descendo → pairando → subindo → escondido
///
/// Mas cada braço tem o seu ATRASO dentro do evento, então eles não descem nem sobem em bloco: quando
/// os primeiros já estão pairando, os últimos ainda estão entrando. Descendo juntos, sete tentáculos
/// leriam como uma cortina — o desencontro é o que faz parecer um bicho se mexendo.
///
/// E cada um tem RITMO e AMPLITUDE próprios, com a ondulação crescendo do talo pra ponta.
function criarTentaculos(cfg, canvas) {
    let fase = 'escondido';
    let relogio = entre(cfg.espera) * .4;   // a primeira espera é curta: a cena não pode abrir vazia
    let descido = 0;                        // o avanço do EVENTO; cada braço lê isto pelo seu atraso
    let t = 0;

    // Sorteados UMA vez: ritmo e amplitude por quadro fariam os tentáculos tremerem em vez de ondular.
    const bracos = Array.from({ length: cfg.quantos }, (_, i) => {
        const u = cfg.quantos === 1 ? 0 : (i / (cfg.quantos - 1)) * 2 - 1;   // -1 .. 1, 0 no centro
        return {
            u,
            // O do CENTRO é o mais longo e o mais grosso, e a queda pras pontas é suave. É isso que
            // dá a leitura de UM bicho com um corpo lá em cima, e não de N tentáculos enfileirados.
            comprimento: 1 - Math.abs(u) * .52,
            grossura: 1 - Math.abs(u) * .4,
            // Os do meio chegam primeiro e os das pontas por último — o bicho desce de bruços, não
            // de lado. Sorteio um pouco em cima disso pra a fileira não ficar simétrica demais.
            atraso: Math.abs(u) * cfg.atraso + Math.random() * cfg.atraso * .35,
            fase: Math.random() * Math.PI * 2,
            ritmo: entre(cfg.ondular),
            onda: entre(cfg.onda),
        };
    });

    return (ctx, dt) => {
        t += dt;
        relogio -= dt;

        switch (fase) {
            case 'escondido':
                if (relogio <= 0) fase = 'descendo';
                break;
            case 'descendo':
                descido = Math.min(1, descido + dt / cfg.descer);
                if (descido === 1) { fase = 'pairando'; relogio = entre(cfg.pairar); }
                break;
            case 'pairando':
                if (relogio <= 0) fase = 'subindo';
                break;
            case 'subindo':
                descido = Math.max(0, descido - dt / cfg.subir);
                if (descido === 0) { fase = 'escondido'; relogio = entre(cfg.espera); }
                break;
        }

        if (descido <= 0) return;

        const cx = canvas.width / 2;
        const meia = canvas.width * cfg.largura * .5;

        ctx.save();

        // --- a ESCURIDÃO no alto: um roxo sangrando da borda de cima pra baixo. É a única coisa que
        //     representa o corpo, e ela não tem forma nenhuma de propósito — dar contorno a isso
        //     seria desenhar o monstro. Serve pra os tentáculos não parecerem recortes colados no
        //     céu: eles saem de ALGUMA coisa, e essa coisa é só mais escura que a noite.
        const alturaSombra = canvas.height * cfg.sombra * descido;
        const veu = ctx.createLinearGradient(0, -10, 0, alturaSombra);
        veu.addColorStop(0, `rgba(${cfg.escuro}, ${.85 * descido})`);
        veu.addColorStop(.55, `rgba(${cfg.escuro}, ${.4 * descido})`);
        veu.addColorStop(1, `rgba(${cfg.escuro}, 0)`);
        ctx.fillStyle = veu;
        ctx.fillRect(0, -10, canvas.width, alturaSombra + 10);

        // um brilho fraco no meio dessa escuridão, na direção de onde vem o braço maior
        const halo = ctx.createRadialGradient(cx, 0, 0, cx, 0, meia);
        halo.addColorStop(0, `rgba(${cfg.brilho}, ${.13 * descido})`);
        halo.addColorStop(1, `rgba(${cfg.brilho}, 0)`);
        ctx.fillStyle = halo;
        ctx.fillRect(cx - meia, -10, meia * 2, meia);

        // --- os braços
        for (const b of bracos) {
            // O atraso dele consumido do avanço do evento: 0 até o evento passar do seu atraso, e
            // daí em diante 0..1 no que restou. É isto que escalona a entrada e a saída.
            const d = Math.max(0, (descido - b.atraso) / (1 - b.atraso));
            if (d <= .01) continue;

            const x0 = cx + b.u * meia;
            desenharTentaculo(ctx,
                x0, -8,
                canvas.height * cfg.alcance * b.comprimento * d,
                canvas.width * cfg.talo * b.grossura,
                b, t, cfg);
        }

        ctx.restore();
    };
}

/// Um tentáculo: fita que afina do talo até a ponta, com uma ONDA VIAJANDO pra baixo.
///
/// A onda viaja porque a fase desconta a distância percorrida (`- p * 3.4`): sem esse desconto, o
/// tentáculo inteiro iria pro mesmo lado ao mesmo tempo e pareceria um limpador de parabrisa. E a
/// amplitude é multiplicada por `p * p`, então a raiz quase não sai do lugar e a ponta chicoteia —
/// é assim que corda pendurada se move.
function desenharTentaculo(ctx, x0, y0, comprimento, talo, b, t, cfg) {
    const passos = 16;
    const esq = [], dir = [];

    for (let k = 0; k <= passos; k++) {
        const p = k / passos;
        const x = x0 + Math.sin(b.fase + t * b.ritmo * 2.1 - p * 3.4) * b.onda * p * p;
        const y = y0 + comprimento * p;
        const w = talo * Math.pow(1 - p, .75);
        esq.push([x - w, y]);
        dir.push([x + w, y]);
    }

    ctx.beginPath();
    ctx.moveTo(esq[0][0], esq[0][1]);
    for (const q of esq) ctx.lineTo(q[0], q[1]);
    for (let k = dir.length - 1; k >= 0; k--) ctx.lineTo(dir[k][0], dir[k][1]);
    ctx.closePath();
    ctx.fillStyle = cfg.corpo;
    ctx.fill();

    // A nervura clara, só nos dois terços de cima: ela dá volume ao tubo, e para antes da ponta
    // porque lá a fita já é fina demais pra caber duas cores — insistir viraria serrilhado.
    const ate = Math.floor(passos * .66);
    ctx.beginPath();
    ctx.moveTo(esq[0][0] + (dir[0][0] - esq[0][0]) * .34, esq[0][1]);
    for (let k = 0; k <= ate; k++) ctx.lineTo(esq[k][0] + (dir[k][0] - esq[k][0]) * .34, esq[k][1]);
    for (let k = ate; k >= 0; k--) ctx.lineTo(esq[k][0] + (dir[k][0] - esq[k][0]) * .62, esq[k][1]);
    ctx.closePath();
    ctx.fillStyle = cfg.corpoClaro;
    ctx.fill();

    // as VENTOSAS: uma fileira de pontos acesos descendo pelo braço, sumindo junto com a fita. São
    // o único detalhe, e existem porque sem elas o tentáculo é um tubo — com elas, é bicho.
    for (let k = 2; k <= passos - 2; k += 2) {
        const p = k / passos;
        const w = talo * Math.pow(1 - p, .75);
        if (w < 2) break;
        ctx.fillStyle = `rgba(${cfg.brilho}, ${.55 * (1 - p * .7)})`;
        ctx.beginPath();
        ctx.arc(esq[k][0] + w, esq[k][1], Math.max(1, w * .26), 0, Math.PI * 2);
        ctx.fill();
    }
}

/// A RUÍNA: o reator estourado no meio da cidade, queimando e vazando veneno. Uma só, no centro.
///
/// O fogo é feito de labaredas independentes, cada uma com o próprio ritmo e a própria altura — é a
/// dessincronia que faz chama parecer chama. Um brilho pulsando sozinho leria como lâmpada.
function criarRuina(cfg, canvas) {
    const labaredas = Array.from({ length: cfg.labaredas }, (_, i) => ({
        // Espalhadas pela boca do reator, as do meio mais altas (o miolo é onde queima mais).
        posicao: (i + .5) / cfg.labaredas,
        ritmo: 2.4 + Math.random() * 2.6,
        fase: Math.random() * Math.PI * 2,
        alturaBase: .55 + Math.random() * .45,
    }));

    let t = 0;

    return (ctx, dt) => {
        t += dt;

        const l = canvas.height * cfg.largura, h = canvas.height * cfg.altura;
        const cx = canvas.width / 2;
        const base = canvas.height;

        ctx.save();

        // --- o clarão do incêndio, atrás de tudo: é ele que põe a ruína no meio de uma cidade
        //     escura, em vez de deixá-la como um recorte preto sobre o fundo.
        const pulso = .82 + Math.sin(t * 3.1) * .1 + Math.sin(t * 7.7) * .05;
        const clarao = ctx.createRadialGradient(cx, base - h * .6, 0, cx, base - h * .6, l * 1.5 * pulso);
        clarao.addColorStop(0, `rgba(${cfg.fogo}, .3)`);
        clarao.addColorStop(.45, `rgba(${cfg.fogo}, .1)`);
        clarao.addColorStop(1, `rgba(${cfg.fogo}, 0)`);
        ctx.fillStyle = clarao;
        ctx.beginPath();
        ctx.arc(cx, base - h * .6, l * 1.5 * pulso, 0, Math.PI * 2);
        ctx.fill();

        // --- as labaredas, saindo da boca rasgada do reator
        for (const f of labaredas) {
            const x = cx - l * .3 + l * .6 * f.posicao;
            // duas ondas de frequências diferentes: uma só daria um pulsar regular demais
            const viva = .6 + Math.sin(t * f.ritmo + f.fase) * .25 + Math.sin(t * f.ritmo * 2.3) * .15;
            const alt = h * .62 * f.alturaBase * viva;
            const larg = l * .07 * (.8 + viva * .4);

            const chama = ctx.createLinearGradient(x, base - h * .52, x, base - h * .52 - alt);
            chama.addColorStop(0, `rgba(${cfg.brasa}, .85)`);
            chama.addColorStop(.35, `rgba(${cfg.fogo}, .6)`);
            chama.addColorStop(1, `rgba(${cfg.fogo}, 0)`);
            ctx.fillStyle = chama;

            ctx.beginPath();
            ctx.moveTo(x - larg, base - h * .52);
            // a ponta balança pro lado: fogo não sobe reto
            ctx.quadraticCurveTo(x - larg * .5, base - h * .52 - alt * .6,
                x + Math.sin(t * f.ritmo * .8 + f.fase) * larg * 1.2, base - h * .52 - alt);
            ctx.quadraticCurveTo(x + larg * .5, base - h * .52 - alt * .6, x + larg, base - h * .52);
            ctx.closePath();
            ctx.fill();
        }

        // --- a carcaça: parede rasgada, cúpula desabada e vergalhão torto
        ctx.fillStyle = cfg.silhueta;
        ctx.beginPath();
        ctx.moveTo(cx - l * .5, base);
        ctx.lineTo(cx - l * .5, base - h * .46);
        ctx.lineTo(cx - l * .38, base - h * .62);
        ctx.lineTo(cx - l * .3, base - h * .5);      // o rasgo por onde o fogo sai
        ctx.lineTo(cx - l * .18, base - h * .58);
        ctx.lineTo(cx - l * .05, base - h * .44);
        ctx.lineTo(cx + l * .08, base - h * .6);
        ctx.lineTo(cx + l * .2, base - h * .47);
        ctx.lineTo(cx + l * .3, base - h * .78);     // o pedaço que ficou de pé
        ctx.lineTo(cx + l * .42, base - h * .72);
        ctx.lineTo(cx + l * .5, base - h * .9);
        ctx.lineTo(cx + l * .5, base);
        ctx.closePath();
        ctx.fill();

        ctx.strokeStyle = cfg.silhueta;
        ctx.lineWidth = 2.4;
        for (const v of [[-.34, .66, -.4, .86], [-.1, .5, -.02, .72], [.16, .52, .24, .74]]) {
            ctx.beginPath();
            ctx.moveTo(cx + l * v[0], base - h * v[1]);
            ctx.quadraticCurveTo(cx + l * v[2], base - h * (v[3] + .06), cx + l * v[2], base - h * v[3]);
            ctx.stroke();
        }

        // --- o BRILHO do chão, subindo. Não há poça desenhada: o veneno se acumulou fora de quadro,
        //     abaixo da borda, e o que se vê daqui é só a luz dele batendo no ar.
        //
        //     Meia elipse (de Math.PI a 0) por isso mesmo — a metade de baixo não existe, ela está
        //     além do rodapé. Uma poça desenhada obrigava a decidir a forma dela, o reflexo, a
        //     borda; o brilho diz a mesma coisa e não tem forma pra errar.
        const subindo = ctx.createRadialGradient(cx, base, 0, cx, base, l * .95);
        subindo.addColorStop(0, `rgba(${cfg.veneno}, ${.3 + Math.sin(t * 1.3) * .05})`);
        subindo.addColorStop(.4, `rgba(${cfg.veneno}, .11)`);
        subindo.addColorStop(1, `rgba(${cfg.veneno}, 0)`);
        ctx.fillStyle = subindo;
        ctx.beginPath();
        ctx.ellipse(cx, base, l * .95, h * .62, 0, Math.PI, 0);
        ctx.fill();

        // --- o veneno escorrendo: uma BARRA que desce e cai no buraco.
        //
        // A primeira versão crescia de cima pra baixo e apagava no ar. A segunda descia inteira e
        // ficava pendurada. Esta é a que o Gabriel descreveu, e é a que tem física: a CABEÇA desce
        // primeiro (o jorro saindo), depois a fonte fecha e a CAUDA desce atrás — a barra encurta
        // por cima até sumir dentro da poça. Nada aparece nem some no meio do ar.
        // Desenhado como as CHAMAS logo acima — gradiente que apaga nas pontas, e não um traço de
        // caneta. O risco opaco de contorno fixo lia como cabo pendurado; o gradiente lê como
        // líquido, que é o mesmo vocabulário do resto da ruína. E some a bola da ponta: gota
        // redonda só existe quando o líquido se solta, e este não se solta — ele escorre.
        // Cada fio sai de um ponto PRÓPRIO da parede rasgada — alturas embaralhadas, não uma escada
        // regular. Era `.42 - i * .03`, que dava quatro origens em degrau e denunciava a fórmula;
        // com alturas diferentes de verdade, os fios acabam de escorrer em momentos diferentes e a
        // parede parece furada em vários lugares, que é o que ela é.
        const fios = [
            { x: -.34, y: .5 }, { x: -.12, y: .36 }, { x: .13, y: .63 }, { x: .33, y: .44 },
        ];
        for (let i = 0; i < fios.length; i++) {
            const x = cx + l * fios[i].x;
            const y0 = base - h * fios[i].y;
            const queda = base - y0;

            const ciclo = 3.6 + i * .8;                     // cada fio no seu tempo
            const fase = ((t + i * 1.9) % ciclo) / ciclo;

            // 45% do ciclo a cabeça descendo, 45% a cauda alcançando, 10% de pausa seca
            const cabeca = Math.min(1, fase / .45);
            const cauda = Math.max(0, Math.min(1, (fase - .45) / .45));
            if (cauda >= 1) continue;                        // já caiu inteiro: nada a desenhar

            const yCauda = y0 + queda * cauda;
            const yCabeca = y0 + queda * cabeca;
            const larg = l * .012;

            const fio = ctx.createLinearGradient(x, yCauda, x, yCabeca);
            fio.addColorStop(0, `rgba(${cfg.veneno}, 0)`);
            fio.addColorStop(.3, `rgba(${cfg.veneno}, .38)`);
            fio.addColorStop(1, `rgba(${cfg.venenoClaro}, .72)`);
            ctx.fillStyle = fio;

            // afina pra cima e engrossa na frente, como um fio de líquido escorrendo de verdade
            ctx.beginPath();
            ctx.moveTo(x - larg * .45, yCauda);
            ctx.quadraticCurveTo(x - larg, (yCauda + yCabeca) / 2, x - larg * .9, yCabeca - larg);
            ctx.quadraticCurveTo(x, yCabeca + larg * .8, x + larg * .9, yCabeca - larg);
            ctx.quadraticCurveTo(x + larg, (yCauda + yCabeca) / 2, x + larg * .45, yCauda);
            ctx.closePath();
            ctx.fill();
        }

        ctx.restore();
    };
}

/// O MOTOR de tudo que fica PRESO NO HORIZONTE: as corujas da mata, as bobinas do laboratório, os
/// espantalhos entre as lápides. Duplicar estas 40 linhas por cliente seria abrir a porta pra eles
/// divergirem em silêncio.
///
/// O que cada tema traz é só: de que ladrilho ler a posição, onde estão os pontos, e o que desenhar.
///
/// O RELÓGIO é opcional: quem declara `aceso` pisca (coruja, bobina), quem não declara está sempre
/// aceso (espantalho). É a ausência do campo que decide — assim "não pisca" não precisou de
/// configuração nenhuma, e não há um `piscando: false` pra alguém esquecer de casar com o resto.
function criarNoHorizonte(cfg, canvas, desenhar) {
    const pisca = cfg.aceso !== undefined;
    const arena = document.getElementById('arena');
    let pontos = [];
    let assinatura = '';

    const remontar = () => {
        const estilo = getComputedStyle(arena);
        const passo = parseFloat(estilo.getPropertyValue(cfg.ladrilho[0])) || 320;
        const altura = parseFloat(estilo.getPropertyValue(cfg.ladrilho[1])) || 190;

        // Só refaz quando a GEOMETRIA muda. Refazer sempre destruiria o relógio de cada um a cada
        // conferida, e aí nenhum chegaria a acender — piscariam do zero pra sempre.
        const agora = `${canvas.width}|${canvas.height}|${passo}|${altura}`;
        if (agora === assinatura) return;
        assinatura = agora;

        const baseY = canvas.height - altura;   // o ladrilho do horizonte é ancorado no rodapé

        pontos = [];
        for (let tile = 0; tile * passo < canvas.width; tile++) {
            for (const p of cfg.pontos) {
                pontos.push({
                    // `lado` (quando existe) encosta a figura no flanco em vez de a espetar no meio.
                    x: tile * passo + p.x * passo + (p.lado ?? 0) * cfg.tamanho * .78,
                    y: baseY + p.y * altura,
                    lado: p.lado ?? 1,
                    // Quem pisca chega dormindo (a primeira vez demora, e cada um demora o seu);
                    // quem não pisca já nasce à vista.
                    aceso: !pisca,
                    resta: pisca ? entre(cfg.acordar) : Infinity,
                });
            }
        }
    };

    remontar();
    let conferir = 0;

    return (ctx, dt) => {
        // A arena muda de tamanho com a janela. Confere de vez em quando, e não a cada quadro,
        // porque getComputedStyle força layout — e o remontar só refaz de fato se algo mudou.
        conferir -= dt;
        if (conferir <= 0) { conferir = 1; remontar(); }

        for (const c of pontos) {
            if (pisca) {
                c.resta -= dt;
                if (c.resta <= 0) {
                    c.aceso = !c.aceso;
                    // Aceso: sempre o mesmo tempo. Apagado: sorteado, e é daqui que vem o desencontro.
                    c.resta = c.aceso ? cfg.aceso : entre(cfg.apagado);
                }
            }
            desenhar(ctx, c, cfg);
        }
    };
}

const criarCorujas = (cfg, canvas) => criarNoHorizonte(cfg, canvas,
    (ctx, c, k) => desenharCoruja(ctx, c.x, c.y, k.tamanho, c.lado, c.aceso, k));

const criarBobinas = (cfg, canvas) => criarNoHorizonte(cfg, canvas,
    (ctx, c, k) => c.aceso && desenharRaio(ctx, c.x, c.y, k.tamanho, k));

const criarEspantalhos = (cfg, canvas) => criarNoHorizonte(cfg, canvas,
    (ctx, c, k) => desenharEspantalho(ctx, c.x, c.y, k.tamanho, k));

/// A LINHA DO HORIZONTE do Reino: os topos por onde se anda, da esquerda pra direita.
///
/// Ela existe separada porque ganhou um SEGUNDO cliente. Enquanto só o castelo usava essas frações,
/// elas podiam morar soltas dentro dele; agora o ninja pula de um telhado pro outro e precisa das
/// mesmas — e repetir `.74`, `1.12` e `.62` nos dois lugares seria garantir que um dia discordassem.
///
/// O topo em que se PISA é o do fuste (entre as ameias, não em cima delas), e a largura de pouso é a
/// do fuste: as duas saem do mesmo `alt` que o `desenharTorre` usa, então não há como divergirem.
function telhadosDoReino(cfg, l, h) {
    const hTorre = h * cfg.torre;
    const cl = hTorre * .62;
    const cx = l * .5;

    const torre = (x, alt, doCastelo = false) => ({ torre: true, doCastelo, x, alt, larg: alt * .26, y: h - alt });

    return [
        torre(l * .10, hTorre * .74),
        torre(l * .28, hTorre * .74),
        torre(cx - cl, hTorre * 1.12, true),
        // O telhado do castelo: a corrida LARGA no meio do skyline. Encolhido nas pontas (`cl * 1.44`
        // em vez de `cl * 2`) pra o ninja não acabar pisando dentro das duas torres que o ladeiam —
        // elas ficam nas QUINAS do telhado, então a corrida tem que parar antes do fuste de cada uma.
        // O fator sai da conta: a torre ocupa até `cl - alt * .13`, e `.72` fecha com folga.
        //
        // Esta entrada carrega junto as medidas do CORPO do castelo (`cl`, `alt`), porque o telhado é
        // literalmente o topo dele. É o que deixa o `criarCastelo` desenhar a fachada sem reescrever
        // o `.62` — a fração continua existindo num lugar só.
        { torre: false, doCastelo: false, x: cx, larg: cl * 1.44, cl, alt: hTorre, y: h - hTorre },
        torre(cx + cl, hTorre * 1.12, true),
        torre(l * .72, hTorre * .74),
        torre(l * .90, hTorre * .74),
    ].sort((a, b) => a.x - b.x);
}

/// O NINJA nos telhados — a referência do 🥷 no cenário dele.
///
/// A ESCALA mandou no traço, como sempre. A torre tem ~9% da altura da tela de largura, então o ninja
/// cabe em ~22px, e anatomia nesse tamanho vira sujeira (é a mesma conta que já derrubou toda
/// tentativa de "melhorar" uma peça de horizonte). Só que silhueta preta CHAPADA também não servia: contra o céu
/// claro ela leria como recorte de papel — exatamente o motivo de os exércitos terem deixado de ser
/// pretos e ganhado material. A saída é o meio: quatro formas (cabeça, corpo, duas pernas em tesoura,
/// sem braços — braço a 22px é ruído) num azul quase preto, que lê como preto mas fica DENTRO do
/// quadro em vez de virar buraco. É a mesma correção que o pano do espantalho levou.
///
/// A FAIXA esvoaçando atrás é o que diz "ninja" e "velocidade" ao mesmo tempo, e ela funciona nesse
/// tamanho porque é forma em MOVIMENTO, não anatomia. É também o rastro de sombra pedido — só que
/// preso nele em vez de solto no telhado.
///
/// E a BOMBA DE FUMAÇA é a estrela, não o boneco: sumir num lugar e brotar noutro é o que conta a
/// história, e esse evento é legível mesmo quando a figura não é. Fumaça branca contra céu de dia
/// se lê de longe — é a peça que menos depende da escala.
///
/// Diretor de uma fase por vez, como os exércitos:
///   correndo → pulando → correndo → … → fumaça → oculto → surgindo em OUTRO telhado → correndo
/// O que se sorteia é a ESPERA até o próximo teleporte, nunca a duração de um gesto.
function criarNinja(base, canvas, castelo) {
    let telhados = [];
    let assinatura = '';

    // As medidas dele chegam em FRAÇÃO da altura da tela e viram px aqui, uma vez por
    // redimensionamento. Em px fixos o ninja seria a única coisa da cena que não escala com o
    // castelo: numa janela baixa a torre tem 23px de largura e ele não caberia em cima dela; numa
    // alta ele viraria um grão. Resolver pra dentro do mesmo nome deixa o resto do código igual.
    let cfg = base;

    let i = 0;                  // em que telhado ele está
    let destino = 0;            // pra qual ele está pulando
    let dir = Math.random() < .5 ? 1 : -1;
    let x = 0, y = 0;
    // Ele COMEÇA FORA, e invisível. A batalha abre num castelo vazio, e ele chega depois — chegar é
    // um acontecimento, estar ali desde sempre não é. A primeira espera é a metade de uma normal só
    // pra a primeira aparição não demorar meia partida.
    let fase = 'oculto';
    let alfa = 0;
    let perna = 0;
    let salto = null;
    let relogio = entre(cfg.fora) * .5;
    let emCena = 0;
    let saida = 'fumaca';       // como esta visita vai terminar; sorteado na entrada (ver `naBorda`)
    let rastro = [];
    let fumacas = [];

    const remontar = () => {
        const agora = `${canvas.width}|${canvas.height}`;
        if (agora === assinatura) return;
        assinatura = agora;

        const esc = canvas.height;
        cfg = {
            ...base,
            tamanho: esc * base.tamanho,
            velocidade: esc * base.velocidade,
            arco: esc * base.arco,
            fumacaRaio: esc * base.fumacaRaio,
        };

        telhados = telhadosDoReino(castelo, canvas.width, canvas.height);
        i = Math.min(i, telhados.length - 1);
        destino = Math.min(destino, telhados.length - 1);

        // A janela mudou de tamanho debaixo dele: recoloca em cima do telhado atual em vez de
        // deixá-lo andando no ar. O rastro é jogado fora porque ele descreve posições que já não
        // existem — mantê-lo desenharia uma fita ligando o ninja a um lugar que sumiu.
        const t = telhados[i];
        x = Math.min(Math.max(x, t.x - t.larg * .5), t.x + t.larg * .5);
        y = t.y;
        rastro = [];
        if (fase === 'pulando') fase = 'correndo';
    };

    const soltarFumaca = (fx, fy) => fumacas.push({
        x: fx, y: fy, t: 0,
        bolhas: Array.from({ length: cfg.bolhas }, () => ({
            ang: Math.random() * Math.PI * 2,
            dist: entre([.25, 1]),
            r: entre([.5, 1]),
            giro: (Math.random() - .5) * 1.5,
        })),
    });

    const pularPara = (k) => {
        const alvo = telhados[k];
        // Ele pousa logo DENTRO da borda de chegada, não em cima dela: pousar na quina faz o passo
        // seguinte já sair da plataforma, e o ninja pisca entre correr e pular.
        const x1 = dir > 0 ? alvo.x - alvo.larg * .5 + cfg.tamanho * .4
                           : alvo.x + alvo.larg * .5 - cfg.tamanho * .4;
        destino = k;
        salto = {
            x0: x, y0: y, x1, y1: alvo.y, p: 0,
            arco: cfg.arco,
            // A duração sai da DISTÂNCIA, não de um número fixo: pulo curto entre torres vizinhas e
            // pulo longo pro castelo têm que parecer o mesmo salto, e não o mesmo tempo.
            dura: Math.max(.42, Math.abs(x1 - x) / cfg.velocidade * .78),
        };
        fase = 'pulando';
    };

    /// O salto que NÃO tem pouso: ele se joga da última torre e sai da tela. Pode passar por cima do
    /// muro e sumir no canto — não há nada além da borda que precise dar conta dele.
    const sairDaTela = () => {
        const alvo = dir > 0 ? canvas.width + cfg.tamanho * 6 : -cfg.tamanho * 6;
        salto = {
            x0: x, y0: y, x1: alvo, y1: y + canvas.height * .2, p: 0,
            arco: cfg.arco * 2.4,               // pulo de saída é o mais alto: é uma fuga, não um passo
            dura: Math.max(.55, Math.abs(alvo - x) / cfg.velocidade * .85),
        };
        fase = 'saindo';
    };

    /// Chegou na ponta do telhado. Quem manda é o RELÓGIO DE CENA: enquanto não zera, ele segue
    /// pulando (e dá meia-volta se acabou o skyline). Quando zera, ele vai embora — e COMO ele vai
    /// embora foi sorteado lá atrás, na hora em que entrou (ver `surgindo`):
    ///
    ///   'fumaca' — solta a bomba na primeira borda que encontrar e some ali mesmo.
    ///   'borda'  — ignora as bordas do meio, segue pulando até a PONTA do skyline e se joga de lá,
    ///              passando por cima do muro e sumindo fora da tela.
    ///
    /// Sortear o modo na ENTRADA, e não na hora de sair, é o que faz a saída pela ponta acontecer de
    /// verdade: decidindo só ao chegar numa borda, ele quase sempre estaria no meio quando o tempo
    /// zerasse, e a fuga pela lateral viraria acidente raro em vez de metade das saídas.
    const naBorda = () => {
        const k = i + dir;
        const naPonta = k < 0 || k >= telhados.length;

        if (emCena <= 0) {
            if (naPonta) { sairDaTela(); return; }
            if (saida === 'fumaca') { soltarFumaca(x, y - cfg.tamanho * .45); fase = 'fumaca'; return; }
            pularPara(k);       // 'borda': segue caminho até a ponta
            return;
        }
        if (naPonta) { dir = -dir; return; }
        pularPara(k);
    };

    remontar();
    let conferir = 0;

    return (ctx, dt) => {
        // Mesma razão do criarNoHorizonte: medir a cada quadro forçaria layout à toa, e o remontar
        // só refaz de fato quando a geometria mudou.
        conferir -= dt;
        if (conferir <= 0) { conferir = 1; remontar(); }
        if (!telhados.length) return;

        emCena -= dt;
        relogio -= dt;

        switch (fase) {
            case 'correndo': {
                const t = telhados[i];
                x += cfg.velocidade * dir * dt;
                perna += dt * 15;
                y = t.y;

                const esq = t.x - t.larg * .5, dirLim = t.x + t.larg * .5;
                if (dir > 0 && x >= dirLim - cfg.tamanho * .3) { x = dirLim - cfg.tamanho * .3; naBorda(); }
                else if (dir < 0 && x <= esq + cfg.tamanho * .3) { x = esq + cfg.tamanho * .3; naBorda(); }
                break;
            }

            // As duas parábolas são a MESMA conta: o pulo entre telhados e o salto de saída só
            // diferem no alvo e na altura do arco, e os dois vêm prontos dentro do `salto`.
            case 'pulando':
            case 'saindo': {
                salto.p = Math.min(1, salto.p + dt / salto.dura);
                const p = salto.p;
                x = salto.x0 + (salto.x1 - salto.x0) * p;
                // Parábola por cima da reta que liga os dois topos — assim o arco é o mesmo subindo
                // pra uma torre alta ou descendo pra uma baixa.
                y = salto.y0 + (salto.y1 - salto.y0) * p - salto.arco * 4 * p * (1 - p);
                perna += dt * 4;                            // pernas quase paradas no ar
                if (p >= 1) {
                    if (fase === 'saindo') { fase = 'oculto'; relogio = entre(cfg.fora); alfa = 0; rastro = []; }
                    else { i = destino; y = telhados[i].y; fase = 'correndo'; }
                }
                break;
            }

            case 'fumaca':
                alfa = Math.max(0, alfa - dt / cfg.sumir);
                if (alfa === 0) { fase = 'oculto'; relogio = entre(cfg.fora); rastro = []; }
                break;

            case 'oculto':
                if (relogio <= 0) {
                    // Brota em QUALQUER outro telhado, nunca no mesmo: reaparecer onde sumiu não é
                    // teleporte, é o ninja piscando.
                    let k = i;
                    if (telhados.length > 1) while (k === i) k = Math.floor(Math.random() * telhados.length);
                    i = k;
                    const t = telhados[i];
                    dir = Math.random() < .5 ? 1 : -1;
                    x = t.x + (Math.random() - .5) * t.larg * .5;
                    y = t.y;
                    soltarFumaca(x, y - cfg.tamanho * .45);
                    fase = 'surgindo';
                }
                break;

            case 'surgindo':
                alfa = Math.min(1, alfa + dt / cfg.surgir);
                if (alfa === 1) {
                    fase = 'correndo';
                    emCena = entre(cfg.emCena);
                    saida = Math.random() < cfg.sairPelaBorda ? 'borda' : 'fumaca';
                }
                break;
        }

        // --- a fumaça, viva por conta própria: ela tem que continuar abrindo DEPOIS de o ninja
        //     sumir e ANTES de ele aparecer, então não pode morar dentro de nenhuma fase.
        for (let k = fumacas.length - 1; k >= 0; k--) {
            const f = fumacas[k];
            f.t += dt;
            const p = f.t / cfg.fumacaDura;
            if (p >= 1) { fumacas.splice(k, 1); continue; }

            const abre = 1 - Math.pow(1 - p, 2.2);      // estoura rápido e desacelera
            const opaco = (1 - p) * (1 - p) * .5;
            for (const b of f.bolhas) {
                const d = cfg.fumacaRaio * b.dist * abre;
                ctx.fillStyle = `rgba(${cfg.fumaca}, ${opaco})`;
                ctx.beginPath();
                ctx.arc(f.x + Math.cos(b.ang + b.giro * abre) * d,
                    f.y + Math.sin(b.ang + b.giro * abre) * d * .78 - cfg.fumacaRaio * abre * .32,
                    cfg.fumacaRaio * b.r * (.3 + abre * .5), 0, Math.PI * 2);
                ctx.fill();
            }
        }

        if (alfa <= 0) return;

        if (fase === 'correndo' || fase === 'pulando' || fase === 'saindo') {
            rastro.push({ x, y });
            if (rastro.length > cfg.rastro) rastro.shift();
        }

        desenharNinja(ctx, x, y, perna, alfa, fase === 'pulando' || fase === 'saindo', rastro, cfg);
    };
}

/// O ninja em quatro formas mais a faixa. Tudo em fração de `t` (a altura dele), pra ele encolher
/// junto com a janela sem nenhuma conta solta.
function desenharNinja(ctx, x, y, perna, alfa, noAr, rastro, cfg) {
    const t = cfg.tamanho;

    ctx.save();
    ctx.lineCap = 'round';

    // --- a SOMBRA: as últimas posições viram um rastro que afina até sumir.
    //
    // Ela tem a ALTURA INTEIRA do ninja (`lineWidth` vai até `t`) e é centrada no meio do corpo
    // (`y - .5t`), então a faixa cobre exatamente dos pés à cabeça. Era uma fita fina saindo da nuca
    // — lia como cachecol. Do tamanho dele, lê como o que é: a sombra do corpo ficando pra trás.
    // Com o boneco menor, é ela que carrega a cena, e a figura só confirma de perto.
    for (let k = 1; k < rastro.length -1; k++) {
        const q = k / rastro.length;
        ctx.globalAlpha = alfa * q * q * .8;
        ctx.strokeStyle = `rgba(${cfg.faixa}, 1)`;
        ctx.lineWidth = t * q;
        ctx.beginPath();
        ctx.moveTo(rastro[k - 1].x, rastro[k - 1].y - t * .62);
        ctx.lineTo(rastro[k].x, rastro[k].y - t * .62);
        ctx.stroke();
    }

    ctx.globalAlpha = alfa;
    ctx.fillStyle = cfg.corpo;
    ctx.strokeStyle = cfg.corpo;

    // --- as pernas em tesoura. No ar elas se recolhem: o passo vira um agachamento, que é o que
    //     separa "pulando" de "correndo no vazio".
    const passo = noAr ? t * .16 : Math.sin(perna) * t * .3;
    ctx.lineWidth = t * .13;
    ctx.beginPath();
    ctx.moveTo(x, y - t * .34); ctx.lineTo(x + passo, y - (noAr ? t * .16 : 0));
    ctx.moveTo(x, y - t * .34); ctx.lineTo(x - passo, y - (noAr ? t * .2 : 0));
    ctx.stroke();

    // --- o corpo, afunilado do quadril pro ombro
    ctx.beginPath();
    ctx.moveTo(x - t * .17, y - t * .3);
    ctx.lineTo(x + t * .17, y - t * .3);
    ctx.lineTo(x + t * .12, y - t * .66);
    ctx.lineTo(x - t * .12, y - t * .66);
    ctx.closePath();
    ctx.fill();

    // --- a cabeça
    ctx.beginPath();
    ctx.arc(x, y - t * .78, t * .15, 0, Math.PI * 2);
    ctx.fill();

    ctx.restore();
}

/// A CIDADE MURADA do Reino: a muralha atravessando a tela, torres com telhado cônico e bandeira, o
/// castelo no meio e as casas apinhadas ATRÁS do muro — só telhado e janela acesa aparecendo por
/// cima dele, que é como se vê uma cidade murada de fora. Casa inteira à vista significaria que não
/// há muro nenhum.
///
/// Tudo estático: aqui o que se move são os exércitos, e cenário que também mexe brigaria com eles.
/// A vida vem das JANELAS ACESAS, que são muitas e irregulares.
function criarCastelo(cfg, canvas) {
    // Sorteados UMA vez: telhado e tamanho de casa sorteados por quadro fariam a cidade inteira
    // tremeluzir. E as nuvens guardam a própria posição porque ela ANDA.
    let cena = null;

    const montar = (l) => ({
        l,
        casas: Array.from({ length: cfg.casas }, (_, i) => ({
            x: (i + .5) / cfg.casas,
            alt: .5 + Math.random() * .5,
            larg: .55 + Math.random() * .5,
            azul: Math.random() < .4,
        })),
        nuvens: Array.from({ length: cfg.nuvens }, () => ({
            x: Math.random() * l,
            y: .06 + Math.random() * .3,
            r: .04 + Math.random() * .05,
            v: .6 + Math.random() * .8,
        })),
    });

    return (ctx, dt) => {
        const l = canvas.width, base = canvas.height, h = canvas.height;
        const hMuro = h * cfg.muro;
        const hTorre = h * cfg.torre;
        const ameia = hMuro * .17;

        if (!cena || cena.l !== l) cena = montar(l);

        ctx.save();

        // --- as nuvens, andando devagar lá em cima
        for (const n of cena.nuvens) {
            n.x += cfg.vento * n.v * dt;
            if (n.x - h * n.r * 3 > l) n.x = -h * n.r * 3;
            desenharNuvem(ctx, n.x, h * n.y, h * n.r, cfg);
        }

        // --- os morros ao longe, azulados pela distância
        ctx.fillStyle = cfg.morro;
        ctx.beginPath();
        ctx.moveTo(0, base - hMuro * .9);
        for (let i = 0; i <= 6; i++) {
            const x = (i / 6) * l;
            const alt = hMuro * (.9 + Math.sin(i * 1.7) * .34);
            ctx.quadraticCurveTo(x - l / 12, base - alt - hMuro * .3, x, base - alt);
        }
        ctx.lineTo(l, base); ctx.lineTo(0, base);
        ctx.closePath();
        ctx.fill();

        // --- as casas, ATRÁS do muro: telhado + parede, e só o alto aparecendo
        for (const c of cena.casas) {
            const cx = c.x * l, cl = hMuro * .34 * c.larg, ct = hMuro * .5 * c.alt;
            const teto = base - hMuro * .82 - ct;

            ctx.fillStyle = cfg.sombra;
            ctx.fillRect(cx - cl * .8, teto + ct * .5, cl * 1.6, ct);

            ctx.fillStyle = c.azul ? cfg.telhadoAlt : cfg.telhado;
            ctx.beginPath();
            ctx.moveTo(cx - cl, teto + ct * .5);
            ctx.lineTo(cx, teto);
            ctx.lineTo(cx + cl, teto + ct * .5);
            ctx.closePath();
            ctx.fill();

            ctx.fillStyle = cfg.janela;
            ctx.fillRect(cx - cl * .16, teto + ct * .74, cl * .32, ct * .3);
        }

        // --- a muralha: pedra clara, com uma faixa de sombra embaixo pra ela ter espessura
        ctx.fillStyle = cfg.pedra;
        ctx.fillRect(0, base - hMuro, l, hMuro);
        ctx.fillStyle = cfg.sombra;
        ctx.fillRect(0, base - hMuro * .22, l, hMuro * .22);
        ctx.fillStyle = cfg.pedra;
        for (let x = 0; x < l; x += ameia * 2) ctx.fillRect(x, base - hMuro - ameia * .8, ameia, ameia * .8);

        // as juntas da pedra, em duas fileiras — o que impede o muro de ler como bloco chapado
        ctx.strokeStyle = 'rgba(0, 0, 0, .1)';
        ctx.lineWidth = 1;
        for (const f of [.34, .66]) {
            ctx.beginPath();
            ctx.moveTo(0, base - hMuro * f); ctx.lineTo(l, base - hMuro * f);
            ctx.stroke();
        }

        // As torres da muralha. As posições e alturas vêm do `telhadosDoReino` — o mesmo lugar de
        // onde o ninja tira por onde pular, então não há como o telhado desenhado e o telhado
        // pisado discordarem. As do CASTELO ficam pra depois: elas pintam por cima da fachada.
        const telhados = telhadosDoReino(cfg, l, h);
        for (const t of telhados) if (t.torre && !t.doCastelo) desenharTorre(ctx, t.x, base, t.alt, cfg);

        // --- o castelo no meio
        const topoDoCastelo = telhados.find(t => !t.torre);
        const cx = topoDoCastelo.x, cl = topoDoCastelo.cl;
        ctx.fillStyle = cfg.pedra;
        ctx.fillRect(cx - cl, base - hTorre, cl * 2, hTorre);
        ctx.fillStyle = cfg.sombra;
        ctx.fillRect(cx + cl * .62, base - hTorre, cl * .38, hTorre);   // a face que não pega sol
        ctx.fillStyle = cfg.pedra;
        for (let x = cx - cl; x < cx + cl; x += ameia * 2) ctx.fillRect(x, base - hTorre - ameia * .8, ameia, ameia * .8);

        ctx.fillStyle = cfg.janela;
        for (let fila = 0; fila < 3; fila++) {
            for (let j = -2; j <= 2; j++) {
                ctx.fillRect(cx + j * cl * .34 - cl * .05, base - hTorre * (.82 - fila * .22), cl * .1, hTorre * .1);
            }
        }

        // o portão, com o arco de pedra em volta
        ctx.fillStyle = cfg.sombra;
        ctx.beginPath();
        ctx.moveTo(cx - cl * .3, base);
        ctx.lineTo(cx - cl * .3, base - hMuro * .56);
        ctx.quadraticCurveTo(cx, base - hMuro * .92, cx + cl * .3, base - hMuro * .56);
        ctx.lineTo(cx + cl * .3, base);
        ctx.closePath();
        ctx.fill();
        ctx.fillStyle = '#2a2036';
        ctx.beginPath();
        ctx.moveTo(cx - cl * .22, base);
        ctx.lineTo(cx - cl * .22, base - hMuro * .5);
        ctx.quadraticCurveTo(cx, base - hMuro * .82, cx + cl * .22, base - hMuro * .5);
        ctx.lineTo(cx + cl * .22, base);
        ctx.closePath();
        ctx.fill();

        for (const t of telhados) if (t.doCastelo) desenharTorre(ctx, t.x, base, t.alt, cfg);

        // --- a grama do campo, na frente de tudo: é onde os exércitos pisam
        ctx.fillStyle = cfg.grama;
        ctx.fillRect(0, base - h * .06, l, h * .06);
        ctx.fillStyle = cfg.gramaSombra;
        ctx.fillRect(0, base - h * .06, l, h * .012);

        ctx.restore();
    };
}

/// Uma nuvem: três bolhas sobrepostas com a base achatada. Achatar embaixo é o que a faz flutuar em
/// vez de boiar — nuvem de dia tem fundo reto.
function desenharNuvem(ctx, x, y, r, cfg) {
    ctx.save();
    ctx.fillStyle = `rgba(${cfg.nuvem}, .82)`;
    ctx.beginPath();
    ctx.ellipse(x - r, y, r * .9, r * .6, 0, Math.PI, 0);
    ctx.ellipse(x, y, r * 1.3, r * .95, 0, Math.PI, 0);
    ctx.ellipse(x + r * 1.1, y, r * .8, r * .55, 0, Math.PI, 0);
    ctx.fillRect(x - r * 1.9, y - 1, r * 3.8, 2);
    ctx.fill();
    ctx.restore();
}

/// Uma torre: fuste, ameias, telhado cônico e a bandeira. O cone e a bandeira são o que separam
/// "torre de castelo" de "cilindro em pé".
function desenharTorre(ctx, x, base, h, cfg) {
    const l = h * .26;

    ctx.save();
    ctx.fillStyle = cfg.pedra;
    ctx.fillRect(x - l * .5, base - h, l, h);
    ctx.fillStyle = cfg.sombra;
    ctx.fillRect(x + l * .18, base - h, l * .32, h);   // o lado sem sol, que dá volume ao cilindro

    // ameias
    const ameia = l * .3;
    ctx.fillStyle = cfg.pedra;
    for (let k = 0; k < 3; k++) ctx.fillRect(x - l * .5 + k * ameia * 1.6, base - h - ameia * .7, ameia, ameia * .7);

    // telhado cônico
    ctx.fillStyle = cfg.telhado;
    ctx.beginPath();
    ctx.moveTo(x - l * .72, base - h - ameia * .7);
    ctx.lineTo(x, base - h - l * 1.5);
    ctx.lineTo(x + l * .72, base - h - ameia * .7);
    ctx.closePath();
    ctx.fill();
    ctx.fillStyle = 'rgba(0, 0, 0, .16)';
    ctx.beginPath();
    ctx.moveTo(x, base - h - l * 1.5);
    ctx.lineTo(x + l * .72, base - h - ameia * .7);
    ctx.lineTo(x, base - h - ameia * .7);
    ctx.closePath();
    ctx.fill();

    // mastro e bandeira
    ctx.fillStyle = cfg.sombra;
    ctx.fillRect(x - l * .035, base - h - l * 2.1, l * .07, l * .62);
    ctx.fillStyle = cfg.bandeira;
    ctx.beginPath();
    ctx.moveTo(x + l * .035, base - h - l * 2.1);
    ctx.lineTo(x + l * .62, base - h - l * 1.9);
    ctx.lineTo(x + l * .035, base - h - l * 1.7);
    ctx.closePath();
    ctx.fill();

    // a fresta de tiro
    ctx.fillStyle = cfg.janela;
    ctx.fillRect(x - l * .07, base - h * .78, l * .14, h * .12);

    ctx.restore();
}

/// Um espantalho: cruz de madeira, pano esfarrapado pendurado e cabeça de abóbora.
///
/// A abóbora é o ÚNICO ponto de cor do cemitério inteiro, e por isso é de baixa saturação — laranja
/// aceso aqui brigaria com a lua e roubaria a cena. A cara fica um tom acima, como se houvesse uma
/// vela dentro: é a diferença entre os dois laranjas que faz ler como abóbora entalhada, e não como
/// uma bola laranja espetada num poste.
function desenharEspantalho(ctx, x, base, s, cfg) {
    ctx.save();
    ctx.translate(x, base);
    ctx.scale(s, s);

    // a cruz
    ctx.fillStyle = cfg.poste;
    ctx.fillRect(-.05, -1.5, .1, 1.5);
    ctx.fillRect(-.62, -1.12, 1.24, .085);

    // o pano: ombros caídos e barra rasgada em dentes
    ctx.fillStyle = cfg.pano;
    ctx.beginPath();
    ctx.moveTo(-.58, -1.1);
    ctx.quadraticCurveTo(0, -1.22, .58, -1.1);
    ctx.lineTo(.44, -.36);
    ctx.lineTo(.3, -.52); ctx.lineTo(.16, -.3);
    ctx.lineTo(.02, -.5); ctx.lineTo(-.14, -.28);
    ctx.lineTo(-.3, -.48); ctx.lineTo(-.44, -.34);
    ctx.closePath();
    ctx.fill();

    // trapos esvoaçando nas pontas dos braços
    ctx.beginPath();
    ctx.moveTo(-.62, -1.14); ctx.lineTo(-.78, -.98); ctx.lineTo(-.58, -1.0);
    ctx.moveTo(.62, -1.14); ctx.lineTo(.78, -.98); ctx.lineTo(.58, -1.0);
    ctx.fill();

    // a abóbora
    const cy = -1.42, r = .3;
    ctx.fillStyle = `rgb(${cfg.abobora})`;
    ctx.beginPath();
    ctx.ellipse(0, cy, r, r * .92, 0, 0, Math.PI * 2);
    ctx.fill();

    // os gomos, escurecidos por cima da própria cor
    ctx.strokeStyle = 'rgba(0, 0, 0, .28)';
    ctx.lineWidth = .028;
    for (const g of [-.16, 0, .16]) {
        ctx.beginPath();
        ctx.ellipse(g, cy, Math.abs(g) === 0 ? r * .34 : r * .2, r * .9, 0, 0, Math.PI * 2);
        ctx.stroke();
    }

    // o cabinho
    ctx.fillStyle = cfg.poste;
    ctx.fillRect(-.035, cy - r * .96 - .1, .07, .12);

    // a cara acesa: dois olhos e a boca de dentes
    ctx.fillStyle = `rgb(${cfg.cara})`;
    ctx.beginPath();
    ctx.moveTo(-.16, cy - .04); ctx.lineTo(-.05, cy - .04); ctx.lineTo(-.105, cy - .14);
    ctx.moveTo(.16, cy - .04); ctx.lineTo(.05, cy - .04); ctx.lineTo(.105, cy - .14);
    ctx.fill();
    ctx.beginPath();
    ctx.moveTo(-.17, cy + .08);
    ctx.lineTo(-.09, cy + .16); ctx.lineTo(-.03, cy + .08);
    ctx.lineTo(.03, cy + .16); ctx.lineTo(.09, cy + .08);
    ctx.lineTo(.17, cy + .16);
    ctx.lineTo(.12, cy + .2); ctx.lineTo(-.12, cy + .2);
    ctx.closePath();
    ctx.fill();

    ctx.restore();
}

/// A descarga da bobina de Tesla: uma coroa de raios saindo da bola, com o traço quebrando em
/// ziguezague. É redesenhada a cada quadro com ângulos NOVOS de propósito — raio parado no ar não
/// existe, e é o tremer que faz a faísca parecer elétrica em vez de desenhada.
function desenharRaio(ctx, x, y, tamanho, cfg) {
    ctx.save();

    // o halo da bola acesa
    const halo = ctx.createRadialGradient(x, y, 0, x, y, tamanho * 1.6);
    halo.addColorStop(0, `rgba(${cfg.cor}, .9)`);
    halo.addColorStop(.3, `rgba(${cfg.cor}, .35)`);
    halo.addColorStop(1, `rgba(${cfg.cor}, 0)`);
    ctx.fillStyle = halo;
    ctx.beginPath();
    ctx.arc(x, y, tamanho * 1.6, 0, Math.PI * 2);
    ctx.fill();

    ctx.strokeStyle = `rgba(${cfg.cor}, .85)`;
    ctx.lineWidth = 1.3;
    ctx.lineCap = 'round';

    for (let r = 0; r < 4; r++) {
        const angulo = -Math.PI / 2 + (Math.random() - .5) * 2.4;
        const alcance = tamanho * (1.4 + Math.random() * 1.6);

        ctx.beginPath();
        ctx.moveTo(x, y);
        // Três quebras: menos que isso vira risco reto, mais vira novelo.
        for (let k = 1; k <= 3; k++) {
            const d = alcance * (k / 3);
            const desvio = (Math.random() - .5) * tamanho * .7;
            ctx.lineTo(x + Math.cos(angulo) * d - Math.sin(angulo) * desvio,
                       y + Math.sin(angulo) * d + Math.cos(angulo) * desvio);
        }
        ctx.stroke();
    }

    ctx.restore();
}

/// Uma coruja empoleirada: silhueta parada, olhos que acendem. O corpo fica SEMPRE visível — ela
/// está ali o tempo todo, e é isso que faz o olho acender ser um bicho olhando, em vez de duas luzes
/// surgindo do nada no meio do mato.
///
/// `lado` vira a cabeça pro lado de fora da árvore (ela está na beirada do tronco, olhando pro
/// campo), o que de quebra faz as duas corujas do ladrilho não serem a mesma figura repetida.
function desenharCoruja(ctx, x, y, s, lado, aceso, cfg) {
    ctx.save();
    ctx.translate(x, y);
    ctx.scale(s, s);
    ctx.fillStyle = cfg.corpo;

    // Corpo em gota: cabeça larga sem pescoço, que é o que faz uma silhueta ler como coruja.
    ctx.beginPath();
    ctx.moveTo(0, -1);
    ctx.bezierCurveTo(.92, -.98, 1.06, .2, .68, .98);
    ctx.quadraticCurveTo(0, 1.18, -.68, .98);
    ctx.bezierCurveTo(-1.06, .2, -.92, -.98, 0, -1);
    ctx.closePath();
    ctx.fill();

    // Tufos de orelha — dois espetinhos, e é o detalhe que descarta "passarinho qualquer".
    ctx.beginPath();
    ctx.moveTo(-.62, -.72); ctx.lineTo(-.78, -1.32); ctx.lineTo(-.24, -.94);
    ctx.moveTo(.62, -.72); ctx.lineTo(.78, -1.32); ctx.lineTo(.24, -.94);
    ctx.fill();

    // Um pé de galho sob ela, pra não parecer flutuando colada no tronco.
    ctx.fillRect(-.5, .96, 1, .16);

    if (!aceso) { ctx.restore(); return; }

    // Os olhos, com halo: o halo é o que faz ler como brilho no escuro em vez de dois pixels acesos.
    // Levemente deslocados pro lado de fora — a cabeça está virada pro campo.
    for (const olho of [-1, 1]) {
        const ox = olho * .32 + lado * .1;
        const oy = -.38;
        const g = ctx.createRadialGradient(ox, oy, 0, ox, oy, .62);
        g.addColorStop(0, `rgba(${cfg.olho}, .98)`);
        g.addColorStop(.3, `rgba(${cfg.olho}, .5)`);
        g.addColorStop(1, `rgba(${cfg.olho}, 0)`);
        ctx.fillStyle = g;
        ctx.beginPath();
        ctx.arc(ox, oy, .62, 0, Math.PI * 2);
        ctx.fill();
    }

    ctx.restore();
}

/// Quem pode atravessar o céu. Assinatura única de propósito — cada um usa o que precisa e ignora o
/// resto —, porque o motor de voo não tem que saber o que está carregando.
const VOADORES = {
    morcego: (ctx, x, y, s, fase, paraDireita, canvas, cfg) =>
        desenharMorcego(ctx, x, y, s, fase, paraDireita, cfg.cor),
    disco: (ctx, x, y, s, fase, paraDireita, canvas, cfg) =>
        desenharDisco(ctx, x, y, s, fase, canvas, cfg),
    fantasma: (ctx, x, y, s, fase, paraDireita, canvas, cfg) =>
        desenharFantasma(ctx, x, y, s, fase, paraDireita, cfg),
};

/// Um FANTASMA: manto arredondado em cima, barra ondulando embaixo, dois vazios no lugar dos olhos.
///
/// Ele é translúcido e tem halo — sem isso viraria um pinguim branco. E a barra ONDULA pelo tempo,
/// não pela posição: é o que faz o manto parecer flutuar mesmo quando ele está quase parado.
function desenharFantasma(ctx, x, y, s, fase, paraDireita, cfg) {
    ctx.save();
    ctx.translate(x, y);
    ctx.scale(paraDireita ? s : -s, s);

    // o halo — a assombração acende o ar em volta
    const halo = ctx.createRadialGradient(0, 0, 0, 0, 0, 1.5);
    halo.addColorStop(0, `rgba(${cfg.cor}, .16)`);
    halo.addColorStop(1, `rgba(${cfg.cor}, 0)`);
    ctx.fillStyle = halo;
    ctx.beginPath();
    ctx.arc(0, 0, 1.5, 0, Math.PI * 2);
    ctx.fill();

    // o manto: cúpula em cima, três badalos ondulando embaixo
    ctx.fillStyle = `rgba(${cfg.cor}, .5)`;
    ctx.beginPath();
    ctx.arc(0, -.1, .62, Math.PI, 0);
    ctx.lineTo(.62, .42);
    for (let i = 0; i < 3; i++) {
        const largura = 1.24 / 3;
        const x0 = .62 - i * largura;
        const balanco = Math.sin(fase * .9 + i * 1.3) * .13;
        ctx.quadraticCurveTo(x0 - largura * .5, .78 + balanco, x0 - largura, .42 + balanco * .4);
    }
    ctx.lineTo(-.62, -.1);
    ctx.closePath();
    ctx.fill();

    // os olhos: VAZIOS recortados, não pintados de preto — assim eles são o fundo aparecendo
    ctx.globalCompositeOperation = 'destination-out';
    ctx.beginPath();
    ctx.ellipse(-.22, -.16, .12, .17, 0, 0, Math.PI * 2);
    ctx.ellipse(.22, -.16, .12, .17, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.globalCompositeOperation = 'source-over';

    ctx.restore();
}

/// O DISCO VOADOR do 👽 e do 👾: casco em silhueta, cúpula, luzes girando na barriga e o FEIXE
/// varrendo o chão. O feixe é o que faz o disco pertencer à cena em vez de ser um adesivo colado no
/// céu — ele toca o campo, então há uma nave ali de verdade.
///
/// `fase` é a mesma variável que bate a asa do morcego. Aqui ela gira as luzes: um motor de voo só,
/// dois bichos.
function desenharDisco(ctx, x, y, s, fase, canvas, cfg) {
    ctx.save();

    // O feixe primeiro, pra o casco pousar por cima dele.
    const alcanceY = canvas.height - y;
    const feixe = ctx.createLinearGradient(x, y, x, canvas.height);
    feixe.addColorStop(0, `rgba(${cfg.luz}, .3)`);
    feixe.addColorStop(.55, `rgba(${cfg.luz}, .08)`);
    feixe.addColorStop(1, `rgba(${cfg.luz}, 0)`);
    ctx.fillStyle = feixe;
    ctx.beginPath();
    ctx.moveTo(x - s * .5, y);
    ctx.lineTo(x + s * .5, y);
    ctx.lineTo(x + s * .5 + alcanceY * .16, canvas.height);
    ctx.lineTo(x - s * .5 - alcanceY * .16, canvas.height);
    ctx.closePath();
    ctx.fill();

    ctx.translate(x, y);
    ctx.fillStyle = cfg.cor;

    // casco: elipse achatada
    ctx.beginPath();
    ctx.ellipse(0, 0, s, s * .3, 0, 0, Math.PI * 2);
    ctx.fill();

    // cúpula
    ctx.beginPath();
    ctx.ellipse(0, -s * .16, s * .42, s * .34, 0, Math.PI, 0);
    ctx.fill();

    // o brilho da cúpula — o único ponto claro, e o que diz "tem alguém pilotando"
    const vidro = ctx.createRadialGradient(0, -s * .3, 0, 0, -s * .3, s * .38);
    vidro.addColorStop(0, `rgba(${cfg.luz}, .75)`);
    vidro.addColorStop(1, `rgba(${cfg.luz}, 0)`);
    ctx.fillStyle = vidro;
    ctx.beginPath();
    ctx.arc(0, -s * .3, s * .38, 0, Math.PI * 2);
    ctx.fill();

    // as luzes da barriga, girando: a mais próxima da frente é a mais acesa
    for (let i = 0; i < 5; i++) {
        const a = fase * .7 + i * (Math.PI * 2 / 5);
        const lx = Math.cos(a) * s * .68;
        const brilho = (Math.sin(a) + 1) / 2;   // some ao passar por trás do casco
        ctx.fillStyle = `rgba(${cfg.luz}, ${.15 + brilho * .75})`;
        ctx.beginPath();
        ctx.arc(lx, s * .16, s * .075, 0, Math.PI * 2);
        ctx.fill();
    }

    ctx.restore();
}

/// Um morcego, em curvas. O bater de asa é o seno abrindo e fechando a PONTA (o corpo fica parado) —
/// é o que dá vida com duas linhas em vez de uma folha de sprites.
function desenharMorcego(ctx, x, y, s, faseDaAsa, paraDireita, cor) {
    const bate = Math.sin(faseDaAsa);           // -1 asa embaixo, +1 asa em cima
    const alto = -0.55 - bate * 0.75;           // altura da ponta da asa
    const meio = 0.10 - bate * 0.28;            // o "cotovelo", que segue a ponta com atraso

    ctx.save();
    ctx.translate(x, y);
    ctx.scale(paraDireita ? s : -s, s);         // espelha inteiro pra virar de lado
    ctx.fillStyle = cor;

    // corpo + orelhas
    ctx.beginPath();
    ctx.ellipse(0, 0, 0.24, 0.36, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.beginPath();
    ctx.moveTo(-0.14, -0.28); ctx.lineTo(-0.2, -0.6); ctx.lineTo(-0.02, -0.34);
    ctx.moveTo(0.14, -0.28); ctx.lineTo(0.2, -0.6); ctx.lineTo(0.02, -0.34);
    ctx.fill();

    // as duas asas, uma o espelho da outra
    for (const lado of [1, -1]) {
        ctx.beginPath();
        ctx.moveTo(lado * 0.12, -0.12);
        ctx.quadraticCurveTo(lado * 0.9, alto, lado * 1.75, alto * 0.55);   // borda de cima até a ponta
        ctx.quadraticCurveTo(lado * 1.25, meio + 0.30, lado * 1.02, meio);  // 1º festão
        ctx.quadraticCurveTo(lado * 0.82, meio + 0.34, lado * 0.6, meio - 0.04);
        ctx.quadraticCurveTo(lado * 0.42, meio + 0.30, lado * 0.16, 0.16);  // volta ao corpo
        ctx.closePath();
        ctx.fill();
    }

    ctx.restore();
}

// ---------- partida ----------
document.getElementById('alternarEstatisticas').classList.toggle('ativo', mostrarEstatisticas);
document.getElementById('alternarLog').classList.toggle('ativo', mostrarLog);
aplicarVelocidade();      // sincroniza o C# com o 2x inicial
mostrarCena('menu');      // o jogo sempre abre no menu — evita o flash da arena vazia
mandar('pronto');         // destrava a thread do jogo no C#
