using ApostlesWar.Domain;

/// <summary>
/// Capacidade B do modelo de capacidades, lado ATACANTE — espelho do IModificaDanoRecebido.
/// Uma passiva (ou status) do atacante modifica o dano que ELE causa, consultada sob demanda
/// pela Ação Dano: o multiplicador da habilidade volta a ser só o número da hab, e os
/// modificadores do atacante são dobrados sozinhos (varrendo Habilidades + StatusAtivos).
///
/// Forma MULTIPLICADOR (1.0 = neutro): dobra ANTES do único (int) do Atacar, então a passiva
/// não mexe nos números declarados pelas habilidades. É o molde pra passivas futuras
/// "causa X% mais dano se o alvo tem Y".
/// Implementadores: Piromancer (25% a mais contra alvo com Queima).
/// </summary>
public interface IModificaDanoCausado
{
    double MultiplicadorDeDano(Combate atacante, Combate alvo);
}
