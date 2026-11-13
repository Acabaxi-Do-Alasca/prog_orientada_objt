using ControleFinanceiro.Model;
using ControleFinanceiro.Persistence;

namespace ControleFinanceiro.Persistence.Tests;

public class RepositorioMemoriaTests
{
    [Fact]
    public void AdicionarCategoria_MesmaCategoriaDuasVezes_GuardaUmaSo()
    {
        var repositorio = new RepositorioMemoria();
        var vendas = new Categoria("Vendas", TipoCategoria.Receita);

        repositorio.AdicionarCategoria(vendas);
        repositorio.AdicionarCategoria(vendas);

        Assert.Single(repositorio.Categorias);
    }

    [Fact]
    public void AdicionarLancamento_ApareceNaListaENaBuscaPorId()
    {
        var repositorio = new RepositorioMemoria();
        var vendas = new Categoria("Vendas", TipoCategoria.Receita);
        repositorio.AdicionarCategoria(vendas);
        var receita = new Receita(DateTime.Today, 100m, "Venda de bolo", vendas, "Pix");

        repositorio.AdicionarLancamento(receita);

        Assert.Contains(receita, repositorio.Lancamentos);
        Assert.Same(receita, repositorio.BuscarLancamentoPorId(receita.Id));
    }

    [Fact]
    public void BuscarLancamentoPorId_RetornaNulo_QuandoIdNaoExiste()
    {
        Assert.Null(new RepositorioMemoria().BuscarLancamentoPorId(9999));
    }

    [Fact]
    public void AdicionarLancamento_ComCategoriaNaoCadastrada_LancaExcecao()
    {
        var repositorio = new RepositorioMemoria();
        var despesa = new Despesa(DateTime.Today, 40m, "Farinha", new Categoria("Ingredientes", TipoCategoria.Despesa));

        Assert.Throws<ArgumentException>(() => repositorio.AdicionarLancamento(despesa));
    }

    [Fact]
    public void AdicionarLancamento_MesmoLancamentoDuasVezes_LancaExcecao()
    {
        var repositorio = new RepositorioMemoria();
        var ingredientes = new Categoria("Ingredientes", TipoCategoria.Despesa);
        repositorio.AdicionarCategoria(ingredientes);
        var despesa = new Despesa(DateTime.Today, 40m, "Farinha", ingredientes);
        repositorio.AdicionarLancamento(despesa);

        Assert.Throws<ArgumentException>(() => repositorio.AdicionarLancamento(despesa));
        Assert.Single(repositorio.Lancamentos);
    }

    [Fact]
    public void ObterOrcamento_MesmoMes_RetornaOMesmoOrcamento()
    {
        var repositorio = new RepositorioMemoria();

        Assert.Same(repositorio.ObterOrcamento(9, 2026), repositorio.ObterOrcamento(9, 2026));
        Assert.NotSame(repositorio.ObterOrcamento(9, 2026), repositorio.ObterOrcamento(10, 2026));
    }
}
