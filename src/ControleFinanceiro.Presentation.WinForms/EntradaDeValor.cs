using System.Globalization;

namespace ControleFinanceiro.Presentation.WinForms;

internal static class EntradaDeValor
{
    /// <summary>Aceita vírgula ou ponto como separador decimal ("120,50" ou "120.50"), sem separador de milhar.</summary>
    public static bool TentarLer(string texto, out decimal valor) =>
        decimal.TryParse(texto.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out valor);

    public static string Formatar(decimal valor) => valor.ToString("0.00", CultureInfo.CurrentCulture);

    /// <summary>Texto da exceção sem o sufixo técnico " (Parameter 'nome')" que o .NET acrescenta em ArgumentException.</summary>
    public static string MensagemParaUsuario(Exception erro)
    {
        var mensagem = erro.Message;
        if (erro is ArgumentException { ParamName: { } parametro } && mensagem.EndsWith($"'{parametro}')"))
            mensagem = mensagem[..mensagem.LastIndexOf(" (", StringComparison.Ordinal)];

        return mensagem;
    }
}
