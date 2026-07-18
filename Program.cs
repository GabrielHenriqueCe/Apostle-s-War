using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text.Json;
using ApostlesWar.Services;

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
var menuService = new MenuService(faccaoService, arsenalService, capitulosService);
var campeoesService = new CampeoesService(personagemService, campanhaService, menuService, capitulosService);
var apresentacao = new ApresentacaoConsole();
var controladorJogador = new ControladorJogador(menuService);   // trocar por controlador automático liga o modo auto
var controladorBot = new ControladorBot(selecaoDeAlvoService);
var combateService = new CombateService(arsenalService, campanhaService, campeoesService, personagemService, menuService, selecaoDeAlvoService, controladorJogador, controladorBot, apresentacao);
new GerenciadorDeJogoService(arsenalService, campeoesService, capitulosService, menuService, combateService).Executar();

#endregion
