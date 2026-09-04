using ControleFinanceiro.Model;

namespace ControleFinanceiro.Model.Tests;

public class OrcamentoTests
{
    [Fact]
    public void AdicionarItem_IncluiItemNaListaDoOrcamento()
    {
        var orcamento = new Orcamento(mes: 9, ano: 2026);
        var categoria = new Categoria("Ingredientes", TipoCategoria.Despesa);

        var item = orcamento.AdicionarItem(categoria, 500m);

        Assert.Single(orcamento.Itens);
        Assert.Contains(item, orcamento.Itens);
    }

    [Fact]
    public void CalcularTotalPlanejado_SomaValorDeTodosOsItens()
    {
        var orcamento = new Orcamento(mes: 9, ano: 2026);
        var ingredientes = new Categoria("Ingredientes", TipoCategoria.Despesa);
        var aluguel = new Categoria("Aluguel", TipoCategoria.Despesa);

        orcamento.AdicionarItem(ingredientes, 500m);
        orcamento.AdicionarItem(aluguel, 800m);

        Assert.Equal(1300m, orcamento.CalcularTotalPlanejado());
    }

    [Fact]
    public void ItemOrcamento_ReferenciaAMesmaCategoriaCompartilhada()
    {
        var orcamento = new Orcamento(mes: 9, ano: 2026);
        var ingredientes = new Categoria("Ingredientes", TipoCategoria.Despesa);
        var despesa = new Despesa(DateTime.Today, 80m, "Compra de açúcar", ingredientes);

        var item = orcamento.AdicionarItem(ingredientes, 500m);

        Assert.Equal(despesa.Categoria, item.Categoria);
    }

    [Fact]
    public void MesInvalido_LancaExcecao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Orcamento(mes: 13, ano: 2026));
    }
}
