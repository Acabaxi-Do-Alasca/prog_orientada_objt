using ControleFinanceiro.Model;
using ControleFinanceiro.Persistence;

namespace ControleFinanceiro.Service;

/// <summary>
/// Regras de negócio do controle financeiro. A tela só conversa com esta classe; os dados ficam no
/// <see cref="IRepositorioFinanceiro"/> recebido no construtor, que pode ser em memória ou em arquivo texto.
/// Toda operação que altera dados termina chamando <see cref="IRepositorioFinanceiro.Salvar"/>.
/// </summary>
/// <remarks>
/// Os métodos que gravam podem lançar <see cref="ArquivoDeDadosException"/> se o repositório não conseguir salvar.
/// Dados inválidos ou operações proibidas pelas regras lançam <see cref="ArgumentException"/>.
/// </remarks>
public class ServicoFinanceiro
{
    public const int DiasAlertaMinimo = 0;
    public const int DiasAlertaMaximo = 60;

    private readonly IRepositorioFinanceiro _repositorio;

    public ServicoFinanceiro(IRepositorioFinanceiro repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public IReadOnlyCollection<Categoria> Categorias => _repositorio.Categorias;

    /// <summary>Com quantos dias de antecedência o sistema avisa sobre um vencimento.</summary>
    public int DiasAlerta => _repositorio.DiasAlerta;

    // ---------- categorias ----------

    /// <summary>No primeiro uso, sem nenhuma categoria, cadastra duas categorias de exemplo do negócio da Célia.</summary>
    public void CriarCategoriasIniciaisSeNecessario()
    {
        if (_repositorio.Categorias.Count > 0)
            return;

        _repositorio.AdicionarCategoria(new Categoria("Venda de encomendas", TipoCategoria.Receita));
        _repositorio.AdicionarCategoria(new Categoria("Ingredientes", TipoCategoria.Despesa));
        _repositorio.Salvar();
    }

    public Categoria CadastrarCategoria(string nome, TipoCategoria tipo)
    {
        var nomeLimpo = ExigirNomeLivre(nome, ignorar: null);
        var categoria = new Categoria(nomeLimpo, tipo);
        _repositorio.AdicionarCategoria(categoria);
        _repositorio.Salvar();
        return categoria;
    }

    /// <summary>Renomeia a categoria ou troca o tipo. O tipo só muda se a categoria ainda não tiver lançamentos.</summary>
    public void EditarCategoria(Categoria categoria, string novoNome, TipoCategoria novoTipo)
    {
        ExigirCategoriaCadastrada(categoria);
        var nomeLimpo = ExigirNomeLivre(novoNome, ignorar: categoria);

        if (novoTipo != categoria.Tipo && ContarLancamentos(categoria) > 0)
            throw new ArgumentException(
                $"A categoria \"{categoria.Nome}\" já tem lançamentos de {categoria.Tipo.ToString().ToLowerInvariant()}; não é possível trocar o tipo.",
                nameof(novoTipo));

        categoria.Nome = nomeLimpo;
        categoria.Tipo = novoTipo;
        _repositorio.Salvar();
    }

    /// <summary>Exclui a categoria e os valores planejados dela. Não exclui categoria que tenha lançamentos.</summary>
    public void ExcluirCategoria(Categoria categoria)
    {
        ExigirCategoriaCadastrada(categoria);

        var lancamentos = ContarLancamentos(categoria);
        if (lancamentos > 0)
            throw new ArgumentException(
                $"A categoria \"{categoria.Nome}\" tem {lancamentos} lançamento(s). Exclua ou mude esses lançamentos de categoria antes.",
                nameof(categoria));

        _repositorio.RemoverCategoria(categoria);
        _repositorio.Salvar();
    }

    public int ContarLancamentos(Categoria categoria) =>
        _repositorio.Lancamentos.Count(lancamento => lancamento.Categoria.Equals(categoria));

    private string ExigirNomeLivre(string nome, Categoria? ignorar)
    {
        var nomeLimpo = nome?.Trim() ?? string.Empty;

        if (_repositorio.Categorias.Any(c => !c.Equals(ignorar) && string.Equals(c.Nome, nomeLimpo, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Já existe uma categoria chamada \"{nomeLimpo}\".", nameof(nome));

        return string.IsNullOrWhiteSpace(nomeLimpo)
            ? throw new ArgumentException("Nome da categoria é obrigatório.", nameof(nome))
            : nomeLimpo;
    }

    private void ExigirCategoriaCadastrada(Categoria categoria)
    {
        ArgumentNullException.ThrowIfNull(categoria);

        if (!_repositorio.Categorias.Contains(categoria))
            throw new ArgumentException($"A categoria \"{categoria.Nome}\" não está cadastrada.", nameof(categoria));
    }

    private static void ExigirCategoriaDoTipo(Categoria categoria, TipoCategoria tipo)
    {
        ArgumentNullException.ThrowIfNull(categoria);

        if (categoria.Tipo != tipo)
            throw new ArgumentException(
                $"A categoria \"{categoria.Nome}\" é de {categoria.Tipo.ToString().ToLowerInvariant()}, não de {tipo.ToString().ToLowerInvariant()}.",
                nameof(categoria));
    }

    // ---------- lançamentos ----------

    public Receita RegistrarReceita(DateTime vencimento, decimal valor, string descricao, Categoria categoria, string? origem, bool jaRecebida)
    {
        ExigirCategoriaDoTipo(categoria, TipoCategoria.Receita);
        return Registrar(new Receita(vencimento.Date, valor, Limpar(descricao), categoria, OrigemOuPadrao(origem)), jaRecebida);
    }

    public Despesa RegistrarDespesa(DateTime vencimento, decimal valor, string descricao, Categoria categoria, bool jaPaga)
    {
        ExigirCategoriaDoTipo(categoria, TipoCategoria.Despesa);
        return Registrar(new Despesa(vencimento.Date, valor, Limpar(descricao), categoria), jaPaga);
    }

    private T Registrar<T>(T lancamento, bool jaPago) where T : LancamentoFinanceiro
    {
        if (jaPago)
            lancamento.MarcarComoPago();

        _repositorio.AdicionarLancamento(lancamento);
        _repositorio.Salvar();
        return lancamento;
    }

    /// <summary>
    /// Altera um lançamento existente. Receita continua receita e despesa continua despesa: para trocar o tipo,
    /// exclua e cadastre de novo. A origem só é usada em receitas.
    /// </summary>
    public void EditarLancamento(int idLancamento, DateTime vencimento, decimal valor, string descricao, Categoria categoria, string? origem, bool pago)
    {
        var lancamento = BuscarLancamento(idLancamento);
        ExigirCategoriaCadastrada(categoria);
        ExigirCategoriaDoTipo(categoria, lancamento is Receita ? TipoCategoria.Receita : TipoCategoria.Despesa);

        lancamento.Atualizar(vencimento.Date, valor, Limpar(descricao), categoria);

        if (lancamento is Receita receita)
            receita.OrigemRecebimento = OrigemOuPadrao(origem);

        if (pago)
            lancamento.MarcarComoPago();
        else
            lancamento.MarcarComoPendente();

        _repositorio.Salvar();
    }

    public void ExcluirLancamento(int idLancamento)
    {
        _repositorio.RemoverLancamento(BuscarLancamento(idLancamento));
        _repositorio.Salvar();
    }

    public void MarcarComoPago(int idLancamento)
    {
        BuscarLancamento(idLancamento).MarcarComoPago();
        _repositorio.Salvar();
    }

    public LancamentoFinanceiro BuscarLancamento(int idLancamento) =>
        _repositorio.BuscarLancamentoPorId(idLancamento)
            ?? throw new ArgumentException($"Lançamento {idLancamento} não encontrado.", nameof(idLancamento));

    private static string Limpar(string? texto) => texto?.Trim() ?? string.Empty;

    private static string OrigemOuPadrao(string? origem) =>
        string.IsNullOrWhiteSpace(origem) ? "Não informado" : origem.Trim();

    // ---------- filtros, totais e saldo ----------

    /// <summary>Filtra os lançamentos por tipo usando iteração tradicional (foreach + condição).</summary>
    public List<LancamentoFinanceiro> FiltrarPorTipo(TipoLancamentoFiltro filtro)
    {
        var resultado = new List<LancamentoFinanceiro>();

        foreach (var lancamento in _repositorio.Lancamentos)
        {
            var atendeAoFiltro = filtro switch
            {
                TipoLancamentoFiltro.Receitas => lancamento is Receita,
                TipoLancamentoFiltro.Despesas => lancamento is Despesa,
                _ => true
            };

            if (atendeAoFiltro)
                resultado.Add(lancamento);
        }

        return resultado;
    }

    /// <summary>Soma o valor dos lançamentos filtrados usando iteração tradicional (for).</summary>
    public decimal CalcularTotal(TipoLancamentoFiltro filtro)
    {
        var filtrados = FiltrarPorTipo(filtro);
        decimal total = 0m;

        for (var i = 0; i < filtrados.Count; i++)
        {
            total += filtrados[i].Valor;
        }

        return total;
    }

    /// <summary>
    /// Saldo previsto: receitas menos despesas de todos os lançamentos, pagos ou pendentes,
    /// via polimorfismo de <see cref="LancamentoFinanceiro.ImpactoNoSaldo"/>.
    /// </summary>
    public decimal CalcularSaldoPrevisto() => _repositorio.Lancamentos.Sum(lancamento => lancamento.ImpactoNoSaldo);

    /// <summary>Saldo realizado: só o que já foi pago ou recebido, ou seja, o dinheiro que de fato passou pelo caixa.</summary>
    public decimal CalcularSaldoRealizado() => _repositorio.Lancamentos
        .Where(lancamento => lancamento.Status == StatusLancamento.Pago)
        .Sum(lancamento => lancamento.ImpactoNoSaldo);

    // ---------- orçamento ----------

    /// <summary>Define (ou atualiza) quanto se planeja gastar ou receber numa categoria no mês.</summary>
    public ItemOrcamento DefinirValorPlanejado(int mes, int ano, Categoria categoria, decimal valorPlanejado)
    {
        ExigirCategoriaCadastrada(categoria);

        var item = _repositorio.ObterOrcamento(mes, ano).AdicionarItem(categoria, valorPlanejado);
        _repositorio.Salvar();
        return item;
    }

    /// <summary>Tira a categoria do orçamento do mês. Retorna falso se ela não tinha valor planejado.</summary>
    public bool RemoverValorPlanejado(int mes, int ano, Categoria categoria)
    {
        ArgumentNullException.ThrowIfNull(categoria);

        var removeu = _repositorio.ObterOrcamento(mes, ano).RemoverItem(categoria);
        if (removeu)
            _repositorio.Salvar();

        return removeu;
    }

    public IReadOnlyList<ComparacaoOrcamento> CompararOrcamento(int mes, int ano) =>
        _repositorio.ObterOrcamento(mes, ano).CompararCom(_repositorio.Lancamentos);

    // ---------- vencimentos ----------

    public void DefinirDiasAlerta(int dias)
    {
        if (dias is < DiasAlertaMinimo or > DiasAlertaMaximo)
            throw new ArgumentOutOfRangeException(nameof(dias), $"O prazo do alerta deve ficar entre {DiasAlertaMinimo} e {DiasAlertaMaximo} dias.");

        _repositorio.DiasAlerta = dias;
        _repositorio.Salvar();
    }

    /// <summary>Alertas usando o prazo configurado em <see cref="DiasAlerta"/>.</summary>
    public List<LancamentoFinanceiro> ListarAlertasDeVencimento(DateTime referencia) =>
        ListarAlertasDeVencimento(referencia, DiasAlerta);

    /// <summary>
    /// Lançamentos não pagos que já venceram ou vencem nos próximos <paramref name="diasAlerta"/> dias,
    /// do vencimento mais antigo para o mais recente.
    /// </summary>
    public List<LancamentoFinanceiro> ListarAlertasDeVencimento(DateTime referencia, int diasAlerta)
    {
        var alertas = new List<LancamentoFinanceiro>();

        foreach (var lancamento in _repositorio.Lancamentos)
        {
            if (lancamento.EstaVencido(referencia) || lancamento.VencimentoProximo(referencia, diasAlerta))
                alertas.Add(lancamento);
        }

        alertas.Sort((a, b) => a.Data.CompareTo(b.Data));
        return alertas;
    }
}
