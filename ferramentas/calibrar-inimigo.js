// De onde saem as OITO ÂNCORAS do nível do inimigo (GDD-progressao §Os inimigos não têm itens).
//
// A pergunta: o jogador tem item, o inimigo não. Em que NÍVEL um time sem equipamento nenhum tem o
// mesmo poder que o time do jogador equipado? A resposta não é um chute nem uma razão fixa ("3× o
// nível"), porque a curva do nível é AFIM (`1 + 29(n−1)/59`) e não proporcional: a mesma razão de
// níveis vale 7,4× de status lá embaixo e 2,9× lá em cima.
//
// COMO: monta os dois times (um de cada tipo), calcula o poder de cada lado e acha por bisseção o
// nível em que eles empatam. Poder = dano por tempo × HP efetivo:
//
//     poder = (Σ ATK × (1 + taxa × dcrit) × Velocidade) × (Σ HP × (DEF + 5000) / 5000)
//
// que é o mesmo que dizer "os dois times se matam no mesmo número de turnos".
//
// O QUE ELE NÃO TEM: habilidade, cura e posição. Os dois lados perdem o mesmo, então a FORMA da
// curva sobrevive — a escala não. É instrumento de calibragem, e a verificação final é em jogo.
//
// ATENÇÃO — ESTE ARQUIVO DUPLICA CONSTANTES DO C#. As fichas do `Arquetipos`, o
// `Combate.DefesaDeMeiaReducao` e a grade do `GDD-itens.md` estão copiados aqui. Ele NÃO é fonte da
// verdade: quando alguém mexer lá e não aqui, o script mente calado. Ao mexer nos números do jogo,
// rodar isto de novo é parte do trabalho, não um extra.
//
// Uso:  node ferramentas/calibrar-inimigo.js
'use strict';

// ---------- constantes copiadas do C# ----------

// ApostlesWar.Domain/Models/Arquetipos.cs — a ficha do NÍVEL 1 de cada tipo
const FICHAS = {
  'Guardião':   { hp: 1000, atk: 17, def: 50, vel:  85, taxa: 0.05, dcrit: 0.60 },
  'Combatente': { hp:  840, atk: 45, def: 40, vel:  95, taxa: 0.25, dcrit: 0.90 },
  'Suporte':    { hp:  670, atk: 27, def: 32, vel: 105, taxa: 0.10, dcrit: 0.70 },
  'Atirador':   { hp:  500, atk: 50, def: 17, vel: 110, taxa: 0.15, dcrit: 0.80 },
};
const TIPOS = Object.keys(FICHAS);

const NIVEL_MAXIMO = 60;                    // Arquetipos.NivelMaximo
const GANHO_POR_ESTRELA = 2;                // Arquetipos.GanhoPorEstrela
const DEFESA_MEIA_REDUCAO = 5000;           // Combate.DefesaDeMeiaReducao

// Arquetipos.FatorDoNivel — SEM o clamp de 60: é justamente o inimigo acima dele que se quer medir
const fatorDoNivel = n => 1 + 29 * (n - 1) / (NIVEL_MAXIMO - 1);

// Arquetipos.Velocidade — o clamp de 60 FICA, e vale pros dois lados. É ele que impede o inimigo
// nv 428 de agir quatro vezes por turno do jogador.
const velocidade = (t, n) =>
  FICHAS[t].vel + Math.trunc(Math.min(n, NIVEL_MAXIMO) / 10) * GANHO_POR_ESTRELA;

// ---------- a grade de itens do GDD-itens.md (a projetada, não a implementada) ----------

// Máximo de cada principal no MÍTICO NÍVEL 60 (§OS 9 SLOTS). O que cai abaixo disso é o fatorItem.
const MAX = { atkFlat: 500, hpFlat: 11000, defFlat: 500, pct: 0.50, dcrit: 1.00, vel: 50 };

const fatorItem = nivelDoItem => (10 + 1.5 * nivelDoItem) / 100;   // §A ESCALA: 10 + 1,5 × nível (%)
const SUBS = { raro: 2, epico: 3, lendario: 4, mitico: 5 };        // §Raridade → quantas subs
const SUB = principal => principal / 6;                            // §a rolagem de sub

// ---------- os dois lados ----------

function inimigo(tipo, nivel) {
  const f = FICHAS[tipo], k = fatorDoNivel(nivel);
  const hp = Math.round(f.hp * k), atk = Math.round(f.atk * k), def = Math.round(f.def * k);
  return {
    dps: atk * (1 + f.taxa * f.dcrit) * velocidade(tipo, nivel),
    ehp: hp * (def + DEFESA_MEIA_REDUCAO) / DEFESA_MEIA_REDUCAO,
  };
}

// O jogador com o arsenal daquele ponto da campanha. Os slots de acessório entram pela ESTRELA do
// apóstolo (Pulseira em 4★ = nv 40, Colar em 6★ = nv 60), e o nível do item acompanha o do apóstolo
// — os dois sobem jogando e são travados pelo mesmo pedágio.
function jogador(tipo, nivelApostolo, nivelItem, raridade) {
  const f = FICHAS[tipo], k = fatorDoNivel(nivelApostolo), fi = fatorItem(nivelItem);
  const slots = 7 + (nivelApostolo >= 40 ? 1 : 0) + (nivelApostolo >= 60 ? 1 : 0);

  // principais: Arma(ATK) Elmo(HP) Escudo(DEF) Manopla(DanoCrit) Peitoral(ATK%) Calça(HP%)
  //             Bota(Velocidade) Pulseira(DanoCrit) Colar(ATK cheio)
  let atk = Math.round(f.atk * k) + MAX.atkFlat * fi + (slots >= 9 ? MAX.atkFlat * fi : 0);
  let hp  = Math.round(f.hp  * k) + MAX.hpFlat  * fi;
  let def = Math.round(f.def * k) + MAX.defFlat * fi;
  let atkPct = MAX.pct * fi, hpPct = MAX.pct * fi, defPct = 0;
  let dcrit = f.dcrit + MAX.dcrit * fi + (slots >= 8 ? 0.50 * fi : 0);
  let taxa = f.taxa;
  const vel = velocidade(tipo, nivelApostolo) + MAX.vel * fi;   // a Bota é a ÚNICA fonte do jogo

  // subs: espalhadas pelos cinco stats que mexem no poder (ATK% HP% DEF% Taxa DanoCrit)
  const porStat = (slots * SUBS[raridade]) / 5;
  atkPct += porStat * SUB(MAX.pct) * fi;
  hpPct  += porStat * SUB(MAX.pct) * fi;
  defPct += porStat * SUB(MAX.pct) * fi;
  taxa   += porStat * SUB(MAX.pct) * fi;
  dcrit  += porStat * SUB(MAX.dcrit) * fi;

  atk *= (1 + atkPct); hp *= (1 + hpPct); def *= (1 + defPct);
  taxa = Math.min(1, taxa);                                     // Combate.TaxaCrit: clamp 0..1

  return {
    dps: atk * (1 + taxa * dcrit) * vel,
    ehp: hp * (def + DEFESA_MEIA_REDUCAO) / DEFESA_MEIA_REDUCAO,
  };
}

const poderDoTime = lado => {
  let dps = 0, ehp = 0;
  for (const t of TIPOS) { const u = lado(t); dps += u.dps; ehp += u.ehp; }
  return dps * ehp;
};

function nivelQueEmpata(poderAlvo) {
  let lo = 1, hi = 200000;
  for (let i = 0; i < 500; i++) {
    const meio = (lo + hi) / 2;
    if (poderDoTime(t => inimigo(t, meio)) < poderAlvo) lo = meio; else hi = meio;
  }
  return (lo + hi) / 2;
}

// ---------- a régua do jogador: a curva de XP do §A CURVA DE XP ----------

const POTE_POR_APOSTOLO = 18;                      // o `q`: o 72 do pote dividido pelos 4 em campo
const xpParaNivel = L => 50 * L * (L - 1);         // acumulado de `custo N→N+1 = 100 × N`
const nivelPorXp = (xp, teto) => { let n = 1; while (n < teto && xpParaNivel(n + 1) <= xp) n++; return n; };
const indice = (capitulo, fase) => 7 * capitulo + fase;              // k ∈ [8, 63]
const somaAntesDe = k => { let s = 0; for (let j = 8; j < k; j++) s += j; return s; };

// piso/teto = a faixa de nível que o MATERIAL permite (§O TETO DE DIFICULDADE); raridade = até onde
// o drop daquela dificuldade alcança.
const DIFICULDADES = [
  { nome: 'Fácil',    mult: 1, piso: 1,  teto: 30, raridade: 'raro' },
  { nome: 'Normal',   mult: 2, piso: 30, teto: 40, raridade: 'epico' },
  { nome: 'Difícil',  mult: 3, piso: 40, teto: 50, raridade: 'lendario' },
  { nome: 'Pesadelo', mult: 4, piso: 50, teto: 60, raridade: 'mitico' },
];

const nivelDoJogadorEm = (d, k) => nivelPorXp(
  Math.min(xpParaNivel(d.piso) + POTE_POR_APOSTOLO * d.mult * somaAntesDe(k), xpParaNivel(d.teto)),
  d.teto);

// ---------- saída ----------

console.log('JOGADOR — nível ao ENTRAR em cada capítulo (q = ' + POTE_POR_APOSTOLO + ')\n');
console.log('  dificuldade  ' + [1,2,3,4,5,6,7,8].map(c => String(c).padStart(4)).join('') + '   fim');
for (const d of DIFICULDADES) {
  const linha = [1,2,3,4,5,6,7,8].map(c => String(nivelDoJogadorEm(d, indice(c, 1))).padStart(4)).join('');
  // o "fim" é DEPOIS de fechar a 8-7 (k = 64 conta a última fase como já jogada)
  console.log(`  ${d.nome.padEnd(12)}${linha}   ${nivelDoJogadorEm(d, indice(8, 7) + 1)}`);
}

console.log('\nINIMIGO — as âncoras, e o quanto a reta entre elas erra\n');
console.log('  dificuldade  início(1-1)  fim(8-7)  erro máx da reta');
for (const d of DIFICULDADES) {
  const calibrado = [];
  for (let k = 8; k <= 63; k++) {
    const n = nivelDoJogadorEm(d, k);
    calibrado.push(nivelQueEmpata(poderDoTime(t => jogador(t, n, n, d.raridade))));
  }
  const ini = calibrado[0], fim = calibrado[55];
  let erroMax = 0, onde = 0;
  for (let i = 0; i < 56; i++) {
    const reta = ini + (fim - ini) * i / 55;
    const erro = Math.abs(reta - calibrado[i]) / calibrado[i] * 100;
    if (erro > erroMax) { erroMax = erro; onde = i + 8; }
  }
  const cap = Math.floor(onde / 7), fase = onde % 7 || 7;
  console.log(`  ${d.nome.padEnd(12)}${String(Math.round(ini)).padStart(8)}${String(Math.round(fim)).padStart(10)}`
    + `        ${erroMax.toFixed(0)}% (na ${cap}-${fase})`);
}
console.log('\n  nível do inimigo = início + (fim − início) × (7×capítulo + fase − 8) / 55');
