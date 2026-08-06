// O HORIZONTE: medir o ladrilho que o CSS do tema declarou, e fazer coisas passarem na linha dele.
//
// Os valores do CSS têm de ser px CRUS — `getComputedStyle` devolve propriedade customizada como
// TEXTO, então `min()`/`clamp()`/`vh` viram NaN e caem no padrão EM SILÊNCIO, ancorando tudo no
// lugar errado. Quem encolhe os ladrilhos em janela baixa é o @media do estilo.css.

import { entre } from './basicos.js';

/// Lê o tamanho do ladrilho de horizonte que o CSS do tema declarou — hoje as corujas e os espantalhos
/// do cemitério e as bobinas do laboratório, todos pelo `criarNoHorizonte`.
///
/// (Ela saiu de dentro do `criarNoHorizonte` quando os chifres do Oni e a clava do Troll pareciam ir
/// precisar dela também. Não foi o que aconteceu: as duas aparições passaram a subir de trás de uma
/// MOITA, que é canvas, e não têm mais nada a ver com o ladrilho. A função fica extraída de qualquer
/// forma — o `parseFloat` com fallback é exatamente o tipo de detalhe que não se quer ver inline no meio
/// de uma máquina de fases, e ela documenta a armadilha abaixo num lugar só.)
///
/// É este `parseFloat` que obriga os valores do CSS a serem PX CRUS: propriedade customizada NÃO é
/// resolvida pelo getComputedStyle, então um `min()`/`clamp()`/`vh` voltaria como o texto literal, a
/// conta daria NaN e o padrão abaixo assumiria EM SILÊNCIO — ancorando tudo no lugar errado sem
/// quebrar nada. Quem encolhe os ladrilhos em janela baixa é o @media do estilo.css, em px.
export function medirLadrilho(nomes, passoPadrao = 320, alturaPadrao = 190) {
    return {
        passo: medirDoTema(nomes[0], passoPadrao),
        altura: medirDoTema(nomes[1], alturaPadrao),
    };
}

/// Um número que o CSS do tema declarou. É AQUI que mora a armadilha do parágrafo acima, num lugar só:
/// `getComputedStyle` devolve propriedade customizada como TEXTO, então o valor tem que ser algo que o
/// `parseFloat` leia — px crus (os ladrilhos) ou número puro (as linhas do mar e da areia dos
/// Místicos, que o CSS usa via `calc(var(--mar-linha) * 1%)`). Qualquer `min()`/`clamp()` vira NaN e
/// cai no padrão EM SILÊNCIO.
export function medirDoTema(nome, padrao) {
    const valor = parseFloat(getComputedStyle(document.getElementById('arena')).getPropertyValue(nome));
    return Number.isFinite(valor) ? valor : padrao;
}

/// O MOTOR de tudo que fica PRESO NO HORIZONTE: as corujas da mata, as bobinas do laboratório, os
/// espantalhos entre as lápides. Duplicar estas 40 linhas por cliente seria abrir a porta pra eles
/// divergirem em silêncio.
///
/// O que cada tema traz é só: de que ladrilho ler a posição, onde estão os pontos, e o que desenhar.
///
/// O RELÓGIO é opcional: quem declara `aceso` pisca (coruja, bobina), quem não declara está sempre
/// aceso (espantalho). É a ausência do campo que decide — assim "não pisca" não precisou de
/// configuração nenhuma, e não há um `piscando: false` pra alguém esquecer de casar com o resto.
export function criarNoHorizonte(cfg, canvas, desenhar) {
    const pisca = cfg.aceso !== undefined;
    let pontos = [];
    let assinatura = '';

    const remontar = () => {
        const { passo, altura } = medirLadrilho(cfg.ladrilho);

        // Só refaz quando a GEOMETRIA muda. Refazer sempre destruiria o relógio de cada um a cada
        // conferida, e aí nenhum chegaria a acender — piscariam do zero pra sempre.
        const agora = `${canvas.width}|${canvas.height}|${passo}|${altura}`;
        if (agora === assinatura) return;
        assinatura = agora;

        const baseY = canvas.height - altura;   // o ladrilho do horizonte é ancorado no rodapé

        pontos = [];
        for (let tile = 0; tile * passo < canvas.width; tile++) {
            for (const p of cfg.pontos) {
                pontos.push({
                    // `lado` (quando existe) encosta a figura no flanco em vez de a espetar no meio.
                    x: tile * passo + p.x * passo + (p.lado ?? 0) * cfg.tamanho * .78,
                    y: baseY + p.y * altura,
                    lado: p.lado ?? 1,
                    // Quem pisca chega dormindo (a primeira vez demora, e cada um demora o seu);
                    // quem não pisca já nasce à vista.
                    aceso: !pisca,
                    resta: pisca ? entre(cfg.acordar) : Infinity,
                });
            }
        }
    };

    remontar();
    let conferir = 0;

    return (ctx, dt) => {
        // A arena muda de tamanho com a janela. Confere de vez em quando, e não a cada quadro,
        // porque getComputedStyle força layout — e o remontar só refaz de fato se algo mudou.
        conferir -= dt;
        if (conferir <= 0) { conferir = 1; remontar(); }

        for (const c of pontos) {
            if (pisca) {
                c.resta -= dt;
                if (c.resta <= 0) {
                    c.aceso = !c.aceso;
                    // Aceso: sempre o mesmo tempo. Apagado: sorteado, e é daqui que vem o desencontro.
                    c.resta = c.aceso ? cfg.aceso : entre(cfg.apagado);
                }
            }
            desenhar(ctx, c, cfg);
        }
    };
}
