using ControleFinanceiro.Model;
using ControleFinanceiro.Persistence;

namespace ControleFinanceiro.Persistence.Tests;

/// <summary>Cada teste usa uma pasta temporária própria, apagada no final.</summary>
public sealed class RepositorioArquivoTextoTests : IDisposable
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "cf-testes-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_pasta))
            Directory.Delete(_pasta, recursive: true);
    }

    [Fact]
    public void PastaInexistente_ComecaVazio()
    {
        var repositorio = new RepositorioArquivoTexto(_pasta);

        Assert.Empty(repositorio.Categorias);
        Assert.Empty(repositorio.Lancamentos);
    }

    [Fact]
    public void Salvar_ERecarregar_PreservaTodosOsDados()
    {
        var original = new RepositorioArquivoTexto(_pasta);
        var vendas = new Categoria("Venda de encomendas", TipoCategoria.Receita);
        var ingredientes = new Categoria("Ingredientes", TipoCategoria.Despesa);
        original.AdicionarCategoria(vendas);
        original.AdicionarCategoria(ingredientes);

        var receita = new Receita(new DateTime(2026, 11, 13), 350.50m, "Bolo de aniversário", vendas, "Pix");
        receita.MarcarComoPago();
        var despesa = new Despesa(new DateTime(2026, 11, 20), 120m, "Chocolate", ingredientes, recorrente: true);
        original.AdicionarLancamento(receita);
        original.AdicionarLancamento(despesa);
        original.ObterOrcamento(11, 2026).AdicionarItem(ingredientes, 200m);
        original.Salvar();

        var recarregado = new RepositorioArquivoTexto(_pasta);

        Assert.Equal(2, recarregado.Categorias.Count);
        var receitaLida = Assert.IsType<Receita>(recarregado.BuscarLancamentoPorId(receita.Id));
        Assert.Equal(new DateTime(2026, 11, 13), receitaLida.Data);
        Assert.Equal(350.50m, receitaLida.Valor);
        Assert.Equal("Bolo de aniversário", receitaLida.Descricao);
        Assert.Equal("Pix", receitaLida.OrigemRecebimento);
        Assert.Equal(StatusLancamento.Pago, receitaLida.Status);
        Assert.Equal(vendas, receitaLida.Categoria);

        var despesaLida = Assert.IsType<Despesa>(recarregado.BuscarLancamentoPorId(despesa.Id));
        Assert.True(despesaLida.Recorrente);
        Assert.Equal(StatusLancamento.Pendente, despesaLida.Status);

        var item = Assert.Single(recarregado.ObterOrcamento(11, 2026).Itens);
        Assert.Equal(ingredientes, item.Categoria);
        Assert.Equal(200m, item.ValorPlanejado);
    }

    [Fact]
    public void Recarregar_LancamentosApontamParaAMesmaInstanciaDaCategoria()
    {
        var original = new RepositorioArquivoTexto(_pasta);
        var ingredientes = new Categoria("Ingredientes", TipoCategoria.Despesa);
        original.AdicionarCategoria(ingredientes);
        original.AdicionarLancamento(new Despesa(DateTime.Today, 10m, "Açúcar", ingredientes));
        original.AdicionarLancamento(new Despesa(DateTime.Today, 20m, "Farinha", ingredientes));
        original.Salvar();

        var recarregado = new RepositorioArquivoTexto(_pasta);

        // Agregação: as duas despesas lidas compartilham o mesmo objeto Categoria.
        Assert.Same(recarregado.Lancamentos[0].Categoria, recarregado.Lancamentos[1].Categoria);
        Assert.Same(recarregado.Categorias.Single(), recarregado.Lancamentos[0].Categoria);
    }

    [Fact]
    public void DepoisDeRecarregar_NovosIdsNaoRepetemOsDoArquivo()
    {
        var categorias = Path.Combine(_pasta, "categorias.txt");
        Directory.CreateDirectory(_pasta);
        File.WriteAllLines(categorias, ["Id;Nome;Tipo", "900000;Antiga;Despesa"]);

        var repositorio = new RepositorioArquivoTexto(_pasta);
        var nova = new Categoria("Nova", TipoCategoria.Despesa);

        Assert.Equal(900000, repositorio.Categorias.Single().Id);
        Assert.True(nova.Id > 900000);
    }

    [Fact]
    public void TextoComPontoEVirgula_EGravadoERelidoSemQuebrarALinha()
    {
        var original = new RepositorioArquivoTexto(_pasta);
        var vendas = new Categoria("Vendas; festas", TipoCategoria.Receita);
        original.AdicionarCategoria(vendas);
        original.AdicionarLancamento(new Receita(DateTime.Today, 80m, @"Doces; salgados \ bebidas", vendas, "Pix"));
        original.Salvar();

        var recarregado = new RepositorioArquivoTexto(_pasta);

        Assert.Equal("Vendas; festas", recarregado.Categorias.Single().Nome);
        Assert.Equal(@"Doces; salgados \ bebidas", recarregado.Lancamentos.Single().Descricao);
    }

    [Fact]
    public void ArquivoCorrompido_LancaExcecaoComArquivoELinha()
    {
        Directory.CreateDirectory(_pasta);
        File.WriteAllLines(Path.Combine(_pasta, "categorias.txt"), ["Id;Nome;Tipo", "1;Vendas;Receita", "2;Ingredientes;Gasto"]);

        var erro = Assert.Throws<ArquivoDeDadosException>(() => new RepositorioArquivoTexto(_pasta));

        Assert.Contains("categorias.txt, linha 3", erro.Message);
        Assert.Contains("Gasto", erro.Message);
    }

    [Fact]
    public void LancamentoComCategoriaInexistente_LancaExcecaoComALinha()
    {
        Directory.CreateDirectory(_pasta);
        File.WriteAllLines(Path.Combine(_pasta, "categorias.txt"), ["Id;Nome;Tipo", "1;Vendas;Receita"]);
        File.WriteAllLines(Path.Combine(_pasta, "lancamentos.txt"),
            ["Id;Tipo;Vencimento;Valor;Descricao;Status;CategoriaId;Origem;Recorrente", "1;Receita;2026-11-13;50;Bolo;Pendente;7;Pix;"]);

        var erro = Assert.Throws<ArquivoDeDadosException>(() => new RepositorioArquivoTexto(_pasta));

        Assert.Contains("lancamentos.txt, linha 2", erro.Message);
        Assert.Contains("categoria 7", erro.Message);
    }

    [Fact]
    public void LinhaComCamposFaltando_LancaExcecao()
    {
        Directory.CreateDirectory(_pasta);
        File.WriteAllLines(Path.Combine(_pasta, "categorias.txt"), ["Id;Nome;Tipo", "1;Vendas"]);

        var erro = Assert.Throws<ArquivoDeDadosException>(() => new RepositorioArquivoTexto(_pasta));

        Assert.Contains("esperava 3 campos", erro.Message);
    }

    [Fact]
    public void Salvar_NaoDeixaArquivosTemporarios()
    {
        var repositorio = new RepositorioArquivoTexto(_pasta);
        repositorio.AdicionarCategoria(new Categoria("Vendas", TipoCategoria.Receita));

        repositorio.Salvar();

        Assert.Equal(
            ["categorias.txt", "lancamentos.txt", "orcamentos.txt"],
            Directory.GetFiles(_pasta).Select(Path.GetFileName).Order());
    }
}
