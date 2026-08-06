// O que voa e o que paira: partículas, névoa e os bichos que atravessam a cena. Compartilhado por
// vários temas — cada um traz a própria config e a própria paleta.

import { entre } from './basicos.js';

/// Partículas pequenas subindo ou caindo, com vaivém horizontal pra não andarem em linha reta.
/// Dois campos OPCIONAIS, que só o Folclore usa hoje (ausentes, nada muda — é o que mantém a poeira do
/// Reino, a cinza do cemitério e o pó da invasão exatamente como estavam):
///   sopro   · o quanto o vento do tema empurra a partícula de lado, em fração da largura por segundo.
///             É aqui que o maestro fica VISÍVEL: são dezenas de grãos riscando pro mesmo lado ao
///             mesmo tempo, e nenhuma peça sozinha consegue anunciar um sopro tão bem quanto isso.
///   cintila · faixa de velocidade do pisca. Brasa não é poeira: ela acende, tremula e MORRE — daí o
///             alfa também cair conforme ela sobe (ela esfria subindo). Poeira que pisca pareceria
///             defeito; brasa que NÃO pisca parece confete.
export function criarPo(cfg, canvas, vento, fogo) {
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
        // Cada brasa tremula no SEU ritmo e na SUA contramão — se todas piscassem juntas, o ar
        // inteiro ligaria e desligaria, que é a mesma lição das corujas e das labaredas.
        pisca: cfg.cintila ? entre(cfg.cintila) : 0,
        fasePisca: Math.random() * Math.PI * 2,
    });

    // Na PRIMEIRA leva nasce espalhada pela tela inteira, e não na borda: senão o ar começa
    // parecendo um enxame entrando de uma vez.
    let grãos = Array.from({ length: cfg.quantas }, () => nova(Math.random() * canvas.height));

    return (ctx, dt) => {
        // Sem maestro no tema, `vento` nem existe e isto vale 0 — a conta abaixo vira `+= 0`.
        const sopro = (vento?.forca ?? 0) * (cfg.sopro ?? 0) * canvas.width;
        // `doFogo` amarra estas partículas a uma fogueira: elas somem quando ela apaga e voltam quando ela
        // pega. Ausente (os outros três temas), vale 1 e nada muda — poeira do Reino não depende de fogo.
        const aceso = cfg.doFogo ? (fogo?.viva ?? 1) : 1;

        for (let i = 0; i < grãos.length; i++) {
            const p = grãos[i];
            p.y -= p.vy * dt;
            p.x += sopro * dt;
            p.fase += dt;
            p.fasePisca += dt * p.pisca;
            // Reciclada quando sai por cima/baixo — e agora também pelos LADOS, senão um vento que
            // sopra sempre pra mesma banda esvaziaria metade da tela e empilharia a outra.
            if ((sobe ? p.y < -10 : p.y > canvas.height + 10) || p.x < -20 || p.x > canvas.width + 20) grãos[i] = nova();

            // A brasa esfria subindo: quanto mais longe do fogo, mais apagada. `sobe` decide de que
            // borda se mede a distância, pra não haver um segundo campo dizendo a mesma coisa.
            const percorrido = sobe ? 1 - p.y / canvas.height : p.y / canvas.height;
            const vida = cfg.cintila
                ? Math.max(0, 1 - percorrido) * (.55 + .45 * Math.sin(p.fasePisca))
                : 1;

            ctx.beginPath();
            ctx.arc(p.x + Math.sin(p.fase) * p.deriva, p.y, p.r, 0, Math.PI * 2);
            ctx.fillStyle = `rgba(${cfg.cor}, ${p.alfa * vida * aceso})`;
            ctx.fill();
        }
    };
}

/// Manchas grandes e translúcidas passeando de lado, agarradas ao chão. São poucas e enormes de
/// propósito: névoa é uma massa que se move, não um monte de bolinhas.
export function criarNevoa(cfg, canvas) {
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

/// Quem pode atravessar o céu. Assinatura única de propósito — cada um usa o que precisa e ignora o
/// resto —, porque o motor de voo não tem que saber o que está carregando.
export const VOADORES = {
    morcego: (ctx, x, y, s, fase, paraDireita, canvas, cfg) =>
        desenharMorcego(ctx, x, y, s, fase, paraDireita, cfg.cor),
    disco: (ctx, x, y, s, fase, paraDireita, canvas, cfg) =>
        desenharDisco(ctx, x, y, s, fase, canvas, cfg),
    fantasma: (ctx, x, y, s, fase, paraDireita, canvas, cfg) =>
        desenharFantasma(ctx, x, y, s, fase, paraDireita, cfg),
    corvo: (ctx, x, y, s, fase, paraDireita, canvas, cfg) =>
        desenharCorvo(ctx, x, y, s, fase, paraDireita, cfg),
};

/// Bichos atravessando a tela. Aparecem em levas, não em fila: cada um espera um tempo sorteado
/// antes de entrar, e sorteia de novo ao sair — é o que faz parecer que passou um bando, em vez de
/// uma esteira de morcegos saindo de um cano.
/// A `revoada` (opcional) troca "cada bicho no seu tempo" por BANDO: um rumo só, uma altura-base só, e
/// todos entrando quase juntos — renovados de uma vez quando o último sai. Sem ela, o comportamento é
/// o de sempre, que é o certo pro morcego, pro disco e pro fantasma: aqueles são aparições SOLTAS, e
/// bicho solto sorteando o próprio rumo é o que os faz parecer muitos. O corvo é o oposto — bando é o
/// vento ficando visível, e vento não sopra em seis direções ao mesmo tempo.
///
/// E é o bando a única camada da FRENTE que obedece ao maestro: quando o redemoinho passa por baixo,
/// ele se abre. É essa reação que diz que o vento é da cena inteira, e não só do chão.
export function criarVoadores(cfg, canvas, vento) {
    const revoada = cfg.revoada;

    // Do GRUPO, e não de cada um. Ficam fora do `novo` de propósito: são sorteados uma vez por leva.
    let rumo = Math.random() < .5;
    let altura = .08 + Math.random() * .34;
    let esperaDaLeva = Math.random() * 3;

    const novo = (primeiraVez) => {
        const paraDireita = revoada ? rumo : Math.random() < .5;
        // No bando, cada um entra um pouco atrás do outro e um pouco acima/abaixo — a formação é
        // frouxa. Em fila reta e alinhados, seis corvos leriam como um enfeite de barra de menu.
        const atraso = revoada ? Math.random() * revoada.aberturaX * canvas.width : 0;
        return {
            paraDireita,
            x: (paraDireita ? -60 - atraso : canvas.width + 60 + atraso),
            y: canvas.height * (revoada
                ? altura + (Math.random() - .5) * revoada.aberturaY
                : 0.08 + Math.random() * 0.42),                 // voam na parte de cima
            vx: entre(cfg.velocidade) * (paraDireita ? 1 : -1),
            tamanho: entre(cfg.tamanho),
            bobo: entre([14, 34]),                 // amplitude do sobe-e-desce
            fase: Math.random() * Math.PI * 2,
            asa: Math.random() * Math.PI * 2,
            velocidadeDaAsa: entre([9, 15]),
            // A primeira leva espera pouco, pra a cena não começar vazia por 10 segundos.
            espera: revoada ? esperaDaLeva : (primeiraVez ? Math.random() * 3 : entre(cfg.intervalo)),
            fora: false,
        };
    };

    let bando = Array.from({ length: cfg.quantos }, () => novo(true));

    return (ctx, dt) => {
        for (let i = 0; i < bando.length; i++) {
            const m = bando[i];
            if (m.fora) continue;
            if (m.espera > 0) { m.espera -= dt; continue; }

            m.x += m.vx * dt;
            m.fase += dt * 2.2;
            m.asa += dt * m.velocidadeDaAsa;

            // O ESPALHAR: perto do redemoinho, o bicho é jogado pra cima e empurrado pro lado. `perto`
            // cai com a distância, então quem está longe segue o voo — o bando se ABRE em vez de
            // desviar em bloco, que é a diferença entre bichos assustados e uma fila mudando de faixa.
            if (revoada && vento && Math.abs(vento.forca) > .04) {
                const perto = 1 - Math.min(1, Math.abs(m.x - vento.x) / (canvas.width * .2));
                if (perto > 0) {
                    m.y -= perto * Math.abs(vento.forca) * canvas.height * revoada.espalhar * dt;
                    m.x += vento.forca * canvas.width * .1 * perto * dt;
                    m.velocidadeDaAsa = Math.min(26, m.velocidadeDaAsa + perto * 18 * dt);
                }
            }

            if (m.vx > 0 ? m.x > canvas.width + 60 : m.x < -60) {
                if (!revoada) { bando[i] = novo(false); continue; }
                // No bando ninguém volta sozinho: quem sai fica FORA, e a leva só se renova quando o
                // último atravessou. Renovar um por um transformaria o bando numa esteira — que é
                // exatamente o que o comentário de cima diz que este motor evitava só por sorte.
                m.fora = true;
                if (bando.every(b => b.fora)) {
                    rumo = Math.random() < .5;
                    altura = .08 + Math.random() * .34;
                    esperaDaLeva = entre(cfg.intervalo);
                    for (let j = 0; j < bando.length; j++) bando[j] = novo(false);
                }
                continue;
            }

            // A FORMA é do tema: um motor de voo só, e o desenho é que muda. O fantasma do
            // cemitério, o disco da invasão, o corvo do Folclore e o morcego (guardado pros 🔱
            // Decaídos) atravessam a tela pela mesma conta.
            const y = m.y + Math.sin(m.fase) * m.bobo;
            VOADORES[cfg.forma ?? 'morcego'](ctx, m.x, y, m.tamanho, m.asa, m.paraDireita, canvas, cfg);
        }
    };
}

/// Um CORVO do bando do 👺 Tengu (que é, na origem, o demônio-pássaro — o karasu-tengu é literalmente
/// o tengu-corvo).
///
/// Ele é DUAS ASAS e um corpo curto, e nada mais. Aqui a escala mandou no traço outra vez: a 12px, o
/// que se lê de um pássaro é o ÂNGULO das asas, então elas são o desenho inteiro e o corpo é só o que
/// as une. Pena, olho e pata seriam três sujeiras.
///
/// A diferença dele com o morcego (guardado pros 🔱 Decaídos) não está no contorno — está no BATER:
/// a asa do morcego é membrana e vibra curto e nervoso; a do corvo é remo, sobe muito e desce devagar.
/// Por isso a curva do bater aqui é assimétrica (`Math.pow` no seno): a subida é rápida e a descida se
/// arrasta, que é o que faz um bando parecer que está indo pra algum lugar.
///
/// O BICO é a única concessão a detalhe, e ela se paga: um triângulo à frente é o que impede o vulto de
/// ler como morcego quando as asas estão embaixo.
export function desenharCorvo(ctx, x, y, s, faseDaAsa, paraDireita, cfg) {
    // 0 = asas no alto, 1 = asas embaixo. A potência deforma o tempo, não o desenho.
    const bater = Math.pow((Math.sin(faseDaAsa) + 1) / 2, .6);
    const abre = -s * .5 + bater * s * .95;      // a que altura a ponta da asa está

    ctx.save();
    ctx.translate(x, y);
    ctx.scale(paraDireita ? 1 : -1, 1);

    ctx.fillStyle = cfg.cor;

    // as asas: uma pra cada lado, com a ponta mais estreita que a raiz (remo, não pano esticado)
    for (const lado of [-1, 1]) {
        ctx.beginPath();
        ctx.moveTo(0, -s * .06);
        ctx.quadraticCurveTo(lado * s * .7, abre - s * .16, lado * s * 1.25, abre);
        ctx.quadraticCurveTo(lado * s * .66, abre + s * .2, 0, s * .12);
        ctx.closePath();
        ctx.fill();
    }

    // o corpo: curto e roliço, com a cauda em cunha atrás
    ctx.beginPath();
    ctx.ellipse(0, 0, s * .34, s * .19, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.beginPath();
    ctx.moveTo(-s * .26, -s * .1);
    ctx.lineTo(-s * .72, s * .04);
    ctx.lineTo(-s * .26, s * .14);
    ctx.closePath();
    ctx.fill();

    // a cabeça e o BICO
    ctx.beginPath();
    ctx.ellipse(s * .34, -s * .06, s * .15, s * .13, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.fillStyle = cfg.bico ?? cfg.cor;
    ctx.beginPath();
    ctx.moveTo(s * .44, -s * .12);
    ctx.lineTo(s * .74, -s * .02);
    ctx.lineTo(s * .44, s * .04);
    ctx.closePath();
    ctx.fill();

    ctx.restore();
}

/// Um FANTASMA: manto arredondado em cima, barra ondulando embaixo, dois vazios no lugar dos olhos.
///
/// Ele é translúcido e tem halo — sem isso viraria um pinguim branco. E a barra ONDULA pelo tempo,
/// não pela posição: é o que faz o manto parecer flutuar mesmo quando ele está quase parado.
export function desenharFantasma(ctx, x, y, s, fase, paraDireita, cfg) {
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
export function desenharDisco(ctx, x, y, s, fase, canvas, cfg) {
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
export function desenharMorcego(ctx, x, y, s, faseDaAsa, paraDireita, cor) {
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
