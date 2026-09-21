using System.Globalization;
using ControleFinanceiro.Model;
using ControleFinanceiro.Persistence;

namespace ControleFinanceiro.Presentation.WinForms;

public class FormPrincipal : Form
{
    private readonly RepositorioMemoria _repositorio = new();

    // Cadastro de categoria
    private readonly TextBox _txtNomeCategoria = new();
    private readonly ComboBox _cmbTipoCategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button _btnAdicionarCategoria = new() { Text = "Adicionar Categoria" };
    private readonly ListBox _lstCategorias = new();

    // Cadastro de lançamento
    private readonly TextBox _txtDescricao = new();
    private readonly TextBox _txtValor = new();
    private readonly ComboBox _cmbTipoLancamento = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbCategoriaLancamento = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtOrigem = new();
    private readonly Button _btnAdicionarLancamento = new() { Text = "Adicionar Lançamento" };
    private readonly Label _lblMensagem = new() { ForeColor = Color.Firebrick, AutoSize = false };

    // Listagem
    private readonly ComboBox _cmbFiltro = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button _btnFiltrar = new() { Text = "Filtrar" };
    private readonly Label _lblTotal = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
    private readonly DataGridView _dgvLancamentos = new()
    {
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    public FormPrincipal()
    {
        Text = "Controle Financeiro — Célia Doces";
        ClientSize = new Size(940, 620);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(960, 660);

        Controls.Add(MontarGrupoCategoria());
        Controls.Add(MontarGrupoLancamento());
        Controls.Add(MontarGrupoListagem());

        CarregarDadosIniciais();
        AtualizarGrid(TipoLancamentoFiltro.Todos);
    }

    private GroupBox MontarGrupoCategoria()
    {
        var grupo = new GroupBox { Text = "Categoria", Location = new Point(12, 12), Size = new Size(900, 100) };

        var lblNome = new Label { Text = "Nome:", Location = new Point(12, 28), AutoSize = true };
        _txtNomeCategoria.Location = new Point(70, 25);
        _txtNomeCategoria.Size = new Size(200, 23);

        var lblTipo = new Label { Text = "Tipo:", Location = new Point(290, 28), AutoSize = true };
        _cmbTipoCategoria.Location = new Point(330, 25);
        _cmbTipoCategoria.Size = new Size(120, 23);
        _cmbTipoCategoria.Items.AddRange([TipoCategoria.Receita, TipoCategoria.Despesa]);
        _cmbTipoCategoria.SelectedIndex = 0;

        _btnAdicionarCategoria.Location = new Point(470, 23);
        _btnAdicionarCategoria.Size = new Size(160, 26);
        _btnAdicionarCategoria.Click += BtnAdicionarCategoria_Click;

        _lstCategorias.Location = new Point(12, 58);
        _lstCategorias.Size = new Size(870, 32);

        grupo.Controls.AddRange([lblNome, _txtNomeCategoria, lblTipo, _cmbTipoCategoria, _btnAdicionarCategoria, _lstCategorias]);
        return grupo;
    }

    private GroupBox MontarGrupoLancamento()
    {
        var grupo = new GroupBox { Text = "Novo Lançamento", Location = new Point(12, 120), Size = new Size(900, 160) };

        var lblDescricao = new Label { Text = "Descrição:", Location = new Point(12, 28), AutoSize = true };
        _txtDescricao.Location = new Point(100, 25);
        _txtDescricao.Size = new Size(250, 23);

        var lblValor = new Label { Text = "Valor (R$):", Location = new Point(365, 28), AutoSize = true };
        _txtValor.Location = new Point(445, 25);
        _txtValor.Size = new Size(100, 23);

        var lblTipo = new Label { Text = "Tipo:", Location = new Point(560, 28), AutoSize = true };
        _cmbTipoLancamento.Location = new Point(600, 25);
        _cmbTipoLancamento.Size = new Size(120, 23);
        _cmbTipoLancamento.Items.AddRange([TipoCategoria.Receita, TipoCategoria.Despesa]);
        _cmbTipoLancamento.SelectedIndex = 0;

        var lblCategoria = new Label { Text = "Categoria:", Location = new Point(12, 63), AutoSize = true };
        _cmbCategoriaLancamento.Location = new Point(100, 60);
        _cmbCategoriaLancamento.Size = new Size(250, 23);

        var lblOrigem = new Label { Text = "Origem (se Receita):", Location = new Point(365, 63), AutoSize = true };
        _txtOrigem.Location = new Point(520, 60);
        _txtOrigem.Size = new Size(200, 23);

        _btnAdicionarLancamento.Location = new Point(12, 98);
        _btnAdicionarLancamento.Size = new Size(200, 28);
        _btnAdicionarLancamento.Click += BtnAdicionarLancamento_Click;

        _lblMensagem.Location = new Point(225, 102);
        _lblMensagem.Size = new Size(660, 20);

        grupo.Controls.AddRange([
            lblDescricao, _txtDescricao, lblValor, _txtValor, lblTipo, _cmbTipoLancamento,
            lblCategoria, _cmbCategoriaLancamento, lblOrigem, _txtOrigem,
            _btnAdicionarLancamento, _lblMensagem
        ]);
        return grupo;
    }

    private GroupBox MontarGrupoListagem()
    {
        var grupo = new GroupBox { Text = "Lançamentos", Location = new Point(12, 290), Size = new Size(900, 300) };

        var lblFiltro = new Label { Text = "Filtrar por:", Location = new Point(12, 25), AutoSize = true };
        _cmbFiltro.Location = new Point(90, 22);
        _cmbFiltro.Size = new Size(150, 23);
        _cmbFiltro.Items.AddRange([TipoLancamentoFiltro.Todos, TipoLancamentoFiltro.Receitas, TipoLancamentoFiltro.Despesas]);
        _cmbFiltro.SelectedIndex = 0;

        _btnFiltrar.Location = new Point(250, 21);
        _btnFiltrar.Size = new Size(100, 25);
        _btnFiltrar.Click += BtnFiltrar_Click;

        _lblTotal.Location = new Point(620, 22);
        _lblTotal.Size = new Size(266, 25);

        _dgvLancamentos.Location = new Point(12, 55);
        _dgvLancamentos.Size = new Size(874, 230);

        grupo.Controls.AddRange([lblFiltro, _cmbFiltro, _btnFiltrar, _lblTotal, _dgvLancamentos]);
        return grupo;
    }

    private void CarregarDadosIniciais()
    {
        // Categorias de exemplo do negócio da cliente (Célia Doces), para facilitar a demonstração.
        AdicionarCategoriaNaTela(_repositorio.AdicionarCategoria("Venda de encomendas", TipoCategoria.Receita));
        AdicionarCategoriaNaTela(_repositorio.AdicionarCategoria("Ingredientes", TipoCategoria.Despesa));
    }

    private void AdicionarCategoriaNaTela(Categoria categoria)
    {
        _lstCategorias.Items.Add(categoria);
        _cmbCategoriaLancamento.Items.Add(categoria);

        if (_cmbCategoriaLancamento.SelectedIndex < 0)
            _cmbCategoriaLancamento.SelectedIndex = 0;
    }

    private void BtnAdicionarCategoria_Click(object? sender, EventArgs e)
    {
        _lblMensagem.Text = string.Empty;
        var nome = _txtNomeCategoria.Text.Trim();
        var tipo = (TipoCategoria)_cmbTipoCategoria.SelectedItem!;

        try
        {
            var categoria = _repositorio.AdicionarCategoria(nome, tipo);
            AdicionarCategoriaNaTela(categoria);
            _txtNomeCategoria.Clear();
            _txtNomeCategoria.Focus();
        }
        catch (ArgumentException ex)
        {
            _lblMensagem.Text = ex.Message;
        }
    }

    private void BtnAdicionarLancamento_Click(object? sender, EventArgs e)
    {
        _lblMensagem.Text = string.Empty;

        if (_cmbCategoriaLancamento.SelectedItem is not Categoria categoria)
        {
            _lblMensagem.Text = "Cadastre e selecione uma categoria antes de adicionar o lançamento.";
            return;
        }

        var descricao = _txtDescricao.Text.Trim();
        var valorTexto = _txtValor.Text.Trim().Replace(',', '.');

        if (!decimal.TryParse(valorTexto, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor))
        {
            _lblMensagem.Text = "Informe um valor numérico válido.";
            return;
        }

        var tipo = (TipoCategoria)_cmbTipoLancamento.SelectedItem!;

        try
        {
            LancamentoFinanceiro lancamento = tipo == TipoCategoria.Receita
                ? new Receita(DateTime.Today, valor, descricao, categoria,
                    string.IsNullOrWhiteSpace(_txtOrigem.Text) ? "Não informado" : _txtOrigem.Text.Trim())
                : new Despesa(DateTime.Today, valor, descricao, categoria);

            _repositorio.AdicionarLancamento(lancamento);

            _txtDescricao.Clear();
            _txtValor.Clear();
            _txtOrigem.Clear();
            _txtDescricao.Focus();

            AtualizarGrid((TipoLancamentoFiltro)_cmbFiltro.SelectedItem!);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
        {
            _lblMensagem.Text = ex.Message;
        }
    }

    private void BtnFiltrar_Click(object? sender, EventArgs e) =>
        AtualizarGrid((TipoLancamentoFiltro)_cmbFiltro.SelectedItem!);

    private void AtualizarGrid(TipoLancamentoFiltro filtro)
    {
        var lancamentosFiltrados = _repositorio.FiltrarPorTipo(filtro);

        // Iteração tradicional (foreach) para projetar as entidades de domínio em itens de exibição.
        var itensParaExibicao = new List<LancamentoGridItem>();
        foreach (var lancamento in lancamentosFiltrados)
        {
            itensParaExibicao.Add(LancamentoGridItem.DeLancamento(lancamento));
        }

        _dgvLancamentos.DataSource = null;
        _dgvLancamentos.DataSource = itensParaExibicao;

        _lblTotal.Text = $"Total: {_repositorio.CalcularTotal(filtro):C}";
    }
}
