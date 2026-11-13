using ControleFinanceiro.Model;
using ControleFinanceiro.Persistence;

namespace ControleFinanceiro.Persistence.Tests;

public class RepositorioMemoriaTests
{
    [Fact]
    public void AdicionarCategoria_ApareceNaColecaoDeCategorias()
    {
        var repositorio = new RepositorioMemoria();

        var categoria = repositorio.AdicionarCategoria("Vendas", TipoCategoria.Receita);

        Assert.Contains(categoria, repositorio.Categorias);
    }

    [Fact]
    public void AdicionarLancamento_ApareceNaListaENoDicionarioPorId()
    {
        var repositorio = new RepositorioMemoria();
        var categoria = repositorio.AdicionarCategoria("Vendas", TipoCategoria.Receita);
        var receita = new Receita(DateTime.Today, 100m, "Venda de bolo", categoria, "Pix");

        repositorio.AdicionarLancamento(receita);

        Assert.Contains(receita, repositorio.Lancamentos);
        Assert.Equal(receita, repositorio.BuscarPorId(receita.Id));
    }

    [Fact]
    public void BuscarPorId_RetornaNulo_QuandoIdNaoExiste()
    {
        var repositorio = new RepositorioMemoria();

        Assert.Null(repositorio.BuscarPorId(9999));
    }

    [Fact]
    public void FiltrarPorTipo_RetornaApenasReceitas()
    {
        var repositorio = new RepositorioMemoria();
        var categoriaReceita = repositorio.AdicionarCategoria("Vendas", TipoCategoria.Receita);
        var categoriaDespesa = repositorio.AdicionarCategoria("Ingredientes", TipoCategoria.Despesa);

        var receita = new Receita(DateTime.Today, 100m, "Venda de bolo", categoriaReceita, "Pix");
        var despesa = new Despesa(DateTime.Today, 40m, "Compra de farinha", categoriaDespesa);
        repositorio.AdicionarLancamento(receita);
        repositorio.AdicionarLancamento(despesa);

        var receitas = repositorio.FiltrarPorTipo(TipoLancamentoFiltro.Receitas);

        Assert.Single(receitas);
        Assert.IsType<Receita>(receitas[0]);
    }

    [Fact]
    public void FiltrarPorTipo_Todos_RetornaReceitasEDespesas()
    {
        var repositorio = new RepositorioMemoria();
        var categoriaReceita = repositorio.AdicionarCategoria("Vendas", TipoCategoria.Receita);
        var categoriaDespesa = repositorio.AdicionarCategoria("Ingredientes", TipoCategoria.Despesa);

        repositorio.AdicionarLancamento(new Receita(DateTime.Today, 100m, "Venda de bolo", categoriaReceita, "Pix"));
        repositorio.AdicionarLancamento(new Despesa(DateTime.Today, 40m, "Compra de farinha", categoriaDespesa));

        Assert.Equal(2, repositorio.FiltrarPorTipo(TipoLancamentoFiltro.Todos).Count);
    }

    [Fact]
    public void CalcularTotal_SomaApenasOsLancamentosDoFiltro()
    {
        var repositorio = new RepositorioMemoria();
        var categoriaReceita = repositorio.AdicionarCategoria("Vendas", TipoCategoria.Receita);
        var categoriaDespesa = repositorio.AdicionarCategoria("Ingredientes", TipoCategoria.Despesa);

        repositorio.AdicionarLancamento(new Receita(DateTime.Today, 100m, "Venda de bolo", categoriaReceita, "Pix"));
        repositorio.AdicionarLancamento(new Receita(DateTime.Today, 50m, "Venda de doce", categoriaReceita, "Dinheiro"));
        repositorio.AdicionarLancamento(new Despesa(DateTime.Today, 40m, "Compra de farinha", categoriaDespesa));

        Assert.Equal(150m, repositorio.CalcularTotal(TipoLancamentoFiltro.Receitas));
        Assert.Equal(40m, repositorio.CalcularTotal(TipoLancamentoFiltro.Despesas));
        Assert.Equal(190m, repositorio.CalcularTotal(TipoLancamentoFiltro.Todos));
    }

    [Fact]
    public void CalcularSaldo_SubtraiDespesasDasReceitas()
    {
        var repositorio = new RepositorioMemoria();
        var categoriaReceita = repositorio.AdicionarCategoria("Vendas", TipoCategoria.Receita);
        var categoriaDespesa = repositorio.AdicionarCategoria("Ingredientes", TipoCategoria.Despesa);

        repositorio.AdicionarLancamento(new Receita(DateTime.Today, 100m, "Venda de bolo", categoriaReceita, "Pix"));
        repositorio.AdicionarLancamento(new Receita(DateTime.Today, 50m, "Venda de doce", categoriaReceita, "Dinheiro"));
        repositorio.AdicionarLancamento(new Despesa(DateTime.Today, 40m, "Compra de farinha", categoriaDespesa));

        Assert.Equal(110m, repositorio.CalcularSaldo());
    }

    [Fact]
    public void CalcularSaldo_FicaNegativo_QuandoDespesasSuperamReceitas()
    {
        var repositorio = new RepositorioMemoria();
        var categoriaReceita = repositorio.AdicionarCategoria("Vendas", TipoCategoria.Receita);
        var categoriaDespesa = repositorio.AdicionarCategoria("Aluguel", TipoCategoria.Despesa);

        repositorio.AdicionarLancamento(new Receita(DateTime.Today, 300m, "Encomendas", categoriaReceita, "Pix"));
        repositorio.AdicionarLancamento(new Despesa(DateTime.Today, 800m, "Aluguel da cozinha", categoriaDespesa));

        Assert.Equal(-500m, repositorio.CalcularSaldo());
    }

    [Fact]
    public void CalcularSaldo_EZero_SemLancamentos()
    {
        Assert.Equal(0m, new RepositorioMemoria().CalcularSaldo());
    }

    [Fact]
    public void ObterOrcamento_MesmoMes_RetornaOMesmoOrcamento()
    {
        var repositorio = new RepositorioMemoria();

        Assert.Same(repositorio.ObterOrcamento(9, 2026), repositorio.ObterOrcamento(9, 2026));
        Assert.NotSame(repositorio.ObterOrcamento(9, 2026), repositorio.ObterOrcamento(10, 2026));
    }

    [Fact]
    public void CompararOrcamento_UsaOsLancamentosCadastrados()
    {
        var repositorio = new RepositorioMemoria();
        var ingredientes = repositorio.AdicionarCategoria("Ingredientes", TipoCategoria.Despesa);
        repositorio.ObterOrcamento(9, 2026).AdicionarItem(ingredientes, 500m);
        repositorio.AdicionarLancamento(new Despesa(new DateTime(2026, 9, 10), 620m, "Chocolate", ingredientes));

        var comparacao = Assert.Single(repositorio.CompararOrcamento(9, 2026));

        Assert.Equal(620m, comparacao.Realizado);
        Assert.True(comparacao.ForaDoPlanejado);
    }

    [Fact]
    public void ListarAlertasDeVencimento_TrazVencidosEVencendo_OrdenadosPorData()
    {
        var hoje = new DateTime(2026, 11, 13);
        var repositorio = new RepositorioMemoria();
        var contas = repositorio.AdicionarCategoria("Contas", TipoCategoria.Despesa);

        var vencendo = new Despesa(hoje.AddDays(3), 200m, "Gás", contas);
        var vencida = new Despesa(hoje.AddDays(-2), 150m, "Luz", contas);
        var paga = new Despesa(hoje.AddDays(-5), 90m, "Água", contas);
        paga.MarcarComoPago();
        var distante = new Despesa(hoje.AddDays(30), 800m, "Aluguel", contas);

        foreach (var lancamento in new[] { vencendo, vencida, paga, distante })
            repositorio.AdicionarLancamento(lancamento);

        var alertas = repositorio.ListarAlertasDeVencimento(hoje, diasAlerta: 5);

        Assert.Equal([vencida, vencendo], alertas);
    }
}
