using ControleFinanceiro.Persistence;

namespace ControleFinanceiro.Persistence.Tests;

public class LinhaDeTextoTests
{
    [Theory]
    [InlineData("simples")]
    [InlineData("com; ponto e vírgula")]
    [InlineData(@"com \ barra")]
    [InlineData(@"termina com barra \")]
    [InlineData("")]
    public void JuntarEDividir_DevolveOsMesmosCampos(string campo)
    {
        var linha = LinhaDeTexto.Juntar("1", campo, "fim");

        Assert.Equal(["1", campo, "fim"], LinhaDeTexto.Dividir(linha));
    }
}
