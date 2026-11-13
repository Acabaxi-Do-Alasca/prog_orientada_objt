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

    [Fact]
    public void RemoverItem_TiraACategoriaDoOrcamento()
    {
        var orcamento = new Orcamento(mes: 11, ano: 2026);
        var ingredientes = new Categoria("Ingredientes", TipoCategoria.Despesa);
        var aluguel = new Categoria("Aluguel", TipoCategoria.Despesa);
        orcamento.AdicionarItem(ingredientes, 200m);
        orcamento.AdicionarItem(aluguel, 800m);

        Assert.True(orcamento.RemoverItem(ingredientes));
        Assert.False(orcamento.RemoverItem(ingredientes));
        Assert.Equal(800m, orcamento.CalcularTotalPlanejado());
    }

    [Fact]
    public void AdicionarItem_MesmaCategoria_AtualizaValorEmVezDeDuplicar()
    {
        var orcamento = new Orcamento(mes: 9, ano: 2026);
        var ingredientes = new Categoria("Ingredientes", TipoCategoria.Despesa);

        orcamento.AdicionarItem(ingredientes, 500m);
        orcamento.AdicionarItem(ingredientes, 650m);

        Assert.Single(orcamento.Itens);
        Assert.Equal(650m, orcamento.CalcularTotalPlanejado());
    }

    [Fact]
    public void CompararCom_ConsideraApenasLancamentosDoMesDoOrcamento()
    {
        var orcamento = new Orcamento(mes: 9, ano: 2026);
        var ingredientes = new Categoria("Ingredientes", TipoCategoria.Despesa);
        orcamento.AdicionarItem(ingredientes, 500m);

        var lancamentos = new List<LancamentoFinanceiro>
        {
            new Despesa(new DateTime(2026, 9, 5), 200m, "Farinha", ingredientes),
            new Despesa(new DateTime(2026, 9, 20), 100m, "Açúcar", ingredientes),
            new Despesa(new DateTime(2026, 10, 1), 999m, "Compra de outubro", ingredientes)
        };

        var comparacao = Assert.Single(orcamento.CompararCom(lancamentos));

        Assert.Equal(500m, comparacao.Planejado);
        Assert.Equal(300m, comparacao.Realizado);
        Assert.Equal(-200m, comparacao.Desvio);
        Assert.Equal(60m, comparacao.PercentualRealizado);
        Assert.False(comparacao.ForaDoPlanejado);
    }

    [Fact]
    public void CompararCom_DespesaAcimaDoPlanejado_FicaForaDoPlanejado()
    {
        var orcamento = new Orcamento(mes: 9, ano: 2026);
        var ingredientes = new Categoria("Ingredientes", TipoCategoria.Despesa);
        orcamento.AdicionarItem(ingredientes, 100m);

        var comparacao = Assert.Single(orcamento.CompararCom(
            [new Despesa(new DateTime(2026, 9, 5), 150m, "Chocolate", ingredientes)]));

        Assert.True(comparacao.ForaDoPlanejado);
        Assert.Equal(50m, comparacao.Desvio);
    }

    [Fact]
    public void CompararCom_ReceitaAbaixoDaMeta_FicaForaDoPlanejado()
    {
        var orcamento = new Orcamento(mes: 9, ano: 2026);
        var vendas = new Categoria("Vendas", TipoCategoria.Receita);
        orcamento.AdicionarItem(vendas, 2000m);

        var comparacao = Assert.Single(orcamento.CompararCom(
            [new Receita(new DateTime(2026, 9, 5), 1200m, "Encomendas", vendas, "Pix")]));

        Assert.True(comparacao.ForaDoPlanejado);
    }

    [Fact]
    public void CompararCom_IncluiCategoriaComGastoNaoPlanejado()
    {
        var orcamento = new Orcamento(mes: 9, ano: 2026);
        var embalagens = new Categoria("Embalagens", TipoCategoria.Despesa);

        var comparacao = Assert.Single(orcamento.CompararCom(
            [new Despesa(new DateTime(2026, 9, 5), 80m, "Caixas", embalagens)]));

        Assert.Equal(embalagens, comparacao.Categoria);
        Assert.Equal(0m, comparacao.Planejado);
        Assert.Null(comparacao.PercentualRealizado);
        Assert.True(comparacao.ForaDoPlanejado);
    }

    [Fact]
    public void CompararCom_CategoriaPlanejadaSemLancamentos_TemRealizadoZero()
    {
        var orcamento = new Orcamento(mes: 9, ano: 2026);
        var aluguel = new Categoria("Aluguel", TipoCategoria.Despesa);
        orcamento.AdicionarItem(aluguel, 800m);

        var comparacao = Assert.Single(orcamento.CompararCom([]));

        Assert.Equal(0m, comparacao.Realizado);
        Assert.False(comparacao.ForaDoPlanejado);
    }
}
