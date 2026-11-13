using ControleFinanceiro.Model;

namespace ControleFinanceiro.Persistence;

/// <summary>
/// Onde os dados do sistema ficam guardados. A camada Service depende só desta interface,
/// então trocar memória por arquivo texto (ou, no futuro, por banco de dados) não muda as regras nem a tela.
/// </summary>
public interface IRepositorioFinanceiro
{
    IReadOnlyCollection<Categoria> Categorias { get; }
    IReadOnlyList<LancamentoFinanceiro> Lancamentos { get; }

    /// <summary>Com quantos dias de antecedência o sistema avisa sobre um vencimento.</summary>
    int DiasAlerta { get; set; }

    void AdicionarCategoria(Categoria categoria);

    /// <summary>Remove a categoria e os valores planejados dela. Não remove categoria que tenha lançamentos.</summary>
    void RemoverCategoria(Categoria categoria);

    void AdicionarLancamento(LancamentoFinanceiro lancamento);
    void RemoverLancamento(LancamentoFinanceiro lancamento);
    LancamentoFinanceiro? BuscarLancamentoPorId(int id);

    /// <summary>Retorna o orçamento do mês, criando um vazio se ainda não existir.</summary>
    Orcamento ObterOrcamento(int mes, int ano);

    /// <summary>Grava o estado atual: cadastros, alterações, exclusões, pagamentos e configurações.</summary>
    void Salvar();
}
