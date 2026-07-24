using System.ComponentModel;
using System.Reflection;

namespace ApostlesWar.Domain
{
    /// <summary>
    /// Lê o nome de exibição de um valor de enum a partir do seu <see cref="DescriptionAttribute"/>
    /// — "LadoSombrio" vira "Lado Sombrio". Mora no Domain, ao lado dos `[Description]` que ele lê
    /// (Enum/Enums.cs): o nome de uma facção é DADO do jogo, não formatação de uma tela — por isso
    /// qualquer pele (console hoje, front depois) alcança sem depender da outra.
    ///
    /// Veio do projeto externo GHUtils (`Helper.GetDescricao`), dissolvido em jul/2026: era o único
    /// membro daquela biblioteca ainda em uso — o `ConsoleUtils` inteiro tinha morrido quando a porta
    /// IEntrada trouxe o `Navegacao.MoverCursor`. Extensão (e não `Helper.X(valor)`) pra chamada ler
    /// como frase: `faccao.Descricao()`.
    /// </summary>
    public static class DescricaoDeEnum
    {
        /// <summary>Descrição do valor, ou o próprio nome dele quando não há `[Description]`.</summary>
        public static string Descricao(this Enum valor)
        {
            FieldInfo? campo = valor.GetType().GetField(valor.ToString());
            DescriptionAttribute? atributo = campo?.GetCustomAttribute<DescriptionAttribute>();
            return atributo?.Description ?? valor.ToString();
        }
    }
}
