using ControleFinanceiro.Model;
using ControleFinanceiro.Persistence;

namespace ControleFinanceiro.Service;

/// <summary>
/// Regras de negócio do controle financeiro. A tela só conversa com esta classe; os dados ficam no
/// <see cref="IRepositorioFinanceiro"/> recebido no construtor, que pode ser em memória ou em arquivo texto.
/// Toda operação que altera dados termina chamando <see cref="IRepositorioFinanceiro.Salvar"/>.
/// </summary>
/// <remarks>
/// Os métodos que gravam podem lançar <see cref="ArquivoDeDadosException"/> se o repositório não conseguir salvar.
/// </remarks>
public class ServicoFinanceiro
{
    private readonly IRepositorioFinanceiro _repositorio;

    public ServicoFinanceiro(IRepositorioFinanceiro repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public IReadOnlyCollection<Categoria> Categorias => _repositorio.Categorias;

    /// <summary>No primeiro uso, sem nenhuma categoria, cadastra duas categorias de exemplo do negócio da Célia.</summary>
    public void CriarCategoriasIniciaisSeNecessario()
    {
        if (_repositorio.Categorias.Count > 0)
            return;

        _repositorio.AdicionarCategoria(new Categoria("Venda de encomendas", TipoCategoria.Receita));
        _repositorio.AdicionarCategoria(new Categoria("Ingredientes", TipoCategoria.Despesa));
        _repositorio.Salvar();
    }

    public Categoria CadastrarCategoria(string nome, TipoCategoria tipo)
    {
        var nomeLimpo = nome?.Trim() ?? string.Empty;

        if (_repositorio.Categorias.Any(c => string.Equals(c.Nome, nomeLimpo, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Já existe uma categoria chamada \"{nomeLimpo}\".", nameof(nome));

        var categoria = new Categoria(nomeLimpo, tipo);
        _repositorio.AdicionarCategoria(categoria);
        _repositorio.Salvar();
        return categoria;
    }

    public Receita RegistrarReceita(DateTime vencimento, decimal valor, string descricao, Categoria categoria, string? origem, bool jaRecebida)
    {
        ExigirCategoriaDoTipo(categoria, TipoCategoria.Receita);
        var origemInformada = string.IsNullOrWhiteSpace(origem) ? "Não informado" : origem.Trim();
        return Registrar(new Receita(vencimento.Date, valor, descricao?.Trim() ?? string.Empty, categoria, origemInformada), jaRecebida);
    }

    public Despesa RegistrarDespesa(DateTime vencimento, decimal valor, string descricao, Categoria categoria, bool jaPaga)
    {
        ExigirCategoriaDoTipo(categoria, TipoCategoria.Despesa);
        return Registrar(new Despesa(vencimento.Date, valor, descricao?.Trim() ?? string.Empty, categoria), jaPaga);
    }

    private T Registrar<T>(T lancamento, bool jaPago) where T : LancamentoFinanceiro
    {
        if (jaPago)
            lancamento.MarcarComoPago();

        _repositorio.AdicionarLancamento(lancamento);
        _repositorio.Salvar();
        return lancamento;
    }

    private static void ExigirCategoriaDoTipo(Categoria categoria, TipoCategoria tipo)
    {
        ArgumentNullException.ThrowIfNull(categoria);

        if (categoria.Tipo != tipo)
            throw new ArgumentException(
                $"A categoria \"{categoria.Nome}\" é de {categoria.Tipo.ToString().ToLowerInvariant()}, não de {tipo.ToString().ToLowerInvariant()}.",
                nameof(categoria));
    }

    public void MarcarComoPago(int idLancamento)
    {
        var lancamento = _repositorio.BuscarLancamentoPorId(idLancamento)
            ?? throw new ArgumentException($"Lançamento {idLancamento} não encontrado.", nameof(idLancamento));

        lancamento.MarcarComoPago();
        _repositorio.Salvar();
    }

    /// <summary>Filtra os lançamentos por tipo usando iteração tradicional (foreach + condição).</summary>
    public List<LancamentoFinanceiro> FiltrarPorTipo(TipoLancamentoFiltro filtro)
    {
        var resultado = new List<LancamentoFinanceiro>();

        foreach (var lancamento in _repositorio.Lancamentos)
        {
            var atendeAoFiltro = filtro switch
            {
                TipoLancamentoFiltro.Receitas => lancamento is Receita,
                TipoLancamentoFiltro.Despesas => lancamento is Despesa,
                _ => true
            };

            if (atendeAoFiltro)
                resultado.Add(lancamento);
        }

        return resultado;
    }

    /// <summary>Soma o valor dos lançamentos filtrados usando iteração tradicional (for).</summary>
    public decimal CalcularTotal(TipoLancamentoFiltro filtro)
    {
        var filtrados = FiltrarPorTipo(filtro);
        decimal total = 0m;

        for (var i = 0; i < filtrados.Count; i++)
        {
            total += filtrados[i].Valor;
        }

        return total;
    }

    /// <summary>Saldo do caixa: receitas menos despesas, via polimorfismo de <see cref="LancamentoFinanceiro.ImpactoNoSaldo"/>.</summary>
    public decimal CalcularSaldo() => _repositorio.Lancamentos.Sum(lancamento => lancamento.ImpactoNoSaldo);

    /// <summary>Define (ou atualiza) quanto se planeja gastar ou receber numa categoria no mês.</summary>
    public ItemOrcamento DefinirValorPlanejado(int mes, int ano, Categoria categoria, decimal valorPlanejado)
    {
        ArgumentNullException.ThrowIfNull(categoria);

        if (!_repositorio.Categorias.Contains(categoria))
            throw new ArgumentException($"A categoria \"{categoria.Nome}\" não está cadastrada.", nameof(categoria));

        var item = _repositorio.ObterOrcamento(mes, ano).AdicionarItem(categoria, valorPlanejado);
        _repositorio.Salvar();
        return item;
    }

    public IReadOnlyList<ComparacaoOrcamento> CompararOrcamento(int mes, int ano) =>
        _repositorio.ObterOrcamento(mes, ano).CompararCom(_repositorio.Lancamentos);

    /// <summary>
    /// Lançamentos não pagos que já venceram ou vencem nos próximos <paramref name="diasAlerta"/> dias,
    /// do vencimento mais antigo para o mais recente.
    /// </summary>
    public List<LancamentoFinanceiro> ListarAlertasDeVencimento(DateTime referencia, int diasAlerta)
    {
        var alertas = new List<LancamentoFinanceiro>();

        foreach (var lancamento in _repositorio.Lancamentos)
        {
            if (lancamento.EstaVencido(referencia) || lancamento.VencimentoProximo(referencia, diasAlerta))
                alertas.Add(lancamento);
        }

        alertas.Sort((a, b) => a.Data.CompareTo(b.Data));
        return alertas;
    }
}
