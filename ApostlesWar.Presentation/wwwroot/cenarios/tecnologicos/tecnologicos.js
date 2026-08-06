import { criarNoHorizonte } from '../comum/ladrilho.js';
import { entre } from '../comum/basicos.js';
// A noite da invasão, montada em cima de quem luta aqui: 👽/👾 nos discos que cruzam o céu
// varrendo o chão com o feixe, 🧑‍🔬 nas bobinas do laboratório soltando faísca, 🤖 nas fagulhas
// de solda subindo. Nenhuma peça é mecanismo novo — são as MESMAS camadas dos outros dois com
// outro vocabulário, que era o teste de verdade do seam.
export const ar = {
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
};
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
export function criarTentaculos(cfg, canvas) {
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
export function desenharTentaculo(ctx, x0, y0, comprimento, talo, b, t, cfg) {
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
export function criarRuina(cfg, canvas) {
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

export const criarBobinas = (cfg, canvas) => criarNoHorizonte(cfg, canvas,
    (ctx, c, k) => c.aceso && desenharRaio(ctx, c.x, c.y, k.tamanho, k));


/// A descarga da bobina de Tesla: uma coroa de raios saindo da bola, com o traço quebrando em
/// ziguezague. É redesenhada a cada quadro com ângulos NOVOS de propósito — raio parado no ar não
/// existe, e é o tremer que faz a faísca parecer elétrica em vez de desenhada.

/// A descarga da bobina de Tesla: uma coroa de raios saindo da bola, com o traço quebrando em
/// ziguezague. É redesenhada a cada quadro com ângulos NOVOS de propósito — raio parado no ar não
/// existe, e é o tremer que faz a faísca parecer elétrica em vez de desenhada.
export function desenharRaio(ctx, x, y, tamanho, cfg) {
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
