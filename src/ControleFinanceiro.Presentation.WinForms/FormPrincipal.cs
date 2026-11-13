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
    private readonly ListBox _lstCategorias = new() { FormattingEnabled = true };

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
    private readonly Label _lblResumo = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleRight };
    private readonly Label _lblSaldo = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
    private readonly DataGridView _dgvLancamentos = NovaGrade();

    // Orçamento
    private readonly DateTimePicker _dtpMesOrcamento = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "MM/yyyy", ShowUpDown = true };
    private readonly ComboBox _cmbCategoriaOrcamento = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtValorPlanejado = new();
    private readonly Button _btnDefinirPlanejado = new() { Text = "Definir valor planejado" };
    private readonly Label _lblMensagemOrcamento = new() { ForeColor = Color.Firebrick, AutoSize = false };
    private readonly Label _lblResumoOrcamento = new() { AutoSize = false, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
    private readonly DataGridView _dgvOrcamento = NovaGrade();

    public FormPrincipal()
    {
        Text = "Controle Financeiro — Célia Doces";
        ClientSize = new Size(940, 660);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(960, 700);

        var abaLancamentos = new TabPage("Lançamentos") { UseVisualStyleBackColor = true };
        abaLancamentos.Controls.AddRange([MontarGrupoCategoria(), MontarGrupoLancamento(), MontarGrupoListagem()]);

        var abaOrcamento = new TabPage("Orçamento") { UseVisualStyleBackColor = true };
        abaOrcamento.Controls.AddRange([MontarGrupoDefinicaoOrcamento(), MontarGrupoComparacaoOrcamento()]);

        var abas = new TabControl { Dock = DockStyle.Fill };
        abas.TabPages.AddRange([abaLancamentos, abaOrcamento]);
        Controls.Add(abas);

        CarregarDadosIniciais();
        AtualizarGrid(TipoLancamentoFiltro.Todos);
        AtualizarOrcamento();
    }

    private static DataGridView NovaGrade() => new()
    {
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

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
        _lstCategorias.Format += (_, e) =>
        {
            if (e.ListItem is Categoria categoria)
                e.Value = $"{categoria.Nome} ({categoria.Tipo})";
        };

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
        _cmbTipoLancamento.SelectedIndexChanged += (_, _) => RecarregarCategoriasDoLancamento();

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

        _lblResumo.Location = new Point(370, 22);
        _lblResumo.Size = new Size(330, 25);

        _lblSaldo.Location = new Point(706, 22);
        _lblSaldo.Size = new Size(180, 25);

        _dgvLancamentos.Location = new Point(12, 55);
        _dgvLancamentos.Size = new Size(874, 230);

        grupo.Controls.AddRange([lblFiltro, _cmbFiltro, _btnFiltrar, _lblResumo, _lblSaldo, _dgvLancamentos]);
        return grupo;
    }

    private GroupBox MontarGrupoDefinicaoOrcamento()
    {
        var grupo = new GroupBox { Text = "Planejar orçamento do mês", Location = new Point(12, 12), Size = new Size(900, 90) };

        var lblMes = new Label { Text = "Mês:", Location = new Point(12, 28), AutoSize = true };
        _dtpMesOrcamento.Location = new Point(50, 25);
        _dtpMesOrcamento.Size = new Size(100, 23);
        _dtpMesOrcamento.ValueChanged += (_, _) => AtualizarOrcamento();

        var lblCategoria = new Label { Text = "Categoria:", Location = new Point(170, 28), AutoSize = true };
        _cmbCategoriaOrcamento.Location = new Point(240, 25);
        _cmbCategoriaOrcamento.Size = new Size(220, 23);

        var lblValor = new Label { Text = "Planejado (R$):", Location = new Point(475, 28), AutoSize = true };
        _txtValorPlanejado.Location = new Point(575, 25);
        _txtValorPlanejado.Size = new Size(100, 23);

        _btnDefinirPlanejado.Location = new Point(690, 23);
        _btnDefinirPlanejado.Size = new Size(190, 26);
        _btnDefinirPlanejado.Click += BtnDefinirPlanejado_Click;

        _lblMensagemOrcamento.Location = new Point(12, 58);
        _lblMensagemOrcamento.Size = new Size(870, 20);

        grupo.Controls.AddRange([
            lblMes, _dtpMesOrcamento, lblCategoria, _cmbCategoriaOrcamento,
            lblValor, _txtValorPlanejado, _btnDefinirPlanejado, _lblMensagemOrcamento
        ]);
        return grupo;
    }

    private GroupBox MontarGrupoComparacaoOrcamento()
    {
        var grupo = new GroupBox { Text = "Planejado x Realizado", Location = new Point(12, 110), Size = new Size(900, 480) };

        _lblResumoOrcamento.Location = new Point(12, 25);
        _lblResumoOrcamento.Size = new Size(874, 22);

        _dgvOrcamento.Location = new Point(12, 52);
        _dgvOrcamento.Size = new Size(874, 415);
        _dgvOrcamento.CellFormatting += DgvOrcamento_CellFormatting;

        grupo.Controls.AddRange([_lblResumoOrcamento, _dgvOrcamento]);
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
        RecarregarCategoriasDoLancamento();
        PreencherComboDeCategorias(_cmbCategoriaOrcamento, _repositorio.Categorias);
    }

    private void RecarregarCategoriasDoLancamento()
    {
        var tipo = (TipoCategoria)_cmbTipoLancamento.SelectedItem!;
        PreencherComboDeCategorias(_cmbCategoriaLancamento, _repositorio.Categorias.Where(categoria => categoria.Tipo == tipo));
    }

    private static void PreencherComboDeCategorias(ComboBox combo, IEnumerable<Categoria> categorias)
    {
        var selecionada = combo.SelectedItem;

        combo.Items.Clear();
        foreach (var categoria in categorias.OrderBy(categoria => categoria.Nome))
            combo.Items.Add(categoria);

        if (selecionada is not null && combo.Items.Contains(selecionada))
            combo.SelectedItem = selecionada;
        else if (combo.Items.Count > 0)
            combo.SelectedIndex = 0;
    }

    private static bool TentarLerValor(string texto, out decimal valor) =>
        decimal.TryParse(texto.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out valor);

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
        var tipo = (TipoCategoria)_cmbTipoLancamento.SelectedItem!;

        if (_cmbCategoriaLancamento.SelectedItem is not Categoria categoria)
        {
            _lblMensagem.Text = $"Cadastre uma categoria do tipo {tipo} antes de adicionar o lançamento.";
            return;
        }

        if (!TentarLerValor(_txtValor.Text, out var valor))
        {
            _lblMensagem.Text = "Informe um valor numérico válido.";
            return;
        }

        var descricao = _txtDescricao.Text.Trim();

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
            AtualizarOrcamento();
        }
        catch (ArgumentException ex)
        {
            _lblMensagem.Text = ex.Message;
        }
    }

    private void BtnFiltrar_Click(object? sender, EventArgs e) =>
        AtualizarGrid((TipoLancamentoFiltro)_cmbFiltro.SelectedItem!);

    private void BtnDefinirPlanejado_Click(object? sender, EventArgs e)
    {
        _lblMensagemOrcamento.Text = string.Empty;

        if (_cmbCategoriaOrcamento.SelectedItem is not Categoria categoria)
        {
            _lblMensagemOrcamento.Text = "Cadastre uma categoria antes de planejar o orçamento.";
            return;
        }

        if (!TentarLerValor(_txtValorPlanejado.Text, out var valorPlanejado))
        {
            _lblMensagemOrcamento.Text = "Informe um valor planejado numérico válido.";
            return;
        }

        try
        {
            var mes = _dtpMesOrcamento.Value;
            _repositorio.ObterOrcamento(mes.Month, mes.Year).AdicionarItem(categoria, valorPlanejado);
            _txtValorPlanejado.Clear();
            AtualizarOrcamento();
        }
        catch (ArgumentException ex)
        {
            _lblMensagemOrcamento.Text = ex.Message;
        }
    }

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

        AtualizarResumoDoCaixa();
    }

    private void AtualizarResumoDoCaixa()
    {
        var receitas = _repositorio.CalcularTotal(TipoLancamentoFiltro.Receitas);
        var despesas = _repositorio.CalcularTotal(TipoLancamentoFiltro.Despesas);
        var saldo = _repositorio.CalcularSaldo();

        _lblResumo.Text = $"Receitas: {receitas:C}    Despesas: {despesas:C}";
        _lblSaldo.Text = $"Saldo: {saldo:C}";
        _lblSaldo.ForeColor = saldo < 0 ? Color.Firebrick : Color.DarkGreen;
    }

    private void AtualizarOrcamento()
    {
        var mes = _dtpMesOrcamento.Value;
        var itens = _repositorio.CompararOrcamento(mes.Month, mes.Year)
            .OrderBy(comparacao => comparacao.Categoria.Tipo)
            .ThenBy(comparacao => comparacao.Categoria.Nome)
            .Select(OrcamentoGridItem.DeComparacao)
            .ToList();

        _dgvOrcamento.DataSource = null;
        _dgvOrcamento.DataSource = itens;

        var foraDoPlanejado = itens.Count(item => item.ForaDoPlanejado);
        _lblResumoOrcamento.Text = itens.Count == 0
            ? "Nenhum valor planejado nem lançamento neste mês."
            : foraDoPlanejado == 0
                ? "Todas as categorias estão dentro do planejado."
                : $"{foraDoPlanejado} categoria(s) fora do planejado.";
        _lblResumoOrcamento.ForeColor = foraDoPlanejado == 0 ? Color.DarkGreen : Color.Firebrick;
    }

    private void DgvOrcamento_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.CellStyle is null || _dgvOrcamento.Rows[e.RowIndex].DataBoundItem is not OrcamentoGridItem item)
            return;

        if (item.ForaDoPlanejado)
            e.CellStyle.BackColor = Color.MistyRose;

        if (e.Value is decimal valor)
        {
            e.Value = valor.ToString("C");
            e.FormattingApplied = true;
        }
    }
}
