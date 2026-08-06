import { criarNoHorizonte } from '../comum/ladrilho.js';
import { desenharFantasma } from '../comum/ar.js';
import { entre } from '../comum/basicos.js';
// Cemitério: cinza caindo, névoa no chão, morcegos cruzando o céu e corujas nas árvores.
export const ar = {
    po: { cor: '178, 205, 186', quantas: 34, subida: [-9, -3], raio: [0.5, 2.0], opacidade: [.08, .3] },
    nevoa: { cor: '150, 190, 170', quantas: 7, deriva: [6, 20], raio: [140, 320], opacidade: [.03, .08] },
    // Aqui NÃO há `voadores`. Os fantasmas do cemitério passeiam pela tela como sempre, mas
    // agora eles NASCEM do caixão em vez de entrarem pela borda — ver `caixao` mais abaixo.
    // Entrar pela borda dizia que vinham de fora; saindo da cova, o cemitério passa a ter uma
    // origem, e a peça do meio deixa de ser um enfeite pra virar a causa do resto.
    // (O morcego segue guardado pros 🔱 Decaídos, que é onde ele pertence.)
    // As corujas nas árvores. `poleiros` são posições DENTRO do ladrilho da mata, em fração —
    // assim elas seguem o ladrilho quando ele muda de tamanho, e cada árvore desenhada no SVG
    // ganha a sua sem ninguém recontar pixel. `lado` encosta a coruja num flanco do tronco: uma
    // à esquerda, a outra à direita, pra as duas árvores não ficarem iguais.
    //
    // O olho fica APAGADO a maior parte do tempo, e o tempo ACESO é sempre o mesmo — é o que faz
    // parecer bicho olhando de vez em quando, e não lâmpada piscando. O que varia é QUANDO cada
    // uma abre: o tempo apagado é sorteado a cada ciclo, então elas nunca casam.
    corujas: {
        corpo: '#060f0b', olho: '236, 246, 205', tamanho: 11,
        ladrilho: ['--mata-passo', '--mata-altura'],
        // Tirados do próprio SVG da mata (ladrilho 320×190): o tronco grande fica em x≈47 e o
        // menor em x≈234, daí .148 e .733. Antes eram valores ajustados a olho, que estavam
        // compensando o ladrilho ancorado no centro — com a âncora consertada, a conta fecha.
        pontos: [{ x: .148, y: .24, lado: -1 }, { x: .733, y: .30, lado: 1 }],
        aceso: 1.7, apagado: [5, 17], acordar: [0, 13],
    },
    // Os espantalhos fincados no meio das lápides. Mesmo motor das corujas — posição vinda do
    // ladrilho — só que SEM relógio: eles não piscam, estão sempre lá. É a ausência de `aceso`
    // que diz isso; nenhuma configuração extra foi precisa.
    espantalhos: {
        // O pano é PANO, não sombra: um marrom-ferrugem gasto. Estava quase preto e sumia contra
        // o mato — e um espantalho que não se vê não assusta ninguém. Ele e a abóbora são as
        // únicas coisas quentes do cemitério, e conversam entre si sem competir: o pano é escuro
        // e terroso, a cabeça é clara e acesa.
        poste: '#050b08', pano: '#5c3623', tamanho: 40,
        // A abóbora é o único ponto de COR do cemitério inteiro. Baixa saturação de propósito:
        // laranja aceso aqui brigaria com a lua e roubaria a cena.
        abobora: '148, 84, 32', cara: '255, 172, 66',
        ladrilho: ['--mata-passo', '--mata-altura'],
        // UM por ladrilho, fincado no chão (y ≈ .95 é quase o rodapé), no vão entre a cruz e a
        // árvore menor (x≈196) — onde o SVG não desenhou nada. Dois por ladrilho enchiam demais:
        // espantalho é figura solitária, e repetido de perto vira plantação.
        pontos: [{ x: .613, y: .95 }],
    },
    // O 💀 saindo da cova, no meio do cemitério — a única peça do tema que ACONTECE em vez de só
    // estar lá. O que sai dele são fantasmas; ver o doc do `criarCaixao`.
    caixao: {
        madeira: '#2a1d16', ferro: '#12100e', dentro: '#040706', terra: '#0b120e',
        // O mesmo verde espectral do pó, da névoa e dos fantasmas que já cruzam o céu: a luz
        // nova tinha que pertencer ao cemitério que já existe, não trazer uma paleta própria.
        brilho: '178, 255, 214',
        // `cor` (e não um nome próprio) porque é o que o `desenharFantasma` já espera receber —
        // é o mesmo desenho dos voadores, reusado inteiro.
        cor: '208, 238, 222',
        // Fração da altura da arena. Encolheu (era .72) pra abrir espaço pros fantasmas: com o
        // caixão menor, o que ocupa a cena passa a ser o que SAI dele, que é o ponto.
        // Metade do que era (.30 × .54). Com os fantasmas passeando pela tela o tempo todo, o
        // caixão não precisa mais carregar a cena sozinho — ele virou a FONTE deles, e fonte
        // grande demais rouba a atenção do que sai dela.
        largura: .15, altura: .27, canto: 4,
        // Os caixõezinhos em volta: `x` em múltiplos da largura do grande, `y` em fração da
        // altura dele, `giro` em radianos e `atraso` no quanto cada um demora a romper o chão.
        // Tortos e em alturas diferentes de propósito — enfileirados e retos, virariam cerca.
        menorTamanho: .095,
        menores: [
            { x: -2.6, y: .06, giro: -.38, atraso: .5 },
            { x: -1.5, y: .10, giro: .26, atraso: 0 },
            { x: 1.6, y: .09, giro: -.3, atraso: .25 },
            { x: 2.7, y: .05, giro: .44, atraso: .7 },
        ],
        // O raio do clarão, em MÚLTIPLOS da largura do caixão. É a única alavanca do quanto a
        // luz vaza pelos lados do log (~300px de coluna): com o caixão menor, `2.4` deixava a
        // luz morrer justo na borda dele. Aumentar isto é o ajuste se ainda parecer escondido.
        clarao: 5.5,
        espera: [10, 20], revirar: .8, subir: 2.4, abrir: 1.1,
        fechar: .9, descer: 2.2, assentar: 1.3,
        // A leva. `subida` e `tamanho` são fração da altura da arena, como todo o resto.
        // A LEVA. Três por abertura, e cada fantasma dura DOIS ciclos do caixão: a leva 1 sai, a
        // leva 2 sai (seis na tela), e quando a 3 vem a 1 já está se apagando. É o rodízio que
        // segura o teto em 6 sem ninguém contar nada — o `maximo` é só rede de segurança.
        porLeva: 3, maximo: 6, intervalo: .5,
        acender: .6, apagar: 2.2,
        // `subida` e `deriva` são FAIXAS e não valores únicos: fantasmas todos na mesma
        // velocidade leem como um efeito só. `assumir` é em quanto tempo a deriva lateral toma
        // o lugar do impulso de saída — é o que liga "saiu do caixão" a "passeia pela tela".
        subida: [.05, .11], deriva: [.02, .05], assumir: 2.4, tamanho: .058,
        // A faixa de altitude que eles percorrem, em fração da altura da arena: de quase
        // encostado no topo (.06) até a metade de baixo (.66). `mudarAltura` é de quanto em
        // quanto tempo cada um escolhe outra, e `buscarAltura` o quanto ele demora a chegar lá
        // — lento de propósito, senão a troca vira teleporte vertical em vez de voo.
        //
        // `altitude`, e NÃO `altura`: esta config já tem uma `altura`, que é a do caixão. Chamar
        // as duas de `altura` no mesmo objeto não dá erro nenhum em JS — a segunda simplesmente
        // apaga a primeira, e o caixão inteiro vira NaN.
        altitude: [.06, .66], mudarAltura: [4, 10], buscarAltura: 5,
    },
};
/// O CAIXÃO que sobe no meio do cemitério — a referência do 💀 no cenário dele.
///
/// Fica no CENTRO, como a ruína dos Tecnológicos e o castelo do Reino: coisa única e nomeada mora no
/// meio, e o log (uma coluna de ~300px em z 4) passa na frente dela nos três casos sem que isso tenha
/// atrapalhado nenhum. O que resolve não é fugir do log — é o CLARÃO, que tem raio bem maior que a
/// peça e vaza pelos dois lados dele. A luz é o que anuncia o acontecimento; a silhueta só confirma.
///
/// EM PÉ, e não deitado: caixão deitado visto de frente é um retângulo, e o que sobe do chão precisa
/// ter uma direção.
///
/// O que sai de dentro são FANTASMAS, e não uma caveira. A caveira falhava por escala — a 70px o
/// crânio já é quase só silhueta, e detalhe nessa medida lê como borrão — a lição que todo cenário
/// aqui já cobrou. Os fantasmas resolvem os dois problemas de uma vez: não dependem de detalhe (são vulto
/// e halo), o desenho JÁ EXISTE (`desenharFantasma`, o mesmo dos que cruzam o céu), e eles dão à cena
/// um ANTES e um DEPOIS — o caixão passa a ser de onde a assombração vem, e não um objeto que abre.
///
/// A TAMPA abre em DUAS METADES, cada uma girando pra fora na dobradiça do próprio lado. Uma folha
/// só, girando pra um lado, jogava o peso da figura pro outro e brigava com uma peça que é simétrica
/// em tudo mais. Com duas, o vão nasce no meio — que é justamente de onde os fantasmas saem.
///
/// Diretor de uma fase por vez, como os exércitos e o ninja:
///   enterrado → terra → subindo → abrindo → soltando → fechando → descendo → assentando → enterrado
///
/// A TERRA é a primeira a aparecer e a última a sumir, das três opções possíveis (sempre visível,
/// nunca, ou acompanhando). Ela revirar ANTES de qualquer coisa aparecer é o que transforma a subida
/// em consequência: o chão racha, e só então sai o que estava embaixo. Deixá-la fixa faria dela mais
/// uma lápide do cenário — perderia a antecipação, que é a parte barata e mais eficaz do susto.
///
/// Só a ESPERA enterrado é sorteada. Os gestos têm duração fixa: sortear quanto tempo uma tampa leva
/// pra abrir faria a mesma tampa parecer pesada numa vez e leve na outra.
export function criarCaixao(cfg, canvas) {
    let fase = 'enterrado';
    let relogio = entre(cfg.espera) * .35;   // a primeira espera é curta: a cena não pode abrir vazia
    let terra = 0;       // 0 = chão intacto, 1 = monte revirado
    let emerso = 0;      // 0 = enterrado, 1 = fora
    let abertura = 0;    // 0 = fechado, 1 = tampa escancarada
    let aSoltar = 0;     // quantos fantasmas ainda faltam sair nesta leva
    let proximo = 0;     // relógio até o próximo sair
    let leva = 0;        // quantas aberturas já houve — é o relógio de vida dos fantasmas
    let fantasmas = [];
    let t = 0;

    // Os caixões PEQUENOS em volta, sorteados uma vez: eles não têm ciclo nenhum, só acompanham o
    // grande subindo e descendo. Sorteados por quadro, tremeriam.
    const menores = cfg.menores.map(m => ({ ...m, ondula: Math.random() * Math.PI * 2 }));

    return (ctx, dt) => {
        t += dt;
        relogio -= dt;

        const l = canvas.height * cfg.largura, h = canvas.height * cfg.altura;
        const cx = canvas.width / 2;
        const base = canvas.height;
        // O pé do caixão fica sempre ABAIXO da borda: ele sai do chão, não desliza de uma fresta.
        const topo = base - h * emerso;
        // A BOCA: de onde os fantasmas saem. É o vão que se abre no meio, no alto do caixão.
        const bocaY = topo + h * .22;

        switch (fase) {
            case 'enterrado':
                if (relogio <= 0) fase = 'terra';
                break;
            case 'terra':
                terra = Math.min(1, terra + dt / cfg.revirar);
                if (terra === 1) {
                    fase = 'subindo';
                    // A LEVA vira aqui, no instante em que o chão termina de revirar — e é aqui que
                    // os fantasmas velhos recebem a ordem de apagar. Eles duram DOIS ciclos: a leva
                    // 1 sai, a leva 2 sai (seis na tela), e quando a 3 vem a 1 já está se apagando.
                    //
                    // Marcar AQUI, e não quando a tampa abre, é o que faz a conta fechar: daqui até
                    // o primeiro fantasma novo há a subida inteira mais a abertura (~3,5s), e a
                    // fade leva 2,2s. Marcando mais tarde, os velhos ainda estariam se apagando na
                    // hora de os novos saírem, e o teto de 6 barraria a leva nova pela metade —
                    // o caixão abriria pra soltar dois. Também é melhor de ver: eles se dissolvem
                    // ENQUANTO o caixão sobe, então a troca de guarda tem o gesto certo.
                    leva++;
                    for (const f of fantasmas) if (f.leva <= leva - 2) f.apagando = true;
                }
                break;
            case 'subindo':
                emerso = Math.min(1, emerso + dt / cfg.subir);
                if (emerso === 1) fase = 'abrindo';
                break;
            case 'abrindo':
                abertura = Math.min(1, abertura + dt / cfg.abrir);
                if (abertura === 1) { fase = 'soltando'; aSoltar = cfg.porLeva; proximo = 0; }
                break;
            case 'soltando':
                proximo -= dt;
                if (aSoltar > 0 && proximo <= 0) {
                    // O teto é rede de segurança, não a regra: com o rodízio das levas a conta já
                    // fecha em 6. Ele existe pra o caso de um ciclo atropelar o outro.
                    if (fantasmas.length < cfg.maximo) {
                        fantasmas.push({
                            leva,
                            x: cx + (Math.random() - .5) * l * .5,
                            y: bocaY,
                            // Nasce SUBINDO e sem deriva; a deriva entra depois (ver o passeio). Sair
                            // já andando de lado leria como "passava por ali", não como "saiu daí".
                            vy: -entre(cfg.subida) * canvas.height,
                            vx: 0,
                            deriva: (Math.random() < .5 ? -1 : 1) * entre(cfg.deriva) * canvas.width,
                            // A ALTITUDE que ele persegue, e quando vai trocar por outra. É isto que
                            // espalha o bando pela tela inteira: sem alvo, todos ficariam na faixa
                            // em que o impulso de saída os largou.
                            alvoY: canvas.height * entre(cfg.altitude),
                            trocar: entre(cfg.mudarAltura),
                            s: entre([.72, 1.25]),
                            bobo: entre([14, 34]),
                            fase: Math.random() * Math.PI * 2,
                            asa: Math.random() * Math.PI * 2,
                            paraDireita: Math.random() < .5,
                            t: 0,
                            alfa: 0,
                        });
                    }
                    aSoltar--;
                    proximo = cfg.intervalo;
                }
                // Fecha logo depois do ÚLTIMO SAIR — e não depois de ele sumir. Os fantasmas agora
                // vivem dois ciclos inteiros passeando pela tela; esperar o fim deles deixaria o
                // caixão escancarado o tempo todo, e ele voltou a ser um acontecimento breve.
                if (aSoltar <= 0) fase = 'fechando';
                break;
            case 'fechando':
                abertura = Math.max(0, abertura - dt / cfg.fechar);
                if (abertura === 0) fase = 'descendo';
                break;
            case 'descendo':
                emerso = Math.max(0, emerso - dt / cfg.descer);
                if (emerso === 0) fase = 'assentando';
                break;
            case 'assentando':
                terra = Math.max(0, terra - dt / cfg.assentar);
                if (terra === 0) { fase = 'enterrado'; relogio = entre(cfg.espera); }
                break;
        }

        // --- o PASSEIO dos fantasmas, fora da máquina de fases: eles vivem dois ciclos do caixão, e
        //     têm que seguir andando enquanto a tampa fecha, o caixão desce e o próximo sobe.
        for (let k = fantasmas.length - 1; k >= 0; k--) {
            const f = fantasmas[k];
            f.t += dt;

            // Sai de dentro subindo e vai TROCANDO o impulso vertical pela deriva lateral: é essa
            // troca que transforma "saiu do caixão" em "está passeando pela tela", sem os dois
            // momentos precisarem de dois sistemas.
            f.vy *= .988;
            f.vx += (f.deriva - f.vx) * Math.min(1, dt / cfg.assumir);
            f.x += f.vx * dt;
            f.y += f.vy * dt;

            // Troca de altitude de vez em quando, cada um no seu tempo. A perseguição do alvo está
            // SEMPRE ligada, mas é lenta: enquanto o impulso de saída é forte ela mal se nota, e
            // quando ele se esgota é ela que passa a mandar. Um só mecanismo cobre os dois momentos.
            f.trocar -= dt;
            if (f.trocar <= 0) {
                f.alvoY = canvas.height * entre(cfg.altitude);
                f.trocar = entre(cfg.mudarAltura);
            }
            f.y += (f.alvoY - f.y) * Math.min(1, dt / cfg.buscarAltura);

            f.fase += dt * 1.6;
            f.asa += dt * 2.2;
            f.paraDireita = f.vx >= 0;

            // Dá a volta pelas bordas em vez de morrer nelas: são eles que povoam a tela entre uma
            // abertura e outra, e um fantasma que some ao encostar na borda deixaria o cemitério
            // vazio na metade do ciclo.
            const folga = canvas.height * cfg.tamanho * 2;
            if (f.x < -folga) f.x = canvas.width + folga;
            else if (f.x > canvas.width + folga) f.x = -folga;
            if (f.y < -folga) f.y = -folga;

            // Acende ao sair e apaga só quando MANDAM apagar (a leva dele venceu). Nada de morrer
            // por cronômetro próprio: quem manda no fim deles é o caixão.
            const alvo = f.apagando ? 0 : 1;
            const passo = dt / (f.apagando ? cfg.apagar : cfg.acender);
            f.alfa += Math.sign(alvo - f.alfa) * Math.min(passo, Math.abs(alvo - f.alfa));
            if (f.apagando && f.alfa <= 0) fantasmas.splice(k, 1);
        }

        if (terra <= 0 && emerso <= 0 && fantasmas.length === 0) return;

        ctx.save();

        // --- o clarão, ATADO À ABERTURA: com a tampa fechada não há luz nenhuma, e é isso que faz a
        //     abertura ser um acontecimento em vez de um objeto sempre aceso.
        if (abertura > 0) {
            const pulso = .8 + Math.sin(t * 2.6) * .12 + Math.sin(t * 6.3) * .06;
            const raio = l * cfg.clarao * abertura * pulso;
            const clarao = ctx.createRadialGradient(cx, bocaY, 0, cx, bocaY, raio);
            clarao.addColorStop(0, `rgba(${cfg.brilho}, ${.3 * abertura})`);
            clarao.addColorStop(.45, `rgba(${cfg.brilho}, ${.12 * abertura})`);
            clarao.addColorStop(1, `rgba(${cfg.brilho}, 0)`);
            ctx.fillStyle = clarao;
            ctx.beginPath();
            ctx.arc(cx, bocaY, raio, 0, Math.PI * 2);
            ctx.fill();
        }

        // --- os caixões PEQUENOS, em diagonal, brotando na terra em volta. São enfeite: não abrem,
        //     não soltam nada, só acompanham o grande. Ficam ANTES dele no desenho porque são a
        //     companhia, não o assunto — o grande passa na frente quando se cruzam.
        for (const m of menores) {
            const mh = canvas.height * cfg.menorTamanho;
            const ml = mh * .42;
            // Brotam um pouco atrasados em relação ao grande (`emerso` elevado), como se a terra os
            // empurrasse junto. O expoente é o atraso: sem ele, todos rompem o chão no mesmo quadro.
            const brota = Math.pow(emerso, 1.5 + m.atraso);
            if (brota <= .01) continue;

            ctx.save();
            ctx.translate(cx + l * m.x, base - h * m.y * brota);
            ctx.rotate(m.giro);
            ctx.globalAlpha = Math.min(1, brota * 1.4);
            ctx.fillStyle = cfg.madeira;
            contornoDoCaixao(ctx, 0, -mh * brota, 0, ml, mh, cfg.canto * .5);
            ctx.fill();
            // uma cruz só, bem simples: nessa escala é o que separa "caixão" de "tábua espetada"
            ctx.fillStyle = cfg.ferro;
            ctx.fillRect(-ml * .06, -mh * brota * .78, ml * .12, mh * brota * .4);
            ctx.fillRect(-ml * .22, -mh * brota * .68, ml * .44, mh * brota * .1);
            ctx.restore();
        }
        ctx.globalAlpha = 1;

        if (emerso > 0) {
            // --- a MADEIRA inteira, fechada. A tampa não é desenhada em peças: é este corpo, e o
            //     que abre é um VÃO cavado nele (logo abaixo).
            ctx.fillStyle = cfg.madeira;
            contornoDoCaixao(ctx, cx, topo, base + 8, l, h, cfg.canto);
            ctx.fill();

            // as tábuas e a cruz de ferro, na madeira fechada
            ctx.strokeStyle = 'rgba(0, 0, 0, .32)';
            ctx.lineWidth = 1.5;
            for (const lado of [-1, 1]) {
                ctx.beginPath();
                ctx.moveTo(cx + lado * l * .22, topo + h * .06);
                ctx.lineTo(cx + lado * l * .12, base);
                ctx.stroke();
            }
            ctx.fillStyle = cfg.ferro;
            ctx.fillRect(cx - l * .05, topo + h * .1, l * .1, h * .38);
            ctx.fillRect(cx - l * .2, topo + h * .19, l * .4, h * .09);

            // --- a ABERTURA: um vão escuro que cresce do MEIO pra fora, recortado no contorno do
            //     caixão. É a tampa partindo ao meio e as duas metades sumindo pros lados.
            //
            //     Era duas folhas transformadas por `scale` em direção às dobradiças. Aquilo não
            //     funcionava: cada folha carregava o contorno INTEIRO do caixão espremido, então o
            //     que se via eram dois caixõezinhos deformados nas laterais em vez de duas metades
            //     de tampa — e nas aberturas parciais elas se sobrepunham no meio. Cavar o vão é o
            //     inverso e não tem como errar: o que não é vão É tampa, e o meio abre primeiro
            //     porque o vão nasce no eixo.
            if (abertura > 0) {
                ctx.save();
                contornoDoCaixao(ctx, cx, topo, base + 8, l, h, cfg.canto);
                ctx.clip();
                ctx.fillStyle = cfg.dentro;
                const meio = l * .5 * abertura;
                ctx.fillRect(cx - meio, topo - 2, meio * 2, h + 12);
                ctx.restore();
            }
        }

        // --- o MONTE DE TERRA revirada. Vem DEPOIS do caixão pra cobrir a junta com o chão, e tem
        //     vida própria: sobe antes de tudo e assenta depois de tudo.
        if (terra > 0) {
            ctx.fillStyle = cfg.terra;
            ctx.beginPath();
            ctx.ellipse(cx, base - 2, l * (1.9 + emerso * .8) * terra, h * .14 * terra, 0, Math.PI, 0);
            ctx.fill();
        }

        // --- os fantasmas por ÚLTIMO: eles saem de dentro e passam na frente da madeira.
        for (const f of fantasmas) {
            ctx.globalAlpha = f.alfa;
            desenharFantasma(ctx, f.x, f.y + Math.sin(f.fase) * f.bobo,
                canvas.height * cfg.tamanho * f.s, f.asa, f.paraDireita, cfg);
        }
        ctx.globalAlpha = 1;

        ctx.restore();
    };
}

/// A silhueta hexagonal do caixão, com os cantos LEVEMENTE arredondados — madeira velha não tem
/// quina viva, e o arredondado é o que tira a leitura de "polígono desenhado".
///
/// São os OMBROS (o ponto mais largo, a 26% do topo) que fazem a forma ler como caixão; sem eles é
/// uma caixa comprida. O `pe` é a altura do PÉ e vem de fora: o caixão grande passa da borda de
/// baixo (caixão saindo do chão não tem fundo à vista), e os pequenos, que são desenhados girados
/// em volta do próprio centro, terminam no zero.
export function contornoDoCaixao(ctx, cx, topo, pe, l, h, canto) {
    const pontos = [
        [cx - l * .30, topo],
        [cx + l * .30, topo],
        [cx + l * .50, topo + h * .26],
        [cx + l * .22, pe],
        [cx - l * .22, pe],
        [cx - l * .50, topo + h * .26],
    ];
    const meio = (a, b) => [(a[0] + b[0]) / 2, (a[1] + b[1]) / 2];

    // O idioma do polígono arredondado: começa no MEIO de um lado (pra o primeiro arcTo ter de onde
    // partir) e vai ligando meio-de-lado a meio-de-lado, curvando em cada vértice.
    ctx.beginPath();
    const inicio = meio(pontos[0], pontos[1]);
    ctx.moveTo(inicio[0], inicio[1]);
    for (let k = 1; k <= pontos.length; k++) {
        const v = pontos[k % pontos.length];
        const seguinte = meio(v, pontos[(k + 1) % pontos.length]);
        ctx.arcTo(v[0], v[1], seguinte[0], seguinte[1], canto);
    }
    ctx.closePath();
}

export const criarCorujas = (cfg, canvas) => criarNoHorizonte(cfg, canvas,
    (ctx, c, k) => desenharCoruja(ctx, c.x, c.y, k.tamanho, c.lado, c.aceso, k));


export const criarEspantalhos = (cfg, canvas) => criarNoHorizonte(cfg, canvas,
    (ctx, c, k) => desenharEspantalho(ctx, c.x, c.y, k.tamanho, k));







/// Um espantalho: cruz de madeira, pano esfarrapado pendurado e cabeça de abóbora.
///
/// A abóbora é o ÚNICO ponto de cor do cemitério inteiro, e por isso é de baixa saturação — laranja
/// aceso aqui brigaria com a lua e roubaria a cena. A cara fica um tom acima, como se houvesse uma
/// vela dentro: é a diferença entre os dois laranjas que faz ler como abóbora entalhada, e não como
/// uma bola laranja espetada num poste.

/// Um espantalho: cruz de madeira, pano esfarrapado pendurado e cabeça de abóbora.
///
/// A abóbora é o ÚNICO ponto de cor do cemitério inteiro, e por isso é de baixa saturação — laranja
/// aceso aqui brigaria com a lua e roubaria a cena. A cara fica um tom acima, como se houvesse uma
/// vela dentro: é a diferença entre os dois laranjas que faz ler como abóbora entalhada, e não como
/// uma bola laranja espetada num poste.
export function desenharEspantalho(ctx, x, base, s, cfg) {
    ctx.save();
    ctx.translate(x, base);
    ctx.scale(s, s);

    // a cruz
    ctx.fillStyle = cfg.poste;
    ctx.fillRect(-.05, -1.5, .1, 1.5);
    ctx.fillRect(-.62, -1.12, 1.24, .085);

    // o pano: ombros caídos e barra rasgada em dentes
    ctx.fillStyle = cfg.pano;
    ctx.beginPath();
    ctx.moveTo(-.58, -1.1);
    ctx.quadraticCurveTo(0, -1.22, .58, -1.1);
    ctx.lineTo(.44, -.36);
    ctx.lineTo(.3, -.52); ctx.lineTo(.16, -.3);
    ctx.lineTo(.02, -.5); ctx.lineTo(-.14, -.28);
    ctx.lineTo(-.3, -.48); ctx.lineTo(-.44, -.34);
    ctx.closePath();
    ctx.fill();

    // trapos esvoaçando nas pontas dos braços
    ctx.beginPath();
    ctx.moveTo(-.62, -1.14); ctx.lineTo(-.78, -.98); ctx.lineTo(-.58, -1.0);
    ctx.moveTo(.62, -1.14); ctx.lineTo(.78, -.98); ctx.lineTo(.58, -1.0);
    ctx.fill();

    // a abóbora
    const cy = -1.42, r = .3;
    ctx.fillStyle = `rgb(${cfg.abobora})`;
    ctx.beginPath();
    ctx.ellipse(0, cy, r, r * .92, 0, 0, Math.PI * 2);
    ctx.fill();

    // os gomos, escurecidos por cima da própria cor
    ctx.strokeStyle = 'rgba(0, 0, 0, .28)';
    ctx.lineWidth = .028;
    for (const g of [-.16, 0, .16]) {
        ctx.beginPath();
        ctx.ellipse(g, cy, Math.abs(g) === 0 ? r * .34 : r * .2, r * .9, 0, 0, Math.PI * 2);
        ctx.stroke();
    }

    // o cabinho
    ctx.fillStyle = cfg.poste;
    ctx.fillRect(-.035, cy - r * .96 - .1, .07, .12);

    // a cara acesa: dois olhos e a boca de dentes
    ctx.fillStyle = `rgb(${cfg.cara})`;
    ctx.beginPath();
    ctx.moveTo(-.16, cy - .04); ctx.lineTo(-.05, cy - .04); ctx.lineTo(-.105, cy - .14);
    ctx.moveTo(.16, cy - .04); ctx.lineTo(.05, cy - .04); ctx.lineTo(.105, cy - .14);
    ctx.fill();
    ctx.beginPath();
    ctx.moveTo(-.17, cy + .08);
    ctx.lineTo(-.09, cy + .16); ctx.lineTo(-.03, cy + .08);
    ctx.lineTo(.03, cy + .16); ctx.lineTo(.09, cy + .08);
    ctx.lineTo(.17, cy + .16);
    ctx.lineTo(.12, cy + .2); ctx.lineTo(-.12, cy + .2);
    ctx.closePath();
    ctx.fill();

    ctx.restore();
}

/// Uma coruja empoleirada: silhueta parada, olhos que acendem. O corpo fica SEMPRE visível — ela
/// está ali o tempo todo, e é isso que faz o olho acender ser um bicho olhando, em vez de duas luzes
/// surgindo do nada no meio do mato.
///
/// `lado` vira a cabeça pro lado de fora da árvore (ela está na beirada do tronco, olhando pro
/// campo), o que de quebra faz as duas corujas do ladrilho não serem a mesma figura repetida.
export function desenharCoruja(ctx, x, y, s, lado, aceso, cfg) {
    ctx.save();
    ctx.translate(x, y);
    ctx.scale(s, s);
    ctx.fillStyle = cfg.corpo;

    // Corpo em gota: cabeça larga sem pescoço, que é o que faz uma silhueta ler como coruja.
    ctx.beginPath();
    ctx.moveTo(0, -1);
    ctx.bezierCurveTo(.92, -.98, 1.06, .2, .68, .98);
    ctx.quadraticCurveTo(0, 1.18, -.68, .98);
    ctx.bezierCurveTo(-1.06, .2, -.92, -.98, 0, -1);
    ctx.closePath();
    ctx.fill();

    // Tufos de orelha — dois espetinhos, e é o detalhe que descarta "passarinho qualquer".
    ctx.beginPath();
    ctx.moveTo(-.62, -.72); ctx.lineTo(-.78, -1.32); ctx.lineTo(-.24, -.94);
    ctx.moveTo(.62, -.72); ctx.lineTo(.78, -1.32); ctx.lineTo(.24, -.94);
    ctx.fill();

    // Um pé de galho sob ela, pra não parecer flutuando colada no tronco.
    ctx.fillRect(-.5, .96, 1, .16);

    if (!aceso) { ctx.restore(); return; }

    // Os olhos, com halo: o halo é o que faz ler como brilho no escuro em vez de dois pixels acesos.
    // Levemente deslocados pro lado de fora — a cabeça está virada pro campo.
    for (const olho of [-1, 1]) {
        const ox = olho * .32 + lado * .1;
        const oy = -.38;
        const g = ctx.createRadialGradient(ox, oy, 0, ox, oy, .62);
        g.addColorStop(0, `rgba(${cfg.olho}, .98)`);
        g.addColorStop(.3, `rgba(${cfg.olho}, .5)`);
        g.addColorStop(1, `rgba(${cfg.olho}, 0)`);
        ctx.fillStyle = g;
        ctx.beginPath();
        ctx.arc(ox, oy, .62, 0, Math.PI * 2);
        ctx.fill();
    }

    ctx.restore();
}
