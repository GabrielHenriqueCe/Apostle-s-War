import { comListras, desenharChama, entre } from '../comum/basicos.js';
import { criarNoHorizonte, medirDoTema, medirLadrilho } from '../comum/ladrilho.js';
import { desenharMorcego } from '../comum/ar.js';
import { criarNevoa, criarPo, criarVoadores } from '../comum/ar.js';
// 🔱 A VILA ÉLFICA VENDIDA. O 🧝 Elfo entregou a vila aos demônios; eles subiram POR DENTRO da
// Árvore do Mundo, que rachou e escorre lava. A cena é o dia seguinte ao massacre — a única luz
// vem de BAIXO (ver o bloco do tema no estilo.css), e não há corpo nenhum em cena: o que conta o
// que houve é o RASTRO. A vila arrasada é ladrilho; a árvore e o fogo são canvas.
//
// A CENA TEM UM CICLO, e ele é o que o Gabriel desenhou: os morcegos passam → a lava PARA de
// escorrer (o resto segue pulsando) → um momento de nada → o chão TREME → a terra se abre no meio
// → joga lava nova na árvore → treme outra vez → fecha. Quem manda nisso é a `fenda`, que escreve
// no maestro; ninguém chama ninguém.
export const ar = {
    // Os FOCOS DE INCÊNDIO da vila — as casas e as pontes que ainda estão queimando. Os `pontos`
    // são coordenadas DO LADRILHO (fração do passo e da altura dele), e caem em cima do que está
    // desenhado no SVG: o telhado da casa da direita, a ponta da ponte estourada e a escada de
    // corda caída no chão. Mexeu no desenho lá, mexe aqui.
    //
    // Eles REPETEM por ladrilho de propósito, e isso não contradiz a regra do endereço: o
    // incêndio não é "aquela casa ali", é "as casas que estão queimando" — e o desenho que repete
    // já traz uma cópia de cada uma. Canvas aqui não é por endereço, é porque fogo se mexe.
    fogos: {
        ladrilho: ['--vila-passo', '--vila-altura'],
        pontos: [{ x: .795, y: .2 }, { x: .655, y: .475 }, { x: .61, y: .52 }],
        tamanho: .02, labaredas: 3, fogo: '236, 88, 26', brasa: '255, 180, 88',
    },

    // A ÁRVORE DO MUNDO rachada — a peça central e a única fonte de luz. Ela ESCREVE o maestro.
    //
    // ENORME E NO MEIO (`x: .5`), com galhos e raízes GIGANTES esticando pros lados: é o que o
    // Gabriel pediu depois de ver a 1ª versão, que era alta, fina e de lado — lia como uma árvore
    // grande num canto, e não como a coisa que ocupa o mundo.
    //
    // Ela NÃO tem `assentada`: está plantada na linha da vila, que o `--vila-chao` do CSS
    // declara e o `medidorDoChaoDaVila` entrega pronta. Uma cópia dessa linha aqui seria um
    // segundo chão pra divergir do primeiro, e é exatamente o que a cena mostrou quando a árvore
    // vivia 37% de faixa abaixo das casas.
    //
    // `altura` é fração da arena; `largura` e `galhoComp` são fração da ALTURA dela, e não da
    // tela, senão o tronco engordaria em janela larga. Ela passa um pouco do topo da arena de
    // propósito: árvore CORTADA pela borda lê como grande demais pra caber.
    //
    // As RAÍZES saem de BAIXO da árvore e se emaranham em curvas bruscas até os cantos (o pedido
    // do Gabriel, ao pé da letra). Elas se medem na MEIA-LARGURA DA TELA (`raizAlcance`, 1 = chega
    // na borda), porque "canto do mapa" é medida da TELA e não da árvore — na unidade errada, a
    // mesma intenção precisaria de um número diferente pra cada formato de janela (é a lição da
    // ondulação do dragão). `raizNos` é quantos nós tem o caminho de cada uma (mais nós = mais
    // emaranhado), `raizOnda` é o quanto cada nó sai da linha, e `raizFundo` é a faixa de
    // profundidades em que elas se assentam — é ela que faz uma passar por cima da outra.
    //
    // `acima` é o quanto ela recua da linha das casas, em fração da faixa do ladrilho: é o que
    // abre a terra da frente pro terremoto acontecer à vista.
    arvore: {
        casca: '#090406', cascaLuz: '#1a0b0c',
        lava: '#c81c0c', lavaQuente: '#ff9a24', luz: '255, 78, 30',
        x: .5, altura: .89, largura: .17, acima: .16,
        raizes: 8, raizNos: 7, raizAlcance: [1.04, 1.4], raizFundo: [.14, .95],
        raizOnda: .3, raizGrossa: .72, raizFina: .05,
        galhos: 12, galhoDe: .3, galhoComp: [.28, .62],
        clarao: 1.6, pulso: [.6, 1], respiro: [.5, 1.3], tremor: .004,
        // A lava, no modelo do VENENO da ruína dos ⚙️ Tecnológicos: cabeça desce, fonte fecha,
        // cauda desce atrás. `de`/`ate` é a faixa do tronco de onde eles saem (vários pontos, não
        // uma boca só), `grandes` é a fatia que nasce bem mais grossa, e eles se sobrepõem à
        // vontade — dois fios no mesmo lugar viram um só, mais largo, que é como lava se junta.
        // `rapidos` é a fatia que desce MUITO mais depressa (o `ciclo` deles é multiplicado por
        // `pressa`): lava não escorre toda no mesmo ritmo, e é o desencontro de velocidade que
        // separa "vários fios" de "um efeito repetido N vezes".
        escorridos: {
            quantos: 12, de: .06, ate: .92, ciclo: [6, 12], larg: [.035, .08],
            grandes: .3, rapidos: .4, pressa: [.5, .75], engrossa: 1.7,
            // O quanto da boca os fios usam pra se espalhar (1 = a boca toda) e o quanto eles
            // AFUNDAM nela. A lava lá dentro é menor que a boca — quem mirasse a borda cairia na
            // beirada de pedra, e o pedido era cair na lava.
            espalha: .38, mergulha: .16,
            // O S: o quanto o fio serpenteia na descida, em fração da MEIA-LARGURA DO TRONCO.
            // Era fração da largura do próprio fio, e aí fio fino descia praticamente reto.
            serpente: [.22, .5],
        },
    },

    // As RACHADURAS, correndo POR CIMA DAS RAÍZES (e não soltas pelo chão, como na 1ª versão).
    // Elas não sabem onde as raízes estão: leem a lista que a árvore publica no maestro, em pixel.
    veias: {
        cor: '186, 26, 12', quente: '255, 150, 54',
        largura: .0055,
        // Quantas gretas soltas aparecem NA CASCA da árvore, e o quanto elas chegam perto da
        // borda do tronco (1 = encostando nela).
        naArvore: 6, abraco: .74,
    },

    // O BURACO NA TERRA, onde a lava da árvore cai — e o DIRETOR do ciclo (ver
    // `criarBuracoDoInferno`). Ele SEMPRE existe: `base` é o tamanho dele em repouso, e a erupção
    // só o escancara até 1. Antes eram duas peças (a poça ao pé da árvore e uma fenda que abria
    // longe dela), e o Gabriel apontou que isso já era o mesmo buraco contado duas vezes.
    //
    // `linha` é onde ele fica na terra da frente (0 = no pé da árvore, 1 = no rodapé), `largura` e
    // `funda` são a boca — que é FIXA: ele não cresce na erupção, apenas treme. E `chama` é a
    // coluna de fogo que sobe dele, a única luz que ele joga. Tudo o mais é duração de fase, e o
    // que se sorteia é a ESPERA, nunca a duração do gesto.
    buraco: {
        cor: '#0b0405', lava: '#c81c0c', lavaQuente: '#ff9a24', luz: '255, 78, 30',
        largura: .175, funda: .082, linha: .5, chama: .12,
        depois: [1.2, 3], secar: 1.6, seco: [2.5, 5],
        avisar: 1.5, jorrar: 2.6, acalmar: 1.8,
        respingos: 70, forca: .88,
        // Quanto tempo o brilho da árvore leva pra apagar depois de aceso. É a única duração da
        // cena que NÃO é uma fase: ela corre por fora da máquina, em todas elas, porque o apagar
        // tem de atravessar o fim da erupção sem ser interrompido por ela.
        brilhoDura: 6.5,
    },

    // A FUMAÇA que sobe da árvore e das rachaduras. Escura — o que queima aqui já queimou, então
    // isto é fuligem. Ela é o que impede o chão aceso de ler como adesivo colado na terra.
    fumaca: {
        cor: '26, 14, 14', colunas: 4, baforadas: 8, alcance: .46, abre: .16, opacidade: .34,
    },

    // A COLUNA DE MORCEGOS — o 🧛 Vampiro, sem desenhar um vampiro. Mesmo bicho do 🦇, outro
    // COMPORTAMENTO: em vez de atravessar a tela, eles se juntam num rodamoinho sobre a árvore,
    // seguram e estouram pra fora. Bando é bicho; coluna é alguém MANDANDO neles. É ela que
    // dispara o ciclo da fenda, ao estourar.
    coluna: {
        cor: '#0a0506', quantos: 16, tamanho: [10, 19], raio: .1, alto: .26,
        espera: [14, 26], juntar: 2.8, girar: 3.6, estourar: 1.6,
    },

    // Os MORCEGOS que atravessam — o 🦇, e o motor já existia (o `desenharMorcego` estava escrito
    // e guardado pra esta facção desde o cemitério). Em REVOADA: bicho que sobrevoa massacre anda
    // em bando, e o bando é o que os separa da coluna do Vampiro, que é a MESMA espécie.
    voadores: {
        forma: 'morcego', cor: '#090405', quantos: 6, tamanho: [9, 21],
        velocidade: [110, 240], intervalo: [4, 10],
        // Sem `espalhar` de propósito: ele é o quanto o bando se ABRE quando o vento passa por
        // baixo, e neste tema não há vento nenhum soprando (o maestro daqui é LUZ). Deixá-lo aqui
        // seria uma chave que nada lê — o motor só a consulta dentro do `if` do vento.
        revoada: { aberturaX: .34, aberturaY: .13 },
    },

    // A CINZA CAINDO. `subida` negativa = cai (o motor tira a direção do SINAL), cor morta e sem
    // `cintila`: brasa que sobe e pisca é do Folclore. É a única coisa desta cena que se move de
    // cima pra baixo, e é ela que diz que o incêndio grande foi ONTEM.
    po: { cor: '104, 88, 84', quantas: 36, subida: [-11, -3], raio: [0.6, 2.2], opacidade: [.1, .32] },
    // A NÉVOA quente rente ao chão: o ar tremendo em cima da terra que ainda está queimando.
    nevoa: { cor: '168, 46, 26', quantas: 5, deriva: [5, 17], raio: [150, 340], opacidade: [.035, .08] },
};

/// Monta a cena deste capítulo. A ORDEM É A PROFUNDIDADE — o que vem antes fica atrás.
///
/// O núcleo (`iniciarAr`) não sabe que este tema existe: ele chama `montar` e recebe as camadas
/// prontas.
export function montar({ fundo, frente, maestro }) {
    // O QUARTO dado compartilhado, e o primeiro que não é vento nem estado de peça. Três campos, e
    // três donos diferentes — ninguém escreve no campo de ninguém:
    //   pulso      · 0..1, a respiração da lava. Escrito pela Árvore do Mundo dos 🔱 Decaídos, lido
    //                pelas rachaduras e pela fumaça.
    //   raizes     · a geometria das raízes JÁ EM PIXEL DE TELA, publicada pela árvore pras
    //                rachaduras correrem em cima delas sem ter de recalcular nada.
    //   escorrendo · 0..1, se a lava está descendo. Escrito pela FENDA, lido pela árvore.
    //   tremor     · 0..1, o chão tremendo. Escrito pela fenda, lido por quem está plantado nele.
    //   jorro      · 0..1, a lava nova sendo cuspida na árvore. Fenda escreve, árvore lê.
    //   passagem   · contador de passagens dos morcegos. A COLUNA escreve, a fenda espera mudar.
    //
    // É a prova de que o maestro não era um jeito de falar de VENTO: aqui o dado é LUZ e o resto é
    // dramaturgia, e o formato é o mesmo — nasce sempre, ninguém pergunta nada a ninguém, e uma
    // camada que ignore tudo isto continua correta. Sem árvore em cena, `pulso` fica em 1 pra sempre.
    const inferno = { pulso: 1, raizes: [], escorrendo: 1, tremor: 0, jorro: 0, passagem: 0 };

    return {
        noFundo: [
            // 🔱 Os Decaídos, na ordem em que a vila é vista. A CASA e os FOGOS vêm primeiro por serem o
            // que está mais longe (moram na linha do ladrilho, com a vila arrasada). Depois a FENDA, que
            // é o CHÃO se abrindo — e por isso vem antes da árvore: as raízes passam POR CIMA dela, que é
            // o que faz a terra parecer ter rachado por baixo de uma coisa que já estava lá. A ÁRVORE é a
            // peça grande; as RACHADURAS vêm logo depois porque correm em cima das raízes que ela acabou
            // de desenhar; e a fumaça e os morcegos por último, porque são o que está no AR.
            //
            // As quatro recebem a config da árvore (e não uma cópia das medidas) pelo mesmo motivo do
            // ninja com o castelo e dos sentados com o banheiro: quem sabe onde a árvore está é a árvore.
            criarFogosDaVila(ar.fogos, fundo),
            criarBuracoDoInferno(ar.buraco, fundo, ar.arvore, inferno),
            criarArvoreDoMundo(ar.arvore, fundo, inferno),
            criarVeias(ar.veias, fundo, ar.arvore, inferno),
            criarFumacaDoInferno(ar.fumaca, fundo, ar.arvore, inferno),
            criarColunaDeMorcegos(ar.coluna, fundo, ar.arvore, inferno),
            // Os respingos vêm DEPOIS da árvore de propósito: o buraco está mais perto do que ela, e o que
            // salta dele passa na frente do tronco. Quem os move é a camada do buraco; esta só pinta.
            criarRespingosDoInferno(ar.buraco, fundo, inferno),
            criarNevoa(ar.nevoa, fundo),
        ].filter(Boolean),
        naFrente: [
            criarPo(ar.po, frente, maestro.vento, maestro.fogo),
            criarVoadores(ar.voadores, frente, maestro.vento),
        ].filter(Boolean),
    };
}
/// A geometria do CHÃO deste tema, num lugar só. O topo do ladrilho da vila é o HORIZONTE (é lá que a
/// vila arrasada está de pé) e `--vila-chao` diz onde, dentro do ladrilho, ficam os PÉS dela. Daí saem
/// a ÚNICA linha de chão da cena: a árvore, a fenda e as rachaduras estão plantadas onde a vila
/// está, e não numa terra própria mais à frente. Já esteve nas duas alturas, e o Gabriel viu na hora
/// que eram dois chãos ("está estranho, coloca a árvore no nível das casinhas") — paralaxe se conta
/// por tamanho e por cor, não por altura na tela.
///
/// Existe porque quatro peças têm de concordar nisto, e duas cópias da mesma conta divergem no meio da
/// cena. É a lição do `--mata-passo` com as corujas, e a do banheiro publicando as bordas da porta em
/// PIXEL PRONTO em vez da fração.
///
/// Mede de VEZ EM QUANDO, e não a cada quadro, pelo mesmo motivo do `criarNoHorizonte`:
/// `getComputedStyle` força layout. O que muda entre duas medidas é a janela ter cruzado uma faixa do
/// @media — e isso não acontece no meio de um gesto.
export function medidorDoChaoDaVila(canvas) {
    let medida = { linha: 0, faixa: 0 };
    let conferir = 0;

    return (dt) => {
        conferir -= dt;
        if (conferir > 0) return medida;
        conferir = 1;

        const { altura } = medirLadrilho(['--vila-passo', '--vila-altura'], 340, 220);
        // Ladrilho mais alto que a arena poria o horizonte acima do topo da tela — quem impede isso é
        // a escada de @media do estilo.css, e este `max` é só o cinto de segurança.
        const horizonte = Math.max(0, canvas.height - altura);
        const faixa = canvas.height - horizonte;
        medida = { linha: horizonte + faixa * (medirDoTema('--vila-chao', 55) / 100), faixa };
        return medida;
    };
}

/// Onde a ÁRVORE está plantada: um tanto ACIMA da linha das casas, medido na faixa do ladrilho.
///
/// Ela não fica na mesma linha que a vila e nem tem um chão próprio — as duas já foram tentadas e as
/// duas estavam erradas. Chão próprio lá embaixo dava DOIS CHÃOS na cena; exatamente na linha das
/// casas não sobrava terra na frente dela, e o terremoto abria escondido atrás do tronco e das
/// raízes. Recuada um pouco, ela ABRE a faixa de terra da frente — que é onde o Inferno rasga, à vista.
///
/// O recuo é curto de propósito: a vila é o ladrilho do CSS, que pinta SEMPRE atrás do canvas, então
/// a árvore vai por cima das casas esteja ela onde estiver. Com um recuo curto a leitura continua
/// sendo "a árvore está no meio da vila"; com um recuo grande, ela viraria uma árvore distante
/// pintada por cima do que deveria estar na frente dela — e aí o cenário mentiria sobre a ordem das
/// coisas. É o limite que a camada impõe, e ele é real.
export const linhaDaArvore = (medida, arvoreCfg) => medida.linha - medida.faixa * arvoreCfg.acima;

/// Um MEMBRO da árvore — tronco, raiz ou galho, todos a mesma peça: uma forma que sai de um ponto numa
/// direção, afina até a ponta e verga no meio. Ela ENTRA num caminho já aberto (não chama `beginPath`),
/// porque a árvore inteira é UM caminho só: é ele que é preenchido e depois vira `clip()` pra lava
/// escorrer por dentro sem vazar (o padrão do `comListras`).
///
/// E é por ser um caminho só que o SENTIDO do traço importa: subcaminhos de sentidos opostos se anulam
/// onde se sobrepõem (regra `nonzero`), e as raízes e os galhos se sobrepõem ao tronco de propósito,
/// que é como eles emendam sem fresta. Isto aqui não pode errar nisso por construção: o traço vai
/// SEMPRE pela margem do lado `+n` e volta pela do lado `−n`, e como `n` é a normal do sentido de
/// marcha, girar o membro gira a forma inteira junto — o sentido de rotação do contorno não muda com o
/// ângulo. Foi exatamente aqui que a perna do T-Rex abriu um rasgo na junta.
export function tracarMembro(ctx, x0, y0, ang, comp, r0, r1, curva) {
    const dx = Math.cos(ang), dy = Math.sin(ang);
    const nx = -dy, ny = dx;

    const xm = x0 + dx * comp * .5 + nx * curva * comp;
    const ym = y0 + dy * comp * .5 + ny * curva * comp;
    const x1 = x0 + dx * comp, y1 = y0 + dy * comp;
    const rm = (r0 + r1) * .5 * 1.15;   // a barriga do meio, pra o contorno não afundar na curva

    ctx.moveTo(x0 + nx * r0, y0 + ny * r0);
    ctx.quadraticCurveTo(xm + nx * rm, ym + ny * rm, x1 + nx * r1, y1 + ny * r1);
    ctx.lineTo(x1 - nx * r1, y1 - ny * r1);
    ctx.quadraticCurveTo(xm - nx * rm, ym - ny * rm, x0 - nx * r0, y0 - ny * r0);
    ctx.closePath();
}

/// O TREMOR do chão, em pixel, pra quem está plantado nele. Sai do maestro, então quem chama não
/// precisa saber que existe uma fenda se abrindo em algum lugar da cena — só que o chão está tremendo.
/// Duas frequências altas e primas entre si: uma só daria vibração de motor, e é o desencontro que faz
/// ler como terra batendo.
export const tremorDoChao = (inferno, canvas, t, escala) => {
    const f = (inferno?.tremor ?? 0) * escala * canvas.height;
    return { x: Math.sin(t * 47) * f, y: Math.sin(t * 39 + 1.3) * f * .55 };
};

/// Um caminho SUAVE por uma lista de pontos: quadráticas pelos pontos médios, que é o jeito barato de
/// passar por N nós sem bico em cada um. Entra num caminho já aberto, como o `tracarMembro`.
///
/// O `moveTo` fica de fora de propósito: quem chama decide se está começando um subcaminho (a
/// rachadura) ou continuando um que já vem de outro lado (a margem de volta de uma fita).
export function curvaPelosPontos(ctx, pontos) {
    for (let i = 0; i < pontos.length - 1; i++) {
        const m = { x: (pontos[i].x + pontos[i + 1].x) / 2, y: (pontos[i].y + pontos[i + 1].y) / 2 };
        ctx.quadraticCurveTo(pontos[i].x, pontos[i].y, m.x, m.y);
    }
    ctx.lineTo(pontos[pontos.length - 1].x, pontos[pontos.length - 1].y);
}

/// Uma RAIZ como FITA: as duas margens saem da normal de cada nó, e o preenchimento é um só. É a mesma
/// técnica do corpo do dragão e do tentáculo do Invasor, e ela existe pela mesma razão — uma fila de
/// elipses vira pontilhado quando a peça afina, e raiz afina até virar fio.
///
/// A NORMAL DA BASE vem do VIZINHO, não dela mesma: sem isso a corda da base gira um tantinho a cada
/// quadro e abre fresta contra o tronco (a armadilha que a fita do T-Rex já cobrou).
///
/// O sentido do traço é o mesmo do `tracarMembro` — vai pela margem `+n` e volta pela `−n` —, e é isso
/// que deixa raiz, tronco e galho conviverem num CAMINHO SÓ sem a regra `nonzero` abrir buraco onde
/// eles se sobrepõem.
export function tracarFitaDeRaiz(ctx, pontos, r0, r1) {
    const n = pontos.length;
    const normais = pontos.map((_, i) => {
        const a = pontos[Math.max(0, i - 1)], b = pontos[Math.min(n - 1, i + 1)];
        const dx = b.x - a.x, dy = b.y - a.y;
        const h = Math.hypot(dx, dy) || 1;
        return { x: -dy / h, y: dx / h };
    });
    const raio = (i) => Math.max(.3, r0 + (r1 - r0) * (i / (n - 1)));

    const margem = (lado) => pontos.map((p, i) => ({
        x: p.x + normais[i].x * raio(i) * lado,
        y: p.y + normais[i].y * raio(i) * lado,
    }));

    const ida = margem(1), volta = margem(-1).reverse();
    ctx.moveTo(ida[0].x, ida[0].y);
    curvaPelosPontos(ctx, ida);
    ctx.lineTo(volta[0].x, volta[0].y);
    curvaPelosPontos(ctx, volta);
    ctx.closePath();
}

/// A ÁRVORE DO MUNDO rachada — a peça central do capítulo e a ÚNICA fonte de luz dele.
///
/// A história: o 🧝 Elfo vendeu a vila élfica, e os demônios subiram POR DENTRO da árvore. Por isso a
/// peça central e a fonte de luz são a mesma coisa — o arranjo mais barato que uma cena pode ter, e o
/// terceiro jeito de fazê-lo neste front (no Folclore era a fogueira; nos Místicos, a lâmpada).
///
/// ELA É ENORME E FICA NO MEIO, com galhos gigantes esticando pros LADOS e as RAÍZES saindo de BAIXO
/// dela, se emaranhando em curvas bruscas pela terra até os cantos do mapa. As raízes são o pedido mais
/// específico que esta pele recebeu, e cada palavra dele virou uma regra do gerador logo abaixo.
///
/// ELA ESCREVE O MAESTRO: `pulso` (a respiração da luz) e `raizes` (o caminho de cada raiz JÁ EM PIXEL
/// DE TELA, pras rachaduras correrem em cima delas sem recalcular nada). E LÊ `escorrendo`, `jorro`,
/// `tremor` e `buraco`, que são do buraco no chão.
///
/// A LAVA É RECORTADA NA PRÓPRIA ÁRVORE (`comListras`): o caminho é montado UMA vez e serve pras duas
/// coisas — preencher a casca e recortar o que escorre por dentro. Os escorridos passam folgados da
/// borda e a própria silhueta apara, então os dois nunca podem discordar, porque são o MESMO caminho.
export function criarArvoreDoMundo(cfg, canvas, inferno) {
    const medir = medidorDoChaoDaVila(canvas);
    let t = 0;
    let jorrouAntes = 0;

    // A geometria é sorteada UMA vez e guardada NORMALIZADA, pra a árvore não trocar de forma quando a
    // janela muda de tamanho. Sortear por quadro daria uma árvore diferente a cada 16ms.
    const curvaDoTronco = (Math.random() - .5) * .06;

    // As RAÍZES. Quatro pra cada lado, e nenhuma delas é um arco: cada uma é uma lista de nós que
    // AVANÇA pro canto e MERGULHA na terra, com o desvio de cada nó sorteado em cheio — é daí que
    // saem as curvas bruscas e o emaranhado. Elas se cruzam porque cada uma escolhe uma profundidade
    // própria (`fundo`) dentro da mesma faixa estreita de chão: numa terra de ~100px, quatro caminhos
    // com fundos diferentes não têm como não passar um por cima do outro.
    const raizes = Array.from({ length: cfg.raizes }, (_, i) => {
        const lado = i % 2 ? 1 : -1;
        return {
            lado,
            alcance: entre(cfg.raizAlcance),
            fundo: entre(cfg.raizFundo),
            // O quanto cada nó sai da linha, e pra que lado. Sorteado NÓ A NÓ (e não uma senoide) —
            // senoide dá ondulação regular, que é o oposto de "curva brusca".
            desvios: Array.from({ length: cfg.raizNos }, () => (Math.random() * 2 - 1)),
            // Avanço irregular: nós apertados perto do tronco (é lá que o emaranhado aparece) e
            // esticados no fim, onde ela já é um fio correndo pro canto.
            passos: Array.from({ length: cfg.raizNos }, () => .5 + Math.random()),
        };
    });

    const galhos = Array.from({ length: cfg.galhos }, (_, i) => {
        const lado = i % 2 ? 1 : -1;
        const u = cfg.galhoDe + (Math.floor(i / 2) / Math.max(1, Math.floor(cfg.galhos / 2) - 1 || 1)) * (1 - cfg.galhoDe);
        const comp = (cfg.galhoComp[1] - (cfg.galhoComp[1] - cfg.galhoComp[0]) * u) * (.82 + Math.random() * .36);
        return {
            u: Math.min(.99, u + (Math.random() - .5) * .05),
            ang: lado > 0 ? -.16 * Math.PI + Math.random() * .3 * Math.PI
                : Math.PI + .16 * Math.PI - Math.random() * .3 * Math.PI,
            comp,
            curva: (Math.random() - .5) * .4,
            garfo: Math.random() < .7 ? {
                giro: (Math.random() < .5 ? -1 : 1) * (.1 + Math.random() * .26) * Math.PI,
                comp: comp * (.3 + Math.random() * .3),
                curva: (Math.random() - .5) * .5,
            } : null,
        };
    });

    // Os ESCORRIDOS de lava, no modelo do VENENO da ruína dos ⚙️ Tecnológicos: a CABEÇA desce primeiro,
    // a fonte fecha e a CAUDA desce atrás — a barra encurta por cima até sumir. Nada nasce nem morre no
    // meio do ar, que é o que dá física ao escorrer. O que muda pra lava: são muitos, saem de pontos
    // diferentes, alguns são bem mais grossos, alguns descem bem mais rápido, e podem se sobrepor.
    const escorridos = Array.from({ length: cfg.escorridos.quantos }, () => {
        const grande = Math.random() < cfg.escorridos.grandes;
        // O fio GROSSO desce mais devagar (peso) e o RÁPIDO é um respingo fino que dispara — as duas
        // coisas mexem no mesmo número, o ciclo, e é por isso que elas se compõem sozinhas.
        const pressa = Math.random() < cfg.escorridos.rapidos ? entre(cfg.escorridos.pressa) : 1;
        return {
            u: cfg.escorridos.de + Math.random() * (cfg.escorridos.ate - cfg.escorridos.de),
            desvio: (Math.random() - .5) * 1.8,
            // Onde ele CAI dentro do buraco, de -1 (borda esquerda) a 1 (direita). Sem isto os fios
            // caem todos no mesmo palmo, porque a boca é bem mais larga que o tronco de onde eles
            // saem — e lava jorrando num ponto só de um buraco largo lê como torneira.
            alvo: (Math.random() * 2 - 1),
            ciclo: entre(cfg.escorridos.ciclo) * (grande ? 1.35 : 1) * pressa,
            atraso: Math.random() * 8,
            larg: entre(cfg.escorridos.larg) * (grande ? cfg.escorridos.engrossa : 1),
            // Perto de UMA volta inteira na descida: é isso que desenha o S. Menos que isso vira
            // uma curva só (uma vírgula), mais vira zigue-zague de mola.
            onda: .72 + Math.random() * .56,
            serpente: entre(cfg.escorridos.serpente),
            fase: Math.random() * Math.PI * 2,
            // Ninguém nasce escorrendo: a cena ABRE com a árvore seca, e a lava só desce depois que o
            // chão treme pela primeira vez. É o buraco que manda (ver `criarBuracoDoInferno`), e é o
            // que dá ao primeiro terremoto a função de ACENDER a cena em vez de só sacudi-la.
            ativo: false,
        };
    });

    return (ctx, dt) => {
        t += dt;

        const medida = medir(dt);
        const chao = linhaDaArvore(medida, cfg);
        const A = canvas.height * cfg.altura;
        const L = A * cfg.largura * .5;         // meia-largura do tronco na base
        const cx = canvas.width * cfg.x;

        // A RESPIRAÇÃO, escrita no maestro. Duas senoides fora de compasso, porque uma só vira
        // metrônomo. O `clamp` é o que garante que quem lê receba sempre 0..1: do lado de lá isto vira
        // alfa e largura de traço, e um valor fora da faixa chegaria como cor inválida (silenciosa) ou
        // raio negativo (fatal), longe de quem o produziu.
        const respiro = Math.sin(t * cfg.respiro[0]) * .6 + Math.sin(t * cfg.respiro[1] + 1.7) * .4;
        const jorro = inferno.jorro ?? 0;
        const pulso = Math.min(1, Math.max(0, cfg.pulso[0] + (cfg.pulso[1] - cfg.pulso[0]) * (respiro + 1) * .5 + jorro * .5));
        inferno.pulso = pulso;

        // Onde cada fio está no ciclo dele, 0..1. Abaixo de .9 ele está DESCENDO (45% do ciclo a
        // cabeça descendo, 45% a cauda alcançando); daí pra cima é a pausa seca, em que ele não
        // aparece na tela.
        const faseDoFio = (e) => ((((t + e.atraso) % e.ciclo) + e.ciclo) % e.ciclo) / e.ciclo;
        const descendo = (e) => e.ativo && faseDoFio(e) < .9;

        // A ERUPÇÃO acorda quem está PARADO — e só. Ela já reiniciou todos de uma vez, e o que se via
        // era um fio no meio da descida voltar pro topo no mesmo quadro: lava sumindo do nada. Nada
        // nesta cena pode desaparecer no meio do caminho; o que a erupção faz é NASCER MAIS.
        // Detecção de BORDA (e não do valor), senão isto rodaria a cada quadro enquanto ela durasse.
        if (jorro > .5 && jorrouAntes <= .5) {
            for (const e of escorridos) {
                if (descendo(e)) continue;
                e.ativo = true;
                e.atraso = -t + Math.random() * .6;
            }
        }
        jorrouAntes = jorro;

        // O ponto do EIXO do tronco na altura `u` — a mesma quadrática que o `tracarMembro` desenha,
        // avaliada. Base e topo ficam no mesmo x; quem entorta o tronco é só o ponto de controle.
        //
        // O `+` tem de bater com o `nx * curva * comp` do `tracarMembro` com `ang = -π/2` (ali a
        // normal vale (1, 0)). Nasceu com o sinal trocado, e o defeito seria MUDO: os galhos ficariam
        // pendurados no ar a até 29px do tronco, longe o bastante pra ver e perto o bastante pra
        // parecer que a árvore inteira é que está torta.
        const noTronco = (u) => {
            const xm = cx + curvaDoTronco * A, ym = chao - A * .5;
            const k = 1 - u;
            return {
                x: k * k * cx + 2 * k * u * xm + u * u * cx,
                y: k * k * chao + 2 * k * u * ym + u * u * (chao - A),
            };
        };

        // O CAMINHO de cada raiz, em pixel de tela. Três regras, e as três são pedido do Gabriel:
        //
        //   1. sai de BAIXO da árvore — o primeiro nó fica no eixo do tronco, escondido por ele, e é
        //      isso que faz a raiz parecer nascer debaixo da árvore em vez de brotar do lado dela.
        //   2. NUNCA sobe pra área das casas — do segundo nó em diante o y é preso abaixo da linha da
        //      vila. Raiz que sobe ali passaria por cima do ladrilho e viraria raiz voando no
        //      horizonte, que é a leitura errada de uma cena que tem UM chão só.
        //   3. termina no CANTO — o alcance é fração da meia-largura da tela (1 = a borda), e alguns
        //      passam de 1 de propósito: raiz que morre um palmo antes do canto lê como raiz cortada.
        const terra = Math.max(1, canvas.height - medida.linha);
        const caminhoDaRaiz = (r) => {
            const total = r.passos.reduce((a, b) => a + b, 0);
            const alvo = canvas.width * .5 * r.alcance;
            const fundoY = medida.linha + terra * r.fundo;
            const pontos = [{ x: cx, y: chao }];
            let andou = 0;
            for (let k = 0; k < r.passos.length; k++) {
                andou += r.passos[k];
                const u = andou / total;
                // a descida acontece CEDO (raiz de árvore mergulha logo depois de sair do tronco) e
                // depois ela corre quase deitada — daí a raiz de u, que sobe rápido e achata.
                const desce = Math.sqrt(u);
                const y = chao + (fundoY - chao) * desce + r.desvios[k] * terra * cfg.raizOnda * (1 - u * .55);
                pontos.push({
                    x: cx + r.lado * alvo * u,
                    // o `max` é a regra 2, e vale do 2º nó em diante porque o 1º está debaixo do tronco
                    y: Math.min(canvas.height - 2, Math.max(medida.linha + terra * .06, y)),
                });
            }
            return pontos;
        };

        const caminhosDasRaizes = raizes.map(caminhoDaRaiz);

        // Publicado no maestro em PIXEL PRONTO: as rachaduras só percorrem a lista, sem ter uma
        // segunda opinião sobre onde a raiz está. É a lição do banheiro com as bordas da porta.
        inferno.raizes = caminhosDasRaizes;

        // E o EIXO DO TRONCO, pelo mesmo motivo e no mesmo formato: as rachaduras que sobem na árvore
        // andam por ele. Vai com a MEIA-LARGURA em cada altura (`r`), que é o que permite a rachadura
        // encostar na casca em vez de subir cravada no meio — a largura é da árvore, e quem sobe nela
        // não pode ter uma segunda opinião sobre onde a borda está.
        inferno.tronco = Array.from({ length: 16 }, (_, i) => {
            const u = i / 15;
            const p = noTronco(u);
            return { u, x: p.x, y: p.y, r: L * (1 - .74 * u) };
        });

        const sacode = tremorDoChao(inferno, canvas, t, cfg.tremor);

        const caminho = () => {
            ctx.beginPath();
            tracarMembro(ctx, cx, chao, -Math.PI / 2, A, L, L * .26, curvaDoTronco);
            for (const pontos of caminhosDasRaizes)
                tracarFitaDeRaiz(ctx, pontos, L * cfg.raizGrossa, L * cfg.raizFina);
            for (const g of galhos) {
                const p = noTronco(g.u);
                const raiz = L * (.4 - g.u * .26);
                tracarMembro(ctx, p.x, p.y, g.ang, A * g.comp, raiz, raiz * .16, g.curva);
                if (g.garfo) {
                    const px = p.x + Math.cos(g.ang) * A * g.comp;
                    const py = p.y + Math.sin(g.ang) * A * g.comp;
                    tracarMembro(ctx, px, py, g.ang + g.garfo.giro, A * g.garfo.comp, raiz * .2, raiz * .06, g.garfo.curva);
                }
            }
        };

        ctx.save();
        ctx.translate(sacode.x, sacode.y);

        // 1. o CLARÃO. Ele é o que põe a árvore DENTRO da cena em vez de deixá-la colada em cima —
        //    mesmo papel do clarão da lâmpada na praia, e mesma razão pra vir antes de tudo: o tronco
        //    tem de ser recortado contra a própria luz.
        //
        //    A POÇA de luz que se espalhava no chão ao pé dela SAIU (pedido do Gabriel: "remove aquela
        //    luz que você joga lateralmente, deixa só o fogo pra cima"). Ele está certo e a razão é
        //    física: a lava não fica mais parada ali embaixo — ela cai dentro do buraco, e quem
        //    ilumina agora é o fogo que SOBE de dentro dele.
        const brilho = inferno.brilho ?? 0;
        const alvoDaLuz = chao - A * .22;
        // Acesa, ela ilumina MAIS e mais LONGE: o clarão cresce junto com o brilho, senão a árvore
        // acenderia sem que a cena em volta soubesse disso.
        const raio = A * cfg.clarao * .4 * (.9 + pulso * .1) * (1 + brilho * .3);
        const clarao = ctx.createRadialGradient(cx, alvoDaLuz, 0, cx, alvoDaLuz, raio);
        clarao.addColorStop(0, `rgba(${cfg.luz}, ${Math.min(1, .28 * pulso + brilho * .42)})`);
        clarao.addColorStop(.38, `rgba(${cfg.luz}, ${Math.min(1, .1 * pulso + brilho * .18)})`);
        clarao.addColorStop(1, `rgba(${cfg.luz}, 0)`);
        ctx.fillStyle = clarao;
        ctx.beginPath();
        ctx.arc(cx, alvoDaLuz, raio, 0, Math.PI * 2);
        ctx.fill();

        // Onde a lava VAI PARAR: o fundo do buraco, e não o pé do tronco. É por isso que o fio é
        // calculado aqui fora e desenhado em dois pedaços logo abaixo — o de cima recortado na
        // árvore, o de baixo solto no ar entre ela e a boca do buraco. Sem buraco em cena (ou com ele
        // acima do pé, que não acontece), a lava termina no chão como antes.
        // O fundo desce um tanto da profundidade da boca: o fio tem de terminar DENTRO da lava que
        // está lá embaixo, e não na linha da borda. Parando na borda, a cabeça ficava pousada em cima
        // do buraco em vez de mergulhar nele.
        const alvoDaLava = (inferno.buraco && inferno.buraco.y > chao)
            ? inferno.buraco.y + inferno.buraco.funda * cfg.escorridos.mergulha
            : chao;

        const fios = [];
        for (const e of escorridos) {
            // Quem está PARADO só volta quando o buraco manda escorrer de novo — e é isto que
            // acende a cena na primeira vez, porque todos nascem parados. O `continue` antigo
            // pulava o fio inativo antes de qualquer conferência, então nada além da erupção
            // conseguia acordá-lo: o fogo só podia nascer uma fase DEPOIS do tremor.
            //
            // O atraso sorteado é o que impede os doze de largarem no mesmo quadro, que leria
            // como um interruptor sendo ligado.
            if (!e.ativo) {
                if ((inferno.escorrendo ?? 1) > .5) { e.ativo = true; e.atraso = -t + Math.random() * 1.4; }
                continue;
            }

            const fase = faseDoFio(e);
            // 45% do ciclo a cabeça descendo, 45% a cauda alcançando, 10% de pausa seca.
            const cabeca = Math.min(1, fase / .45);
            const cauda = Math.max(0, Math.min(1, (fase - .45) / .45));
            // Na PAUSA SECA é que ele decide se recomeça. É por isso que a lava não some de uma vez
            // quando o buraco manda secar: cada fio termina a descida dele e só então fica parado.
            if (cauda >= 1) { e.ativo = (inferno.escorrendo ?? 1) > .5; continue; }

            const p = noTronco(e.u);
            const x0 = p.x + e.desvio * L;
            const y0 = p.y;
            const queda = Math.max(1, alvoDaLava - y0);
            const yCauda = y0 + queda * cauda;
            const yCabeca = y0 + queda * cabeca;
            if (yCabeca - yCauda < .5) continue;      // fio de comprimento zero não tem o que pintar
            const w = L * e.larg;

            // A largura do que sobrou da boca do buraco pra este fio mirar. Zero sem buraco em cena.
            const meiaDaBoca = (inferno.buraco?.meia ?? 0) * cfg.escorridos.espalha;
            const abaixoDoChao = Math.max(1, alvoDaLava - chao);

            fios.push({
                x0, yCauda, yCabeca, w,
                // O caminho lateral do fio soma duas coisas: o S que ele faz o tempo todo, e um
                // desvio que só existe DEPOIS do pé da árvore — é ele que abre o leque e faz cada um
                // cair num ponto diferente da boca. Em cima do tronco o leque vale zero, senão a lava
                // sairia da casca antes de chegar no chão.
                desviaEm: (y) => {
                    const serpente = Math.sin((y - y0) / queda * e.onda * Math.PI * 2 + e.fase) * L * e.serpente;
                    if (y <= chao) return serpente;
                    const p = Math.min(1, (y - chao) / abaixoDoChao);
                    return serpente + p * p * (3 - 2 * p) * e.alvo * meiaDaBoca;
                },
                // Onde o pé da árvore corta este fio, em fração do comprimento dele. É o número que
                // divide o desenho em dois: <= 0 quer dizer que ele já está inteiro fora da árvore,
                // >= 1 que ele ainda não chegou no chão.
                kNoChao: Math.min(1, Math.max(0, (chao - yCauda) / (yCabeca - yCauda))),
            });
        }

        // Um pedaço de fio, de `k0` a `k1` do comprimento dele. A CABEÇA arredondada só entra no
        // pedaço que termina na ponta — é ela que separa "líquido descendo" de "traço de caneta", e
        // desenhá-la no meio do fio poria uma bolha no lugar errado.
        const desenharFio = (f, k0, k1, comCabeca) => {
            if (k1 - k0 <= .001) return;
            const passos = 8;
            const ponto = (k) => {
                const y = f.yCauda + (f.yCabeca - f.yCauda) * k;
                return { x: f.x0 + f.desviaEm(y), y, w: f.w * (.42 + .58 * k) };
            };

            const tinta = ctx.createLinearGradient(f.x0, f.yCauda, f.x0, f.yCabeca);
            tinta.addColorStop(0, `rgba(${cfg.luz}, 0)`);
            tinta.addColorStop(.25, cfg.lava);
            tinta.addColorStop(1, cfg.lavaQuente);
            ctx.fillStyle = tinta;
            ctx.globalAlpha = .55 + pulso * .45;

            ctx.beginPath();
            for (let i = 0; i <= passos; i++) {
                const q = ponto(k0 + (k1 - k0) * (i / passos));
                i ? ctx.lineTo(q.x - q.w, q.y) : ctx.moveTo(q.x - q.w, q.y);
            }
            const fim = ponto(k1);
            if (comCabeca) ctx.quadraticCurveTo(fim.x, fim.y + fim.w * 1.5, fim.x + fim.w, fim.y);
            else ctx.lineTo(fim.x + fim.w, fim.y);
            for (let i = passos - 1; i >= 0; i--) {
                const q = ponto(k0 + (k1 - k0) * (i / passos));
                ctx.lineTo(q.x + q.w, q.y);
            }
            ctx.closePath();
            ctx.fill();
            ctx.globalAlpha = 1;
        };

        // 2. a ÁRVORE, e a lava DENTRO dela. Um caminho, dois usos.
        comListras(ctx, caminho, cfg.casca, () => {
            // a casca não é chapada: o miolo do tronco pega a luz que sobe de dentro dele
            const volume = ctx.createLinearGradient(cx - L, 0, cx + L, 0);
            volume.addColorStop(0, cfg.casca);
            volume.addColorStop(.5, cfg.cascaLuz);
            volume.addColorStop(1, cfg.casca);
            ctx.fillStyle = volume;
            ctx.globalAlpha = .5;
            ctx.fillRect(cx - L * 1.2, chao - A, L * 2.4, A);
            ctx.globalAlpha = 1;

            // A ÁRVORE INTEIRA ACESA. Um retângulo por cima do recorte pinta tronco, raízes e galhos
            // de uma vez — é a mesma vantagem que o `comListras` já dava à lava, e a razão de a árvore
            // ser UM caminho só. Ele tem de cobrir o mundo todo porque as raízes passam das bordas.
            if (brilho > .01) {
                const acesa = ctx.createLinearGradient(0, chao - A, 0, canvas.height);
                acesa.addColorStop(0, cfg.lava);
                acesa.addColorStop(.65, cfg.lavaQuente);
                acesa.addColorStop(1, cfg.lava);
                ctx.fillStyle = acesa;
                ctx.globalAlpha = Math.min(1, brilho * .62);
                ctx.fillRect(-canvas.width, chao - A - 20, canvas.width * 3, canvas.height * 2);
                ctx.globalAlpha = 1;
            }

            // A GRETA acesa no pé do tronco SAIU: era lava que caía e FICAVA ali, parada. É a mesma
            // razão que juntou a poça com o buraco — lava parada em dois lugares é a mesma coisa
            // contada duas vezes. O que desce agora não para no pé da árvore.
            //
            // Aqui entra só o pedaço do fio que ainda está DENTRO da árvore; o resto é pintado logo
            // depois, fora do recorte. A emenda é exata porque os dois pedaços saem do mesmo `k`.
            for (const f of fios) desenharFio(f, 0, f.kNoChao, f.kNoChao >= 1);
        });

        // 3. o RESTO DO MESMO FIO: do pé da árvore até dentro do buraco, agora fora do recorte,
        //    porque este pedaço já não está na árvore — está no ar entre ela e a boca.
        //
        //    Aqui morreu o VERTEDOURO, que eram três fios fixos ligando as duas peças. Eles cumpriam
        //    a função (a lava chegava no buraco) e mentiam sobre ela: eram sempre três, sempre
        //    acesos e sempre iguais, enquanto o que descia pelo tronco eram dezoito fios de ritmos e
        //    grossuras diferentes. O Gabriel viu isso na hora — "o que é esse negócio que desce 3
        //    lava dali?". Um fio só, contínuo, do alto da árvore até o fundo do buraco, não tem como
        //    divergir do que está acontecendo em cima: é a mesma peça.
        for (const f of fios) {
            desenharFio(f, f.kNoChao, 1, true);

            // O BATE: enquanto a cabeça está no fundo, um brilho curto no ponto exato em que ela
            // entra. É o que faltava pra a queda TERMINAR em algum lugar — sem ele a lava chegava na
            // boca e simplesmente parava de existir, que é o que se via de errado.
            if (f.yCabeca >= alvoDaLava - 1) {
                const px = f.x0 + f.desviaEm(alvoDaLava);
                const raio = f.w * 3.4;
                const bate = ctx.createRadialGradient(px, alvoDaLava, 0, px, alvoDaLava, raio);
                bate.addColorStop(0, `rgba(${cfg.luz}, ${.5 + pulso * .3})`);
                bate.addColorStop(1, `rgba(${cfg.luz}, 0)`);
                ctx.fillStyle = bate;
                ctx.beginPath();
                // achatado: é luz batendo na superfície da poça, não uma bola no ar
                ctx.ellipse(px, alvoDaLava, raio, raio * .45, 0, 0, Math.PI * 2);
                ctx.fill();
            }
        }

        ctx.restore();
    };
}

/// As RACHADURAS, correndo POR CIMA DAS RAÍZES — e só. O Gabriel foi explícito sobre isso: "tava massa
/// com as linhas de rachadura apenas seguindo elas SEM SAIR DA RAIZ". Os ramos que se abriam pro lado
/// saíram; o que sobra é a lava vista por dentro da própria raiz, que é o que a cena está contando (o
/// que queima por dentro é a árvore, e a terra só racha onde ela chegou).
///
/// Ela não calcula raiz nenhuma: percorre a lista de pontos que a árvore publicou no maestro, já em
/// pixel de tela. Depois que a raiz virou um caminho emaranhado, isso deixou de ser economia e virou a
/// única saída sã — refazer o mesmo emaranhado aqui seria duas cópias de um gerador aleatório, que
/// nem com a mesma semente dariam o mesmo desenho.
///
/// A geometria é PARADA e a luz é que anda: as gretas são as raízes, que não se mexem, e o que muda por
/// quadro é o brilho — que lê o pulso e ainda tem uma ondinha própria por rachadura, senão todas
/// acenderiam juntas e a terra inteira piscaria.
export function criarVeias(cfg, canvas, arvoreCfg, inferno) {
    let t = 0;

    const perRaiz = Array.from({ length: 24 }, () => ({
        de: Math.random() * .14,
        ate: .76 + Math.random() * .24,
        fase: Math.random() * Math.PI * 2,
        brilho: .55 + Math.random() * .45,
    }));

    // As rachaduras QUE APARECEM NA ÁRVORE são peças SOLTAS, e não a continuação das do chão. Já
    // foram a continuação: a raiz emendava no tronco e subia, e o que se via era exatamente a EMENDA
    // — dois traços de sentidos diferentes grudados num ponto, que o olho lê como um só desenho mal
    // costurado. Soltas, cada uma é uma greta na casca e ninguém procura de onde ela veio. É a mesma
    // razão pela qual as raízes emendam no tronco por RECOBRIMENTO e não por encaixe.
    const naArvore = Array.from({ length: cfg.naArvore }, () => ({
        de: Math.random() * .52,                 // a altura do tronco em que ela começa
        comp: .08 + Math.random() * .26,         // e o quanto ela sobe dali
        // O passeio dela pela largura do tronco. Passando de meia volta ela cruza pro outro lado e
        // abraça a árvore; o `abraco` é o quanto ela chega perto da casca.
        volta: .5 + Math.random() * 1.4,
        giro: Math.random() * Math.PI * 2,
        lado: Math.random() < .5 ? -1 : 1,
        fase: Math.random() * Math.PI * 2,
        brilho: .5 + Math.random() * .5,
    }));

    return (ctx, dt) => {
        t += dt;

        const raizes = inferno.raizes ?? [];
        const tronco = inferno.tronco ?? [];
        if (!raizes.length) return;      // o 1º quadro roda antes de a árvore publicar; não é erro

        const largura = canvas.height * cfg.largura;
        const sacode = tremorDoChao(inferno, canvas, t, arvoreCfg.tremor);

        ctx.save();
        ctx.translate(sacode.x, sacode.y);
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';

        for (let i = 0; i < raizes.length; i++) {
            const pontos = raizes[i];
            const k = perRaiz[i % perRaiz.length];
            const brilho = Math.min(1, Math.max(0, (inferno.pulso ?? 1) * k.brilho * (.72 + .28 * Math.sin(t * 1.7 + k.fase))));

            // A rachadura não começa no tronco nem vai até a ponta: um pedaço do meio da raiz, que é
            // como pedra abre de verdade. `de`/`ate` são fatias da lista de nós.
            const dePonto = Math.max(1, Math.floor(pontos.length * k.de));
            const atePonto = Math.max(dePonto + 2, Math.ceil(pontos.length * k.ate));
            const trecho = pontos.slice(dePonto, atePonto);
            if (trecho.length < 2) continue;

            const tracar = () => {
                ctx.beginPath();
                ctx.moveTo(trecho[0].x, trecho[0].y);
                curvaPelosPontos(ctx, trecho);
            };

            // Três passadas: a larga é o CALOR saindo da fresta (é ela que ilumina a terra em volta), a
            // do meio é a lava, a fina é o fio quente. Uma passada só dá ou risco duro ou mancha sem borda.
            tracar();
            ctx.strokeStyle = `rgba(${cfg.cor}, ${.26 * brilho})`;
            ctx.lineWidth = largura * 3.6;
            ctx.stroke();

            tracar();
            ctx.strokeStyle = `rgba(${cfg.cor}, ${.72 * brilho})`;
            ctx.lineWidth = largura;
            ctx.stroke();

            tracar();
            ctx.strokeStyle = `rgba(${cfg.quente}, ${.58 * brilho})`;
            ctx.lineWidth = largura * .36;
            ctx.stroke();
        }

        // As gretas NA CASCA. Mesmas três passadas (calor, lava, fio quente), outro caminho: um
        // pedaço do eixo do tronco, passeando pela largura dele. Como o tronco é publicado com a
        // meia-largura em cada altura, elas afinam junto com ele sem saber que estão afinando.
        for (const g of naArvore) {
            const pontos = tronco.filter(p => p.u >= g.de && p.u <= g.de + g.comp);
            if (pontos.length < 2) continue;

            const brilho = Math.min(1, Math.max(0, (inferno.pulso ?? 1) * g.brilho * (.72 + .28 * Math.sin(t * 1.7 + g.fase))));
            const caminho = pontos.map((p, i) => {
                const q = i / (pontos.length - 1);
                return { x: p.x + g.lado * p.r * cfg.abraco * Math.sin(q * Math.PI * g.volta + g.giro), y: p.y };
            });

            const tracar = () => {
                ctx.beginPath();
                ctx.moveTo(caminho[0].x, caminho[0].y);
                curvaPelosPontos(ctx, caminho);
            };

            tracar();
            ctx.strokeStyle = `rgba(${cfg.cor}, ${.22 * brilho})`;
            ctx.lineWidth = largura * 2.8;
            ctx.stroke();

            tracar();
            ctx.strokeStyle = `rgba(${cfg.cor}, ${.66 * brilho})`;
            ctx.lineWidth = largura * .8;
            ctx.stroke();

            tracar();
            ctx.strokeStyle = `rgba(${cfg.quente}, ${.52 * brilho})`;
            ctx.lineWidth = largura * .3;
            ctx.stroke();
        }

        ctx.restore();
    };
}

/// O BURACO NA TERRA — onde a lava cai, e por onde o Inferno entra em erupção.
///
/// Ele nasceu como duas peças e virou uma, e a correção é do Gabriel: *"a verdade é que isso aí já É o
/// buraco na terra, não precisávamos criar outro, era só acrescentar a erupção direto ali"*. Havia a
/// poça de lava ao pé da árvore (onde os escorridos caíam) e havia uma fenda que abria e fechava a
/// alguns metros dela — duas bocas de inferno na mesma cena, e nenhuma explicando a outra. Agora é UMA:
/// o buraco SEMPRE existiu, a lava da árvore cai direto nele, e o que o ciclo acrescenta é a ERUPÇÃO.
///
/// É também o DIRETOR da cena, e o roteiro é dele:
///
///   os morcegos passam → a lava PARA de escorrer (o resto segue pulsando) → um momento de nada
///   → o chão TREME → o buraco se escancara e CUSPE lava pra cima → treme de novo → se fecha
///
/// Duas coisas fazem isso funcionar, e as duas já estavam no manual. O AVISO: o chão treme ANTES, como
/// a moita treme antes de o Oni subir. E a PAUSA: a lava secando é o que transforma o abrir em susto,
/// porque a cena parece ter acabado. "Do nada", que foi como ele descreveu.
///
/// A luz dele vai toda pra CIMA (pedido dele também: "remove aquela luz que você joga lateralmente,
/// deixa só o fogo pra cima"). Não há mais elipse de clarão espalhada no chão: o que se vê é a coluna
/// quente subindo da boca, e ela diz a mesma coisa sem chapar a terra em volta.
///
/// Ele não CHAMA ninguém: escreve `escorrendo`, `tremor`, `jorro` e `buraco` no maestro, e lê
/// `passagem` (que a coluna de morcegos incrementa ao estourar).
export function criarBuracoDoInferno(cfg, canvas, arvoreCfg, inferno) {
    const medir = medidorDoChaoDaVila(canvas);
    let fase = 'dormindo';
    let relogio = 0;
    let q = 0;
    let t = 0;
    let vistas = 0;          // quantas passagens de morcego já foram consumidas
    let primeira = true;     // a 1ª erupção é encurtada: ver o comentário das esperas
    let respingos = [];

    // Começa SECO e APAGADO. Quem acende é a erupção: os respingos sobem, batem na árvore, ela
    // inteira brilha, e é o brilho que traz a lava de volta.
    inferno.escorrendo = 0;
    inferno.tremor = 0;
    inferno.jorro = 0;
    inferno.brilho = 0;

    return (ctx, dt) => {
        t += dt;
        relogio -= dt;

        // O brilho APAGA sozinho, sempre, em qualquer fase — quem o acende é a erupção, lá embaixo, e
        // quem o apaga é o tempo. Por isso ele vive fora da máquina de fases: um "apagar" que fosse
        // fase seria cortado no meio pela próxima passagem de morcegos.
        inferno.brilho = Math.max(0, (inferno.brilho ?? 0) - dt / cfg.brilhoDura);

        const { linha } = medir(dt);
        const chao = linha + (canvas.height - linha) * cfg.linha;
        const cx = canvas.width * arvoreCfg.x;

        switch (fase) {
            case 'dormindo':
                // Espera os morcegos. O gatilho é o CONTADOR mudar, e não um flag ligado/desligado:
                // flag exigiria alguém desligá-lo, e aí duas peças seriam donas do mesmo estado.
                if ((inferno.passagem ?? 0) !== vistas) {
                    vistas = inferno.passagem ?? 0;
                    fase = 'secando';
                    relogio = primeira ? cfg.depois[0] * .5 : entre(cfg.depois);
                }
                break;
            case 'secando':
                inferno.escorrendo = Math.max(0, inferno.escorrendo - dt / cfg.secar);
                if (relogio <= 0 && inferno.escorrendo <= 0) { fase = 'seca'; relogio = primeira ? cfg.seco[0] * .6 : entre(cfg.seco); }
                break;
            case 'seca':
                if (relogio <= 0) { fase = 'avisando'; q = 0; }
                break;
            case 'avisando':
                q += dt / cfg.avisar;
                inferno.tremor = q * q;                 // o tremor CRESCE: começa quase nada e vira pânico
                // É O TREMOR que manda a lava voltar a descer — e, na primeira vez, que a manda
                // COMEÇAR: a batalha abre com a árvore seca e escura, e é o chão sacudindo que a
                // acende. Estava em 'jorrando', o que atrasava a lava em um gesto inteiro.
                if (q >= 1) { fase = 'jorrando'; q = 0; primeira = false; }
                break;
            case 'jorrando':
                q += dt / cfg.jorrar;
                inferno.jorro = Math.sin(Math.min(1, q) * Math.PI);
                inferno.tremor = .8 + Math.sin(q * Math.PI) * .2;
                // O BRILHO sobe depois que os respingos já estão no ar (daí o `q - .2`): eles saem,
                // sobem, alcançam a árvore, e ela acende. Sobe rápido porque pegar fogo é rápido; é o
                // apagar que é lento, e ele mora fora daqui.
                inferno.brilho = Math.max(inferno.brilho, Math.min(1, (q - .2) / .35));
                // E a LAVA só volta depois que ela acendeu. Esta é a ordem inteira em uma linha: o
                // buraco cospe, a árvore pega fogo, a árvore escorre.
                if (inferno.brilho >= .6) inferno.escorrendo = 1;
                if (q >= 1) { fase = 'acalmando'; q = 0; inferno.jorro = 0; }
                break;
            case 'acalmando':
                // O tremor CAI até parar, e é só isso que fecha o ciclo. Não há mais o que fechar: o
                // buraco não abriu, ele tremeu — quem some é o abalo, não a boca.
                q += dt / cfg.acalmar;
                inferno.tremor = Math.max(0, 1 - q);
                if (q >= 1) { fase = 'dormindo'; inferno.tremor = 0; }
                break;
        }

        // O tamanho é FIXO: ele não cresce na erupção, apenas treme (pedido do Gabriel). Buraco que
        // incha e murcha lê como boca de bicho; buraco que treme lê como terra aguentando o que vem de
        // baixo — e o susto continua inteiro, porque quem o carrega é o tremor e o que salta de dentro.
        // Publicado no maestro pra árvore saber onde despejar a lava dela.
        const meia = canvas.width * cfg.largura * .5;
        const funda = Math.max(1, medir(dt).faixa * cfg.funda);
        inferno.buraco = { x: cx, y: chao, meia, funda };

        // Os RESPINGOS da erupção: lava cuspida pra cima, que cai de volta. Nascem só enquanto ele
        // jorra — avançar PRIMEIRO e descartar depois, senão a partícula que morre neste quadro ainda
        // é desenhada com o raio já negativo, e raio negativo LANÇA.
        if (fase === 'jorrando' && respingos.length < cfg.respingos) {
            for (let i = 0; i < 5; i++) {
                const lado = Math.random() < .5 ? -1 : 1;
                respingos.push({
                    x: cx + lado * Math.random() * meia,
                    y: chao,
                    vx: lado * Math.random() * canvas.width * .04,
                    vy: -(.5 + Math.random() * .8) * canvas.height * cfg.forca,
                    r: canvas.height * (.002 + Math.random() * .005),
                    vida: 1,
                });
            }
        }
        for (const s of respingos) {
            s.vida -= dt * .5;
            s.x += s.vx * dt;
            s.y += s.vy * dt;
            s.vy += canvas.height * 1.1 * dt;          // gravidade
        }
        respingos = respingos.filter(s => s.vida > 0 && s.y < chao + canvas.height * .05);
        inferno.respingos = respingos;

        const sacode = tremorDoChao(inferno, canvas, t, arvoreCfg.tremor);
        const pulso = inferno.pulso ?? 1;

        ctx.save();
        ctx.translate(sacode.x, sacode.y);

        // 1. a BOCA: um losango de pontas finas, e não uma elipse — buraco que a terra abriu tem ponta,
        //    e é a ponta que diz que ela RASGOU em vez de afundar.
        const boca = (escala) => {
            ctx.beginPath();
            ctx.moveTo(cx - meia * escala, chao);
            ctx.quadraticCurveTo(cx - meia * escala * .4, chao - funda * escala, cx, chao - funda * escala * .55);
            ctx.quadraticCurveTo(cx + meia * escala * .4, chao - funda * escala, cx + meia * escala, chao);
            ctx.quadraticCurveTo(cx + meia * escala * .4, chao + funda * escala, cx, chao + funda * escala * .55);
            ctx.quadraticCurveTo(cx - meia * escala * .4, chao + funda * escala, cx - meia * escala, chao);
            ctx.closePath();
        };

        boca(1);
        ctx.fillStyle = cfg.cor;
        ctx.fill();

        // 2. a LAVA no fundo dele, menor que a boca: é a distância entre as duas que dá PROFUNDIDADE —
        //    lava encostando na borda leria como uma poça pintada no chão.
        const dentro = ctx.createLinearGradient(cx - meia, chao, cx + meia, chao);
        dentro.addColorStop(0, cfg.lava);
        dentro.addColorStop(.5, cfg.lavaQuente);
        dentro.addColorStop(1, cfg.lava);
        ctx.fillStyle = dentro;
        ctx.globalAlpha = .5 + pulso * .5;
        boca(.66);
        ctx.fill();
        ctx.globalAlpha = 1;

        // 3. o FOGO PRA CIMA, e nenhuma luz pro lado. Uma coluna que nasce larga na boca e some antes
        //    de chegar na copa: é ela que ilumina a cena de baixo agora que a poça no chão saiu.
        const alto = canvas.height * cfg.chama * (.8 + pulso * .2 + (inferno.jorro ?? 0) * .8);
        const coluna = ctx.createLinearGradient(0, chao + funda, 0, chao - alto);
        coluna.addColorStop(0, `rgba(${cfg.luz}, ${.34 * pulso})`);
        coluna.addColorStop(.35, `rgba(${cfg.luz}, ${.16 * pulso})`);
        coluna.addColorStop(1, `rgba(${cfg.luz}, 0)`);
        ctx.fillStyle = coluna;
        ctx.beginPath();
        ctx.moveTo(cx - meia, chao + funda * .4);
        // as duas laterais sobem FECHANDO: fogo que sobe se estreita, e é o estreitar que dá a
        // leitura de coluna em vez de cortina
        ctx.quadraticCurveTo(cx - meia * .8, chao - alto * .5, cx - meia * .18, chao - alto);
        ctx.lineTo(cx + meia * .18, chao - alto);
        ctx.quadraticCurveTo(cx + meia * .8, chao - alto * .5, cx + meia, chao + funda * .4);
        ctx.closePath();
        ctx.fill();

        ctx.restore();
    };
}

/// Os RESPINGOS da erupção, pintados DEPOIS da árvore — eles passam NA FRENTE dela.
///
/// É a única peça do tema que fura a ordem das camadas, e ela tem razão física pra isso: o buraco está
/// mais PERTO do que a árvore (mais embaixo na terra), então o que salta de dentro dele cruza a vista
/// por cima do tronco, e não por trás. Enquanto eram pintados junto com o buraco, o tronco engolia
/// justamente o auge da erupção.
///
/// Quem MOVE as gotas continua sendo o buraco, que é o dono do estado; esta camada só PINTA o que
/// encontra publicado no maestro. É a mesma divisão das raízes com as rachaduras — uma peça calcula,
/// outra desenha, e nenhuma das duas tem uma segunda opinião sobre a primeira.
export function criarRespingosDoInferno(cfg, canvas, inferno) {
    return (ctx) => {
        for (const s of inferno.respingos ?? []) {
            const viva = Math.max(0, Math.min(1, s.vida));
            const vel = Math.hypot(s.vx, s.vy);
            const r = Math.max(.4, s.r * (.55 + viva * .45));
            // O ESTICAR sai da velocidade: no topo do arco ela é quase zero e a gota fica redonda; na
            // subida e na queda ela alonga na direção do voo. É a mesma gota o tempo todo, e é a
            // FORMA que conta que ela subiu e está caindo — sem nenhum estado a mais pra manter.
            const comp = r + Math.min(r * 5, vel * .05);
            const ang = Math.atan2(s.vy, s.vx);

            ctx.save();
            ctx.translate(s.x, s.y);
            ctx.rotate(ang);

            // o rastro: um fio que fica pra trás, mais apagado, e que só aparece quando ela está
            // rápida. Sem ele a gota esticada lê como um risco; com ele, lê como algo em voo.
            const rastro = ctx.createLinearGradient(0, 0, -comp * 2.2, 0);
            rastro.addColorStop(0, `rgba(${cfg.luz}, ${viva * .5})`);
            rastro.addColorStop(1, `rgba(${cfg.luz}, 0)`);
            ctx.fillStyle = rastro;
            ctx.beginPath();
            ctx.moveTo(0, -r * .55);
            ctx.quadraticCurveTo(-comp * 1.2, -r * .12, -comp * 2.2, 0);
            ctx.quadraticCurveTo(-comp * 1.2, r * .12, 0, r * .55);
            ctx.closePath();
            ctx.fill();

            // a GOTA: ponta na frente, bojo atrás — a forma de qualquer pingo que voa.
            ctx.fillStyle = `rgba(${cfg.luz}, ${viva * .95})`;
            ctx.beginPath();
            ctx.moveTo(comp, 0);
            ctx.quadraticCurveTo(r * .2, -r, -r * .9, 0);
            ctx.quadraticCurveTo(r * .2, r, comp, 0);
            ctx.closePath();
            ctx.fill();

            ctx.restore();
        }
    };
}

/// A FUMAÇA que sobe da árvore e da terra rachada. Ela é ESCURA — o que queimou aqui já queimou, então
/// isto é fuligem, e não o vapor claro da lâmpada dos Místicos.
///
/// Ela existe por uma razão de leitura: chão aceso sem nada subindo dele lê como adesivo colado na
/// terra. É a mesma função que a coluna da fogueira cumpria no Folclore, com o sinal trocado — lá a
/// fumaça dizia que o fogo estava vivo; aqui ela diz que o calor está SAINDO de baixo.
export function criarFumacaDoInferno(cfg, canvas, arvoreCfg, inferno) {
    const medir = medidorDoChaoDaVila(canvas);
    let t = 0;

    // Uma coluna na árvore e as outras espalhadas pela terra, cada uma com o seu relógio. Colunas
    // sincronizadas denunciariam que é um efeito só — a mesma dessincronia das corujas e das labaredas.
    const colunas = Array.from({ length: cfg.colunas }, (_, i) => ({
        desvio: i === 0 ? 0 : (i % 2 ? 1 : -1) * (.06 + Math.random() * .2),
        alto: i === 0 ? 1 : .5 + Math.random() * .4,
        baforadas: Array.from({ length: cfg.baforadas }, (_, j) => ({
            u: j / cfg.baforadas,
            vel: .07 + Math.random() * .07,
            raio: .7 + Math.random() * .7,
            giro: Math.random() * Math.PI * 2,
            balanco: .6 + Math.random() * .9,
        })),
    }));

    return (ctx, dt) => {
        t += dt;

        const chao = linhaDaArvore(medir(dt), arvoreCfg);
        const cx = canvas.width * arvoreCfg.x;

        for (const c of colunas) {
            const baseX = cx + c.desvio * canvas.width;
            // A coluna do meio sobe do BURACO (é lá que a lava está parada e queimando); as outras
            // saem da terra em volta. Ela pergunta ao maestro em vez de calcular: o buraco é dono da
            // posição dele, e a fumaça não pode ter uma segunda opinião sobre onde ele está.
            const baseY = c.desvio === 0 ? (inferno.buraco?.y ?? chao) : chao;
            const alcance = canvas.height * cfg.alcance * c.alto;

            for (const b of c.baforadas) {
                const u = (b.u + t * b.vel) % 1;
                // o raio ABRE com a subida (a fumaça se espalha), e o alfa nasce e morre no caminho
                const abre = canvas.height * cfg.abre * (.18 + .82 * u) * b.raio;
                const x = baseX + Math.sin(u * b.balanco * Math.PI * 2 + b.giro) * abre * .5;
                const y = baseY - alcance * u;
                const alfa = Math.sin(Math.min(1, u * 1.15) * Math.PI) * cfg.opacidade * (.55 + (inferno.pulso ?? 1) * .45);

                const g = ctx.createRadialGradient(x, y, 0, x, y, abre);
                g.addColorStop(0, `rgba(${cfg.cor}, ${alfa})`);
                g.addColorStop(1, `rgba(${cfg.cor}, 0)`);
                ctx.fillStyle = g;
                ctx.beginPath();
                ctx.arc(x, y, abre, 0, Math.PI * 2);
                ctx.fill();
            }
        }
    };
}

/// A COLUNA DE MORCEGOS — o 🧛 Vampiro, e nenhum vampiro desenhado. (A peça que o Gabriel aprovou de
/// primeira: *"gostei dos morcegos ao redor da arvore"*.)
///
/// Três dos quatro apóstolos desta facção são figura humana ou quase (vampiro, elfo, diabo), e figura
/// humana pequena em canvas fica esquisita — foi a lição que virou o gênio numa lâmpada e a sereia numa
/// cauda. Aqui a saída é OUTRA: o vampiro é o MESMO bicho do 🦇, com outro comportamento. Os morcegos
/// do `voadores` atravessam a tela em bando; estes se JUNTAM num rodamoinho sobre a árvore, seguram, e
/// estouram pra fora. Bando é bicho; coluna é alguém MANDANDO neles, que é o que ele faz.
///
///   espera → juntando (eles entram pelas bordas e caem na hélice) → girando → estourando → espera
///
/// E é ela que DISPARA o ciclo do Inferno: ao estourar, incrementa `inferno.passagem`. Não sabe quem
/// lê — a fenda é que fica esperando o número mudar. Sem fenda no tema, o número sobe e ninguém liga.
export function criarColunaDeMorcegos(cfg, canvas, arvoreCfg, inferno) {
    const medir = medidorDoChaoDaVila(canvas);
    let fase = 'espera';
    let relogio = entre(cfg.espera) * .26;  // a primeira espera é curta: a cena não pode abrir vazia
    let q = 0;

    const bichos = Array.from({ length: cfg.quantos }, (_, i) => ({
        ang: (i / cfg.quantos) * Math.PI * 2 + Math.random() * .5,
        giro: 1.5 + Math.random() * .9,
        alto: Math.random(),
        tamanho: entre(cfg.tamanho),
        asa: Math.random() * Math.PI * 2,
        velocidadeDaAsa: 10 + Math.random() * 7,
        // De onde ele VEM. Sem isto, os dezesseis apareceriam do nada em volta do mesmo ponto.
        entradaX: Math.random() < .5 ? -.12 : 1.12,
        entradaY: .05 + Math.random() * .5,
    }));

    return (ctx, dt) => {
        relogio -= dt;

        if (fase === 'espera' && relogio <= 0) { fase = 'juntando'; q = 0; }
        else if (fase === 'juntando') { q += dt / cfg.juntar; if (q >= 1) { fase = 'girando'; q = 0; relogio = cfg.girar; } }
        else if (fase === 'girando' && relogio <= 0) { fase = 'estourando'; q = 0; }
        else if (fase === 'estourando') {
            q += dt / cfg.estourar;
            if (q >= 1) {
                fase = 'espera'; q = 0; relogio = entre(cfg.espera);
                // O bando passou. Quem quiser que faça alguma coisa com isso.
                inferno.passagem = (inferno.passagem ?? 0) + 1;
            }
        }

        if (fase === 'espera') return;

        const chao = linhaDaArvore(medir(dt), arvoreCfg);
        const cx = canvas.width * arvoreCfg.x;
        // A coluna mora na altura da copa: mais baixo ela ficaria atrás da luta, mais alto sairia da tela.
        const cy = chao - canvas.height * arvoreCfg.altura * .62;
        const alto = canvas.height * cfg.alto;

        // O RAIO é o que conta a fase: ele encolhe enquanto eles se juntam e ABRE no estouro. Fora
        // isso, nada muda de desenho — é o mesmo voo o tempo todo.
        const aberto = fase === 'estourando' ? 1 + q * q * 5 : 1;
        const raio = canvas.width * cfg.raio * aberto;
        const alfa = fase === 'estourando' ? Math.max(0, 1 - q) : 1;

        ctx.save();
        ctx.globalAlpha = alfa;

        for (const b of bichos) {
            b.ang += b.giro * dt;
            b.asa += b.velocidadeDaAsa * dt;

            const na = {
                x: cx + Math.cos(b.ang) * raio,
                y: cy - (b.alto - .5) * alto + Math.sin(b.ang) * raio * .24,
            };

            let x = na.x, y = na.y;
            if (fase === 'juntando') {
                // `q³` deixa a chegada MANSA: eles vêm depressa de longe e assentam devagar na hélice.
                // Com interpolação reta, dezesseis morcegos travariam no lugar todos no mesmo quadro.
                const k = 1 - Math.pow(1 - q, 3);
                x = canvas.width * b.entradaX + (na.x - canvas.width * b.entradaX) * k;
                y = canvas.height * b.entradaY + (na.y - canvas.height * b.entradaY) * k;
            }

            // O que está na frente (embaixo, no seno) é maior — é a única profundidade que uma hélice
            // precisa, e ela nunca deixa o tamanho chegar a zero.
            const s = b.tamanho * (.78 + .32 * (.5 + .5 * Math.sin(b.ang)));
            desenharMorcego(ctx, x, y, s, b.asa, Math.sin(b.ang) < 0, cfg.cor);
        }

        ctx.restore();
    };
}

/// Os FOCOS DE INCÊNDIO na vila: as pontes e as casas que ainda estão QUEIMANDO (pedido do Gabriel —
/// "algumas caídas outras queimando"). Eles são canvas e não ladrilho pelo motivo mais simples que
/// existe: fogo tremula, e ladrilho é imagem parada.
///
/// Usam o `criarNoHorizonte`, o mesmo motor das corujas do cemitério e das bobinas do laboratório —
/// mas SEM `aceso`, porque incêndio não pisca: ele arde. Quem repete por ladrilho aqui está certo, ao
/// contrário do que este front costuma fazer: as casas queimando são as do próprio desenho que
/// repete, então cada cópia do ladrilho tem as suas, exatamente onde elas estão pintadas. Canvas aqui
/// não é por endereço — é porque fogo TREMULA, e ladrilho é imagem parada.
///
/// A chama é a mesma da ruína dos ⚙️ Tecnológicos: duas frequências fora de compasso na altura, e a
/// ponta balançando pro lado. Fogo não sobe reto, e um brilho pulsando sozinho leria como lâmpada.
export const criarFogosDaVila = (cfg, canvas) => {
    let t = 0;
    const desenhar = criarNoHorizonte(cfg, canvas, (ctx, c, k) => desenharChama(ctx, c.x, c.y, canvas.height * k.tamanho, t + c.x, k));
    return (ctx, dt) => { t += dt; desenhar(ctx, dt); };
};
