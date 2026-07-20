using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text.Json;
using ApostlesWar.Services;
using ApostlesWar.View;
using ApostlesWar.Controllers;

#region Program

/// <summary>
/// Ponto de entrada do jogo
/// </summary>

Console.OutputEncoding = System.Text.Encoding.UTF8;

var capitulosService = new CapitulosService();
var arsenalService = new ArsenalService(capitulosService);
var campanhaService = new CampanhaService();
var personagemService = new PersonagemService();
var faccaoService = new FaccaoService();
var selecaoDeAlvoService = new SelecaoDeAlvoService();
var entrada = new EntradaConsole();                 // porta de ENTRADA (trocar por EntradaWeb/Unity no porte)
var menuView = new MenuView(faccaoService, arsenalService, capitulosService, entrada);
var combateView = new CombateView(entrada);
var campeoesService = new CampeoesService(personagemService, campanhaService, menuView, capitulosService);
var apresentacao = new ApresentacaoConsole();       // porta de SAÍDA (par do entrada)
var controladorJogador = new ControladorJogador(combateView, entrada);   // trocar por controlador automático liga o modo auto
var controladorBot = new ControladorBot(selecaoDeAlvoService);
var combateService = new CombateService(arsenalService, campanhaService, campeoesService, personagemService, combateView, selecaoDeAlvoService, controladorJogador, controladorBot, apresentacao);
new GerenciadorDeJogoService(arsenalService, campeoesService, capitulosService, menuView, combateService, entrada).Executar();

#endregion
