namespace ControleFinanceiro.Model;

public enum TipoCategoria
{
    Receita,
    Despesa
}

/// <summary>
/// Categoria de um lançamento (ex.: "Venda de encomenda", "Ingredientes", "Aluguel").
/// Existe de forma independente e pode ser referenciada por vários lançamentos e itens
/// de orçamento — associação por agregação, não composição.
/// </summary>
public class Categoria : IEquatable<Categoria>
{
    private static readonly GeradorDeId Ids = new();

    public int Id { get; }
    public string Nome { get; set; }
    public TipoCategoria Tipo { get; set; }

    public Categoria(string nome, TipoCategoria tipo)
        : this(Ids.Proximo(), nome, tipo)
    {
    }

    /// <summary>Recria uma categoria já existente (lida do arquivo) mantendo o Id original.</summary>
    internal Categoria(int id, string nome, TipoCategoria tipo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da categoria é obrigatório.", nameof(nome));

        Ids.Registrar(id);
        Id = id;
        Nome = nome;
        Tipo = tipo;
    }

    public bool Equals(Categoria? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => Equals(obj as Categoria);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => Nome;
}
