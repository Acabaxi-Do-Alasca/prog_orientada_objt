using ControleFinanceiro.Model;
using ControleFinanceiro.Persistence;

namespace ControleFinanceiro.Presentation.WinForms;

public record DadosDoLancamento(DateTime Vencimento, decimal Valor, string Descricao, Categoria Categoria, string? Origem, bool Pago);

/// <summary>
/// Janela de edição de um lançamento. Quem a abre informa o que fazer ao salvar; se a operação recusar
/// os dados, a mensagem aparece aqui mesmo e a janela continua aberta para correção.
/// </summary>
public class FormEditarLancamento : Form
{
    private readonly Action<DadosDoLancamento> _salvar;
    private readonly bool _ehReceita;

    private readonly TextBox _txtDescricao = new();
    private readonly TextBox _txtValor = new();
    private readonly DateTimePicker _dtpVencimento = new() { Format = DateTimePickerFormat.Short };
    private readonly ComboBox _cmbCategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtOrigem = new();
    private readonly CheckBox _chkPago = new() { Text = "Já pago / recebido", AutoSize = true };
    private readonly Label _lblMensagem = new() { ForeColor = Color.Firebrick, AutoSize = false };

    public FormEditarLancamento(LancamentoFinanceiro lancamento, IEnumerable<Categoria> categoriasDoTipo, Action<DadosDoLancamento> salvar)
    {
        _salvar = salvar;
        _ehReceita = lancamento is Receita;

        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = $"Editar {(_ehReceita ? "receita" : "despesa")} nº {lancamento.Id}";
        ClientSize = new Size(460, 295);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        AdicionarCampo("Descrição:", _txtDescricao, 20, 300);
        AdicionarCampo("Valor (R$):", _txtValor, 55, 120);
        AdicionarCampo("Vencimento:", _dtpVencimento, 90, 150);
        AdicionarCampo("Categoria:", _cmbCategoria, 125, 300);
        if (_ehReceita)
            AdicionarCampo("Origem:", _txtOrigem, 160, 300);

        _chkPago.Location = new Point(130, _ehReceita ? 195 : 160);
        _lblMensagem.Location = new Point(16, 222);
        _lblMensagem.Size = new Size(428, 20);

        var btnSalvar = new Button { Text = "Salvar", Location = new Point(240, 252), Size = new Size(100, 30) };
        var btnCancelar = new Button { Text = "Cancelar", Location = new Point(346, 252), Size = new Size(100, 30), DialogResult = DialogResult.Cancel };
        btnSalvar.Click += (_, _) => Salvar();
        AcceptButton = btnSalvar;
        CancelButton = btnCancelar;
        Controls.AddRange([_chkPago, _lblMensagem, btnSalvar, btnCancelar]);
        ResumeLayout(false);
        PerformLayout();

        _txtDescricao.Text = lancamento.Descricao;
        _txtValor.Text = EntradaDeValor.Formatar(lancamento.Valor);
        _dtpVencimento.Value = lancamento.Data;
        foreach (var categoria in categoriasDoTipo.OrderBy(c => c.Nome))
            _cmbCategoria.Items.Add(categoria);
        _cmbCategoria.SelectedItem = lancamento.Categoria;
        _txtOrigem.Text = (lancamento as Receita)?.OrigemRecebimento ?? string.Empty;
        _chkPago.Checked = lancamento.Status == StatusLancamento.Pago;
    }

    private void AdicionarCampo(string rotulo, Control campo, int y, int largura)
    {
        var lbl = new Label { Text = rotulo, Location = new Point(16, y + 3), AutoSize = true };
        campo.Location = new Point(130, y);
        campo.Size = new Size(largura, 23);
        Controls.AddRange([lbl, campo]);
    }

    private void Salvar()
    {
        _lblMensagem.Text = string.Empty;

        if (!EntradaDeValor.TentarLer(_txtValor.Text, out var valor))
        {
            _lblMensagem.Text = "Informe um valor numérico válido.";
            return;
        }

        if (_cmbCategoria.SelectedItem is not Categoria categoria)
        {
            _lblMensagem.Text = "Escolha uma categoria.";
            return;
        }

        try
        {
            _salvar(new DadosDoLancamento(_dtpVencimento.Value, valor, _txtDescricao.Text, categoria, _ehReceita ? _txtOrigem.Text : null, _chkPago.Checked));
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex) when (ex is ArgumentException or ArquivoDeDadosException)
        {
            _lblMensagem.Text = EntradaDeValor.MensagemParaUsuario(ex);
        }
    }
}
