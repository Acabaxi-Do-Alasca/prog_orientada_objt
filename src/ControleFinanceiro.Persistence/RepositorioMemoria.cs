using ControleFinanceiro.Model;

namespace ControleFinanceiro.Persistence;

/// <summary>
/// Guarda as entidades em coleções genéricas, em memória. Os dados somem quando o programa fecha;
/// <see cref="RepositorioArquivoTexto"/> herda desta classe para gravá-los em arquivo.
/// </summary>
public class RepositorioMemoria : IRepositorioFinanceiro
{
    // HashSet: Categoria implementa Equals/GetHashCode por Id, então a mesma categoria não entra duas vezes.
    private readonly HashSet<Categoria> _categorias = new();

    // List: mantém a ordem de cadastro dos lançamentos.
    private readonly List<LancamentoFinanceiro> _lancamentos = new();

    // Dictionary: busca rápida de um lançamento pelo Id.
    private readonly Dictionary<int, LancamentoFinanceiro> _lancamentosPorId = new();

    // Dictionary com chave composta (ano, mês): um orçamento por mês.
    private readonly Dictionary<(int Ano, int Mes), Orcamento> _orcamentos = new();

    public IReadOnlyCollection<Categoria> Categorias => _categorias;
    public IReadOnlyList<LancamentoFinanceiro> Lancamentos => _lancamentos.AsReadOnly();

    protected IReadOnlyCollection<Orcamento> Orcamentos => _orcamentos.Values;

    public void AdicionarCategoria(Categoria categoria)
    {
        ArgumentNullException.ThrowIfNull(categoria);
        _categorias.Add(categoria);
    }

    public void AdicionarLancamento(LancamentoFinanceiro lancamento)
    {
        ArgumentNullException.ThrowIfNull(lancamento);

        if (!_categorias.Contains(lancamento.Categoria))
            throw new ArgumentException($"A categoria \"{lancamento.Categoria.Nome}\" não está cadastrada.", nameof(lancamento));
        if (!_lancamentosPorId.TryAdd(lancamento.Id, lancamento))
            throw new ArgumentException($"Já existe um lançamento com o Id {lancamento.Id}.", nameof(lancamento));

        _lancamentos.Add(lancamento);
    }

    public LancamentoFinanceiro? BuscarLancamentoPorId(int id) =>
        _lancamentosPorId.TryGetValue(id, out var lancamento) ? lancamento : null;

    public Orcamento ObterOrcamento(int mes, int ano)
    {
        if (!_orcamentos.TryGetValue((ano, mes), out var orcamento))
        {
            orcamento = new Orcamento(mes, ano);
            _orcamentos[(ano, mes)] = orcamento;
        }

        return orcamento;
    }

    /// <summary>Em memória não há o que gravar; as subclasses sobrescrevem para persistir.</summary>
    public virtual void Salvar()
    {
    }
}
