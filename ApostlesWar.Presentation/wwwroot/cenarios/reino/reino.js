import { entre } from '../comum/basicos.js';
import { criarNoHorizonte } from '../comum/ladrilho.js';
import { criarPo } from '../comum/ar.js';
// A cidade murada sob cerco: a muralha e o castelo ao fundo, os dois exércitos trocando tiro nas
// bordas, e a poeira dourada das tochas subindo.
export const ar = {
    // A PAISAGEM é a coisa mais distante — por isso é a primeira do fundo (ver iniciarAr): os
    // exércitos atiram na frente dela, não por cima.
    castelo: {
        // Esta paleta é PAR do céu do `reino.css`: o canvas pinta em `z-index: 0`, ACIMA do fundo
        // CSS, então os dois lados se leem juntos.
        //
        // O que dá DESTAQUE ao castelo não é a pedra clara — é o telhado, a bandeira e as janelas
        // escuras CONTRA ela. Uma passada escureceu a paleta inteira no mesmo fator pra baixar a
        // claridade da cena; isso preserva as razões entre as cores e mesmo assim apaga tudo,
        // porque tira a saturação em valor absoluto e encolhe a faixa entre o mais claro e o mais
        // escuro. Baixar luz é mexer no CÉU, que é o campo; o assunto tem que manter a cor cheia.
        pedra: '#b4bbcc', sombra: '#868da6', telhado: '#bd4438', telhadoAlt: '#3d70ad',
        grama: '#5f9444', gramaSombra: '#4a7a38', janela: '#232a45', bandeira: '#e0b95e',
        // O morro é o único que fica lavado de propósito: ele é a coisa MAIS distante, e perder
        // cor com a distância é o que o põe atrás do castelo sem precisar de outro truque.
        morro: '#7099a2', nuvem: '248, 251, 255',
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
};

/// Monta a cena deste capítulo. A ORDEM É A PROFUNDIDADE — o que vem antes fica atrás.
///
/// O núcleo (`iniciarAr`) não sabe que este tema existe: ele chama `montar` e recebe as camadas
/// prontas. Era o contrário até ago/2026, quando UMA lista no núcleo servia os 8 temas e cada
/// item vinha guardado por `config.X &&` — os guardas eram o preço de a lista não ser de ninguém.
export function montar({ fundo, frente, maestro }) {
    return {
        noFundo: [
            criarCastelo(ar.castelo, fundo),
            // O ninja vem logo DEPOIS do castelo: ele anda em cima dos telhados que o castelo acabou de
            // desenhar, e é do castelo que ele tira a geometria — por isso recebe as duas configurações.
            criarNinja(ar.ninja, fundo, ar.castelo),
            criarExercitos(ar.exercitos, fundo),
        ].filter(Boolean),
        naFrente: [
            criarPo(ar.po, frente, maestro.vento, maestro.fogo),
        ].filter(Boolean),
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
export function criarExercitos(cfg, canvas) {
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
export const INCLINACAO = .22;

/// A espada erguida pelo atacante: sobe da borda de baixo, inclinada pra dentro do campo. `subida`
/// vai de 0 a 1 e é o quanto dela já entrou em cena.
export function desenharEspada(ctx, borda, chao, lado, subida, cfg) {
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
export function desenharDefesa(ctx, borda, chao, lado, subida, cfg) {
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
export function desenharLanca(ctx, x, base, h, lado, indice, cfg) {
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
export function desenharEscudo(ctx, x, base, h, cfg) {
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
export function desenharCajado(ctx, borda, chao, lado, subida, cfg) {
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
export function desenharEsfera(ctx, borda, chao, lado, subida, cfg) {
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
export function desenharBolaDeFogo(ctx, x, y, p, posicao, cfg) {
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
export function criarExplosao(x, y) {
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
export function desenharExplosao(ctx, e, cfg) {
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
export function desenharFlecha(ctx, x, y, angulo, cfg) {
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

/// A LINHA DO HORIZONTE do Reino: os topos por onde se anda, da esquerda pra direita.
///
/// Ela existe separada porque ganhou um SEGUNDO cliente. Enquanto só o castelo usava essas frações,
/// elas podiam morar soltas dentro dele; agora o ninja pula de um telhado pro outro e precisa das
/// mesmas — e repetir `.74`, `1.12` e `.62` nos dois lugares seria garantir que um dia discordassem.
///
/// O topo em que se PISA é o do fuste (entre as ameias, não em cima delas), e a largura de pouso é a
/// do fuste: as duas saem do mesmo `alt` que o `desenharTorre` usa, então não há como divergirem.
export function telhadosDoReino(cfg, l, h) {
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
export function criarNinja(base, canvas, castelo) {
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
export function desenharNinja(ctx, x, y, perna, alfa, noAr, rastro, cfg) {
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
export function criarCastelo(cfg, canvas) {
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
export function desenharNuvem(ctx, x, y, r, cfg) {
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
export function desenharTorre(ctx, x, base, h, cfg) {
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
