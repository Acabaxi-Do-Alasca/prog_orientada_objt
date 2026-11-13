namespace ControleFinanceiro.Model;

/// <summary>Planejado x realizado de uma categoria em um mês.</summary>
public record ComparacaoOrcamento(Categoria Categoria, decimal Planejado, decimal Realizado)
{
    public decimal Desvio => Realizado - Planejado;

    public decimal? PercentualRealizado => Planejado == 0 ? null : Math.Round(Realizado / Planejado * 100, 1);

    /// <summary>Despesa acima do planejado ou receita abaixo da meta.</summary>
    public bool ForaDoPlanejado => Categoria.Tipo == TipoCategoria.Despesa
        ? Realizado > Planejado
        : Realizado < Planejado;
}
