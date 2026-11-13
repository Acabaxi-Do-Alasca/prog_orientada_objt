namespace ControleFinanceiro.Model;

public class Receita : LancamentoFinanceiro
{
    public string OrigemRecebimento { get; set; }

    public Receita(DateTime data, decimal valor, string descricao, Categoria categoria, string origemRecebimento)
        : base(data, valor, descricao, categoria)
    {
        OrigemRecebimento = ValidarOrigem(origemRecebimento);
    }

    /// <summary>Recria uma receita já existente (lida do arquivo) mantendo o Id original.</summary>
    internal Receita(int id, DateTime data, decimal valor, string descricao, Categoria categoria, string origemRecebimento)
        : base(id, data, valor, descricao, categoria)
    {
        OrigemRecebimento = ValidarOrigem(origemRecebimento);
    }

    private static string ValidarOrigem(string origemRecebimento) =>
        string.IsNullOrWhiteSpace(origemRecebimento)
            ? throw new ArgumentException("Origem do recebimento é obrigatória.", nameof(origemRecebimento))
            : origemRecebimento;

    public override decimal ImpactoNoSaldo => Valor;
}
