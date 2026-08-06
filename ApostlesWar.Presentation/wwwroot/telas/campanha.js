// CAMPANHA — o mapa de capítulos, as fases, o fim de fase e a conquista de um champ.
//
// Quatro TELAS num arquivo só, porque são quatro cenas do MESMO fluxo: sair de uma quase sempre é
// entrar na seguinte, e separá-las faria quatro arquivos que só conversam entre si.

import { abrirTela } from '../nucleo/cena.js';
import { compendioChamp } from './compendio.js';
import { avatarDoJogador } from './menu.js';
import { configurarSlotDnD, criarCelulaPicker, criarSlot, sortearTime, tornarPickerArrastavel } from '../ui/time.js';
import { mandar } from '../nucleo/ponte.js';

// ---------- Campanha: mapa ----------
const ESPACO_NO = 210;   // distância horizontal entre facções
let mapaNodeX = [];      // x de cada nó (o marcador anda até um deles)
let mapaSelecionado = -1;// facção com o marcador em cima e o botão "Entrar" visível; -1 = nenhuma


export const campanhaMapa = {
    cena: 'campanhaMapa',
    montar(m) {
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
        marcador.textContent = avatarDoJogador();
        trilha.replaceChildren(marcador, ...nos);

        // posiciona SEM animar (troca de cena); zera o pan
        marcador.style.transition = 'none';
        marcador.style.left = mapaNodeX[Math.max(0, Math.min(m.posicao, mapaNodeX.length - 1))] + 'px';
        mapaOffset = 0; mapaOffsetBase = 0; trilha.style.transform = 'translateX(0px)';
        requestAnimationFrame(() => { marcador.style.transition = ''; });
},
};

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


export const campanhaFases = {
    cena: 'campanhaFases',
    montar(f) {
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
},
};

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


export const fimDeFase = {
    cena: 'fimDeFase',
    montar(f) {
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
},
};

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


export const conquista = {
    cena: 'conquista',
    montar(champ) {
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
},
};

// Fim da animação (ou pulo): a mesma cena vira a ficha do champ. O C# continua achando que estamos
// na conquista — o que ele espera é um "continuar", e é o Esc/Sair daqui que vai mandá-lo.
function abrirFichaDaConquista() {
    if (!conquistaEmCurso) return;
    abrirTela(compendioChamp, conquistaEmCurso, 'conquistaChamp');
    conquistaEmCurso = null;
}

document.getElementById('conquista').addEventListener('dblclick', abrirFichaDaConquista);
