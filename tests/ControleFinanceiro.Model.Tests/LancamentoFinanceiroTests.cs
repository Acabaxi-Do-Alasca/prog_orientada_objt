using ControleFinanceiro.Model;

namespace ControleFinanceiro.Model.Tests;

public class LancamentoFinanceiroTests
{
    private static Categoria NovaCategoriaVendas() => new("Vendas", TipoCategoria.Receita);
    private static Categoria NovaCategoriaIngredientes() => new("Ingredientes", TipoCategoria.Despesa);

    [Fact]
    public void DoisLancamentosComMesmosDados_SaoEntidadesDistintas()
    {
        var categoria = NovaCategoriaVendas();
        var data = new DateTime(2026, 9, 10);

        var receita1 = new Receita(data, 150m, "Encomenda de bolo", categoria, "Pix");
        var receita2 = new Receita(data, 150m, "Encomenda de bolo", categoria, "Pix");

        Assert.NotEqual(receita1.Id, receita2.Id);
        Assert.NotEqual<LancamentoFinanceiro>(receita1, receita2);
    }

    [Fact]
    public void ReceitaEDespesa_CompartilhamAMesmaCategoria_PorAgregacao()
    {
        var categoria = NovaCategoriaIngredientes();

        var despesa1 = new Despesa(DateTime.Today, 80m, "Compra de açúcar", categoria);
        var despesa2 = new Despesa(DateTime.Today, 45m, "Compra de farinha", categoria);

        Assert.Same(categoria, despesa1.Categoria);
        Assert.Equal(despesa1.Categoria, despesa2.Categoria);
    }

    [Fact]
    public void ListaPolimorfica_AceitaReceitasEDespesas()
    {
        var categoriaReceita = NovaCategoriaVendas();
        var categoriaDespesa = NovaCategoriaIngredientes();

        List<LancamentoFinanceiro> lancamentos =
        [
            new Receita(DateTime.Today, 200m, "Venda de doces", categoriaReceita, "Dinheiro"),
            new Despesa(DateTime.Today, 60m, "Compra de embalagens", categoriaDespesa)
        ];

        Assert.Equal(2, lancamentos.Count);
        Assert.IsType<Receita>(lancamentos[0]);
        Assert.IsType<Despesa>(lancamentos[1]);
    }

    [Fact]
    public void VencimentoProximo_RetornaVerdadeiro_QuandoDentroDoPrazoDeAlerta()
    {
        var referencia = new DateTime(2026, 9, 4);
        var despesa = new Despesa(referencia.AddDays(3), 120m, "Conta de luz", NovaCategoriaIngredientes());

        Assert.True(despesa.VencimentoProximo(referencia, diasAlerta: 5));
    }

    [Fact]
    public void VencimentoProximo_RetornaFalso_QuandoLancamentoJaFoiPago()
    {
        var referencia = new DateTime(2026, 9, 4);
        var despesa = new Despesa(referencia.AddDays(1), 120m, "Conta de luz", NovaCategoriaIngredientes())
        {
            Status = StatusLancamento.Pago
        };

        Assert.False(despesa.VencimentoProximo(referencia));
    }

    [Fact]
    public void ValorMenorOuIgualAZero_LancaExcecao()
    {
        var categoria = NovaCategoriaIngredientes();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Despesa(DateTime.Today, 0m, "Compra inválida", categoria));
    }

    [Fact]
    public void ImpactoNoSaldo_ReceitaSomaEDespesaSubtrai()
    {
        var receita = new Receita(DateTime.Today, 150m, "Venda de bolo", NovaCategoriaVendas(), "Pix");
        var despesa = new Despesa(DateTime.Today, 40m, "Compra de farinha", NovaCategoriaIngredientes());

        Assert.Equal(150m, receita.ImpactoNoSaldo);
        Assert.Equal(-40m, despesa.ImpactoNoSaldo);
    }

    [Fact]
    public void SituacaoEm_PendenteComDataPassada_FicaVencido()
    {
        var hoje = new DateTime(2026, 11, 13);
        var despesa = new Despesa(hoje.AddDays(-1), 120m, "Conta de luz", NovaCategoriaIngredientes());

        Assert.True(despesa.EstaVencido(hoje));
        Assert.Equal(StatusLancamento.Vencido, despesa.SituacaoEm(hoje));
    }

    [Fact]
    public void SituacaoEm_VencendoHoje_AindaNaoEstaVencido()
    {
        var hoje = new DateTime(2026, 11, 13);
        var despesa = new Despesa(hoje, 120m, "Conta de luz", NovaCategoriaIngredientes());

        Assert.False(despesa.EstaVencido(hoje));
        Assert.Equal(StatusLancamento.Pendente, despesa.SituacaoEm(hoje));
    }

    [Fact]
    public void MarcarComoPago_TiraOLancamentoDeVencido()
    {
        var hoje = new DateTime(2026, 11, 13);
        var despesa = new Despesa(hoje.AddDays(-10), 120m, "Conta de luz", NovaCategoriaIngredientes());

        despesa.MarcarComoPago();

        Assert.False(despesa.EstaVencido(hoje));
        Assert.Equal(StatusLancamento.Pago, despesa.SituacaoEm(hoje));
    }

    [Fact]
    public void Atualizar_MudaOsDadosMasNaoOId()
    {
        var despesa = new Despesa(new DateTime(2026, 11, 10), 120m, "Luz", NovaCategoriaIngredientes());
        var id = despesa.Id;
        var outra = new Categoria("Contas fixas", TipoCategoria.Despesa);

        despesa.Atualizar(new DateTime(2026, 11, 20), 135m, "Conta de luz", outra);

        Assert.Equal(id, despesa.Id);
        Assert.Equal(135m, despesa.Valor);
        Assert.Equal("Conta de luz", despesa.Descricao);
        Assert.Same(outra, despesa.Categoria);
    }

    [Fact]
    public void Atualizar_ComDescricaoVazia_LancaExcecaoSemAlterarNada()
    {
        var despesa = new Despesa(DateTime.Today, 120m, "Luz", NovaCategoriaIngredientes());

        Assert.Throws<ArgumentException>(() => despesa.Atualizar(DateTime.Today, 200m, " ", NovaCategoriaIngredientes()));
        Assert.Equal(120m, despesa.Valor);
        Assert.Equal("Luz", despesa.Descricao);
    }

    [Fact]
    public void MarcarComoPendente_DesfazOPagamento()
    {
        var hoje = new DateTime(2026, 11, 13);
        var despesa = new Despesa(hoje.AddDays(-1), 120m, "Luz", NovaCategoriaIngredientes());
        despesa.MarcarComoPago();

        despesa.MarcarComoPendente();

        Assert.Equal(StatusLancamento.Vencido, despesa.SituacaoEm(hoje));
    }
}
