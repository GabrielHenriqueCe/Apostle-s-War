// O AR DO CENÁRIO: as duas telas de canvas, o maestro e o laço de animação.
//
// Não sabe que tema existe — recebe o módulo do capítulo e pede a cena. Ver `montar` em
// cenarios/<faccao>/<faccao>.js.

import { ar, montar } from '../cenarios/tecnologicos/tecnologicos.js';
import { criarPo } from '../cenarios/comum/ar.js';

// ---------- o ar do cenário (canvas) ----------
// Canvas atrás de tudo (z -1), rodando SÓ enquanto há cenário. Sem cenário o laço é cancelado e o
// canvas escondido: capítulo sem pele não pode custar quadro nenhum.
//
// Tudo aqui anda por DELTA DE TEMPO (`* dt`), nunca por quadro — assim a cena tem a mesma velocidade
// num monitor de 60Hz e num de 144Hz.
//
// ESTA FUNÇÃO NÃO CONHECE TEMA NENHUM. Ela recebe o módulo do cenário e pede a cena; quem sabe o que
// desenhar, em que ordem e com que config é o próprio capítulo. Até ago/2026 era o contrário: uma
// lista única aqui dentro citava os builders dos oito temas, cada item guardado por um
// `config.X &&` — e os guardas eram exatamente o preço de a lista não ser de ninguém.
let arFrame = null;

export function iniciarAr(cenario) {
    const telas = [document.getElementById('particulasFundo'), document.getElementById('particulas')];

    if (arFrame !== null) { cancelAnimationFrame(arFrame); arFrame = null; }
    for (const t of telas) t.hidden = !cenario;
    if (!cenario) return;

    const [fundo, frente] = telas;
    const ctxFundo = fundo.getContext('2d');
    const ctxFrente = frente.getContext('2d');

    const dimensionar = () => {
        for (const t of telas) { t.width = t.clientWidth; t.height = t.clientHeight; }
    };
    dimensionar();

    // O MAESTRO: dado que uma peça escreve e as outras leem, sem ninguém perguntar nada a ninguém.
    // Uma camada que o ignore continua correta — sem redemoinho, `vento.forca` fica 0 pra sempre e
    // todas as contas viram `+= 0`.
    //
    // Só o que é lido por peça COMPARTILHADA mora aqui: o `criarPo` do comum lê os dois. Maestro de
    // um tema só (as portas do banheiro, a luz do Inferno, a noite de Natal) nasce dentro do
    // `montar` daquele tema, que é o tempo de vida certo pra um estado de cena.
    const maestro = { vento: { forca: 0, x: 0 }, fogo: { viva: 1 } };

    const { noFundo, naFrente } = cenario.montar({ fundo, frente, maestro });

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





























































// ---------- o tema do campo de batalha ----------
// O REGISTRO dos capítulos é INJETADO pelo composition root (o jogo.js), não importado daqui: é o
// que mantém a promessa de que o núcleo não sabe que Folclore existe. Mesma ideia do Program.cs
// montando os services no back — quem conhece as peças concretas é quem liga os fios, uma vez só.
let cenarios = {};
export const registrarCenarios = (mapa) => { cenarios = mapa; };

let temaAtual = '';

export function aplicarTema(tema) {
    if (tema === temaAtual) return;   // o estado chega dezenas de vezes por turno; só reage à TROCA
    temaAtual = tema;

    if (tema) document.body.dataset.tema = tema;
    else document.body.removeAttribute('data-tema');

    iniciarAr(cenarios[tema]);
}
