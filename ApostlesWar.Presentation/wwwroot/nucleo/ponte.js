// A PONTE com o C#. Todo o tráfego nos dois sentidos passa por aqui — é a única peça do front que
// sabe que existe um processo do outro lado.
//
// Não é HTTP: é a mensagem in-process da webview. Quem consome é o `jogo.js`, que registra o
// ouvinte e distribui pelas telas.

export const ponte = window.chrome.webview;

// ---------- envio ----------
// `texto` só é usado quando o valor é uma string (ex: o nome do perfil); o resto manda só o índice.
export const mandar = (tipo, valor = 0, texto = null) => ponte.postMessage(JSON.stringify({ tipo, valor, texto }));
