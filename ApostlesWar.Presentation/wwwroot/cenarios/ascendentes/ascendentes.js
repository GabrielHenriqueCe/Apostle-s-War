import { caixaRedonda, desenharChama, entre } from '../comum/basicos.js';
import { medirDoTema } from '../comum/ladrilho.js';
import { comListras } from '../comum/basicos.js';
import { criarNoHorizonte } from '../comum/ladrilho.js';
// ❄️ ASCENDENTES — a SALA DE NATAL. O oitavo e último capítulo, e o segundo INTERIOR do front (ver
// o bloco no estilo.css).
//
// Duas peles anteriores morreram vendo em jogo, e as duas ensinaram: a CATEDRAL com a rosácea e o
// sino (bonita no papel, e prédio no meio da neve lê como arquitetura, não como Natal) e a versão
// EXTERNA com a árvore no meio da nevasca ("ficou vazio"). O que sobreviveu das duas foi a árvore
// e a iluminação colorida — e a saída foi o Gabriel virar a cena do avesso: em vez de pôr a casa
// de Natal no meio da neve, pôr a neve do lado de fora de uma JANELA.
//
// A cena tem UM ROTEIRO, e é ele que a segura de pé. Cada peça sozinha é um enfeite; encadeadas,
// uma explica a outra:
//
//   a estrela BRILHA → o trenó cruza a janela da esquerda pra direita → o fogo da lareira APAGA
//   → ele desce de bunda pela chaminé (só da barriga pra baixo, entalado) → o presente desce
//   flutuando e vai pra debaixo da árvore, EMPURRANDO outro pra fora da imagem → ele sobe
//   → passa de volta pela janela, da direita pra esquerda → o fogo volta → repete
//
// É o mesmo princípio do ciclo do Inferno dos 🔱 Decaídos, com uma diferença que muda tudo: lá as
// fases eram de UMA peça (a fenda), e aqui elas atravessam QUATRO que estão em cantos diferentes da
// tela. Por isso o roteiro virou um maestro (o `natal`) em vez de uma máquina de estados dentro de
// um builder — quem escreve é um só, e a árvore, a janela, a lareira e os presentes só LEEM.
//
// Os apóstolos: ⛄ os bonecos de neve, vistos pela janela; 🎅 o dono da noite inteira; 🎭 as máscaras
// penduradas na árvore. O 😇 Anjo ficou SEM representação nesta versão — a coluna de luz que era
// dele não cabe num interior, e a lista de peças da cena é do Gabriel.
export const ar = {
    // O ROTEIRO. Só números: quanto dura cada passo, em segundos. Ele é curto de propósito — o
    // ciclo inteiro dá uns 17s, porque o pedido foi "tempo curto entre animações".
    //
    // `entregar` é o ÚNICO que não é um passo: o presente tem relógio próprio e atravessa a
    // fronteira entre dois passos (ele começa a descer quando o Papai Noel senta na lareira e
    // ainda está viajando quando ele começa a subir). Era isso ou picar a viagem em pedaços que
    // teriam de casar com os passos — e casar duas durações é o jeito conhecido de elas
    // divergirem.
    // `descer` é bem mais curto que `subir`: cair é rápido e desentalar é trabalhoso, e essa
    // assimetria é metade da piada. Junto com a aceleração da queda (o `pernas²` na lareira), é o
    // que faz o tombo LER como tombo em vez de descida de elevador.
    // `passar` e `voltar` são os passos LONGOS do ciclo, e é de propósito: a travessia é a única
    // coisa da noite que acontece longe, atrás do vidro, e o que está longe tem de demorar pra
    // registrar. Curtos, os quatro bichos cruzavam a janela antes de alguém olhar pra lá — e a
    // metade da cena que mora do lado de fora simplesmente não era vista.
    // A ORDEM da noite, e cada número é a duração de um passo em segundos:
    //   brilhar → passar → apagar → TREMER → descer → entalado (joga o presente e ESPERA ele
    //   chegar) → SACUDIR → subir → voltar → repouso
    //
    // `entalado` tem 0.6, e mesmo assim dura uns três segundos: ele é o único passo que também
    // espera a entrega terminar (ver o `preso` no roteiro). O número aqui é só o mínimo.
    //
    // `subir` não aparece: ele usa a duração de `descer`, porque o percurso é o mesmo e ele sobe
    // na mesma velocidade com que caiu.
    roteiro: {
        brilhar: 1.6, passar: 5.6, apagar: 1.1,
        tremer: .7, descer: .9, entalado: .6, sacudir: .7,
        voltar: 5.6, repouso: 2.2, entregar: 2.9,
    },
    // A JANELA e o que se vê por ela. É UM builder porque é uma composição só: o céu, os
    // pinheiros, a neve, os bonecos e o trenó são todos RECORTADOS pelo mesmo retângulo, e quem
    // sabe onde esse retângulo está é a janela.
    //
    // Tudo aqui é medido em fração da JANELA, e não da tela. É o que faz a paisagem ser um
    // quadro: mudar o tamanho da janela leva a neve, os pinheiros e os bonecos junto, sem que
    // nenhum número precise ser reajustado.
    janela: {
        x: .5, y: .42, largura: .32, altura: .46,
        colunas: 2, linhas: 1,
        // O CÉU é o mesmo gradiente de seis paradas que era do CSS na pele externa: escuro no alto
        // e clareando pra baixo, sem estrela e sem lua. Ele ficou porque foi a única coisa que o
        // Gabriel aprovou nas três rodadas — só passou a ser visto por um buraco na parede.
        ceu: ['#0b1020', '#141c33', '#222c49', '#38416a', '#55608c', '#6d789f'],
        clarao: '246, 204, 132',
        neve: '#a9b7d4', neveSombra: '#8b9cbb',
        pinheiro: '#131a2c', pinheiroLonge: '#2a3350', neveNoPinheiro: '#808fb0',
        // O ladrilho da mata, na altura da ARENA e não da janela — é o que mantém a mata do mesmo
        // tamanho que ela tinha na pele externa (193px numa arena de 750). `ladrilhoChao` é onde
        // os pés das árvores encostam, em fração da altura do ladrilho, e saiu do `--neve-chao`
        // que morreu com o CSS antigo.
        ladrilhoAltura: .257, ladrilhoChao: .44,
        floco: '226, 238, 255', flocos: 34,
        // Onde ele cruza (fração da altura da janela) e quanto mede (fração da ARENA — o mesmo
        // tamanho de antes; ver o comentário no `criarVistaDaJanela`).
        vooAltura: .26, vooTamanho: .075, vooOndas: 1.2, vooBalanco: .035,
        bonecos: {
            neve: '#e8effc', neveSombra: '#9caed0', carvao: '#0f1522', cenoura: '#d97a2b',
            galho: '#4c3b2f', cachecol: '#8e2f34', sombra: '8, 14, 32',
            // fração da ARENA, o mesmo .052 da pele externa
            tamanho: .052,
            pontos: [{ x: .22, fundura: .32 }, { x: .56, fundura: .72 }, { x: .82, fundura: .46 }],
        },
        treno: {
            corpo: '#0a0f1c', brilho: '246, 214, 160',
            presente: '#a82f38', presenteAlt: '#2f6f52', fita: '#e8d49a',
            pisca: ['255, 106, 90', '132, 214, 255', '255, 206, 110', '150, 240, 170'],
            nariz: '255, 74, 58',
            renas: 3, separacao: 1.2, piscas: 5, piscaRitmo: [1.4, 3.2],
        },
    },
    // A MOLDURA da janela e as CORTINAS, amarradas nos cantos. Elas são canvas e não CSS pelo
    // mesmo motivo das palmeiras dos Místicos: precisam ficar exatamente em cima do vidro que o
    // canvas acabou de recortar, e duas contas do mesmo retângulo divergiriam.
    cortinas: {
        moldura: '#6b4a33', molduraLuz: '#8a6242', molduraSombra: '#3d2819',
        varao: '#4a3222', ponteira: '#c9a227',
        pano: '#8c2434', panoLuz: '#b8404e', panoSombra: '#54101e', laco: '#d9b45b',
        // `larguraTopo` é o quanto de vidro cada cortina cobre lá em cima, e `amarra` é a altura
        // do laço em fração da janela. Amarrada no meio, ela fica com a cintura no lugar em que o
        // olho espera — e é a AMARRA que faz o pano ler como cortina em vez de faixa vertical.
        larguraTopo: .3, amarra: .46, aperto: .34, folgas: 3,
    },
    // A LAREIRA do canto direito, com o fogo que APAGA quando o Papai Noel chega. A chama é a
    // mesma da ruína dos ⚙️ Tecnológicos e dos focos da vila élfica — terceiro cliente do
    // `desenharChama`, sem uma linha nova.
    lareira: {
        pedra: '#544039', pedraLuz: '#6d5449', pedraSombra: '#332420', argamassa: '#6f594e',
        cornija: '#6b4a33', cornijaLuz: '#8a6242', dentro: '#150e0c', lenha: '#3a2519',
        fogo: '255, 168, 66', brasa: '255, 242, 208', labaredas: 3,
        // Em fração da tela. `boca` é o vão, em fração da própria lareira.
        x: .83, largura: .22, altura: .46, boca: .62, bocaAltura: .5,
        // O quanto ela sacode no aviso, em fração da altura da boca. Pouquíssimo de propósito: o
        // que assusta é o chão avisar, não a parede pular.
        tremorAmp: .035,
        // 🎭 As máscaras acima da chaminé. `tamanho` e `afasta` são frações da LARGURA da lareira,
        // pra elas escalarem com a peça em que estão penduradas em vez de com a tela.
        mascaras: { azul: '#3f74d8', amarela: '#e3b33c', traco: '#12141c', tamanho: .15, afasta: .34, giro: .2 },
        // 🎅 ENTALADO: só da barriga pra baixo, que é a parte que cabe no vão. Estilo chapado, sem
        // sombreado — o mesmo do 🦸 e do 🦹 sentados no banheiro do ⭐ Especial.
        // `pelo` é o debrum branco, e ele não é enfeite: é o que separa casaco de calça, calça de
        // bota e o meio do casaco das bordas. Sem ele sobra um bloco vermelho com uma faixa preta.
        terno: '#a8202c', pelo: '#f4f6fa', bota: '#241a16', fivela: '#d9b45b',
        poeira: '208, 196, 186', poeiraDura: .9, mexe: .04,
    },
    // 🎄 A ÁRVORE, agora no canto ESQUERDO. É a mesma da rodada anterior — a única peça que o
    // Gabriel aprovou de primeira — com uma diferença: a estrela NÃO brilha o tempo todo. Ela
    // acende antes de o Papai Noel aparecer e apaga depois que ele passa, e é isso que a
    // transforma de enfeite em AVISO. Luz constante não anuncia nada.
    arvoreDeNatal: {
        copa: '#0d2a1e', copaLuz: '#17402c', copaSombra: '#071710', tronco: '#2a1b12',
        neve: '#d6e2f6',
        luzes: ['255, 106, 90', '132, 214, 255', '255, 206, 110'],
        bola: ['#c0392f', '#c9a227', '#2f6f52', '#8e5bb5'],
        estrela: '255, 232, 160', estrelaNucleo: '255, 253, 240',
        // A árvore e a lareira andam pros CANTOS e a janela fica com o meio inteiro: é o que faz
        // as três peças serem três coisas em vez de uma fileira. A largura é .24 e não mais: a
        // ramada de baixo abre pros dois lados, e mais larga ela encavalaria na moldura.
        x: .19, largura: .24, altura: .58,
        ramadas: 6, sacada: .035, afina: .8, tronco0: .07,
        luzesPorRamada: 9, luzRaio: .0045, luzPisca: [.7, 2.2], luzClarao: 5,
        bolas: 7, bolaRaio: .011,
        // 😇 Os ANJINHOS pendurados, balançando cada um no seu compasso. MAIORES que as bolas de
        // propósito: é o que faz o olho parar neles em vez de lê-los como mais um enfeite redondo.
        // (Máscaras de teatro moraram aqui, e eram o 🎭 Mímico — saíram por pedido do Gabriel, e
        // com elas o Mímico ficou sem representação nesta pele.)
        anjos: [{ ramada: 2, u: -.62, tamanho: .042 }, { ramada: 3, u: .58, tamanho: .037 }],
        anjoPano: '#f2ead6', anjoAsa: '#ffffff', anjoPele: '#e8b48c',
        anjoAureola: '#e8c96a', anjoFio: '#8a7a5c', anjoBalanco: [.9, 1.5],
        // `repouso` é o quanto a estrela fica acesa QUANDO NÃO está anunciando ninguém. Não é
        // zero: apagada de todo ela viraria um enfeite escuro no topo, e o que se quer é que ela
        // ACENDA, não que ela ligue.
        estrelaTamanho: .055, estrelaHalo: 7.5, raios: 4, repouso: .16, pulso: .12, ritmo: .9,
        clarao: .46, pocas: .13, alcance: .5,
    },
    // Os PRESENTES: a pilha debaixo da árvore e o que desce flutuando da lareira.
    //
    // Eles são de UM dono só porque a chegada MEXE na pilha (o novo entra e empurra a fila até o
    // último sair de cena). Com a árvore desenhando a pilha e outra camada trazendo o de cima,
    // duas peças seriam donas do mesmo estado — que é exatamente o que o maestro existe pra
    // evitar, e não dá pra resolver com um campo.
    presentes: {
        cor: ['#a82f38', '#2f6f52', '#3a5aa8', '#c9a227'], fita: '#e8d49a',
        tamanho: .05, passo: .052, comeca: 3, teto: 4, saida: -.06, empurrar: 3.4,
        // A viagem: ele sai da boca da lareira, SOBE um pouco (foi largado, não jogado) e desce
        // até o pé da árvore. `arco` é a altura da barriga do caminho, em fração da tela.
        arco: .16, giro: 2.2,
    },
};

/// Monta a cena deste capítulo. A ORDEM É A PROFUNDIDADE — o que vem antes fica atrás.
///
/// O núcleo (`iniciarAr`) não sabe que este tema existe: ele chama `montar` e recebe as camadas
/// prontas. Era o contrário até ago/2026, quando UMA lista no núcleo servia os 8 temas e cada
/// item vinha guardado por `config.X &&` — os guardas eram o preço de a lista não ser de ninguém.
export function montar({ fundo, frente, maestro }) {
    // O QUINTO dado compartilhado, e o primeiro que é um ROTEIRO: a noite de Natal dos ❄️ Ascendentes.
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

    return {
        noFundo: [
            // ❄️ Os Ascendentes, na ordem em que a SALA é vista, de fora pra dentro. O ROTEIRO vem primeiro
            // e não desenha NADA — ele só escreve o maestro, e vindo antes garante que as quatro peças
            // leiam o mesmo instante da história no mesmo quadro. Depois a VISTA (o que está do outro lado
            // do vidro, recortada nele), a MOLDURA com as cortinas por cima dela, e então o que está
            // dentro da sala: a LAREIRA, a ÁRVORE e os PRESENTES no chão, que são a coisa mais perto.
            //
            // As cortinas recebem a config da janela, e os presentes as da árvore e da lareira, pelo mesmo
            // motivo do ninja com o castelo e dos sentados com o banheiro: quem sabe onde uma peça está e
            // quanto ela mede é ela mesma, e a segunda cópia de uma medida diverge no meio da cena.
            criarRoteiroDaNoite(ar.roteiro, fundo, natal),
            criarVistaDaJanela(ar.janela, fundo, natal),
            criarCortinas(ar.cortinas, fundo, ar.janela),
            criarLareira(ar.lareira, fundo, natal),
            criarArvoreDeNatal(ar.arvoreDeNatal, fundo, natal),
            criarPresentes(ar.presentes, fundo, natal, ar.arvoreDeNatal, ar.lareira),
        ].filter(Boolean),
        naFrente: [
        ].filter(Boolean),
    };
}
/// Onde é o CHÃO da sala dos ❄️ Ascendentes, em pixel. Mede de vez em quando e não a cada quadro, pelo
/// mesmo motivo do `criarNoHorizonte`: `getComputedStyle` força layout, e o que muda entre duas
/// medidas é a janela ter cruzado uma faixa do @media — isso não acontece no meio de um gesto.
///
/// A linha mora no CSS (`--sala-chao`) porque é lá que o piso é pintado. Uma segunda cópia dela aqui
/// e o tapete ficaria num lugar e a árvore noutro — é a lição do `--mata-passo` com as corujas.
export function medidorDoChaoDaSala(canvas) {
    let linha = 0, conferir = 0;
    return (dt) => {
        conferir -= dt;
        if (conferir > 0) return linha;
        conferir = 1;
        linha = canvas.height * (medirDoTema('--sala-chao', 74) / 100);
        return linha;
    };
}

/// O ROTEIRO DA NOITE — o maestro dos ❄️ Ascendentes, e a única camada do front que NÃO DESENHA NADA.
///
/// Isso é de propósito. As quatro peças da cena (a árvore num canto, a janela no meio, a lareira no
/// outro canto e os presentes no chão) contam UMA história, e nenhuma delas é o lugar certo pra
/// guardá-la: a estrela brilhar é assunto da árvore, o fogo apagar é da lareira, e o que amarra as
/// duas não é nenhuma das duas. O ciclo do Inferno dos 🔱 Decaídos coube dentro da fenda porque as
/// fases eram todas DELA; aqui elas atravessam a tela inteira.
///
/// Então o roteiro escreve, e os outros só leem — ninguém pergunta nada a ninguém, e uma peça que
/// ignore o maestro continua correta. O fluxo:
///
///   brilhar → passar (E→D) → apagar → descer → entalado → subir → voltar (D→E) → repouso → repete
///
/// `entregar` é o único relógio que NÃO é um passo: o presente começa a descer quando ele senta na
/// lareira e ainda está viajando quando ele começa a subir. Picar a viagem em pedaços que casassem
/// com os passos seria pôr duas durações pra concordar, que é o jeito conhecido de elas divergirem.
///
/// E o aviso de entrega é um CONTADOR e não um flag, pela razão de sempre: quem espera guarda o
/// último valor que viu, e um flag precisaria de alguém pra desligá-lo — dois donos do mesmo estado.
export const PASSOS_DA_NOITE = [
    'brilhar', 'passar', 'apagar', 'tremer', 'descer', 'entalado', 'sacudir', 'subir', 'voltar', 'repouso',
];

/// Quanto dura cada passo. `subir` NÃO tem número próprio: ele usa o de `descer`, porque o percurso é
/// o mesmo e o pedido era que ele subisse na mesma velocidade com que caiu. Dois números iguais numa
/// config são dois números que um dia vão ficar diferentes sem ninguém perceber.
export const duracaoDoPasso = (cfg, passo) => (passo === 'subir' ? cfg.descer : cfg[passo]);

export function criarRoteiroDaNoite(cfg, canvas, natal) {
    let i = 0;
    let resta = duracaoDoPasso(cfg, PASSOS_DA_NOITE[0]);
    let entrega = -1;

    return (ctx, dt) => {
        resta -= dt;

        // ELE SÓ SOBE DEPOIS QUE O PRESENTE CHEGA. O `entalado` é o único passo que não termina só
        // pelo relógio: ele também espera a entrega acabar. Sem isso o Papai Noel subia pela chaminé
        // com o presente ainda flutuando no meio da sala, e a cena contava a história fora de ordem.
        //
        // E é isto, e não "põe o `entalado` maior que o `entregar`", porque assim não há dois números
        // tendo de concordar: mexer na duração da viagem do presente não pode exigir que alguém se
        // lembre de mexer na do entalado também. A dependência fica DITA, em vez de calibrada.
        const preso = PASSOS_DA_NOITE[i] === 'entalado' && entrega >= 0;

        if (resta <= 0 && !preso) {
            const acabou = PASSOS_DA_NOITE[i];
            i = (i + 1) % PASSOS_DA_NOITE.length;
            resta = duracaoDoPasso(cfg, PASSOS_DA_NOITE[i]);
            // ele cai e SÓ ENTÃO joga o presente: a entrega começa quando a queda acaba
            if (acabou === 'descer') entrega = 0;
        }

        const passo = PASSOS_DA_NOITE[i];
        const q = Math.max(0, Math.min(1, 1 - resta / duracaoDoPasso(cfg, passo)));

        natal.passo = passo;

        // A ESTRELA acende ANTES de ele aparecer e apaga depois que ele passa. É o que a transforma de
        // enfeite em aviso: luz constante não anuncia nada.
        natal.brilho = passo === 'brilhar' ? q
            : passo === 'passar' ? 1
                : passo === 'apagar' ? 1 - q
                    : 0;

        // O FOGO cai quando ele chega e volta enquanto ele vai embora.
        natal.fogo = passo === 'apagar' ? 1 - q
            : (passo === 'tremer' || passo === 'descer' || passo === 'entalado' || passo === 'sacudir') ? 0
                : passo === 'subir' ? q * .35
                    : passo === 'voltar' ? Math.min(1, .35 + q * 1.3)
                        : 1;

        // O VOO atrás da janela. `voo` é 0 quando ele não está em cena — e não um booleano à parte,
        // porque quem lê precisa do PROGRESSO, e um segundo campo dizendo a mesma coisa poderia
        // discordar dele.
        natal.voo = (passo === 'passar' || passo === 'voltar') ? q : 0;
        natal.sentido = passo === 'voltar' ? -1 : 1;

        // As PERNAS pra fora da lareira, e o esperneio de quem está entalado.
        natal.pernas = passo === 'descer' ? q
            : (passo === 'entalado' || passo === 'sacudir') ? 1
                : passo === 'subir' ? 1 - q
                    : 0;
        natal.mexe = (passo === 'entalado' || passo === 'sacudir') ? 1 : 0;

        // O AVISO é um PASSO, e não a fração final de outro. Ele acontece duas vezes: `tremer` antes de
        // ele cair e `sacudir` antes de ele subir.
        //
        // A primeira versão amarrava o tremor ao fim de `apagar` e ao fim de `entalado`, e isso ficou
        // ridículo assim que o `entalado` passou a ESPERAR o presente chegar: com o passo esticando, o
        // "fim dele" durava o tempo todo da espera, e a lareira tremia sem parar por três segundos.
        // Aviso é um acontecimento curto; grudá-lo na cauda de um passo de duração variável é pedir
        // que ele dure o que o outro durar.
        //
        // A envoltória é um seno: nasce em zero, enche no meio e volta a zero. Ligar e desligar num
        // degrau daria um estalo — tremida começa e termina, não é uma chave.
        natal.tremor = (passo === 'tremer' || passo === 'sacudir') ? Math.sin(q * Math.PI) : 0;

        if (entrega >= 0) {
            entrega += dt / cfg.entregar;
            if (entrega >= 1) { entrega = -1; natal.entregues++; }
        }
        natal.entrega = entrega;
    };
}

/// O retângulo do vidro, em pixel. UMA conta, três clientes (a vista que se recorta nele, a moldura
/// que o emoldura e as cortinas que penduram nele). Duas cópias divergiriam no meio da cena, que é a
/// lição do banheiro publicando as bordas da porta em pixel pronto em vez da fração.
/// As oito árvores do ladrilho que era do CSS na pele externa, em fração do próprio ladrilho (320×230,
/// com o chão em y=101). Elas viraram dado porque a mata agora é CANVAS — ela precisa ser recortada
/// pelo vidro, e ladrilho de CSS não sabe o que é uma janela.
///
/// Os números saíram do SVG antigo, e ficam aqui pra a mata da janela ser exatamente a mesma mata: o
/// pedido era "o fundo que tinha antes", e redesenhar de cabeça daria uma mata PARECIDA, que é o jeito
/// de duas versões da mesma coisa divergirem.
export const PINHEIROS_DO_LADRILHO = [
    { x: .113, alto: .235, largo: .041, longe: true },
    { x: .325, alto: .209, largo: .038, longe: true },
    { x: .563, alto: .252, largo: .044, longe: true },
    { x: .800, alto: .217, largo: .038, longe: true },
    { x: .213, alto: .357, largo: .053, longe: false },
    { x: .444, alto: .400, largo: .059, longe: false },
    { x: .681, alto: .330, largo: .050, longe: false },
    { x: .913, alto: .304, largo: .047, longe: false },
];

/// Uma conífera de três andares, com a coroa de neve. É o desenho do ladrilho antigo virado função: a
/// silhueta é a mesma, e o que ela ganhou foi poder ser desenhada em qualquer escala.
export function desenharPinheiroNevado(ctx, x, chao, alt, larg, cor, neve) {
    ctx.fillStyle = cor;
    ctx.beginPath();
    ctx.moveTo(x, chao - alt);
    ctx.lineTo(x + larg * .55, chao - alt * .58);
    ctx.lineTo(x + larg * .30, chao - alt * .58);
    ctx.lineTo(x + larg * .85, chao - alt * .28);
    ctx.lineTo(x + larg * .50, chao - alt * .28);
    ctx.lineTo(x + larg, chao);
    ctx.lineTo(x - larg, chao);
    ctx.lineTo(x - larg * .50, chao - alt * .28);
    ctx.lineTo(x - larg * .85, chao - alt * .28);
    ctx.lineTo(x - larg * .30, chao - alt * .58);
    ctx.lineTo(x - larg * .55, chao - alt * .58);
    ctx.closePath();
    ctx.fill();

    // a coroa: sem ela é um pinheiro qualquer, com ela é um pinheiro no inverno
    ctx.fillStyle = neve;
    ctx.beginPath();
    ctx.moveTo(x, chao - alt);
    ctx.lineTo(x + larg * .3, chao - alt * .78);
    ctx.lineTo(x - larg * .3, chao - alt * .78);
    ctx.closePath();
    ctx.fill();
}

export const caixaDaJanela = (cfg, canvas) => {
    const l = canvas.width * cfg.largura;
    const a = canvas.height * cfg.altura;
    return { x: canvas.width * cfg.x - l / 2, y: canvas.height * cfg.y - a / 2, l, a };
};

/// A VISTA PELA JANELA — o céu, os pinheiros, a neve, os ⛄ e o 🎅 cruzando lá fora.
///
/// É um builder só porque é uma composição só, e porque todos se recortam no MESMO retângulo. O
/// recorte é o que dispensa a parede de ter um buraco: em vez de pintar a sala por cima da paisagem e
/// torcer pra sobrar exatamente o vão, a paisagem já nasce cortada — é mais barato e mais certo que
/// acertar a borda à mão, que foi o que o `comListras` do ⭐ Especial ensinou.
///
/// Tudo aqui é medido em fração da JANELA e nunca da tela, e é isso que faz a paisagem ser um quadro:
/// mudar o tamanho do vidro leva a neve, os pinheiros e os bonecos junto, sem reajustar um número.
export function criarVistaDaJanela(cfg, canvas, natal) {
    const flocos = Array.from({ length: cfg.flocos }, () => ({
        u: Math.random(), v: Math.random(),
        vy: .05 + Math.random() * .1, deriva: (Math.random() - .5) * .04,
        r: .004 + Math.random() * .007, alfa: .3 + Math.random() * .5, fase: Math.random() * 6,
    }));
    const piscas = Array.from({ length: cfg.treno.piscas }, (_, i) => ({
        cor: cfg.treno.pisca[i % cfg.treno.pisca.length],
        ritmo: entre(cfg.treno.piscaRitmo), fase: Math.random() * 6,
    }));
    let t = 0;
    let anterior = null;

    return (ctx, dt) => {
        t += dt;
        const j = caixaDaJanela(cfg, canvas);
        const h = canvas.height;

        // A PAISAGEM É UM RECORTE, NÃO UMA MINIATURA, e essa é a decisão que manda em tudo aqui.
        //
        // A primeira versão media a mata, a neve, os bonecos e o trenó em fração da JANELA: proporção
        // impecável e leitura errada, porque encolher a cena inteira faz o vidro parecer um quadro
        // pendurado na parede em vez de um buraco. Olhar por uma janela não diminui o mundo — mostra
        // MENOS dele. Então tudo aqui é medido na altura da ARENA, exatamente como era na pele
        // externa, e o que o vidro faz é cortar. É por isso que o trenó e os bonecos têm o mesmo
        // tamanho de antes: eles não foram reajustados, eles nunca mudaram.
        const ladrilhoA = h * cfg.ladrilhoAltura;
        const ladrilhoL = ladrilhoA * (320 / 230);      // a proporção do desenho antigo, preservada
        const chao = j.y + j.a - ladrilhoA * (1 - cfg.ladrilhoChao);

        ctx.save();
        ctx.beginPath();
        ctx.rect(j.x, j.y, j.l, j.a);
        ctx.clip();

        // O CÉU, com as mesmas seis paradas do gradiente que era do CSS na versão externa: escuro no
        // alto e clareando pra baixo, que é o que a nevasca à noite faz.
        const ceu = ctx.createLinearGradient(0, j.y, 0, j.y + j.a);
        for (let i = 0; i < cfg.ceu.length; i++) ceu.addColorStop(i / (cfg.ceu.length - 1), cfg.ceu[i]);
        ctx.fillStyle = ceu;
        ctx.fillRect(j.x, j.y, j.l, j.a);

        // o clarão baixo no horizonte, que é o que recorta a mata contra a luz
        const brilho = ctx.createRadialGradient(j.x + j.l * .5, chao, 0, j.x + j.l * .5, chao, j.l * .5);
        brilho.addColorStop(0, `rgba(${cfg.clarao}, .26)`);
        brilho.addColorStop(1, `rgba(${cfg.clarao}, 0)`);
        ctx.fillStyle = brilho;
        ctx.fillRect(j.x, j.y, j.l, j.a);

        // A MATA, repetida a partir da borda esquerda do vidro. São as mesmas oito árvores do ladrilho
        // antigo, nas mesmas posições e nas mesmas duas distâncias — a de trás mais clara e menor,
        // porque o que está longe tem a cor do AR que está no caminho, e não a cor dele.
        for (let tile = -1; j.x + tile * ladrilhoL < j.x + j.l; tile++) {
            const x0 = j.x + tile * ladrilhoL;
            for (const p of PINHEIROS_DO_LADRILHO) {
                desenharPinheiroNevado(ctx, x0 + p.x * ladrilhoL, chao,
                    p.alto * ladrilhoA, p.largo * ladrilhoL,
                    p.longe ? cfg.pinheiroLonge : cfg.pinheiro, cfg.neveNoPinheiro);
            }
        }

        // a faixa de NEVE do chão
        const solo = ctx.createLinearGradient(0, chao, 0, j.y + j.a);
        solo.addColorStop(0, cfg.neve);
        solo.addColorStop(1, cfg.neveSombra);
        ctx.fillStyle = solo;
        ctx.beginPath();
        ctx.moveTo(j.x, chao);
        for (let k = 0; k <= 4; k++) {
            ctx.quadraticCurveTo(j.x + j.l * (k + .5) / 4, chao + (k % 2 ? 5 : -5),
                j.x + j.l * (k + 1) / 4, chao);
        }
        ctx.lineTo(j.x + j.l, j.y + j.a);
        ctx.lineTo(j.x, j.y + j.a);
        ctx.closePath();
        ctx.fill();

        const faixa = j.y + j.a - chao;
        for (const b of cfg.bonecos.pontos) {
            desenharBonecoDeNeve(ctx, j.x + b.x * j.l, chao + faixa * b.fundura,
                h * cfg.bonecos.tamanho * (.74 + .5 * b.fundura), cfg.bonecos);
        }

        // 🎅 A PASSAGEM. Ele entra e sai FORA do vidro nas duas pontas, e é o recorte que o faz surgir
        // da borda em vez de aparecer do nada no meio do quadro.
        if (natal.voo > 0) {
            // Na altura da ARENA, como os bonecos e pelo mesmo motivo: ele é do mesmo tamanho que era
            // na pele externa, e o vidro só decide o quanto dele se vê por vez.
            const s = h * cfg.vooTamanho;
            const margem = s * 3.2;
            const de = natal.sentido > 0 ? j.x - margem : j.x + j.l + margem;
            const ate = natal.sentido > 0 ? j.x + j.l + margem : j.x - margem;
            const x = de + (ate - de) * natal.voo;
            const y = j.y + j.a * cfg.vooAltura
                + Math.sin(natal.voo * Math.PI * cfg.vooOndas) * h * cfg.vooBalanco;
            // A inclinação sai da ROTA (o ângulo entre este ponto e o do quadro passado), e não de um
            // segundo número — assim ele nunca aponta pra um lado e anda pro outro.
            const ang = anterior ? Math.atan2(y - anterior.y, x - anterior.x) : (natal.sentido > 0 ? 0 : Math.PI);
            anterior = { x, y };
            desenharTreno(ctx, x, y, s, ang, t * 7, piscas, cfg.treno);
        } else {
            anterior = null;
        }

        for (const f of flocos) {
            f.v += f.vy * dt;
            f.fase += dt;
            if (f.v > 1) { f.v -= 1; f.u = Math.random(); }
            ctx.beginPath();
            ctx.arc(j.x + (f.u + Math.sin(f.fase) * f.deriva) * j.l, j.y + f.v * j.a, f.r * j.a, 0, Math.PI * 2);
            ctx.fillStyle = `rgba(${cfg.floco}, ${f.alfa})`;
            ctx.fill();
        }

        ctx.restore();
    };
}

/// A MOLDURA da janela e as CORTINAS amarradas nos cantos.
///
/// Elas são canvas e não `::before`/`::after` do CSS pelo mesmo motivo das palmeiras dos 🐉 Místicos:
/// precisam ficar exatamente em cima do vidro que o canvas acabou de recortar, e duas contas do mesmo
/// retângulo divergiriam. Aqui o vão vem pronto do `caixaDaJanela`, que é a fonte única.
///
/// O que faz o pano ler como CORTINA e não como faixa vertical é a AMARRA: a cintura apertada no meio
/// com o laço. Sem ela sobram dois retângulos vermelhos nas bordas do vidro.
export function criarCortinas(cfg, canvas, janelaCfg) {
    return (ctx, dt) => {
        const j = caixaDaJanela(janelaCfg, canvas);
        const m = j.l * .035;                       // a espessura da moldura

        // A MOLDURA: quatro barras EM VOLTA do vidro, com a de cima e a da esquerda mais claras (a luz
        // da sala vem da árvore, que está àquele lado).
        //
        // Elas são quatro barras e não um retângulo cheio, e isso já foi um bug: um `fillRect` do
        // tamanho da janela inteira, feito pra ser a sombra do rebaixo, TAPAVA o vidro — a paisagem
        // era desenhada e a moldura pintava por cima dela um retângulo opaco. O sintoma era "não vejo
        // o fundo na janela", e não havia erro nenhum pra bancada pegar: a camada de baixo rodava
        // certinho e o defeito era a de cima existir.
        ctx.fillStyle = cfg.moldura;
        ctx.fillRect(j.x - m, j.y - m, j.l + m * 2, m);
        ctx.fillRect(j.x - m, j.y + j.a, j.l + m * 2, m);
        ctx.fillRect(j.x - m, j.y - m, m, j.a + m * 2);
        ctx.fillRect(j.x + j.l, j.y - m, m, j.a + m * 2);
        ctx.fillStyle = cfg.molduraLuz;
        ctx.fillRect(j.x - m, j.y - m, j.l + m * 2, m * .3);
        ctx.fillRect(j.x - m, j.y - m, m * .3, j.a + m * 2);

        // as TRAVESSAS que dividem o vidro
        ctx.fillStyle = cfg.moldura;
        for (let c = 1; c <= janelaCfg.colunas; c++) {
            ctx.fillRect(j.x + (j.l / (janelaCfg.colunas + 1)) * c - m * .3, j.y, m * .6, j.a);
        }
        for (let f = 1; f <= janelaCfg.linhas; f++) {
            ctx.fillRect(j.x, j.y + (j.a / (janelaCfg.linhas + 1)) * f - m * .3, j.l, m * .6);
        }

        // o PEITORIL, que projeta pra fora dos dois lados
        ctx.fillStyle = cfg.molduraLuz;
        ctx.fillRect(j.x - m * 2.4, j.y + j.a + m, j.l + m * 4.8, m * .9);
        ctx.fillStyle = cfg.molduraSombra;
        ctx.fillRect(j.x - m * 2.4, j.y + j.a + m + m * .9, j.l + m * 4.8, m * .35);

        // as CORTINAS
        const topo = j.y - m * 1.6;
        const pe = j.y + j.a + m * 1.4;
        const larg = j.l * cfg.larguraTopo;
        const cintura = j.y + j.a * cfg.amarra;
        for (const lado of [-1, 1]) {
            const borda = lado < 0 ? j.x - m : j.x + j.l + m;
            const dentro = (u) => borda + lado * -1 * u;   // `u` cresce PRA DENTRO do vidro

            ctx.beginPath();
            ctx.moveTo(borda + lado * m * 2.4, topo);
            ctx.lineTo(dentro(larg), topo);
            ctx.quadraticCurveTo(dentro(larg * .96), cintura - j.a * .12, dentro(larg * cfg.aperto), cintura);
            ctx.quadraticCurveTo(dentro(larg * 1.05), cintura + j.a * .3, dentro(larg * .7), pe);
            ctx.lineTo(borda + lado * m * 2.4, pe);
            ctx.closePath();
            const g = ctx.createLinearGradient(borda, 0, dentro(larg), 0);
            g.addColorStop(0, cfg.panoSombra);
            g.addColorStop(.45, cfg.pano);
            g.addColorStop(1, cfg.panoLuz);
            ctx.fillStyle = g;
            ctx.fill();

            // as FOLGAS: o pano tem vinco, e é o vinco que diz que ele é pesado
            ctx.strokeStyle = cfg.panoSombra;
            ctx.lineWidth = Math.max(1, j.l * .006);
            for (let i = 1; i <= cfg.folgas; i++) {
                const u = larg * (i / (cfg.folgas + 1));
                ctx.beginPath();
                ctx.moveTo(dentro(u), topo + j.a * .04);
                ctx.quadraticCurveTo(dentro(u * cfg.aperto * 1.3), cintura, dentro(u * .78), pe - j.a * .03);
                ctx.stroke();
            }

            // o LAÇO na cintura
            ctx.fillStyle = cfg.laco;
            ctx.beginPath();
            ctx.ellipse(dentro(larg * cfg.aperto * .62), cintura, larg * cfg.aperto * .8, j.a * .022, 0, 0, Math.PI * 2);
            ctx.fill();
        }

        // o VARÃO por cima de tudo, com as ponteiras
        ctx.fillStyle = cfg.varao;
        ctx.fillRect(j.x - m * 3.4, topo - m * .9, j.l + m * 6.8, m * .8);
        ctx.fillStyle = cfg.ponteira;
        for (const x of [j.x - m * 3.4, j.x + j.l + m * 3.4]) {
            ctx.beginPath();
            ctx.arc(x, topo - m * .5, m * .62, 0, Math.PI * 2);
            ctx.fill();
        }
    };
}

/// A LAREIRA do canto direito — a pedra, o fogo que APAGA quando ele chega, e o 🎅 ENTALADO.
///
/// A chama é a mesma da ruína dos ⚙️ Tecnológicos e dos focos da vila élfica: terceiro cliente do
/// `desenharChama`, sem uma linha nova. O que muda é que a altura dela agora vem do maestro, então o
/// fogo morre e volta sem que esta camada precise de fase nenhuma.
///
/// E o Papai Noel aparece só DA BARRIGA PRA BAIXO, que é a decisão que torna a peça desenhável: a
/// mesma saída que o ⭐ Especial usou pro 🦸 e o 🦹 atrás do jornal. Corpo humano inteiro em canvas
/// fica esquisito; duas pernas de bota saindo de um vão são legíveis e engraçadas.
/// O contorno da BOCA da lareira, com o fundo em `fundo`. Ele existe separado porque tem DOIS
/// clientes que não podem discordar: o preenchimento escuro do vão (`fundo` = o chão) e o recorte do
/// 🎅 descendo (`fundo` = o rodapé da tela, pra as botas passarem do chão). É o padrão do `comListras`
/// do ⭐ Especial — monta o caminho uma vez e usa pras duas coisas.
///
/// Ele deixa o caminho ABERTO, sem preencher: quem chama decide se aquilo é buraco preto ou `clip()`.
export function caminhoDaBoca(ctx, cx, topo, largura, fundo) {
    ctx.beginPath();
    ctx.moveTo(cx - largura / 2, fundo);
    ctx.lineTo(cx - largura / 2, topo + largura * .18);
    ctx.quadraticCurveTo(cx, topo - largura * .1, cx + largura / 2, topo + largura * .18);
    ctx.lineTo(cx + largura / 2, fundo);
    ctx.closePath();
}

export function criarLareira(cfg, canvas, natal) {
    const medirChao = medidorDoChaoDaSala(canvas);
    let t = 0;
    // A POEIRA do tombo. Ela é disparada pela CHEGADA (`pernas` encostando em 1) e não por um passo do
    // roteiro: assim a lareira não precisa saber os nomes das fases de ninguém — ela só olha o número
    // que já lê e percebe que ele parou de subir.
    let pousou = false;
    let poeira = 0;

    return (ctx, dt) => {
        t += dt;
        if (natal.pernas > .99 && !pousou) { pousou = true; poeira = 1; }
        if (natal.pernas < .5) pousou = false;
        if (poeira > 0) poeira = Math.max(0, poeira - dt / cfg.poeiraDura);
        const chao = medirChao(dt);
        const w = canvas.width, h = canvas.height;
        const cx = w * cfg.x;
        const larg = w * cfg.largura;
        const alt = h * cfg.altura;
        const topo = chao - alt;
        const bocaL = larg * cfg.boca;
        const bocaA = alt * cfg.bocaAltura;
        const bocaTopo = chao - bocaA;

        // O TREMOR do aviso sacode a lareira INTEIRA — pedra, cornija, fogo, máscaras e ele. Fica num
        // `translate` só, aqui em cima, porque tremer é uma coisa que acontece com a peça toda: cada
        // parte com o seu deslocamento seria a lareira se desmontando, não tremendo. Duas frequências
        // fora de compasso e a vertical mais curta que a horizontal, que é como um tranco no chão
        // chega numa parede.
        const trem = natal.tremor * cfg.tremorAmp * bocaA;
        ctx.save();
        ctx.translate(Math.sin(t * 44) * trem, Math.sin(t * 37 + 1.1) * trem * .55);

        // o corpo de pedra
        ctx.fillStyle = cfg.pedra;
        ctx.fillRect(cx - larg / 2, topo, larg, alt);
        // as juntas: fiadas alternadas, e é a alternância que faz ler como alvenaria em vez de grade
        ctx.strokeStyle = cfg.argamassa;
        ctx.lineWidth = Math.max(1, alt * .006);
        const fiada = alt * .1;
        for (let y = topo + fiada; y < chao; y += fiada) {
            ctx.beginPath();
            ctx.moveTo(cx - larg / 2, y);
            ctx.lineTo(cx + larg / 2, y);
            ctx.stroke();
        }
        for (let i = 0, y = topo; y < chao - fiada * .5; y += fiada, i++) {
            const passo = larg / 4;
            for (let x = cx - larg / 2 + (i % 2 ? passo / 2 : 0); x < cx + larg / 2; x += passo) {
                if (x <= cx - larg / 2 + 1) continue;
                ctx.beginPath();
                ctx.moveTo(x, y);
                ctx.lineTo(x, Math.min(chao, y + fiada));
                ctx.stroke();
            }
        }

        // o VÃO, pelo caminho compartilhado — o mesmo que vai recortar o Papai Noel lá embaixo
        caminhoDaBoca(ctx, cx, bocaTopo, bocaL, chao);
        ctx.fillStyle = cfg.dentro;
        ctx.fill();

        // a LENHA e o FOGO
        ctx.fillStyle = cfg.lenha;
        for (let i = 0; i < 3; i++) {
            ctx.save();
            ctx.translate(cx + (i - 1) * bocaL * .18, chao - bocaA * .06 - (i === 1 ? bocaA * .07 : 0));
            ctx.rotate((i - 1) * .22);
            ctx.beginPath();
            ctx.ellipse(0, 0, bocaL * .22, bocaA * .045, 0, 0, Math.PI * 2);
            ctx.fill();
            ctx.restore();
        }
        if (natal.fogo > .02) {
            for (let i = 0; i < cfg.labaredas; i++) {
                desenharChama(ctx, cx + (i - 1) * bocaL * .2, chao - bocaA * .05,
                    bocaA * .42 * natal.fogo, t * (1 + i * .3) + i * 2.1, cfg);
            }
            // o clarão no piso da sala. Ele MORRE junto com o fogo, e é isso que faz a sala inteira
            // escurecer quando o Papai Noel chega — sem que nada além do fogo precise saber disso.
            const luz = ctx.createRadialGradient(cx, chao, 0, cx, chao, larg * 1.5);
            luz.addColorStop(0, `rgba(${cfg.fogo}, ${.18 * natal.fogo})`);
            luz.addColorStop(1, `rgba(${cfg.fogo}, 0)`);
            ctx.fillStyle = luz;
            ctx.beginPath();
            ctx.arc(cx, chao, larg * 1.5, 0, Math.PI * 2);
            ctx.fill();
        }

        // a CORNIJA, por cima da pedra
        ctx.fillStyle = cfg.cornija;
        ctx.fillRect(cx - larg * .58, topo - alt * .06, larg * 1.16, alt * .06);
        ctx.fillStyle = cfg.cornijaLuz;
        ctx.fillRect(cx - larg * .58, topo - alt * .06, larg * 1.16, alt * .015);

        // 🎭 AS MÁSCARAS na parede acima da chaminé — o Mímico, que estava sem representação desde as
        // máscaras saírem da árvore. A triste vem primeiro e a alegre por cima, meio encavaladas e
        // tortas pra lados contrários: é assim que o emoji as põe, e emparelhadas e retas elas leriam
        // como dois quadros pendurados em vez de um par.
        const ms = larg * cfg.mascaras.tamanho;
        const my = topo - alt * .06 - ms * .95;
        desenharMascaraTeatro(ctx, cx - ms * cfg.mascaras.afasta, my + ms * .13, ms,
            cfg.mascaras.amarela, false, -cfg.mascaras.giro, cfg.mascaras.traco);
        desenharMascaraTeatro(ctx, cx + ms * cfg.mascaras.afasta, my, ms,
            cfg.mascaras.azul, true, cfg.mascaras.giro, cfg.mascaras.traco);

        // 🎅 ENTALADO. Ele DESCE recortado pela boca da lareira, que é o mesmo jeito de descer do cocô
        // pelo ralo, do golfinho saindo d'água e da cauda da sereia: a peça inteira existe o tempo
        // todo e quem a mostra aos poucos é a BORDA por onde ela passa. O recorte não tem fundo — só
        // as laterais e o topo —, e é isso que deixa as botas passarem do chão em vez de serem
        // cortadas na soleira.
        //
        // E a queda ACELERA (`pernas²`): coisa que cai não desce a velocidade constante, e foi por
        // isso que a primeira versão parecia que ele estava sendo baixado por uma corda.
        if (natal.pernas > .001) {
            const q = natal.pernas * natal.pernas;
            const bal = Math.sin(t * 11) * cfg.mexe * bocaA * natal.mexe;
            // `py` é a BARRA DE BAIXO do casaco, e ela termina no chão: quem caiu de bunda está
            // sentado NO chão, não pendurado acima dele. Tudo o mais é medido a partir daqui.
            const inicio = bocaTopo - bocaA * .8;
            const py = inicio + q * (chao - inicio);

            if (poeira > 0) {
                for (let i = 0; i < 3; i++) {
                    const p = 1 - poeira;
                    ctx.beginPath();
                    ctx.ellipse(cx + (i - 1) * bocaL * .26, chao - bocaA * .02,
                        bocaL * (.1 + p * .3), bocaA * (.03 + p * .07), 0, 0, Math.PI * 2);
                    ctx.fillStyle = `rgba(${cfg.poeira}, ${.35 * poeira})`;
                    ctx.fill();
                }
            }

            // O RECORTE é o ARCO da boca, e não um retângulo — é o MESMO caminho que preencheu o vão
            // ali em cima, com o fundo estendido pra baixo do chão. Com o retângulo ele passava por
            // cima da pedra nas duas quinas de cima, onde a curva já se afastou: os ombros dele
            // apareciam ENCOSTADOS no arco em vez de sumirem atrás dele. É o padrão do `comListras`:
            // monta o caminho uma vez, usa pra preencher E pra recortar, e as duas nunca discordam.
            //
            // E o recorte tem DOIS subcaminhos: o vão até a soleira, e um retângulo largo dali pra
            // baixo. As paredes da lareira só mandam nele enquanto ele está DENTRO dela — passando do
            // chão ele está na sala, e ali nada tem por que cortá-lo. Com o vão sozinho as botas eram
            // ceifadas por uma linha invisível no meio do piso.
            //
            // Os dois subcaminhos correm no MESMO sentido de propósito: pela regra `nonzero` eles se
            // somam onde se sobrepõem. Em sentidos contrários se anulariam e abririam um buraco
            // exatamente na emenda — foi o que rasgou a perna do 🦖 no ⭐ Especial.
            ctx.save();
            caminhoDaBoca(ctx, cx, bocaTopo, bocaL, chao);
            ctx.rect(cx - bocaL, chao, bocaL * 2, canvas.height - chao);
            ctx.clip();

            const meio = cx + bal * .5;
            // O casaco é ALTO — mais alto que a boca —, e o que sobra por cima some atrás do arco.
            // Ele nasceu com 58% da altura do vão e ficava mais largo do que alto: um bloco vermelho
            // deitado, que não lê como corpo nenhum. Corpo é coluna; o que decide a leitura não é o
            // tamanho dele, é a PROPORÇÃO — e como o vão manda na largura (ele tem de encher de parede
            // a parede pra parecer entalado), quem tinha de crescer era a altura.
            const alturaCasaco = bocaA * 1.05;
            const cintoY = py - bocaA * .36;
            const cintoH = bocaA * .11;

            // 1. AS PERNAS, primeiro, pra a terminação do casaco cair POR CIMA delas. Trapézio chapado
            //    e bota de caixa arredondada, no molde do 🦸 e do 🦹 do banheiro: ali a saída foi
            //    mostrar só a canela e o pé, aqui é só da cintura pra baixo — nos dois casos o que
            //    resolve corpo humano em canvas é ESCOLHER O PEDAÇO, e não desenhar melhor.
            //
            //    Elas saem PRA FORA e pra baixo porque ele está sentado: com as pernas retas, um
            //    homem de bunda no chão lê como um homem em pé enfiado no buraco.
            //    E elas são COMPRIDAS: a primeira versão dava 22px de perna à mostra contra um casaco
            //    de 180, e o que aparecia embaixo da barra parecia um toco. Perna de quem caiu
            //    sentado vai LONGE pra frente — aqui ela desce bem abaixo da soleira, que é onde o
            //    chão da sala está e onde ela tem de estar pra ler como perna esticada.
            const pe = py + bocaA * .34;
            for (const lado of [-1, 1]) {
                const px = meio + lado * bocaL * .2;
                const chute = bal * lado * 1.6;
                const pontaX = px + lado * bocaL * .14 + chute;
                ctx.fillStyle = cfg.terno;
                ctx.beginPath();
                ctx.moveTo(px - bocaL * .13, py - bocaA * .16);
                ctx.lineTo(px + bocaL * .13, py - bocaA * .16);
                ctx.lineTo(pontaX + bocaL * .115, pe);
                ctx.lineTo(pontaX - bocaL * .115, pe);
                ctx.closePath();
                ctx.fill();

                // a LINHA BRANCA acima da bota — o punho de pelo da calça. Sem ela a perna e a bota
                // viram uma peça vermelha e preta emendada, e é esse fio que diz que são duas coisas.
                ctx.fillStyle = cfg.pelo;
                ctx.fillRect(pontaX - bocaL * .13, pe - bocaA * .055, bocaL * .26, bocaA * .055);

                ctx.fillStyle = cfg.bota;
                caixaRedonda(ctx, pontaX - bocaL * .145, pe, bocaL * .29, bocaA * .13, bocaL * .05);
                ctx.fill();
                // a sola virada pra frente: é ela que diz que o pé aponta pra fora da lareira
                ctx.beginPath();
                ctx.ellipse(pontaX + lado * bocaL * .05, pe + bocaA * .115,
                    bocaL * .17, bocaA * .042, 0, 0, Math.PI * 2);
                ctx.fill();
            }

            // 2. O CASACO, ocupando o vão de parede a parede — é isso que diz que ele está ENTALADO.
            //    Sobrando folga dos lados, a leitura vira "sentado", que é outra piada.
            //
            //    O caminho dele é montado UMA vez e serve pras duas coisas: preencher o vermelho e
            //    RECORTAR tudo o que vem por cima. Sem o recorte, o debrum das bordas e o cinto são
            //    retângulos retos passando por fora dos cantos arredondados — sobrariam pontas
            //    brancas e pretas nas quatro quinas. É a mesma lição do `comListras`: a borda que
            //    apara é a própria forma, e as duas nunca podem discordar porque são o mesmo caminho.
            caixaRedonda(ctx, meio - bocaL * .5, py - alturaCasaco, bocaL, alturaCasaco, bocaL * .16);
            ctx.fillStyle = cfg.terno;
            ctx.fill();

            ctx.save();
            ctx.clip();

            // 3. A TERMINAÇÃO do casaco, do cinto até o chão. Ela pinta por cima do alto das pernas,
            //    que é como uma roupa se comporta.
            ctx.fillStyle = cfg.terno;
            ctx.fillRect(meio - bocaL * .5, cintoY + cintoH, bocaL, py - (cintoY + cintoH));

            // 4. A LINHA BRANCA do MEIO — a abotoadura, INTEIRA, do alto até a barra. Ela vem DEPOIS
            //    da terminação de propósito: enquanto a terminação era pintada por último, ela apagava
            //    a listra justamente abaixo do cinto, e a abotoadura parecia acabar na fivela. Casaco
            //    abotoa até embaixo.
            //
            //    Já um debrum vertical em cada LATERAL do casaco existiu e saiu: naquela altura as
            //    duas faixas leem como PUNHO DE BRAÇO, e ele não tem braço nenhum em cena. As "bordas"
            //    de um casaco de Papai Noel são a abotoadura e a BARRA de baixo, não os flancos.
            ctx.fillStyle = cfg.pelo;
            ctx.fillRect(meio - bocaL * .045, py - alturaCasaco, bocaL * .09, alturaCasaco);

            // 5. O CINTO, POR CIMA de tudo — inclusive da abotoadura, que ele corta em duas. É essa a
            //    ordem, e é ela que faz o cinto não ser o fim da roupa: o casaco continua embaixo dele.
            ctx.fillStyle = cfg.bota;
            ctx.fillRect(meio - bocaL * .5, cintoY, bocaL, cintoH);
            ctx.fillStyle = cfg.fivela;
            ctx.fillRect(meio - bocaL * .09, cintoY - cintoH * .18, bocaL * .18, cintoH * 1.36);

            // 6. a barra de pelo, fechando a roupa embaixo
            ctx.fillStyle = cfg.pelo;
            ctx.fillRect(meio - bocaL * .5, py - bocaA * .055, bocaL, bocaA * .055);

            ctx.restore();
            ctx.restore();
        }

        ctx.restore();   // fecha o `translate` do tremor
    };
}

/// 🎭 Uma MÁSCARA de teatro, no molde do emoji: a face lisa, os olhos e a boca em PRETO e nada mais.
/// `sorri` decide qual das duas ela é, e a diferença entre a alegre e a triste é só a foice da boca
/// virada — o mesmo desenho, dois estados, como o morcego e o vampiro dos 🔱 Decaídos.
///
/// A boca é uma FOICE preenchida e não um traço: contorno fino some a esta escala, e o que se
/// reconhece numa máscara de teatro a 60px é a mancha escura da boca aberta, não a linha dela.
export function desenharMascaraTeatro(ctx, cx, cy, s, cor, sorri, giro, traco) {
    ctx.save();
    ctx.translate(cx, cy);
    ctx.rotate(giro);

    // O TOPO é CÔNCAVO — as duas pontas de cima sobem e o meio da testa afunda entre elas. Ele nasceu
    // convexo, com um bico no alto, e assim uma máscara de teatro lê como ovo pintado: é a mordida no
    // meio da testa que diz que aquilo é uma casca vestida no rosto de alguém, e não uma cabeça.
    ctx.fillStyle = cor;
    ctx.beginPath();
    ctx.moveTo(-s * .72, -s * .76);
    ctx.quadraticCurveTo(0, -s * .42, s * .72, -s * .76);
    ctx.quadraticCurveTo(s * .92, -s * .1, s * .6, s * .42);
    ctx.quadraticCurveTo(s * .3, s, 0, s);
    ctx.quadraticCurveTo(-s * .3, s, -s * .6, s * .42);
    ctx.quadraticCurveTo(-s * .92, -s * .1, -s * .72, -s * .76);
    ctx.closePath();
    ctx.fill();

    // Os olhos inclinam pra lados contrários nas duas: erguidos na alegre, caídos na triste. É o
    // ÂNGULO deles que faz a leitura tanto quanto a boca — com os dois iguais, uma máscara de boca
    // pra baixo lê como surpresa, não como tristeza.
    ctx.fillStyle = traco;
    for (const lado of [-1, 1]) {
        ctx.beginPath();
        ctx.ellipse(lado * s * .33, -s * .22, s * .2, s * .13, lado * (sorri ? .38 : -.38), 0, Math.PI * 2);
        ctx.fill();
    }

    ctx.beginPath();
    ctx.moveTo(-s * .36, s * (sorri ? .2 : .52));
    ctx.quadraticCurveTo(0, s * (sorri ? .68 : .04), s * .36, s * (sorri ? .2 : .52));
    ctx.quadraticCurveTo(0, s * (sorri ? .46 : .26), -s * .36, s * (sorri ? .2 : .52));
    ctx.closePath();
    ctx.fill();

    ctx.restore();
}

/// 🎄 A ÁRVORE DE NATAL, no canto ESQUERDO — a peça que sobreviveu a três versões desta pele.
///
/// É um builder só porque é uma composição só: a luz colorida no piso sai dos pisca-piscas que estão
/// nela, a estrela ilumina as ramadas de cima, e as máscaras estão penduradas nas beiradas que este
/// código acabou de traçar. Separar faria metades lendo o mesmo número de dois lugares.
///
/// A ESTRELA lê o maestro e é a única coisa da árvore que não tem vida própria. Ela acende ANTES de o
/// Papai Noel aparecer na janela e apaga depois que ele passa — antes ela brilhava o tempo todo, e
/// brilho constante não anuncia coisa nenhuma. O `repouso` existe pra ela não sumir no intervalo:
/// apagada de todo, viraria um enfeite escuro no topo.
export function criarArvoreDeNatal(cfg, canvas, natal) {
    const medirChao = medidorDoChaoDaSala(canvas);
    let t = 0;

    const luzes = [];
    for (let r = 0; r < cfg.ramadas; r++) {
        for (let i = 0; i < cfg.luzesPorRamada; i++) {
            luzes.push({
                ramada: r,
                u: (i + .5) / cfg.luzesPorRamada * 2 - 1,
                cor: cfg.luzes[(r + i) % cfg.luzes.length],
                ritmo: entre(cfg.luzPisca),
                fase: Math.random() * Math.PI * 2,
            });
        }
    }
    const bolas = Array.from({ length: cfg.bolas }, (_, i) => ({
        ramada: 1 + (i % (cfg.ramadas - 2)),
        u: (Math.random() - .5) * 1.5,
        cor: cfg.bola[i % cfg.bola.length],
    }));
    const anjos = cfg.anjos.map((a, i) => ({ ...a, ritmo: entre(cfg.anjoBalanco), fase: i * 2.1 }));

    return (ctx, dt) => {
        t += dt;
        const base = medirChao(dt);
        const l = canvas.width, h = canvas.height;
        const cx = l * cfg.x;
        const H = h * cfg.altura;
        const W = l * cfg.largura * .5;

        const aceso = Math.max(0, (cfg.repouso + (1 - cfg.repouso) * natal.brilho)
            * (1 + cfg.pulso * Math.sin(t * cfg.ritmo)));

        // Onde fica a beirada de cada ramada. UMA conta, quatro clientes (a copa, as luzes, as bolas e
        // as máscaras) — duas cópias divergiriam no meio da cena.
        const daRamada = (r) => {
            const f = r / (cfg.ramadas - 1);
            return { y: base - H * (cfg.tronco0 + f * .78), meia: W * (1 - f * cfg.afina) };
        };
        // Um ponto NA beirada, com `u` de -1 (ponta esquerda) a +1 (direita). A beirada verga no meio,
        // então o y sai da mesma parábola que o desenho usa pra fechar a ramada.
        const naBeirada = (r, u) => {
            const { y, meia } = daRamada(r);
            return { x: cx + u * meia, y: y + (1 - u * u) * H * cfg.sacada };
        };

        // ---- a luz no PISO ----
        const halo = ctx.createRadialGradient(cx, base, 0, cx, base, l * cfg.clarao);
        halo.addColorStop(0, `rgba(${cfg.estrela}, ${Math.min(1, .16 * aceso)})`);
        halo.addColorStop(.45, `rgba(${cfg.estrela}, ${Math.min(1, .05 * aceso)})`);
        halo.addColorStop(1, `rgba(${cfg.estrela}, 0)`);
        ctx.fillStyle = halo;
        ctx.beginPath();
        ctx.arc(cx, base, l * cfg.clarao, 0, Math.PI * 2);
        ctx.fill();

        for (let i = 0; i < cfg.luzes.length; i++) {
            // Cada poça respira no seu tempo, e é isso que faz o piso parecer iluminado por MUITAS
            // luzinhas em vez de por um holofote colorido.
            const pulsa = .7 + .3 * Math.sin(t * (1.1 + i * .43) + i);
            ctx.beginPath();
            ctx.ellipse(cx + (i - 1) * l * cfg.pocas, base + h * .06,
                l * cfg.pocas * 1.6, h * cfg.alcance * .1, 0, 0, Math.PI * 2);
            ctx.fillStyle = `rgba(${cfg.luzes[i]}, ${Math.min(1, .1 * pulsa)})`;
            ctx.fill();
        }

        ctx.fillStyle = cfg.tronco;
        ctx.fillRect(cx - W * .07, base - H * cfg.tronco0 - 1, W * .14, H * cfg.tronco0 + 1);

        for (let r = 0; r < cfg.ramadas; r++) {
            const { y, meia } = daRamada(r);
            const apice = y - H * .17;
            const g = ctx.createLinearGradient(cx - meia, apice, cx + meia, y);
            g.addColorStop(0, cfg.copaLuz);
            g.addColorStop(1, cfg.copaSombra);
            ctx.fillStyle = g;
            ctx.beginPath();
            ctx.moveTo(cx, apice);
            ctx.lineTo(cx + meia, y);
            ctx.quadraticCurveTo(cx, y + H * cfg.sacada * 2, cx - meia, y);
            ctx.closePath();
            ctx.fill();

            ctx.strokeStyle = cfg.neve;
            ctx.lineCap = 'round';
            ctx.lineWidth = Math.max(1.5, H * .009);
            ctx.beginPath();
            ctx.moveTo(cx + meia, y);
            ctx.quadraticCurveTo(cx, y + H * cfg.sacada * 2, cx - meia, y);
            ctx.stroke();
        }

        for (const b of bolas) {
            const p = naBeirada(b.ramada, b.u);
            const raio = l * cfg.bolaRaio;
            ctx.fillStyle = b.cor;
            ctx.beginPath();
            ctx.arc(p.x, p.y + raio * 1.4, raio, 0, Math.PI * 2);
            ctx.fill();
            ctx.fillStyle = 'rgba(255, 255, 255, .5)';
            ctx.beginPath();
            ctx.arc(p.x - raio * .34, p.y + raio * 1.05, raio * .28, 0, Math.PI * 2);
            ctx.fill();
        }

        for (const a of anjos) {
            const p = naBeirada(a.ramada, a.u);
            const s = l * a.tamanho;
            ctx.save();
            ctx.translate(p.x, p.y);
            // o cordão sai do galho e o anjinho balança PENDURADO nele — girar em torno do centro dele
            // daria um anjo rodopiando no ar
            ctx.rotate(Math.sin(t * a.ritmo + a.fase) * .2);
            ctx.strokeStyle = cfg.anjoFio;
            ctx.lineWidth = Math.max(1, s * .05);
            ctx.beginPath();
            ctx.moveTo(0, 0);
            ctx.lineTo(0, s * .42);
            ctx.stroke();
            desenharAnjinho(ctx, 0, s * .42, s, cfg);
            ctx.restore();
        }

        for (const z of luzes) {
            const p = naBeirada(z.ramada, z.u);
            const brilho = Math.max(0, .35 + .65 * Math.sin(t * z.ritmo + z.fase));
            const raio = l * cfg.luzRaio;
            const g = ctx.createRadialGradient(p.x, p.y, 0, p.x, p.y, raio * cfg.luzClarao);
            g.addColorStop(0, `rgba(${z.cor}, ${Math.min(1, .55 * brilho)})`);
            g.addColorStop(1, `rgba(${z.cor}, 0)`);
            ctx.fillStyle = g;
            ctx.beginPath();
            ctx.arc(p.x, p.y, raio * cfg.luzClarao, 0, Math.PI * 2);
            ctx.fill();
            ctx.fillStyle = `rgba(${z.cor}, ${Math.min(1, .5 + .5 * brilho)})`;
            ctx.beginPath();
            ctx.arc(p.x, p.y, raio, 0, Math.PI * 2);
            ctx.fill();
        }

        desenharEstrela(ctx, cx, daRamada(cfg.ramadas - 1).y - H * .21, h * cfg.estrelaTamanho, aceso, t, cfg);
    };
}

/// Os PRESENTES: a pilha debaixo da árvore e o que desce flutuando da lareira.
///
/// São de UM dono só porque a chegada MEXE na pilha — o novo entra encostado na árvore e empurra a
/// fila inteira até o último sair de cena. Com a árvore desenhando a pilha e outra camada trazendo o
/// de cima, duas peças seriam donas do mesmo estado, que é justamente o que o maestro existe pra
/// evitar (e não é um caso que um campo compartilhado resolva: aqui o estado é uma LISTA que muda).
///
/// Cada caixa PERSEGUE o lugar dela em vez de pular pra ele. É o empurrão que tem de ser visto: sem a
/// perseguição, a fila se reorganizaria entre dois quadros e a de fora simplesmente sumiria.
export function criarPresentes(cfg, canvas, natal, arvoreCfg, lareiraCfg) {
    const medirChao = medidorDoChaoDaSala(canvas);
    let visto = 0;
    const pilha = [];
    const nova = (i) => ({
        alvo: arvoreCfg.x - i * cfg.passo,
        x: arvoreCfg.x - i * cfg.passo,
        cor: cfg.cor[i % cfg.cor.length],
        giro: (Math.random() - .5) * .2,
    });
    for (let i = 0; i < cfg.comeca; i++) pilha.unshift(nova(i));

    return (ctx, dt) => {
        const chao = medirChao(dt);
        const w = canvas.width, h = canvas.height;
        const s = h * cfg.tamanho;

        if (natal.entregues !== visto) {
            visto = natal.entregues;
            for (const p of pilha) p.alvo -= cfg.passo;
            pilha.push({ alvo: arvoreCfg.x, x: arvoreCfg.x, cor: cfg.cor[visto % cfg.cor.length], giro: (Math.random() - .5) * .2 });
            // O que passou do teto é EMPURRADO pra fora da imagem — ele não some, ele sai. É um `for`
            // sobre o excedente e não um `while (pilha.length > teto)`: quem sai daqui só sai da lista
            // lá embaixo, quando terminar de deslizar, então a condição do while nunca mudaria e o
            // laço travava a cena inteira. Marcar não é remover.
            for (let k = 0; k < pilha.length - cfg.teto; k++) pilha[k].alvo = cfg.saida;
        }

        for (let i = pilha.length - 1; i >= 0; i--) {
            const p = pilha[i];
            p.x += (p.alvo - p.x) * Math.min(1, dt * cfg.empurrar);
            if (p.x <= cfg.saida + .004) { pilha.splice(i, 1); continue; }
            desenharCaixa(ctx, w * p.x, chao, s, p.cor, p.giro, cfg);
        }

        // o que vem descendo da lareira, flutuando
        if (natal.entrega >= 0) {
            const q = natal.entrega;
            const dx = w * lareiraCfg.x;
            const dy = chao - h * lareiraCfg.altura * lareiraCfg.bocaAltura * .5;
            const ax = w * arvoreCfg.x;
            const x = dx + (ax - dx) * q;
            // ele SOBE um pouco antes de descer: foi largado, não jogado
            const y = dy + (chao - dy) * q - Math.sin(q * Math.PI) * h * cfg.arco;
            desenharCaixa(ctx, x, y, s, cfg.cor[visto % cfg.cor.length], Math.sin(q * cfg.giro * Math.PI) * .5, cfg);
        }
    };
}

/// Uma caixa de presente. Caixa, fita cruzada e laço — três formas, e nenhuma precisa ser LIDA:
/// caixinha com fita é reconhecida, que é o mesmo motivo pelo qual a vieira ficou na praia dos
/// Místicos. O `giro` é o que separa a pilha (tortinhas, como quem largou ali) da que vem voando.
export function desenharCaixa(ctx, x, y, s, cor, giro, cfg) {
    ctx.save();
    ctx.translate(x, y);
    ctx.rotate(giro);

    ctx.fillStyle = cor;
    ctx.fillRect(-s * .5, -s, s, s);

    ctx.fillStyle = cfg.fita;
    ctx.fillRect(-s * .09, -s, s * .18, s);
    ctx.fillRect(-s * .5, -s * .6, s, s * .16);

    ctx.beginPath();
    ctx.ellipse(-s * .17, -s * 1.06, s * .17, s * .1, -.4, 0, Math.PI * 2);
    ctx.fill();
    ctx.beginPath();
    ctx.ellipse(s * .17, -s * 1.06, s * .17, s * .1, .4, 0, Math.PI * 2);
    ctx.fill();

    ctx.restore();
}

/// A ESTRELA do topo. O que a faz LER como estrela e não como uma bola amarela são os RAIOS — os
/// espetos longos e finos que saem dela —, e é por isso que eles são desenhados antes do corpo: assim
/// eles nascem de dentro dela em vez de ficarem grudados por fora.
///
/// Só o ALFA responde ao brilho; nenhum raio de `arc` depende dele. É de propósito: `aceso` é o produto
/// da respiração da estrela pelo recuo que a coluna do Anjo impõe, e raio negativo num `arc` lança e
/// mata o `requestAnimationFrame` da cena inteira.
export function desenharEstrela(ctx, cx, cy, s, aceso, t, cfg) {
    ctx.save();

    // O HALO é a peça mais importante desta função, e ela cresceu: a estrela é o AVISO de que ele vem,
    // e um aviso que não muda a luz do cômodo inteiro não é aviso. O que se vê de longe não é o
    // desenho de cinco pontas, é o clarão em volta dele.
    const halo = ctx.createRadialGradient(cx, cy, 0, cx, cy, s * cfg.estrelaHalo);
    halo.addColorStop(0, `rgba(${cfg.estrelaNucleo}, ${Math.min(1, .55 * aceso)})`);
    halo.addColorStop(.18, `rgba(${cfg.estrela}, ${Math.min(1, .34 * aceso)})`);
    halo.addColorStop(.5, `rgba(${cfg.estrela}, ${Math.min(1, .12 * aceso)})`);
    halo.addColorStop(1, `rgba(${cfg.estrela}, 0)`);
    ctx.fillStyle = halo;
    ctx.beginPath();
    ctx.arc(cx, cy, s * cfg.estrelaHalo, 0, Math.PI * 2);
    ctx.fill();

    // os espetos, girando devagar — é o giro que impede a cruz de luz de virar um enfeite parado
    ctx.save();
    ctx.translate(cx, cy);
    ctx.rotate(t * .12);
    for (let i = 0; i < cfg.raios; i++) {
        ctx.rotate(Math.PI * 2 / cfg.raios);
        const comp = s * (i % 2 === 0 ? 6.2 : 3.6);
        const g = ctx.createLinearGradient(0, 0, 0, -comp);
        g.addColorStop(0, `rgba(${cfg.estrelaNucleo}, ${Math.min(1, .7 * aceso)})`);
        g.addColorStop(1, `rgba(${cfg.estrela}, 0)`);
        ctx.fillStyle = g;
        ctx.beginPath();
        ctx.moveTo(-s * .2, 0);
        ctx.lineTo(0, -comp);
        ctx.lineTo(s * .2, 0);
        ctx.closePath();
        ctx.fill();
    }
    ctx.restore();

    // o corpo: cinco pontas, uma volta só alternando raio cheio e raio do miolo
    ctx.beginPath();
    for (let i = 0; i < 10; i++) {
        const a = -Math.PI / 2 + i * Math.PI / 5;
        const r = i % 2 === 0 ? s : s * .42;
        const x = cx + Math.cos(a) * r, y = cy + Math.sin(a) * r;
        if (i === 0) ctx.moveTo(x, y); else ctx.lineTo(x, y);
    }
    ctx.closePath();
    const corpo = ctx.createRadialGradient(cx, cy, 0, cx, cy, s);
    corpo.addColorStop(0, `rgba(${cfg.estrelaNucleo}, ${Math.min(1, .98 * aceso)})`);
    corpo.addColorStop(1, `rgba(${cfg.estrela}, ${Math.min(1, .8 * aceso)})`);
    ctx.fillStyle = corpo;
    ctx.fill();

    ctx.restore();
}

/// 😇 Um ANJINHO de árvore de Natal. É o apóstolo que estava sem representação desde a cena virar
/// interior: a coluna de luz que era dele não cabe dentro de uma casa, e enfeite de árvore cabe.
///
/// Máscaras de teatro moraram neste lugar (eram o 🎭 Mímico) e saíram por pedido do Gabriel. O que
/// faz o anjinho ler são três coisas e nenhuma delas é o rosto: as ASAS abertas atrás, o vestido em
/// SINO e a AURÉOLA solta em cima da cabeça. É a mesma regra do sinal em vez da figura — a esta escala
/// um rosto seria sujeira, então ele não tem nenhum.
export function desenharAnjinho(ctx, cx, cy, s, cfg) {
    // as asas primeiro, pra nascerem de trás do corpo em vez de ficarem coladas por cima
    ctx.fillStyle = cfg.anjoAsa;
    for (const lado of [-1, 1]) {
        ctx.beginPath();
        ctx.moveTo(cx, cy + s * .34);
        ctx.quadraticCurveTo(cx + lado * s * .72, cy + s * .04, cx + lado * s * .5, cy + s * .56);
        ctx.quadraticCurveTo(cx + lado * s * .3, cy + s * .5, cx, cy + s * .34);
        ctx.closePath();
        ctx.fill();
    }

    // o vestido em sino
    ctx.fillStyle = cfg.anjoPano;
    ctx.beginPath();
    ctx.moveTo(cx, cy + s * .26);
    ctx.quadraticCurveTo(cx + s * .16, cy + s * .5, cx + s * .34, cy + s * .84);
    ctx.quadraticCurveTo(cx, cy + s * .96, cx - s * .34, cy + s * .84);
    ctx.quadraticCurveTo(cx - s * .16, cy + s * .5, cx, cy + s * .26);
    ctx.closePath();
    ctx.fill();

    ctx.fillStyle = cfg.anjoPele;
    ctx.beginPath();
    ctx.arc(cx, cy + s * .22, s * .19, 0, Math.PI * 2);
    ctx.fill();

    // a auréola SOLTA acima da cabeça, e não encostada nela: é o vão que a faz ler como auréola em
    // vez de chapéu
    ctx.strokeStyle = cfg.anjoAureola;
    ctx.lineWidth = Math.max(1, s * .07);
    ctx.beginPath();
    ctx.ellipse(cx, cy - s * .06, s * .21, s * .075, 0, 0, Math.PI * 2);
    ctx.stroke();
}

/// Um boneco de neve — uma das poucas figuras do jogo que já SÃO o próprio sinal: três bolas leem a
/// 20px, sem nenhuma anatomia pra dar errado. O problema que tirou o corpo do Herói e do Vilão de cena
/// não existe aqui.
export function desenharBonecoDeNeve(ctx, x, base, s, cfg) {
    ctx.beginPath();
    ctx.ellipse(x, base, s * .58, s * .15, 0, 0, Math.PI * 2);
    ctx.fillStyle = `rgba(${cfg.sombra}, .32)`;
    ctx.fill();

    ctx.save();
    ctx.translate(x, base);

    const bola = (cy, r) => {
        const g = ctx.createLinearGradient(-r, cy - r, r, cy + r);
        g.addColorStop(0, cfg.neve);
        g.addColorStop(1, cfg.neveSombra);
        ctx.fillStyle = g;
        ctx.beginPath();
        ctx.arc(0, cy, r, 0, Math.PI * 2);
        ctx.fill();
    };

    // Os galhos vêm ANTES das bolas: assim eles nascem de dentro do corpo em vez de ficarem colados
    // por cima dele.
    ctx.strokeStyle = cfg.galho;
    ctx.lineCap = 'round';
    ctx.lineWidth = Math.max(1, s * .045);
    ctx.beginPath();
    for (const lado of [-1, 1]) {
        ctx.moveTo(lado * s * .16, -s * .98);
        ctx.lineTo(lado * s * .64, -s * 1.32);
        ctx.moveTo(lado * s * .48, -s * 1.2);
        ctx.lineTo(lado * s * .68, -s * 1.14);
    }
    ctx.stroke();

    bola(-s * .42, s * .42);
    bola(-s * .97, s * .30);

    ctx.fillStyle = cfg.carvao;
    for (let i = 0; i < 3; i++) {
        ctx.beginPath();
        ctx.arc(0, -s * (.78 + i * .15), s * .03, 0, Math.PI * 2);
        ctx.fill();
    }

    // O cachecol tapa a emenda entre o corpo e a cabeça, que é o único lugar em que três círculos
    // empilhados denunciam que são três círculos empilhados.
    ctx.fillStyle = cfg.cachecol;
    ctx.fillRect(-s * .27, -s * 1.3, s * .54, s * .12);
    ctx.beginPath();
    ctx.moveTo(s * .14, -s * 1.24);
    ctx.quadraticCurveTo(s * .36, -s * 1.1, s * .28, -s * .84);
    ctx.lineTo(s * .12, -s * .88);
    ctx.quadraticCurveTo(s * .22, -s * 1.06, s * .04, -s * 1.2);
    ctx.closePath();
    ctx.fill();

    bola(-s * 1.46, s * .22);

    ctx.fillStyle = cfg.carvao;
    for (const lado of [-1, 1]) {
        ctx.beginPath();
        ctx.arc(lado * s * .085, -s * 1.52, s * .032, 0, Math.PI * 2);
        ctx.fill();
    }
    ctx.fillStyle = cfg.cenoura;
    ctx.beginPath();
    ctx.moveTo(0, -s * 1.47);
    ctx.lineTo(s * .26, -s * 1.44);
    ctx.lineTo(0, -s * 1.4);
    ctx.closePath();
    ctx.fill();

    ctx.restore();
}

/// O trenó e a parelha. Desenhado sempre virado pra DIREITA, e quem cuida do sentido E da inclinação é
/// UMA rotação pelo ângulo de marcha.
///
/// E é aí que mora a armadilha que o dragão já cobrou: girar ~180° (que é o que "ir pra esquerda" é)
/// inverte os DOIS eixos, e o trenó sairia de cabeça pra baixo com as renas correndo no teto. O espelho
/// vertical quando `cos(ângulo) < 0` é a mesma correção do golfinho e das patas do dragão.
export function desenharTreno(ctx, x, y, s, angulo, fase, piscas, cfg) {
    ctx.save();
    ctx.translate(x, y);
    ctx.rotate(angulo);
    if (Math.cos(angulo) < 0) ctx.scale(1, -1);

    ctx.fillStyle = cfg.corpo;
    ctx.strokeStyle = cfg.corpo;
    ctx.lineCap = 'round';

    // A BARRA da parelha, antes das renas pra passar por baixo delas. Ela é HORIZONTAL, na altura do
    // arreio: nasceu como um risco em diagonal saindo do trenó e chegando na última rena, e ficava
    // torta, atravessando os bichos pelo meio. Arreio é peça rígida — quem inclina é o conjunto todo,
    // e disso já cuida a rotação pelo ângulo de marcha.
    //
    // E ela passa NO MEIO dos bichos, não por cima: o corpo da rena tem o centro em −.24 nesta escala
    // (é o `s * .46` do `desenharRena` vezes o −.52 do dorso dela), e a barra estava em −.52, ou seja,
    // flutuando um corpo inteiro acima da parelha. Este número tem de sair daquele, senão os dois
    // divergem na primeira vez que alguém mexer no tamanho da rena.
    const arreio = -s * .46 * .52;
    ctx.lineWidth = Math.max(1, s * .05);
    ctx.beginPath();
    ctx.moveTo(s * .55, arreio);
    ctx.lineTo(s * (1 + (cfg.renas - 1) * cfg.separacao), arreio);
    ctx.stroke();
    // e o tirante curto que sobe do trenó até a barra
    ctx.beginPath();
    ctx.moveTo(s * .42, -s * .2);
    ctx.lineTo(s * .58, arreio);
    ctx.stroke();

    for (let i = 0; i < cfg.renas; i++) {
        // A da FRENTE é a última do laço, e é ela que leva o nariz vermelho.
        desenharRena(ctx, s * (1 + i * cfg.separacao), 0, s * .46, fase + i * 1.7, i === cfg.renas - 1, cfg);
    }

    // o casco do trenó, com a popa enrolada
    ctx.fillStyle = cfg.corpo;
    ctx.beginPath();
    ctx.moveTo(-s * .62, 0);
    ctx.lineTo(s * .42, 0);
    ctx.quadraticCurveTo(s * .52, -s * .06, s * .44, -s * .2);
    ctx.lineTo(-s * .34, -s * .2);
    ctx.quadraticCurveTo(-s * .66, -s * .24, -s * .6, -s * .54);
    ctx.quadraticCurveTo(-s * .88, -s * .48, -s * .74, -s * .14);
    ctx.closePath();
    ctx.fill();

    // o SACO: a corcova atrás do banco, e o motivo de o trenó ler como carregado
    ctx.beginPath();
    ctx.arc(-s * .16, -s * .2, s * .24, Math.PI, Math.PI * 2);
    ctx.fill();

    // o patim
    ctx.lineWidth = Math.max(1, s * .05);
    ctx.beginPath();
    ctx.moveTo(s * .5, s * .1);
    ctx.lineTo(-s * .56, s * .1);
    ctx.quadraticCurveTo(-s * .78, s * .1, -s * .76, -s * .04);
    ctx.stroke();

    // o FIO quente na quina de cima. Num céu fechado a silhueta preta some; este fio é o que a
    // recorta, e é o mesmo argumento que tirou o preto chapado dos exércitos do Reino.
    ctx.strokeStyle = `rgba(${cfg.brilho}, .5)`;
    ctx.lineWidth = Math.max(1, s * .035);
    ctx.beginPath();
    ctx.moveTo(-s * .34, -s * .21);
    ctx.lineTo(s * .43, -s * .21);
    ctx.stroke();

    // os PISCA-PISCAS pendurados na borda do trenó, cada um no seu ritmo — juntos, seriam um letreiro.
    for (let i = 0; i < piscas.length; i++) {
        const z = piscas[i];
        const px = -s * .3 + (i / Math.max(1, piscas.length - 1)) * s * .72;
        const brilho = Math.max(0, .3 + .7 * Math.sin(fase * .14 * z.ritmo + z.fase));
        const raio = s * .045;
        const g = ctx.createRadialGradient(px, -s * .23, 0, px, -s * .23, raio * 4.5);
        g.addColorStop(0, `rgba(${z.cor}, ${Math.min(1, .6 * brilho)})`);
        g.addColorStop(1, `rgba(${z.cor}, 0)`);
        ctx.fillStyle = g;
        ctx.beginPath();
        ctx.arc(px, -s * .23, raio * 4.5, 0, Math.PI * 2);
        ctx.fill();
        ctx.fillStyle = `rgba(${z.cor}, ${Math.min(1, .55 + .45 * brilho)})`;
        ctx.beginPath();
        ctx.arc(px, -s * .23, raio, 0, Math.PI * 2);
        ctx.fill();
    }

    ctx.restore();
}

/// Uma rena. A GALHADA é a única concessão a detalhe e ela se paga: sem ela o vulto lê como cavalo, e
/// cavalo puxando trenó é outra história.
///
/// `guia` é a da frente, e ela leva o NARIZ VERMELHO — a única coisa quente no céu inteiro da cena, e
/// a que diz de quem é o trenó sem precisar desenhar ninguém sentado nele.
export function desenharRena(ctx, x, y, s, fase, guia, cfg) {
    ctx.save();
    ctx.translate(x, y);
    ctx.fillStyle = cfg.corpo;
    ctx.strokeStyle = cfg.corpo;

    ctx.beginPath();
    ctx.ellipse(0, -s * .52, s * .44, s * .22, 0, 0, Math.PI * 2);
    ctx.fill();

    const passo = Math.sin(fase) * s * .2;
    ctx.lineWidth = Math.max(1, s * .09);
    ctx.beginPath();
    ctx.moveTo(-s * .28, -s * .48); ctx.lineTo(-s * .28 - passo, s * .08);
    ctx.moveTo(-s * .12, -s * .48); ctx.lineTo(-s * .12 + passo, s * .08);
    ctx.moveTo(s * .16, -s * .48); ctx.lineTo(s * .16 + passo, s * .08);
    ctx.moveTo(s * .3, -s * .48); ctx.lineTo(s * .3 - passo, s * .08);
    ctx.stroke();

    ctx.lineWidth = Math.max(1, s * .15);
    ctx.beginPath();
    ctx.moveTo(s * .3, -s * .6);
    ctx.lineTo(s * .56, -s * .92);
    ctx.stroke();
    ctx.beginPath();
    ctx.ellipse(s * .64, -s * .98, s * .18, s * .11, -.5, 0, Math.PI * 2);
    ctx.fill();

    ctx.lineWidth = Math.max(1, s * .06);
    ctx.beginPath();
    ctx.moveTo(s * .56, -s * 1.06); ctx.lineTo(s * .46, -s * 1.44);
    ctx.moveTo(s * .5, -s * 1.26); ctx.lineTo(s * .28, -s * 1.36);
    ctx.moveTo(s * .5, -s * 1.36); ctx.lineTo(s * .64, -s * 1.52);
    ctx.moveTo(s * .7, -s * 1.04); ctx.lineTo(s * .8, -s * 1.38);
    ctx.moveTo(s * .76, -s * 1.22); ctx.lineTo(s * .94, -s * 1.3);
    ctx.stroke();

    if (guia) {
        const nx = s * .8, ny = -s * 1.02;
        const g = ctx.createRadialGradient(nx, ny, 0, nx, ny, s * .5);
        g.addColorStop(0, `rgba(${cfg.nariz}, .6)`);
        g.addColorStop(1, `rgba(${cfg.nariz}, 0)`);
        ctx.fillStyle = g;
        ctx.beginPath();
        ctx.arc(nx, ny, s * .5, 0, Math.PI * 2);
        ctx.fill();
        ctx.fillStyle = `rgba(${cfg.nariz}, 1)`;
        ctx.beginPath();
        ctx.arc(nx, ny, s * .11, 0, Math.PI * 2);
        ctx.fill();
    }

    ctx.restore();
}
