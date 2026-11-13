using System.ComponentModel;
using ControleFinanceiro.Model;

namespace ControleFinanceiro.Presentation.WinForms;

/// <summary>Projeção de <see cref="ComparacaoOrcamento"/> para exibição no DataGridView.</summary>
public class OrcamentoGridItem
{
    public string Categoria { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public decimal Planejado { get; init; }
    public decimal Realizado { get; init; }
    public decimal Desvio { get; init; }

    [DisplayName("% Realizado")]
    public string PercentualRealizado { get; init; } = string.Empty;

    [DisplayName("Situação")]
    public string Situacao { get; init; } = string.Empty;

    // Usados só pela tela (cor da linha e "Remover valor planejado"); Browsable(false) evita que virem colunas.
    [Browsable(false)]
    public bool ForaDoPlanejado { get; init; }

    [Browsable(false)]
    public int CategoriaId { get; init; }

    public static OrcamentoGridItem DeComparacao(ComparacaoOrcamento comparacao) => new()
    {
        CategoriaId = comparacao.Categoria.Id,
        Categoria = comparacao.Categoria.Nome,
        Tipo = comparacao.Categoria.Tipo.ToString(),
        Planejado = comparacao.Planejado,
        Realizado = comparacao.Realizado,
        Desvio = comparacao.Desvio,
        PercentualRealizado = comparacao.PercentualRealizado is { } percentual ? $"{percentual:0.#}%" : "—",
        Situacao = DescreverSituacao(comparacao),
        ForaDoPlanejado = comparacao.ForaDoPlanejado
    };

    private static string DescreverSituacao(ComparacaoOrcamento comparacao) =>
        (comparacao.Categoria.Tipo, comparacao.ForaDoPlanejado, comparacao.Planejado) switch
        {
            (TipoCategoria.Despesa, true, 0m) => "Gasto não planejado",
            (TipoCategoria.Despesa, true, _) => "Acima do orçamento",
            (TipoCategoria.Despesa, false, _) => "Dentro do orçamento",
            (TipoCategoria.Receita, true, _) => "Abaixo da meta",
            (TipoCategoria.Receita, false, 0m) => "Sem meta definida",
            _ => "Meta atingida"
        };
}
