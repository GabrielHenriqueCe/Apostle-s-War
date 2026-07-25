#region Program

/// <summary>
/// Ponto de entrada do jogo: sobe a janela webview.
///
/// Já foram DUAS peles sobre o mesmo motor (console e webview, escolhidas por `--front`). O console
/// morreu quando o front ficou jogável de ponta a ponta — era o plano desde sempre (ROADMAP
/// §Princípios: "a camada de apresentação do console morre no porte"). O composition root do que
/// sobrou mora no <see cref="ApostlesWar.Presentation.Front.AppFront"/>.
/// </summary>

return ApostlesWar.Presentation.Front.AppFront.Rodar();

#endregion
