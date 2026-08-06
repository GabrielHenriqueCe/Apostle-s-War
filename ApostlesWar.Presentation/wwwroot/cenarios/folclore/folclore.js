import { entre } from '../comum/basicos.js';
import { criarNoHorizonte } from '../comum/ladrilho.js';
import { criarPo, criarVoadores } from '../comum/ar.js';
// 🪬 A clareira na mata fechada, e a fogueira no meio dela.
//
// Este tema não é sobre um lugar — folclore não é paisagem, é o que se CONTA. Por isso os quatro
// que lutam aqui estão TODOS em cena sem que nenhum seja desenhado por inteiro: os 👹 chifres do
// Oni subindo de trás de uma moita, a 🧌 clava do Troll passeando atrás de outra, o 🌪️ redemoinho
// e os 🐦‍⬛ corvos do 👺 Tengu, e as 🃏 cartas do 🤡 Palhaço que o redemoinho levanta do chão. O
// cenário mostra o SINAL de cada um, que é a lição que o Ninja e o Caixão já cobraram: figura
// pequena com anatomia lê como sujeira, e chifre/clava/bico leem a 20px.
//
// (Um desfile de vultos dos quatro já viveu dentro da fumaça da fogueira, e saiu justamente
// porque eles passaram a estar em todo o resto da cena — a história está em `criarFogueira`.)
//
// E é o primeiro tema com um MAESTRO. Nos outros três cada peça tem o seu relógio, e a
// dessincronia é o que dá vida (segue valendo aqui, pras aparições). O que passou a existir é uma
// CAUSA COMUM: o `vento`, escrito pelo redemoinho e lido por quem obedece — o fogo verga (e chega
// a APAGAR), a fumaça inclina, as brasas riscam pro lado, os corvos se abrem. Uma coisa acontece e
// a cena inteira responde; é o que separa este tema de "a quarta pele" e o torna uma ideia
// diferente sobre cenário.
export const ar = {
    // As BRASAS da fogueira. Sobem como a poeira do Reino, mas com os dois campos que só brasa
    // usa: `cintila` (acende, tremula, esfria e morre) e `sopro` (o quanto o vento as risca).
    po: {
        cor: '255, 176, 84', quantas: 44, subida: [16, 44], raio: [0.6, 2.0],
        opacidade: [.2, .7], cintila: [5, 13], sopro: .17,
        // Estas brasas são DA FOGUEIRA: quando ela apaga, elas apagam com ela.
        doFogo: true,
    },
    // 🐦‍⬛ A revoada. O Tengu é o demônio-pássaro (o karasu-tengu é literalmente o tengu-corvo),
    // então o corvo é a espécie certa; o que ele NÃO pode ser é solitário — corvo sozinho vira
    // mascote do cenário, e o que se quer aqui é o vento ficando visível.
    voadores: {
        forma: 'corvo', cor: '#140b08', bico: '#4a2a12',
        quantos: 7, velocidade: [46, 82], tamanho: [13, 21], intervalo: [8, 19],
        // `aberturaX`/`aberturaY` em fração da tela: o quanto a formação se estica e se espalha.
        // `espalhar` é a força do susto quando o redemoinho passa por baixo.
        revoada: { espalhar: .55, aberturaX: .22, aberturaY: .11 },
    },
    // 🌪️ O REDEMOINHO do Tengu — o maestro. Ele é o único que ESCREVE no vento; todo o resto lê.
    redemoinho: {
        poeira: '214, 170, 118', folha: '#7c4420',
        // Em fração da altura da arena. BEM GRANDE (era .5 × .085) porque o Gabriel quer que ele seja
        // "o efeito impactante da cena" — e ele é o maestro, então tamanho aqui é hierarquia: a coisa
        // que manda em todas as outras não pode ser a menor da tela. Continua mais alto que largo, que
        // é o que o mantém sendo redemoinho em vez de nuvem de poeira.
        altura: .95, largura: .17,
        // Espera longa e travessia rápida, como toda aparição daqui. Ele é o EVENTO do tema — se
        // estivesse sempre em cena, o vento deixaria de ser notícia e viraria clima.
        espera: [12, 24], atravessar: [5.5, 9.5],
        // `forca` é o pico do que ele escreve no vento (no meio da travessia). 1 = o valor cheio.
        // `perfil` afina esse pico: com 2.2, o sopro sobe e DESCE mais rápido, e passa a maior parte
        // da travessia perto de zero. Foi o Gabriel apontando que "a fumaça demora demais pra virar
        // pro outro lado" — o atraso não estava na fumaça (ela lê o vento no mesmo quadro), estava na
        // rajada ficar cheia por segundos demais.
        forca: 1, perfil: 2.2,
        // O BAMBOLEIO do eixo: o redemoinho não sobe reto, ele DANÇA (pedido do Gabriel). `gingado` é
        // o quanto o eixo passeia pros lados em fração da largura, e `ritmo`/`ritmo2` são as duas
        // frequências que fazem a dança não repetir — uma só daria um bambolear de metrônomo.
        gingado: 1.5, ritmo: .9, ritmo2: 2.3,
        // O que roda dentro dele.
        graos: 52, folhas: 10, giro: 3.4,
        // AS CARTAS, que agora são dele. Elas vivem em três estados — no CHÃO, no VÓRTICE e CAINDO —
        // e é o ciclo entre eles que dá o efeito que o Gabriel descreveu: ao passar, ele absorve as
        // que estão no chão e cospe outras pra fora, que pousam e ficam lá. O chão nunca fica igual
        // duas vezes, e nada disso precisa de estado compartilhado: um dono só, três estados.
        //
        // `tamanho` é fração da ALTURA DA ARENA e não da largura do redemoinho — é o pedido de "as
        // cartas ao redor não devem aumentar o tamanho". Com ele crescendo pra .17 da tela, uma carta
        // proporcional a ele viraria um outdoor girando.
        //
        // `alcance` é a que distância (em raios da base) ele pega uma carta do chão; `soltar` é a
        // chance por segundo de uma cartas do vórtice ser cuspida; `orbita` é até quantos raios do
        // cone elas podem girar — passar de 1 é o que faz algumas rodarem POR FORA da poeira, e é daí
        // que vem a leitura de vórtice em vez de coluna.
        cartas: {
            quantas: 22, noChaoAoIniciar: 14, tamanho: .042,
            alcance: 2.2, soltar: .35, orbita: 1.9,
        },
    },
    // 🔥 O SÍTIO DA FOGUEIRA: o fogo, as pedras, as achas, a coluna de fumaça e as estacas com
    // máscaras em volta. É UMA composição, e por isso um builder só — as sombras das estacas vêm do
    // pulso desta chama e as máscaras são iluminadas por ela; separar em peças faria duas metades
    // lendo o mesmo número de dois lugares.
    //
    // Ela APAGA quando o redemoinho passa por cima e volta a pegar depois, com faísca e fumaça preta.
    // É a única interação de verdade entre duas peças do tema, e não custou nada além de comparar a
    // posição dele (que já está no `vento`) com a dela.
    fogueira: {
        acha: '#241206', pedra: '#3d2c23', pedraLuz: '#6b503f',
        fogo: '255, 148, 44', brasa: '255, 234, 182',
        // Em fração da altura da arena.
        largura: .16, altura: .12, labaredas: 6,
        // O raio do clarão, em múltiplos da largura do fogo. Mesma alavanca do caixão: a coluna
        // do log (~280px) passa na frente do centro nos quatro temas, e o que resolve não é fugir
        // dela — é a luz ter raio maior que a peça e vazar pelos dois lados.
        clarao: 3.4,
        // O ESTALO: a fogueira racha e cospe faísca. É a voz PRÓPRIA do centro — sem ele, o fogo
        // só se mexeria quando o vento mandasse, e o maestro comeria a cena inteira.
        estalo: [3.5, 10], faiscas: 16, estalar: .5,
        // O CICLO DA CHAMA: o redemoinho passa por cima e APAGA a fogueira; depois vem uma faísca e
        // ela pega de novo (ideia do Gabriel). É a única coisa da cena em que duas peças se afetam de
        // verdade, e não custou encanamento novo — a fogueira compara `vento.x` com a própria posição.
        //
        // `alcanceDoVento` é a distância (em larguras de fogo) dentro da qual o redemoinho conta como
        // "por cima". `apagar` e `reacender` são as durações das transições; `escuro` é o tempo morta
        // (sorteado, pra a volta não ser cronometrável); `faisca` é a pausa entre a faísca e o fogo
        // pegar; `brilhoTotem` é quanto dura o pisca das máscaras no instante em que ele pega.
        alcanceDoVento: 1.6,
        apagar: .55, escuro: [3.5, 7], faisca: .9, faiscasDoReacender: 9,
        reacender: 3.2, brilhoTotem: 1.4,
        // A COLUNA DE FUMAÇA. `alcance` é até onde sobe (fração da altura da arena), `abre` o quanto
        // se alarga no topo (múltiplos da largura do fogo) e `sopros` quantas baforadas sobem em
        // rodízio dentro dela.
        //
        // `sopros` foi de 6 pra 11 e a `opacidade` caiu de .27 pra .2. As duas mudanças são a mesma
        // decisão: a fumaça ficou FLUIDA. Ela era densa e com poucas baforadas porque precisava
        // sustentar um vulto legível dentro dela; sem o vulto, muitas baforadas fracas e fora de
        // compasso leem como massa rolando, e é isso que fumaça é.
        //
        // `fuligem` é a cor da fumaça de fogo MORRENDO. A cor real é interpolada entre `cor` e ela
        // conforme a chama cai, então o preto não é um segundo estado: é o mesmo número.
        coluna: { cor: '212, 178, 152', fuligem: '54, 46, 42', alcance: .8, abre: 2.8, sopros: 11, opacidade: .2 },
        // 🎭 As ESTACAS com máscaras, em volta do fogo. É o que faz a fogueira ser um SÍTIO em vez
        // de uma fogueira no vazio — e são elas que projetam as sombras que dançam no chão,
        // provando que o fogo ilumina algo. `x` em múltiplos da largura do fogo, `alt` em fração
        // da altura dele, `giro` em radianos (nenhuma reta: estaca fincada à mão fica torta).
        // TRÊS e não quatro, e cada uma MAIOR: quatro máscaras pequenas leram como bolhas em
        // palitos (o Gabriel: "as máscaras ficaram ruins"). O que conserta máscara é tamanho e
        // contorno, não quantidade — então uma saiu e as três que ficaram cresceram, ganharam
        // borda escura e uma fatia de luz do lado do fogo. `escala` é a máscara em fração da
        // altura da estaca.
        // `aceso` é a cor do halo que elas dão no instante em que a fogueira pega.
        // A máscara é MADEIRA, não osso: os três tons vão de nó escuro (`mascara`) a madeira acesa
        // pelo fogo (`mascaraLuz`), com o ocre no meio. `luz` é a DIREÇÃO do gradiente e muda em cada
        // uma — centro, cima, baixo. `tribo`/`faixas` também são por máscara: com gradiente e pintura
        // variando, as três param de ler como três cópias do mesmo objeto.
        estacas: {
            poste: '#1e1008', mascara: '#5e3d23', mascaraOcre: '#96663a', mascaraLuz: '#c99a63',
            traco: '#1b0c04', borda: '#0e0603', sombra: '18, 8, 4', aceso: '255, 206, 122',
            escala: .62,
            pontos: [
                { x: -2.7, alt: 1.9, giro: -.11, cara: 'longa', luz: 'centro', tribo: '#8f2f22', faixas: 'meia' },
                { x: -1.6, alt: 1.45, giro: .09, cara: 'redonda', luz: 'baixo', tribo: '#1f120a', faixas: 'raios' },
                { x: 2.5, alt: 1.75, giro: .13, cara: 'longa', luz: 'cima', tribo: '#c98322', faixas: 'barra' },
            ],
        },
        // As CARTAS no chão saíram daqui e passaram a ser do REDEMOINHO. Elas estavam paradas e eram
        // enfeite; agora ele as levanta ao passar e cospe outras de volta, então quem tem de ser o
        // dono delas é quem as movimenta. Dois donos pro mesmo objeto seria o começo de duas verdades
        // sobre onde cada carta está.
    },
    // 🌿 As MOITAS da clareira: os arbustos do primeiro plano. São a única coisa da cena com
    // ENDEREÇO, e existem pra isso: o 👹 levanta os chifres de trás de UMA delas e o 🧌 levanta a
    // clava de trás de outra. É o que torna as duas aparições um acontecimento num LUGAR, em vez
    // de uma figura surgindo no ar em coordenada arbitrária.
    //
    // `largura` e `espaco` em fração da altura da arena (`espaco` = o vão entre uma e a seguinte).
    moitas: {
        folha: '#100a06', contorno: '#7b4f2a', galho: '#422913',
        // O CONTORNO não é enfeite: a massa é quase preta e a cena é escura, então sem uma linha
        // clara em volta ela desaparece no fundo — foi o que o Gabriel viu ("quase não vejo ela").
        // É a mesma correção das máscaras, invertida: lá uma borda escura pra a face clara
        // destacar, aqui uma borda clara pra a massa escura existir.
        fio: .034,
        // Em fração da altura da arena. `altura` é MEDIDA e não derivada da largura: na primeira
        // versão o monte vinha do raio dos bojos, que era escalado pela LARGURA, e ele chegava a
        // ~140px enquanto as árvores têm ~165 — moita do tamanho de árvore, e o Oni sumindo
        // inteiro atrás dela. Altura própria e baixa é o que impede isso de voltar.
        largura: .155, altura: .062, espaco: .34,
    },
    // 👹 Os CHIFRES do Oni: sobem de trás de uma moita SORTEADA, param, os olhos acendem, e
    // afundam. VIGÍLIA — ele olha o fogo e não faz nada. Algo observando é o que faz o fogo
    // parecer o lugar de alguém.
    //
    // A moita é sorteada A CADA VISITA. Num ponto fixo a segunda aparição já seria previsível e a
    // terceira, decoração. E é por isso que ele não usa o `criarNoHorizonte`: aquele planta uma
    // cópia por ladrilho, e aparição é UMA.
    chifres: {
        corpo: '#180c06',
        // O chifre em quatro tons, da raiz à ponta, mais a cor dos anéis: ele era branco chapado.
        chifreRaiz: '#6b5a45', chifre: '#c9bc9e', chifrePonta: '#f2ead6', chifreAnel: '#7d6b52',
        olho: '255, 152, 68',
        // O pisca: `piscar` é o ciclo inteiro e `piscada` o tempo com o olho fechado. Fechado curto
        // (0.18 de 2.6) porque o que se lê é a INTERRUPÇÃO — olho fechado por muito tempo lê como
        // lâmpada queimando, não como bicho piscando.
        piscar: 2.6, piscada: .18,
        // Em fração da altura da arena.
        tamanho: .1,
        // As esperas encurtaram (era [13,25]): o Gabriel pediu pra elas aparecerem mais. Continua
        // sendo aparição — o tempo fora é ainda o dobro do tempo em cena, que é o que a mantém sendo
        // um acontecimento em vez de mascote do cenário.
        espera: [7, 14], subir: 1.1, olhar: [3, 7], descer: 1,
        // A TREMIDINHA na moita, antes de subir e antes de afundar (ideia do Gabriel). É o mesmo
        // truque da TERRA revirando antes do caixão sair: o aviso é a parte barata e mais eficaz
        // do susto, e transforma a subida em CONSEQUÊNCIA — o arbusto mexe, e só então sai o que
        // estava atrás dele. Sem isso, a figura simplesmente aparecia.
        tremer: .75,
    },
    // 🧌 A CLAVA do Troll — mesmo mecanismo dos chifres, outro gesto. Ela sobe de trás de uma
    // moita, VIRA PRA UM LADO, pausa, vira pro outro, pausa, e afunda.
    //
    // Ela já foi uma travessia acima das copas, e não funcionava por um motivo estrutural: o canvas
    // é FILHO da arena, então pinta sempre depois do `background` dela — enquanto a mata é ladrilho
    // de CSS, nada pode ficar ATRÁS das árvores, e a clava passava por cima delas. Trazê-la pro
    // primeiro plano, atrás de uma moita, resolve a profundidade com ordem de desenho em vez de um
    // z-index impossível: a moita é desenhada DEPOIS, e o que sobra pra fora é o que se vê.
    //
    // O gesto é o desenho todo. Sem ele, uma clava parada atrás de um arbusto é um poste; com o
    // olha-de-um-lado-olha-do-outro, quem está segurando ela existe.
    clava: {
        madeira: '#241408', metal: '#cfd6dc', brilho: '#f2f6f8',
        // Em fração da altura da arena. MENOR e mais ESTREITA que a primeira versão (era .19 com a
        // cabeça em .3 de meia-largura, e o Gabriel disse que estava "muito grande, muito largo" —
        // lia como tronco de árvore, não como clava). A proporção largura/comprimento caiu de .6
        // pra .38, e é a proporção que faz a leitura, não o tamanho absoluto.
        tamanho: .17,
        espera: [8, 16], subir: 1.1, descer: 1, tremer: .75,
        // `andar` é quanto ele leva pra atravessar o arbusto de ponta a ponta, `passos` quantas
        // passadas cabem nessa travessia, `gingado` o quanto a maça inclina em cada passada, e
        // `descido` o quanto ela afunda na folhagem (fração do próprio tamanho).
        andar: 5.5, passos: 4, gingado: .1, descido: .22,
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
            // O Folclore. As moitas vêm ANTES das duas aparições, e elas se escondem por RECORTE (ver
            // `criarAparicaoNaMoita`) em vez de por ordem de pintura. A ordem contrária seria mais simples e
            // foi a primeira tentativa — mas então o brilho dos olhos do Oni ficava atrás da folhagem, e o
            // pedido era justamente que ele vazasse ENTRE as folhas. Luz passa por folha; chifre não.
            //
            // Depois vem o sítio da fogueira, e o redemoinho por último de todos: ele atravessa a clareira
            // INTEIRA, na frente do maestro.fogo e das estacas, que é o que faz o sopro parecer ter chegado onde a
            // gente está. Ele é também o único que recebe o `maestro.vento` pra ESCREVER; a fogueira, pra ler.
            criarMoitas(ar.moitas, fundo),
            criarChifres(ar.chifres, fundo, ar.moitas),
            criarClava(ar.clava, fundo, ar.moitas),
            criarFogueira(ar.fogueira, fundo, maestro.vento, maestro.fogo),
            criarRedemoinho(ar.redemoinho, fundo, maestro.vento),
        ].filter(Boolean),
        naFrente: [
            criarPo(ar.po, frente, maestro.vento, maestro.fogo),
            criarVoadores(ar.voadores, frente, maestro.vento),
        ].filter(Boolean),
    };
}
/// O SÍTIO DA FOGUEIRA — a peça central do 🪬 Folclore.
///
/// É UM builder e não cinco porque é UMA composição: as sombras das estacas saem do pulso desta chama,
/// as máscaras são iluminadas por ela e a coluna de fumaça nasce dela. Separar em peças faria metades
/// lendo o mesmo número de dois lugares — o erro que o `--mata-passo` e as corujas já ensinaram a não
/// cometer. O `criarCastelo` tem o mesmo formato: muro, torres, casas, morro e nuvens num builder só.
///
/// A FOGUEIRA MORRE E RENASCE, e este é o ciclo que dá vida à cena (ideia do Gabriel):
///
///   acesa → o redemoinho passa POR CIMA dela → apagando → apagada (só fumaça preta)
///         → faísca → reacendendo → acesa → ... e aí o redemoinho volta a passar
///
/// A detecção de "por cima" não precisou de encanamento novo: o redemoinho já escreve `vento.x` no
/// maestro, e a fogueira sabe onde ela mesma está. Duas coisas que já existiam, e a interação sai de
/// uma comparação — nenhuma peça precisou saber que a outra existe.
///
/// A FULIGEM é o que conta a história do apagar. Fogo morrendo faz fumaça PRETA, então a cor da coluna
/// é interpolada entre o cinza-quente normal e a fuligem conforme a chama cai. Sem isso, o fogo
/// simplesmente desapareceria e a coluna seguiria clara — o que leria como bug, não como fogo apagando.
///
/// Um DESFILE de vultos dos quatro champs já viveu dentro desta fumaça. Saiu por pedido do Gabriel: os
/// quatro passaram a estar referenciados em todo o resto da cena (chifres, maça, corvos, cartas,
/// máscaras), e o vulto virou repetição — além de deixar a fumaça rígida, porque ela precisava manter
/// uma silhueta legível. Sem ele, a coluna pôde ficar fluida, que é o que fumaça é.
export function criarFogueira(cfg, canvas, vento, fogo) {
    const labaredas = Array.from({ length: cfg.labaredas }, (_, i) => ({
        posicao: (i + .5) / cfg.labaredas,
        ritmo: 2.6 + Math.random() * 2.8,
        fase: Math.random() * Math.PI * 2,
        alturaBase: .55 + Math.random() * .45,
    }));

    // As baforadas da coluna, em RODÍZIO: cada uma sobe do fogo até o fim da coluna e volta pro começo.
    // Espalhadas na largada (`i / sopros`) pra a coluna já nascer cheia — todas em u=0 fariam a fumaça
    // começar como uma bola só subindo.
    //
    // São MUITAS agora (11 contra 6) e cada uma com velocidade, raio e DUAS frequências de bamboleio
    // próprias. É o que trocou fumaça-de-desenho por fumaça fluida: com poucas baforadas grandes o olho
    // acompanha cada bola subindo; com muitas, sobrepostas e fora de compasso, o que se vê é uma massa
    // que rola. Isso só ficou possível quando o vulto saiu — antes a coluna tinha de manter uma
    // silhueta estável pra ele ser legível dentro dela.
    const sopros = Array.from({ length: cfg.coluna.sopros }, (_, i) => ({
        u: i / cfg.coluna.sopros,
        vel: .07 + Math.random() * .1,
        raio: .7 + Math.random() * .7,
        bamboleio: Math.random() * Math.PI * 2,
        ritmo: 1 + Math.random() * 1.3,
        ritmo2: 2.2 + Math.random() * 2,
    }));

    // O CICLO da chama. `viva` é 0..1 e multiplica tudo que é fogo: altura da labareda, raio do clarão,
    // comprimento das sombras, brasa no ar. Um número só, pra as cinco coisas não poderem discordar.
    let fase = 'acesa';
    let viva = 1;
    let relogio = 0;
    let brilhoDosTotens = 0;

    // O estalo: relógio próprio, e só acontece com a fogueira acesa.
    let paraEstalar = entre(cfg.estalo);
    let clarãoDoEstalo = 0;
    let faiscas = [];

    let t = 0;

    const cuspirFaiscas = (quantas, cx, y, forca) => {
        for (let i = 0; i < quantas; i++) {
            const ang = -Math.PI / 2 + (Math.random() - .5) * 1.5;
            const vel = (.16 + Math.random() * .34) * canvas.height * forca;
            faiscas.push({
                x: cx + (Math.random() - .5) * canvas.height * cfg.largura * .5, y,
                vx: Math.cos(ang) * vel, vy: Math.sin(ang) * vel,
                vida: .5 + Math.random() * .7, idade: 0, r: .8 + Math.random() * 1.4,
            });
        }
    };

    return (ctx, dt) => {
        t += dt;

        const l = canvas.height * cfg.largura, h = canvas.height * cfg.altura;
        const cx = canvas.width / 2;
        const base = canvas.height;
        const bocaY = base - h * .52;                  // de onde saem as chamas e a fumaça
        const v = vento?.forca ?? 0;

        // O pulso da chama: duas frequências, como no reator. Uma só daria um pisca regular demais.
        const pulso = .84 + Math.sin(t * 3.3) * .1 + Math.sin(t * 8.1) * .06;

        // ---------- o ciclo apagar → faísca → reacender ----------
        // "Por cima" = o redemoinho está sobre a fogueira E está soprando de verdade. As duas condições
        // juntas: só a posição faria a fogueira apagar quando ele passa longe e fraco na borda da tela.
        const emCima = vento && Math.abs(vento.x - cx) < l * cfg.alcanceDoVento && Math.abs(v) > .3;

        relogio -= dt;
        switch (fase) {
            case 'acesa':
                if (emCima) fase = 'apagando';
                break;
            case 'apagando':
                viva = Math.max(0, viva - dt / cfg.apagar);
                if (viva === 0) { fase = 'apagada'; relogio = entre(cfg.escuro); }
                break;
            case 'apagada':
                if (relogio <= 0) {
                    fase = 'faisca';
                    relogio = cfg.faisca;
                    // a faísca que reacende: poucas, fracas e no meio das achas
                    cuspirFaiscas(cfg.faiscasDoReacender, cx, bocaY, .45);
                }
                break;
            case 'faisca':
                if (relogio <= 0) {
                    fase = 'reacendendo';
                    // Os TOTENS piscam no instante em que o fogo pega (pedido do Gabriel). É de graça e
                    // amarra as máscaras ao fogo: elas reagem porque estão ali, iluminadas por ele.
                    brilhoDosTotens = 1;
                }
                break;
            case 'reacendendo':
                viva = Math.min(1, viva + dt / cfg.reacender);
                if (viva === 1) fase = 'acesa';
                break;
        }
        brilhoDosTotens = Math.max(0, brilhoDosTotens - dt / cfg.brilhoTotem);

        // O maestro do fogo: as brasas no ar (que são camada da FRENTE, outro builder) leem daqui.
        // Brasa saindo de fogueira apagada seria o detalhe que denuncia que o apagar é só pintura.
        if (fogo) fogo.viva = viva;

        // ---------- o estalo, só com fogo aceso ----------
        clarãoDoEstalo = Math.max(0, clarãoDoEstalo - dt / cfg.estalar);
        if (fase === 'acesa') {
            paraEstalar -= dt;
            if (paraEstalar <= 0) {
                paraEstalar = entre(cfg.estalo);
                clarãoDoEstalo = 1;
                cuspirFaiscas(cfg.faiscas, cx, bocaY - h * .1, 1);
            }
        }

        ctx.save();

        // ---------- 1. o clarão: é ele que põe a fogueira dentro de uma mata escura em vez de deixá-la
        //     como um recorte laranja. E é o que vaza pelos dois lados da coluna do log.
        if (viva > 0) {
            const raio = l * cfg.clarao * (pulso + clarãoDoEstalo * .22) * viva;
            const clarao = ctx.createRadialGradient(cx, bocaY, 0, cx, bocaY, raio);
            clarao.addColorStop(0, `rgba(${cfg.fogo}, ${(.34 + clarãoDoEstalo * .1) * viva})`);
            clarao.addColorStop(.4, `rgba(${cfg.fogo}, ${.12 * viva})`);
            clarao.addColorStop(1, `rgba(${cfg.fogo}, 0)`);
            ctx.fillStyle = clarao;
            ctx.beginPath();
            ctx.arc(cx, bocaY, raio, 0, Math.PI * 2);
            ctx.fill();
        }

        // ---------- 2. as SOMBRAS das estacas, esticando no chão pra LONGE do fogo.
        //     Elas são a prova de que o fogo ilumina algo — sem sombra, a fogueira é um adesivo aceso no
        //     meio da tela. E é o COMPRIMENTO delas que pulsa com a chama, não a opacidade: sombra que
        //     pisca lê como lâmpada com mau contato; sombra que ESTICA lê como fogo. Some com a chama.
        if (viva > .02) {
            for (const e of cfg.estacas.pontos) {
                const ex = cx + e.x * l;
                const lado = Math.sign(e.x) || 1;
                const comprimento = l * (1.5 + pulso * .7) * (1 + Math.abs(e.x) * .12) * viva;
                const g = ctx.createLinearGradient(ex, base, ex + lado * comprimento, base);
                g.addColorStop(0, `rgba(${cfg.estacas.sombra}, ${.5 * viva})`);
                g.addColorStop(1, `rgba(${cfg.estacas.sombra}, 0)`);
                ctx.fillStyle = g;
                ctx.beginPath();
                ctx.moveTo(ex, base - h * .04);
                ctx.lineTo(ex + lado * comprimento, base - h * .02);
                ctx.lineTo(ex + lado * comprimento, base + h * .1);
                ctx.lineTo(ex, base + h * .06);
                ctx.closePath();
                ctx.fill();
            }
        }

        // ---------- 3. a COLUNA DE FUMAÇA ----------
        const alcance = canvas.height * cfg.coluna.alcance;
        // A inclinação cresce com a altura (u²): o pé da fumaça está preso ao fogo, e é só mais em cima
        // que o vento a arrasta. Linear, a coluna inteira tombaria como um poste caindo.
        const desvio = (u) => v * l * 2.6 * u * u;
        const meia = (u) => l * (.42 + (cfg.coluna.abre - .42) * u) * .5;

        // A FULIGEM: fogo morrendo faz fumaça preta. `1 - viva` é exatamente isso, e sai de graça do
        // número que já governa a chama — não há um segundo relógio pra a cor da fumaça discordar dela.
        const fuligem = 1 - viva;
        const corDaFumaca = cfg.coluna.cor.split(',').map(Number).map((c, i) => {
            const preto = cfg.coluna.fuligem.split(',').map(Number)[i];
            return Math.round(c + (preto - c) * fuligem);
        }).join(', ');
        // Apagada ela rarefaz; no instante da faísca dá uma baforada. Uma coluna de densidade constante
        // faria a fogueira morta fumegar igual à acesa.
        const densidade = cfg.coluna.opacidade
            * (fase === 'apagada' ? .4 : fase === 'faisca' ? 1.15 : 1);

        ctx.save();
        ctx.beginPath();
        for (let i = 0; i <= 12; i++) {
            const u = i / 12;
            const x = cx + desvio(u) - meia(u);
            const y = bocaY - alcance * u;
            i === 0 ? ctx.moveTo(x, y) : ctx.lineTo(x, y);
        }
        for (let i = 12; i >= 0; i--) {
            const u = i / 12;
            ctx.lineTo(cx + desvio(u) + meia(u), bocaY - alcance * u);
        }
        ctx.closePath();
        ctx.clip();

        // o corpo: quente embaixo quando há fogo (a base da fumaça pega a cor da chama), apagando no alto
        const corpo = ctx.createLinearGradient(cx, bocaY, cx, bocaY - alcance);
        corpo.addColorStop(0, `rgba(${viva > .3 ? cfg.fogo : corDaFumaca}, ${densidade * .9})`);
        corpo.addColorStop(.22, `rgba(${corDaFumaca}, ${densidade})`);
        corpo.addColorStop(1, `rgba(${corDaFumaca}, 0)`);
        ctx.fillStyle = corpo;
        ctx.fillRect(cx - l * cfg.coluna.abre, bocaY - alcance, l * cfg.coluna.abre * 2, alcance + h);

        // as baforadas. Cada uma tem duas frequências de vaivém, e é a soma delas que faz a massa rolar
        // em vez de subir em fila.
        //
        // JÁ TENTEI TIRAR O CONE, e ficou pior — fica registrado pra ninguém tentar de novo. A ideia
        // era copiar o vapor da lâmpada dos 🐉 Místicos, que é só baforada sem corpo nenhum e lê muito
        // bem. Mas os dois têm trabalhos diferentes: o vapor é um FIAPO fino saindo de um bico, e
        // baforada solta é exatamente a forma disso; esta aqui é uma COLUNA de fumaça de fogueira, e
        // sem um corpo por trás as bolas leem como bolhas soltas subindo. O cone dá a massa, e as
        // baforadas dão o movimento dentro dela — é a soma que funciona.
        for (const s of sopros) {
            s.u += s.vel * dt;
            if (s.u > 1) s.u -= 1;
            s.bamboleio += dt;

            const balanco = Math.sin(s.bamboleio * s.ritmo) * .62 + Math.sin(s.bamboleio * s.ritmo2) * .38;
            const y = bocaY - alcance * s.u;
            const x = cx + desvio(s.u) + balanco * l * .3 * s.u;
            const r = meia(s.u) * s.raio * 1.5;
            // Apaga nas duas pontas: nasce do fogo (onde a chama já pinta) e morre no alto.
            const forca = Math.sin(Math.min(1, s.u * 1.15) * Math.PI) * densidade;

            const g = ctx.createRadialGradient(x, y, 0, x, y, r);
            g.addColorStop(0, `rgba(${corDaFumaca}, ${forca})`);
            g.addColorStop(1, `rgba(${corDaFumaca}, 0)`);
            ctx.fillStyle = g;
            ctx.beginPath();
            ctx.ellipse(x, y, r, r * (.72 + Math.sin(s.bamboleio * s.ritmo) * .12), 0, 0, Math.PI * 2);
            ctx.fill();
        }
        ctx.restore();

        // ---------- 4. as PEDRAS em volta: o círculo que diz que a fogueira foi FEITA por alguém.
        //     Mais claras na face virada pro fogo, escuras na de fora — é a única coisa da cena com duas
        //     faces, e é o que dá volume à base sem desenhar detalhe.
        for (let i = 0; i < 7; i++) {
            const ang = Math.PI + (i / 6) * Math.PI;   // meia-lua: só as da frente se veem
            const px = cx + Math.cos(ang) * l * .56;
            const py = base - h * .06 + Math.sin(ang) * h * .1;
            ctx.fillStyle = i % 2 ? cfg.pedra : cfg.pedraLuz;
            ctx.beginPath();
            ctx.ellipse(px, py, l * .11, h * .1, ang * .3, 0, Math.PI * 2);
            ctx.fill();
        }

        // ---------- 5. as ACHAS cruzadas, em X. Duas toras paralelas leriam como banco.
        ctx.strokeStyle = cfg.acha;
        ctx.lineWidth = Math.max(3, l * .1);
        ctx.lineCap = 'round';
        for (const s of [-1, 1]) {
            ctx.beginPath();
            ctx.moveTo(cx - s * l * .44, base - h * .02);
            ctx.lineTo(cx + s * l * .3, base - h * .42);
            ctx.stroke();
        }

        // ---------- 6. as LABAREDAS. Mesmo desenho do reator (gradiente que apaga na ponta, e não
        //     traço), com duas diferenças que são o tema inteiro: a ponta é DEITADA PELO VENTO, e a
        //     altura é multiplicada por `viva` — é assim que o fogo morre e volta sem um segundo desenho.
        if (viva > .02) {
            for (const f of labaredas) {
                const x = cx - l * .34 + l * .68 * f.posicao;
                const tremer = .6 + Math.sin(t * f.ritmo + f.fase) * .25 + Math.sin(t * f.ritmo * 2.4) * .15;
                const alt = h * (1.5 + clarãoDoEstalo * .3) * f.alturaBase * tremer * viva;
                const larg = l * .1 * (.8 + tremer * .4);
                // O balanço natural da chama MAIS o empurrão do vento. O primeiro nunca desaparece: um
                // fogo que só se mexe quando o vento passa fica parado e morto no resto do tempo.
                const ponta = x + Math.sin(t * f.ritmo * .8 + f.fase) * larg * 1.2 + v * l * 2.4;

                const chama = ctx.createLinearGradient(x, bocaY, ponta, bocaY - alt);
                chama.addColorStop(0, `rgba(${cfg.brasa}, ${.9 * viva})`);
                chama.addColorStop(.35, `rgba(${cfg.fogo}, ${.68 * viva})`);
                chama.addColorStop(1, `rgba(${cfg.fogo}, 0)`);
                ctx.fillStyle = chama;

                ctx.beginPath();
                ctx.moveTo(x - larg, bocaY);
                ctx.quadraticCurveTo(x - larg * .5, bocaY - alt * .6, ponta, bocaY - alt);
                ctx.quadraticCurveTo(x + larg * .5, bocaY - alt * .6, x + larg, bocaY);
                ctx.closePath();
                ctx.fill();
            }
        }

        // ---------- 7. as FAÍSCAS: sobem, o vento as arrasta e elas apagam. Servem o estalo (fogo aceso)
        //     e a faísca que reacende (fogo morto) — mesmo desenho, dois donos.
        for (let i = faiscas.length - 1; i >= 0; i--) {
            const f = faiscas[i];
            f.idade += dt;
            if (f.idade >= f.vida) { faiscas.splice(i, 1); continue; }

            f.vy += canvas.height * .34 * dt;         // a gravidade traz a faísca de volta
            f.x += (f.vx + v * canvas.width * .1) * dt;
            f.y += f.vy * dt;

            ctx.fillStyle = `rgba(${cfg.brasa}, ${(1 - f.idade / f.vida) * .9})`;
            ctx.beginPath();
            ctx.arc(f.x, f.y, f.r, 0, Math.PI * 2);
            ctx.fill();
        }

        // ---------- 8. as ESTACAS com máscaras, na frente de tudo: é o anel que transforma a fogueira
        //     num SÍTIO. Ficam por último porque estão entre o fogo e a gente. O `brilhoDosTotens` é o
        //     pisca do instante em que o fogo pega; o `pulso * viva` é a luz normal delas, que morre
        //     junto com a chama — máscara acesa em volta de fogueira apagada entregaria o truque.
        for (const e of cfg.estacas.pontos) {
            desenharEstaca(ctx, cx + e.x * l, base - h * .02, h * e.alt, e,
                Math.sign(e.x) || 1, pulso * (.25 + viva * .75), brilhoDosTotens, cfg.estacas);
        }

        ctx.restore();
    };
}

/// Uma ESTACA com máscara amarrada. Reescrita depois de o Gabriel dizer que as máscaras ficaram ruins —
/// e o diagnóstico é que elas eram PEQUENAS e sem contorno, então leram como bolhas pálidas em palitos.
///
/// Três coisas mudaram, e as três são sobre LEITURA e não sobre detalhe:
///   1. TAMANHO. A máscara saiu de .42 da altura da estaca pra `cfg.escala` (.62), e são três em vez de
///      quatro. Quatro pequenas ocupavam a mesma área que três grandes e não diziam nada.
///   2. CONTORNO. Uma borda escura em volta. Sem ela, a face clara encostava direto no fundo escuro e a
///      silhueta se desfazia justamente onde ela precisa ser lida.
///   3. FORMA. Duas caras de verdade — `longa` (comprida, queixo em ponta) e `redonda` —, em vez de duas
///      bocas diferentes na mesma oval. O que se reconhece de longe é o contorno, não a boca.
///
/// Continua sem `destination-out` pros olhos: aquilo apaga pixel de verdade, e o clarão do fogo passaria
/// POR DENTRO da máscara. Olho é traço escuro pintado em cima, e ponto.
export function desenharEstaca(ctx, x, base, alt, ponto, lado, pulso, brilho, cfg) {
    const { giro, cara, tribo, faixas } = ponto;
    const s = alt * (cfg.escala ?? .62);
    const longa = cara === 'longa';
    const rx = s * (longa ? .4 : .5), ry = s * (longa ? .6 : .5);

    ctx.save();
    ctx.translate(x, base);
    ctx.rotate(giro);

    // o poste
    ctx.strokeStyle = cfg.poste;
    ctx.lineWidth = Math.max(2, s * .14);
    ctx.lineCap = 'round';
    ctx.beginPath();
    ctx.moveTo(0, 0);
    ctx.lineTo(0, -alt);
    ctx.stroke();

    ctx.translate(0, -alt + s * .12);

    // O BRILHO do totem no instante em que a fogueira pega (ver `criarFogueira`): um halo atrás da
    // máscara, e nada além. Pintado ANTES dela pra vazar pelas beiradas — halo por cima lavaria a cara e
    // apagaria os olhos e a boca justamente no momento em que se quer chamar atenção pra eles.
    if (brilho > 0) {
        const raio = s * 1.5;
        const halo = ctx.createRadialGradient(0, 0, 0, 0, 0, raio);
        halo.addColorStop(0, `rgba(${cfg.aceso}, ${.5 * brilho})`);
        halo.addColorStop(.45, `rgba(${cfg.aceso}, ${.18 * brilho})`);
        halo.addColorStop(1, `rgba(${cfg.aceso}, 0)`);
        ctx.fillStyle = halo;
        ctx.beginPath();
        ctx.arc(0, 0, raio, 0, Math.PI * 2);
        ctx.fill();
    }

    // O contorno da cara, desenhado como caminho pra as duas formas: a `longa` tem queixo em ponta (é o
    // que a faz parecer entalhada em madeira), a `redonda` é uma oval cheia.
    const contorno = () => {
        ctx.beginPath();
        if (longa) {
            ctx.moveTo(0, -ry);
            ctx.quadraticCurveTo(rx, -ry * .8, rx, -ry * .05);
            ctx.quadraticCurveTo(rx * .9, ry * .5, 0, ry);
            ctx.quadraticCurveTo(-rx * .9, ry * .5, -rx, -ry * .05);
            ctx.quadraticCurveTo(-rx, -ry * .8, 0, -ry);
        } else {
            ctx.ellipse(0, 0, rx, ry, 0, 0, Math.PI * 2);
        }
        ctx.closePath();
    };

    // a borda escura, um pouco maior que a face: é ela que separa a máscara do fundo
    ctx.save();
    ctx.scale(1.14, 1.1);
    contorno();
    ctx.fillStyle = cfg.borda;
    ctx.fill();
    ctx.restore();

    // A FACE em MADEIRA, com três paradas — claro, ocre, escuro. É o mesmo truque do chifre do Oni: o
    // tom não é chapado, ele tem uma direção, e é a direção que faz a peça parecer entalhada em vez de
    // recortada.
    //
    // E a direção MUDA de totem pra totem (`luz`): uma acende do CENTRO pras bordas, outra de CIMA pra
    // baixo, outra de BAIXO pra cima. Três máscaras com o mesmo gradiente leriam como três cópias do
    // mesmo objeto; variar só a luz as separa sem precisar de três desenhos.
    //
    // No modo `centro` o foco é deslocado pro lado do fogo (`lado`), porque a luz da cena vem de lá — um
    // brilho no meio exato pareceria iluminação própria.
    const luz = ponto.luz === 'centro'
        ? ctx.createRadialGradient(lado * rx * .22, -ry * .12, 0, 0, 0, Math.max(rx, ry) * 1.15)
        : ponto.luz === 'baixo'
            ? ctx.createLinearGradient(0, ry, 0, -ry)
            : ctx.createLinearGradient(0, -ry, 0, ry);
    luz.addColorStop(0, cfg.mascaraLuz);
    luz.addColorStop(.5, cfg.mascaraOcre);
    luz.addColorStop(1, cfg.mascara);
    ctx.fillStyle = luz;
    ctx.globalAlpha = .8 + pulso * .18;
    contorno();
    ctx.fill();
    ctx.globalAlpha = 1;

    // AS FAIXAS DE TRIBO, recortadas pela própria cara pra a pintura não escorrer fora dela — máscara é
    // pintada NA madeira, e tinta que passa da borda entrega que são duas figuras empilhadas.
    //
    // Cada máscara tem o seu padrão (`faixas`) e a sua cor (`tribo`), e é essa variedade que as separa:
    // três máscaras iguais em três estacas leem como um objeto repetido, não como três máscaras.
    if (tribo) {
        ctx.save();
        contorno();
        ctx.clip();
        ctx.fillStyle = tribo;
        if (faixas === 'barra') {
            // uma barra larga na linha dos olhos e duas marcas curtas no queixo
            ctx.fillRect(-rx, -ry * .38, rx * 2, ry * .3);
            for (const o of [-1, 1]) ctx.fillRect(o * rx * .34 - rx * .06, ry * .52, rx * .12, ry * .4);
        } else if (faixas === 'meia') {
            // a metade de cima pintada: o padrão mais forte dos três, pro totem mais alto
            ctx.fillRect(-rx, -ry, rx * 2, ry * .52);
        } else {
            // raios: três diagonais em cada face, saindo do centro pra fora
            for (const o of [-1, 1]) {
                for (let i = 0; i < 3; i++) {
                    ctx.save();
                    ctx.translate(o * rx * .3, -ry * .1 + i * ry * .34);
                    ctx.rotate(o * .5);
                    ctx.fillRect(-rx * .28, -ry * .05, rx * .56, ry * .1);
                    ctx.restore();
                }
            }
        }
        ctx.restore();
    }

    // olhos: fendas inclinadas, uma pra cada lado. Inclinadas e não redondas porque olho redondo lê
    // como surpresa, e máscara de causo é cara parada.
    ctx.fillStyle = cfg.traco;
    for (const o of [-1, 1]) {
        ctx.save();
        ctx.translate(o * rx * .42, -ry * .2);
        ctx.rotate(o * .3);
        ctx.beginPath();
        ctx.ellipse(0, 0, rx * .26, ry * .13, 0, 0, Math.PI * 2);
        ctx.fill();
        ctx.restore();
    }

    // a boca, e a sobrancelha que dá expressão sem custar nada
    ctx.beginPath();
    if (longa) {
        ctx.rect(-rx * .34, ry * .3, rx * .68, ry * .1);         // fenda reta: cara séria
    } else {
        ctx.moveTo(-rx * .3, ry * .26);                          // bico: triângulo pra baixo
        ctx.lineTo(rx * .3, ry * .26);
        ctx.lineTo(0, ry * .6);
    }
    ctx.closePath();
    ctx.fill();

    ctx.fillRect(-rx * .62, -ry * .52, rx * 1.24, ry * .09);     // a sobrancelha, uma barra só

    ctx.restore();
}

/// A paleta da CARTA mora aqui, e não nas configs de tema, porque ela é do OBJETO e não do cenário:
/// baralho é vermelho e creme em qualquer clareira. Os dois lugares que desenham carta (o chão da
/// fogueira e o redemoinho que a levanta) são builders diferentes, com configs diferentes — se a cor
/// morasse nas duas, seriam duas chances de um dia discordarem, e a carta que voa TEM de ser
/// reconhecidamente a mesma que estava no chão. Foi exatamente esse o defeito: a config do redemoinho
/// tinha as cores, a da fogueira não, e as cartas do chão saíam com `fillStyle` inválido — que o
/// canvas IGNORA em silêncio, herdando a última cor usada em vez de dar erro.
export const CARTA = { dorso: '#8e1f2a', face: '#ecdfc4', fio: '#2a1206' };

/// Uma CARTA de baralho: retângulo com cantos redondos, dorso liso ou face com um naipe. Serve o chão
/// da clareira (parada) e o redemoinho (rodando) — é a mesma carta, então é um desenho só.
///
/// `mostrandoFace` decide o lado que se vê. No redemoinho ela gira, e é a TROCA de lado no meio do
/// giro que faz o retângulo parecer uma carta virando em vez de um cartão deslizando.
export function desenharCarta(ctx, x, y, s, giro, mostrandoFace) {
    const l = s * .68, a = s;

    ctx.save();
    ctx.translate(x, y);
    ctx.rotate(giro);

    // Cantos redondos à mão, com `arcTo`, e NÃO com `ctx.roundRect`: aquele só existe no Chromium 99+,
    // e é a única API do arquivo inteiro que dependeria da versão do runtime do WebView2 na máquina.
    // Numa máquina com runtime antigo ele lançaria TypeError a cada quadro, e o cenário morreria
    // inteiro por causa de um raio de canto. O `arcTo` é do Canvas desde sempre.
    const r = s * .1;
    ctx.beginPath();
    ctx.moveTo(-l / 2 + r, -a / 2);
    ctx.arcTo(l / 2, -a / 2, l / 2, a / 2, r);
    ctx.arcTo(l / 2, a / 2, -l / 2, a / 2, r);
    ctx.arcTo(-l / 2, a / 2, -l / 2, -a / 2, r);
    ctx.arcTo(-l / 2, -a / 2, l / 2, -a / 2, r);
    ctx.closePath();
    ctx.fillStyle = mostrandoFace ? CARTA.face : CARTA.dorso;
    ctx.fill();
    ctx.lineWidth = Math.max(.6, s * .04);
    ctx.strokeStyle = CARTA.fio;
    ctx.stroke();

    // O naipe: um losango, e só na face. A esta escala o desenho do naipe não importa — o que
    // importa é ter UMA marca no meio, que é o que diz "carta" e não "papel".
    if (mostrandoFace) {
        ctx.fillStyle = CARTA.dorso;
        ctx.beginPath();
        ctx.moveTo(0, -a * .16);
        ctx.lineTo(l * .17, 0);
        ctx.lineTo(0, a * .16);
        ctx.lineTo(-l * .17, 0);
        ctx.closePath();
        ctx.fill();
    }

    ctx.restore();
}

/// AS MOITAS da clareira, e a única coisa da cena que tem ENDEREÇO.
///
/// Elas existem por causa das aparições: o 👹 sobe os chifres de trás de UMA delas, o 🧌 levanta a clava
/// de trás de outra. Sem um arbusto concreto, as duas apareceriam no ar em coordenada arbitrária — e
/// "de trás de quê?" é justamente a pergunta que faz uma aparição ser um acontecimento num lugar.
///
/// Por isso elas são canvas e não entraram no ladrilho do CSS: num ladrilho que repete a cada 372px
/// existe uma cópia idêntica de cada moita na tela, então "aquela moita ali" não quer dizer nada — e o
/// que é "a terceira" muda com a largura da janela.
///
/// A LISTA É DETERMINÍSTICA (zero Math.random), pelo mesmo motivo do `telhadosDoReino`: ela tem TRÊS
/// clientes — a camada que desenha, os chifres e a clava. Se cada um sorteasse a sua, o Oni subiria
/// atrás de uma moita que não está desenhada ali. O sorteio fica em QUEM ESCOLHE, não em onde elas
/// estão; e de quebra elas não pulam de lugar quando a janela muda de tamanho.
/// A lista é MEMOIZADA, e isso deixou de ser só economia: os três clientes recebem o MESMO array, então
/// a tremidinha que o Oni escreve numa moita é a tremidinha que a camada de desenho lê. Antes cada um
/// construía a sua cópia — idênticas em valor, porque a geração é determinística, mas objetos
/// diferentes; escrever em uma não afetava as outras, e o tremor nunca sairia do lugar.
export let moitasMemo = { chave: '', lista: [] };

export function moitasDaMata(cfg, l, h) {
    const chave = `${l}|${h}|${cfg.largura}|${cfg.altura}|${cfg.espaco}`;
    if (moitasMemo.chave === chave) return moitasMemo.lista;

    // Hash de índice em vez de sorteio: variedade estável. O `- Math.floor` é o que traz pra 0..1.
    const r = (i, k) => {
        const x = Math.sin(i * 127.1 + k * 311.7) * 43758.5453;
        return x - Math.floor(x);
    };

    const quantas = Math.max(4, Math.round(l / (h * cfg.espaco)));

    const lista = Array.from({ length: quantas }, (_, i) => {
        const larg = h * cfg.largura * (.66 + r(i, 1) * .7);
        // A altura vem da ALTURA configurada, não da largura. Amarrada num intervalo estreito
        // (.8 a 1.25) porque moita é moita: a variedade que interessa é a do contorno, e a de tamanho
        // solta foi o que produziu o monte do tamanho de uma árvore.
        const alt = h * cfg.altura * (.8 + r(i, 2) * .45);

        // O CONTORNO: uma crista de pontos com alturas diferentes, mais alta no meio. É esta lista que
        // faz cada moita ter cara própria — e ela vira o desenho inteiro, sem bojo nenhum por cima
        // (o Gabriel: "podem ser só com um contorno, não precisa de mais círculos").
        const bicos = 5 + Math.floor(r(i, 3) * 3);
        const crista = Array.from({ length: bicos }, (_, k) => {
            const u = -1 + (2 * k) / (bicos - 1);
            // o arco geral (seno) dá a cúpula; o hash quebra a regularidade dela
            const arco = .34 + .66 * Math.sin(((k + .5) / bicos) * Math.PI);
            return { u, a: arco * (.74 + r(i, 20 + k) * .42) };
        });

        return {
            // Espaçamento irregular: passo fixo viraria cerca viva. O empurrão é limitado a 70% do vão
            // pra elas não se atropelarem nem abrirem buraco no meio da clareira.
            x: l * ((i + .5) / quantas) + (r(i, 4) - .5) * (l / quantas) * .7,
            larg, alt, crista,
            // Os galhos secos saindo por cima, que é o que dá silhueta ao topo.
            galhos: Array.from({ length: 3 }, (_, g) => ({
                u: (r(i, 40 + g) - .5) * 1.5,
                sobe: .5 + r(i, 50 + g) * .7,
                torto: (r(i, 60 + g) - .5) * .5,
            })),
            // A ALTURA REAL do que fica visível, publicada aqui: é a crista mais alta. As aparições
            // ancoram nela, então "onde a folhagem termina" é UM número, e não uma conta que a camada
            // de desenho e as aparições fazem cada uma do seu jeito — que foi exatamente o defeito que
            // engoliu o Oni: a crista calculada como `alt * .92` ficava abaixo da folhagem de verdade.
            topo: alt * Math.max(...crista.map(c => c.a)),
            // Escrito pelas aparições (0 = parada, 1 = tremendo forte) e lido pelo desenho.
            tremor: 0,
        };
    });

    moitasMemo = { chave, lista };
    return lista;
}

/// Desenha as moitas. Vem DEPOIS dos chifres e da clava na fila de camadas, e é essa ordem que faz a
/// profundidade: o que a moita cobre, ela cobre — sem clip, sem z-index, sem máscara.
export function criarMoitas(cfg, canvas) {
    let t = 0;

    return (ctx, dt) => {
        t += dt;
        const moitas = moitasDaMata(cfg, canvas.width, canvas.height);

        for (const m of moitas) {
            desenharMoita(ctx, m, canvas.height, canvas.width, t, cfg);
            // O tremor DECAI aqui, e quem está tremendo o reescreve a cada quadro. Assim ele se cura
            // sozinho: se uma aparição for cancelada no meio do gesto (a janela mudou de tamanho, por
            // exemplo), a moita para de tremer em vez de tremer pra sempre.
            if (m.tremor > 0) m.tremor = Math.max(0, m.tremor - dt * 5);
        }
    };
}

/// Uma MOITA: bojos de folhagem sobrepostos, uns poucos galhos saindo por cima, e uma fatia de luz do
/// lado do fogo. Os bojos vêm do mesmo hash da posição, então a mesma moita é sempre a mesma moita.
///
/// O topo é IRREGULAR de propósito — é a borda por onde os chifres e a clava aparecem, e uma borda reta
/// entregaria que ali existe um recorte em vez de um arbusto.
/// Uma MOITA: UM contorno fechado, e nada por dentro. A versão anterior empilhava bojos de folhagem, e
/// duas coisas davam errado — o raio deles vinha da LARGURA, então o monte crescia até quase a altura das
/// árvores, e a massa quase preta sem linha nenhuma em volta desaparecia no fundo escuro.
///
/// Agora é fill escuro + STROKE claro. O stroke é o que faz a moita existir: contra uma cena escura, a
/// silhueta preta não tem borda, e sem borda não há forma. A crista de bicos vem pronta da
/// `moitasDaMata`, então o topo é irregular — e ele precisa ser, porque é por ali que o Oni e a clava
/// aparecem: um topo reto entregaria que existe um recorte em vez de um arbusto.
export function desenharMoita(ctx, m, alturaDaArena, larguraDaArena, t, cfg) {
    const base = alturaDaArena;

    // A TREMIDINHA: chacoalho horizontal rápido, com duas frequências pra não virar vibração de motor.
    // Só a folhagem mexe — o pé fica plantado, e é isso que faz parecer alguém mexendo o arbusto de
    // dentro em vez de o arbusto inteiro escorregando pro lado.
    const tremor = m.tremor > 0
        ? (Math.sin(t * 41) * .6 + Math.sin(t * 67) * .4) * m.tremor * m.larg * .07
        : 0;

    ctx.save();
    ctx.beginPath();
    ctx.moveTo(m.x - m.larg, base);
    for (const c of m.crista) {
        // o empurrão do tremor cresce com a altura do ponto: o pé não anda, a copa anda
        ctx.lineTo(m.x + c.u * m.larg + tremor * c.a, base - m.alt * c.a);
    }
    ctx.lineTo(m.x + m.larg, base);
    ctx.closePath();

    ctx.fillStyle = cfg.folha;
    ctx.fill();
    ctx.strokeStyle = cfg.contorno;
    ctx.lineWidth = Math.max(1.4, m.larg * cfg.fio);
    ctx.lineJoin = 'round';
    ctx.stroke();

    // Os galhos secos: também no tom do contorno, senão eles somem pelo mesmo motivo que a massa somia.
    ctx.strokeStyle = cfg.galho;
    ctx.lineWidth = Math.max(1, m.larg * .024);
    ctx.lineCap = 'round';
    for (const g of m.galhos) {
        const gx = m.x + g.u * m.larg * .8 + tremor;
        const gy = base - m.alt * .7;
        ctx.beginPath();
        ctx.moveTo(gx, gy);
        ctx.lineTo(gx + g.torto * m.larg * .3 + tremor * 1.6, gy - m.alt * g.sobe);
        ctx.stroke();
    }

    ctx.restore();
}

/// OS CHIFRES DO ONI — a referência do 👹, e a peça mais barata do tema: dois triângulos curvos e dois
/// pontos de luz.
///
/// Ela funciona por VIGÍLIA, não por ação. Ele sobe atrás da moita, os olhos acendem, ele fica
/// olhando o fogo, e afunde. Não ataca nada, não atravessa nada — e é justamente o não-fazer que dá o
/// efeito: uma coisa que observa transforma a fogueira em "o lugar de alguém que está sendo vigiado".
///
/// O x é SORTEADO a cada visita, e é por isso que ele não usa o `criarNoHorizonte`: aquele planta uma
/// cópia por ladrilho (é o certo pra coruja, que é fauna), e aparição é UMA. Num ponto fixo, a segunda
/// vez já seria previsível e a terceira, decoração.
///
/// O que ele lê do ladrilho é só a ALTURA, pra encostar na linha das árvores — a mesma fonte das
/// corujas e das bobinas, então mexer no ladrilho leva os chifres junto.
/// O MOTOR das duas aparições que sobem de trás de uma moita — os chifres do 👹 e a clava do 🧌.
///
/// Ele foi extraído quando apareceu o SEGUNDO cliente, não antes: a clava era uma travessia acima das
/// copas e virou isto, e nesse momento as duas passaram a ter a mesma espinha — esperar, escolher uma
/// moita, subir de trás dela, fazer o seu gesto, afundar. Duplicar essa máquina de fases daria duas
/// chances de elas divergirem em silêncio (uma reaparecendo na mesma moita, a outra não).
///
/// O que cada cliente traz é só o GESTO: uma função que recebe quanto tempo ele está lá em cima e
/// devolve o que desenhar. Os chifres acendem os olhos; a clava vira de um lado, pausa, vira do outro.
///
/// Não há CLIP aqui, e é de propósito: quem tapa a parte de baixo é a própria moita, desenhada DEPOIS
/// na fila de camadas. Recortar num retângulo daria uma borda reta atravessando o arbusto — o corte tem
/// que ser a silhueta da folhagem, e o jeito de conseguir isso de graça é ordem de pintura.
/// A config das MOITAS chega de fora (`moitasCfg`) em vez de ser copiada na config dos chifres e da
/// clava: quem manda na geometria dos arbustos é a camada `moitas`, e três cópias do mesmo
/// `largura`/`espaco` seriam três chances de o Oni subir atrás de um arbusto de outro tamanho.
export function criarAparicaoNaMoita(cfg, canvas, moitasCfg, gesto) {
    let fase = 'oculto';
    let relogio = entre(cfg.espera) * .45;
    let fora = 0;                       // 0 = todo escondido atrás da moita, 1 = todo à vista
    let moita = null;
    let emCena = 0;                     // quanto tempo já faz que ele está lá em cima
    let moitas = [];
    let assinatura = '';

    return (ctx, dt) => {
        const agora = `${canvas.width}|${canvas.height}`;
        if (agora !== assinatura) {
            assinatura = agora;
            moitas = moitasDaMata(moitasCfg, canvas.width, canvas.height);
            // A moita escolhida deixa de existir quando a janela muda: melhor cancelar a visita do que
            // continuar subindo atrás de um arbusto que mudou de tamanho no meio do gesto.
            if (moita) { moita = null; fase = 'oculto'; fora = 0; relogio = entre(cfg.espera) * .3; }
        }
        if (!moitas.length) return;

        relogio -= dt;
        const s = canvas.height * cfg.tamanho;

        // As duas fases de TREMOR escrevem na moita todo quadro; a camada das moitas decai o valor. A
        // força faz um arco (seno) em vez de ligar e desligar: o arbusto começa a mexer, mexe forte no
        // meio e assenta — chacoalho quadrado leria como falha de desenho.
        const sacudir = () => {
            const quanto = Math.max(0, relogio) / cfg.tremer;
            moita.tremor = Math.sin(quanto * Math.PI) * .9 + .1;
        };

        switch (fase) {
            case 'oculto':
                if (relogio <= 0) {
                    // A moita é sorteada AQUI, uma vez por visita. Sorteada por quadro, ele piscaria
                    // pela clareira inteira; fixa, a segunda visita já seria previsível.
                    moita = moitas[Math.floor(Math.random() * moitas.length)];
                    fase = 'tremendo';
                    relogio = cfg.tremer;
                    emCena = 0;
                    // O gesto sorteia aqui o que é dele (a clava sorteia pra que lado vai andar). Tem de
                    // ser UMA vez por visita: dentro do desenho, mudaria a cada quadro.
                    gesto.comecou?.();
                }
                break;
            case 'tremendo':
                // A moita mexe ANTES de qualquer coisa aparecer. É o mesmo papel da terra revirando
                // antes do caixão do cemitério: o aviso é o que transforma a subida em consequência.
                sacudir();
                if (relogio <= 0) fase = 'subindo';
                break;
            case 'subindo':
                fora = Math.min(1, fora + dt / cfg.subir);
                if (fora === 1) fase = 'em cena';
                break;
            case 'em cena':
                emCena += dt;
                // Quem decide quando o gesto acabou é o GESTO, não um cronômetro daqui: se fosse um
                // tempo fixo, a clava afundaria no meio de uma virada.
                if (gesto.acabou(emCena)) { fase = 'avisando'; relogio = cfg.tremer; }
                break;
            case 'avisando':
                // E treme DE NOVO antes de afundar, fechando o gesto do mesmo jeito que ele abriu.
                sacudir();
                if (relogio <= 0) fase = 'descendo';
                break;
            case 'descendo':
                fora = Math.max(0, fora - dt / cfg.descer);
                if (fora === 0) { fase = 'oculto'; relogio = entre(cfg.espera); moita = null; }
                break;
        }

        if (!moita || fora <= 0) return;

        // A figura é ancorada no TOPO REAL da folhagem (`moita.topo`, publicado pela `moitasDaMata`), e
        // desenha pra CIMA a partir dali — o que ela põe abaixo do zero fica atrás do arbusto.
        //
        // Antes isto era `moita.alt * .92`, um palpite: a folhagem de verdade subia mais que isso, e o
        // Oni inteiro (chifres, testa e olhos) nascia dentro da região coberta. Não aparecia nunca.
        const crista = canvas.height - moita.topo;

        // O RECORTE pela silhueta da moita. Esta camada agora é desenhada DEPOIS das moitas (senão o
        // brilho dos olhos não poderia vazar por cima da folhagem), e por isso ela não pode mais contar
        // com a ordem de pintura pra esconder o que está atrás do arbusto — precisa recortar.
        //
        // E o recorte é a CRISTA, não uma linha reta na altura dela: reto, a moita entregaria que ali
        // existe um corte de tesoura em vez de folhagem. O polígono é "tudo acima do chão menos o miolo
        // da moita" — desce pela direita da tela, anda pelo chão até o pé direito do arbusto, sobe pela
        // crista, desce no pé esquerdo e volta pelo chão.
        ctx.save();
        ctx.beginPath();
        ctx.moveTo(0, 0);
        ctx.lineTo(canvas.width, 0);
        ctx.lineTo(canvas.width, canvas.height);
        ctx.lineTo(moita.x + moita.larg, canvas.height);
        for (let k = moita.crista.length - 1; k >= 0; k--) {
            const c = moita.crista[k];
            ctx.lineTo(moita.x + c.u * moita.larg, canvas.height - moita.alt * c.a);
        }
        ctx.lineTo(moita.x - moita.larg, canvas.height);
        ctx.lineTo(0, canvas.height);
        ctx.closePath();
        ctx.clip();

        ctx.translate(moita.x, crista + s * (1 - fora));
        gesto.desenhar(ctx, s, emCena, fase);
        ctx.restore();

        // O passe DA FRENTE, sem recorte: é para o que tem de aparecer POR CIMA da folhagem em vez de
        // atrás dela — hoje só os olhos do Oni, que são luz vazando ENTRE as folhas (pedido do Gabriel:
        // "os olhos entre as folhagens"). Luz não é tapada por folha, ela passa; então ela é a única
        // coisa aqui que ignora a moita de propósito.
        if (!gesto.desenharNaFrente) return;
        ctx.save();
        ctx.translate(moita.x, crista + s * (1 - fora));
        gesto.desenharNaFrente(ctx, s, emCena, fase, moita);
        ctx.restore();
    };
}

/// 👹 OS CHIFRES DO ONI. Vigília: ele sobe, para, os olhos acendem, e afunda. Não ataca nada e não
/// atravessa nada — é o não-fazer que dá o efeito, porque uma coisa que OBSERVA transforma a fogueira
/// no lugar de alguém que está sendo vigiado.
///
/// Os olhos só acendem depois que ele parou. Acesos durante a subida, a aparição perderia o tempo dela:
/// primeiro a forma, depois a luz — é a ordem que assusta.
export function criarChifres(cfg, canvas, moitasCfg) {
    let olhar = entre(cfg.olhar);

    return criarAparicaoNaMoita(cfg, canvas, moitasCfg, {
        acabou: (emCena) => {
            if (emCena < olhar) return false;
            olhar = entre(cfg.olhar);      // o próximo olhar dura outro tanto
            return true;
        },
        // O y=0 é a LINHA DA FOLHAGEM, e o desenho sai pra CIMA a partir dela: o que ficar abaixo do zero
        // é recortado pela silhueta da moita. Por isso a testa é um domo BAIXO — só a tampa dela passa
        // da folhagem — e os chifres levam quase toda a altura.
        desenhar: (ctx, s) => {
            // a testa, escura: ela não é a figura, é o que dá de onde os chifres saem
            ctx.fillStyle = cfg.corpo;
            ctx.beginPath();
            ctx.ellipse(0, s * .2, s * .4, s * .34, 0, 0, Math.PI * 2);
            ctx.fill();

            // Os CHIFRES em BRANCO-OSSO (pedido do Gabriel: eles quase não apareciam). E a razão é a
            // mesma do contorno da moita e da borda das máscaras: contra uma cena escura, forma escura
            // não tem silhueta. Osso claro resolve de uma vez, e ainda casa com o vocabulário — chifre é
            // osso, então a cor certa é a cor de osso, não uma concessão de contraste.
            //
            // A ESPESSURA foi ajustada três vezes, e vale registrar o porquê: eu engrossei a raiz de .26
            // pra .34 porque eles "não apareciam" — mas a causa real era outra (eram escuros e a moita
            // alta os engolia). Consertados aqueles dois, a grossura sobrou duas vezes. Agora a raiz tem
            // ~.20 de largura, e o que faz um chifre ser chifre é AFINAR até a ponta, não ser largo.
            // A COR do chifre: gradiente da RAIZ pra PONTA, escuro embaixo e osso claro em cima, mais três
            // anéis de crescimento na metade de baixo. Antes era branco chapado, e chifre não é branco —
            // ele é escuro e sujo onde nasce na cabeça e vai clareando até a ponta gasta. O gradiente é o
            // que dá essa direção, e os anéis são o que dizem que aquilo CRESCEU em vez de ser um dente
            // colado. Os dois recortados pelo contorno do chifre, senão a tinta escorre pra fora dele.
            for (const lado of [-1, 1]) {
                const traco = () => {
                    ctx.beginPath();
                    ctx.moveTo(lado * s * .06, -s * .04);
                    ctx.lineTo(lado * s * .26, 0);
                    ctx.quadraticCurveTo(lado * s * .44, -s * .46, lado * s * .27, -s * .94);
                    ctx.quadraticCurveTo(lado * s * .26, -s * .48, lado * s * .05, -s * .12);
                    ctx.closePath();
                };

                const g = ctx.createLinearGradient(0, 0, lado * s * .12, -s * .94);
                g.addColorStop(0, cfg.chifreRaiz);
                g.addColorStop(.42, cfg.chifre);
                g.addColorStop(1, cfg.chifrePonta);
                ctx.fillStyle = g;
                traco();
                ctx.fill();

                ctx.save();
                traco();
                ctx.clip();
                ctx.fillStyle = cfg.chifreAnel;
                for (const a of [.1, .26, .42]) {
                    ctx.save();
                    ctx.translate(lado * s * (.2 - a * .18), -s * a);
                    ctx.rotate(lado * .34);
                    ctx.fillRect(-s * .22, -s * .022, s * .44, s * .044);
                    ctx.restore();
                }
                ctx.restore();
            }
        },
        // OS OLHOS vêm no passe da FRENTE, sem recorte, e ficam ABAIXO do zero — ou seja, dentro da
        // região da folhagem. É o "entre as folhagens" que o Gabriel pediu, e o desenho concorda com a
        // física: luz atravessa folha, então o brilho vaza; chifre não atravessa, então ele é recortado.
        //
        // E eles PISCAM. Abrir e fechar é o que os faz parecer olhos em vez de duas lanternas presas num
        // arbusto — e o fechado dura pouco, porque piscada longa lê como lâmpada com mau contato.
        desenharNaFrente: (ctx, s, emCena, fase) => {
            if (fase !== 'em cena' && fase !== 'avisando') return;

            // O ciclo: aberto por `piscar`, e uma fechada rápida no fim dele. Os dois olhos piscam
            // JUNTOS (é o mesmo bicho) — ao contrário das corujas do cemitério, que são bichos
            // diferentes e por isso nunca casam.
            const ciclo = emCena % cfg.piscar;
            const fechado = ciclo > cfg.piscar - cfg.piscada;
            if (fechado) return;

            // some suave nas duas pontas da abertura, senão a piscada vira um corte seco
            const beira = Math.min(1, (cfg.piscar - cfg.piscada - ciclo) / .12, ciclo / .12);

            for (const lado of [-1, 1]) {
                const ox = lado * s * .17, oy = s * .3;
                const raio = s * .16;
                const g = ctx.createRadialGradient(ox, oy, 0, ox, oy, raio);
                g.addColorStop(0, `rgba(${cfg.olho}, ${.98 * beira})`);
                g.addColorStop(.34, `rgba(${cfg.olho}, ${.55 * beira})`);
                g.addColorStop(1, `rgba(${cfg.olho}, 0)`);
                ctx.fillStyle = g;
                ctx.beginPath();
                ctx.arc(ox, oy, raio, 0, Math.PI * 2);
                ctx.fill();
            }
        },
    });
}

/// 🧌 A CLAVA DO TROLL. Mesmo motor dos chifres, gesto oposto: ela sobe, VIRA pra um lado, pausa, vira
/// pro outro, pausa, e afunda. Você nunca vê o Troll — vê o que ele carrega, e a escala da clava é o
/// que diz o tamanho do dono.
///
/// A PAUSA em cada ponta é o gesto inteiro. Sem ela o vaivém lê como pêndulo, ou seja coisa solta e
/// balançando; com ela, alguém está olhando pra um lado, decidindo, e olhando pro outro.
///
/// O tempo é montado a partir das durações, e não escrito à mão: virar → pausar → virar → pausar. Assim
/// mexer no `virar` não deixa um `acabou` desatualizado apontando pro instante errado.
export function criarClava(cfg, canvas, moitasCfg) {
    let rumo = 1;

    return criarAparicaoNaMoita(cfg, canvas, moitasCfg, {
        comecou: () => { rumo = Math.random() < .5 ? -1 : 1; },
        acabou: (emCena) => emCena >= cfg.andar,
        desenhar: (ctx, s, emCena, fase, moita) => {
            // ELE ANDA. O gesto antigo era balançar de um lado pro outro no mesmo ponto, e o Gabriel
            // cortou: "ela não balança de um lado para o outro, ele anda de uma ponta do arbusto até a
            // outra ponta". A diferença é grande — balanço é um objeto pendurado; travessia é alguém
            // ATRAVESSANDO ali atrás, e é o arbusto inteiro que passa a ser o palco.
            //
            // O sentido é sorteado por visita (`rumo`), senão ele sempre andaria pra direita e a segunda
            // aparição já entregaria a coreografia.
            const p = Math.min(1, emCena / cfg.andar);

            // A travessia é a largura da moita MENOS a meia-largura da própria maça: ela tem de acabar o
            // passeio ainda ATRÁS da folhagem, dos dois lados.
            //
            // Antes era `moita.larg * .78`, uma fração fixa que ignorava o tamanho da maça — e nas moitas
            // mais estreitas da clareira (elas variam de .66 a 1.36 do tamanho base) a ponta do passeio
            // caía fora do arbusto. Aí a maça aparecia INTEIRA, do cabo ao chão, porque o recorte só
            // protege o x da moita: passando dele, não há folhagem nenhuma pra esconder o que está abaixo
            // da crista. Era o "começa antes e morre depois dela".
            //
            // O piso de 12% existe pra que numa moita muito pequena ela ainda ande um pouco em vez de
            // ficar plantada no meio — pouco movimento é melhor que nenhum.
            const larguraDaMoita = moita?.larg ?? s;
            const meiaMaca = s * .34;                    // cabeça + espinhos, que é o que mais se abre
            const largura = Math.max(larguraDaMoita * .12, larguraDaMoita - meiaMaca);
            const x = rumo * (-largura + largura * 2 * p);

            // O PASSO: sobe-e-desce curto no ritmo da caminhada, e uma inclinação que acompanha. Sem ele
            // a maça desliza num trilho; com ele, quem carrega tem perna.
            const passo = Math.sin(p * Math.PI * 2 * cfg.passos);
            const y = -Math.abs(passo) * s * .07;

            ctx.save();
            // `descido` afunda a maça inteira: menos dela passa da folhagem, e o que sobra à vista é a
            // cabeça com os espinhos em vez de quase o cabo todo. Some no gesto sem mexer no tamanho.
            ctx.translate(x, y + s * cfg.descido);
            ctx.rotate(passo * cfg.gingado);

            // A MAÇA, no molde do cajado do mago do Reino: haste reta com um fio de luz numa borda, e a
            // cabeça por cima. Aquele desenho funciona porque é GEOMETRIA SIMPLES — retângulo, polígono,
            // um brilho — e não a silhueta orgânica que eu tinha feito aqui (curvas de Bézier que, no
            // tamanho da cena, leram como um tronco de árvore).
            ctx.fillStyle = cfg.madeira;
            ctx.fillRect(-s * .035, -s * .62, s * .07, s * .74);
            ctx.fillStyle = 'rgba(255, 226, 180, .18)';
            ctx.fillRect(-s * .035, -s * .62, s * .022, s * .74);

            // a cabeça: um bloco de madeira, mais larga que a haste e curta
            ctx.fillStyle = cfg.madeira;
            ctx.beginPath();
            ctx.moveTo(-s * .13, -s * .58);
            ctx.lineTo(s * .13, -s * .58);
            ctx.lineTo(s * .15, -s * .86);
            ctx.lineTo(-s * .15, -s * .86);
            ctx.closePath();
            ctx.fill();

            // OS ESPINHOS METÁLICOS, que são o pedido e o que dá destaque: seis cunhas de metal claro
            // saindo da cabeça, três por lado, mais uma no topo. O metal é a única coisa clara da peça,
            // então é ele que se lê primeiro — mesma jogada dos chifres do Oni virarem osso.
            //
            // MENORES que na primeira versão (alcance .3 → .24, espessura .03 → .024, e o do topo de
            // 1.02 → .96): eles estavam disputando com a cabeça em vez de decorá-la. Espinho é detalhe
            // que aponta; grande demais, ele passa a ser a forma da peça.
            ctx.fillStyle = cfg.metal;
            for (const e of [[-1, -.62], [-1, -.72], [-1, -.82], [1, -.62], [1, -.72], [1, -.82]]) {
                ctx.beginPath();
                ctx.moveTo(e[0] * s * .13, s * (e[1] + .024));
                ctx.lineTo(e[0] * s * .24, s * e[1]);
                ctx.lineTo(e[0] * s * .13, s * (e[1] - .024));
                ctx.closePath();
                ctx.fill();
            }
            ctx.beginPath();                                  // o espinho de cima
            ctx.moveTo(-s * .05, -s * .86);
            ctx.lineTo(0, -s * .96);
            ctx.lineTo(s * .05, -s * .86);
            ctx.closePath();
            ctx.fill();

            // o fio de luz no metal: uma lasca clara na borda de cima da cabeça
            ctx.fillStyle = cfg.brilho;
            ctx.fillRect(-s * .15, -s * .87, s * .3, s * .022);

            ctx.restore();
        },
    });
}

/// O REDEMOINHO — a referência do 👺 no chão, e O MAESTRO: a única peça do jogo que ESCREVE num valor
/// que outras leem.
///
/// Ele nasce fora de uma borda, atravessa a clareira e morre na outra. Enquanto atravessa, escreve
/// `vento.forca` (o SINAL é a direção, e o pico é no meio da travessia) e `vento.x` (onde ele está).
/// Daí o fogo verga, a fumaça inclina, as brasas riscam e os corvos se abrem — nenhum deles sabe que
/// existe um redemoinho, todos só leem um número.
///
/// A FORÇA sai de um seno da travessia (0 nas bordas, 1 no meio) em vez de ligar e desligar: um sopro
/// que começa cheio pareceria um interruptor, e o que se quer é a rajada CHEGANDO. Quando ele não está
/// em cena, o vento decai pra zero em vez de zerar de uma vez — pelo mesmo motivo, do outro lado.
///
/// Dentro dele giram grãos de poeira, folhas e CARTAS. As cartas são o 🤡 entrando com causa: o chão da
/// clareira tem cartas caídas (ver `criarFogueira`), e o vento passou por lá. Uma carta voando à esmo
/// pela mata não teria de onde ter vindo — e "por que isto está aqui?" é a pergunta que derrubou o
/// Folclore antigo, com o torii ao lado da roda-gigante.
export function criarRedemoinho(cfg, canvas, vento) {
    // Cada coisa que roda tem a própria altura no cone, o próprio ângulo e a própria velocidade. Em
    // fração da altura do redemoinho, pra tudo escalar junto com ele.
    const roda = (quantos, tipo) => Array.from({ length: quantos }, () => ({
        tipo,
        u: Math.random(),
        ang: Math.random() * Math.PI * 2,
        vel: .7 + Math.random() * .9,
        // As folhas sobem e descem dentro do cone; a poeira fica na altura dela.
        subindo: tipo === 'grão' ? 0 : (.06 + Math.random() * .16) * (Math.random() < .5 ? -1 : 1),
        tamanho: .5 + Math.random() * .7,
        giro: Math.random() * Math.PI * 2,
        vira: 2 + Math.random() * 4,
    }));

    const conteudo = [...roda(cfg.graos, 'grão'), ...roda(cfg.folhas, 'folha')];

    // AS CARTAS, em três estados: `chao` (paradas), `vortice` (girando) e `caindo` (cuspidas, voltando).
    // Todas nascem no CHÃO, espalhadas em volta do centro — é a marca do 🤡 que já estava na cena, e é o
    // que dá ao redemoinho o que absorver. Sem carta parada antes, "levantar cartas" não teria de onde.
    const cartas = Array.from({ length: cfg.cartas.quantas }, () => ({
        estado: 'chao',
        // Concentradas em volta do meio (onde fica a fogueira) e não uniformes na tela: é o sítio que
        // tem baralho derrubado, não a mata inteira.
        x: canvas.width * (.5 + (Math.random() - .5) * .8),
        y: 0, vx: 0, vy: 0,
        giro: Math.random() * Math.PI * 2,
        vira: 2 + Math.random() * 4,
        u: .1 + Math.random() * .8,
        ang: Math.random() * Math.PI * 2,
        vel: .7 + Math.random() * .9,
        subindo: (.06 + Math.random() * .16) * (Math.random() < .5 ? -1 : 1),
        // Até onde ela orbita, em raios do cone. Acima de 1 ela gira POR FORA da poeira, e é daí que sai
        // a leitura de vórtice: um anel de coisas rodando MAIOR que a coluna que as gira.
        orbita: .55 + Math.random() * (cfg.cartas.orbita - .55),
        face: Math.random() < .6,
    }));

    let fase = 'oculto';
    let relogio = entre(cfg.espera) * .5;
    let progresso = 0;
    let sentido = 1;
    let repetiu = 0;                    // quantas passagens seguidas vieram do MESMO lado
    let duracao = 0;
    let t = 0;

    // O LADO é sorteado, mas nunca mais de duas vezes seguidas o mesmo.
    //
    // Sorteio puro é justo e ainda assim ficou ruim: medindo 95 passagens deu 49 pela esquerda contra 46
    // pela direita — mas com uma sequência de SEIS pela esquerda em fila. Com 12 a 24s de espera entre
    // elas, seis é dois minutos e meio de tornado sempre do mesmo lado, e foi exatamente o que o Gabriel
    // viu ("o tornado só vem da esquerda?"). Não era impressão dele nem bug meu: era naipe.
    //
    // O teto de dois conserta sem virar alternância fixa (que seria previsível pelo outro extremo): duas
    // pela esquerda ainda podem acontecer, três não.
    const sortearLado = () => {
        const novo = repetiu >= 2 ? -sentido : (Math.random() < .5 ? 1 : -1);
        repetiu = novo === sentido ? repetiu + 1 : 1;
        sentido = novo;
    };

    return (ctx, dt) => {
        t += dt;
        relogio -= dt;

        const alt = canvas.height * cfg.altura;
        const larg = canvas.height * cfg.largura;
        const base = canvas.height;
        const tamCarta = canvas.height * cfg.cartas.tamanho;
        const passando = fase === 'passando';

        if (!passando) {
            // O vento MORRE devagar depois que ele sai — a poeira e o fogo demoram a assentar. Zerar de
            // uma vez faria a chama voltar à vertical num quadro, e isso lê como corte de vídeo. Mas
            // "devagar" era devagar DEMAIS a 1.4: a 3.6 a coluna volta ao prumo em ~0.3s em vez de ~0.7s.
            vento.forca += (0 - vento.forca) * Math.min(1, dt * 3.6);
            if (relogio <= 0) {
                fase = 'passando';
                progresso = 0;
                sortearLado();
                duracao = entre(cfg.atravessar);
            }
        } else {
            progresso += dt / duracao;
            if (progresso >= 1) { fase = 'oculto'; relogio = entre(cfg.espera); }
        }

        // Geometria da passagem. Fora dela, `x` fica longe da tela pra a conta de "perto de uma carta"
        // dar sempre falso sem precisar de um `if` a mais em cada lugar que a usa.
        const x = fase === 'passando'
            ? (sentido > 0
                ? -larg * 2 + (canvas.width + larg * 4) * progresso
                : canvas.width + larg * 2 - (canvas.width + larg * 4) * progresso)
            : -1e6;

        if (fase === 'passando') {
            // AQUI é onde o maestro fala. Todo o resto do tema é consequência destas duas linhas. O
            // expoente `perfil` estreita o pico: a rajada chega e passa, em vez de ficar cheia metade
            // da travessia.
            vento.forca = sentido * cfg.forca * Math.pow(Math.sin(progresso * Math.PI), cfg.perfil);
            vento.x = x;
        }

        // O raio do cone na altura u. A base é estreita (ele toca o chão num ponto) e o topo abre — é o
        // que faz a forma ser um redemoinho e não um cilindro de poeira.
        //
        // O raio TAMBÉM dança: infla e murcha em alturas diferentes conforme o tempo, então o cone deixa
        // de ser um triângulo perfeito. Sem isto o bamboleio do eixo move uma forma rígida, e o que se lê
        // é um cone sendo arrastado em vez de uma coisa girando fora de eixo.
        const raio = (u) => larg * (.18 + u * 1) * (1 + Math.sin(t * cfg.ritmo2 + u * 4.2) * .16);

        // O EIXO, que é o que faz ele parecer dançar: duas frequências passeando pros lados, mais a
        // inclinação no sentido da marcha. O desvio cresce com a altura (u²) porque o pé está preso no
        // chão — mesmo princípio da coluna de fumaça vergando com o vento.
        const eixo = (u) => sentido * larg * .5 * u * u
            + (Math.sin(t * cfg.ritmo) * .62 + Math.sin(t * cfg.ritmo2 + 1.7) * .38) * larg * cfg.gingado * u * u;

        ctx.save();

        // ---------- as CARTAS NO CHÃO: sempre desenhadas, com ou sem redemoinho em cena.
        //     Achatadas (`scale(1, .4)`) porque estão deitadas no chão e vistas de cima — em pé, uma carta
        //     no chão lê como placa fincada. E são elas que o redemoinho vem buscar.
        for (const c of cartas) {
            if (c.estado !== 'chao') continue;
            ctx.save();
            ctx.translate(c.x, base - tamCarta * .12);
            ctx.scale(1, .4);
            desenharCarta(ctx, 0, 0, tamCarta, c.giro, c.face);
            ctx.restore();

            // ABSORVER: perto da base do redemoinho, ela é sugada. `alcance` é em raios da base, então a
            // boca dele cresce junto com ele — não há um número em px pra desatualizar.
            if (fase === 'passando' && Math.abs(c.x - (x + eixo(0))) < raio(0) * cfg.cartas.alcance) {
                c.estado = 'vortice';
                c.u = .05 + Math.random() * .3;      // entra por baixo, que é por onde ela foi pega
                c.ang = Math.random() * Math.PI * 2;
            }
        }

        // ---------- as CARTAS CAINDO: cuspidas pra fora, voltando ao chão em balística.
        for (const c of cartas) {
            if (c.estado !== 'caindo') continue;
            c.vy += canvas.height * .55 * dt;         // gravidade
            c.vx *= (1 - dt * 1.1);                   // o ar segura o giro lateral: carta não é pedra
            c.x += c.vx * dt;
            c.y += c.vy * dt;
            c.giro += dt * c.vira * .5;

            const chao = base - tamCarta * .12;
            if (c.y >= chao) {
                // Pousou. Ela FICA onde caiu, e é isso que faz o chão nunca ficar igual duas vezes.
                c.estado = 'chao';
                c.y = 0; c.vx = 0; c.vy = 0;
                c.x = Math.max(tamCarta, Math.min(canvas.width - tamCarta, c.x));
                c.face = Math.random() < .6;          // caiu de um lado ou do outro
                continue;
            }
            ctx.save();
            ctx.globalAlpha = .95;
            desenharCarta(ctx, c.x, c.y, tamCarta, c.giro, c.face);
            ctx.restore();
        }

        if (fase !== 'passando') { ctx.restore(); return; }

        // O corpo: um véu de poeira, pra o redemoinho ter MASSA. Só os grãos soltos o fariam parecer um
        // enxame de mosquitos, e a leitura de "coluna de ar girando" viria de nada.
        const veu = ctx.createLinearGradient(x, base, x, base - alt);
        veu.addColorStop(0, `rgba(${cfg.poeira}, .22)`);
        veu.addColorStop(.6, `rgba(${cfg.poeira}, .12)`);
        veu.addColorStop(1, `rgba(${cfg.poeira}, 0)`);
        ctx.fillStyle = veu;
        ctx.beginPath();
        ctx.moveTo(x - raio(0), base);
        for (let i = 1; i <= 10; i++) {
            const u = i / 10;
            ctx.lineTo(x + eixo(u) - raio(u), base - alt * u);
        }
        for (let i = 10; i >= 0; i--) {
            const u = i / 10;
            ctx.lineTo(x + eixo(u) + raio(u), base - alt * u);
        }
        ctx.closePath();
        ctx.fill();

        // ---------- a poeira e as folhas girando dentro dele
        for (const c of conteudo) {
            // Sobe e desce dentro do cone, quicando nas pontas em vez de reaparecer do outro lado —
            // reaparecer faria a folha piscar de baixo pra cima na cara do jogador.
            if (c.subindo) {
                c.u += c.subindo * dt;
                if (c.u > 1 || c.u < .04) { c.subindo *= -1; c.u = Math.max(.04, Math.min(1, c.u)); }
            }
            c.ang += dt * cfg.giro * c.vel;
            c.giro += dt * c.vira;

            const r = raio(c.u);
            const cxx = x + eixo(c.u) + Math.cos(c.ang) * r;
            const cyy = base - alt * c.u;
            // Achatado: o que está na frente do eixo fica um pouco mais baixo que o que está atrás. É o
            // que dá a volta ao giro sem projeção 3D nenhuma.
            const profundidade = Math.sin(c.ang);
            const y = cyy + profundidade * r * .18;
            // Quem está atrás é mais apagado — a poeira do véu está entre ele e a gente.
            const alfa = .45 + (profundidade + 1) * .27;

            if (c.tipo === 'grão') {
                ctx.fillStyle = `rgba(${cfg.poeira}, ${alfa * .7})`;
                ctx.beginPath();
                ctx.arc(cxx, y, larg * .035 * c.tamanho, 0, Math.PI * 2);
                ctx.fill();
            } else {
                ctx.save();
                ctx.globalAlpha = alfa;
                ctx.translate(cxx, y);
                ctx.rotate(c.giro);
                ctx.fillStyle = cfg.folha;
                ctx.beginPath();
                ctx.ellipse(0, 0, larg * .12 * c.tamanho, larg * .05 * c.tamanho, 0, 0, Math.PI * 2);
                ctx.fill();
                ctx.restore();
            }
        }

        // ---------- as CARTAS NO VÓRTICE: mesmo giro, mas com órbita PRÓPRIA (algumas por fora da
        //     poeira) e TAMANHO FIXO — em fração da tela, não do redemoinho. Com ele grande, carta
        //     proporcional a ele viraria um outdoor girando.
        for (const c of cartas) {
            if (c.estado !== 'vortice') continue;

            c.u += c.subindo * dt;
            if (c.u > 1 || c.u < .04) { c.subindo *= -1; c.u = Math.max(.04, Math.min(1, c.u)); }
            c.ang += dt * cfg.giro * c.vel;
            c.giro += dt * c.vira;

            const r = raio(c.u) * c.orbita;
            const profundidade = Math.sin(c.ang);
            const cxx = x + eixo(c.u) + Math.cos(c.ang) * r;
            const cyy = base - alt * c.u + profundidade * r * .18;

            ctx.save();
            ctx.globalAlpha = .5 + (profundidade + 1) * .25;
            // A carta mostra a FACE ou o DORSO conforme o lado em que está do giro — é a troca no meio da
            // volta que a faz parecer virando, e não deslizando de lado.
            desenharCarta(ctx, cxx, cyy, tamCarta, c.giro, profundidade > 0);
            ctx.restore();

            // CUSPIR: sorteio por segundo, e ela sai na tangente do giro (pra fora e pra cima). Sair na
            // direção em que já estava girando é o que faz parecer arremesso em vez de teleporte.
            if (Math.random() < cfg.cartas.soltar * dt) {
                c.estado = 'caindo';
                c.x = cxx;
                c.y = cyy;
                c.vx = Math.cos(c.ang) * larg * 1.8 + sentido * larg * .6;
                c.vy = -canvas.height * (.1 + Math.random() * .18);
            }
        }

        ctx.restore();
    };
}
