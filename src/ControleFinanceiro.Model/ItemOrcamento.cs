namespace ControleFinanceiro.Model;

/// <summary>
/// Valor planejado para uma categoria dentro de um <see cref="Orcamento"/>. Só pode ser
/// criado através de <see cref="Orcamento.AdicionarItem"/> — composição: um item não
/// existe fora do orçamento ao qual pertence.
/// </summary>
public class ItemOrcamento
{
    // Agregação: a Categoria existe independentemente do item de orçamento.
    public Categoria Categoria { get; }
    public decimal ValorPlanejado { get; set; }

    internal ItemOrcamento(Categoria categoria, decimal valorPlanejado)
    {
        if (valorPlanejado < 0)
            throw new ArgumentOutOfRangeException(nameof(valorPlanejado), "O valor planejado não pode ser negativo.");

        Categoria = categoria ?? throw new ArgumentNullException(nameof(categoria));
        ValorPlanejado = valorPlanejado;
    }
}
