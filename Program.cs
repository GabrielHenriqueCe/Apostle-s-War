using ApostlesWar;
using System;
using System.Collections.Generic;
using System.Text.Json;
using v1_Apostle_s_War.Services;

#region Skill

/// <summary>
/// Retorna o emoji de relógio proporcional ao progresso do cooldown
/// </summary>
string ObterRelogio(int turnosRestantes, int cooldownTotal)
{
    if (turnosRestantes == 0) return "🕛";

    string[] relogios = { "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙" };
    int turnosPassados = cooldownTotal - turnosRestantes;
    int indice = (int)Math.Round((double)turnosPassados * 9 / cooldownTotal) - 1;
    indice = Math.Clamp(indice, 0, 8);
    return relogios[indice];
}

#endregion

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
var menuService = new MenuService(faccaoService, arsenalService, capitulosService);
var campeoesService = new CampeoesService(personagemService, campanhaService, menuService, capitulosService);
new GerenciadorDeJogoService(arsenalService, campanhaService, campeoesService, capitulosService, faccaoService, menuService, personagemService).Executar();
#endregion
