using ControleFinanceiro.Model;
using ControleFinanceiro.Persistence;

namespace ControleFinanceiro.Presentation.WinForms;

/// <summary>Janela para renomear uma categoria ou trocar o tipo dela. Erros aparecem aqui mesmo, sem fechar a janela.</summary>
public class FormEditarCategoria : Form
{
    private readonly Action<string, TipoCategoria> _salvar;

    private readonly TextBox _txtNome = new();
    private readonly ComboBox _cmbTipo = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _lblMensagem = new() { ForeColor = Color.Firebrick, AutoSize = false };

    public FormEditarCategoria(Categoria categoria, Action<string, TipoCategoria> salvar)
    {
        _salvar = salvar;

        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = $"Editar categoria \"{categoria.Nome}\"";
        ClientSize = new Size(420, 175);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var lblNome = new Label { Text = "Nome:", Location = new Point(16, 23), AutoSize = true };
        _txtNome.Location = new Point(100, 20);
        _txtNome.Size = new Size(300, 23);

        var lblTipo = new Label { Text = "Tipo:", Location = new Point(16, 58), AutoSize = true };
        _cmbTipo.Location = new Point(100, 55);
        _cmbTipo.Size = new Size(140, 23);
        _cmbTipo.Items.AddRange([TipoCategoria.Receita, TipoCategoria.Despesa]);

        _lblMensagem.Location = new Point(16, 90);
        _lblMensagem.Size = new Size(388, 36);

        var btnSalvar = new Button { Text = "Salvar", Location = new Point(200, 132), Size = new Size(100, 30) };
        var btnCancelar = new Button { Text = "Cancelar", Location = new Point(306, 132), Size = new Size(100, 30), DialogResult = DialogResult.Cancel };
        btnSalvar.Click += (_, _) => Salvar();
        AcceptButton = btnSalvar;
        CancelButton = btnCancelar;

        Controls.AddRange([lblNome, _txtNome, lblTipo, _cmbTipo, _lblMensagem, btnSalvar, btnCancelar]);
        ResumeLayout(false);
        PerformLayout();

        _txtNome.Text = categoria.Nome;
        _cmbTipo.SelectedItem = categoria.Tipo;
    }

    private void Salvar()
    {
        _lblMensagem.Text = string.Empty;

        try
        {
            _salvar(_txtNome.Text, (TipoCategoria)_cmbTipo.SelectedItem!);
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex) when (ex is ArgumentException or ArquivoDeDadosException)
        {
            _lblMensagem.Text = EntradaDeValor.MensagemParaUsuario(ex);
        }
    }
}
