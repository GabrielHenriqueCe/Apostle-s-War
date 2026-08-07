import { caixaRedonda, comListras, entre } from '../comum/basicos.js';
import { medirDoTema } from '../comum/ladrilho.js';
import { criarNevoa, criarPo } from '../comum/ar.js';
// ⭐ ESPECIAL — o banheiro público. O primeiro INTERIOR do front (ver o bloco no estilo.css).
//
// Os quatro apóstolos entram pelo SINAL, como sempre, mas aqui a regra teve de ir mais longe que nos
// Místicos: 🦸 e 🦹 são CORPO HUMANO, que é o que fica esquisito em canvas. A saída foi tirar o
// corpo da vista — eles estão sentados atrás de um jornal aberto, e o que sobra são as pernas com
// a calça caída, duas mãozinhas na borda do papel (com o dedinho pra fora) e o topo da cabeça
// com a máscara. Nenhum dos dois é desenhado.
//
// O 🦖 fica de COSTAS, e essa foi a decisão que destravou a cena inteira (é do Gabriel): de costas
// ele pode levantar a cauda, e o 💩 sai de onde tem que sair, no centro do quadro. Ele também é o
// MAESTRO daqui — o rugido varre de um lado ao outro e o vento varre a sala junto, então os dois
// jornais balançam em tempos diferentes sem ninguém ter sincronizado nada.
//
// E o 💩 é o único apóstolo do front que NASCE em cena em vez de estar lá desde o começo. Ele fica um
// tempo no chão fedendo, com as moscas, e some pelo ralo antes de o próximo cair — senão em dois
// minutos o banheiro tem uma fileira deles, e o que era acontecimento vira decoração.
export const ar = {
    banheiro: {
        divisoria: '#66837a', divisoriaLuz: '#87a599', divisoriaSombra: '#3f564d',
        porta: '#78968b', trinco: '#33463f', dentro: '#1b2521',
        louca: '#e4ede7', loucaSombra: '#aebfb6', metal: '#93a8a0',
        tubo: '#f4fff9', luz: '226, 255, 240',

        // A FILEIRA da parede, em fração da LARGURA da arena. `porta` é cabine fechada e pronto;
        // `cabine` é a de alguém — o `quem` diz qual —, e ela também nasce FECHADA: só escancara
        // quando o rugido bate nela, e torna a fechar sozinha depois.
        //
        // (Havia um mictório e uma pia nas pontas. Saíram: as cabines já enchem a parede, e o que
        // eles faziam era disputar espaço com a única faixa de tela que está livre.)
        //
        // As duas cabines com gente ficam bem nas pontas porque é lá que a tela está LIVRE: as
        // caixas dos combatentes encostam no meio (ver `#ladoEsquerdo`/`#ladoDireito` no CSS) e
        // deixam uma faixa vazia de cada lado. Peça que precisa ser lida mora nessa faixa.
        // As seis cabines ENCOSTAM umas nas outras e cobrem a parede inteira: 1/6 da largura cada,
        // com os centros em 1/12, 3/12, … Antes a largura (.148) era menor que o passo entre os
        // vãos (~.16), então sobrava uma tira de parede nua entre cada par e cada cabine desenhava
        // só a divisória dela — a fileira lia como seis caixas soltas em vez de um banheiro. Com
        // elas encostadas, a divisória do meio é UMA só, partilhada pelas duas vizinhas.
        largura: .16667, topo: .17, pe: .05, portaLargura: .88,
        vaos: [
            { x: .08333, tipo: 'cabine', quem: 'heroi' },
            { x: .25, tipo: 'porta' },
            { x: .41667, tipo: 'porta' },
            { x: .58333, tipo: 'porta' },
            { x: .75, tipo: 'porta' },
            { x: .91667, tipo: 'cabine', quem: 'vilao' },
        ],
        // A porta que o rugido abre: a partir de que sopro ela cede, quanto tempo fica escancarada
        // e quanto demora pra fechar. Abre num tranco e fecha devagar — é a diferença entre uma
        // porta que foi ARROMBADA e uma que alguém está manobrando.
        portaLimiar: .26, portaAberta: 5, portaAbrir: 14, portaFechar: .9,

        // As luminárias, em fração da largura. O fluorescente FALHA de vez em quando — e é a
        // mesma regra das corujas: o que se sorteia é a ESPERA, nunca a duração do gesto, senão
        // vira lâmpada de discoteca em vez de lâmpada velha. Cada tubo tem o seu relógio.
        luzes: [.22, .5, .78],
        luzLargura: .13, luzAltura: .02, luzY: .05,
        piscaEspera: [7, 22], piscaDura: .18,
        // O quanto as portas chacoalham quando o rugido bate nelas, em frações da cabine. É a
        // parte mais barata do rugido e a que mais convence: o susto fica na SALA, não no bicho.
        treme: .014,
    },

    // Os dois leitores. Tudo aqui é medido na LARGURA DA CABINE (que vem do `banheiro`), pra não
    // haver dois lugares decidindo o tamanho de um homem sentado dentro dela.
    sentados: {
        pele: '#d7a67e', calca: '#33406b', calcaSombra: '#212b4c', meia: '#e8e6dc',
        jornal: '#dcd8c6', jornalVerso: '#c8c3ae', tinta: '#4d4a3b',
        // As máscaras, que são a única coisa que diz QUEM está ali. O herói é o azul e o dourado
        // do 🦸; o vilão é o roxo e o verde do 🦹, com a testa em bico (bravo) contra a testa
        // redonda do outro. A diferença tem que dar pra ver a 30px de cabeça.
        heroi: { mascara: '#2f5fd0', mascaraLuz: '#5d8cf2', detalhe: '#f0c53c', olho: '#f4f8ff', bico: 0 },
        vilao: { mascara: '#3d2456', mascaraLuz: '#63407f', detalhe: '#7ee08e', olho: '#d6ffdf', bico: 1 },
        // Virar a página: relógios independentes, e o que se sorteia é a espera.
        espera: [6, 17], virar: .8,
        // O quanto o jornal TREME no rugido (em frações da cabine, por unidade de vento). Tremer e
        // não vergar: papel na mão de quem levou um susto vibra, não oscila.
        treme: .09,
    },

    // 🦖 O T-REX, de costas, no meio da sala. Ele é a peça central E o maestro.
    //
    // ELE NÃO TEM CABEÇA EM CENA, e isso é decisão, não economia (do Gabriel): ele é grande demais
    // pra sala, e o pescoço sai pelo alto do quadro. O que se vê são duas pernas enormes, o tronco
    // cortado em cima e a cauda — e um bicho que não cabe na tela lê como MAIOR do que qualquer
    // bicho que coubesse. De quebra, sumiu a peça mais cara de animar.
    //
    // O preço é que o rugido perdeu o rosto que o mostrava. Quem o mostra agora é a CAUDA, que
    // varre pouco pro lado, e a sala inteira: as portas escancaram, o pó risca e o fluorescente
    // gagueja. O rugido é a única coisa da cena que se vê pelo efeito e nunca pela causa.
    trex: {
        dorso: '#4f6d3c', dorsoLuz: '#6d8f52', barriga: '#9fb173', escuro: '#33482a',
        // A garra vai da RAIZ escura à ponta clara. Unha é matéria translúcida, e uma cor só —
        // que era o que havia — lê como plástico branco colado no pé.
        garra: '#efe9d2', garraRaiz: '#82764f', carne: '#7c2f3a',

        x: .5,
        // A altura do DORSO acima do chão, em fração da altura da arena. Passa de 1 na prática (o
        // topo do tronco fica ACIMA da borda) — é isso que corta o bicho em cima.
        altura: .8,
        // O CICLO. Cada beat é uma fase, e a ordem é a do Gabriel:
        //   lendo → (descarga, se há cocô) → inspira → RUGE (cauda baixa, varrendo pouco) →
        //   para → levanta o rabo → bomba → lendo
        // A descarga vem ANTES do rugido, e não depois da bomba, porque é assim que o chão está
        // limpo na hora em que o próximo cai — e assim o sumiço do cocô é um beat que dá pra ver,
        // em vez de acontecer enquanto ninguém está olhando pra ele.
        espera: [11, 22],
        inspirar: .5, rugir: 2.2, parar: .6, levantar: .85, bombar: .7,
        // O pico do vento, e o quanto a CAUDA varre junto (em frações do lado dela). O lado em que
        // a varredura COMEÇA é sorteado — sempre começar pela direita viraria tique em duas
        // partidas. Varrer POUCO é o pedido: cauda desse tamanho passeando muito vira limpador de
        // para-brisa.
        forca: .55, varredura: .5,
        // A cauda em repouso fica CENTRADA e deitada no chão, na nossa direção — de costas, o
        // rabo dele aponta pra cá, e é por isso que ele está sempre à vista. Daí ela abana: um
        // pouco pra um lado, um pouco pro outro, devagar. É o que mantém o bicho vivo parado.
        repouso: 0, abano: .42, abanoRitmo: .8,
    },

    // 💩 O COCÔ — a única peça do front que não existe até acontecer.
    coco: {
        corpo: '#5b3a1d', corpoLuz: '#7e5129', ponta: '#8d5f31',
        olho: '#f7f3e6', pupila: '#20160d', boca: '#20160d',
        tamanho: .14,
        // Onde ele cai, em fração da FAIXA DO PISO (0 = na quina da parede, 1 = na borda de
        // baixo). No meio do chão, que é onde o ralo de um banheiro fica — e é também o único
        // lugar em que ele não briga com os pés do bicho.
        raloY: .45,
        // O fedor: a mesma coluna de vapor da lâmpada dos Místicos, verde e mais baixa. Ela verga
        // com o vento pelo mesmo desenho (desvio crescendo com u², porque o pé está preso).
        fedor: '158, 196, 86', baforadas: 5, alcance: .13, abre: 1.5, giro: 1.2,
        // Ele abre os olhos DEPOIS de cair. É o beat que transforma "caiu uma coisa" em "chegou
        // alguém" — e é o mesmo princípio do aviso antes da aparição, só que ao contrário.
        acordar: 1,
        moscas: 4, mosca: '#141a12', moscaRaio: 1.8, moscaOrbita: 1.5, moscaRitmo: [1.6, 3.2],
        // O RALO, que é um ALÇAPÃO. Ele não dá descarga: as duas folhas dele abrem PRA BAIXO, o
        // cocô fica um tempo parado no ar sobre o buraco — o beat do Papa-Léguas antes do Coiote
        // despencar — e só então cai. Ao cair ele é RECORTADO na boca do buraco, do mesmo jeito
        // que o golfinho dos Místicos é recortado na linha d'água.
        //
        // O ralo é desenhado por ESTA peça, e não pelo banheiro, porque quem decide onde o cocô
        // cai é a geometria do bicho — dois lugares combinando o mesmo `x` é exatamente o erro
        // que o `--mata-passo` e as corujas já ensinaram a não cometer aqui.
        // O raio do ralo em fração da ALTURA da arena. Ele tem de ser mais largo que o cocô, senão
        // a queda não lê: `tamanho` .14 dá 98px de largura numa arena de 700, e o ralo a .055 dava
        // 77 — ele descia por um buraco menor que ele. A .085 a boca vale 119px e o engole com
        // folga. Os dois números andam JUNTOS: mexer no `tamanho` pede mexer aqui.
        ralo: '#5a6b64', raloLuz: '#7d8f87', raloFundo: '#0d120f', raloTamanho: .085,
        queda: 2.8,
        // Os RESPINGOS: quando a cauda passa por cima dele, voa cocô pro chão. Eles secam sozinhos
        // — sem isso, dois minutos de partida deixavam o banheiro inteiro salpicado.
        respingos: [3, 7], respingoRaio: [.005, .013], respingoForca: .16, respingoVida: [4, 8],
    },

    // A NÉVOA rente ao chão, esverdeada: é o cheiro da sala, não do cocô (o dele é a coluna). Ela
    // existe pra a cena ter ar entre a parede e a luta — sem ela o azulejo encosta nos bonecos.
    nevoa: { cor: '150, 190, 130', quantas: 5, deriva: [5, 16], raio: [150, 320], opacidade: [.03, .07] },
    // O PÓ na luz do fluorescente. Ele quase não anda — é poeira parada de banheiro fechado —, e
    // é ele que faz a varredura do rugido ficar visível na sala INTEIRA e não só nos jornais.
    po: {
        cor: '226, 255, 240', quantas: 30, subida: [3, 12], raio: [0.5, 1.5],
        opacidade: [.08, .28], sopro: .12,
    },
};

/// Monta a cena deste capítulo. A ORDEM É A PROFUNDIDADE — o que vem antes fica atrás.
///
/// O núcleo (`iniciarAr`) não sabe que este tema existe: ele chama `montar` e recebe as camadas
/// prontas. Era o contrário até ago/2026, quando UMA lista no núcleo servia os 8 temas e cada
/// item vinha guardado por `config.X &&` — os guardas eram o preço de a lista não ser de ninguém.
export function montar({ fundo, frente, maestro }) {
    // O TERCEIRO dado compartilhado, e o primeiro que é um MAPA em vez de um número: as portas das
    // cabines do ⭐ Especial, uma entrada por apóstolo que mora atrás de uma. O banheiro ESCREVE (é ele
    // que decide quando o rugido arromba e quando a porta volta a fechar) e os sentados LEEM — cada um
    // se recorta na abertura da sua.
    //
    // Podia ser o banheiro anotando isso no próprio `vaos` da config, e seria menos código. Mas a
    // config é um `const` de módulo, partilhado entre TODAS as batalhas: o estado de uma porta ficaria
    // pendurado nela depois que a luta acabasse. Este objeto nasce e morre com o cenário, que é o
    // tempo de vida certo pra um estado de cena.
    const portas = {};

    return {
        noFundo: [
            // ⭐ O banheiro, na ordem em que a sala é vista: a parede e a mobília primeiro (é o fundo de
            // tudo), depois quem está sentado nela, e o 🦖 por último porque ele está EM PÉ no meio do
            // salão — na frente das cabines e atrás dos combatentes.
            //
            // Os sentados recebem a config do banheiro (e não uma cópia das medidas) pelo mesmo motivo do
            // ninja recebendo a do castelo: a cabine é que sabe onde ela está e quanto mede, e um homem
            // sentado dentro dela não pode ter uma segunda opinião sobre isso.
            criarBanheiro(ar.banheiro, fundo, maestro.vento, portas),
            criarSentados(ar.sentados, fundo, maestro.vento, ar.banheiro, portas),
            criarTrex(ar.trex, fundo, maestro.vento, ar.coco),
            criarNevoa(ar.nevoa, fundo),
        ].filter(Boolean),
        naFrente: [
            criarPo(ar.po, frente, maestro.vento, maestro.fogo),
        ].filter(Boolean),
    };
}
/// 🚻 O BANHEIRO — a sala do ⭐ Especial: as luminárias, a fileira de cabines, o mictório e a pia.
///
/// É UM builder e não cinco pelo mesmo motivo do sítio da fogueira e do castelo: é UMA composição. As
/// peças partilham o chão (`--piso-linha`) e a largura da cabine, e separá-las faria metades lendo o
/// mesmo número de dois lugares — que é o erro que as corujas e o `--mata-passo` já ensinaram aqui.
///
/// Ele LÊ o vento: quando o rugido do 🦖 varre a sala, as portas chacoalham, as duas cabines com gente
/// ESCANCARAM e o fluorescente gagueja. Nada disso sabe que existe um dinossauro — tudo lê o mesmo
/// dado que o redemoinho do Folclore escrevia, e é exatamente isso que o maestro é.
///
/// E ele ESCREVE uma coisa: o quanto cada porta de cabine está aberta, no mapa `portas`. Quem lê são
/// os sentados, que se recortam nessa abertura. A porta é do banheiro porque é da sala, não de quem
/// está sentado atrás dela — e assim há um dono só do relógio de abrir e fechar.
export function criarBanheiro(cfg, canvas, vento, portas) {
    const piso = medirDoTema('--piso-linha', 74) / 100;

    // Uma porta por cabine com gente. `restante` é quanto tempo ela ainda fica escancarada; quando
    // zera, ela volta sozinha — o rugido só REARMA esse relógio, nunca fecha nada.
    const trancas = {};
    for (const vao of cfg.vaos) if (vao.tipo === 'cabine') trancas[vao.quem] = { abertura: 0, restante: 0 };

    // Cada tubo falha no SEU tempo, e espalhados na largada pra os três não estrearem juntos: luz que
    // pisca em coro lê como efeito, luz que pisca sozinha lê como lâmpada velha.
    const tubos = cfg.luzes.map((x, i) => ({
        x,
        relogio: entre(cfg.piscaEspera) * (i + 1) / cfg.luzes.length,
        falha: 0,
    }));

    let t = 0;

    return (ctx, dt) => {
        t += dt;

        const chao = canvas.height * piso;
        const v = vento?.forca ?? 0;
        const larg = canvas.width * cfg.largura;
        const topo = canvas.height * cfg.topo;
        const pe = canvas.height * cfg.pe;

        for (const tubo of tubos) {
            tubo.relogio -= dt;
            if (tubo.relogio <= 0) { tubo.falha = cfg.piscaDura; tubo.relogio = entre(cfg.piscaEspera); }
            // O rugido é a SEGUNDA causa da mesma falha, não um efeito novo: quem já sabia gaguejar
            // passa a gaguejar também quando a sala treme, e não precisou de campo nenhum pra isso.
            if (Math.abs(v) > .3 && tubo.falha <= 0 && Math.random() < .2) tubo.falha = cfg.piscaDura * .6;
            tubo.falha = Math.max(0, tubo.falha - dt);

            // Durante a falha ela GAGUEJA em vez de apagar: fluorescente morrendo tremula, e apagar
            // liso pareceria alguém no interruptor.
            const aceso = tubo.falha > 0 ? (Math.sin(t * 44) > 0 ? .28 : .9) : 1;
            desenharLuminaria(ctx, canvas.width * tubo.x, canvas.height * cfg.luzY,
                canvas.width * cfg.luzLargura, canvas.height * cfg.luzAltura, aceso, cfg);
        }

        // O tremor é o mesmo pra todas as portas — é a SALA que treme, não cada porta por si — e é
        // rápido: porta batendo no batente é chacoalho, não balanço.
        const tremor = Math.sin(t * 38) * Math.abs(v) * cfg.treme * larg;

        // As trancas cedendo. O rugido REARMA o relógio (por isso `Math.max`, e não uma atribuição:
        // um segundo sopro durante a mesma varredura não pode ENCURTAR o tempo que já estava correndo).
        // Abrir é um tranco e fechar é lento, e a assimetria é a leitura inteira: porta que abre
        // depressa foi arrombada, porta que abre devagar foi aberta por alguém.
        for (const nome in trancas) {
            const p = trancas[nome];
            if (Math.abs(v) > cfg.portaLimiar) p.restante = Math.max(p.restante, cfg.portaAberta);
            p.restante = Math.max(0, p.restante - dt);
            const alvo = p.restante > 0 ? 1 : 0;
            const vel = alvo > p.abertura ? cfg.portaAbrir : cfg.portaFechar;
            p.abertura += (alvo - p.abertura) * Math.min(1, dt * vel);
        }

        for (const vao of cfg.vaos) {
            const cx = canvas.width * vao.x;
            const p = trancas[vao.quem];
            const abertura = p ? p.abertura : 0;
            const vista = desenharCabine(ctx, cx, topo, chao, larg, pe, abertura, tremor, cfg);
            // As duas bordas da porta em PIXEL DE TELA — a interna e a de baixo. São os únicos números
            // que saem daqui, e é por eles que os sentados se recortam. Mandar o `x` e o `y` prontos
            // (e não a fração da abertura) é o que impede as duas peças de terem cada uma a sua conta
            // da largura e da folga da folha.
            if (p) portas[vao.quem] = { abertura, ...vista };
        }
    };
}

/// A calha do fluorescente e o clarão dela. O clarão vem PRIMEIRO, atrás do tubo, pelo mesmo motivo
/// do clarão da lâmpada dos Místicos: é ele que põe a luminária dentro da sala em vez de deixá-la
/// colada por cima.
export function desenharLuminaria(ctx, cx, cy, larg, alt, aceso, cfg) {
    const raio = Math.max(1, larg * .95);
    const g = ctx.createRadialGradient(cx, cy, 0, cx, cy, raio);
    g.addColorStop(0, `rgba(${cfg.luz}, ${.26 * aceso})`);
    g.addColorStop(1, `rgba(${cfg.luz}, 0)`);
    ctx.fillStyle = g;
    ctx.beginPath();
    ctx.ellipse(cx, cy, raio, raio * .62, 0, 0, Math.PI * 2);
    ctx.fill();

    ctx.fillStyle = cfg.metal;
    caixaRedonda(ctx, cx - larg / 2, cy - alt * 1.2, larg, alt * 1.2, alt * .35);
    ctx.fill();

    ctx.fillStyle = cfg.tubo;
    ctx.globalAlpha = .3 + .7 * aceso;
    caixaRedonda(ctx, cx - larg * .46, cy - alt * .3, larg * .92, alt * .82, alt * .41);
    ctx.fill();
    ctx.globalAlpha = 1;
}

/// Uma cabine: o vão escuro, o vaso, a porta e as duas divisórias. As divisórias NÃO chegam ao chão
/// nem ao teto, e esse é o detalhe que diz "banheiro público" antes de qualquer outro.
///
/// Devolve as bordas da PORTA (a interna e a de baixo) — é por elas que os sentados sabem até onde
/// estão à vista. A de baixo importa tanto quanto a outra: a folha não chega ao chão, então os PÉS de
/// quem está lá dentro aparecem mesmo com a cabine trancada, que é o que todo banheiro público faz.
export function desenharCabine(ctx, cx, topo, chao, larg, pe, abertura, tremor, cfg) {
    const base = chao - pe;
    const alt = base - topo;
    const meia = larg / 2;

    // o vão: o fundo da cabine é mais escuro que a parede, e é o que dá profundidade a ela
    ctx.fillStyle = cfg.dentro;
    ctx.fillRect(cx - meia, topo, larg, alt + pe);

    // O vaso vai SEMPRE, mesmo nas que nunca abrem: atrás da porta ele não aparece, e quando ela abre
    // já está lá. Pular o desenho economizaria pouco e criaria um estado a mais pra manter combinado.
    desenharVaso(ctx, cx, chao, larg, cfg);

    // A PORTA, encolhendo pra dobradiça (à esquerda). Encolher É o giro visto de frente — a folha
    // some do batente pra dentro —, e sai por um número só, sem perspectiva nenhuma pra acertar.
    const folga = alt * .06;
    const pl = larg * cfg.portaLargura;
    const x0 = cx - pl / 2 + tremor;
    const largPorta = Math.max(0, pl * (1 - abertura * .94));
    const livre = x0 + largPorta;

    ctx.fillStyle = cfg.porta;
    ctx.fillRect(x0, topo + folga, largPorta, alt - folga * 2);
    // a quina interna: é ela que dá ESPESSURA ao painel quando ele está de meio-lado
    ctx.fillStyle = cfg.divisoriaSombra;
    ctx.fillRect(livre - Math.max(1, larg * .022), topo + folga, Math.max(1, larg * .022), alt - folga * 2);

    // o trinco e a plaquinha de ocupado, que somem junto com a folha
    if (abertura < .55) {
        ctx.globalAlpha = 1 - abertura / .55;
        ctx.fillStyle = cfg.trinco;
        ctx.fillRect(x0 + largPorta * .74, topo + alt * .46, Math.max(1, larg * .1), alt * .035);
        ctx.beginPath();
        ctx.arc(x0 + largPorta * .5, topo + alt * .2, Math.max(1, larg * .07), 0, Math.PI * 2);
        ctx.fill();
        ctx.globalAlpha = 1;
    }

    // As divisórias, por cima de tudo: elas são o que está mais perto de quem olha. A da esquerda pega
    // a luz do teto e a da direita não — duas cores no mesmo painel é o que impede a fileira inteira
    // de ler como uma cerca chapada.
    for (const lado of [-1, 1]) {
        const x = cx + lado * meia;
        ctx.fillStyle = lado < 0 ? cfg.divisoriaLuz : cfg.divisoria;
        ctx.fillRect(x - larg * .022, topo, larg * .044, alt);
        // o pezinho de metal em que ela se apoia
        ctx.fillStyle = cfg.metal;
        ctx.fillRect(x - larg * .012, base, larg * .024, pe);
    }
    // o topo da divisória, mais claro: é a quina virada pra luz do teto
    ctx.fillStyle = cfg.divisoriaLuz;
    ctx.fillRect(cx - meia - larg * .022, topo, larg + larg * .044, Math.max(1, larg * .014));

    return { livre, baixo: base - folga };
}

/// O vaso, de frente: caixa acoplada na parede e a bacia. Duas formas e um assento — o suficiente
/// pra ler como privada, e detalhe a mais nesta escala viraria confusão.
export function desenharVaso(ctx, cx, chao, larg, cfg) {
    const a = larg * .95;

    ctx.fillStyle = cfg.louca;
    caixaRedonda(ctx, cx - larg * .21, chao - a * .78, larg * .42, a * .3, larg * .03);
    ctx.fill();

    ctx.fillStyle = cfg.loucaSombra;
    ctx.fillRect(cx - larg * .09, chao - a * .48, larg * .18, a * .2);

    ctx.fillStyle = cfg.louca;
    ctx.beginPath();
    ctx.ellipse(cx, chao - a * .3, larg * .2, a * .16, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.beginPath();
    ctx.moveTo(cx - larg * .17, chao - a * .3);
    ctx.quadraticCurveTo(cx - larg * .14, chao, cx, chao);
    ctx.quadraticCurveTo(cx + larg * .14, chao, cx + larg * .17, chao - a * .3);
    ctx.closePath();
    ctx.fill();

    // o assento levantado atrás, que é o que impede a bacia de ler como uma pia baixa
    ctx.fillStyle = cfg.loucaSombra;
    ctx.beginPath();
    ctx.ellipse(cx, chao - a * .32, larg * .21, a * .07, 0, Math.PI, Math.PI * 2);
    ctx.fill();
}

/// 🦸🦹 OS SENTADOS — o Herói e o Vilão lendo jornal, cada um na sua cabine.
///
/// A regra dos Místicos ("mostrar o SINAL, não a figura") aqui teve de ir mais longe do que lá, porque
/// os dois são corpo HUMANO — o que fica esquisito em canvas, e o Ninja só escapa por ser preto,
/// distante e em movimento. A saída foi esconder o corpo atrás do que a cena já tinha: o jornal
/// aberto. O que sobra é o que dá pra desenhar sem anatomia — as pernas com a calça caída no meio
/// delas, duas mãozinhas na borda do papel com o DEDINHO pra fora, e o topo da cabeça com a máscara.
/// Nenhum dos dois é desenhado inteiro em lugar nenhum, e é por isso que os dois funcionam.
///
/// Tudo é medido na LARGURA DA CABINE, que vem do `banheiro`: quem sabe o tamanho de um homem sentado
/// é a cabine em que ele está sentado, e uma segunda opinião sobre isso divergiria em silêncio.
export function criarSentados(cfg, canvas, vento, banheiroCfg, portas) {
    const piso = medirDoTema('--piso-linha', 74) / 100;

    // Só as cabines com `quem` têm gente. Uma `cabine` sem `quem` declarado abre e mostra o vaso
    // vazio — o que é uma cabine perfeitamente válida, e nasce funcionando sem nenhum `if` extra.
    const gente = banheiroCfg.vaos
        .filter(v => v.tipo === 'cabine' && cfg[v.quem])
        .map(v => ({
            x: v.x,
            quem: v.quem,
            pele: cfg[v.quem],
            relogio: entre(cfg.espera) * Math.random(),
            virando: -1,        // <0 = quieto; 0..1 = a folha atravessando
        }));

    let t = 0;

    return (ctx, dt) => {
        t += dt;

        const chao = canvas.height * piso;
        const larg = canvas.width * banheiroCfg.largura;
        const v = vento?.forca ?? 0;

        for (const q of gente) {
            // O relógio da página corre mesmo com a porta fechada: ele está lendo esse tempo todo, e
            // parar o relógio faria a primeira virada acontecer sempre logo depois de a porta abrir.
            if (q.virando < 0) {
                q.relogio -= dt;
                if (q.relogio <= 0) { q.virando = 0; q.relogio = entre(cfg.espera); }
            } else {
                q.virando += dt / cfg.virar;
                if (q.virando >= 1) q.virando = -1;
            }

            // A PORTA é quem decide o quanto dele se vê, e o recorte são DOIS retângulos: o que a
            // folha já liberou de lado, e a FRESTA DE BAIXO, que existe sempre. Porta de cabine não
            // chega ao chão — mesmo trancada, os pés de quem está lá dentro aparecem, e é o detalhe
            // que faz a cabine fechada continuar tendo alguém atrás dela.
            //
            // As duas bordas vêm prontas do banheiro. Recalculá-las aqui seria ter duas contas da
            // mesma folha, e elas divergiriam em silêncio no meio da abertura.
            const porta = portas[q.quem];
            if (!porta) continue;

            const cx = canvas.width * q.x;
            const meia = larg / 2 - larg * .022;         // por dentro das divisórias
            ctx.save();
            ctx.beginPath();
            ctx.rect(porta.livre, 0, Math.max(0, cx + meia - porta.livre), canvas.height);
            ctx.rect(cx - meia, porta.baixo, meia * 2, Math.max(0, canvas.height - porta.baixo));
            ctx.clip();
            desenharSentado(ctx, cx, chao, larg, q, v, t, cfg);
            ctx.restore();
        }
    };
}

export function desenharSentado(ctx, cx, chao, L, q, v, t, cfg) {
    const pele = q.pele;
    // O tronco respira de leve. Sem isto o boneco fica de porcelana, e o jornal denuncia primeiro.
    const respiro = Math.sin(t * 1.3 + cx) * L * .008;

    // 1. AS PERNAS. Só canela e pé: a coxa está atrás do jornal, e desenhá-la seria desenhar o corpo
    //    que a cena inteira existe pra não mostrar.
    for (const lado of [-1, 1]) {
        const x = cx + lado * L * .18;
        ctx.fillStyle = cfg.pele;
        ctx.beginPath();
        ctx.moveTo(x - L * .075, chao - L * .42);
        ctx.lineTo(x + L * .075, chao - L * .42);
        ctx.lineTo(x + L * .065, chao - L * .07);
        ctx.lineTo(x - L * .065, chao - L * .07);
        ctx.closePath();
        ctx.fill();

        // a meia e o pé, chapados no chão
        ctx.fillStyle = cfg.meia;
        caixaRedonda(ctx, x - L * .075, chao - L * .1, L * .15, L * .1, L * .03);
        ctx.fill();
        ctx.fillStyle = cfg.calcaSombra;
        ctx.beginPath();
        ctx.ellipse(x + lado * L * .02, chao - L * .03, L * .1, L * .035, 0, 0, Math.PI * 2);
        ctx.fill();
    }

    // 2. A CALÇA, caída no MEIO das pernas — não no tornozelo. É um bolo só, atravessando as duas,
    //    com três dobras: calça caída é uma massa amassada, e uma faixa lisa leria como bermuda.
    const calcaY = chao - L * .3;
    ctx.fillStyle = cfg.calca;
    caixaRedonda(ctx, cx - L * .32, calcaY, L * .64, L * .17, L * .05);
    ctx.fill();
    ctx.fillStyle = cfg.calcaSombra;
    for (let i = 0; i < 3; i++) {
        ctx.beginPath();
        ctx.ellipse(cx - L * .2 + i * L * .2, calcaY + L * .12, L * .09, L * .028, 0, 0, Math.PI * 2);
        ctx.fill();
    }

    // 3. A CABEÇA, antes do jornal: o papel vai passar por cima dela e cortá-la na altura dos olhos,
    //    que é o que faz sobrar só o topo. Desenhar na ordem contrária exigiria recortar à mão.
    //
    //    Ela DESCEU junto com o fundo do V do jornal (era 1.3): a dobra do meio virou o ponto mais
    //    baixo da borda de cima, e a cabeça tinha de acompanhar, senão sobraria rosto demais à mostra.
    //    Os olhos ficam rente à dobra, e as duas pontas levantadas do V passam a emoldurá-la.
    const cabecaY = chao - L * 1.08;
    const r = L * .185;
    ctx.fillStyle = cfg.pele;
    ctx.beginPath();
    ctx.arc(cx, cabecaY, r, 0, Math.PI * 2);
    ctx.fill();

    // A MÁSCARA. Ela é RECORTADA no círculo da cabeça, e é essa a correção: antes era um contorno
    // fechado tentando acompanhar o crânio por fora, e nas DIAGONAIS a curva passava raspando o
    // círculo — sobrava um fio de pele em cada quina, nos dois. Com o recorte, a borda externa sai de
    // graça e é exata, e o único caminho que sobra pra desenhar é o de BAIXO, que é justamente o que
    // diferencia os dois: reta no 🦸, em bico bravo no 🦹.
    ctx.save();
    ctx.beginPath();
    ctx.arc(cx, cabecaY, r, 0, Math.PI * 2);
    ctx.clip();

    ctx.fillStyle = pele.mascara;
    ctx.beginPath();
    ctx.moveTo(cx - r * 1.5, cabecaY - r * 1.5);
    ctx.lineTo(cx + r * 1.5, cabecaY - r * 1.5);
    ctx.lineTo(cx + r * 1.5, cabecaY + r * .34);
    ctx.lineTo(cx + r * .5, cabecaY + r * (pele.bico ? .06 : .3));
    ctx.lineTo(cx, cabecaY + r * (pele.bico ? .54 : .34));
    ctx.lineTo(cx - r * .5, cabecaY + r * (pele.bico ? .06 : .3));
    ctx.lineTo(cx - r * 1.5, cabecaY + r * .34);
    ctx.closePath();
    ctx.fill();

    ctx.fillStyle = pele.mascaraLuz;
    ctx.beginPath();
    ctx.ellipse(cx - r * .42, cabecaY - r * .5, r * .32, r * .16, -.4, 0, Math.PI * 2);
    ctx.fill();

    // o raio do 🦸 na testa vai DENTRO do recorte (é pintura na máscara)
    if (!pele.bico) {
        ctx.fillStyle = pele.detalhe;
        ctx.beginPath();
        ctx.moveTo(cx - r * .2, cabecaY - r * .92);
        ctx.lineTo(cx + r * .26, cabecaY - r * .68);
        ctx.lineTo(cx + r * .02, cabecaY - r * .56);
        ctx.lineTo(cx + r * .22, cabecaY - r * .3);
        ctx.lineTo(cx - r * .24, cabecaY - r * .56);
        ctx.lineTo(cx, cabecaY - r * .68);
        ctx.closePath();
        ctx.fill();
    }

    // os buracos dos olhos, rente à borda de cima do jornal
    ctx.fillStyle = pele.olho;
    for (const lado of [-1, 1]) {
        ctx.beginPath();
        ctx.ellipse(cx + lado * r * .44, cabecaY + r * .16, r * .26, r * .17, 0, 0, Math.PI * 2);
        ctx.fill();
    }
    ctx.restore();

    // Os espinhos do 🦹 vêm DEPOIS do recorte, porque eles são a única coisa da máscara que sai da
    // cabeça de propósito — é o que dá silhueta a ele contra a testa lisa do outro.
    if (pele.bico) {
        ctx.fillStyle = pele.detalhe;
        for (const lado of [-1, 1]) {
            ctx.beginPath();
            ctx.moveTo(cx + lado * r * .58, cabecaY - r * .5);
            ctx.lineTo(cx + lado * r * 1.26, cabecaY - r * .92);
            ctx.lineTo(cx + lado * r * .78, cabecaY - r * .12);
            ctx.closePath();
            ctx.fill();
        }
    }

    // 4. O JORNAL, que é um LIVRO ABERTO virado pra ELE. Nós vemos o verso, e é isso que decide a
    //    forma inteira: as duas bordas de fora fogem pra longe de nós (ele abraça o papel), então
    //    elas são CÔNCAVAS, e a dobra do meio é a parte mais perto de quem olha — por isso é o ponto
    //    mais alto da silhueta, com o topo caindo pros dois cantos.
    //
    //    Ele também encolheu: estava com a largura da cabine inteira, e jornal do tamanho do banheiro
    //    lê como parede, não como papel.
    const jornalY = chao - L * 1.22;
    const jornalA = L * .78;
    const meia = L * .36;

    ctx.save();
    ctx.translate(cx, jornalY + jornalA * .12);
    // O rugido não VERGA o jornal, TREME ele — mesma frequência alta das portas da cabine, e pela
    // mesma razão: susto sacode, vento é que empurra. E o tremor é do papel só; ele fica firme.
    ctx.translate(Math.sin(t * 42 + cx) * Math.abs(v) * cfg.treme * L, 0);
    ctx.translate(0, respiro);

    // A METADE, uma forma só espelhada pelos dois lados.
    //
    // A borda de cima é um V: a DOBRA é o ponto mais BAIXO e as duas pontas sobem. É o que um jornal
    // aberto faz na mão de quem o segura pelas laterais — o meio cede e os cantos ficam empinados —,
    // e a primeira versão tinha o V ao contrário, com o meio empinado, que é o que fazia o papel ler
    // como uma placa. A borda externa desce daí CURVANDO PRA DENTRO, porque ela foge de nós.
    const dobra = jornalA * .13;
    const metade = (lado, cor) => {
        ctx.fillStyle = cor;
        ctx.beginPath();
        ctx.moveTo(0, dobra);
        ctx.quadraticCurveTo(lado * meia * .42, jornalA * .02, lado * meia, -jornalA * .05);
        ctx.quadraticCurveTo(lado * meia * .84, jornalA * .5, lado * meia * .9, jornalA * .95);
        ctx.quadraticCurveTo(lado * meia * .52, jornalA * 1.04, 0, jornalA * .98);
        ctx.closePath();
        ctx.fill();
    };

    const tinta = (lado) => {
        ctx.fillStyle = cfg.tinta;
        for (let i = 0; i < 6; i++) {
            ctx.globalAlpha = i === 0 ? .6 : .22;
            ctx.fillRect(lado < 0 ? -meia * .8 : meia * .12, jornalA * (.24 + i * .11),
                meia * .68, jornalA * (i === 0 ? .045 : .018));
        }
        ctx.globalAlpha = 1;
    };

    // A FOLHA VIRANDO. A borda de cima do jornal é a LINHA D'ÁGUA dela: a folha sobe do lado de lá,
    // rompe a superfície, atravessa por cima e afunda no outro lado — do mesmo jeito que o golfinho
    // dos Místicos sai da água em partes em vez de aparecer inteiro em cima dela.
    //
    // É o RECORTE que faz isso, e não a ordem de pintura. Cobrir com o jornal desenhado depois já
    // escondia a parte de baixo, mas deixava a ponta escapando PELOS LADOS, além dos cantos do V —
    // e ali não há papel nenhum pra tapar. O recorte é a região acima da borda, e fora do jornal ela
    // segue reta na altura dos cantos: a folha deitada fica abaixo dessa linha e some inteira.
    if (q.virando >= 0) {
        const alt = jornalA * .68;
        const larg = meia * .72;
        const canto = -jornalA * .05;

        ctx.save();
        ctx.beginPath();
        ctx.moveTo(-meia * 2.4, canto);
        ctx.lineTo(-meia, canto);
        ctx.quadraticCurveTo(-meia * .42, jornalA * .02, 0, dobra);
        ctx.quadraticCurveTo(meia * .42, jornalA * .02, meia, canto);
        ctx.lineTo(meia * 2.4, canto);
        ctx.lineTo(meia * 2.4, -jornalA * 3);
        ctx.lineTo(-meia * 2.4, -jornalA * 3);
        ctx.closePath();
        ctx.clip();

        ctx.translate(0, dobra);            // ela gira em volta da dobra, no fundo do V
        ctx.rotate((1 - q.virando * 2) * 1.6);
        ctx.fillStyle = cfg.jornalVerso;
        ctx.beginPath();
        ctx.moveTo(-larg * .05, 0);
        ctx.quadraticCurveTo(-larg * .26, -alt * .55, -larg * .1, -alt);
        ctx.quadraticCurveTo(larg * .48, -alt * .92, larg * .84, -alt * .66);
        ctx.quadraticCurveTo(larg * .58, -alt * .2, larg * .05, 0);
        ctx.closePath();
        ctx.fill();
        ctx.restore();
    }

    metade(-1, cfg.jornal); tinta(-1);
    metade(1, cfg.jornal); tinta(1);

    // a dobra do meio, que é a quina virada pra nós
    ctx.strokeStyle = cfg.jornalVerso;
    ctx.lineWidth = Math.max(1, meia * .024);
    ctx.beginPath();
    ctx.moveTo(0, dobra);
    ctx.lineTo(0, jornalA * .98);
    ctx.stroke();

    // 5. OS DEDOS, agarrados nas BORDAS LATERAIS e desenhados aqui dentro, no espaço do próprio
    //    jornal — assim eles acompanham o papel de graça quando ele treme. Cada um cruza a borda: um
    //    tanto pra fora, um tanto pra dentro, que é como um dedo segura uma folha.
    //
    //    Eles ficam COLADOS um no outro e ACIMA do meio da folha: dedo espaçado lê como quatro coisas
    //    separadas encostadas no papel, e é a fileira junta que lê como mão. O `u` do passo é a altura
    //    de um dedo, então eles se tocam sem se cobrir.
    ctx.fillStyle = cfg.pele;
    for (const lado of [-1, 1]) {
        for (let i = 0; i < 4; i++) {
            const u = .28 + i * .045;
            ctx.beginPath();
            ctx.ellipse(lado * meia * (.965 - u * .15), jornalA * u,
                L * .038, L * .017, lado * .12, 0, Math.PI * 2);
            ctx.fill();
        }
    }

    ctx.restore();
}

/// A altura do X do 🦖, em frações da altura do dorso. Mora aqui fora porque DUAS peças precisam
/// dela: o corpo, que desenha o X, e o cocô, que tem de sair exatamente dali. Estava escrita duas
/// vezes e as duas já discordavam — a bomba nascia bem acima do buraco de onde devia sair.
export const TREX_ONDE_SAI = .58;

/// Onde fica a RAIZ da cauda do 🦖 e qual o raio dela, em frações da altura do dorso. Mora aqui fora
/// pelo mesmo motivo do `TREX_ONDE_SAI`: duas peças precisam dela. O CÍRCULO da raiz é desenhado pelo
/// corpo (antes da mancha da barriga, pra a barriga passar por cima dele) e o resto da cauda é
/// desenhado pela cauda — as duas têm de concordar no ponto exato, e duas cópias de um número
/// divergem na primeira vez que alguém mexe numa delas.
export const TREX_RAIZ = { y: -.86, raio: .25 };

/// 🦖 O T-REX — a peça central do ⭐ Especial, e o MAESTRO dele.
///
/// Ele fica DE COSTAS, e essa decisão (do Gabriel) é o que destravou a cena inteira. De frente ele
/// seria só um bicho grande; de costas ele pode levantar a cauda, e o 💩 sai de onde tem que sair, no
/// meio do quadro. O custo é que de costas não se lê boca aberta — e a solução virou o melhor gesto
/// da cena: pra rugir ele VIRA A CABEÇA e varre de um lado ao outro. O vento varre junto, então o
/// jornal de uma ponta balança antes do da outra sem que ninguém tenha sincronizado nada.
///
/// O CICLO, na ordem que o Gabriel descreveu:
///
///   lendo → (descarga, se sobrou cocô) → inspira → RUGE varrendo, cauda baixa → para → levanta o
///   rabo → BOMBA → lendo
///
/// A descarga vem ANTES do rugido e não depois da bomba: assim o chão está limpo na hora em que o
/// próximo cai, e o sumiço é um beat que dá pra ver em vez de acontecer com ninguém olhando.
///
/// O cocô é DESTE builder e não de um seu, pelo mesmo motivo do caixão ser dono dos fantasmas que
/// saem dele: é uma composição só, com um relógio só. E o RALO também mora aqui — quem decide onde o
/// cocô cai é a geometria do bicho, e ter o banheiro combinando o mesmo `x` por fora seria o erro que
/// as corujas e o `--mata-passo` já ensinaram a não cometer.
export function criarTrex(cfg, canvas, vento, cocoCfg) {
    const piso = medirDoTema('--piso-linha', 74) / 100;

    let fase = 'lendo';
    let relogio = entre(cfg.espera) * .5;
    let progresso = 0;
    let sentido = 1;                    // por que lado a varredura do rugido COMEÇA
    let t = 0;

    // As poses PERSEGUEM um alvo em vez de saltarem pra ele. É o que dá peso ao bicho: um alvo novo a
    // cada fase, e um corpo deste tamanho levando o seu tempo pra chegar lá.
    let rabo = 0;                       // 0 = deitada no chão, 1 = em pé
    let desvio = 0;                     // pra que lado a cauda está, somado ao repouso dela
    let rugido = 0;                     // 0..1, o quanto o tronco está inflado

    // O cocô só existe entre a bomba e a queda. `null` é o estado normal da sala.
    let coco = null;
    // Os respingos que a cauda arranca dele. Vivem por conta própria e secam sozinhos.
    let respingos = [];
    const moscas = Array.from({ length: cocoCfg.moscas }, () => ({
        fase: Math.random() * Math.PI * 2,
        ritmo: entre(cocoCfg.moscaRitmo),
        raio: .6 + Math.random() * .7,
        alt: .3 + Math.random() * .9,
    }));

    // As baforadas do fedor sobem em RODÍZIO, como o vapor da lâmpada: todas em u=0 fariam uma bola
    // só subindo em vez de uma coluna.
    const baforadas = Array.from({ length: cocoCfg.baforadas }, (_, i) => ({
        u: i / cocoCfg.baforadas,
        vel: .1 + Math.random() * .1,
        raio: .6 + Math.random() * .7,
        giro: Math.random() * Math.PI * 2,
    }));

    const perseguir = (atual, alvo, vel, dt) => atual + (alvo - atual) * Math.min(1, dt * vel);
    const suave = (x) => x * x * (3 - 2 * x);

    return (ctx, dt) => {
        t += dt;

        const chao = canvas.height * piso;
        const A = canvas.height * cfg.altura;
        const cx = canvas.width * cfg.x;
        // No MEIO da faixa do piso, e não colado no pé do bicho: é onde o ralo de um banheiro fica, e
        // é o único ponto do chão em que o cocô não briga com as pernas dele por espaço.
        const cocoY = chao + (canvas.height - chao) * cocoCfg.raloY;
        const cocoS = canvas.height * cocoCfg.tamanho;

        // ---------- o relógio ----------
        if (fase === 'lendo') {
            relogio -= dt;
            if (relogio <= 0) {
                fase = coco ? 'caindo' : 'inspira';
                progresso = 0;
                if (coco) coco.indo = 0;
            }
        } else {
            const duracao = { caindo: cocoCfg.queda, inspira: cfg.inspirar, rugindo: cfg.rugir,
                parando: cfg.parar, levantando: cfg.levantar, bombando: cfg.bombar }[fase];
            progresso += dt / duracao;
            if (progresso >= 1) {
                progresso = 0;
                if (fase === 'caindo') { coco = null; fase = 'inspira'; }
                else if (fase === 'inspira') {
                    fase = 'rugindo';
                    // O lado em que a varredura começa é SORTEADO. Começar sempre pela direita viraria
                    // tique na segunda partida, e o gesto é curto demais pra sustentar um tique.
                    sentido = Math.random() < .5 ? 1 : -1;
                } else if (fase === 'rugindo') fase = 'parando';
                else if (fase === 'parando') fase = 'levantando';
                else if (fase === 'levantando') fase = 'bombando';
                else {
                    fase = 'lendo';
                    relogio = entre(cfg.espera);
                    coco = { idade: 0, indo: -1 };
                }
            }
        }

        // ---------- as poses ----------
        const rugindo = fase === 'rugindo';

        // A cauda BAIXA pro rugido e SOBE pra bombar — a ordem é a do Gabriel, e ela importa: se
        // subisse já no rugido, os dois gestos virariam um só e o segundo perderia a surpresa.
        //
        // A subida tem CURVA EM S, e o alvo é que a faz. Perseguir um alvo que salta de 0 pra 1
        // arranca depressa e vai freando: é o movimento de um elástico, não o de um rabo pesado. Com
        // o próprio alvo acelerando e desacelerando, o perseguir só põe a inércia por cima. A descida
        // é mais lenta que a subida (2.2 contra 5) porque ele levanta com vontade e baixa relaxando.
        const alvoRabo = fase === 'levantando' ? suave(progresso) : (fase === 'bombando' ? 1 : 0);
        rabo = perseguir(rabo, alvoRabo, alvoRabo > rabo ? 5 : 2.2, dt);
        // Sem cabeça em cena, é o tronco que carrega o esforço: ele infla na inspiração e no rugido.
        rugido = perseguir(rugido, rugindo || fase === 'inspira' ? 1 : 0, 5, dt);

        // Em repouso a cauda ABANA sozinha; no rugido ela VARRE, seguindo o mesmo cosseno do vento —
        // rabo e sopro apontam pro mesmo lado ao mesmo tempo, e é isso que faz o gesto explicar a
        // rajada em vez de acontecer ao lado dela. Varre POUCO, que foi o pedido.
        const alvoDesvio = rugindo
            ? sentido * Math.cos(progresso * Math.PI) * cfg.varredura
            : Math.sin(t * cfg.abanoRitmo) * cfg.abano;
        const antes = desvio;
        desvio = perseguir(desvio, alvoDesvio, rugindo ? 8 : 3.5, dt);

        // A CAUDA BATENDO NO COCÔ. O gatilho é o abano CRUZAR O CENTRO, e não uma conta de distância
        // entre duas peças: o cocô cai no eixo do bicho, e é justamente aí que a ponta da cauda passa.
        // Consequência, não coincidência — a mesma ideia da espuma que a onda dispara na praia.
        if (coco && coco.indo < 0 && rabo < .3 && antes !== 0 && Math.sign(desvio) !== Math.sign(antes)) {
            const n = Math.round(entre(cocoCfg.respingos));
            for (let i = 0; i < n; i++) respingos.push({
                x: cx, y: cocoY - cocoS * (.2 + Math.random() * .5),
                // pro lado pra onde a cauda estava indo, com força sorteada
                vx: Math.sign(desvio) * (.35 + Math.random()) * canvas.width * cocoCfg.respingoForca,
                vy: -(.3 + Math.random() * .9) * canvas.height * .13,
                r: entre(cocoCfg.respingoRaio) * canvas.height,
                // cada um assenta numa altura sua: espalhados em profundidade, e não numa linha só
                pouso: cocoY + (Math.random() - .35) * cocoS * .9,
                vida: entre(cocoCfg.respingoVida),
                parado: false,
            });
        }

        // ---------- o MAESTRO ----------
        // Duas linhas, como no redemoinho do Folclore, e todo o resto da sala é consequência delas. O
        // envelope (`sin^.35`) faz a rajada nascer e morrer dentro do rugido; o `cos` é a varredura,
        // e é ele que troca o SINAL no meio — o sopro vai pra um lado, morre, e volta pro outro.
        if (rugindo) {
            vento.forca = sentido * cfg.forca * Math.cos(progresso * Math.PI)
                * Math.pow(Math.sin(progresso * Math.PI), .35);
            vento.x = cx;
        } else {
            // Morre devagar depois que ele fecha a boca: zerar de um quadro pro outro faria os jornais
            // voltarem ao prumo num salto, e isso lê como corte de vídeo.
            vento.forca += (0 - vento.forca) * Math.min(1, dt * 3.6);
        }

        // ---------- o cocô ----------
        if (coco) {
            coco.idade += dt;
            if (fase === 'caindo') coco.indo = progresso;
        }
        // A bomba: ele sai do X e CAI. A queda é acelerada (q²) porque coisa que cai acelera, e uma
        // queda linear é a diferença entre "caiu" e "desceu".
        const bombando = fase === 'bombando' ? progresso : -1;

        // ---------- o desenho ----------
        // O RALO primeiro: ele está no chão, embaixo de tudo. Depois o bicho, e o cocô por ÚLTIMO,
        // porque ele cai à frente das pernas e tapá-lo com elas seria esconder a única coisa da cena
        // que o jogador está esperando ver.
        //
        // O ALÇAPÃO abre no começo da queda e fecha no fim dela. Fora da queda ele está fechado, e é
        // o `-1` do `indo` que diz isso sem precisar de um segundo estado.
        const raio = canvas.height * cocoCfg.raloTamanho;
        const q = coco && coco.indo >= 0 ? coco.indo : -1;
        const abre = q < 0 ? 0 : Math.min(1, q / .18, (1 - q) / .1);
        desenharRalo(ctx, cx, cocoY, raio, abre, cocoCfg);

        ctx.save();
        ctx.translate(cx, chao);
        desenharTRex(ctx, A, { rabo, desvio, rugido, t }, cfg);
        ctx.restore();

        if (bombando >= 0) {
            // saindo e caindo: o tamanho cresce até destacar, e daí é queda livre. A queda é acelerada
            // (q²) porque coisa que cai acelera — linear é a diferença entre "caiu" e "desceu".
            const saindo = Math.min(1, bombando / .4);
            const q = Math.max(0, (bombando - .4) / .6);
            const nasce = chao - A * TREX_ONDE_SAI;
            const y = nasce + (cocoY - nasce) * q * q;
            ctx.save();
            ctx.translate(cx, y);
            desenharCoco(ctx, cocoS * saindo, 0, cocoCfg);
            ctx.restore();
        } else if (coco) {
            // A QUEDA, em três tempos: o alçapão abre (e ele fica lá, parado, boiando sobre o buraco),
            // ele DESPENCA, e o alçapão fecha. O tempo parado é o beat do Papa-Léguas — o que faz a
            // queda ter graça não é a queda, é a pausa que vem antes dela.
            const despenca = q < 0 ? 0 : Math.max(0, (q - .48) / .42);
            const caiu = Math.min(1, despenca);
            // Acelerando (q²), porque coisa que cai acelera — e o tremeliquezinho de antes é ele
            // percebendo o chão que sumiu debaixo dele.
            //
            // 2.0× a altura dele é o bastante pra sumir INTEIRO por baixo da boca do ralo, e não mais:
            // com 3.2× ele já tinha desaparecido aos 63% da queda e o resto virava tempo morto.
            const desce = caiu * caiu * cocoS * 2;
            const treme = q > .2 && despenca <= 0 ? Math.sin(t * 34) * cocoS * .025 : 0;

            ctx.save();
            // O RECORTE da boca do buraco vale SÓ ENQUANTO ELE DESCE: tudo acima da borda de trás do
            // ralo, mais o buraco inteiro; daí pra baixo é chão, e chão tapa. É o mesmo desenho que
            // recorta o golfinho dos Místicos na linha d'água — ele entra na água em partes, este
            // entra no ralo em partes.
            //
            // Parado, ele NÃO pode ser recortado: o cocô é bem mais largo que o ralo, então o recorte
            // comia os cantos de baixo dele o tempo todo, e era isso que fazia a peça parecer quebrada.
            //
            // O retângulo desce até a LINHA DO CENTRO do ralo, e não até o topo do anel. Parece
            // detalhe e não é: o arco de cima SOBE em curva, então parando no topo sobrava uma fresta
            // em meia-lua nas laterais — o cocô não era pintado ali e o anel de trás aparecia por
            // dentro dele. Do centro pra cima o retângulo cobre tudo sem buraco, e do centro pra baixo
            // quem fecha é o próprio arco de baixo, que é o único que tem de tapar alguma coisa.
            if (despenca > 0) {
                ctx.beginPath();
                ctx.rect(0, 0, canvas.width, Math.max(0, cocoY));
                ctx.ellipse(cx, cocoY, raio, raio * .42, 0, 0, Math.PI * 2);
                ctx.clip();
            }
            ctx.translate(cx + treme, cocoY + desce);
            desenharCoco(ctx, cocoS, Math.min(1, Math.max(0, (coco.idade - cocoCfg.acordar) * 2)), cocoCfg);
            ctx.restore();

            // O fedor e as moscas só enquanto ele ainda está inteiro em cena: caindo, as duas coisas
            // seriam cheiro e mosca pendurados no ar em volta de nada.
            if (despenca <= 0) {
                desenharFedor(ctx, cx, cocoY, cocoS, t, vento?.forca ?? 0, baforadas, canvas, cocoCfg);
                desenharMoscas(ctx, cx, cocoY, cocoS, t, 1, moscas, cocoCfg);
            }
        }

        // A metade DA FRENTE do anel, depois do cocô: é ela que tapa o que já afundou no buraco.
        desenharAro(ctx, cx, cocoY, raio, cocoCfg);

        // OS RESPINGOS, por último. Avança PRIMEIRO, desenha com o valor clampado e SÓ ENTÃO descarta:
        // na ordem contrária, o que morre neste quadro ainda é desenhado com raio já negativo — e raio
        // negativo num `arc` LANÇA, matando o laço do cenário inteiro.
        for (const r of respingos) {
            if (!r.parado) {
                r.vy += canvas.height * 1.7 * dt;
                r.x += r.vx * dt;
                r.y += r.vy * dt;
                if (r.y >= r.pouso) { r.y = r.pouso; r.parado = true; }
            }
            r.vida -= dt;
            const alfa = Math.min(1, Math.max(0, r.vida / 1.8));
            ctx.fillStyle = cocoCfg.corpo;
            ctx.globalAlpha = alfa;
            ctx.beginPath();
            // No ar é bolinha; no chão é lambuza achatada — a mesma partícula, duas leituras.
            ctx.ellipse(r.x, r.y, Math.max(.4, r.r * (r.parado ? 1.5 : 1)),
                Math.max(.4, r.r * (r.parado ? .4 : 1)), 0, 0, Math.PI * 2);
            ctx.fill();
            ctx.globalAlpha = 1;
        }
        respingos = respingos.filter(r => r.vida > 0);
    };
}

/// O bicho em si, DE COSTAS. A origem já está entre os pés dele, no chão, e `A` é a altura do dorso.
/// Tudo daqui pra baixo é fração de `A`: o bicho inteiro escala com a arena a partir de um número só.
export function desenharTRex(ctx, A, pose, cfg) {
    // Respirar, e INCHAR no rugido. Sem cabeça em cena, é o TRONCO que tem de carregar o esforço — por
    // isso ele cresce um tanto que seria exagero em qualquer outra peça.
    const incha = 1 + Math.sin(pose.t * 1.5) * .012 + pose.rugido * .05;
    // O QUADRIL tem de ser mais largo que a ponta de fora da coxa (que chega a .64A), senão a perna
    // sobra pra fora do tronco e o bicho fica com cara de ter as pernas penduradas ao lado do corpo.
    // Num bicho de duas pernas é o contrário: a bacia é a peça larga e a coxa nasce DENTRO dela.
    const quadril = A * .76 * incha;
    const ombro = A * .42;
    // Onde a cauda está: o lado em que ela DESCANSA mais o quanto ela abanou (ou varreu) — e tudo
    // isso VAI A ZERO conforme ela sobe. Ela levanta reto pra cima e desce reto pra baixo, sem
    // passear de lado no meio do caminho: era na diagonal que a fita atravessava a horizontal e a
    // barriga dela pulava pro lado errado.
    const lat = (cfg.repouso + pose.desvio) * (1 - pose.rabo);

    // ---- as PERNAS. São a metade de baixo do bicho e a única que se vê inteira, então são elas que
    //      dão a escala dele. Grossas de verdade: perna fina aqui viraria galinha grande.
    for (const lado of [-1, 1]) {
        ctx.save();
        // ESPELHAR, em vez de somar `lado` a cada número. A primeira versão fazia as duas contas à mão
        // e elas já não batiam: uma perna ia a .70A do centro e a outra a .68A. Espelho aqui é seguro
        // — não há raio nenhum nestas formas, só `moveTo`/`lineTo`/bezier.
        ctx.scale(lado, 1);
        const px = A * .36;
        // As duas pernas têm a MESMA cor agora. Uma era mais escura pra dar profundidade, mas com
        // listra em cima a perna escura engolia as listras dela — e o pedido era tigrado nas duas. O
        // que separa as pernas passa a ser o VÃO entre elas e o degradê, que clareia pro alto porque
        // a luz vem do teto.
        const perna = ctx.createLinearGradient(0, -A * .7, 0, 0);
        perna.addColorStop(0, cfg.dorsoLuz);
        perna.addColorStop(1, cfg.dorso);

        // A perna inteira num CAMINHO só — coxa, canela e pé —, porque ele serve pras duas coisas:
        // preencher e RECORTAR as listras. Eram três caminhos separados quando as listras não
        // precisavam de recorte.
        // OS TRÊS SUBCAMINHOS TÊM DE GIRAR NO MESMO SENTIDO. Num caminho só, o canvas preenche pela
        // regra `nonzero`: subcaminhos de sentidos CONTRÁRIOS se anulam onde se sobrepõem. A coxa e a
        // canela se sobrepõem de propósito (é assim que a junta emenda sem fresta), e como a coxa
        // corria ao contrário das outras duas, a emenda virou BURACO — dava pra ver o fundo do
        // banheiro através da perna. A coxa aqui está traçada ao contrário do que era, e é só isso.
        const caminhoDaPerna = () => {
            ctx.beginPath();
            // a coxa: gorda em cima, fina embaixo. É a massa que diz "bicho que anda em duas pernas".
            ctx.moveTo(px + A * .28, -A * .68);
            ctx.bezierCurveTo(px + A * .31, -A * .5, px + A * .25, -A * .36, px + A * .13, -A * .3);
            ctx.lineTo(px - A * .13, -A * .3);
            ctx.bezierCurveTo(px - A * .19, -A * .32, px - A * .31, -A * .46, px - A * .28, -A * .68);
            ctx.closePath();
            // a canela
            ctx.moveTo(px - A * .13, -A * .33);
            ctx.lineTo(px + A * .13, -A * .33);
            ctx.lineTo(px + A * .11, -A * .08);
            ctx.lineTo(px - A * .11, -A * .08);
            ctx.closePath();
            // o pé (de costas, os dedos apontam pra longe de nós — o que se vê é o calcanhar)
            ctx.moveTo(px - A * .2, 0);
            ctx.quadraticCurveTo(px - A * .21, -A * .12, px - A * .09, -A * .12);
            ctx.lineTo(px + A * .09, -A * .12);
            ctx.quadraticCurveTo(px + A * .21, -A * .12, px + A * .2, 0);
            ctx.closePath();
        };

        // AS LISTRAS DA PERNA: horizontais, e RECORTADAS na perna — elas podem passar folgadamente da
        // silhueta que o recorte apara. Desenhá-las tentando acertar a borda à mão era o que fazia
        // elas saírem do bicho.
        comListras(ctx, caminhoDaPerna, perna, () => {
            ctx.fillStyle = cfg.escuro;
            ctx.globalAlpha = .55;
            for (const yy of [-.63, -.52, -.41, -.26, -.15]) {
                const y = A * yy, w = A * .4;
                ctx.beginPath();
                ctx.moveTo(px + w, y);
                ctx.quadraticCurveTo(px + w * .1, y + A * .01, px - w, y + A * .026);
                ctx.quadraticCurveTo(px + w * .15, y + A * .042, px + w, y + A * .046);
                ctx.closePath();
                ctx.fill();
            }
            ctx.globalAlpha = 1;
        });

        // AS GARRAS. A FORMA é a de sempre — só a cor mudou: degradê da raiz escura pra ponta clara,
        // porque unha é matéria translúcida e cor chapada lê como plástico colado no pé.
        for (const g of [-1, 1]) {
            const unha = ctx.createLinearGradient(px + g * A * .12, -A * .04, px + g * A * .25, 0);
            unha.addColorStop(0, cfg.garraRaiz);
            unha.addColorStop(1, cfg.garra);
            ctx.fillStyle = unha;
            ctx.beginPath();
            ctx.moveTo(px + g * A * .17, -A * .05);
            ctx.lineTo(px + g * A * .25, 0);
            ctx.lineTo(px + g * A * .14, 0);
            ctx.closePath();
            ctx.fill();
        }
        ctx.restore();
    }

    // ---- o CORPO: largo na garupa, estreitando pro ombro, e CORTADO pelo alto do quadro (o pescoço e
    //      a cabeça saem de cena — ver o comentário do tema). A luz vem do teto, então o degradê
    //      clareia pra cima: é ele que impede as costas de lerem como um recorte de papel verde.
    const g = ctx.createLinearGradient(0, -A * 1.3, 0, -A * .5);
    g.addColorStop(0, cfg.dorsoLuz);
    g.addColorStop(1, cfg.dorso);
    const caminhoDoCorpo = () => {
        ctx.beginPath();
        // O ponto mais largo é uma CURVA, não um vértice. Antes o lado descia até (quadril, −.62A) e a
        // borda de baixo saía dali de través: os dois traços se encontravam num ângulo, e o quadril
        // ficava com uma quina. Agora a lateral vira pra dentro sozinha e emenda na barriga com a
        // mesma inclinação — bicho não tem canto.
        ctx.moveTo(-quadril * .97, -A * .78);
        ctx.bezierCurveTo(-quadril * 1.02, -A * 1, -ombro * 1.32, -A * 1.14, -ombro, -A * 1.3);
        ctx.lineTo(ombro, -A * 1.3);
        ctx.bezierCurveTo(ombro * 1.32, -A * 1.14, quadril * 1.02, -A * 1, quadril * .97, -A * .78);
        ctx.bezierCurveTo(quadril * .92, -A * .54, quadril * .52, -A * .42, 0, -A * .42);
        ctx.bezierCurveTo(-quadril * .52, -A * .42, -quadril * .92, -A * .54, -quadril * .97, -A * .78);
        ctx.closePath();
    };

    // AS LISTRAS DO TRONCO são VERTICAIS, como as de um tigre: descem do dorso pelos flancos. E cada
    // uma segue uma LONGITUDE do barril — o x dela é uma fração FIXA da meia-largura do corpo naquela
    // altura, então ela abre junto com o corpo e fecha junto com ele. É isso que faz a listra envolver
    // a forma; horizontal, ela só atravessava.
    //
    // (Aqui havia uma espinha reta e duas fileiras de escama. Três traços não fazem volume, fazem
    // rabisco: um corpo deste tamanho precisa de marca que acompanhe a FORMA.)
    const meiaLargura = (v) => ombro + (quadril - ombro) * Math.pow(Math.max(0, v), .55);
    const alturaDo = (v) => -A * (1.3 - .88 * v);

    comListras(ctx, caminhoDoCorpo, g, () => {
        ctx.fillStyle = cfg.escuro;
        ctx.globalAlpha = .5;
        // `s` é a longitude (fração da meia-largura), `de`/`ate` o trecho de altura que ela cobre e
        // `esp` a espessura. Comprimentos diferentes de propósito: listra de tigre não é pente.
        for (const [s, de, ate, esp] of [
            [-.86, .02, .74, .052], [-.6, .0, .92, .046], [-.34, .08, .6, .038],
            [.3, .0, .68, .04], [.56, .06, .96, .05], [.84, .0, .78, .044],
        ]) {
            const n = 9;
            ctx.beginPath();
            for (let i = 0; i <= n; i++) {
                const t = i / n, v = de + (ate - de) * t;
                // Afina até ZERO nas duas pontas (o seno), que é o que dá a ponta de listra de bicho
                // em vez da barra de espessura constante.
                ctx.lineTo(s * meiaLargura(v) - A * esp * Math.sin(Math.PI * t), alturaDo(v));
            }
            for (let i = n; i >= 0; i--) {
                const t = i / n, v = de + (ate - de) * t;
                ctx.lineTo(s * meiaLargura(v) + A * esp * Math.sin(Math.PI * t), alturaDo(v));
            }
            ctx.closePath();
            ctx.fill();
        }
        ctx.globalAlpha = 1;
    });

    // ---- o CÍRCULO da raiz da cauda, na cor do dorso. Ele é desenhado AQUI, e não lá na cauda, por
    //      uma razão só: a mancha da barriga, que vem logo abaixo, tem de passar POR CIMA DELE.
    //
    //      Antes a cauda inteira vinha por último e o círculo cobria a barriga do tronco — a mancha
    //      morria numa linha reta no encosto da raiz, como se a barriga acabasse ali. Com o círculo
    //      antes dela, a barriga entra por cima e a metade de baixo da raiz vira continuação do
    //      ventre, que é o que ela é. O que continua vindo DEPOIS é a meia-lua menor e a fita: a
    //      barriga sobrepõe este círculo e nada mais.
    ctx.fillStyle = cfg.dorso;
    ctx.beginPath();
    ctx.arc(0, A * TREX_RAIZ.y, A * TREX_RAIZ.raio, 0, Math.PI * 2);
    ctx.fill();

    // ---- a MANCHA DA BARRIGA, num verde mais claro, na parte de baixo do tronco. Ela CONTINUA pela
    //      parte de baixo da cauda (ver `desenharCauda`), e é aí que ela deixa de ser enfeite: quando
    //      o rabo começa a subir, a mancha sobe junto e denuncia o gesto ANTES de ele acontecer.
    //      É a mesma regra da moita que treme antes de a coisa levantar — o aviso é a parte barata do
    //      susto, e a que mais rende.
    //
    //      Ela vem antes do X de propósito: o X está a .58A, dentro da faixa dela, e pintada depois a
    //      mancha o engoliria.
    ctx.fillStyle = cfg.barriga;
    ctx.beginPath();
    ctx.moveTo(-quadril * .6, -A * .74);
    ctx.quadraticCurveTo(-quadril * .72, -A * .52, 0, -A * .46);
    ctx.quadraticCurveTo(quadril * .72, -A * .52, quadril * .6, -A * .74);
    ctx.quadraticCurveTo(0, -A * .84, -quadril * .6, -A * .74);
    ctx.closePath();
    ctx.fill();

    // ---- o X. Ele fica ABAIXO da raiz da cauda, e é por isso que levantar o rabo o descobre: com a
    //      cauda caída, ela passa por cima dele no caminho pro chão. A piada inteira num sinal só.
    const exposto = Math.min(1, Math.max(0, (pose.rabo - .35) / .4));
    if (exposto > .01) {
        ctx.globalAlpha = exposto;
        const y = -A * TREX_ONDE_SAI;
        ctx.fillStyle = cfg.escuro;
        ctx.beginPath();
        ctx.ellipse(0, y, A * .087, A * .073, 0, 0, Math.PI * 2);
        ctx.fill();
        ctx.strokeStyle = cfg.carne;
        ctx.lineWidth = Math.max(1, A * .017);
        ctx.lineCap = 'round';
        for (const d of [-1, 1]) {
            ctx.beginPath();
            ctx.moveTo(-d * A * .047, y - A * .04);
            ctx.lineTo(d * A * .047, y + A * .04);
            ctx.stroke();
        }
        ctx.globalAlpha = 1;
    }

    // ---- a CAUDA vem POR ÚLTIMO, sempre. Num bicho de costas, o rabo sai na direção de quem olha:
    //      ele está mais perto de nós que o corpo tanto caído no chão quanto levantado, e por isso
    //      não há ordem de pintura pra alternar. Era esse o erro da primeira versão — a cauda apontava
    //      pra LÁ e sumia, quando é justamente a parte dele que está sempre à vista.
    desenharCauda(ctx, A, pose.rabo, lat, cfg);
}

/// A CAUDA. Ela faz DUAS coisas, e só duas — nada de giro, nada de troca de face:
///
///   rabo 0 → .5  · ela pende pra baixo e ENCOLHE até o comprimento ZERO, sumindo dentro da raiz.
///   rabo .5 → 1  · ela CRESCE PRA CIMA a partir do zero, e aí não tem mais listra nenhuma.
///
/// A RAIZ é um círculo partido ao meio: a metade de BAIXO é da cauda menor, na cor da barriga, e a
/// metade de CIMA é da maior, na cor do dorso. A maior SOBREPÕE a menor.
///
/// Em fita (as duas margens pela normal de cada ponto, um preenchimento só), nunca uma fila de
/// elipses — foi o que fez o dragão dos Místicos ler como picado. E afinando até ZERO na ponta, como
/// o tentáculo do 👾 Invasor: piso na largura da ponta deixa um corte reto, não uma ponta.
export function desenharCauda(ctx, A, rabo, lat, cfg) {
    // A raiz fica ACIMA da mancha da barriga do tronco (que sobe até ~.79A), senão a cauda pareceria
    // brotar do meio dela em vez de continuá-la. As medidas vêm do `TREX_RAIZ` porque o CÍRCULO dela é
    // desenhado pelo corpo, não aqui — ver o comentário lá.
    const base = [0, A * TREX_RAIZ.y];
    const raio = A * TREX_RAIZ.raio;

    // As duas metades do gesto, cada uma com o seu 0→1. `sentido` é pra onde a fita aponta: +1 pra
    // baixo enquanto ela encolhe, −1 pra cima depois que ela zera.
    const subindo = rabo >= .5;
    const q = subindo ? (rabo - .5) * 2 : 1 - rabo * 2;
    const sentido = subindo ? -1 : 1;
    const comprimento = A * q * (subindo ? .55 : .96);

    const passos = 20;
    const pontos = [];
    for (let i = 0; i <= passos; i++) {
        const u = i / passos;
        pontos.push({
            // O desvio lateral cresce com u² e é medido no COMPRIMENTO dela: a raiz não sai do lugar
            // (em u=0 vale zero) e a curvatura é a mesma seja a cauda longa ou curta.
            x: lat * comprimento * 1.35 * u * u,
            y: base[1] + sentido * comprimento * u,
            w: raio * Math.pow(1 - u, .75),
        });
    }
    for (let i = 0; i <= passos; i++) {
        const a = pontos[Math.max(0, i - 1)], b = pontos[Math.min(passos, i + 1)];
        const dx = b.x - a.x, dy = b.y - a.y;
        const n = Math.hypot(dx, dy) || 1;
        let nx = -dy / n, ny = dx / n;
        // Forçada pra um lado só. Sem isto ela vira junto com a tangente — e a tangente inverte entre
        // a cauda que desce e a que sobe —, então as duas margens trocavam de lugar no meio do gesto.
        // É a mesma armadilha do dorso e da barriga do dragão dos Místicos.
        if (nx < 0) { nx = -nx; ny = -ny; }
        pontos[i].nx = nx;
        pontos[i].ny = ny;
    }

    // A NORMAL DA BASE é forçada na HORIZONTAL, e as duas seguintes vão sendo puxadas pra ela.
    //
    // O ponto zero em si nunca sai do lugar (em u=0 o desvio lateral vale zero, por construção). Quem
    // mexia era a normal DELE: ela é calculada a partir do vizinho, e o vizinho já carrega o abano —
    // então a corda da base saía inclinada e girava um tantinho pra cada lado, descobrindo o que está
    // atrás da fita (a meia-lua, a borda do círculo). Na horizontal, a base é sempre a corda que
    // passa pelo diâmetro da raiz, e não há fresta pra abrir.
    //
    // As duas seguintes entram na conta pra não sobrar um degrau na emenda: forçar só a primeira
    // deixaria um bico entre ela e a segunda, que ainda estaria inclinada.
    for (let i = 0; i < 3; i++) {
        const puxa = 1 - i / 3;
        let nx = pontos[i].nx * (1 - puxa) + puxa;
        let ny = pontos[i].ny * (1 - puxa);
        const n = Math.hypot(nx, ny) || 1;
        pontos[i].nx = nx / n;
        pontos[i].ny = ny / n;
    }

    const desde = (p, k) => [p.x + p.nx * p.w * k, p.y + p.ny * p.w * k];
    const em = (u, k) => desde(pontos[Math.min(passos, Math.max(0, Math.round(u * passos)))], k);

    const caminhoDaFita = (escala) => {
        ctx.beginPath();
        ctx.moveTo(...desde(pontos[0], escala));
        for (const p of pontos) ctx.lineTo(...desde(p, escala));
        for (let i = passos; i >= 0; i--) ctx.lineTo(...desde(pontos[i], -escala));
        ctx.closePath();
    };
    const fita = (escala, cor) => { caminhoDaFita(escala); ctx.fillStyle = cor; ctx.fill(); };

    // A MEIA-LUA de baixo da raiz, menor, na cor da barriga (0→π varre a metade de baixo, porque no
    // canvas o y cresce pra baixo). Ela fica aí o ciclo todo — é a única parte da barriga que nunca
    // sai de cena.
    //
    // O círculo INTEIRO que fica atrás dela não está mais aqui: ele é desenhado lá no corpo, ANTES da
    // mancha da barriga do tronco, pra que a mancha passe por cima dele. A meia-lua continua vindo
    // depois, e é por isso que a barriga do tronco sobrepõe o círculo grande e mais nada.
    ctx.fillStyle = cfg.barriga;
    ctx.beginPath();
    ctx.arc(0, base[1], raio * .66, 0, Math.PI);
    ctx.closePath();
    ctx.fill();

    // A MAIOR passa POR CIMA da meia-lua: descendo, ela cobre a barriga e o que se vê é só dorso, que
    // é o certo pra um bicho de costas com o rabo caído. E ela leva as LISTRAS TIGRADAS dentro,
    // RECORTADAS nela mesma (`comListras`) — cada uma entra por uma borda e afina até morrer perto do
    // meio, alternando de lado, e o que passar da silhueta o recorte apara.
    //
    // Subindo, a cauda não tem listra nenhuma: quem está virada pra nós aí é a face de baixo.
    if (subindo) {
        fita(1, cfg.dorso);
    } else {
        comListras(ctx, () => caminhoDaFita(1), cfg.dorso, () => {
            ctx.fillStyle = cfg.escuro;
            ctx.globalAlpha = .5;
            for (let k = 0; k < 6; k++) {
                const u = .06 + k * .072;
                const lado = k % 2 ? 1 : -1;
                ctx.beginPath();
                ctx.moveTo(...em(u, lado * 1.25));
                ctx.quadraticCurveTo(...em(u + .045, lado * .78), ...em(u + .085, lado * .08));
                ctx.quadraticCurveTo(...em(u + .05, lado * .5), ...em(u + .036, lado * 1.25));
                ctx.closePath();
                ctx.fill();
            }
            ctx.globalAlpha = 1;
        });
    }

    // E a MENOR passa por cima da maior. Sendo mais fina, não a tapa — sobra uma borda de dorso em
    // volta e as duas sobem juntas. Descendo ela nem existe: nasce quando a cauda chega ao zero.
    if (subindo) fita(.66, cfg.barriga);
}

/// 💩 O COCÔ. Três voltas empilhadas e uma ponta — a silhueta que todo mundo reconhece, e que ler a
/// 40px depende de as voltas DIMINUÍREM depressa. `olhos` é 0..1: ele chega dormindo e acorda.
export function desenharCoco(ctx, s, olhos, cfg) {
    if (s <= .5) return;

    const volta = (y, rx, ry, cor) => {
        ctx.fillStyle = cor;
        ctx.beginPath();
        ctx.ellipse(0, y, rx, ry, 0, 0, Math.PI * 2);
        ctx.fill();
    };

    // As voltas ficam mais JUNTAS do que estavam (os centros eram −.17/−.44/−.66): assim cada uma
    // entra bem na de baixo e a pilha lê como uma peça só, empilhada. Espaçadas, apareciam três
    // elipses separadas com o fundo entre elas nas laterais.
    volta(-s * .17, s * .5, s * .19, cfg.corpo);
    volta(-s * .38, s * .36, s * .16, cfg.corpoLuz);
    volta(-s * .55, s * .23, s * .12, cfg.ponta);

    ctx.fillStyle = cfg.ponta;
    ctx.beginPath();
    ctx.moveTo(-s * .1, -s * .62);
    ctx.quadraticCurveTo(s * .1, -s * .8, s * .04, -s * .9);
    ctx.quadraticCurveTo(-s * .04, -s * .77, -s * .16, -s * .64);
    ctx.closePath();
    ctx.fill();

    if (olhos > .01) {
        ctx.globalAlpha = olhos;
        // Os olhos e a boca acompanharam a compressão das voltas: eles moram na 2ª volta e na emenda
        // entre a 1ª e a 2ª, e ficariam pendurados fora do corpo se tivessem ficado onde estavam.
        for (const lado of [-1, 1]) {
            ctx.fillStyle = cfg.olho;
            ctx.beginPath();
            ctx.ellipse(lado * s * .14, -s * .4, s * .09, s * .11, 0, 0, Math.PI * 2);
            ctx.fill();
            ctx.fillStyle = cfg.pupila;
            ctx.beginPath();
            ctx.arc(lado * s * .14, -s * .39, s * .045, 0, Math.PI * 2);
            ctx.fill();
        }
        ctx.strokeStyle = cfg.boca;
        ctx.lineWidth = Math.max(1, s * .04);
        ctx.lineCap = 'round';
        ctx.beginPath();
        ctx.arc(0, -s * .26, s * .14, .3, Math.PI - .3);
        ctx.stroke();
        ctx.globalAlpha = 1;
    }
}

/// A coluna de fedor. É o vapor da lâmpada dos Místicos outra vez — mesmo rodízio de baforadas, mesmo
/// desvio crescendo com u² (o pé está preso no cocô e quem passeia é o alto) —, verde e mais baixa.
export function desenharFedor(ctx, cx, cy, s, t, v, baforadas, canvas, cfg) {
    const alcance = canvas.height * cfg.alcance;
    for (const b of baforadas) {
        const u = (b.u + t * b.vel) % 1;
        const abre = Math.max(.5, s * (.14 + (cfg.abre - .14) * u) * b.raio);
        const x = cx + Math.sin(u * cfg.giro * Math.PI * 2 + b.giro) * s * .4 * u + v * s * 3.4 * u * u;
        const y = cy - s * .8 - alcance * u;
        const alfa = Math.sin(Math.min(1, u * 1.2) * Math.PI) * .26;
        const g = ctx.createRadialGradient(x, y, 0, x, y, abre);
        g.addColorStop(0, `rgba(${cfg.fedor}, ${alfa})`);
        g.addColorStop(1, `rgba(${cfg.fedor}, 0)`);
        ctx.fillStyle = g;
        ctx.beginPath();
        ctx.arc(x, y, abre, 0, Math.PI * 2);
        ctx.fill();
    }
}

/// As moscas. Cada uma no seu ritmo e na sua altura — juntas leriam como um efeito só, que é a mesma
/// lição das corujas e das labaredas. Elas somem junto com o cocô, porque são consequência dele.
export function desenharMoscas(ctx, cx, cy, s, t, vivas, moscas, cfg) {
    ctx.fillStyle = cfg.mosca;
    ctx.globalAlpha = vivas;
    for (const m of moscas) {
        const a = t * m.ritmo + m.fase;
        const x = cx + Math.sin(a) * s * cfg.moscaOrbita * m.raio;
        const y = cy - s * m.alt + Math.sin(a * 1.7 + m.fase) * s * .18;
        ctx.beginPath();
        ctx.arc(x, y, Math.max(.6, cfg.moscaRaio * (.7 + .3 * Math.cos(a))), 0, Math.PI * 2);
        ctx.fill();
    }
    ctx.globalAlpha = 1;
}

/// O RALO, que é um ALÇAPÃO de duas folhas. `abre` é 0 (fechado) a 1 (escancarado).
///
/// Ele mora aqui, e não no `banheiro`, porque quem decide onde o cocô cai é a geometria do bicho — e
/// um `x` combinado entre duas peças é a divergência silenciosa que este front já pagou pra aprender.
///
/// As folhas abrem PRA BAIXO: cada uma gira em torno da própria borda de fora, e visto de cima isso é
/// a folha ENCOLHENDO em direção a essa borda. Mesmo desenho da porta da cabine, e pelo mesmo motivo:
/// encolher é o giro visto de frente, e sai de um número só sem perspectiva nenhuma pra acertar.
export function desenharRalo(ctx, cx, cy, s, abre, cfg) {
    // o buraco, sempre atrás: é ele que aparece quando as folhas saem da frente
    ctx.fillStyle = cfg.raloFundo;
    ctx.beginPath();
    ctx.ellipse(cx, cy, s, s * .42, 0, 0, Math.PI * 2);
    ctx.fill();

    // As duas folhas, com as grades. Tudo aqui é RECORTADO NO ANEL PARADO — e é esse o ponto: elas
    // descem enquanto encolhem, mas quem manda no limite é a boca do ralo, não elas. Antes o recorte
    // descia junto com a folha, e aí a animação acontecia FORA do anel, com a portinha aparecendo
    // por baixo da boca do buraco.
    const sobra = Math.max(0, 1 - abre);
    if (sobra > .01) {
        const desceu = abre * s * .5;
        ctx.save();
        ctx.beginPath();
        ctx.ellipse(cx, cy, s, s * .42, 0, 0, Math.PI * 2);
        ctx.clip();
        for (const lado of [-1, 1]) {
            ctx.fillStyle = cfg.ralo;
            // a folha encosta na borda de fora e recua até ela
            ctx.fillRect(cx + (lado > 0 ? s * (1 - sobra) : -s), cy + desceu - s * .5, s * sobra, s);
        }
        ctx.strokeStyle = cfg.raloFundo;
        ctx.lineWidth = Math.max(1, s * .09);
        for (let i = -3; i <= 3; i++) {
            ctx.beginPath();
            ctx.moveTo(cx + i * s * .26, cy + desceu - s * .4);
            ctx.lineTo(cx + i * s * .26, cy + desceu + s * .4);
            ctx.stroke();
        }
        ctx.restore();
    }

    // O ANEL, e ele vem em DUAS METADES em momentos diferentes. Esta é a de TRÁS, que fica atrás de
    // tudo o que está dentro do ralo. A da frente é desenhada depois do cocô (ver `desenharAro`).
    aroDoRalo(ctx, cx, cy, s, Math.PI, Math.PI * 2, cfg);
}

/// A metade DA FRENTE do anel do ralo, desenhada depois do cocô. É ela que passa POR CIMA de quem
/// está descendo pelo buraco — chão em primeiro plano tapa o que já afundou —, e é por isso que ela
/// não podia ficar junto com o resto do ralo: lá ela pintava antes e o cocô cobria a boca inteira.
///
/// Separar as duas metades é o mesmo princípio do recorte na linha d'água dos Místicos, com uma
/// vantagem: aqui a borda é uma peça desenhada, então basta pintá-la nos dois momentos certos.
export function desenharAro(ctx, cx, cy, s, cfg) {
    aroDoRalo(ctx, cx, cy, s, 0, Math.PI, cfg);
}

export function aroDoRalo(ctx, cx, cy, s, de, ate, cfg) {
    ctx.strokeStyle = cfg.raloLuz;
    ctx.lineWidth = Math.max(1, s * .16);
    ctx.beginPath();
    ctx.ellipse(cx, cy, s, s * .42, 0, de, ate);
    ctx.stroke();
}
