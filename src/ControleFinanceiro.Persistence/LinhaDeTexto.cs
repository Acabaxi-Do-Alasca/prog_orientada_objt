using System.Text;

namespace ControleFinanceiro.Persistence;

/// <summary>
/// Monta e separa as linhas dos arquivos de dados. Os campos são separados por ";" e um ";" ou "\"
/// dentro do texto (por exemplo, numa descrição) é gravado como "\;" ou "\\".
/// </summary>
internal static class LinhaDeTexto
{
    private const char Separador = ';';
    private const char Escape = '\\';

    public static string Juntar(params string[] campos) =>
        string.Join(Separador, campos.Select(campo => campo.Replace("\\", "\\\\").Replace(";", "\\;")));

    public static List<string> Dividir(string linha)
    {
        var campos = new List<string>();
        var atual = new StringBuilder();

        for (var i = 0; i < linha.Length; i++)
        {
            var caractere = linha[i];

            if (caractere == Escape && i + 1 < linha.Length)
                atual.Append(linha[++i]);
            else if (caractere == Separador)
            {
                campos.Add(atual.ToString());
                atual.Clear();
            }
            else
                atual.Append(caractere);
        }

        campos.Add(atual.ToString());
        return campos;
    }
}
