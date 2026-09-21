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
}
