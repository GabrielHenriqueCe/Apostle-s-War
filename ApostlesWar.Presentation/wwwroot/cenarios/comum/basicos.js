// Peças de traço e de sorteio que mais de um cenário usa. Nada aqui sabe de tema nenhum.

export const entre = ([min, max]) => min + Math.random() * (max - min);

/// Um retângulo de cantos redondos, com `arcTo` — ver o comentário do `desenharCarta` pra saber por
/// que NÃO é `ctx.roundRect`. Só monta o caminho; quem chama decide preencher, contornar ou os dois.
export function caixaRedonda(ctx, x, y, l, a, r) {
    const raio = Math.max(0, Math.min(r, Math.abs(l) / 2, Math.abs(a) / 2));
    ctx.beginPath();
    ctx.moveTo(x + raio, y);
    ctx.arcTo(x + l, y, x + l, y + a, raio);
    ctx.arcTo(x + l, y + a, x, y + a, raio);
    ctx.arcTo(x, y + a, x, y, raio);
    ctx.arcTo(x, y, x + l, y, raio);
    ctx.closePath();
}

/// Pinta uma peça e risca as listras DENTRO dela. O caminho é montado UMA vez e serve pras duas
/// coisas — preencher e RECORTAR —, e é o recorte que impede a listra de vazar pela borda.
///
/// Nasceu porque a primeira leva de listras tentava acertar a silhueta à mão, do lado de fora, e
/// sobrava listra pendurada pra fora do bicho em todo lugar em que a conta não batia com o bezier.
/// Com o recorte, a listra pode passar folgadamente da borda: o que sai é aparado pela forma que já
/// existe, e as duas nunca podem discordar porque são o MESMO caminho.
export function comListras(ctx, caminho, tinta, listras) {
    ctx.save();
    caminho();
    ctx.fillStyle = tinta;
    ctx.fill();
    ctx.clip();
    listras();
    ctx.restore();
}

/// Uma labareda. `t` já vem embaralhado por posição, pra duas chamas vizinhas nunca subirem juntas.
export function desenharChama(ctx, x, base, alt, t, cfg) {
    ctx.save();

    // o clarão da fogueira no que está em volta — é ele que faz a casa queimando iluminar a vizinha
    const brilho = .8 + Math.sin(t * 2.9) * .12 + Math.sin(t * 6.7) * .08;
    const g = ctx.createRadialGradient(x, base - alt * .3, 0, x, base - alt * .3, alt * 1.9 * brilho);
    g.addColorStop(0, `rgba(${cfg.fogo}, .26)`);
    g.addColorStop(.45, `rgba(${cfg.fogo}, .08)`);
    g.addColorStop(1, `rgba(${cfg.fogo}, 0)`);
    ctx.fillStyle = g;
    ctx.beginPath();
    ctx.arc(x, base - alt * .3, alt * 1.9 * brilho, 0, Math.PI * 2);
    ctx.fill();

    for (let i = 0; i < cfg.labaredas; i++) {
        const ritmo = 2.6 + i * 1.1;
        const viva = .62 + Math.sin(t * ritmo + i) * .24 + Math.sin(t * ritmo * 2.3) * .14;
        const h = alt * viva * (1 - i * .18);
        const larg = alt * .2 * (.8 + viva * .4);
        const px = x + (i - (cfg.labaredas - 1) / 2) * alt * .26;

        const chama = ctx.createLinearGradient(px, base, px, base - h);
        chama.addColorStop(0, `rgba(${cfg.brasa}, .9)`);
        chama.addColorStop(.4, `rgba(${cfg.fogo}, .6)`);
        chama.addColorStop(1, `rgba(${cfg.fogo}, 0)`);
        ctx.fillStyle = chama;

        ctx.beginPath();
        ctx.moveTo(px - larg, base);
        ctx.quadraticCurveTo(px - larg * .5, base - h * .6, px + Math.sin(t * ritmo * .8 + i) * larg * 1.2, base - h);
        ctx.quadraticCurveTo(px + larg * .5, base - h * .6, px + larg, base);
        ctx.closePath();
        ctx.fill();
    }

    ctx.restore();
}
