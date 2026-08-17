import { entre } from '../comum/basicos.js';
import { medirDoTema } from '../comum/ladrilho.js';
import { criarPo } from '../comum/ar.js';
// 🐉 A PRAIA NO CREPÚSCULO — a lâmpada na areia, e o dragão dando as voltas dele.
//
// Os quatro estão aqui pelo SINAL, como no Folclore, mas o problema era outro: três dos quatro
// Místicos têm CORPO HUMANO (o 🧞 gênio, a 🧜 sereia, a 🧚 fada), e figura humana pequena desenhada
// em canvas fica esquisita — foi o Gabriel quem cravou isso, e ele está certo: o Ninja é a única
// silhueta humana do front inteiro e só funciona porque é preta, distante e em movimento. Então
// nenhum dos três aparece: o gênio é a LÂMPADA e o que sai dela, a sereia é uma CAUDA que rompe a
// água entre os golfinhos, e a fada é o vaga-lume que é maior que os outros.
//
// O 🐲 dragão é o oposto — ele não tem nada de humano e é a coisa grande da cena. Por isso ele é o
// único que aparece inteiro, e é ele quem dá o EVENTO do tema (ver `criarDragao`: três distâncias,
// ida e volta) e quem escreve no vento. Segundo cliente do maestro, e a prova de que ele não era
// um enfeite do Folclore: aqui quem sopra é outra coisa, e quem lê nem sabe que mudou.
export const ar = {
    // 🌴 As PALMEIRAS que emolduram a cena, arqueando pro centro. É o único tema em que a moldura
    // é canvas — nos outros quatro ela é o `::before`/`::after` do CSS —, e o motivo é o vento:
    // elas VERGAM quando o dragão passa raspando, e pseudo-elemento não lê JS. Duas de cada lado,
    // em tamanhos diferentes, pra a borda não ficar espelhada.
    palmeiras: {
        tronco: '#181129', troncoLuz: '#3a2f4d', folha: '#101c30', folhaLuz: '#2e5064',
        coco: '#1d1526',
        // Em fração da altura da arena; `x` em fração da largura (do lado de fora do miolo, que é
        // onde a luta acontece).
        porLado: 2, altura: [.46, .68], x: [.005, .105],
        // A inclinação BASE (pro centro) e o balanço de clima, cada palmeira no seu ritmo — é a
        // dessincronia de sempre. `ganhoDoVento` é o quanto a rajada do dragão soma nisso.
        inclinacao: [.16, .3], balanco: [.018, .04], ritmo: [.32, .58], ganhoDoVento: .55,
        // As folhas: quantas saem da coroa, o comprimento e a largura delas (em frações da altura
        // da palmeira), e quantos folíolos cada uma tem. FOLÍOLO é o que faz ler como palmeira:
        // uma folha lisa vira uma pena, e uma pena vira um enfeite de fantasia.
        folhas: 9, folhaComprimento: [.34, .5], folhaLargura: .07, foliolos: 12, cocos: 3,
    },
    // 🐲 O DRAGÃO CHINÊS. `tamanho` é a grossura de referência do corpo em fração da altura da
    // arena, e cada passagem multiplica isso pela `escala` dela.
    dragao: {
        // Dois tons e mais nada: dorso e ventre, os mesmos no corpo e na cabeça. Havia um terceiro
        // (`corpoLuz`, verde claro) que servia ao degradê da cabeça e ao halo — os dois saíram, e
        // ele com eles. Cor sem cliente é a próxima a reaparecer onde não devia.
        corpo: '#1c5f4e', ventre: '#d8e9b8', crista: '#f2c14b',
        crina: '#c9452f', chifre: '#e8d9a8', olho: '255, 214, 96',
        // `escama` e `escudo` são cor CSS direta (viram `strokeStyle`), enquanto `olho`, `perola` e
        // `veu` são triplas r,g,b porque entram dentro de um `rgba(...)` com alfa variável. Trocar
        // um pelo outro não quebra nada de forma visível: `strokeStyle = '110, 214, 176'` é
        // inválido, o navegador IGNORA a atribuição em silêncio e desenha com a cor anterior.
        escama: '#6ed6b0', escudo: '#93a86f',
        // A BRUMA é a cor dele na passagem mais distante: azulada, quase a do céu. Longe, bicho
        // não é da cor que ele tem — é da cor do ar que está entre ele e quem olha.
        bruma: '#2c3f66',
        // O VÉU é o foco da passagem de perto: a cena inteira escurece um pouco enquanto ele
        // atravessa, e o olho vai pra ele sem que nada precise piscar.
        veu: '8, 10, 26',
        // ANÉIS é a resolução do corpo, e ele é MUITO comprido de propósito. O corpo é desenhado
        // como FITA CONTÍNUA (duas margens e um preenchimento só, igual ao tentáculo) — a primeira
        // versão empilhava elipses, e como o raio afina até a ponta enquanto o espaço entre elas é
        // constante, a cauda virava linha pontilhada. Fita não tem esse problema em resolução
        // nenhuma, e por isso dá pra alongar à vontade.
        tamanho: .085, aneis: 74, passo: .38, ondulacao: 1.25, perfil: 1.3,
        // O MOVIMENTO é do BICHO, não da passagem: é um dragão só, em três distâncias, e o que
        // muda entre elas tem que ser aspecto (tamanho, nitidez, opacidade) e perspectiva — nunca
        // o jeito de nadar. Estes três números moram aqui fora justamente pra não haver três
        // jeitos.
        //
        // E eles são medidos NO CORPO DELE, que é o que torna isso possível. Media antes contra o
        // percurso e contra a altura da arena — duas coisas que mudam de tamanho junto com ele —,
        // e aí "a mesma ondulação" precisava de números diferentes em cada passagem. Não era
        // escolha de desenho, era artefato de unidade: na prática a de perto fazia ~1,9 ondas ao
        // longo do corpo e as de trás ~0,65, ou seja, quase retas.
        //
        //   ondasNoCorpo    · quantas curvas cabem no comprimento dele. ~2 é serpente.
        //   amplitudeDaOnda · o quanto ela abre, em fração do COMPRIMENTO DE ONDA. É a medida que
        //                     preserva a FORMA do S: em fração da tela, o mesmo número dá uma
        //                     cobra de perto e um fio esticado de longe.
        //   chicote         · o quanto a onda cresce da cabeça pra cauda.
        ondasNoCorpo: 1.9, amplitudeDaOnda: .096, chicote: .8,
        // A espera CHEIA só quando o ciclo volta ao começo; entre as passagens de um mesmo ciclo
        // ele mal sai de cena. Se toda passagem custasse a espera cheia, o ciclo inteiro levaria
        // minutos e ninguém veria que é o MESMO bicho indo e voltando.
        // A pausa entre uma passagem e a seguinte. Um número SÓ, e não uma faixa: o compasso
        // regular é o que faz as três distâncias lerem como o mesmo bicho dando voltas.
        espera: 4.2,
        // Ele COMEÇA NA FRENTE, se apresentando, e daí cada volta é SORTEADA entre as três
        // distâncias — do fundo ele pode vir direto pra frente (ver `criarDragao`). O lado, esse,
        // continua alternando: é o que faz ele reentrar por onde saiu, em vez de teleportar.
        //
        // O que muda por passagem é só ASPECTO e PERSPECTIVA — a ondulação é a mesma nas três, e
        // mora lá em cima. `y` é a altura do eixo em fração da arena, `alongar` estica o corpo sem
        // engordá-lo, `opacidade` é o quanto dele se vê e `detalhe` o quanto se desenha.
        //
        // `velocidade` é em ALTURAS DE ARENA POR SEGUNDO, e não em segundos de travessia: o
        // percurso inclui a largura da janela, e com duração fixa ele passava mais rápido em tela
        // larga. A de perto é a mais veloz de propósito — o que está perto atravessa a vista mais
        // depressa, e é a mesma paralaxe que faz o poste passar voando e a montanha não.
        //
        // A de PERTO é COLOSSAL: o eixo fica bem ACIMA do topo, então o que entra em cena é a
        // BARRIGA dele atravessando o céu — a metade de cima fica fora da tela e a cabeça mal
        // aparece. É o enquadramento que dá o tamanho, e tem um efeito colateral bom: rosto que
        // quase não se vê é rosto que não precisa ser perfeito.
        //
        // E ela DEMORA. Com o corpo medindo umas quatro telas e meia, a cabeça passa, o corpo
        // continua passando por uns vinte segundos, e só então vem a cauda. A demora é o efeito:
        // é ela que diz o tamanho, mais do que qualquer coisa que se desenhe. Encurtar a travessia
        // pra ela ficar cômoda seria desfazer justamente o que a torna impressionante.
        //
        // As outras duas são VULTOS NA BRUMA (`detalhe: 0`), e a diferença entre elas é só tamanho
        // e opacidade. O nível 1 — corpo com ventre e crista, sem os detalhes finos — ficou sem
        // cliente quando o Gabriel pediu o médio também na sombra; não é código morto (o `>= 1` que
        // desenha ventre e crista serve ao nível 2), é um valor que nenhuma passagem escolhe hoje.
        passagens: [
            { escala: 5.5, y: -.18, alongar: .3, velocidade: .75,
              detalhe: 2, opacidade: 1, vento: .85, foco: .45 },
            { escala: .34, y: .26, alongar: 1.35, velocidade: .31, detalhe: 0, opacidade: .46 },
            { escala: .22, y: .16, alongar: 1.7, velocidade: .23, detalhe: 0, opacidade: .32 },
        ],
    },
    // 🧞 A LÂMPADA na areia — o elemento central, e a única coisa quente da cena.
    lampada: {
        metal: '#b8862f', metalLuz: '#f6dc92', metalSombra: '#4a3210', borda: '#2a1b06',
        luz: '255, 198, 96', fumaca: '198, 176, 232', faisca: '255, 226, 150',
        // Onde ela fica: `x` em fração da largura, e `assentada` em fração da FAIXA DE AREIA (0 =
        // na beira d'água, 1 = no rodapé). Amarrada à areia e não à altura da arena, pra ela não
        // descolar do chão se as faixas mudarem.
        x: .5, assentada: .42, tamanho: .085,
        // O clarão em múltiplos da largura da lâmpada. Grande pela mesma razão da fogueira: a
        // coluna do log passa na frente do centro, e o que resolve é a luz vazar pelos dois lados.
        clarao: 4.2,
        // O BAFO: de tempos em tempos ela solta uma espiral de vapor que sobe, abre e se desmancha,
        // com faíscas douradas. É o gênio SEM o gênio — o que se vê é a lâmpada trabalhando.
        espera: [7, 15], soprar: 5.5, baforadas: 9,
        // `alcance` é até onde o vapor sobe (fração da altura da arena), `abre` o quanto ele se
        // alarga no topo (múltiplos da largura da lâmpada), `giro` o quanto a espiral enrola.
        alcance: .3, abre: 2.6, giro: 2.4, faiscas: 14,
    },
    // 🌊 O MAR: as ilhas no horizonte, as ondas rolando pro raso e a espuma subindo na areia.
    //
    // Sem isto o mar era um gradiente de CSS, ou seja, uma parede azul: água parada não existe, e
    // a praia inteira dependia de os golfinhos estarem pulando naquele instante pra parecer viva.
    // As três peças fazem trabalhos diferentes — a ilha dá PROFUNDIDADE (uma referência de tamanho
    // no fundo), a onda dá MOVIMENTO, e a espuma na areia dá a BEIRA, que é o que amarra o mar ao
    // chão onde a luta acontece.
    mar: {
        ilha: '#191631', ilhaLuz: '#3b3358', reflexo: '158, 150, 190',
        // `x` em fração da largura; `largura` e `altura` em frações da ALTURA da arena, pra a ilha
        // não esticar em tela larga. Três, de tamanhos bem diferentes: duas iguais leriam como
        // repetição, e é a diferença entre elas que sugere distâncias diferentes.
        ilhas: [
            { x: .16, largura: .11, altura: .052, picos: 2 },
            { x: .79, largura: .16, altura: .08, picos: 3 },
            { x: .58, largura: .05, altura: .019, picos: 1 },
        ],
        // AS ONDAS. Nascem no horizonte e rolam pro raso; `u` anda em u² pra elas ficarem
        // amontoadas no fundo e abertas na beira, que é como perspectiva funciona na água.
        //
        // Todas com a MESMA velocidade, e é isso que dá a cadência: espaçadas por igual e andando
        // juntas, elas chegam na beira em intervalos regulares (~2,4s aqui). Velocidades sorteadas
        // — como era antes — davam ondas se ultrapassando, que é bonito de perto e errado de
        // longe: marulho não faz isso, e sem cadência não há o que sincronizar com a areia.
        onda: '190, 228, 242', ondas: 15, velocidade: .028, espessura: 2.6, alfa: .3,
        // A ESPUMA da beira é CONSEQUÊNCIA da onda, e não um segundo relógio: cada onda que chega
        // dispara a lavagem, e as línguas só variam em tamanho e num `atraso` pequeno — o bastante
        // pra a beira não subir como uma barra reta, sem desmanchar a sincronia.
        //
        // `lavar` é quanto dura a subida e a volta de uma lavagem. Ela é MAIOR que o intervalo
        // entre as ondas de propósito: assim a água ainda está recuando quando a próxima chega, e
        // é essa sobreposição que faz a beira nunca secar por completo.
        espuma: '228, 244, 252', linguas: 5, avanco: .028, lavar: 3.4, atraso: [0, .5],
    },
    // 🧜 Os GOLFINHOS saltando no mar, e de vez em quando a CAUDA no lugar de um deles.
    //
    // A sereia entra pelo contraste: os golfinhos são escuros, rápidos e vêm em grupo; a cauda é
    // turquesa acesa, sozinha e mais lenta. Uma coisa diferente no meio de um padrão estabelecido
    // lê como acontecimento — e é por isso que os golfinhos têm de existir antes dela.
    golfinhos: {
        corpo: '#16324f', corpoLuz: '#4a7ea6', ventre: '#b9cfdd',
        cauda: '#1f9d92', caudaLuz: '#6ff2d8', caudaBrilho: '#eafff8', escama: '#f7d774',
        // A LIGAÇÃO entre o rabo e o leque, num tom À PARTE (violeta quente contra o turquesa): é
        // ela que fecha a barbatana e põe o contraste bem no ponto pra onde o olho vai.
        juncao: '#b45cc9',
        espuma: '235, 246, 255',
        espera: [3.5, 8], grupo: [1, 3], atraso: .28,
        // `profundidade` 0 = rente ao horizonte (pequeno), 1 = na beira (grande). `salto` é a
        // altura do pulo e `avanco` o quanto ele anda, ambos em múltiplos do tamanho do bicho.
        // `profundidade` para bem antes da beira (0.7, não 0.92): eles são bicho de água funda, e
        // no raso ficavam praticamente na areia — além de disputarem espaço com a espuma.
        profundidade: [.12, .7], tamanho: [.035, .085],
        // `avanco` subiu de 2.6 pra 4.3: o pulo cobre mais chão, e um salto comprido lê como
        // impulso. O curto parecia que ele estava pulando parado.
        salto: 1.5, avanco: 4.3, duracao: [1.3, 1.9],
        // `mergulho` é o quanto o arco começa e termina ABAIXO da linha d'água (em múltiplos do
        // tamanho do bicho). É ele que faz o golfinho sair da água em PARTES e voltar em partes,
        // com o resto recortado pela superfície, em vez de aparecer inteiro do nada.
        mergulho: .85,
        // A sereia: mais rara, maior, mais lenta e sempre mais PERTO (a cauda tem detalhe, e
        // detalhe longe vira sujeira — a lição do Ninja e do Caixão outra vez).
        //
        // Ela NÃO usa `salto` nem `avanco`: não pula. `sereiaAltura` é o quanto da ponta do rabo
        // sai da água (em múltiplos do tamanho dela), e a duração é longa porque emergir é um
        // gesto lento — rápido, viraria um golfinho verde.
        chanceSereia: .22, sereiaTamanho: 1.9, sereiaDuracao: 3.2, sereiaProfundidade: [.45, .72],
        sereiaAltura: 1.15,
    },
    // 🧚 Os VAGA-LUMES, e a FADA que é um deles maior.
    //
    // Ela não tem corpo: é um núcleo aceso, duas asas que piscam e um rastro. O rastro é o que a
    // separa dos outros — vaga-lume acende no lugar, ela DEIXA caminho.
    vagalumes: {
        cor: '255, 236, 168', fada: '186, 255, 236', asa: '255, 255, 255',
        // (as asas e o núcleo dela usam a mesma cor: a asa é luz atravessada, não pano)
        quantos: 26, raio: [.9, 2.2], deriva: [6, 20], pisca: [.5, 1.4], sopro: .1,
        // A faixa em que eles vivem, em fração da altura da arena: do meio do mar até a areia. No
        // céu virariam estrelas (que são da invasão), e é a mesma regra de não repetir assinatura.
        faixa: [.5, .96],
        // As fadas: cada uma espera, atravessa a cena num arco e some. São VÁRIAS e com relógios
        // independentes — o que se quer é encontrar uma de vez em quando, não ver uma fila delas.
        fadas: 4, fadaEspera: [7, 17], fadaAtravessar: [6, 9.5], fadaTamanho: 4.6, fadaRastro: 16,
    },
    // O PÓ é pólen/maresia subindo devagar — e é ele que faz a rajada do dragão ficar VISÍVEL na
    // tela inteira, e não só nas palmeiras. Nenhuma linha nova: é o `criarPo` com `sopro`.
    po: {
        cor: '206, 232, 255', quantas: 34, subida: [8, 26], raio: [0.5, 1.6],
        opacidade: [.12, .4], sopro: .13,
    },
};

/// Monta a cena deste capítulo. A ORDEM É A PROFUNDIDADE — o que vem antes fica atrás.
///
/// O núcleo (`iniciarAr`) não sabe que este tema existe: ele chama `montar` e recebe as camadas
/// prontas.
export function montar({ fundo, frente, maestro }) {
    return {
        noFundo: [
            // Os Místicos, na ordem em que a praia é vista: o dragão está no CÉU e é a coisa mais
            // distante mesmo quando passa perto — vem primeiro. Depois o mar (os saltos), depois a areia
            // (a lâmpada), e as palmeiras por último porque são a moldura: elas ficam na frente de tudo o
            // que é cenário, e ainda assim atrás dos combatentes, que é o lugar de uma borda de cena.
            //
            // O dragão fica no FUNDO mesmo na passagem de perto, pela mesma razão do Invasor: um bicho
            // desse tamanho na tela da frente taparia a luta. O que dá a leitura de "por cima" não é a
            // camada, é ele ser CORTADO pela borda de cima.
            criarDragao(ar.dragao, fundo, maestro.vento),
            criarMar(ar.mar, fundo),
            criarGolfinhos(ar.golfinhos, fundo),
            criarLampada(ar.lampada, fundo, maestro.vento),
            criarPalmeiras(ar.palmeiras, fundo, maestro.vento),
        ].filter(Boolean),
        naFrente: [
            criarPo(ar.po, frente, maestro.vento, maestro.fogo),
            // Os vaga-lumes são da FRENTE pelo mesmo motivo dos voadores: eles estão no ar entre o jogador
            // e o mundo, e é essa separação que dá profundidade à praia.
            criarVagalumes(ar.vagalumes, frente, maestro.vento),
        ].filter(Boolean),
    };
}
/// 🐲 O DRAGÃO CHINÊS — o ciclo de três distâncias.
///
/// O problema de uma criatura ENORME é que ela não cabe na tela, e desenhá-la inteira de uma vez a
/// encolhe até virar enfeite. A saída aqui é o CICLO: ele passa longe e pequeno, volta mais perto, e
/// na terceira vem por cima da tela, cortado pela borda de cima, tão grande que a cabeça sai pelo
/// outro lado antes de a cauda ter entrado. Não é um desenho maior — é a MESMA criatura em três
/// distâncias, e é a comparação entre elas que diz o tamanho dele.
///
/// Ele COMEÇA NA FRENTE e vai embora: a `ORDEM` é 0,1,2,1 — perto, médio, longe, médio, e de volta ao
/// perto. Ele se apresenta primeiro e só então some no fundo, em vez de chegar aos poucos com o auge no
/// fim. Cada passagem inverte o SENTIDO, então ida e volta são o mesmo bicho dando meia-volta lá fora,
/// e não dois dragões correndo pro mesmo lado.
///
/// O CORPO É A CABEÇA NO PASSADO. Cada anel lê a mesma curva num ponto anterior do percurso
/// (`progresso − i * passo`), então a ondulação viaja da cabeça pra cauda sozinha: sem histórico de
/// posições, sem buffer, e sem depender do dt (guardar posição por quadro quebra quando o framerate
/// varia). É a onda que desce no tentáculo, aplicada a um corpo que anda.
///
/// E ele é desenhado como FITA CONTÍNUA, também como o tentáculo: as duas margens são calculadas pela
/// normal de cada anel e o corpo inteiro é UM preenchimento. A primeira versão empilhava elipses, uma
/// por anel — e como o raio afina até a ponta enquanto o espaço entre os anéis é constante, a metade
/// de trás virava linha pontilhada. Foi o que fez o bicho parecer duro e picado. Fita não tem esse
/// problema em resolução nenhuma, e é por isso que dá pra ele ser tão comprido quanto se queira.
///
/// A ONDA é a soma de DUAS frequências, e a amplitude CRESCE em direção à cauda (`chicote`). Uma
/// frequência só dá um metrônomo, e amplitude uniforme faz o corpo inteiro balançar em bloco — as duas
/// coisas juntas são o que separa "cobra nadando" de "fita presa num ventilador". A raiz do movimento
/// é a cabeça, que quase não sai da linha; quem chicoteia é a ponta.
///
/// O DETALHE É FUNÇÃO DA DISTÂNCIA — a lição mais cara deste front, e aqui ela vira configuração:
/// longe ele é silhueta chapada, no meio ganha volume e crista, e perto ganha crina, chifres, bigodes,
/// patas e a pérola. Desenhar tudo sempre e só escalar daria sujeira ilegível na passagem de longe e
/// um bicho de papel na de perto.
///
/// E é ele quem ESCREVE no vento na passagem de perto — o segundo cliente do maestro, depois do
/// redemoinho do Folclore. Um bicho desse tamanho passando raspando TEM que deslocar ar; sem isso ele
/// seria um adesivo enorme escorregando na frente do cenário. Quem lê (palmeiras, fumaça, vaga-lumes,
/// pólen) não sabe que a fonte mudou de tema, que é exatamente a promessa que o maestro fez.
export function criarDragao(cfg, canvas, vento) {
    // A distância da próxima passagem é SORTEADA entre as três (0 perto · 1 médio · 2 longe): ele sai
    // de cena e volta de onde quiser — do fundo pode vir direto pra frente. Só não repete a mesma
    // duas vezes seguidas, porque duas passagens idênticas em fila leem como uma animação em loop, e
    // não como um bicho que foi e voltou.
    //
    // O que NÃO é sorteado é o lado, e é isso que impede o teleporte: o `sentido` alterna sempre, então
    // ele entra pelo lado por onde saiu. Ele deu meia-volta lá fora — e é justamente porque a distância
    // agora muda sozinha que o lado precisa continuar amarrado. Sorteando os dois, ele sumiria à
    // direita e reapareceria à esquerda, que é a única coisa que quebraria a ilusão de ser um bicho só.
    // O SACO DE DISTÂNCIAS: as três entram embaralhadas e são tiradas uma a uma; quando acaba, ele
    // se enche de novo. É o que garante que nenhuma passagem fique esperando muito — cada distância
    // aparece uma vez a cada três —, sem virar a fila fixa perto→médio→longe que um rodízio puro
    // daria. O sorteio livre de antes deixava a passagem de perto sumir por quatro, cinco voltas.
    const sacar = () => {
        if (!saco.length) {
            saco = cfg.passagens.map((_, i) => i);
            for (let i = saco.length - 1; i > 0; i--) {
                const j = Math.floor(Math.random() * (i + 1));
                [saco[i], saco[j]] = [saco[j], saco[i]];
            }
            // A emenda entre dois sacos é o único lugar onde a mesma distância pode sair duas vezes
            // seguidas — e duas passagens idênticas em fila leem como animação em loop.
            if (saco[0] === posicao && saco.length > 1) [saco[0], saco[1]] = [saco[1], saco[0]];
        }
        return saco.shift();
    };

    let saco = [];
    let posicao = 0;                        // ele começa na frente, se apresentando
    let fase = 'fora';
    let relogio = cfg.espera * .3;          // a primeira espera é curta: a cena não pode abrir vazia
    let progresso = 0;
    let sentido = 1;
    let t = 0;

    return (ctx, dt) => {
        t += dt;
        const p = cfg.passagens[posicao];

        if (fase === 'fora') {
            relogio -= dt;
            if (relogio <= 0) { fase = 'passando'; progresso = 0; }
            return;
        }

        // A GEOMETRIA vem antes do relógio porque a duração sai dela. A travessia é medida em
        // VELOCIDADE (alturas de arena por segundo) e não em segundos fixos: o percurso inclui a
        // largura da janela, então com duração fixa ele atravessava MAIS RÁPIDO numa tela mais larga —
        // o mesmo bicho parecia outro dependendo do tamanho da janela. Com velocidade, a duração se
        // ajusta sozinha e o movimento fica igual em qualquer tela.
        const s = canvas.height * cfg.tamanho * p.escala;
        const passo = cfg.passo * p.alongar;
        const comprimento = cfg.aneis * passo * s;
        // O percurso soma o comprimento dele nas DUAS pontas: senão a cauda ainda estaria em cena
        // quando a travessia acabasse, e ele piscaria pra fora.
        const percurso = canvas.width + comprimento * 2 + s * 4;

        progresso += dt / (percurso / (canvas.height * p.velocidade));
        if (progresso >= 1) {
            fase = 'fora';
            sentido = -sentido;
            // A espera é a MESMA pra todas as distâncias, e fixa. Ela já foi sorteada e maior nas
            // pontas, com o argumento de que "de longe cabe demorar pra voltar" — mas em jogo o que
            // isso produz é uma cena que fica parada logo depois da passagem que menos se vê. Um
            // compasso REGULAR faz o bicho ler como um só, indo e voltando; irregular ele parece
            // sumir e ser lembrado de novo.
            relogio = cfg.espera;
            posicao = sacar();
            return;
        }

        const passoU = (passo * s) / percurso;
        const yEixo = canvas.height * p.y;

        // A ONDA sai do CORPO dele, e não da passagem: `ondasNoCorpo` diz quantas curvas cabem no
        // comprimento, e a amplitude é uma fração do comprimento de onda. Convertidas aqui pras
        // unidades do percurso, elas dão a MESMA forma de S nas três distâncias — que é o que faz as
        // três serem o mesmo bicho, e não três animações parecidas.
        const ondas = cfg.ondasNoCorpo * (percurso / comprimento);
        const amp = (comprimento / cfg.ondasNoCorpo) * cfg.amplitudeDaOnda;

        // Duas frequências somadas e a amplitude crescendo pra cauda. `q` é o quanto se andou do
        // corpo: 0 na cabeça, 1 na ponta.
        const ponto = (u, q) => ({
            x: sentido > 0
                ? -comprimento - s * 2 + percurso * u
                : canvas.width + comprimento + s * 2 - percurso * u,
            y: yEixo + amp * (1 + q * cfg.chicote) * (
                Math.sin(u * Math.PI * 2 * ondas + t * cfg.ondulacao) * .72
                + Math.sin(u * Math.PI * 2 * ondas * .43 + t * cfg.ondulacao * .61 + 1.7) * .38),
        });

        const aneis = [];
        for (let i = 0; i < cfg.aneis; i++) {
            const q = i / (cfg.aneis - 1);
            const { x, y } = ponto(progresso - i * passoU, q);
            // PESCOÇO fino (constante) enquanto a cabeça o cobre · TRONCO engrossando depois dela ·
            // CAUDA afinando até FECHAR na ponta (zerar no último anel é o que dá bico em vez de
            // corte reto).
            //
            // O trecho constante do começo não é enfeite, é o conserto de um defeito que se via: o
            // corpo engrossava já nos primeiros 12%, mas o crânio só cobre uns cinco anéis — então o
            // corpo ULTRAPASSAVA a nuca antes de a cabeça acabar, e sobrava um degrau verde escuro
            // logo atrás do rosto. Enquanto a cabeça está por cima, o pescoço tem que ficar mais fino
            // que ela; a barriga só começa a crescer onde ela termina.
            const cheio = q < .1 ? .82
                : q < .3 ? .82 + ((q - .1) / .2) * .18
                : Math.max(0, 1 - Math.pow((q - .3) / .7, 1.25));
            aneis.push({ x, y, r: s * .5 * cheio, q });
        }

        // As duas MARGENS do corpo. A normal é forçada pra CIMA (`ny < 0`): assim as costas são sempre
        // as costas e a barriga é sempre a barriga, em qualquer sentido de marcha e em qualquer curva —
        // sem isso, o bicho vira do avesso quando a onda passa da horizontal.
        const cima = [], baixo = [];
        let naTela = 0;
        for (let i = 0; i < aneis.length; i++) {
            const a = aneis[i];
            if (a.x > 0 && a.x < canvas.width) naTela++;
            const antes = aneis[i - 1] ?? a, depois = aneis[i + 1] ?? a;
            const ang = Math.atan2(antes.y - depois.y, antes.x - depois.x);
            let nx = -Math.sin(ang), ny = Math.cos(ang);
            if (ny > 0) { nx = -nx; ny = -ny; }
            a.nx = nx; a.ny = ny; a.ang = ang;
            cima.push({ x: a.x + nx * a.r, y: a.y + ny * a.r });
            baixo.push({ x: a.x - nx * a.r, y: a.y - ny * a.r });
        }

        const fita = (margemA, margemB) => {
            ctx.beginPath();
            ctx.moveTo(margemA[0].x, margemA[0].y);
            for (const m of margemA) ctx.lineTo(m.x, m.y);
            for (let i = margemB.length - 1; i >= 0; i--) ctx.lineTo(margemB[i].x, margemB[i].y);
            ctx.closePath();
            ctx.fill();
        };

        // O SOPRO sai de onde a CABEÇA está NA TELA, e não do relógio da travessia. Amarrado ao
        // `progresso` (primeira versão) a rajada chegava muito antes dele: o percurso inclui duas vezes
        // o comprimento do corpo de margem, e com um bicho de várias telas de comprimento o progresso
        // 0 ainda tem a cabeça a duas telas de distância. Agora a conta é a distância dele ao meio da
        // arena, e o vento chega COM ele.
        if (p.vento) {
            const alcance = canvas.width * .5 + s * 1.5;
            const perto = 1 - Math.min(1, Math.abs(aneis[0].x - canvas.width * .5) / alcance);
            vento.forca = sentido * p.vento * Math.pow(perto, cfg.perfil);
            vento.x = aneis[0].x;
        }

        ctx.save();
        ctx.globalAlpha = p.opacidade;

        // O FOCO da passagem de perto: a cena inteira recua um pouco atrás dele. O olho vai pro dragão
        // sem que nada precise piscar, e o efeito desaparece junto com ele.
        if (p.foco) {
            // A força do véu sai de QUANTO DELE está em cena, e não do relógio: com um corpo de várias
            // telas, o relógio erra nos dois extremos — escurecia com ele ainda fora e clareava com
            // meio bicho ainda passando.
            ctx.globalAlpha = 1;
            ctx.fillStyle = `rgba(${cfg.veu}, ${p.foco * (naTela / aneis.length)})`;
            ctx.fillRect(0, 0, canvas.width, canvas.height);
            ctx.globalAlpha = p.opacidade;

            // AQUI HAVIA UM HALO: a mesma fita do corpo, 1.9× mais larga, em verde claro a 18%. A
            // intenção era aura; o efeito era outro. Uma cópia da malha do corpo, colorida e quase
            // dobrando a espessura dele, é exatamente a "malha que aparece com uma cor" — e ela fazia
            // o bicho parecer enorme independentemente da escala, porque o que se media no olho era o
            // halo, não o corpo. Foi o que sabotou três rodadas de ajuste de tamanho.
            //
            // O foco já é feito pelo VÉU, que escurece o cenário atrás dele. Destacar tirando dos
            // outros funciona; destacar somando volume ao alvo mente sobre o tamanho dele.
        }

        // O CORPO, num preenchimento só. Longe ele é da cor da BRUMA — bicho distante não tem a cor
        // dele, tem a cor do ar que está no meio do caminho.
        ctx.fillStyle = p.detalhe === 0 ? cfg.bruma : cfg.corpo;
        fita(cima, baixo);

        if (p.detalhe >= 1) {
            // O VENTRE: uma segunda fita, mais estreita e colada na margem de baixo. É o que dá volume
            // sem precisar de um gradiente por anel (que seria 74 gradientes por quadro).
            //
            // Ela ABRE do zero nos primeiros anéis (`nasce`). Começando na largura cheia, a barriga
            // clara aparecia inteira já no anel 0, por baixo de uma cabeça que não a cobria — era o
            // pedaço "a mais" logo atrás do rosto, com a barriga e o fio verde saindo de baixo dela.
            // Agora a fita nasce sem largura no pescoço e cresce, então não há o que sobrar.
            const nasce = (i) => Math.min(1, i / 9);
            ctx.fillStyle = cfg.ventre;
            fita(aneis.map((a, i) => ({
                x: a.x - a.nx * a.r * (1 - .82 * nasce(i)),
                y: a.y - a.ny * a.r * (1 - .82 * nasce(i)),
            })), baixo);

            // A CRISTA dorsal, serrilhada ao longo das costas: é ela que diz de que lado é o dorso.
            ctx.fillStyle = cfg.crista;
            ctx.beginPath();
            for (let i = 2; i < aneis.length - 3; i += 3) {
                const a = aneis[i], b = aneis[i + 1], c = aneis[i + 2];
                if (a.r < .4) continue;
                ctx.moveTo(a.x + a.nx * a.r, a.y + a.ny * a.r);
                ctx.lineTo(b.x + b.nx * b.r * 2.1, b.y + b.ny * b.r * 2.1);
                ctx.lineTo(c.x + c.nx * c.r, c.y + c.ny * c.r);
                ctx.closePath();
            }
            ctx.fill();
        }

        if (p.detalhe >= 2) {
            // OS ESCUDOS VENTRAIS: as faixas atravessadas da barriga, de dois em dois anéis. São o
            // detalhe mais importante da passagem de perto e foram os últimos a existir — o dorso
            // estava cheio de crista, crina e escama, e a barriga, que é a única parte que se vê
            // quando ele passa por cima, era uma chapa lisa. É a mesma regra do resto do front lida ao
            // contrário: detalhe vai onde o olho está, e aqui o olho está EMBAIXO dele.
            ctx.strokeStyle = cfg.escudo;
            for (let i = 9; i < aneis.length - 4; i += 2) {
                const a = aneis[i];
                if (a.r < s * .1) continue;
                ctx.lineWidth = a.r * .1;
                ctx.beginPath();
                ctx.moveTo(a.x - a.nx * a.r * .12, a.y - a.ny * a.r * .12);
                ctx.lineTo(a.x - a.nx * a.r * .97, a.y - a.ny * a.r * .97);
                ctx.stroke();
            }

            // Aqui houve um FIO na beira de baixo, pra dar silhueta à barriga contra o céu. Ele saiu:
            // com `lineWidth` proporcional ao bicho, na passagem de perto virava uma faixa de quase
            // 40px de verde escuro correndo pela barriga inteira — a linha que o Gabriel via e não
            // conseguia nomear. Contorno é detalhe de escala pequena; quando a peça cresce, ele cresce
            // junto e deixa de ser contorno pra virar mancha. A silhueta da barriga já sai do contraste
            // do ventre claro com o céu.

            // AS ESCAMAS: arcos na metade de cima do corpo, de três em três anéis. Só aqui, e nunca de
            // longe — a mesma regra de sempre: detalhe na escala errada lê como sujeira.
            ctx.strokeStyle = cfg.escama;
            ctx.lineWidth = s * .035;
            for (let i = 4; i < aneis.length - 6; i += 3) {
                const a = aneis[i];
                if (a.r < s * .12) continue;
                // O arco é medido a partir da NORMAL (que já aponta pra cima), e não do ângulo de
                // marcha: medido pelo ângulo, ele saía do dorso quando o bicho ia pra direita e da
                // BARRIGA quando ia pra esquerda, porque o ângulo vira 180°. A normal não vira.
                const n = Math.atan2(a.ny, a.nx);
                ctx.beginPath();
                ctx.arc(a.x, a.y, a.r * .62, n - Math.PI * .35, n + Math.PI * .35);
                ctx.stroke();
            }

            // A CRINA vermelha do pescoço: no dragão chinês a juba é a única coisa que não é da cor do
            // corpo, e é o que o separa de uma serpente comprida.
            ctx.strokeStyle = cfg.crina;
            ctx.lineCap = 'round';
            for (let i = 1; i < 12; i++) {
                const a = aneis[i];
                const tufo = a.r * (1.7 - i * .07) * (1 + Math.sin(t * 3 + i * .8) * .14);
                ctx.lineWidth = a.r * .3;
                ctx.beginPath();
                ctx.moveTo(a.x + a.nx * a.r * .5, a.y + a.ny * a.r * .5);
                ctx.lineTo(a.x + a.nx * tufo - Math.cos(a.ang) * tufo * .6,
                    a.y + a.ny * tufo - Math.sin(a.ang) * tufo * .6);
                ctx.stroke();
            }

            // AS PATAS, onde um bicho comprido teria ombro e quadril. Duas, e não quatro: as do outro
            // lado estariam escondidas pelo corpo, e desenhá-las daria um bicho transparente.
            for (const i of [11, 30]) {
                const a = aneis[i];
                if (a) desenharPataDeDragao(ctx, a, a.ang, t + i, cfg);
            }
        }

        const cabeca = aneis[0];
        desenharCabecaDeDragao(ctx, cabeca, cabeca.ang, p.detalhe, t, cfg);

        ctx.restore();
    };
}

/// A CABEÇA. Ela é a única parte em que o dragão chinês se distingue de uma serpente qualquer, e o que
/// faz essa distinção são três coisas nesta ordem de importância: os BIGODES (as duas antenas que
/// flutuam à frente), os CHIFRES de galho, e a mandíbula quadrada. Cor e escama não fazem nada disso.
///
/// A PÉROLA à frente do focinho é o resto da história: no mito ele persegue uma pérola flamejante, e
/// ela paga dois papéis aqui — diz "chinês" de relance e dá ao bicho um MOTIVO pra estar atravessando
/// a tela. Ela é FRIA de propósito: a única luz quente desta praia é a lâmpada do gênio.
export function desenharCabecaDeDragao(ctx, a, ang, nivel, t, cfg) {
    // A cabeça mede 2.4 raios do pescoço, e o número anda AMARRADO à nuca (as quinas de trás, em
    // ±1/2.4 de `s`): assim a nuca vale exatamente o raio do anel 0 e o encaixe com o pescoço é exato,
    // sem a cabeça sobrar por baixo do corpo nem o corpo por fora dela. Mexer num sem mexer no outro
    // reabre o degrau. Já subiu pra 3 numa tentativa de fazer a cabeça dominar o pescoço, e o efeito
    // foi só um bicho com a cabeça 25% maior — o degrau tinha outra causa (o corpo engrossando cedo
    // demais, resolvido no `cheio`).
    const s = a.r * 2.4;

    ctx.save();
    ctx.translate(a.x, a.y);
    ctx.rotate(ang);
    // Girar pelo ângulo já aponta o focinho pro caminho — mas quando ele vai PRA ESQUERDA o giro passa
    // de 90° e inverte os DOIS eixos, então a cabeça sai de cabeça pra baixo (chifres pra baixo,
    // mandíbula pra cima). O espelho vertical desfaz só a inversão de y e mantém o focinho na frente.
    //
    // A crista, a crina e a barriga não sofrem disso porque não vivem neste sistema: elas são
    // construídas a partir da NORMAL, que é forçada pra cima. Quem desenha em coordenada local precisa
    // deste espelho; quem desenha a partir da normal, não. É a mesma correção do golfinho.
    if (Math.cos(ang) < 0) ctx.scale(1, -1);

    // NADA À FRENTE DO FOCINHO. Aqui viveram uma pérola flamejante (a do mito, que ele persegue) e os
    // bigodes compridos, e os dois saíram pelo mesmo motivo prático: uma luz boiando na frente da
    // cabeça lê como PEIXE-LANTERNA, não como dragão, e os bigodes à frente engrossavam justamente a
    // parte do desenho que passa meio fora da tela. O que sobra aponta tudo PRA TRÁS — chifres,
    // crina, patas —, que é a direção que conta velocidade em vez de disputar com o focinho.

    // Os CHIFRES vão ANTES do crânio: eles nascem por trás dele, e sair de baixo é o que os faz
    // parecer presos na cabeça em vez de colados nela.
    if (nivel >= 1) {
        ctx.strokeStyle = cfg.chifre;
        ctx.lineWidth = s * .12;
        ctx.lineCap = 'round';
        for (const lado of [-1, 1]) {
            ctx.beginPath();
            ctx.moveTo(-s * .1, lado * s * .3);
            ctx.quadraticCurveTo(-s * .85, lado * s * .75, -s * 1.25, lado * s * .55);
            ctx.stroke();
        }
    }

    // O CRÂNIO: uma cunha, não uma bola. Focinho comprido é o que impede a leitura de cobra.
    // De longe ele é da BRUMA, chapado, igual ao corpo — a cabeça não pode ser a única coisa nítida
    // de um bicho que está sendo visto através de meio quilômetro de ar.
    //
    // A NUCA (as quinas de trás, em ±1/2.4) é mais baixa que o resto do crânio de propósito: ela é o
    // ponto de solda com o pescoço e vale exatamente o raio dele. Já foi mais alta, e sobrava por
    // baixo do corpo logo atrás da cabeça — era metade do pedaço "a mais" que aparecia ali.
    const nuca = 1 / 2.4;
    ctx.fillStyle = nivel === 0 ? cfg.bruma : cfg.corpo;
    ctx.beginPath();
    ctx.moveTo(-s * .5, -s * nuca);
    ctx.quadraticCurveTo(s * .6, -s * .5, s * 1.15, -s * .16);
    ctx.quadraticCurveTo(s * 1.28, 0, s * 1.15, s * .18);
    ctx.quadraticCurveTo(s * .6, s * .52, -s * .5, s * nuca);
    ctx.closePath();
    ctx.fill();

    if (nivel >= 1) {
        // A mandíbula leva o tom da BARRIGA, e o crânio o tom do dorso — exatamente os mesmos dois do
        // corpo. A cabeça tinha um degradê próprio (dorso → verde claro → ventre) e por isso não batia
        // com o resto: ela era a única parte com luz própria, e lia como uma peça de outro bicho
        // encaixada no pescoço. Cabeça é continuação do corpo, não um objeto à parte.
        ctx.fillStyle = cfg.ventre;
        ctx.beginPath();
        ctx.moveTo(s * .25, s * .26);
        ctx.quadraticCurveTo(s * .95, s * .34, s * 1.12, s * .2);
        ctx.lineTo(s * .95, s * .52);
        ctx.quadraticCurveTo(s * .6, s * .56, s * .25, s * .46);
        ctx.closePath();
        ctx.fill();

        // O OLHO, e é o ÚNICO detalhe do rosto. Sem HALO: o brilho radial que ele tinha era a segunda
        // luz da cabeça e reforçava a leitura de peixe-lanterna. Uma marca clara e sólida basta —
        // olho não precisa acender pra existir, precisa contrastar com o que está em volta.
        //
        // Aqui já viveram dentes e uma forquilha nos chifres, e saíram pelo mesmo motivo: numa cabeça
        // que passa meio fora da tela, detalhe de rosto não é lido como rosto, é lido como sujeira em
        // cima da silhueta. O que carrega a identidade agora são os chifres e a crina — forma, e não
        // traço fino.
        ctx.fillStyle = `rgba(${cfg.olho}, .92)`;
        ctx.beginPath();
        ctx.ellipse(s * .35, -s * .2, s * .11, s * .075, 0, 0, Math.PI * 2);
        ctx.fill();
    }

    ctx.restore();
}

/// Uma PATA: coxa, canela e três garras, RECOLHIDA PRA TRÁS e colada no corpo.
///
/// Ela já pendeu pra baixo, aberta, e ficava errada por dois motivos. O de desenho: perna pendurada é
/// pose de bicho que ANDA, e este está atravessando o céu — quem voa recolhe as patas, e é isso que
/// diz que ele está em movimento. O de enquadramento: na passagem de perto, o que se vê é a barriga
/// dele, e quatro membros pendurados picavam justamente a silhueta que carrega o tamanho.
///
/// Ela rema devagar mesmo colada — pata imóvel num corpo que ondula inteiro denuncia que o resto é
/// animação e ela é adesivo.
export function desenharPataDeDragao(ctx, a, ang, t, cfg) {
    const s = a.r * 2;
    const rema = Math.sin(t * 1.6) * .12;

    ctx.save();
    ctx.translate(a.x, a.y);
    ctx.rotate(ang);
    // O mesmo espelho da cabeça, e pelo mesmo motivo: sem ele as patas apontam pra CIMA quando o bicho
    // atravessa pra esquerda — recolhidas pro dorso em vez de rente à barriga.
    if (Math.cos(ang) < 0) ctx.scale(1, -1);
    ctx.lineCap = 'round';

    // a coxa, saindo do flanco e indo PRA TRÁS (−x), rente à barriga
    const coxaX = -s * .62, coxaY = s * (.42 + rema);
    ctx.strokeStyle = cfg.corpo;
    ctx.lineWidth = s * .22;
    ctx.beginPath();
    ctx.moveTo(-s * .05, s * .22);
    ctx.quadraticCurveTo(-s * .38, s * .44, coxaX, coxaY);
    ctx.stroke();

    // as garras, também pra trás: um leque curto, quase encostado no corpo
    ctx.strokeStyle = cfg.chifre;
    ctx.lineWidth = s * .075;
    for (const g of [-.16, .04, .24]) {
        ctx.beginPath();
        ctx.moveTo(coxaX, coxaY);
        ctx.quadraticCurveTo(coxaX - s * .22, coxaY + g * s * .5, coxaX - s * .42, coxaY + g * s);
        ctx.stroke();
    }
    ctx.restore();
}

/// 🧞 A LÂMPADA na areia — o elemento central dos Místicos, e o gênio SEM o gênio.
///
/// A regra que o Gabriel deu (nada de corpo humano em SVG) não foi uma limitação aqui, foi o desenho:
/// o que se vê é a lâmpada TRABALHANDO. De tempos em tempos ela solta uma espiral de vapor que sobe,
/// abre e se desmancha em faíscas — e quem completa o resto é quem está olhando, que é a mesma aposta
/// do sorriso do palhaço no escuro e dos chifres atrás da moita.
///
/// Ela é a única coisa QUENTE de uma cena fria, e é só por isso que ela é o centro — não por estar no
/// meio da tela. O clarão tem raio grande em múltiplos da largura dela pela lição da fogueira: a
/// coluna do log passa na frente do centro em todos os temas, e o que resolve não é fugir dela, é a
/// luz ter raio maior que a peça e vazar pelos dois lados.
///
/// O pé dela sai da FAIXA DE AREIA declarada no CSS (`--areia-linha`), e não de uma fração da arena:
/// assim ela continua fincada no chão se um dia as faixas mudarem de altura.
export function criarLampada(cfg, canvas, vento) {
    const areia = medirDoTema('--areia-linha', 71) / 100;

    let fase = 'quieta';
    let relogio = entre(cfg.espera) * .4;
    let sopro = 0;                          // 0..1, o avanço do bafo
    let t = 0;

    // As baforadas sobem em RODÍZIO, espalhadas na largada pra a coluna já nascer cheia — mesma
    // solução da fumaça da fogueira, e pelo mesmo motivo: todas em u=0 fariam uma bola só subindo.
    const baforadas = Array.from({ length: cfg.baforadas }, (_, i) => ({
        u: i / cfg.baforadas,
        vel: .14 + Math.random() * .12,
        raio: .65 + Math.random() * .7,
        giro: Math.random() * Math.PI * 2,
        ritmo: .5 + Math.random() * .9,
    }));

    let faiscas = [];

    return (ctx, dt) => {
        t += dt;
        relogio -= dt;

        if (fase === 'quieta' && relogio <= 0) { fase = 'soprando'; sopro = 0; }
        if (fase === 'soprando') {
            sopro += dt / cfg.soprar;
            if (sopro >= 1) { fase = 'quieta'; sopro = 0; relogio = entre(cfg.espera); }
        }
        // O bafo entra e sai suave: `sin(π·u)` em vez do valor cru, senão o vapor liga e desliga.
        const bafo = fase === 'soprando' ? Math.sin(sopro * Math.PI) : 0;

        const l = canvas.height * cfg.tamanho;
        const cx = canvas.width * cfg.x;
        const base = canvas.height * (areia + (1 - areia) * cfg.assentada);
        const bocaX = cx - l * .82, bocaY = base - l * .62;
        const pulso = .86 + Math.sin(t * 2.1) * .08 + Math.sin(t * 5.3) * .06;
        const forca = pulso + bafo * .5;
        const v = vento?.forca ?? 0;

        ctx.save();

        // 1. o CLARÃO. Ele é o que põe a lâmpada dentro da praia em vez de deixá-la colada em cima.
        const raio = l * cfg.clarao * forca;
        const clarao = ctx.createRadialGradient(cx, base - l * .3, 0, cx, base - l * .3, raio);
        clarao.addColorStop(0, `rgba(${cfg.luz}, ${.34 * forca})`);
        clarao.addColorStop(.4, `rgba(${cfg.luz}, ${.12 * forca})`);
        clarao.addColorStop(1, `rgba(${cfg.luz}, 0)`);
        ctx.fillStyle = clarao;
        ctx.beginPath();
        ctx.arc(cx, base - l * .3, raio, 0, Math.PI * 2);
        ctx.fill();

        // 2. a POÇA de luz na areia: achatada, porque é luz batendo no chão e não uma bola.
        const poca = ctx.createRadialGradient(cx, base, 0, cx, base, l * 2.4);
        poca.addColorStop(0, `rgba(${cfg.luz}, ${.3 * forca})`);
        poca.addColorStop(1, `rgba(${cfg.luz}, 0)`);
        ctx.fillStyle = poca;
        ctx.beginPath();
        ctx.ellipse(cx, base, l * 2.4, l * .5, 0, 0, Math.PI * 2);
        ctx.fill();

        // 3. o VAPOR. Sobe da boca do bico, enrola (a espiral é o `giro`) e abre no topo. Ele verga com
        //    o vento pelo mesmo desenho da coluna de fumaça: o desvio cresce com u², porque o pé está
        //    preso na boca da lâmpada e quem passeia é o alto.
        if (bafo > .01) {
            const alcance = canvas.height * cfg.alcance;
            for (const b of baforadas) {
                const u = (b.u + t * b.vel) % 1;
                const abre = l * (.2 + (cfg.abre - .2) * u) * b.raio;
                const x = bocaX + Math.sin(u * cfg.giro * Math.PI * 2 + b.giro) * l * .55 * u
                    + v * l * 3.2 * u * u;
                const y = bocaY - alcance * u;
                const alfa = Math.sin(Math.min(1, u * 1.2) * Math.PI) * .3 * bafo;
                const g = ctx.createRadialGradient(x, y, 0, x, y, abre);
                g.addColorStop(0, `rgba(${cfg.fumaca}, ${alfa})`);
                g.addColorStop(1, `rgba(${cfg.fumaca}, 0)`);
                ctx.fillStyle = g;
                ctx.beginPath();
                ctx.arc(x, y, abre, 0, Math.PI * 2);
                ctx.fill();
            }

            // as faíscas douradas dentro do vapor: são elas que dizem que o que sobe é MÁGICO, e não
            // fumaça de coisa queimando. Nascem no bico e morrem antes do topo, e `faiscas` é o TETO
            // de quantas cabem ao mesmo tempo — sem teto, um bafo longo vira um chafariz.
            if (faiscas.length < cfg.faiscas && Math.random() < bafo * .5) {
                faiscas.push({
                    x: bocaX, y: bocaY,
                    vx: (Math.random() - .5) * l * .5,
                    vy: -(.5 + Math.random() * .7) * canvas.height * cfg.alcance * .5,
                    vida: 1,
                });
            }
        }

        // Avança PRIMEIRO e descarta depois. Na ordem contrária, a faísca que morre neste quadro ainda
        // é desenhada com `vida` já negativa — e raio negativo no canvas LANÇA (ver `Math.max` abaixo).
        for (const f of faiscas) {
            f.vida -= dt * .55;
            f.x += (f.vx + v * l * 6) * dt;
            f.y += f.vy * dt;
            f.vy *= 1 - dt * .5;
            const viva = Math.max(0, f.vida);
            ctx.fillStyle = `rgba(${cfg.faisca}, ${viva * .9})`;
            ctx.beginPath();
            ctx.arc(f.x, f.y, l * .035 * viva, 0, Math.PI * 2);
            ctx.fill();
        }
        faiscas = faiscas.filter(f => f.vida > 0);

        // 4. a LÂMPADA, por cima da própria luz. Bojo baixo, bico comprido e alça — é a silhueta que
        //    todo mundo reconhece, e ela só funciona se o bico for LONGO: bico curto vira bule.
        const metal = ctx.createLinearGradient(cx, base - l * .9, cx, base);
        metal.addColorStop(0, cfg.metalLuz);
        metal.addColorStop(.45, cfg.metal);
        metal.addColorStop(1, cfg.metalSombra);

        ctx.strokeStyle = cfg.borda;
        ctx.lineWidth = l * .045;
        ctx.lineJoin = 'round';

        // A ALÇA, atrás do bojo. Ela é um TRAÇO, e nas outras peças o contorno preto sai do `stroke`
        // em cima do `fill` — coisa que um traço não tem. Por isso ela era a única parte da lâmpada
        // sem borda: aqui o contorno é um segundo traço, mais grosso e escuro, POR BAIXO do metal.
        ctx.beginPath();
        ctx.moveTo(cx + l * .42, base - l * .5);
        ctx.quadraticCurveTo(cx + l * 1.02, base - l * .62, cx + l * .58, base - l * .06);
        ctx.lineWidth = l * .21;
        ctx.strokeStyle = cfg.borda;
        ctx.stroke();
        ctx.lineWidth = l * .12;
        ctx.strokeStyle = cfg.metal;
        ctx.stroke();   // o mesmo caminho, por cima do contorno

        // o bojo
        ctx.fillStyle = metal;
        ctx.beginPath();
        ctx.moveTo(cx - l * .5, base - l * .12);
        ctx.quadraticCurveTo(cx - l * .62, base - l * .62, cx - l * .05, base - l * .66);
        ctx.quadraticCurveTo(cx + l * .58, base - l * .66, cx + l * .5, base - l * .14);
        ctx.quadraticCurveTo(cx + l * .44, base, cx - l * .04, base);
        ctx.quadraticCurveTo(cx - l * .46, base, cx - l * .5, base - l * .12);
        ctx.closePath();
        ctx.fill();
        ctx.lineWidth = l * .045;
        ctx.strokeStyle = cfg.borda;
        ctx.stroke();

        // o bico
        ctx.fillStyle = metal;
        ctx.beginPath();
        ctx.moveTo(cx - l * .4, base - l * .48);
        ctx.quadraticCurveTo(cx - l * .72, base - l * .5, bocaX, bocaY);
        ctx.lineTo(bocaX + l * .1, bocaY + l * .2);
        ctx.quadraticCurveTo(cx - l * .6, base - l * .3, cx - l * .38, base - l * .24);
        ctx.closePath();
        ctx.fill();
        ctx.stroke();

        // a tampa e o botão
        ctx.fillStyle = metal;
        ctx.beginPath();
        ctx.ellipse(cx - l * .02, base - l * .68, l * .2, l * .1, 0, 0, Math.PI * 2);
        ctx.fill();
        ctx.stroke();
        ctx.beginPath();
        ctx.arc(cx - l * .02, base - l * .82, l * .08, 0, Math.PI * 2);
        ctx.fill();
        ctx.stroke();

        // o fio de luz no bojo: uma lasca clara é o que separa metal de papel recortado
        ctx.strokeStyle = `rgba(${cfg.luz}, ${.5 + bafo * .4})`;
        ctx.lineWidth = l * .06;
        ctx.beginPath();
        ctx.moveTo(cx - l * .34, base - l * .46);
        ctx.quadraticCurveTo(cx - l * .1, base - l * .58, cx + l * .22, base - l * .5);
        ctx.stroke();

        ctx.restore();
    };
}

/// 🌊 O MAR — as ilhas, as ondas e a espuma na beira.
///
/// O mar era só um gradiente de CSS, ou seja, uma parede azul: água parada não existe, e a praia
/// dependia de haver um golfinho no ar naquele instante pra parecer viva. As três peças aqui fazem
/// trabalhos diferentes e nenhuma substitui a outra:
///
///   ILHAS   · profundidade. São a única referência de tamanho lá no fundo, e é comparando com elas
///             que a distância do horizonte vira uma distância de verdade.
///   ONDAS   · movimento. Rolam do horizonte pro raso com o avanço em u², que é o que as amontoa no
///             fundo e as abre na beira — perspectiva na água é isso, e sai de graça num expoente.
///   ESPUMA  · a BEIRA. É ela que costura o mar ao chão em que a luta acontece; sem ela a água
///             simplesmente encosta na areia e a praia fica com cara de dois retângulos empilhados.
///
/// Tudo aqui sai das linhas `--mar-linha` e `--areia-linha` do CSS, as mesmas que os golfinhos e a
/// lâmpada leem. Mexer nelas leva o mar inteiro junto.
export function criarMar(cfg, canvas) {
    const mar = medirDoTema('--mar-linha', 46) / 100;
    const areia = medirDoTema('--areia-linha', 71) / 100;

    // Mesma velocidade pra todas (a cadência sai daí), e o que varia é só a FORMA de cada uma.
    const ondas = Array.from({ length: cfg.ondas }, (_, i) => ({
        u: i / cfg.ondas,
        fase: Math.random() * Math.PI * 2,
        curva: .6 + Math.random() * .9,
    }));

    // AS LAVAGENS VIVAS. Cada onda que encosta na beira cria as suas, e elas vivem até o fim — várias
    // gerações se sobrepõem na areia ao mesmo tempo, que é o que a água faz.
    //
    // Antes havia um relógio só ("quanto tempo faz que a última onda chegou"), e como a lavagem dura
    // mais que o intervalo entre ondas, a chegada seguinte REINICIAVA a conta: a espuma que ainda
    // estava recuando sumia de uma vez pra a próxima começar. Uma lista resolve sem nenhum ajuste de
    // tempo — é a mesma diferença entre um evento e um estado.
    let lavagens = [];

    const lavar = () => {
        for (let i = 0; i < cfg.linguas; i++) {
            lavagens.push({
                // Sorteadas A CADA onda, e não fixas: em posições fixas, duas gerações sobrepostas
                // cairiam exatamente uma em cima da outra e o empilhamento não apareceria.
                x: (i + .5) / cfg.linguas + (Math.random() - .5) * .16,
                larg: .5 + Math.random() * .7,
                forca: .7 + Math.random() * .5,
                t: -entre(cfg.atraso),
            });
        }
    };

    return (ctx, dt) => {
        const yMar = canvas.height * mar;
        const yAreia = canvas.height * areia;
        const faixa = yAreia - yMar;

        ctx.save();

        // 1. AS ILHAS, assentadas na linha do horizonte. Silhueta e mais nada: a esta distância,
        //    volume viraria borrão — a mesma lição da mata do cemitério.
        for (const ilha of cfg.ilhas) {
            const cx = canvas.width * ilha.x;
            // Medidas em altura da arena (e não em largura) pra a ilha não esticar em tela larga.
            const w = canvas.height * ilha.largura;
            const h = canvas.height * ilha.altura;

            ctx.fillStyle = cfg.ilha;
            ctx.beginPath();
            ctx.moveTo(cx - w, yMar);
            for (let i = 0; i < ilha.picos; i++) {
                const a = -w + (2 * w * i) / ilha.picos;
                const b = -w + (2 * w * (i + 1)) / ilha.picos;
                // o pico do meio é o mais alto; os das pontas, ombros
                const alto = h * (i === Math.floor(ilha.picos / 2) ? 1 : .62);
                ctx.quadraticCurveTo(cx + (a + b) * .5, yMar - alto * 1.5, cx + b, yMar);
            }
            ctx.closePath();
            ctx.fill();

            // o fio de luz na encosta virada pro poente (o centro da tela), que é de onde vem a única
            // claridade que ainda resta no céu
            ctx.strokeStyle = cfg.ilhaLuz;
            ctx.lineWidth = Math.max(1, h * .05);
            ctx.beginPath();
            ctx.moveTo(cx - w * .1, yMar - h * .95);
            ctx.lineTo(cx + w * .55, yMar - h * .1);
            ctx.stroke();

            // e o REFLEXO na água, logo abaixo: uma mancha achatada e fraca. É o que impede a ilha de
            // parecer colada em cima do mar em vez de estar dentro dele.
            const r = ctx.createLinearGradient(cx, yMar, cx, yMar + h * 1.2);
            r.addColorStop(0, `rgba(${cfg.reflexo}, .18)`);
            r.addColorStop(1, `rgba(${cfg.reflexo}, 0)`);
            ctx.fillStyle = r;
            ctx.fillRect(cx - w * .8, yMar, w * 1.6, h * 1.2);
        }

        // 2. AS ONDAS rolando pro raso.
        for (const o of ondas) {
            o.u += cfg.velocidade * dt;
            // Chegou na beira: recomeça no horizonte e deixa uma lavagem na areia.
            if (o.u > 1) { o.u -= 1; lavar(); }

            const y = yMar + faixa * o.u * o.u;
            const amp = faixa * .014 * (.35 + o.u);
            // entra suave no horizonte e some ao chegar na beira, onde a espuma assume
            const alfa = Math.min(1, o.u * 4) * Math.min(1, (1 - o.u) * 3.5) * cfg.alfa;
            ctx.strokeStyle = `rgba(${cfg.onda}, ${alfa})`;
            ctx.lineWidth = Math.max(.7, cfg.espessura * (.25 + o.u));
            ctx.beginPath();
            for (let x = 0; x <= canvas.width; x += 20) {
                const yy = y + Math.sin((x / canvas.width) * Math.PI * 2 * (2 + o.curva * 3) + o.fase) * amp;
                if (x === 0) ctx.moveTo(x, yy); else ctx.lineTo(x, yy);
            }
            ctx.stroke();
        }

        // 3. A ESPUMA na areia: todas as lavagens vivas, de todas as ondas que chegaram. Cada língua é
        //    meia elipse que sobe pra dentro da praia e recua, e elas se sobrepõem à vontade.
        for (const l of lavagens) l.t += dt;
        lavagens = lavagens.filter(l => l.t < cfg.lavar);

        for (const l of lavagens) {
            const q = l.t / cfg.lavar;
            if (q <= 0) continue;

            // A envoltória é ASSIMÉTRICA (o expoente .6 no `q`): a água sobe rápido e volta devagar,
            // que é o que separa uma lavagem de um pulsar. Simétrica, a beira parecia respirar.
            const env = Math.sin(Math.PI * Math.pow(q, .6));
            const sobe = canvas.height * cfg.avanco * l.forca * env;
            if (sobe < .5) continue;
            const cx = canvas.width * l.x;
            const w = canvas.width * .16 * l.larg;

            ctx.fillStyle = `rgba(${cfg.espuma}, ${.22 * env})`;
            ctx.beginPath();
            ctx.ellipse(cx, yAreia, w, sobe, 0, 0, Math.PI);
            ctx.fill();

            // a beirada, mais clara: é a linha branca que a água deixa no ponto mais alto que alcançou
            ctx.strokeStyle = `rgba(${cfg.espuma}, ${.45 * env})`;
            ctx.lineWidth = 1.6;
            ctx.beginPath();
            ctx.ellipse(cx, yAreia, w, sobe, 0, 0, Math.PI);
            ctx.stroke();
        }

        ctx.restore();
    };
}

/// 🧜 O MAR: os golfinhos saltando, e de vez em quando UMA CAUDA no lugar de um deles.
///
/// A sereia entra por CONTRASTE, e é por isso que os golfinhos têm de existir primeiro: eles são
/// escuros, rápidos e vêm em grupo; ela é turquesa acesa, sozinha, maior e mais lenta. Coisa diferente
/// no meio de um padrão já estabelecido lê como acontecimento — sozinha, ela seria só um desenho.
///
/// E ela é SÓ A CAUDA, pela regra dos corpos humanos. Não é uma concessão: cauda rompendo a água é a
/// imagem que a sereia tem no imaginário, e o torso ia obrigar a desenhar rosto e braços a 40px.
///
/// A PROFUNDIDADE é o que dá o mar. Cada salto sorteia um lugar entre a linha do horizonte e a beira
/// d'água (as duas lidas do CSS), e daí saem juntos a altura na tela e o TAMANHO — longe é pequeno e
/// no raso é grande. Sem isso, todos saltariam do mesmo tamanho e o mar viraria uma parede azul.
export function criarGolfinhos(cfg, canvas) {
    const mar = medirDoTema('--mar-linha', 46) / 100;
    const areia = medirDoTema('--areia-linha', 71) / 100;

    let relogio = entre(cfg.espera) * .3;
    let saltos = [];
    let respingos = [];

    const superficieDe = (prof) => canvas.height * (mar + (areia - mar) * prof);
    const tamanhoDe = (prof, escala) =>
        canvas.height * (cfg.tamanho[0] + (cfg.tamanho[1] - cfg.tamanho[0]) * prof) * escala;

    const lancar = () => {
        const sereia = Math.random() < cfg.chanceSereia;
        // Ela vem SEMPRE mais perto: a cauda tem detalhe, e detalhe pequeno vira sujeira — a mesma
        // lição do Ninja e do Caixão, aqui virando uma faixa de sorteio em vez de um comentário.
        const prof = sereia ? entre(cfg.sereiaProfundidade) : entre(cfg.profundidade);
        const dir = Math.random() < .5 ? 1 : -1;
        const x = canvas.width * (.1 + Math.random() * .8);
        const quantos = sereia ? 1 : Math.round(entre(cfg.grupo));

        for (let i = 0; i < quantos; i++) {
            saltos.push({
                sereia, prof, dir,
                // O grupo sai em fila e escalonado: golfinho pula em bando, mas não em bloco.
                x: x - dir * i * canvas.width * .045,
                atraso: i * cfg.atraso,
                t: 0,
                dur: sereia ? cfg.sereiaDuracao : entre(cfg.duracao),
                escala: sereia ? cfg.sereiaTamanho : 1,
                fora: false,
            });
        }
    };

    const respingar = (x, y, tam) => respingos.push({ x, y, tam, t: 0 });

    /// Respinga na hora em que o bicho ATRAVESSA a linha d'água — nos dois sentidos. Antes o respingo
    /// saía do começo e do fim do relógio do salto, que é quase a mesma coisa e erra justamente onde se
    /// olha: a espuma aparecia antes de haver o que a levantasse.
    const cruzou = (s, fora, x, y, tam) => {
        if (fora === s.fora) return;
        s.fora = fora;
        respingar(x, y, tam);
    };

    return (ctx, dt) => {
        relogio -= dt;
        if (relogio <= 0) { lancar(); relogio = entre(cfg.espera); }

        ctx.save();

        // Os respingos primeiro: eles são ÁGUA e ficam atrás de quem saiu dela.
        // Mesma ordem da faísca: avança, desenha com o valor CLAMPADO, e só então descarta. O `q` passa
        // de 1 no quadro em que o respingo acaba, e `1 − q` viraria um raio negativo.
        for (const r of respingos) {
            r.t += dt * 1.6;
            const q = Math.min(1, r.t);
            const some = 1 - q;
            ctx.strokeStyle = `rgba(${cfg.espuma}, ${some * .55})`;
            ctx.lineWidth = r.tam * .07;
            ctx.beginPath();
            ctx.ellipse(r.x, r.y, r.tam * (.3 + q * 1.1), r.tam * (.08 + q * .24), 0, 0, Math.PI * 2);
            ctx.stroke();
            for (let i = 0; i < 5; i++) {
                const ang = Math.PI + (i / 4) * Math.PI;
                const d = r.tam * q * 1.2;
                ctx.fillStyle = `rgba(${cfg.espuma}, ${some * .7})`;
                ctx.beginPath();
                ctx.arc(r.x + Math.cos(ang) * d, r.y + Math.sin(ang) * d * .5 - r.tam * q * .5,
                    r.tam * .05 * some, 0, Math.PI * 2);
                ctx.fill();
            }
        }
        respingos = respingos.filter(r => r.t < 1);

        saltos = saltos.filter(s => s.t < s.dur);
        // Os mais FUNDOS primeiro: quem está perto passa na frente de quem está longe.
        for (const s of [...saltos].sort((a, b) => a.prof - b.prof)) {
            if (s.atraso > 0) { s.atraso -= dt; continue; }
            s.t += dt;

            const q = Math.min(1, s.t / s.dur);
            const superficie = superficieDe(s.prof);
            const tam = tamanhoDe(s.prof, s.escala);

            if (q >= 1) continue;

            if (s.sereia) {
                // ELA NÃO SALTA. O que rompe a água é a PONTA do rabo, que sobe, abana e afunda no
                // mesmo lugar — e a razão é a mesma que tirou o corpo dela do desenho: cauda sozinha
                // descrevendo o arco de um golfinho não lê como sereia, lê como pedaço solto sendo
                // arremessado. Sem torso pra explicar o impulso, o salto denuncia o que falta.
                const exposto = tam * cfg.sereiaAltura * Math.sin(Math.PI * q);
                // O respingo sai do CRUZAMENTO da linha d'água, não do começo e do fim do relógio: é
                // quando a água é de fato rompida, na subida e na descida.
                cruzou(s, exposto > tam * .1, s.x, superficie, tam);

                ctx.save();
                // O RECORTE na linha d'água é o que dá SUPERFÍCIE ao mar: o que está abaixo dela
                // simplesmente não é pintado, então o rabo SAI da água em vez de estar na frente dela.
                // É a peça que faz a emergência funcionar, e custa três linhas.
                ctx.beginPath();
                ctx.rect(0, 0, canvas.width, superficie);
                ctx.clip();
                ctx.translate(s.x, superficie);
                desenharCaudaDeSereia(ctx, tam, q, exposto, cfg);
                ctx.restore();
                continue;
            }

            // A PARÁBOLA do golfinho. O ângulo sai da derivada dela, e é isso que o faz sair de nariz
            // pra cima, virar no alto e entrar de nariz pra baixo — sem isso ele voa deitado.
            //
            // O arco começa e termina ABAIXO da linha d'água (`mergulho`), e o que está submerso é
            // recortado. É isso que faz ele SAIR da água em partes — focinho, dorso, cauda — em vez de
            // aparecer inteiro do nada em cima dela e sumir do mesmo jeito. Mesma solução da sereia, e
            // a mesma razão: a superfície só existe se alguma coisa for cortada por ela.
            const arco = Math.sin(Math.PI * q);
            const alturaSalto = tam * cfg.salto;
            const fundo = tam * cfg.mergulho;
            const x = s.x + s.dir * tam * cfg.avanco * (q - .5);
            const y = superficie + fundo - (alturaSalto + fundo) * arco;
            const dx = s.dir * tam * cfg.avanco;
            const dy = -(alturaSalto + fundo) * Math.PI * Math.cos(Math.PI * q);

            cruzou(s, y < superficie, x, superficie, tam);

            ctx.save();
            ctx.beginPath();
            ctx.rect(0, 0, canvas.width, superficie);
            ctx.clip();
            ctx.translate(x, y);
            ctx.rotate(Math.atan2(dy, dx));
            // Girar pelo ângulo já aponta o focinho pro caminho; quando ele vai pra esquerda o giro
            // passa de 90°, e sem este espelho o bicho atravessa de barriga pra cima.
            if (s.dir < 0) ctx.scale(1, -1);
            desenharGolfinho(ctx, tam, cfg);
            ctx.restore();
        }

        ctx.restore();
    };
}

/// Um GOLFINHO, de perfil e apontado pro +x. Ele é uma lente com bico: corpo que engrossa no meio,
/// afina no pedúnculo e abre na cauda. O que o faz ler a 30px é o BICO e a nadadeira dorsal — o resto
/// pode ser massa. Olho e boca ficariam com 1px cada e só sujariam a silhueta.
export function desenharGolfinho(ctx, s, cfg) {
    const g = ctx.createLinearGradient(0, -s * .3, 0, s * .3);
    g.addColorStop(0, cfg.corpo);
    g.addColorStop(.55, cfg.corpoLuz);
    g.addColorStop(1, cfg.ventre);

    // a cauda, atrás do corpo
    ctx.fillStyle = cfg.corpo;
    ctx.beginPath();
    ctx.moveTo(-s * .58, 0);
    ctx.quadraticCurveTo(-s * .78, -s * .3, -s * .98, -s * .26);
    ctx.quadraticCurveTo(-s * .8, 0, -s * .98, s * .26);
    ctx.quadraticCurveTo(-s * .78, s * .3, -s * .58, 0);
    ctx.closePath();
    ctx.fill();

    // o corpo
    ctx.fillStyle = g;
    ctx.beginPath();
    ctx.moveTo(s * .92, s * .02);
    ctx.quadraticCurveTo(s * .5, -s * .3, -s * .1, -s * .28);
    ctx.quadraticCurveTo(-s * .5, -s * .22, -s * .6, -s * .04);
    ctx.quadraticCurveTo(-s * .5, s * .16, -s * .1, s * .26);
    ctx.quadraticCurveTo(s * .45, s * .3, s * .92, s * .02);
    ctx.closePath();
    ctx.fill();

    // a dorsal e a peitoral
    ctx.fillStyle = cfg.corpo;
    ctx.beginPath();
    ctx.moveTo(s * .06, -s * .26);
    ctx.quadraticCurveTo(-s * .04, -s * .62, -s * .28, -s * .5);
    ctx.quadraticCurveTo(-s * .2, -s * .3, -s * .22, -s * .24);
    ctx.closePath();
    ctx.fill();
    ctx.beginPath();
    ctx.moveTo(s * .18, s * .18);
    ctx.quadraticCurveTo(s * .02, s * .5, -s * .16, s * .44);
    ctx.quadraticCurveTo(s * .02, s * .28, s * .06, s * .16);
    ctx.closePath();
    ctx.fill();
}

/// A CAUDA DA SEREIA, desenhada DA LINHA D'ÁGUA PRA CIMA (a origem é o ponto em que ela rompe o mar, e
/// quem chama recorta tudo o que está abaixo). `exposto` é o quanto de rabo está fora da água neste
/// instante — sobe até o pico e volta a zero, e é só isso que acontece: ela não viaja e não gira.
///
/// O desenho inteiro é o CONTRASTE com o golfinho: cor acesa em vez de escura, leque aberto em vez de
/// meia-lua, e um fio de escamas douradas que golfinho nenhum tem. É o contraste que faz a troca ser
/// notada — se ela fosse parecida, seria só mais um pulo.
///
/// O `abana` do leque é o que a mantém VIVA enquanto está fora: rabo rígido subindo e descendo lê como
/// objeto empurrado por baixo. E o pedaço submerso (`s * .4` abaixo da origem) existe pra o recorte ter
/// o que cortar — sem ele, a base da cauda ficaria exatamente na linha e apareceria uma emenda reta.
export function desenharCaudaDeSereia(ctx, s, q, exposto, cfg) {
    const abana = Math.sin(q * Math.PI * 3) * .26;
    const submerso = s * .4;
    const alto = exposto + submerso;
    // O eixo: sobe da água até a ponta, e a inclinação cresce com u² — a raiz está presa na água e
    // quem passeia é a ponta. É a mesma conta do tronco da palmeira e da coluna de fumaça.
    const eixo = (u) => ({ x: abana * exposto * u * u, y: submerso - alto * u });

    ctx.save();

    const g = ctx.createLinearGradient(0, -exposto, 0, submerso);
    g.addColorStop(0, cfg.caudaBrilho);
    g.addColorStop(.45, cfg.caudaLuz);
    g.addColorStop(1, cfg.cauda);

    // O RABO: fita que afina de baixo (grossa, na água) até o pedúnculo.
    const passos = 10, esq = [], dir = [];
    for (let i = 0; i <= passos; i++) {
        const u = i / passos, p = eixo(u), w = s * (.3 - u * .21);
        esq.push({ x: p.x - w, y: p.y });
        dir.push({ x: p.x + w, y: p.y });
    }
    ctx.fillStyle = g;
    ctx.beginPath();
    ctx.moveTo(esq[0].x, esq[0].y);
    for (const p of esq) ctx.lineTo(p.x, p.y);
    for (let i = dir.length - 1; i >= 0; i--) ctx.lineTo(dir[i].x, dir[i].y);
    ctx.closePath();
    ctx.fill();

    // As ESCAMAS: arcos horizontais subindo pelo rabo. É o ouro delas que grita "não é golfinho".
    ctx.strokeStyle = cfg.escama;
    ctx.lineWidth = s * .03;
    for (const u of [.25, .5, .72]) {
        const p = eixo(u);
        ctx.beginPath();
        ctx.arc(p.x, p.y, s * (.22 - u * .1), Math.PI * .18, Math.PI * .82);
        ctx.stroke();
    }

    // A LIGAÇÃO (o pedúnculo), no OUTRO tom: é a peça que faltava. Sem ela o rabo e o leque eram duas
    // formas da mesma cor se encostando, e a barbatana parecia inacabada — via-se a emenda, não a
    // junta. Um anel mais claro ali resolve as duas coisas de uma vez: fecha a silhueta e dá o
    // contraste no ponto pra onde o olho vai, que é a ponta.
    const topo = eixo(1);
    ctx.translate(topo.x, topo.y);
    ctx.rotate(abana * .9);

    ctx.fillStyle = cfg.juncao;
    ctx.beginPath();
    ctx.ellipse(0, s * .06, s * .13, s * .09, 0, 0, Math.PI * 2);
    ctx.fill();

    // O LEQUE: UMA peça fechada, com o entalhe no meio — e não duas lâminas soltas. É o entalhe que
    // faz a nadadeira ser uma nadadeira; duas pontas sem nada entre elas leem como forquilha.
    ctx.fillStyle = cfg.caudaLuz;
    ctx.beginPath();
    ctx.moveTo(0, s * .1);
    ctx.quadraticCurveTo(-s * .3, -s * .16, -s * .78, -s * .82);   // sobe pela beira esquerda
    ctx.quadraticCurveTo(-s * .34, -s * .5, 0, -s * .3);           // desce até o ENTALHE do meio
    ctx.quadraticCurveTo(s * .34, -s * .5, s * .78, -s * .82);     // e sobe de novo pela direita
    ctx.quadraticCurveTo(s * .3, -s * .16, 0, s * .1);
    ctx.closePath();
    ctx.fill();

    // os raios da nadadeira, no tom da ligação: três riscos que saem da junta pras pontas
    ctx.strokeStyle = cfg.juncao;
    ctx.lineWidth = s * .026;
    for (const r of [-.62, -.24, .24, .62]) {
        ctx.beginPath();
        ctx.moveTo(0, s * .02);
        ctx.lineTo(r * s * .92, -s * (.5 + Math.abs(r) * .38));
        ctx.stroke();
    }

    // o fio claro na beira: é o que faz a nadadeira ter borda em vez de virar mancha
    ctx.strokeStyle = cfg.caudaBrilho;
    ctx.lineWidth = s * .03;
    ctx.beginPath();
    ctx.moveTo(0, s * .1);
    ctx.quadraticCurveTo(-s * .3, -s * .16, -s * .78, -s * .82);
    ctx.quadraticCurveTo(-s * .34, -s * .5, 0, -s * .3);
    ctx.quadraticCurveTo(s * .34, -s * .5, s * .78, -s * .82);
    ctx.quadraticCurveTo(s * .3, -s * .16, 0, s * .1);
    ctx.stroke();

    ctx.restore();
}

/// 🧚 OS VAGA-LUMES, e a FADA que é um deles maior.
///
/// É a peça mais barata do tema e a que mais rende, porque ela resolve a fada sem desenhar fada: num
/// ar cheio de pontinhos acesos, o que aparece de vez em quando é UM que é maior, mais claro e que
/// deixa RASTRO. Vaga-lume acende no lugar; ela risca o ar. É a diferença de comportamento que diz
/// quem é quem, e nenhuma anatomia precisou ser desenhada — que era o pedido.
///
/// Eles vivem numa FAIXA (do meio do mar até a areia) e não na tela toda: no céu virariam estrelas, e
/// estrela é assinatura dos Tecnológicos. A regra de não repetir a assinatura de ninguém vale também
/// pras peças pequenas.
///
/// Cada um pisca no seu ritmo, como as corujas e as labaredas. Junto, o ar inteiro ligaria e
/// desligaria — que é o defeito clássico e o mais fácil de cometer.
export function criarVagalumes(cfg, canvas, vento) {
    const nova = () => ({
        x: Math.random() * canvas.width,
        y: canvas.height * entre(cfg.faixa),
        r: entre(cfg.raio),
        deriva: entre(cfg.deriva),
        fase: Math.random() * Math.PI * 2,
        ritmo: .25 + Math.random() * .5,
        pisca: entre(cfg.pisca),
        fasePisca: Math.random() * Math.PI * 2,
    });

    let bichos = Array.from({ length: cfg.quantos }, nova);

    // AS FADAS: mesma máquina de sempre — espera, atravessa, some —, uma por bicho. São VÁRIAS, e cada
    // uma com o próprio relógio: a primeira espera é sorteada inteira (`Math.random()`) e não pela
    // metade, senão todas estreariam quase juntas e a cena abriria com um comboio de fadas.
    //
    // O rastro é um anel de posições antigas, e ele é POR FADA. Uma lista só, compartilhada, ligaria a
    // última posição de uma na primeira da outra e riscaria a tela de ponta a ponta.
    const fadas = Array.from({ length: cfg.fadas }, () => ({
        fase: 'fora',
        relogio: entre(cfg.fadaEspera) * Math.random(),
        rastro: [],
        v: null,
    }));
    let t = 0;

    return (ctx, dt) => {
        t += dt;
        const sopro = (vento?.forca ?? 0) * cfg.sopro * canvas.width;

        for (let i = 0; i < bichos.length; i++) {
            const b = bichos[i];
            b.fase += dt * b.ritmo;
            b.fasePisca += dt * b.pisca;
            // Passeio: dois senos fora de compasso, e o vento por cima. Sem o vento eles seguiriam o
            // próprio passeio durante a rajada, e a cena se dividiria em quem obedece e quem não.
            b.x += (Math.sin(b.fase * 1.7) * b.deriva + sopro) * dt;
            b.y += Math.cos(b.fase * 1.1) * b.deriva * .5 * dt;
            if (b.x < -20 || b.x > canvas.width + 20) bichos[i] = nova();

            const aceso = Math.max(0, Math.sin(b.fasePisca));
            if (aceso <= .01) continue;
            const halo = ctx.createRadialGradient(b.x, b.y, 0, b.x, b.y, b.r * 5);
            halo.addColorStop(0, `rgba(${cfg.cor}, ${.5 * aceso})`);
            halo.addColorStop(1, `rgba(${cfg.cor}, 0)`);
            ctx.fillStyle = halo;
            ctx.beginPath();
            ctx.arc(b.x, b.y, b.r * 5, 0, Math.PI * 2);
            ctx.fill();
            ctx.fillStyle = `rgba(${cfg.cor}, ${.9 * aceso})`;
            ctx.beginPath();
            ctx.arc(b.x, b.y, b.r, 0, Math.PI * 2);
            ctx.fill();
        }

        for (const f of fadas) {
            f.relogio -= dt;
            if (f.fase === 'fora' && f.relogio <= 0) {
                const dir = Math.random() < .5 ? 1 : -1;
                f.fase = 'voando';
                f.rastro = [];
                f.v = {
                    dir,
                    x0: dir > 0 ? -canvas.width * .1 : canvas.width * 1.1,
                    x1: dir > 0 ? canvas.width * 1.1 : -canvas.width * .1,
                    base: canvas.height * (cfg.faixa[0] + Math.random() * (cfg.faixa[1] - cfg.faixa[0]) * .7),
                    subida: canvas.height * (.06 + Math.random() * .12),
                    // Cada uma bate asa no seu compasso: em fase, quatro fadas piscariam como um só
                    // efeito — a mesma armadilha das corujas e das labaredas.
                    baterFase: Math.random() * Math.PI * 2,
                    dur: entre(cfg.fadaAtravessar),
                    t: 0,
                };
            }

            if (f.fase !== 'voando') continue;

            const fada = f.v;
            const rastro = f.rastro;
            fada.t += dt;
            const q = fada.t / fada.dur;
            if (q >= 1) { f.fase = 'fora'; f.relogio = entre(cfg.fadaEspera); }
            else {
                const x = fada.x0 + (fada.x1 - fada.x0) * q;
                // Um ARCO, e não uma reta: ela sobe e desce enquanto atravessa, com um bambolear por
                // cima. Reta lê como projétil.
                const y = fada.base - Math.sin(q * Math.PI) * fada.subida
                    + Math.sin(t * 3.1 + fada.baterFase) * canvas.height * .012;
                const alfa = Math.min(1, q * 6, (1 - q) * 6);
                const s = fada.dir;

                rastro.push({ x, y });
                if (rastro.length > cfg.fadaRastro) rastro.shift();
                for (let i = 0; i < rastro.length; i++) {
                    const p = rastro[i], u = i / rastro.length;
                    ctx.fillStyle = `rgba(${cfg.fada}, ${u * u * .4 * alfa})`;
                    ctx.beginPath();
                    ctx.arc(p.x, p.y, cfg.raio[1] * cfg.fadaTamanho * .3 * u, 0, Math.PI * 2);
                    ctx.fill();
                }

                const r = cfg.raio[1] * cfg.fadaTamanho;
                // AS ASAS: dois riscos que batem rápido demais pra se ver a forma — que é justamente o
                // que se vê de uma asa de inseto, e o que dispensa desenhar uma.
                const bate = Math.abs(Math.sin(t * 22 + fada.baterFase));
                ctx.fillStyle = `rgba(${cfg.asa}, ${.28 * alfa})`;
                for (const lado of [-1, 1]) {
                    // O espelho das asas sai do DESLOCAMENTO e do GIRO, nunca de um raio negativo:
                    // `ellipse` com raio negativo lança IndexSizeError, e uma exceção aqui dentro mata
                    // o requestAnimationFrame — a cena inteira congela em silêncio.
                    ctx.beginPath();
                    ctx.ellipse(x - s * r * .25 + lado * r * .35, y - r * .5,
                        r * .95, r * (.26 + bate * .3), lado * .7, 0, Math.PI * 2);
                    ctx.fill();
                }

                const halo = ctx.createRadialGradient(x, y, 0, x, y, r * 6);
                halo.addColorStop(0, `rgba(${cfg.fada}, ${.55 * alfa})`);
                halo.addColorStop(1, `rgba(${cfg.fada}, 0)`);
                ctx.fillStyle = halo;
                ctx.beginPath();
                ctx.arc(x, y, r * 6, 0, Math.PI * 2);
                ctx.fill();
                ctx.fillStyle = `rgba(${cfg.asa}, ${.95 * alfa})`;
                ctx.beginPath();
                ctx.arc(x, y, r * .5, 0, Math.PI * 2);
                ctx.fill();
            }
        }
    };
}

/// 🌴 AS PALMEIRAS que emolduram a praia, arqueando pro centro.
///
/// É o único tema em que a MOLDURA é canvas — nos outros quatro ela é o `::before`/`::after` do CSS —,
/// e o motivo é o vento: elas vergam quando o dragão passa raspando, e pseudo-elemento não lê JS. Foi
/// o cenário pedindo a camada, e não a camada procurando serviço.
///
/// Elas ficam no canvas do FUNDO, atrás dos combatentes. Moldura de PRIMEIRO PLANO não vinga aqui —
/// já foi tentada e caiu duas vezes: coisa grande e perto obriga a acertar o traço, e traço errado
/// em cima da luta é pior que cenário nenhum.
///
/// O tronco verga por u² — o pé está fincado na areia e quem passeia é a copa. É a mesma conta da
/// coluna de fumaça e do eixo do redemoinho; é sempre essa a forma de uma coisa presa embaixo.
export function criarPalmeiras(cfg, canvas, vento) {
    // Guardadas em FRAÇÕES: mudar o tamanho da janela muda a escala da praia, não o lugar das árvores.
    // A faixa de `x` é dividida entre as duas de cada lado: a da FRENTE (i 0) fica na metade de fora e
    // a de TRÁS na de dentro. Sorteando as duas na faixa inteira, elas caíam vizinhas com frequência —
    // e duas palmeiras encostadas leem como uma só, grossa. Assim elas nascem sempre separadas, e o
    // conjunto todo mora mais perto da borda, longe de onde a luta acontece.
    const faixaX = (i) => {
        const [a, b] = cfg.x, meio = a + (b - a) * .45;
        return i === 0 ? a + Math.random() * (meio - a) : meio + Math.random() * (b - meio);
    };

    const criar = (lado, i) => ({
        // `dobra` é pra onde ela se inclina: sempre pro CENTRO, que é o que fecha o arco.
        dobra: lado < 0 ? 1 : -1,
        x: lado < 0 ? faixaX(i) : 1 - faixaX(i),
        // A segunda de cada lado é menor e vem antes (mais longe): duas do mesmo tamanho leriam como
        // uma copiada.
        altura: entre(cfg.altura) * (i === 0 ? 1 : .74),
        atras: i > 0,
        inclinacao: entre(cfg.inclinacao),
        balanco: entre(cfg.balanco),
        ritmo: entre(cfg.ritmo),
        fase: Math.random() * Math.PI * 2,
        folhas: Array.from({ length: cfg.folhas }, () => ({
            comprimento: entre(cfg.folhaComprimento),
            caida: .3 + Math.random() * .45,
        })),
    });

    const arvores = [];
    for (let i = 0; i < cfg.porLado; i++) { arvores.push(criar(-1, i)); arvores.push(criar(1, i)); }
    arvores.sort((a, b) => Number(b.atras) - Number(a.atras));   // as de trás pintam primeiro

    let t = 0;

    return (ctx, dt) => {
        t += dt;
        const v = vento?.forca ?? 0;

        for (const a of arvores) {
            // Três coisas somadas num número só: a inclinação de nascença, o balanço de clima (cada
            // uma no seu ritmo) e a rajada. Quem desenha não precisa saber de onde veio cada parcela.
            const desvio = a.inclinacao * a.dobra
                + Math.sin(t * a.ritmo + a.fase) * a.balanco
                + v * cfg.ganhoDoVento;
            desenharPalmeira(ctx, a, canvas, desvio, t, cfg);
        }
    };
}

export function desenharPalmeira(ctx, a, canvas, desvio, t, cfg) {
    const alt = canvas.height * a.altura;
    const x0 = canvas.width * a.x;
    const base = canvas.height;
    const noTronco = (u) => ({ x: x0 + desvio * alt * u * u, y: base - alt * u });
    const topo = noTronco(1);
    // As de trás são menores e mais escuras: profundidade sai de tamanho + contraste, e a segunda
    // metade é a que a maioria esquece.
    const fundo = a.atras;

    ctx.save();

    // O TRONCO, como fita que afina: amostrado em passos e fechado num polígono só. Curva de largura
    // variável não existe em canvas — quem quer isso constrói as duas margens, como o tentáculo faz.
    const passos = 12;
    const larg = alt * .04;
    const esq = [], dir = [];
    for (let i = 0; i <= passos; i++) {
        const u = i / passos;
        const p = noTronco(u);
        const w = larg * (1 - u * .5);
        esq.push({ x: p.x - w, y: p.y });
        dir.push({ x: p.x + w, y: p.y });
    }
    const g = ctx.createLinearGradient(x0 - larg, 0, x0 + larg, 0);
    g.addColorStop(0, cfg.tronco);
    g.addColorStop(.65, fundo ? cfg.tronco : cfg.troncoLuz);
    g.addColorStop(1, cfg.tronco);
    ctx.fillStyle = g;
    ctx.beginPath();
    ctx.moveTo(esq[0].x, esq[0].y);
    for (const p of esq) ctx.lineTo(p.x, p.y);
    for (let i = dir.length - 1; i >= 0; i--) ctx.lineTo(dir[i].x, dir[i].y);
    ctx.closePath();
    ctx.fill();

    // Os anéis do tronco. São três riscos e resolvem o que textura nenhuma resolveria a esta escala.
    ctx.strokeStyle = cfg.tronco;
    ctx.lineWidth = alt * .012;
    for (let i = 1; i <= 5; i++) {
        const p = noTronco(i / 6);
        ctx.beginPath();
        ctx.moveTo(p.x - larg * .8, p.y);
        ctx.lineTo(p.x + larg * .8, p.y);
        ctx.stroke();
    }

    // OS COCOS, agrupados na coroa. Três bolinhas: é o que diz "coqueiro" sem custar nada.
    ctx.fillStyle = cfg.coco;
    for (let i = 0; i < cfg.cocos; i++) {
        const ang = Math.PI * (.15 + i * .25);
        ctx.beginPath();
        ctx.ellipse(topo.x + Math.cos(ang) * larg * 1.5, topo.y + larg * .9 + Math.sin(ang) * larg * .5,
            larg * .55, larg * .5, 0, 0, Math.PI * 2);
        ctx.fill();
    }

    // AS FOLHAS, em leque. O ângulo é distribuído e não sorteado: leque com buraco lê como palmeira
    // doente, e a variação que interessa (comprimento e queda) já está em cada folha.
    for (let k = 0; k < a.folhas.length; k++) {
        const f = a.folhas[k];
        const u = a.folhas.length === 1 ? .5 : k / (a.folhas.length - 1);
        const ang = -Math.PI * .96 + u * Math.PI * .92;
        // A folha também sente o vento, e mais que o tronco: ela é a parte leve da árvore.
        const sopro = desvio * .5 + Math.sin(t * a.ritmo * 1.7 + k) * .04;
        desenharFolhaDePalmeira(ctx, topo.x, topo.y, ang, f.comprimento * alt,
            f.caida + sopro * .6, sopro, fundo, cfg);
    }

    ctx.restore();
}

/// Uma FOLHA de palmeira: uma lâmina SÓLIDA e serrilhada dos dois lados, com a nervura no meio.
///
/// Ela já foi feita de traços — a nervura e dois riscos por folíolo —, e a copa inteira lia como um
/// punhado de fios: de perto virava um emaranhado de linhas, e contra o céu claro do horizonte quase
/// sumia. Folha é uma SUPERFÍCIE; o que a faz existir é ter área e recortar o fundo. O serrilhado
/// (alternar largura cheia e curta ao longo da borda) dá os folíolos sem desenhar um por um: é a
/// silhueta que conta, não a contagem.
///
/// O que ficou de traço é só a nervura, POR CIMA da lâmina, dividindo as duas metades. Sem ela, a
/// folha fica um chinelo.
export function desenharFolhaDePalmeira(ctx, x0, y0, ang, comp, caida, sopro, fundo, cfg) {
    const pontos = [];
    for (let i = 0; i <= cfg.foliolos; i++) {
        const u = i / cfg.foliolos;
        // A queda entra como u²: a raiz sai reta e a ponta despenca. Linear daria um risco torto.
        const x = x0 + Math.cos(ang) * comp * u + sopro * comp * u * u;
        const y = y0 + Math.sin(ang) * comp * u + caida * comp * u * u;
        pontos.push({ u, x, y });
    }

    // a normal de cada ponto, pra saber pra onde a lâmina abre
    for (let i = 0; i < pontos.length; i++) {
        const a = pontos[Math.max(0, i - 1)], b = pontos[Math.min(pontos.length - 1, i + 1)];
        const dx = b.x - a.x, dy = b.y - a.y, n = Math.hypot(dx, dy) || 1;
        pontos[i].nx = -dy / n;
        pontos[i].ny = dx / n;
        // Larga no meio e fechando nas duas pontas — folha de coqueiro é uma lente, não um retângulo.
        // O serrilhado alterna cheio e curto: é o que dá o recorte dos folíolos na borda.
        pontos[i].w = comp * cfg.folhaLargura * Math.sin(pontos[i].u * Math.PI * .92)
            * (i % 2 ? 1 : .58);
    }

    const borda = (lado) => {
        for (const p of pontos) ctx.lineTo(p.x + p.nx * p.w * lado, p.y + p.ny * p.w * lado);
    };

    ctx.fillStyle = fundo ? cfg.folha : cfg.folhaLuz;
    ctx.beginPath();
    ctx.moveTo(pontos[0].x, pontos[0].y);
    borda(1);
    for (let i = pontos.length - 1; i >= 0; i--) {
        const p = pontos[i];
        ctx.lineTo(p.x - p.nx * p.w, p.y - p.ny * p.w);
    }
    ctx.closePath();
    ctx.fill();

    // a NERVURA por cima, no tom escuro: é ela que separa as duas metades da folha
    ctx.strokeStyle = cfg.folha;
    ctx.lineWidth = comp * .022;
    ctx.lineCap = 'round';
    ctx.beginPath();
    ctx.moveTo(pontos[0].x, pontos[0].y);
    for (const p of pontos) ctx.lineTo(p.x, p.y);
    ctx.stroke();
}
