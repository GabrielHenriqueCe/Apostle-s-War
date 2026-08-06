// ARENA — a montagem dos dois times e quem controla cada lado.
//
// O picker, os slots e o arrastar vêm do `ui/time.js`: a Campanha monta time do mesmo jeito, e é
// ter UM lugar que faz o clique-na-casa e o arrastar nascerem iguais nos dois.

import { configurarSlotDnD, criarCelulaPicker, criarSlot, tornarPickerArrastavel } from '../ui/time.js';
import { mandar } from '../nucleo/ponte.js';

// ---------- montagem da Arena ----------
let arenaCampeoes = [];                                            // pool [{simbolo, nome}] (índice = id)
let arenaTimes = { esq: [null, null, null, null], dir: [null, null, null, null] };  // índices ou null
let arenaControle = { esq: 'jogador', dir: 'bot' };               // padrão: esquerda joga, direita bot
let arenaSlotSel = null;                                           // { lado, i } ou null

export const montagemArena = {
    cena: 'arenaSetup',
    // Desestrutura porque `montar` recebe o `conteudo` da mensagem INTEIRO — o despacho antigo
    // passava `msg.conteudo.campeoes` à mão, e essa foi a única tela em que ele fazia isso.
    montar({ campeoes }) {
        arenaCampeoes = campeoes;
        arenaTimes = { esq: [null, null, null, null], dir: [null, null, null, null] };
        arenaControle = { esq: 'jogador', dir: 'bot' };
        arenaSlotSel = null;
        aplicarToggleArena('esq', 'jogador');
        aplicarToggleArena('dir', 'bot');
        montarPickerArena();
        desenharSlotsArena();
},
};

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
