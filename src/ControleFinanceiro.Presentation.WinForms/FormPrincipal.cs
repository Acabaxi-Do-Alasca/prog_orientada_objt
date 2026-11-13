using System.Diagnostics;
using ControleFinanceiro.Model;
using ControleFinanceiro.Persistence;
using ControleFinanceiro.Service;

namespace ControleFinanceiro.Presentation.WinForms;

public class FormPrincipal : Form
{
    private readonly ServicoFinanceiro _servico;
    private readonly ToolTip _dicas = new();

    // Cadastro de categoria
    private readonly TextBox _txtNomeCategoria = new();
    private readonly ComboBox _cmbTipoCategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button _btnAdicionarCategoria = new() { Text = "Adicionar Categoria" };
    private readonly Button _btnEditarCategoria = new() { Text = "Editar categoria" };
    private readonly Button _btnExcluirCategoria = new() { Text = "Excluir categoria" };
    private readonly ListBox _lstCategorias = new() { FormattingEnabled = true, MultiColumn = true };

    // Cadastro de lançamento
    private readonly TextBox _txtDescricao = new();
    private readonly TextBox _txtValor = new();
    private readonly ComboBox _cmbTipoLancamento = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbCategoriaLancamento = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtOrigem = new();
    private readonly DateTimePicker _dtpVencimento = new() { Format = DateTimePickerFormat.Short };
    private readonly CheckBox _chkPago = new() { Text = "Já pago / recebido", AutoSize = true };
    private readonly Button _btnAdicionarLancamento = new() { Text = "Adicionar Lançamento" };
    private readonly Label _lblMensagem = new() { ForeColor = Color.Firebrick, AutoSize = false };

    // Listagem
    private readonly ComboBox _cmbFiltro = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button _btnFiltrar = new() { Text = "Filtrar" };
    private readonly Button _btnMarcarPago = new() { Text = "Marcar como pago" };
    private readonly Button _btnEditarLancamento = new() { Text = "Editar" };
    private readonly Button _btnExcluirLancamento = new() { Text = "Excluir" };
    private readonly Label _lblResumo = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
    private readonly Label _lblSaldoRealizado = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
    private readonly Label _lblSaldo = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
    private readonly DataGridView _dgvLancamentos = NovaGradeDeLancamentos();

    // Orçamento
    private readonly DateTimePicker _dtpMesOrcamento = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "MM/yyyy", ShowUpDown = true };
    private readonly ComboBox _cmbCategoriaOrcamento = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtValorPlanejado = new();
    private readonly Button _btnDefinirPlanejado = new() { Text = "Definir valor planejado" };
    private readonly Button _btnRemoverPlanejado = new() { Text = "Remover valor planejado" };
    private readonly Label _lblMensagemOrcamento = new() { ForeColor = Color.Firebrick, AutoSize = false };
    private readonly Label _lblResumoOrcamento = new() { AutoSize = false, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
    private readonly DataGridView _dgvOrcamento = NovaGrade(("Categoria", 160), ("Tipo", 70), ("PercentualRealizado", 80), ("Situacao", 160));

    // Vencimentos
    private readonly Label _lblAviso = new()
    {
        Dock = DockStyle.Top,
        Height = 30,
        TextAlign = ContentAlignment.MiddleLeft,
        Padding = new Padding(10, 0, 0, 0),
        BackColor = Color.MistyRose,
        ForeColor = Color.Firebrick,
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
    };
    private readonly GroupBox _grpVencimentos = new() { Location = new Point(12, 12), Size = new Size(900, 578) };
    private readonly Label _lblResumoAlertas = new() { AutoSize = false, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
    private readonly DataGridView _dgvAlertas = NovaGradeDeLancamentos();
    private readonly Button _btnMarcarPagoAlerta = new() { Text = "Marcar selecionado como pago" };
    private readonly NumericUpDown _nudDiasAlerta = new() { Minimum = ServicoFinanceiro.DiasAlertaMinimo, Maximum = ServicoFinanceiro.DiasAlertaMaximo };

    public FormPrincipal(ServicoFinanceiro servico, string pastaDosDados)
    {
        _servico = servico;

        // As posições abaixo estão em pixels de uma tela a 100% (96 DPI); o modo Dpi as amplia
        // junto com a fonte em telas com escala maior (125%, 150%), evitando sobreposição.
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Text = "Controle Financeiro — Célia Doces";
        ClientSize = new Size(940, 712);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(960, 752);

        var abaLancamentos = new TabPage("Lançamentos") { UseVisualStyleBackColor = true };
        abaLancamentos.Controls.AddRange([MontarGrupoCategoria(), MontarGrupoLancamento(), MontarGrupoListagem()]);

        var abaOrcamento = new TabPage("Orçamento") { UseVisualStyleBackColor = true };
        abaOrcamento.Controls.AddRange([MontarGrupoDefinicaoOrcamento(), MontarGrupoComparacaoOrcamento()]);

        var abaVencimentos = new TabPage("Vencimentos") { UseVisualStyleBackColor = true };
        abaVencimentos.Controls.Add(MontarGrupoVencimentos());

        var abas = new TabControl { Dock = DockStyle.Fill };
        abas.TabPages.AddRange([abaLancamentos, abaOrcamento, abaVencimentos]);

        Controls.Add(abas);
        Controls.Add(_lblAviso);
        Controls.Add(MontarBarraDeStatus(pastaDosDados));
        // O controle com Dock.Fill precisa ficar na frente para ocupar só o espaço que sobra abaixo do aviso.
        abas.BringToFront();

        ResumeLayout(false);
        PerformLayout();

        RecarregarCategorias();
        AtualizarTela();
    }

    private static StatusStrip MontarBarraDeStatus(string pastaDosDados)
    {
        var rotulo = new ToolStripStatusLabel($"Dados salvos em: {pastaDosDados}") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
        var abrirPasta = new ToolStripStatusLabel("Abrir pasta") { IsLink = true };
        abrirPasta.Click += (_, _) =>
        {
            Directory.CreateDirectory(pastaDosDados);
            Process.Start(new ProcessStartInfo("explorer.exe") { ArgumentList = { pastaDosDados } });
        };

        var barra = new StatusStrip { SizingGrip = false };
        barra.Items.AddRange([rotulo, abrirPasta]);
        return barra;
    }

    /// <param name="pesos">Peso relativo da largura de cada coluna (padrão 100), para textos longos não serem cortados.</param>
    private static DataGridView NovaGrade(params (string Coluna, float Peso)[] pesos)
    {
        var grade = new DataGridView
        {
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        grade.DataBindingComplete += (_, _) =>
        {
            foreach (var (coluna, peso) in pesos)
            {
                if (grade.Columns[coluna] is { } colunaDaGrade)
                    colunaDaGrade.FillWeight = peso;
            }
        };

        return grade;
    }

    private static DataGridView NovaGradeDeLancamentos() =>
        NovaGrade(("Id", 40), ("Tipo", 70), ("Descricao", 170), ("Categoria", 150), ("Status", 80));

    // ---------- montagem da tela ----------

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

        _btnEditarCategoria.Location = new Point(640, 23);
        _btnEditarCategoria.Size = new Size(115, 26);
        _btnEditarCategoria.Click += (_, _) => EditarCategoriaSelecionada();

        _btnExcluirCategoria.Location = new Point(765, 23);
        _btnExcluirCategoria.Size = new Size(115, 26);
        _btnExcluirCategoria.Click += (_, _) => ExcluirCategoriaSelecionada();

        _lstCategorias.Location = new Point(12, 58);
        _lstCategorias.Size = new Size(870, 32);
        // ColumnWidth não acompanha o escalonamento automático por DPI, então é convertido à mão.
        _lstCategorias.ColumnWidth = _lstCategorias.LogicalToDeviceUnits(215);
        _lstCategorias.Format += (_, e) =>
        {
            if (e.ListItem is Categoria categoria)
                e.Value = $"{categoria.Nome} ({categoria.Tipo})";
        };
        _lstCategorias.DoubleClick += (_, _) => EditarCategoriaSelecionada();
        _dicas.SetToolTip(_lstCategorias, "Selecione uma categoria para editar ou excluir (duplo clique edita).");

        grupo.Controls.AddRange([lblNome, _txtNomeCategoria, lblTipo, _cmbTipoCategoria, _btnAdicionarCategoria,
            _btnEditarCategoria, _btnExcluirCategoria, _lstCategorias]);
        return grupo;
    }

    private GroupBox MontarGrupoLancamento()
    {
        var grupo = new GroupBox { Text = "Novo Lançamento", Location = new Point(12, 120), Size = new Size(900, 160) };

        var lblDescricao = new Label { Text = "Descrição:", Location = new Point(12, 28), AutoSize = true };
        _txtDescricao.Location = new Point(100, 25);
        _txtDescricao.Size = new Size(200, 23);

        var lblValor = new Label { Text = "Valor (R$):", Location = new Point(315, 28), AutoSize = true };
        _txtValor.Location = new Point(390, 25);
        _txtValor.Size = new Size(85, 23);

        var lblTipo = new Label { Text = "Tipo:", Location = new Point(490, 28), AutoSize = true };
        _cmbTipoLancamento.Location = new Point(527, 25);
        _cmbTipoLancamento.Size = new Size(110, 23);
        _cmbTipoLancamento.Items.AddRange([TipoCategoria.Receita, TipoCategoria.Despesa]);
        _cmbTipoLancamento.SelectedIndex = 0;
        _cmbTipoLancamento.SelectedIndexChanged += (_, _) => RecarregarCategoriasDoLancamento();

        var lblCategoria = new Label { Text = "Categoria:", Location = new Point(12, 63), AutoSize = true };
        _cmbCategoriaLancamento.Location = new Point(100, 60);
        _cmbCategoriaLancamento.Size = new Size(250, 23);

        var lblVencimento = new Label { Text = "Vencimento:", Location = new Point(652, 28), AutoSize = true };
        _dtpVencimento.Location = new Point(735, 25);
        _dtpVencimento.Size = new Size(150, 23);

        var lblOrigem = new Label { Text = "Origem (se Receita):", Location = new Point(365, 63), AutoSize = true };
        _txtOrigem.Location = new Point(490, 60);
        _txtOrigem.Size = new Size(147, 23);

        _chkPago.Location = new Point(655, 62);

        _btnAdicionarLancamento.Location = new Point(12, 98);
        _btnAdicionarLancamento.Size = new Size(200, 28);
        _btnAdicionarLancamento.Click += BtnAdicionarLancamento_Click;

        _lblMensagem.Location = new Point(225, 102);
        _lblMensagem.Size = new Size(660, 20);

        grupo.Controls.AddRange([
            lblDescricao, _txtDescricao, lblValor, _txtValor, lblTipo, _cmbTipoLancamento,
            lblVencimento, _dtpVencimento, lblCategoria, _cmbCategoriaLancamento, lblOrigem, _txtOrigem,
            _chkPago, _btnAdicionarLancamento, _lblMensagem
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
        _btnFiltrar.Size = new Size(90, 25);
        _btnFiltrar.Click += (_, _) => AtualizarGrid();

        _btnMarcarPago.Location = new Point(350, 21);
        _btnMarcarPago.Size = new Size(140, 25);
        _btnMarcarPago.Click += (_, _) => MarcarSelecionadoComoPago(_dgvLancamentos);

        _btnEditarLancamento.Location = new Point(500, 21);
        _btnEditarLancamento.Size = new Size(100, 25);
        _btnEditarLancamento.Click += (_, _) => EditarLancamentoSelecionado();

        _btnExcluirLancamento.Location = new Point(610, 21);
        _btnExcluirLancamento.Size = new Size(100, 25);
        _btnExcluirLancamento.Click += (_, _) => ExcluirLancamentoSelecionado();

        _lblResumo.Location = new Point(12, 52);
        _lblResumo.Size = new Size(330, 22);

        _lblSaldoRealizado.Location = new Point(350, 52);
        _lblSaldoRealizado.Size = new Size(270, 22);
        _dicas.SetToolTip(_lblSaldoRealizado, "Só o que já foi pago ou recebido: o dinheiro que de fato passou pelo caixa.");

        _lblSaldo.Location = new Point(626, 52);
        _lblSaldo.Size = new Size(260, 22);
        _dicas.SetToolTip(_lblSaldo, "Todos os lançamentos, pagos ou pendentes.");

        _dgvLancamentos.Location = new Point(12, 80);
        _dgvLancamentos.Size = new Size(874, 205);
        _dgvLancamentos.CellFormatting += (_, e) => FormatarLinhaDeLancamento(_dgvLancamentos, e);
        _dgvLancamentos.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
                EditarLancamentoSelecionado();
        };

        grupo.Controls.AddRange([lblFiltro, _cmbFiltro, _btnFiltrar, _btnMarcarPago, _btnEditarLancamento, _btnExcluirLancamento,
            _lblResumo, _lblSaldoRealizado, _lblSaldo, _dgvLancamentos]);
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

        _btnRemoverPlanejado.Location = new Point(690, 55);
        _btnRemoverPlanejado.Size = new Size(190, 26);
        _btnRemoverPlanejado.Click += (_, _) => RemoverValorPlanejadoSelecionado();
        _dicas.SetToolTip(_btnRemoverPlanejado, "Tira do orçamento do mês a categoria selecionada na tabela abaixo.");

        _lblMensagemOrcamento.Location = new Point(12, 59);
        _lblMensagemOrcamento.Size = new Size(665, 20);

        grupo.Controls.AddRange([
            lblMes, _dtpMesOrcamento, lblCategoria, _cmbCategoriaOrcamento,
            lblValor, _txtValorPlanejado, _btnDefinirPlanejado, _btnRemoverPlanejado, _lblMensagemOrcamento
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

    private GroupBox MontarGrupoVencimentos()
    {
        _lblResumoAlertas.Location = new Point(12, 25);
        _lblResumoAlertas.Size = new Size(874, 22);

        _dgvAlertas.Location = new Point(12, 52);
        _dgvAlertas.Size = new Size(874, 470);
        _dgvAlertas.CellFormatting += (_, e) => FormatarLinhaDeLancamento(_dgvAlertas, e);

        _btnMarcarPagoAlerta.Location = new Point(12, 532);
        _btnMarcarPagoAlerta.Size = new Size(220, 30);
        _btnMarcarPagoAlerta.Click += (_, _) => MarcarSelecionadoComoPago(_dgvAlertas);

        var lblPrazo = new Label { Text = "Avisar com antecedência de", Location = new Point(560, 539), AutoSize = true };
        _nudDiasAlerta.Location = new Point(735, 535);
        _nudDiasAlerta.Size = new Size(60, 23);
        _nudDiasAlerta.Value = _servico.DiasAlerta;
        _nudDiasAlerta.ValueChanged += (_, _) => AlterarPrazoDoAlerta();
        var lblDias = new Label { Text = "dias", Location = new Point(802, 539), AutoSize = true };

        _grpVencimentos.Controls.AddRange([_lblResumoAlertas, _dgvAlertas, _btnMarcarPagoAlerta, lblPrazo, _nudDiasAlerta, lblDias]);
        return _grpVencimentos;
    }

    // ---------- categorias ----------

    private void RecarregarCategorias()
    {
        var selecionada = _lstCategorias.SelectedItem;

        _lstCategorias.Items.Clear();
        foreach (var categoria in _servico.Categorias.OrderBy(categoria => categoria.Id))
            _lstCategorias.Items.Add(categoria);

        if (selecionada is not null && _lstCategorias.Items.Contains(selecionada))
            _lstCategorias.SelectedItem = selecionada;

        RecarregarCategoriasDoLancamento();
        PreencherComboDeCategorias(_cmbCategoriaOrcamento, _servico.Categorias);
    }

    private void RecarregarCategoriasDoLancamento()
    {
        var tipo = (TipoCategoria)_cmbTipoLancamento.SelectedItem!;
        PreencherComboDeCategorias(_cmbCategoriaLancamento, _servico.Categorias.Where(categoria => categoria.Tipo == tipo));
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

    private void BtnAdicionarCategoria_Click(object? sender, EventArgs e)
    {
        _lblMensagem.Text = string.Empty;
        var tipo = (TipoCategoria)_cmbTipoCategoria.SelectedItem!;

        var cadastrou = Executar(_lblMensagem, () => _servico.CadastrarCategoria(_txtNomeCategoria.Text, tipo));
        RecarregarCategorias();

        if (cadastrou)
        {
            _txtNomeCategoria.Clear();
            _txtNomeCategoria.Focus();
        }
    }

    private Categoria? CategoriaSelecionada()
    {
        if (_lstCategorias.SelectedItem is Categoria categoria)
            return categoria;

        Avisar("Selecione uma categoria na lista.");
        return null;
    }

    private void EditarCategoriaSelecionada()
    {
        if (CategoriaSelecionada() is not { } categoria)
            return;

        using var janela = new FormEditarCategoria(categoria, (nome, tipo) => _servico.EditarCategoria(categoria, nome, tipo));
        if (janela.ShowDialog(this) == DialogResult.OK)
        {
            RecarregarCategorias();
            AtualizarTela();
        }
    }

    private void ExcluirCategoriaSelecionada()
    {
        if (CategoriaSelecionada() is not { } categoria)
            return;

        if (!Confirmar($"Excluir a categoria \"{categoria.Nome}\"? Os valores planejados dela no orçamento também serão removidos."))
            return;

        _lblMensagem.Text = string.Empty;
        Executar(_lblMensagem, () => _servico.ExcluirCategoria(categoria));
        RecarregarCategorias();
        AtualizarTela();
    }

    // ---------- lançamentos ----------

    private void BtnAdicionarLancamento_Click(object? sender, EventArgs e)
    {
        _lblMensagem.Text = string.Empty;
        var tipo = (TipoCategoria)_cmbTipoLancamento.SelectedItem!;

        if (_cmbCategoriaLancamento.SelectedItem is not Categoria categoria)
        {
            _lblMensagem.Text = $"Cadastre uma categoria do tipo {tipo} antes de adicionar o lançamento.";
            return;
        }

        if (!EntradaDeValor.TentarLer(_txtValor.Text, out var valor))
        {
            _lblMensagem.Text = "Informe um valor numérico válido.";
            return;
        }

        var registrou = Executar(_lblMensagem, () =>
        {
            if (tipo == TipoCategoria.Receita)
                _servico.RegistrarReceita(_dtpVencimento.Value, valor, _txtDescricao.Text, categoria, _txtOrigem.Text, _chkPago.Checked);
            else
                _servico.RegistrarDespesa(_dtpVencimento.Value, valor, _txtDescricao.Text, categoria, _chkPago.Checked);
        });
        AtualizarTela();

        if (registrou)
        {
            _txtDescricao.Clear();
            _txtValor.Clear();
            _txtOrigem.Clear();
            _chkPago.Checked = false;
            _txtDescricao.Focus();
        }
    }

    private LancamentoFinanceiro? LancamentoSelecionado(DataGridView grade)
    {
        if (grade.CurrentRow?.DataBoundItem is LancamentoGridItem item)
            return _servico.BuscarLancamento(item.Id);

        Avisar("Selecione um lançamento na lista.");
        return null;
    }

    private void EditarLancamentoSelecionado()
    {
        if (LancamentoSelecionado(_dgvLancamentos) is not { } lancamento)
            return;

        var tipo = lancamento is Receita ? TipoCategoria.Receita : TipoCategoria.Despesa;
        using var janela = new FormEditarLancamento(
            lancamento,
            _servico.Categorias.Where(categoria => categoria.Tipo == tipo),
            dados => _servico.EditarLancamento(lancamento.Id, dados.Vencimento, dados.Valor, dados.Descricao, dados.Categoria, dados.Origem, dados.Pago));

        if (janela.ShowDialog(this) == DialogResult.OK)
            AtualizarTela();
    }

    private void ExcluirLancamentoSelecionado()
    {
        if (LancamentoSelecionado(_dgvLancamentos) is not { } lancamento)
            return;

        if (!Confirmar($"Excluir \"{lancamento.Descricao}\" ({lancamento.Valor:C}, vencimento {lancamento.Data:d})?"))
            return;

        _lblMensagem.Text = string.Empty;
        Executar(_lblMensagem, () => _servico.ExcluirLancamento(lancamento.Id));
        AtualizarTela();
    }

    private void MarcarSelecionadoComoPago(DataGridView grade)
    {
        if (LancamentoSelecionado(grade) is not { } lancamento)
            return;

        try
        {
            _servico.MarcarComoPago(lancamento.Id);
        }
        catch (ArquivoDeDadosException ex)
        {
            MessageBox.Show(ex.Message, "Não foi possível salvar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        AtualizarTela();
    }

    // ---------- orçamento ----------

    private void BtnDefinirPlanejado_Click(object? sender, EventArgs e)
    {
        _lblMensagemOrcamento.Text = string.Empty;

        if (_cmbCategoriaOrcamento.SelectedItem is not Categoria categoria)
        {
            _lblMensagemOrcamento.Text = "Cadastre uma categoria antes de planejar o orçamento.";
            return;
        }

        if (!EntradaDeValor.TentarLer(_txtValorPlanejado.Text, out var valorPlanejado))
        {
            _lblMensagemOrcamento.Text = "Informe um valor planejado numérico válido.";
            return;
        }

        var mes = _dtpMesOrcamento.Value;
        var definiu = Executar(_lblMensagemOrcamento, () => _servico.DefinirValorPlanejado(mes.Month, mes.Year, categoria, valorPlanejado));
        AtualizarOrcamento();

        if (definiu)
            _txtValorPlanejado.Clear();
    }

    private void RemoverValorPlanejadoSelecionado()
    {
        _lblMensagemOrcamento.Text = string.Empty;

        if (_dgvOrcamento.CurrentRow?.DataBoundItem is not OrcamentoGridItem item)
        {
            _lblMensagemOrcamento.Text = "Selecione na tabela a categoria que deve sair do orçamento.";
            return;
        }

        var categoria = _servico.Categorias.Single(c => c.Id == item.CategoriaId);
        var mes = _dtpMesOrcamento.Value;

        Executar(_lblMensagemOrcamento, () =>
        {
            if (!_servico.RemoverValorPlanejado(mes.Month, mes.Year, categoria))
                _lblMensagemOrcamento.Text = $"\"{categoria.Nome}\" não tem valor planejado neste mês.";
        });
        AtualizarOrcamento();
    }

    // ---------- vencimentos ----------

    private void AlterarPrazoDoAlerta()
    {
        try
        {
            _servico.DefinirDiasAlerta((int)_nudDiasAlerta.Value);
        }
        catch (Exception ex) when (ex is ArgumentException or ArquivoDeDadosException)
        {
            MessageBox.Show(EntradaDeValor.MensagemParaUsuario(ex), "Prazo do alerta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        AtualizarTela();
    }

    // ---------- atualização da tela ----------

    private void AtualizarTela()
    {
        AtualizarGrid();
        AtualizarOrcamento();
        AtualizarAlertas();
    }

    private void AtualizarGrid()
    {
        var lancamentosFiltrados = _servico.FiltrarPorTipo((TipoLancamentoFiltro)_cmbFiltro.SelectedItem!);

        // Iteração tradicional (foreach) para projetar as entidades de domínio em itens de exibição.
        var itensParaExibicao = new List<LancamentoGridItem>();
        foreach (var lancamento in lancamentosFiltrados)
        {
            itensParaExibicao.Add(LancamentoGridItem.DeLancamento(lancamento, DateTime.Today, _servico.DiasAlerta));
        }

        _dgvLancamentos.DataSource = null;
        _dgvLancamentos.DataSource = itensParaExibicao;

        AtualizarResumoDoCaixa();
    }

    private void AtualizarResumoDoCaixa()
    {
        var receitas = _servico.CalcularTotal(TipoLancamentoFiltro.Receitas);
        var despesas = _servico.CalcularTotal(TipoLancamentoFiltro.Despesas);
        var realizado = _servico.CalcularSaldoRealizado();
        var previsto = _servico.CalcularSaldoPrevisto();

        _lblResumo.Text = $"Receitas: {receitas:C}    Despesas: {despesas:C}";
        _lblSaldoRealizado.Text = $"Saldo realizado: {realizado:C}";
        _lblSaldoRealizado.ForeColor = realizado < 0 ? Color.Firebrick : Color.DarkGreen;
        _lblSaldo.Text = $"Saldo previsto: {previsto:C}";
        _lblSaldo.ForeColor = previsto < 0 ? Color.Firebrick : Color.DarkGreen;
    }

    private void AtualizarOrcamento()
    {
        var mes = _dtpMesOrcamento.Value;
        var itens = _servico.CompararOrcamento(mes.Month, mes.Year)
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

    private void AtualizarAlertas()
    {
        var hoje = DateTime.Today;
        var dias = _servico.DiasAlerta;
        var alertas = _servico.ListarAlertasDeVencimento(hoje)
            .Select(lancamento => LancamentoGridItem.DeLancamento(lancamento, hoje, dias))
            .ToList();

        _dgvAlertas.DataSource = null;
        _dgvAlertas.DataSource = alertas;

        var vencidos = alertas.Count(item => item.Vencido);
        var vencendo = alertas.Count - vencidos;
        var descricao = DescreverAlertas(vencidos, vencendo, dias);

        _grpVencimentos.Text = $"Lançamentos não pagos vencidos ou que vencem nos próximos {dias} dias";
        _lblResumoAlertas.Text = alertas.Count == 0
            ? "Nenhuma conta vencida ou a vencer nos próximos dias."
            : $"{descricao}.";
        _lblResumoAlertas.ForeColor = alertas.Count == 0 ? Color.DarkGreen : Color.Firebrick;

        _lblAviso.Visible = alertas.Count > 0;
        _lblAviso.Text = $"Atenção: {descricao}. Veja a aba Vencimentos.";
    }

    private static string DescreverAlertas(int vencidos, int vencendo, int dias)
    {
        var partes = new List<string>();
        if (vencidos > 0)
            partes.Add($"{vencidos} lançamento(s) vencido(s)");
        if (vencendo > 0)
            partes.Add($"{vencendo} vencendo em até {dias} dias");

        return string.Join(" e ", partes);
    }

    private static void FormatarLinhaDeLancamento(DataGridView grade, DataGridViewCellFormattingEventArgs e)
    {
        if (e.CellStyle is null || grade.Rows[e.RowIndex].DataBoundItem is not LancamentoGridItem item)
            return;

        if (item.Vencido)
            e.CellStyle.BackColor = Color.MistyRose;
        else if (item.VenceEmBreve)
            e.CellStyle.BackColor = Color.LightYellow;

        if (e.Value is decimal valor)
        {
            e.Value = valor.ToString("C");
            e.FormattingApplied = true;
        }
        else if (e.Value is DateTime data)
        {
            e.Value = data.ToString("d");
            e.FormattingApplied = true;
        }
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

    // ---------- utilitários ----------

    /// <summary>
    /// Executa uma operação do serviço e mostra no rótulo os erros esperados: dado inválido ou operação proibida
    /// (<see cref="ArgumentException"/>) ou falha ao salvar o arquivo (<see cref="ArquivoDeDadosException"/>).
    /// </summary>
    private static bool Executar(Label mensagem, Action operacao)
    {
        try
        {
            operacao();
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or ArquivoDeDadosException)
        {
            mensagem.Text = EntradaDeValor.MensagemParaUsuario(ex);
            return false;
        }
    }

    private bool Confirmar(string pergunta) =>
        MessageBox.Show(this, $"{pergunta}\n\nEssa ação não pode ser desfeita.", "Confirmar exclusão",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    private void Avisar(string mensagem) =>
        MessageBox.Show(this, mensagem, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
}
