// CAMPANHA — o mapa de capítulos, as fases, o fim de fase e a conquista de um apóstolo.
//
// Quatro TELAS num arquivo só, porque são quatro cenas do MESMO fluxo: sair de uma quase sempre é
// entrar na seguinte, e separá-las faria quatro arquivos que só conversam entre si.

import { abrirTela } from '../nucleo/cena.js';
import { compendioApostolo } from './compendio.js';
import { avatarDoJogador } from './menu.js';
import { arrastando, configurarSlotDnD, criarCelulaPicker, criarSlot, sortearTime, tornarPickerArrastavel } from '../ui/time.js';
import { mandar } from '../nucleo/ponte.js';
import { montarBarraDificuldade } from '../ui/dificuldade.js';
import { contar, esperar } from '../ui/animacao.js';
import { almaIcone } from '../ui/alma.js';
import { poIcone } from '../ui/po.js';

// ---------- Campanha: mapa ----------
const ESPACO_NO = 210;   // distância horizontal entre facções
let mapaNodeX = [];      // x de cada nó (o marcador anda até um deles)
let mapaSelecionado = -1;// facção com o marcador em cima e o botão "Entrar" visível; -1 = nenhuma


export const campanhaMapa = {
    cena: 'campanhaMapa',
    montar(m) {
        mapaSelecionado = -1;
        montarBarraDificuldade('mapaDificuldade', m.dificuldades, m.dificuldade);
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
        calorDe = null;

        // O time volta MONTADO da última vez (o C# manda os índices; quem guarda identidade é o save).
        // Sem isto, repetir uma fase custava quatro cliques de remontagem antes de qualquer coisa.
        campTime = [null, null, null, null];
        (f.timeMontado || []).slice(0, 4).forEach((idx, i) => { campTime[i] = idx; });

        document.getElementById('fasesTitulo').textContent = `${f.capituloSimbolo} ${f.capituloNome}`;
        montarBarraDificuldade('fasesDificuldade', f.dificuldades, f.dificuldade);
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

    // O que se promete aqui é o SLOT, não uma peça: o principal é sorteado no drop, então dizer um
    // stat mentiria em quatro dos sete slots. Diz-se quantas caem e entre o que o principal sai.
    const it = fase.drop;
    const item = document.getElementById('faseItem');
    item.replaceChildren();
    const emo = document.createElement('span'); emo.className = 'fiEmoji'; emo.textContent = it.simbolo;
    const txt = document.createElement('span'); txt.textContent = `${it.nome} ×${it.quantidade} · `;
    const stat = document.createElement('span'); stat.className = 'fiStat'; stat.textContent = it.principais;
    item.append(emo, txt, stat);

    pintarCalor();   // as casas do inimigo acabaram de ser refeitas: a leitura no ar volta com elas
    atualizarLutarFase();
}

// A rodada desenhada como TABULEIRO: quatro casas, com a mesma peça (`criarSlot`) e na mesma ordem
// das suas. As casas são sempre QUATRO mesmo quando a onda tem menos gente — o vazio diz em quais
// casas o inimigo NÃO está, e isso é o que o jogador precisa pra decidir onde põe cada um.
function grupoRodada(titulo, apostolos) {
    const g = document.createElement('div'); g.className = 'faseGrupo';
    const t = document.createElement('div'); t.className = 'faseGrupoTitulo'; t.textContent = titulo;
    const cs = document.createElement('div'); cs.className = 'faseCasas';
    cs.replaceChildren(...[0, 1, 2, 3].map(casa => {
        // O `criarSlot` pede o ÍNDICE dentro da lista; aqui a lista já vem na ordem das casas,
        // então índice e casa são o mesmo número — e `null` é casa vazia.
        const slot = criarSlot(apostolos, casa < apostolos.length ? casa : null, false, casa);
        slot.classList.add('inimigo');   // mesma cara, sem clique nem arraste
        // "clique e escolha" é convite, e casa de inimigo não se escolhe.
        const vazio = slot.querySelector('.slotVazio');
        if (vazio) vazio.textContent = '—';
        return slot;
    }));
    g.append(t, cs);
    return g;
}

function montarPickerFase() {
    document.getElementById('fasePicker').replaceChildren(...campFases.meusApostolos.map((c, i) => {
        const cel = criarCelulaPicker(c);
        cel.addEventListener('click', () => escolherApostoloFase(i));
        tornarPickerArrastavel(cel, i);
        return cel;
    }));
    desenharSlotsFase();
}

function desenharSlotsFase() {
    document.getElementById('faseSlots').replaceChildren(...campTime.map((idx, i) => {
        const slot = criarSlot(campFases.meusApostolos, idx, campSlotSel === i, i);
        slot.addEventListener('click', () => {
            if (campTime[i] != null) campTime[i] = null;   // casa cheia = remove
            else campSlotSel = i;                           // casa vazia = seleciona
            desenharSlotsFase();
        });
        configurarSlotDnD(slot, campTime, i, desenharSlotsFase);
        // O calor é do par (apóstolo, casa): passar o mouse mostra a casa onde ele JÁ está;
        // arrastar mostra a casa onde ele CAIRIA, que é a pergunta do momento do arraste.
        slot.addEventListener('mouseenter', () => preverCalor(campTime[i], i));
        slot.addEventListener('dragenter', () => preverCalor(apostoloArrastado(), i));
        return slot;
    }));
    pintarCalor();
    atualizarLutarFase();
}

// ---------- O MAPA DE CALOR DA POSIÇÃO ----------
// A pergunta da montagem é "onde eu ponho este?", e a resposta é GEOMETRIA: o multiplicador do
// perfil de distância (GDD §2) em cada casa inimiga, pro apóstolo na casa que ele ocupa.
//
// A grade 4×4 vem PRONTA do C# (`apostolo.posicao[minhaCasa][casaDoAlvo]`, as duas contadas de 0):
// o front não tem cópia da tabela, então arrastar entre casas é só trocar de LINHA. Duas cópias de
// uma fórmula divergem como duas cópias de um número.
let calorDe = null;   // { idx, casa } — de quem é a leitura no ar

const CALOR_MAXIMO = .30;   // o maior desvio do 1,00 que a tabela produz (0,70 … 1,30)

function apostoloArrastado() {
    if (!arrastando) return null;
    return arrastando.tipo === 'picker' ? arrastando.idx : arrastando.arr[arrastando.i];
}

function preverCalor(idx, casa) {
    if (idx == null) return;   // casa vazia não tem o que prever: mantém a leitura anterior
    calorDe = { idx, casa };
    pintarCalor();
}

// Fica no ar depois que o mouse sai, DE PROPÓSITO: comparar duas casas exige olhar o tabuleiro, e
// apagar no `mouseleave` levaria os números embora justo na hora de ler.
function pintarCalor() {
    if (calorDe && campTime[calorDe.casa] !== calorDe.idx) calorDe = null;   // saiu da casa: a leitura morre com ele

    const grade = calorDe && (campFases.meusApostolos[calorDe.idx] || {}).posicao;
    const linha = grade ? grade[calorDe.casa] : null;
    document.querySelectorAll('#faseInimigos .faseCasas').forEach(fileira =>
        [...fileira.children].forEach((slot, casaAlvo) =>
            // Casa VAZIA não recebe leitura: ninguém está lá pra apanhar, e um ×1,30 numa casa sem
            // dono leria como oportunidade que não existe. Por isso as duas rodadas podem divergir.
            aplicarCalor(slot, linha && slot.classList.contains('preenchido') ? linha[casaAlvo] : null)));
}

function aplicarCalor(slot, mult) {
    slot.classList.remove('calor', 'quente', 'frio', 'neutro');
    const marca = slot.querySelector('.slotCalor');
    if (mult == null) { if (marca) marca.remove(); return; }

    // A força é o DESVIO do 1,00, não o valor: 1,00 é o repouso da escala, e é dele que a cor diverge.
    slot.classList.add('calor', mult > 1 ? 'quente' : mult < 1 ? 'frio' : 'neutro');
    slot.style.setProperty('--calor-forca', Math.min(1, Math.abs(mult - 1) / CALOR_MAXIMO).toFixed(3));

    const alvo = marca || document.createElement('span');
    alvo.className = 'slotCalor';
    alvo.textContent = '×' + mult.toFixed(2).replace('.', ',');   // ×1,20 — vírgula, que é como o jogo escreve
    if (!marca) slot.appendChild(alvo);
}

// Clique no picker = adiciona na casa selecionada (ou 1ª vazia). Não duplica.
function escolherApostoloFase(idx) {
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
    campTime = sortearTime(campFases.meusApostolos.length);
    campSlotSel = null;
    desenharSlotsFase();
});

// ---------- Campanha: fim de fase ----------
// Uma tela pros dois desfechos. Em dois momentos: sem opções (é a passagem da recompensa, um clique
// segue pras conquistas) e com opções (é a hora de decidir). Mesma tela nos dois pra o jogador não
// sentir que mudou de lugar entre ganhar o item e escolher o que fazer.
let fimComOpcoes = false;

/// A tela de fim de fase mostra os botoes (Jogar Novamente / Editar / Proxima) ou so o "clique pra
/// continuar"? Quem pergunta e o Esc, la no jogo.js, pra saber o que "voltar" significa aqui.
export const fimDeFaseTemOpcoes = () => fimComOpcoes;


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

        // A XP e a alma vêm ANTES do item: as duas caem por inimigo morto, então aparecem também na
        // derrota — e são a única recompensa que o jogador leva de uma fase perdida.
        //
        // A mesma tela é enviada DUAS vezes quando a fase solta apóstolo novo (a passagem da
        // recompensa e a tela de decisão). A assinatura evita animar o mesmo ganho de novo na
        // segunda: repetir a contagem faria parecer que ele ganhou tudo outra vez.
        const assinatura = JSON.stringify([f.xp, f.ganhos, f.alma, f.po]);
        const animar = assinatura !== ultimaAssinatura;
        ultimaAssinatura = assinatura;

        if (f.ganhos && f.ganhos.length) cont.append(blocoDeGanhos(f.ganhos, animar));
        else if (f.xp > 0) {
            const xp = document.createElement('div');
            xp.className = 'fimFaseXp';
            xp.textContent = `+${f.xp} XP para cada apóstolo em campo`;
            cont.append(xp);
        }

        if (f.alma && f.alma.length) cont.append(blocoDeMaterial('Almas', f.alma, almaIcone, animar));
        // O pó vem DEPOIS da alma e só na vitória (quem decide isso é o C#): ele cai por FASE, não
        // por inimigo, então numa derrota a lista chega vazia e o bloco não aparece.
        if (f.po && f.po.length) cont.append(blocoDeMaterial('Pó', f.po, poIcone, animar));

        const r = f.recompensa;
        if (r && r.itens && r.itens.length) {
            // Os itens ganham CARD, não uma linha de texto: é a recompensa que o jogador veio buscar,
            // e antes ela passava despercebida no meio da tela. São QUATRO agora, e cada uma pode ter
            // sorteado um principal diferente — por isso cada peça tem o card dela.
            const bloco = document.createElement('div'); bloco.className = 'recompensaBloco';
            const t = document.createElement('div'); t.className = 'recompensaTitulo';
            t.textContent = r.itens.length === 1 ? 'Novo item' : 'Novos itens';

            const lista = document.createElement('div'); lista.className = 'recompensaItens';
            lista.replaceChildren(...r.itens.map(it => {
                const card = document.createElement('div'); card.className = 'itemPremio';
                const em = document.createElement('span'); em.className = 'ipEmoji'; em.textContent = it.simbolo;
                const info = document.createElement('div'); info.className = 'ipInfo';
                const nm = document.createElement('div'); nm.className = 'ipNome'; nm.textContent = it.nome;
                const st = document.createElement('div'); st.className = 'ipStat'; st.textContent = `${it.stat} +${it.valor}`;
                info.append(nm, st);
                card.append(em, info);
                return card;
            }));

            bloco.append(t, lista); cont.append(bloco);
        }
        if (r && r.novos && r.novos.length) {
            const bloco = document.createElement('div'); bloco.className = 'recompensaBloco';
            const t = document.createElement('div'); t.className = 'recompensaTitulo'; t.textContent = 'Novos apóstolos';
            const cs = document.createElement('div'); cs.className = 'recompensaApostolos';
            cs.replaceChildren(...r.novos.map(c => {
                const m = document.createElement('div'); m.className = 'miniApostolo';
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

// ---------- o que a fase entregou ----------

let ultimaAssinatura = null;

const MS_DO_GANHO = 900;    // o tempo do enchimento inteiro de um apóstolo, atravessando ou não nível
const MS_ENTRE = 140;       // o atraso de um apóstolo pro seguinte, pra a leitura ser em cascata

function blocoDeGanhos(ganhos, animar) {
    const bloco = document.createElement('div'); bloco.className = 'recompensaBloco';
    const t = document.createElement('div'); t.className = 'recompensaTitulo'; t.textContent = 'Experiência';

    const lista = document.createElement('div'); lista.className = 'ganhoLista';
    lista.replaceChildren(...ganhos.map((g, i) => linhaDeGanho(g, animar, i * MS_ENTRE)));

    bloco.append(t, lista);
    return bloco;
}

function linhaDeGanho(g, animar, atraso) {
    const linha = document.createElement('div'); linha.className = 'ganhoLinha';

    const em = document.createElement('span'); em.className = 'glEmoji'; em.textContent = g.simbolo;
    const nome = document.createElement('span'); nome.className = 'glNome'; nome.textContent = g.nome;

    const nivel = document.createElement('span'); nivel.className = 'glNivel';
    const trilho = document.createElement('span'); trilho.className = 'bnTrilho glTrilho';
    const cheio = document.createElement('span'); cheio.className = 'bnCheio';
    trilho.append(cheio);

    const xp = document.createElement('span'); xp.className = 'glXp';

    const primeiro = g.trechos[0] || { nivel: 1, de: 0, ate: 0 };
    const ultimo = g.trechos[g.trechos.length - 1] || primeiro;

    linha.append(em, nome, nivel, trilho, xp);

    // Os stats entram como filhos da MESMA linha e só existem se algo mudou — a lista vem do C# já
    // filtrada, então uma linha sem subida de nível não ganha bloco vazio.
    if (g.stats && g.stats.length) linha.append(blocoDeStats(g.stats));

    if (!animar) {
        nivel.textContent = `nv ${ultimo.nivel}`;
        cheio.style.width = `${ultimo.ate}%`;
        xp.textContent = `+${g.xpGanha.toLocaleString('pt-BR')} XP`;
        if (g.travou) cheio.classList.add('travada');
        return linha;
    }

    nivel.textContent = `nv ${primeiro.nivel}`;
    cheio.style.width = `${primeiro.de}%`;
    xp.textContent = '+0 XP';
    animarGanho({ nivel, cheio, xp }, g, atraso);
    return linha;
}

// Encher · zerar · encher o próximo, um trecho por nível atravessado. Os trechos vêm PRONTOS do C#
// (`TrechoDeNivel`) — descobrir onde cada nível vira exige a curva de XP, que não tem cópia aqui.
async function animarGanho(el, g, atraso) {
    await esperar(atraso);

    const fatia = MS_DO_GANHO / Math.max(g.trechos.length, 1);
    contar(el.xp, 0, g.xpGanha, MS_DO_GANHO, v => `+${v.toLocaleString('pt-BR')} XP`);

    for (const t of g.trechos) {
        el.nivel.textContent = `nv ${t.nivel}`;
        // Sem transição pra POSICIONAR e com transição pra ANDAR: o reflow no meio é o que impede o
        // navegador de juntar as duas escritas e o trilho voar do 0 ao 100 sem passar pelo caminho.
        el.cheio.style.transition = 'none';
        el.cheio.style.width = `${t.de}%`;
        void el.cheio.offsetWidth;
        el.cheio.style.transition = `width ${fatia}ms linear`;
        el.cheio.style.width = `${t.ate}%`;
        await esperar(fatia);
    }

    // Travado = a barra fica cheia e ACESA. É aqui que o jogador descobre que agora precisa de
    // estrela, no instante em que isso passou a valer pra ele.
    if (g.travou) el.cheio.classList.add('travada');
}

function blocoDeStats(stats) {
    const cont = document.createElement('div'); cont.className = 'glStats';
    cont.replaceChildren(...stats.map(s => {
        const l = document.createElement('span'); l.className = 'glStat';
        const ic = document.createElement('span'); ic.className = 'gsIcone'; ic.textContent = s.icone;
        const rot = document.createElement('span'); rot.className = 'gsRotulo'; rot.textContent = s.rotulo;
        const v = document.createElement('span'); v.className = 'gsValor';
        v.textContent = `${s.de.toLocaleString('pt-BR')} → ${s.ate.toLocaleString('pt-BR')}`;
        l.append(ic, rot, v);
        return l;
    }));
    return cont;
}

// O material ganho na fase — a MESMA forma pra alma e pó, porque é a mesma leitura: a faixa, o
// nome e quanto entrou. `icone` é quem desenha a faixa (a alminha ou o pozinho).
function blocoDeMaterial(titulo, quantias, icone, animar) {
    const bloco = document.createElement('div'); bloco.className = 'recompensaBloco';
    const t = document.createElement('div'); t.className = 'recompensaTitulo'; t.textContent = titulo;

    const lista = document.createElement('div'); lista.className = 'ganhoLista';
    lista.replaceChildren(...quantias.map(q => {
        const l = document.createElement('div'); l.className = 'ganhoLinha';
        const n = document.createElement('span'); n.className = 'agNome'; n.textContent = q.nome;
        const v = document.createElement('span'); v.className = 'agValor';

        if (animar) contar(v, 0, q.quantidade, MS_DO_GANHO, x => `+${x.toLocaleString('pt-BR')}`)
            .catch(() => { });
        else v.textContent = `+${q.quantidade.toLocaleString('pt-BR')}`;

        l.append(icone(q.raridade, 22), n, v);
        return l;
    }));

    bloco.append(t, lista);
    return bloco;
}

document.getElementById('fimJogarNovamente').addEventListener('click', () => mandar('fimDeFase', DECISAO.jogarNovamente));
document.getElementById('fimEditarEquipe').addEventListener('click', () => mandar('fimDeFase', DECISAO.editarEquipe));
document.getElementById('fimProximaFase').addEventListener('click', () => mandar('fimDeFase', DECISAO.proximaFase));

// Clicar na tela só serve na passagem da recompensa. Com as opções à mostra, o clique no vazio não
// pode escolher por ninguém — cada botão diz o que faz.
document.getElementById('fimDeFase').addEventListener('click', e => {
    if (fimComOpcoes || e.target.closest('#fimFaseOpcoes')) return;
    mandar('continuar');
});

// ---------- Campanha: a conquista de um apóstolo ----------
// Ele vem do fundo, pequeno e fora de foco, e cresce até o centro; ao chegar, brilha e some o
// brilho. Terminada a animação a tela vira a FICHA dele — a mesma seção do compêndio. Dois cliques
// pulam pro fim (quem já viu não precisa ver de novo).
let conquistaEmCurso = null;   // o apóstolo que está chegando (guardado pra virar ficha no fim)


export const conquista = {
    cena: 'conquista',
    montar(apostolo) {
        conquistaEmCurso = apostolo;

        document.getElementById('conquistaEmoji').textContent = apostolo.simbolo;
        document.getElementById('conquistaNome').textContent = apostolo.nome;
        document.getElementById('conquistaFaccao').textContent = apostolo.faccao;

        // UMA classe rege a cena inteira (apóstolo, aura, anéis, raios) — assim as peças compartilham a
        // mesma linha do tempo sem cada uma precisar ser ligada à mão.
        const cena = document.getElementById('conquista');
        cena.classList.remove('chegando');
        void cena.offsetWidth;          // reinicia a animação mesmo com dois apóstolos seguidos
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

// Fim da animação (ou pulo): a mesma cena vira a ficha do apóstolo. O C# continua achando que estamos
// na conquista — o que ele espera é um "continuar", e é o Esc/Sair daqui que vai mandá-lo.
function abrirFichaDaConquista() {
    if (!conquistaEmCurso) return;
    abrirTela(compendioApostolo, conquistaEmCurso, 'conquistaApostolo');
    conquistaEmCurso = null;
}

document.getElementById('conquista').addEventListener('dblclick', abrirFichaDaConquista);
