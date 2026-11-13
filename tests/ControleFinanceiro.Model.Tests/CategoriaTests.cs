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

    [Fact]
    public void RecriadaComOMesmoId_EAMesmaEntidade()
    {
        var original = new Categoria("Vendas", TipoCategoria.Receita);
        var lidaDoArquivo = new Categoria(original.Id, "Vendas", TipoCategoria.Receita);

        Assert.Equal(original, lidaDoArquivo);
    }

    [Fact]
    public void DepoisDeRecriarUmIdAlto_NovasCategoriasRecebemIdsMaiores()
    {
        new Categoria(500_000, "Antiga", TipoCategoria.Despesa);

        var nova = new Categoria("Nova", TipoCategoria.Despesa);

        Assert.True(nova.Id > 500_000);
    }

    [Fact]
    public void RecriarComIdZero_LancaExcecao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Categoria(0, "Vendas", TipoCategoria.Receita));
    }
}
