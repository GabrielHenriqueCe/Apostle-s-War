namespace Tests.Bancada
{
    /// <summary>
    /// O <c>[Fact]</c> da BANCADA: só roda com a variável de ambiente <c>BANCADA=1</c>; fora disso o
    /// xUnit o pula. A bancada não afirma nada sobre o motor — ela GERA um relatório versionado, e
    /// custa ~63 s contra os ~0,6 s de todo o resto da suíte.
    ///
    /// Ligar (PowerShell): <c>$env:BANCADA=1; dotnet test</c>.
    /// </summary>
    public sealed class FatoDaBancadaAttribute : FactAttribute
    {
        public FatoDaBancadaAttribute()
        {
            // O xUnit constrói o atributo na DESCOBERTA dos testes, então a variável tem que estar no
            // ambiente antes do `dotnet test` — setá-la de dentro de um fixture chega tarde demais.
            if (Environment.GetEnvironmentVariable("BANCADA") != "1")
                Skip = "bancada: só roda com BANCADA=1 (~63 s, e reescreve docs/bancada-dano.md).";
        }
    }
}
