using ControleFinanceiro.Model;

namespace ControleFinanceiro.Model.Tests;

public class CategoriaTests
{
    [Fact]
    public void DuasCategoriasComMesmoNome_SaoEntidadesDistintas()
    {
        var ingredientes1 = new Categoria("Ingredientes", TipoCategoria.Despesa);
        var ingredientes2 = new Categoria("Ingredientes", TipoCategoria.Despesa);

        Assert.NotEqual(ingredientes1.Id, ingredientes2.Id);
        Assert.NotEqual(ingredientes1, ingredientes2);
    }

    [Fact]
    public void MesmaInstancia_EIgualAElaMesma()
    {
        var vendas = new Categoria("Vendas", TipoCategoria.Receita);

        Assert.Equal(vendas, vendas);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NomeInvalido_LancaExcecao(string? nomeInvalido)
    {
        Assert.Throws<ArgumentException>(() => new Categoria(nomeInvalido!, TipoCategoria.Despesa));
    }
}
