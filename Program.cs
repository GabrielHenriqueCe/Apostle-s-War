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

var repositorioDeSave = new SaveLocal();             // porta de PERSISTÊNCIA (trocar por SaveSteam/SavePlayGames no porte)
var capitulosService = new CapitulosService(repositorioDeSave);
var arsenalService = new ArsenalService(capitulosService, repositorioDeSave);
var personagemService = new PersonagemService();
var selecaoDeAlvoService = new SelecaoDeAlvoService();
var entrada = new EntradaConsole();                 // porta de ENTRADA (trocar por EntradaWeb/Unity no porte)
var apresentacao = new ApresentacaoConsole();       // porta de SAÍDA (par do entrada)
var relogioDoCombate = new RelogioDoCombate();      // contador global de turnos da batalha (CombateService avança, CombateView exibe)
var menuView = new MenuView(arsenalService, capitulosService, entrada, apresentacao);
var combateView = new CombateView(entrada, relogioDoCombate);
var campeoesService = new CampeoesService(personagemService, menuView, capitulosService);
var controladorJogador = new ControladorJogador(combateView, entrada);   // trocar por controlador automático liga o modo auto
var controladorBot = new ControladorBot(selecaoDeAlvoService);
var combateService = new CombateService(arsenalService, campeoesService, personagemService, combateView, selecaoDeAlvoService, controladorJogador, controladorBot, apresentacao, relogioDoCombate);
new GerenciadorDeJogoService(arsenalService, campeoesService, capitulosService, menuView, combateService, entrada).Executar();

#endregion
