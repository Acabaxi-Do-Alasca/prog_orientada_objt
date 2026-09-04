namespace ControleFinanceiro.Model;

public class Despesa : LancamentoFinanceiro
{
    public bool Recorrente { get; set; }

    public Despesa(DateTime data, decimal valor, string descricao, Categoria categoria, bool recorrente = false)
        : base(data, valor, descricao, categoria)
    {
        Recorrente = recorrente;
    }
}
