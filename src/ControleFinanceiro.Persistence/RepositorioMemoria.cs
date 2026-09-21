using ControleFinanceiro.Model;

namespace ControleFinanceiro.Persistence;

/// <summary>
/// Armazena as entidades em coleções genéricas, em memória. Etapa intermediária antes
/// da persistência em arquivo texto (planejada para uma sprint futura).
/// </summary>
public class RepositorioMemoria
{
    // HashSet: Categoria já implementa Equals/GetHashCode por Id, então o HashSet
    // impede que a mesma categoria seja adicionada duas vezes.
    private readonly HashSet<Categoria> _categorias = new();

    // List: mantém a ordem de cadastro dos lançamentos.
    private readonly List<LancamentoFinanceiro> _lancamentos = new();

    // Dictionary: busca rápida de um lançamento pelo Id.
    private readonly Dictionary<int, LancamentoFinanceiro> _lancamentosPorId = new();

    public IReadOnlyCollection<Categoria> Categorias => _categorias;
    public IReadOnlyList<LancamentoFinanceiro> Lancamentos => _lancamentos.AsReadOnly();

    public Categoria AdicionarCategoria(string nome, TipoCategoria tipo)
    {
        var categoria = new Categoria(nome, tipo);
        _categorias.Add(categoria);
        return categoria;
    }

    public void AdicionarLancamento(LancamentoFinanceiro lancamento)
    {
        ArgumentNullException.ThrowIfNull(lancamento);

        _lancamentos.Add(lancamento);
        _lancamentosPorId[lancamento.Id] = lancamento;
    }

    public LancamentoFinanceiro? BuscarPorId(int id) =>
        _lancamentosPorId.TryGetValue(id, out var lancamento) ? lancamento : null;

    /// <summary>Filtra os lançamentos por tipo usando iteração tradicional (foreach + condição).</summary>
    public List<LancamentoFinanceiro> FiltrarPorTipo(TipoLancamentoFiltro filtro)
    {
        var resultado = new List<LancamentoFinanceiro>();

        foreach (var lancamento in _lancamentos)
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
}
