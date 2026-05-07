// ========================================
// IDEIAS FUTURAS - APOSTLE'S WAR
// ========================================

// COMBATE VISUAL - GRID 4x6
// -------
// Layout do grid:
//   Linha 0: cursores/seleção dos inimigos  ⬜🟥⬜⬜
//   Linha 1: personagens inimigos           💂🥷🧙🫅
//   Linha 2: efeitos/explosões inimigos     ⬜💥⬜⬜
//   Linha 3: efeitos/explosões jogador      ⬜⚔️⬜⬜
//   Linha 4: personagens jogador            👷🧑‍🚒👮🧑‍🍳
//   Linha 5: cursores/seleção do jogador    ⬜🟨⬜⬜
//
// 🟨 = personagem com a vez / cursor de seleção
// 🟥 = alvo selecionado
// 🟩 = alvo de cura
// Navegação: A/D para mover cursor | Enter para confirmar | Esc para cancelar
//
// Habilidades com área de efeito:
//   - Ataque simples (1 alvo)
//   - Ataque duplo (2 alvos adjacentes) ex: Ninja ⬜🟨🟨⬜
//   - Magia de fogo, tornado, explosão etc.
//   - Cura com 💉❤️ ou similar
//
// Emojis duplos a corrigir: Bombeiro 🧑‍🚒, Cozinheiro 🧑‍🍳 (ocupam 2 chars no terminal)

// SELEÇÃO DE PERSONAGENS
// -------
// Antes da partida, jogador escolhe 4 personagens slot a slot.
// Interface: grid espelho mostrando personagem atual + linha de todos disponíveis
//
//   ⬜⬜⬜⬜
//   🧙⬜⬜⬜   <- slot sendo preenchido
//   ⬜⬜⬜⬜
//
//   ⬜⬜⬜⬜⬜⬜🟨⬜
//   👷🧑‍🚒👮🧑‍🍳💂🥷🧙🫅   <- navega com A/D, Enter seleciona, Esc volta slot anterior
//
// Regra: não pode repetir personagens.
// Após escolher os 4 → opção "Iniciar Partida"

// LAYOUTS DE TIME
// -------
// 3 slots de layout salvos pelo jogador
// Opções: Editar Layout / Editar Time
// Evita ter que escolher personagens toda partida

// INTERFACE VISUAL V2 (pós Entra21)
// -------
// Layout de combate mais polido
// Animações de habilidade mais elaboradas
// UI de status com barra de HP visual

// ========================================
// CONJUNTOS DE ITENS POR CAPÍTULO
// ========================================
//
// Layout de equipamentos do personagem:
//   🗡️ 👑 🛡️
//   📿 👔 📿
//   👞 👖 👞
//
// (Arma | Elmo | Escudo)
// (Braceletes | Armadura | Braceletes)
// (Botas | Calça | Botas)
//
// Cap 1 - Reino 👑
//   🗡️ 👑 🛡️
//   📿 👔 📿
//   👞 👖 👞
//
// Cap 2 - Lado Sombrio 🌑
//   🏹 🕶️ 💼
//   🦯 🥋 🦯
//   👟 🦽 👟
//
// Cap 3 - Tecnológicos ⚙️
//   🔫 🥽 🧰
//   🦾 🥼 🦾
//   🛼 🦿 🛼
//
// Cap 4 - Folclore 🪬
//   🪃 🧢 🧳
//   🥊 🎽 🥊
//   🩴 🩳 🩴
//
// Cap 5 - Místicos 🐉
//   🪭 👒 👛
//   💅 🥻 💅
//   👠 👙 👠
//
// Cap 6 - Decaídos 🔱
//   🪄 🎩 🎒
//   🩼 🧥 🩼
//   🥾 🦼 🥾
//
// Cap 7 - Apóstolos 🌬️
//   🎄 🧣 🔔
//   🧤 👘 🧤
//   🪽 🩲 🪽

// SISTEMA DE ITENS - FÓRMULAS APROVADAS
// -------
// Valores escalados por capítulo (cap = número do capítulo, 1 a 8)
// Arma:    ATK flat = 120 * cap
// Elmo:    HP flat  = 550 * cap
// Escudo:  DEF flat =  55 * cap
// Peitoral: HP%     =   5% * cap
// Calça:   DEF%     =   5% * cap
// Manopla: TaxaCrit% = 5% + 1% * cap  (duas manoplas somam)
// Bota:    DanoCrit% = 15% + 1% * cap (duas botas somam)
//
// Referência de balanceamento (Operário + itens do cap vs inimigo Fase 7 do mesmo cap +10%):
// Cap 3: Operário HP:3030 ATK:600 DEF:303 vs Alien HP:2904 ATK:581 DEF:290
// Cap 8: Operário HP:6080 ATK:1200 DEF:608 vs Mímico HP:6204 ATK:1241 DEF:620
// TaxaCrit base: 15% | DanoCrit base: 60%