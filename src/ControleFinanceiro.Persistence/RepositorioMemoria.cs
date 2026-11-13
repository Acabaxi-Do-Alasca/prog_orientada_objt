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

    // Dictionary com chave composta (ano, mês): um orçamento por mês.
    private readonly Dictionary<(int Ano, int Mes), Orcamento> _orcamentos = new();

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

    /// <summary>Saldo do caixa: receitas menos despesas, via polimorfismo de <see cref="LancamentoFinanceiro.ImpactoNoSaldo"/>.</summary>
    public decimal CalcularSaldo() => _lancamentos.Sum(lancamento => lancamento.ImpactoNoSaldo);

    /// <summary>Retorna o orçamento do mês, criando um vazio se ainda não existir.</summary>
    public Orcamento ObterOrcamento(int mes, int ano)
    {
        if (!_orcamentos.TryGetValue((ano, mes), out var orcamento))
        {
            orcamento = new Orcamento(mes, ano);
            _orcamentos[(ano, mes)] = orcamento;
        }

        return orcamento;
    }

    public IReadOnlyList<ComparacaoOrcamento> CompararOrcamento(int mes, int ano) =>
        ObterOrcamento(mes, ano).CompararCom(_lancamentos);
}
