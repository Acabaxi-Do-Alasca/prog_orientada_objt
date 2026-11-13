using ControleFinanceiro.Model;
using ControleFinanceiro.Persistence;
using ControleFinanceiro.Service;

namespace ControleFinanceiro.Service.Tests;

public class ServicoFinanceiroTests
{
    private readonly RepositorioMemoria _repositorio = new();
    private readonly ServicoFinanceiro _servico;
    private readonly Categoria _vendas;
    private readonly Categoria _ingredientes;

    public ServicoFinanceiroTests()
    {
        _servico = new ServicoFinanceiro(_repositorio);
        _vendas = _servico.CadastrarCategoria("Vendas", TipoCategoria.Receita);
        _ingredientes = _servico.CadastrarCategoria("Ingredientes", TipoCategoria.Despesa);
    }

    // ---------- categorias ----------

    [Fact]
    public void CriarCategoriasIniciais_SoCriaQuandoNaoHaNenhuma()
    {
        var vazio = new ServicoFinanceiro(new RepositorioMemoria());

        vazio.CriarCategoriasIniciaisSeNecessario();
        vazio.CriarCategoriasIniciaisSeNecessario();
        _servico.CriarCategoriasIniciaisSeNecessario();

        Assert.Equal(2, vazio.Categorias.Count);
        Assert.Equal(2, _servico.Categorias.Count);
    }

    [Fact]
    public void CadastrarCategoria_ComNomeRepetido_LancaExcecao()
    {
        var erro = Assert.Throws<ArgumentException>(() => _servico.CadastrarCategoria("  ingredientes ", TipoCategoria.Despesa));

        Assert.Contains("Já existe", erro.Message);
        Assert.Equal(2, _servico.Categorias.Count);
    }

    // ---------- lançamentos ----------

    [Fact]
    public void RegistrarReceita_SemOrigem_UsaNaoInformado()
    {
        var receita = _servico.RegistrarReceita(DateTime.Today, 100m, "Bolo", _vendas, "  ", jaRecebida: false);

        Assert.Equal("Não informado", receita.OrigemRecebimento);
        Assert.Equal(StatusLancamento.Pendente, receita.Status);
    }

    [Fact]
    public void RegistrarDespesa_JaPaga_EntraComoPaga()
    {
        var despesa = _servico.RegistrarDespesa(DateTime.Today, 40m, "Farinha", _ingredientes, jaPaga: true);

        Assert.Equal(StatusLancamento.Pago, despesa.Status);
    }

    [Fact]
    public void RegistrarReceita_EmCategoriaDeDespesa_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            _servico.RegistrarReceita(DateTime.Today, 100m, "Bolo", _ingredientes, "Pix", jaRecebida: false));
        Assert.Empty(_repositorio.Lancamentos);
    }

    [Fact]
    public void RegistrarDespesa_EmCategoriaDeReceita_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            _servico.RegistrarDespesa(DateTime.Today, 40m, "Farinha", _vendas, jaPaga: false));
    }

    [Fact]
    public void MarcarComoPago_MudaOStatusDoLancamento()
    {
        var despesa = _servico.RegistrarDespesa(DateTime.Today.AddDays(-3), 120m, "Luz", _ingredientes, jaPaga: false);

        _servico.MarcarComoPago(despesa.Id);

        Assert.Equal(StatusLancamento.Pago, despesa.Status);
    }

    [Fact]
    public void MarcarComoPago_IdInexistente_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => _servico.MarcarComoPago(9999));
    }

    // ---------- filtros, totais e saldo ----------

    [Fact]
    public void FiltrarPorTipo_SeparaReceitasEDespesas()
    {
        _servico.RegistrarReceita(DateTime.Today, 100m, "Bolo", _vendas, "Pix", jaRecebida: false);
        _servico.RegistrarDespesa(DateTime.Today, 40m, "Farinha", _ingredientes, jaPaga: false);

        Assert.IsType<Receita>(Assert.Single(_servico.FiltrarPorTipo(TipoLancamentoFiltro.Receitas)));
        Assert.IsType<Despesa>(Assert.Single(_servico.FiltrarPorTipo(TipoLancamentoFiltro.Despesas)));
        Assert.Equal(2, _servico.FiltrarPorTipo(TipoLancamentoFiltro.Todos).Count);
    }

    [Fact]
    public void CalcularTotal_SomaApenasOsLancamentosDoFiltro()
    {
        _servico.RegistrarReceita(DateTime.Today, 100m, "Bolo", _vendas, "Pix", jaRecebida: false);
        _servico.RegistrarReceita(DateTime.Today, 50m, "Doce", _vendas, "Dinheiro", jaRecebida: false);
        _servico.RegistrarDespesa(DateTime.Today, 40m, "Farinha", _ingredientes, jaPaga: false);

        Assert.Equal(150m, _servico.CalcularTotal(TipoLancamentoFiltro.Receitas));
        Assert.Equal(40m, _servico.CalcularTotal(TipoLancamentoFiltro.Despesas));
    }

    [Fact]
    public void CalcularSaldoPrevisto_SubtraiDespesasDasReceitas()
    {
        _servico.RegistrarReceita(DateTime.Today, 150m, "Bolo", _vendas, "Pix", jaRecebida: false);
        _servico.RegistrarDespesa(DateTime.Today, 40m, "Farinha", _ingredientes, jaPaga: false);

        Assert.Equal(110m, _servico.CalcularSaldoPrevisto());
    }

    [Fact]
    public void CalcularSaldoPrevisto_FicaNegativo_QuandoDespesasSuperamReceitas()
    {
        _servico.RegistrarReceita(DateTime.Today, 300m, "Encomendas", _vendas, "Pix", jaRecebida: false);
        _servico.RegistrarDespesa(DateTime.Today, 800m, "Aluguel", _ingredientes, jaPaga: false);

        Assert.Equal(-500m, _servico.CalcularSaldoPrevisto());
    }

    [Fact]
    public void CalcularSaldoPrevisto_EZero_SemLancamentos()
    {
        Assert.Equal(0m, _servico.CalcularSaldoPrevisto());
    }

    // ---------- orçamento ----------

    [Fact]
    public void DefinirValorPlanejado_EComparar_UsaOsLancamentosDoMes()
    {
        _servico.DefinirValorPlanejado(9, 2026, _ingredientes, 500m);
        _servico.RegistrarDespesa(new DateTime(2026, 9, 10), 620m, "Chocolate", _ingredientes, jaPaga: false);

        var comparacao = Assert.Single(_servico.CompararOrcamento(9, 2026));

        Assert.Equal(500m, comparacao.Planejado);
        Assert.Equal(620m, comparacao.Realizado);
        Assert.True(comparacao.ForaDoPlanejado);
    }

    [Fact]
    public void DefinirValorPlanejado_CategoriaNaoCadastrada_LancaExcecao()
    {
        var avulsa = new Categoria("Avulsa", TipoCategoria.Despesa);

        Assert.Throws<ArgumentException>(() => _servico.DefinirValorPlanejado(9, 2026, avulsa, 100m));
    }

    // ---------- vencimentos ----------

    [Fact]
    public void ListarAlertasDeVencimento_TrazVencidosEVencendo_OrdenadosPorData()
    {
        var hoje = new DateTime(2026, 11, 13);
        var vencendo = _servico.RegistrarDespesa(hoje.AddDays(3), 200m, "Gás", _ingredientes, jaPaga: false);
        var vencida = _servico.RegistrarDespesa(hoje.AddDays(-2), 150m, "Luz", _ingredientes, jaPaga: false);
        _servico.RegistrarDespesa(hoje.AddDays(-5), 90m, "Água", _ingredientes, jaPaga: true);
        _servico.RegistrarDespesa(hoje.AddDays(30), 800m, "Aluguel", _ingredientes, jaPaga: false);

        var alertas = _servico.ListarAlertasDeVencimento(hoje, diasAlerta: 5);

        Assert.Equal<LancamentoFinanceiro>([vencida, vencendo], alertas);
    }

    // ---------- integração com o repositório em arquivo ----------

    [Fact]
    public void ComRepositorioEmArquivo_OsDadosSobrevivemAoReiniciar()
    {
        var pasta = Path.Combine(Path.GetTempPath(), "cf-servico-" + Guid.NewGuid().ToString("N"));
        try
        {
            var servico = new ServicoFinanceiro(new RepositorioArquivoTexto(pasta));
            servico.CriarCategoriasIniciaisSeNecessario();
            var ingredientes = servico.Categorias.Single(c => c.Tipo == TipoCategoria.Despesa);
            var luz = servico.RegistrarDespesa(new DateTime(2026, 11, 10), 120m, "Luz", ingredientes, jaPaga: false);
            servico.MarcarComoPago(luz.Id);
            servico.DefinirValorPlanejado(11, 2026, ingredientes, 200m);

            // "Fecha e abre o programa": um serviço novo lendo a mesma pasta.
            var reaberto = new ServicoFinanceiro(new RepositorioArquivoTexto(pasta));
            reaberto.CriarCategoriasIniciaisSeNecessario();

            Assert.Equal(2, reaberto.Categorias.Count);
            Assert.Equal(-120m, reaberto.CalcularSaldoPrevisto());
            Assert.Empty(reaberto.ListarAlertasDeVencimento(new DateTime(2026, 11, 13), 5));
            Assert.Equal(200m, Assert.Single(reaberto.CompararOrcamento(11, 2026)).Planejado);
        }
        finally
        {
            if (Directory.Exists(pasta))
                Directory.Delete(pasta, recursive: true);
        }
    }
}
