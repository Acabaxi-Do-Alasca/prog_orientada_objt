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

    public ItemOrcamento AdicionarItem(Categoria categoria, decimal valorPlanejado)
    {
        var item = new ItemOrcamento(categoria, valorPlanejado);
        _itens.Add(item);
        return item;
    }

    public decimal CalcularTotalPlanejado() => _itens.Sum(item => item.ValorPlanejado);
}
