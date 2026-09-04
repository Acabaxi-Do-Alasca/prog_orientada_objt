namespace ControleFinanceiro.Model;

public class Receita : LancamentoFinanceiro
{
    public string OrigemRecebimento { get; set; }

    public Receita(DateTime data, decimal valor, string descricao, Categoria categoria, string origemRecebimento)
        : base(data, valor, descricao, categoria)
    {
        if (string.IsNullOrWhiteSpace(origemRecebimento))
            throw new ArgumentException("Origem do recebimento é obrigatória.", nameof(origemRecebimento));

        OrigemRecebimento = origemRecebimento;
    }
}
