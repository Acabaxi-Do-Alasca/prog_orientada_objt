using ControleFinanceiro.Model;
using ControleFinanceiro.Persistence;
using ControleFinanceiro.Service;

namespace ControleFinanceiro.Service.Tests;

/// <summary>Editar e excluir categorias e lançamentos, remover valor planejado, prazo de alerta e saldo realizado.</summary>
public class EdicaoEConfiguracaoTests
{
    private readonly RepositorioMemoria _repositorio = new();
    private readonly ServicoFinanceiro _servico;
    private readonly Categoria _vendas;
    private readonly Categoria _ingredientes;

    public EdicaoEConfiguracaoTests()
    {
        _servico = new ServicoFinanceiro(_repositorio);
        _vendas = _servico.CadastrarCategoria("Vendas", TipoCategoria.Receita);
        _ingredientes = _servico.CadastrarCategoria("Ingredientes", TipoCategoria.Despesa);
    }

    // ---------- categorias ----------

    [Fact]
    public void EditarCategoria_RenomeiaMantendoOId()
    {
        var id = _ingredientes.Id;

        _servico.EditarCategoria(_ingredientes, "  Insumos ", TipoCategoria.Despesa);

        Assert.Equal("Insumos", _ingredientes.Nome);
        Assert.Equal(id, _ingredientes.Id);
    }

    [Fact]
    public void EditarCategoria_ParaNomeDeOutra_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => _servico.EditarCategoria(_ingredientes, "vendas", TipoCategoria.Despesa));
        Assert.Equal("Ingredientes", _ingredientes.Nome);
    }

    [Fact]
    public void EditarCategoria_MesmoNome_NaoContaComoRepetido()
    {
        _servico.EditarCategoria(_ingredientes, "INGREDIENTES", TipoCategoria.Despesa);

        Assert.Equal("INGREDIENTES", _ingredientes.Nome);
    }

    [Fact]
    public void EditarCategoria_TrocarTipoComLancamentos_LancaExcecao()
    {
        _servico.RegistrarDespesa(DateTime.Today, 40m, "Farinha", _ingredientes, jaPaga: false);

        Assert.Throws<ArgumentException>(() => _servico.EditarCategoria(_ingredientes, "Ingredientes", TipoCategoria.Receita));
        Assert.Equal(TipoCategoria.Despesa, _ingredientes.Tipo);
    }

    [Fact]
    public void EditarCategoria_TrocarTipoSemLancamentos_Funciona()
    {
        _servico.EditarCategoria(_ingredientes, "Ingredientes", TipoCategoria.Receita);

        Assert.Equal(TipoCategoria.Receita, _ingredientes.Tipo);
    }

    [Fact]
    public void ExcluirCategoria_ComLancamentos_LancaExcecaoEMantemACategoria()
    {
        _servico.RegistrarDespesa(DateTime.Today, 40m, "Farinha", _ingredientes, jaPaga: false);

        var erro = Assert.Throws<ArgumentException>(() => _servico.ExcluirCategoria(_ingredientes));

        Assert.Contains("1 lançamento", erro.Message);
        Assert.Contains(_ingredientes, _servico.Categorias);
    }

    [Fact]
    public void ExcluirCategoria_SemLancamentos_RemoveTambemOsValoresPlanejados()
    {
        _servico.DefinirValorPlanejado(11, 2026, _ingredientes, 200m);

        _servico.ExcluirCategoria(_ingredientes);

        Assert.DoesNotContain(_ingredientes, _servico.Categorias);
        Assert.Empty(_servico.CompararOrcamento(11, 2026));
    }

    // ---------- lançamentos ----------

    [Fact]
    public void EditarLancamento_AlteraOsDadosEMantemOId()
    {
        var despesa = _servico.RegistrarDespesa(new DateTime(2026, 11, 10), 120m, "Luz", _ingredientes, jaPaga: false);
        var outra = _servico.CadastrarCategoria("Contas fixas", TipoCategoria.Despesa);

        _servico.EditarLancamento(despesa.Id, new DateTime(2026, 11, 20), 135.5m, " Conta de luz ", outra, origem: null, pago: true);

        var editada = _servico.BuscarLancamento(despesa.Id);
        Assert.Same(despesa, editada);
        Assert.Equal(new DateTime(2026, 11, 20), editada.Data);
        Assert.Equal(135.5m, editada.Valor);
        Assert.Equal("Conta de luz", editada.Descricao);
        Assert.Equal(outra, editada.Categoria);
        Assert.Equal(StatusLancamento.Pago, editada.Status);
    }

    [Fact]
    public void EditarLancamento_DesmarcarPago_VoltaParaPendente()
    {
        var despesa = _servico.RegistrarDespesa(DateTime.Today, 50m, "Gás", _ingredientes, jaPaga: true);

        _servico.EditarLancamento(despesa.Id, DateTime.Today, 50m, "Gás", _ingredientes, origem: null, pago: false);

        Assert.Equal(StatusLancamento.Pendente, despesa.Status);
    }

    [Fact]
    public void EditarLancamento_Receita_AtualizaAOrigem()
    {
        var receita = _servico.RegistrarReceita(DateTime.Today, 100m, "Bolo", _vendas, "Pix", jaRecebida: false);

        _servico.EditarLancamento(receita.Id, DateTime.Today, 100m, "Bolo", _vendas, "Dinheiro", pago: false);

        Assert.Equal("Dinheiro", receita.OrigemRecebimento);
    }

    [Fact]
    public void EditarLancamento_ParaCategoriaDeOutroTipo_LancaExcecaoSemAlterar()
    {
        var despesa = _servico.RegistrarDespesa(DateTime.Today, 40m, "Farinha", _ingredientes, jaPaga: false);

        Assert.Throws<ArgumentException>(() =>
            _servico.EditarLancamento(despesa.Id, DateTime.Today, 40m, "Farinha", _vendas, origem: null, pago: false));
        Assert.Equal(_ingredientes, despesa.Categoria);
    }

    [Fact]
    public void EditarLancamento_ComValorInvalido_LancaExcecaoSemAlterar()
    {
        var despesa = _servico.RegistrarDespesa(DateTime.Today, 40m, "Farinha", _ingredientes, jaPaga: false);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _servico.EditarLancamento(despesa.Id, DateTime.Today, 0m, "Farinha", _ingredientes, origem: null, pago: false));
        Assert.Equal(40m, despesa.Valor);
    }

    [Fact]
    public void ExcluirLancamento_TiraDaListaEDaBuscaPorId()
    {
        var despesa = _servico.RegistrarDespesa(DateTime.Today, 40m, "Farinha", _ingredientes, jaPaga: false);

        _servico.ExcluirLancamento(despesa.Id);

        Assert.Empty(_servico.FiltrarPorTipo(TipoLancamentoFiltro.Todos));
        Assert.Throws<ArgumentException>(() => _servico.BuscarLancamento(despesa.Id));
    }

    [Fact]
    public void DepoisDeExcluirOsLancamentos_ACategoriaPodeSerExcluida()
    {
        var despesa = _servico.RegistrarDespesa(DateTime.Today, 40m, "Farinha", _ingredientes, jaPaga: false);
        _servico.ExcluirLancamento(despesa.Id);

        _servico.ExcluirCategoria(_ingredientes);

        Assert.DoesNotContain(_ingredientes, _servico.Categorias);
    }

    // ---------- orçamento ----------

    [Fact]
    public void RemoverValorPlanejado_TiraACategoriaDoOrcamentoDoMes()
    {
        _servico.DefinirValorPlanejado(11, 2026, _ingredientes, 200m);

        Assert.True(_servico.RemoverValorPlanejado(11, 2026, _ingredientes));
        Assert.Empty(_servico.CompararOrcamento(11, 2026));
        Assert.False(_servico.RemoverValorPlanejado(11, 2026, _ingredientes));
    }

    // ---------- prazo de alerta ----------

    [Fact]
    public void DiasAlerta_ComecaEmCincoDias()
    {
        Assert.Equal(5, _servico.DiasAlerta);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(61)]
    public void DefinirDiasAlerta_ForaDoIntervalo_LancaExcecao(int dias)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _servico.DefinirDiasAlerta(dias));
        Assert.Equal(5, _servico.DiasAlerta);
    }

    [Fact]
    public void DefinirDiasAlerta_MudaQuemApareceNosAlertas()
    {
        var hoje = new DateTime(2026, 11, 13);
        var aluguel = _servico.RegistrarDespesa(hoje.AddDays(10), 800m, "Aluguel", _ingredientes, jaPaga: false);

        Assert.Empty(_servico.ListarAlertasDeVencimento(hoje));

        _servico.DefinirDiasAlerta(10);

        Assert.Equal<LancamentoFinanceiro>([aluguel], _servico.ListarAlertasDeVencimento(hoje));
    }

    // ---------- saldo realizado x previsto ----------

    [Fact]
    public void SaldoRealizado_ConsideraSoOQueFoiPagoOuRecebido()
    {
        _servico.RegistrarReceita(DateTime.Today, 350m, "Bolo", _vendas, "Pix", jaRecebida: true);
        _servico.RegistrarReceita(DateTime.Today, 480m, "Doces", _vendas, "Pix", jaRecebida: false);
        _servico.RegistrarDespesa(DateTime.Today, 260m, "Chocolate", _ingredientes, jaPaga: true);
        _servico.RegistrarDespesa(DateTime.Today, 120m, "Luz", _ingredientes, jaPaga: false);

        Assert.Equal(90m, _servico.CalcularSaldoRealizado());
        Assert.Equal(450m, _servico.CalcularSaldoPrevisto());
    }

    // ---------- tudo isso sobrevive a fechar e abrir ----------

    [Fact]
    public void EdicoesExclusoesEPrazo_SobrevivemAoReiniciar()
    {
        var pasta = Path.Combine(Path.GetTempPath(), "cf-edicao-" + Guid.NewGuid().ToString("N"));
        try
        {
            var servico = new ServicoFinanceiro(new RepositorioArquivoTexto(pasta));
            var contas = servico.CadastrarCategoria("Contas", TipoCategoria.Despesa);
            var avulsa = servico.CadastrarCategoria("Avulsa", TipoCategoria.Despesa);
            var luz = servico.RegistrarDespesa(new DateTime(2026, 11, 10), 120m, "Luz", contas, jaPaga: false);
            var agua = servico.RegistrarDespesa(new DateTime(2026, 11, 12), 90m, "Água", contas, jaPaga: false);
            servico.EditarCategoria(contas, "Contas fixas", TipoCategoria.Despesa);
            servico.EditarLancamento(luz.Id, new DateTime(2026, 11, 15), 130m, "Conta de luz", contas, origem: null, pago: true);
            servico.ExcluirLancamento(agua.Id);
            servico.ExcluirCategoria(avulsa);
            servico.DefinirDiasAlerta(7);

            var reaberto = new ServicoFinanceiro(new RepositorioArquivoTexto(pasta));

            var categoria = Assert.Single(reaberto.Categorias);
            Assert.Equal("Contas fixas", categoria.Nome);
            var lancamento = Assert.Single(reaberto.FiltrarPorTipo(TipoLancamentoFiltro.Todos));
            Assert.Equal(luz.Id, lancamento.Id);
            Assert.Equal("Conta de luz", lancamento.Descricao);
            Assert.Equal(130m, lancamento.Valor);
            Assert.Equal(StatusLancamento.Pago, lancamento.Status);
            Assert.Equal(7, reaberto.DiasAlerta);
        }
        finally
        {
            if (Directory.Exists(pasta))
                Directory.Delete(pasta, recursive: true);
        }
    }
}
