using ControleFinanceiro.Model;

namespace ControleFinanceiro.Presentation.WinForms;

/// <summary>
/// Projeção de <see cref="LancamentoFinanceiro"/> para exibição no DataGridView —
/// mantém a tela desacoplada das entidades de domínio.
/// </summary>
public class LancamentoGridItem
{
    public int Id { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public DateTime Data { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    public static LancamentoGridItem DeLancamento(LancamentoFinanceiro lancamento) => new()
    {
        Id = lancamento.Id,
        Tipo = lancamento is Receita ? "Receita" : "Despesa",
        Data = lancamento.Data,
        Descricao = lancamento.Descricao,
        Valor = lancamento.Valor,
        Categoria = lancamento.Categoria.Nome,
        Status = lancamento.Status.ToString()
    };
}
