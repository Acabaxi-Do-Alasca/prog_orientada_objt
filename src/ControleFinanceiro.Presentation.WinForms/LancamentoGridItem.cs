using System.ComponentModel;
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
    public DateTime Vencimento { get; init; }

    [DisplayName("Descrição")]
    public string Descricao { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    // Usados só para colorir a linha; Browsable(false) evita que virem colunas.
    [Browsable(false)]
    public bool Vencido { get; init; }

    [Browsable(false)]
    public bool VenceEmBreve { get; init; }

    public static LancamentoGridItem DeLancamento(LancamentoFinanceiro lancamento, DateTime referencia, int diasAlerta)
    {
        var situacao = lancamento.SituacaoEm(referencia);

        return new()
        {
            Id = lancamento.Id,
            Tipo = lancamento is Receita ? "Receita" : "Despesa",
            Vencimento = lancamento.Data,
            Descricao = lancamento.Descricao,
            Valor = lancamento.Valor,
            Categoria = lancamento.Categoria.Nome,
            Status = situacao.ToString(),
            Vencido = situacao == StatusLancamento.Vencido,
            VenceEmBreve = lancamento.VencimentoProximo(referencia, diasAlerta)
        };
    }
}
