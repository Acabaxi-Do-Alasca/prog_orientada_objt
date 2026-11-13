namespace ControleFinanceiro.Model;

public enum StatusLancamento
{
    Pendente,
    Pago,
    Vencido
}

/// <summary>
/// Base de todo lançamento financeiro (receita ou despesa). A identidade de um
/// lançamento é o seu <see cref="Id"/>: dois lançamentos com os mesmos dados
/// (mesma data, valor, descrição e categoria) ainda são entidades distintas.
/// </summary>
public abstract class LancamentoFinanceiro : IEquatable<LancamentoFinanceiro>
{
    private static readonly GeradorDeId Ids = new();

    public int Id { get; }
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public string Descricao { get; set; }
    public StatusLancamento Status { get; set; }

    // Agregação: a Categoria existe independentemente do lançamento.
    public Categoria Categoria { get; }

    protected LancamentoFinanceiro(DateTime data, decimal valor, string descricao, Categoria categoria)
        : this(Ids.Proximo(), data, valor, descricao, categoria)
    {
    }

    /// <summary>Recria um lançamento já existente (lido do arquivo) mantendo o Id original.</summary>
    private protected LancamentoFinanceiro(int id, DateTime data, decimal valor, string descricao, Categoria categoria)
    {
        if (valor <= 0)
            throw new ArgumentOutOfRangeException(nameof(valor), "O valor deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição é obrigatória.", nameof(descricao));

        Ids.Registrar(id);
        Id = id;
        Data = data;
        Valor = valor;
        Descricao = descricao;
        Categoria = categoria ?? throw new ArgumentNullException(nameof(categoria));
        Status = StatusLancamento.Pendente;
    }

    /// <summary>Quanto este lançamento altera o saldo do caixa: positivo para receita, negativo para despesa.</summary>
    public abstract decimal ImpactoNoSaldo { get; }

    public bool VencimentoProximo(DateTime referencia, int diasAlerta = 5)
    {
        if (Status == StatusLancamento.Pago)
            return false;

        var diasParaVencer = (Data.Date - referencia.Date).TotalDays;
        return diasParaVencer >= 0 && diasParaVencer <= diasAlerta;
    }

    public void MarcarComoPago() => Status = StatusLancamento.Pago;

    public bool EstaVencido(DateTime referencia) =>
        Status != StatusLancamento.Pago && Data.Date < referencia.Date;

    /// <summary>Situação na data de referência: um lançamento pendente passa a vencido depois da data.</summary>
    public StatusLancamento SituacaoEm(DateTime referencia) =>
        Status == StatusLancamento.Pago ? StatusLancamento.Pago
        : EstaVencido(referencia) ? StatusLancamento.Vencido
        : StatusLancamento.Pendente;

    public bool Equals(LancamentoFinanceiro? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => Equals(obj as LancamentoFinanceiro);
    public override int GetHashCode() => Id.GetHashCode();
}
