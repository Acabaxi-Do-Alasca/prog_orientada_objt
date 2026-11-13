namespace ControleFinanceiro.Model;

/// <summary>
/// Orçamento planejado para um mês/ano, composto por itens (<see cref="ItemOrcamento"/>)
/// que só existem enquanto pertencerem a este orçamento — composição.
/// </summary>
public class Orcamento
{
    private static int _proximoId = 1;
    private readonly List<ItemOrcamento> _itens = new();

    public int Id { get; }
    public int Mes { get; }
    public int Ano { get; }
    public IReadOnlyList<ItemOrcamento> Itens => _itens.AsReadOnly();

    public Orcamento(int mes, int ano)
    {
        if (mes is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(mes), "Mês deve estar entre 1 e 12.");

        Id = _proximoId++;
        Mes = mes;
        Ano = ano;
    }

    /// <summary>
    /// Adiciona o valor planejado da categoria. Se a categoria já estiver no orçamento,
    /// atualiza o valor em vez de criar um item repetido.
    /// </summary>
    public ItemOrcamento AdicionarItem(Categoria categoria, decimal valorPlanejado)
    {
        var existente = _itens.FirstOrDefault(item => item.Categoria.Equals(categoria));
        if (existente is null)
        {
            var item = new ItemOrcamento(categoria, valorPlanejado);
            _itens.Add(item);
            return item;
        }

        if (valorPlanejado < 0)
            throw new ArgumentOutOfRangeException(nameof(valorPlanejado), "O valor planejado não pode ser negativo.");

        existente.ValorPlanejado = valorPlanejado;
        return existente;
    }

    public decimal CalcularTotalPlanejado() => _itens.Sum(item => item.ValorPlanejado);

    /// <summary>
    /// Compara o planejado com o realizado no mês deste orçamento. Categorias com lançamentos
    /// mas sem valor planejado também entram (planejado = 0), para expor gastos não previstos.
    /// </summary>
    public IReadOnlyList<ComparacaoOrcamento> CompararCom(IEnumerable<LancamentoFinanceiro> lancamentos)
    {
        var lancamentosDoMes = lancamentos
            .Where(lancamento => lancamento.Data.Month == Mes && lancamento.Data.Year == Ano)
            .ToList();

        // Union usa o Equals por Id de Categoria, então nenhuma categoria se repete.
        var categorias = _itens.Select(item => item.Categoria)
            .Union(lancamentosDoMes.Select(lancamento => lancamento.Categoria));

        return categorias
            .Select(categoria => new ComparacaoOrcamento(
                categoria,
                _itens.FirstOrDefault(item => item.Categoria.Equals(categoria))?.ValorPlanejado ?? 0m,
                lancamentosDoMes.Where(lancamento => lancamento.Categoria.Equals(categoria)).Sum(lancamento => lancamento.Valor)))
            .ToList();
    }
}
