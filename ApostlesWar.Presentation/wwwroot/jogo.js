// Apostle's War — a tela.
//
// Contrato com o C#: recebemos ESTADO (retrato completo: como tudo está agora) e EVENTOS (o que
// acabou de acontecer). O estado redesenha; o evento anima. Essa separação é o que permite a tela
// ser burra — ela nunca calcula regra de jogo, só pinta o que chegou.
//
// Fluxo de clique (desenho do Gabriel): clica na habilidade -> clica no inimigo -> USA.
// Sem habilidade escolhida, clicar num personagem só INSPECIONA (mostra ficha e status).

'use strict';

import { caixaRedonda, comListras, desenharChama, entre } from './cenarios/comum/basicos.js';
import { criarNevoa, criarPo, criarVoadores, desenharFantasma, desenharMorcego } from './cenarios/comum/ar.js';
import { criarNoHorizonte, medirDoTema, medirLadrilho } from './cenarios/comum/ladrilho.js';
import { ar as arDecaidos, criarArvoreDoMundo, criarBuracoDoInferno, criarColunaDeMorcegos, criarFogosDaVila, criarFumacaDoInferno, criarRespingosDoInferno, criarVeias } from './cenarios/decaidos/decaidos.js';
import { ar as arMisticos, criarDragao, criarGolfinhos, criarLampada, criarMar, criarPalmeiras, criarVagalumes } from './cenarios/misticos/misticos.js';
import { ar as arEspecial, criarBanheiro, criarSentados, criarTrex } from './cenarios/especial/especial.js';
import { ar as arApostolos, criarArvoreDeNatal, criarCortinas, criarLareira, criarPresentes, criarRoteiroDaNoite, criarVistaDaJanela } from './cenarios/apostolos/apostolos.js';
import { ar as arFolclore, criarAparicaoNaMoita, criarChifres, criarClava, criarFogueira, criarMoitas, criarRedemoinho } from './cenarios/folclore/folclore.js';
import { ar as arReino, criarCastelo, criarExercitos, criarNinja } from './cenarios/reino/reino.js';
import { ar as arLadosombrio, criarCaixao, criarCorujas, criarEspantalhos } from './cenarios/ladosombrio/ladosombrio.js';
import { ar as arTecnologicos, criarBobinas, criarRuina, criarTentaculos } from './cenarios/tecnologicos/tecnologicos.js';

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
//
// Ele nasce DESLIGADO (decisão do Gabriel): a batalha abre mostrando a luta, e quem quiser ler o
// que aconteceu liga. Quem liga, fica ligado — a escolha vale pela sessão inteira, e não volta a
// se esconder na batalha seguinte.
let mostrarLog = false;

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
export const AR_DO_TEMA = {
    reino: arReino,
    ladosombrio: arLadosombrio,
    tecnologicos: arTecnologicos,
    folclore: arFolclore,
    misticos: arMisticos,
    especial: arEspecial,
    decaidos: arDecaidos,
    apostolos: arApostolos,
};

let temaAtual = '';

export function aplicarTema(tema) {
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

    // O MAESTRO do tema, quando existe um. É um objeto só, com dois números: a força do sopro (o
    // SINAL é a direção) e onde está quem está soprando. Uma peça escreve — o redemoinho do Folclore —
    // e as outras leem.
    //
    // É o primeiro acoplamento entre camadas deste motor, e vale dizer por que ele não é o começo de
    // uma bagunça: o vento é um DADO, não uma chamada. Ninguém pergunta nada a ninguém, ninguém sabe
    // quem mais lê, e uma camada que ignore o vento continua correta (é o que os outros três temas
    // fazem — sem redemoinho, `vento.forca` fica 0 pra sempre e todas as contas viram `+= 0`).
    //
    // Nasce sempre, e não só quando há redemoinho: assim quem lê não precisa de dois caminhos, e um
    // tema futuro pode ganhar outra fonte de vento (uma tempestade, um bicho batendo asa) sem que
    // ninguém aqui mude de forma.
    const vento = { forca: 0, x: 0 };

    // O segundo dado compartilhado, pela mesma razão e no mesmo formato: `viva` é 0..1, escrito pela
    // fogueira (que apaga quando o redemoinho passa por cima) e lido pelas BRASAS no ar. Brasa subindo de
    // fogueira apagada seria exatamente o detalhe que denuncia que o apagar é só pintura.
    const fogo = { viva: 1 };

    // O TERCEIRO dado compartilhado, e o primeiro que é um MAPA em vez de um número: as portas das
    // cabines do ⭐ Especial, uma entrada por champ que mora atrás de uma. O banheiro ESCREVE (é ele
    // que decide quando o rugido arromba e quando a porta volta a fechar) e os sentados LEEM — cada um
    // se recorta na abertura da sua.
    //
    // Podia ser o banheiro anotando isso no próprio `vaos` da config, e seria menos código. Mas a
    // config é um `const` de módulo, partilhado entre TODAS as batalhas: o estado de uma porta ficaria
    // pendurado nela depois que a luta acabasse. Este objeto nasce e morre com o cenário, que é o
    // tempo de vida certo pra um estado de cena.
    const portas = {};

    // O QUARTO dado compartilhado, e o primeiro que não é vento nem estado de peça. Três campos, e
    // três donos diferentes — ninguém escreve no campo de ninguém:
    //   pulso      · 0..1, a respiração da lava. Escrito pela Árvore do Mundo dos 🔱 Decaídos, lido
    //                pelas rachaduras e pela fumaça.
    //   raizes     · a geometria das raízes JÁ EM PIXEL DE TELA, publicada pela árvore pras
    //                rachaduras correrem em cima delas sem ter de recalcular nada.
    //   escorrendo · 0..1, se a lava está descendo. Escrito pela FENDA, lido pela árvore.
    //   tremor     · 0..1, o chão tremendo. Escrito pela fenda, lido por quem está plantado nele.
    //   jorro      · 0..1, a lava nova sendo cuspida na árvore. Fenda escreve, árvore lê.
    //   passagem   · contador de passagens dos morcegos. A COLUNA escreve, a fenda espera mudar.
    //
    // É a prova de que o maestro não era um jeito de falar de VENTO: aqui o dado é LUZ e o resto é
    // dramaturgia, e o formato é o mesmo — nasce sempre, ninguém pergunta nada a ninguém, e uma
    // camada que ignore tudo isto continua correta. Sem árvore em cena, `pulso` fica em 1 pra sempre.
    const inferno = { pulso: 1, raizes: [], escorrendo: 1, tremor: 0, jorro: 0, passagem: 0 };

    // O QUINTO dado compartilhado, e o primeiro que é um ROTEIRO: a noite de Natal dos ✝️ Apóstolos.
    // Um escritor só (o `criarRoteiroDaNoite`, que não desenha nada) e quatro leitores espalhados pela
    // tela — a árvore num canto, a janela no meio, a lareira no outro canto e os presentes no chão:
    //   passo     · o nome do beat corrente, pra quem precisa saber "onde estamos" numa palavra
    //   brilho    · 0..1, a estrela anunciando que ele vem. ÁRVORE lê.
    //   fogo      · 0..1, a lareira acesa. LAREIRA lê (e o clarão dela no piso morre junto).
    //   voo       · 0..1, o progresso da travessia atrás da janela; 0 = ele não está em cena
    //   sentido   · 1 = esquerda→direita (a ida), −1 = a volta. JANELA lê os dois.
    //   pernas    · 0..1, o quanto dele está enfiado na lareira; `mexe` liga o esperneio
    //   entrega   · 0..1 o presente descendo da lareira, −1 quando não há nenhum viajando
    //   entregues · CONTADOR de entregas concluídas. Os PRESENTES esperam ele mudar pra empurrar a fila.
    //
    // Por que um maestro e não uma máquina de estados dentro de um builder, como o ciclo do Inferno:
    // lá as fases eram todas da FENDA, e aqui elas atravessam quatro peças que estão em cantos
    // diferentes. Não há uma delas que seja o lugar certo de guardar a história.
    const natal = {
        passo: 'repouso', brilho: 0, fogo: 1, voo: 0, sentido: 1,
        pernas: 0, mexe: 0, entrega: -1, entregues: 0,
    };

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
        // O Folclore. As moitas vêm ANTES das duas aparições, e elas se escondem por RECORTE (ver
        // `criarAparicaoNaMoita`) em vez de por ordem de pintura. A ordem contrária seria mais simples e
        // foi a primeira tentativa — mas então o brilho dos olhos do Oni ficava atrás da folhagem, e o
        // pedido era justamente que ele vazasse ENTRE as folhas. Luz passa por folha; chifre não.
        //
        // Depois vem o sítio da fogueira, e o redemoinho por último de todos: ele atravessa a clareira
        // INTEIRA, na frente do fogo e das estacas, que é o que faz o sopro parecer ter chegado onde a
        // gente está. Ele é também o único que recebe o `vento` pra ESCREVER; a fogueira, pra ler.
        config.moitas && criarMoitas(config.moitas, fundo),
        config.chifres && config.moitas && criarChifres(config.chifres, fundo, config.moitas),
        config.clava && config.moitas && criarClava(config.clava, fundo, config.moitas),
        config.fogueira && criarFogueira(config.fogueira, fundo, vento, fogo),
        config.redemoinho && criarRedemoinho(config.redemoinho, fundo, vento),
        // Os Místicos, na ordem em que a praia é vista: o dragão está no CÉU e é a coisa mais
        // distante mesmo quando passa perto — vem primeiro. Depois o mar (os saltos), depois a areia
        // (a lâmpada), e as palmeiras por último porque são a moldura: elas ficam na frente de tudo o
        // que é cenário, e ainda assim atrás dos combatentes, que é o lugar de uma borda de cena.
        //
        // O dragão fica no FUNDO mesmo na passagem de perto, pela mesma razão do Invasor: um bicho
        // desse tamanho na tela da frente taparia a luta. O que dá a leitura de "por cima" não é a
        // camada, é ele ser CORTADO pela borda de cima.
        config.dragao && criarDragao(config.dragao, fundo, vento),
        config.mar && criarMar(config.mar, fundo),
        config.golfinhos && criarGolfinhos(config.golfinhos, fundo),
        config.lampada && criarLampada(config.lampada, fundo, vento),
        config.palmeiras && criarPalmeiras(config.palmeiras, fundo, vento),
        // ⭐ O banheiro, na ordem em que a sala é vista: a parede e a mobília primeiro (é o fundo de
        // tudo), depois quem está sentado nela, e o 🦖 por último porque ele está EM PÉ no meio do
        // salão — na frente das cabines e atrás dos combatentes.
        //
        // Os sentados recebem a config do banheiro (e não uma cópia das medidas) pelo mesmo motivo do
        // ninja recebendo a do castelo: a cabine é que sabe onde ela está e quanto mede, e um homem
        // sentado dentro dela não pode ter uma segunda opinião sobre isso.
        config.banheiro && criarBanheiro(config.banheiro, fundo, vento, portas),
        config.sentados && config.banheiro && criarSentados(config.sentados, fundo, vento, config.banheiro, portas),
        config.trex && config.coco && criarTrex(config.trex, fundo, vento, config.coco),
        // 🔱 Os Decaídos, na ordem em que a vila é vista. A CASA e os FOGOS vêm primeiro por serem o
        // que está mais longe (moram na linha do ladrilho, com a vila arrasada). Depois a FENDA, que
        // é o CHÃO se abrindo — e por isso vem antes da árvore: as raízes passam POR CIMA dela, que é
        // o que faz a terra parecer ter rachado por baixo de uma coisa que já estava lá. A ÁRVORE é a
        // peça grande; as RACHADURAS vêm logo depois porque correm em cima das raízes que ela acabou
        // de desenhar; e a fumaça e os morcegos por último, porque são o que está no AR.
        //
        // As quatro recebem a config da árvore (e não uma cópia das medidas) pelo mesmo motivo do
        // ninja com o castelo e dos sentados com o banheiro: quem sabe onde a árvore está é a árvore.
        config.fogos && criarFogosDaVila(config.fogos, fundo),
        config.buraco && config.arvore && criarBuracoDoInferno(config.buraco, fundo, config.arvore, inferno),
        config.arvore && criarArvoreDoMundo(config.arvore, fundo, inferno),
        config.veias && config.arvore && criarVeias(config.veias, fundo, config.arvore, inferno),
        config.fumaca && config.arvore && criarFumacaDoInferno(config.fumaca, fundo, config.arvore, inferno),
        config.coluna && config.arvore && criarColunaDeMorcegos(config.coluna, fundo, config.arvore, inferno),
        // Os respingos vêm DEPOIS da árvore de propósito: o buraco está mais perto do que ela, e o que
        // salta dele passa na frente do tronco. Quem os move é a camada do buraco; esta só pinta.
        config.buraco && criarRespingosDoInferno(config.buraco, fundo, inferno),
        // ✝️ Os Apóstolos, na ordem em que a SALA é vista, de fora pra dentro. O ROTEIRO vem primeiro
        // e não desenha NADA — ele só escreve o maestro, e vindo antes garante que as quatro peças
        // leiam o mesmo instante da história no mesmo quadro. Depois a VISTA (o que está do outro lado
        // do vidro, recortada nele), a MOLDURA com as cortinas por cima dela, e então o que está
        // dentro da sala: a LAREIRA, a ÁRVORE e os PRESENTES no chão, que são a coisa mais perto.
        //
        // As cortinas recebem a config da janela, e os presentes as da árvore e da lareira, pelo mesmo
        // motivo do ninja com o castelo e dos sentados com o banheiro: quem sabe onde uma peça está e
        // quanto ela mede é ela mesma, e a segunda cópia de uma medida diverge no meio da cena.
        config.roteiro && criarRoteiroDaNoite(config.roteiro, fundo, natal),
        config.janela && criarVistaDaJanela(config.janela, fundo, natal),
        config.cortinas && config.janela && criarCortinas(config.cortinas, fundo, config.janela),
        config.lareira && criarLareira(config.lareira, fundo, natal),
        config.arvoreDeNatal && criarArvoreDeNatal(config.arvoreDeNatal, fundo, natal),
        config.presentes && config.arvoreDeNatal && config.lareira
            && criarPresentes(config.presentes, fundo, natal, config.arvoreDeNatal, config.lareira),
        config.nevoa && criarNevoa(config.nevoa, fundo),
    ].filter(Boolean);

    const naFrente = [
        config.po && criarPo(config.po, frente, vento, fogo),
        config.voadores && criarVoadores(config.voadores, frente, vento),
        // Os vaga-lumes são da FRENTE pelo mesmo motivo dos voadores: eles estão no ar entre o jogador
        // e o mundo, e é essa separação que dá profundidade à praia.
        config.vagalumes && criarVagalumes(config.vagalumes, frente, vento),
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




























































// ---------- 🔱 DECAÍDOS — a vila élfica vendida ----------




































// ---------- partida ----------
document.getElementById('alternarEstatisticas').classList.toggle('ativo', mostrarEstatisticas);
document.getElementById('alternarLog').classList.toggle('ativo', mostrarLog);
// O `#meio` também tem de nascer no estado certo: quem o escondia era só o clique do botão, então
// com o log começando desligado a faixa dele ficaria à mostra até alguém clicar duas vezes.
document.getElementById('meio').classList.toggle('oculto', !mostrarLog);
aplicarVelocidade();      // sincroniza o C# com o 2x inicial
mostrarCena('menu');      // o jogo sempre abre no menu — evita o flash da arena vazia
mandar('pronto');         // destrava a thread do jogo no C#









