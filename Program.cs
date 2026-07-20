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
var menuView = new MenuView(faccaoService, arsenalService, capitulosService);
var combateView = new CombateView();
var campeoesService = new CampeoesService(personagemService, campanhaService, menuView, capitulosService);
var apresentacao = new ApresentacaoConsole();
var controladorJogador = new ControladorJogador(combateView);   // trocar por controlador automático liga o modo auto
var controladorBot = new ControladorBot(selecaoDeAlvoService);
var combateService = new CombateService(arsenalService, campanhaService, campeoesService, personagemService, combateView, selecaoDeAlvoService, controladorJogador, controladorBot, apresentacao);
new GerenciadorDeJogoService(arsenalService, campeoesService, capitulosService, menuView, combateService).Executar();

#endregion
