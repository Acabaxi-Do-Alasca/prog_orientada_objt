using System.Globalization;
using System.Text;
using ControleFinanceiro.Model;

namespace ControleFinanceiro.Persistence;

/// <summary>
/// Grava os dados em três arquivos texto dentro de <see cref="Pasta"/>: categorias, lançamentos e
/// orçamentos, um registro por linha. Herda o armazenamento em coleções de <see cref="RepositorioMemoria"/>:
/// lê os arquivos para a memória ao ser criado e sobrescreve <see cref="Salvar"/> para gravá-los de volta.
/// </summary>
public class RepositorioArquivoTexto : RepositorioMemoria
{
    private const string ArquivoCategorias = "categorias.txt";
    private const string ArquivoLancamentos = "lancamentos.txt";
    private const string ArquivoOrcamentos = "orcamentos.txt";

    private const string CabecalhoCategorias = "Id;Nome;Tipo";
    private const string CabecalhoLancamentos = "Id;Tipo;Vencimento;Valor;Descricao;Status;CategoriaId;Origem;Recorrente";
    private const string CabecalhoOrcamentos = "Ano;Mes;CategoriaId;ValorPlanejado";

    private const string FormatoData = "yyyy-MM-dd";
    private static readonly CultureInfo Cultura = CultureInfo.InvariantCulture;

    public string Pasta { get; }

    /// <exception cref="ArquivoDeDadosException">Algum arquivo está corrompido ou não pôde ser lido.</exception>
    public RepositorioArquivoTexto(string pasta)
    {
        if (string.IsNullOrWhiteSpace(pasta))
            throw new ArgumentException("A pasta dos dados é obrigatória.", nameof(pasta));

        Pasta = pasta;
        Carregar();
    }

    public override void Salvar()
    {
        try
        {
            Directory.CreateDirectory(Pasta);
            Gravar(ArquivoCategorias, CabecalhoCategorias, Categorias.OrderBy(c => c.Id).Select(LinhaDaCategoria));
            Gravar(ArquivoLancamentos, CabecalhoLancamentos, Lancamentos.Select(LinhaDoLancamento));
            Gravar(ArquivoOrcamentos, CabecalhoOrcamentos, Orcamentos
                .OrderBy(o => o.Ano).ThenBy(o => o.Mes)
                .SelectMany(o => o.Itens.Select(item => LinhaDoItemDeOrcamento(o, item))));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new ArquivoDeDadosException($"Não foi possível gravar os dados em \"{Pasta}\": {ex.Message}", ex);
        }
    }

    // ---------- gravação ----------

    private void Gravar(string arquivo, string cabecalho, IEnumerable<string> linhas)
    {
        // Grava num arquivo temporário e só depois substitui o original: se a gravação falhar no meio,
        // o arquivo anterior continua inteiro.
        var caminho = Path.Combine(Pasta, arquivo);
        var temporario = caminho + ".tmp";
        File.WriteAllLines(temporario, linhas.Prepend(cabecalho), Encoding.UTF8);
        File.Move(temporario, caminho, overwrite: true);
    }

    private static string LinhaDaCategoria(Categoria categoria) =>
        LinhaDeTexto.Juntar(categoria.Id.ToString(Cultura), categoria.Nome, categoria.Tipo.ToString());

    private static string LinhaDoLancamento(LancamentoFinanceiro lancamento)
    {
        var (tipo, origem, recorrente) = lancamento switch
        {
            Receita receita => ("Receita", receita.OrigemRecebimento, ""),
            Despesa despesa => ("Despesa", "", despesa.Recorrente.ToString(Cultura)),
            _ => throw new NotSupportedException($"Tipo de lançamento não suportado: {lancamento.GetType().Name}")
        };

        return LinhaDeTexto.Juntar(
            lancamento.Id.ToString(Cultura), tipo, lancamento.Data.ToString(FormatoData, Cultura),
            lancamento.Valor.ToString(Cultura), lancamento.Descricao, lancamento.Status.ToString(),
            lancamento.Categoria.Id.ToString(Cultura), origem, recorrente);
    }

    private static string LinhaDoItemDeOrcamento(Orcamento orcamento, ItemOrcamento item) =>
        LinhaDeTexto.Juntar(
            orcamento.Ano.ToString(Cultura), orcamento.Mes.ToString(Cultura),
            item.Categoria.Id.ToString(Cultura), item.ValorPlanejado.ToString(Cultura));

    // ---------- leitura ----------

    private void Carregar()
    {
        // As categorias vêm primeiro: lançamentos e orçamentos apontam para elas pelo Id.
        var categoriasPorId = new Dictionary<int, Categoria>();

        LerRegistros(ArquivoCategorias, 3, campos =>
        {
            var categoria = new Categoria(LerInteiro(campos[0]), campos[1], LerEnum<TipoCategoria>(campos[2]));
            if (!categoriasPorId.TryAdd(categoria.Id, categoria))
                throw new FormatException($"a categoria {categoria.Id} aparece mais de uma vez");
            AdicionarCategoria(categoria);
        });

        LerRegistros(ArquivoLancamentos, 9, campos =>
        {
            var id = LerInteiro(campos[0]);
            var data = LerData(campos[2]);
            var valor = LerDecimal(campos[3]);
            var categoria = BuscarCategoria(categoriasPorId, campos[6]);

            LancamentoFinanceiro lancamento = campos[1] switch
            {
                "Receita" => new Receita(id, data, valor, campos[4], categoria, campos[7]),
                "Despesa" => new Despesa(id, data, valor, campos[4], categoria, LerBooleano(campos[8])),
                _ => throw new FormatException($"tipo \"{campos[1]}\" desconhecido (use Receita ou Despesa)")
            };
            lancamento.Status = LerEnum<StatusLancamento>(campos[5]);
            AdicionarLancamento(lancamento);
        });

        LerRegistros(ArquivoOrcamentos, 4, campos =>
        {
            var ano = LerInteiro(campos[0]);
            var mes = LerInteiro(campos[1]);
            ObterOrcamento(mes, ano).AdicionarItem(BuscarCategoria(categoriasPorId, campos[2]), LerDecimal(campos[3]));
        });
    }

    /// <summary>Lê cada linha do arquivo (pulando o cabeçalho e linhas em branco) e informa arquivo e linha em caso de erro.</summary>
    private void LerRegistros(string arquivo, int quantidadeDeCampos, Action<List<string>> lerRegistro)
    {
        var caminho = Path.Combine(Pasta, arquivo);
        if (!File.Exists(caminho))
            return;

        string[] linhas;
        try
        {
            linhas = File.ReadAllLines(caminho, Encoding.UTF8);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new ArquivoDeDadosException($"Não foi possível ler \"{caminho}\": {ex.Message}", ex);
        }

        for (var i = 1; i < linhas.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(linhas[i]))
                continue;

            try
            {
                var campos = LinhaDeTexto.Dividir(linhas[i]);
                if (campos.Count != quantidadeDeCampos)
                    throw new FormatException($"esperava {quantidadeDeCampos} campos separados por \";\", mas encontrou {campos.Count}");

                lerRegistro(campos);
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException)
            {
                throw new ArquivoDeDadosException($"{arquivo}, linha {i + 1}: {ex.Message}", ex);
            }
        }
    }

    private static Categoria BuscarCategoria(Dictionary<int, Categoria> categoriasPorId, string campo)
    {
        var id = LerInteiro(campo);
        return categoriasPorId.TryGetValue(id, out var categoria)
            ? categoria
            : throw new FormatException($"a categoria {id} não existe em {ArquivoCategorias}");
    }

    private static int LerInteiro(string texto) =>
        int.TryParse(texto, NumberStyles.Integer, Cultura, out var valor)
            ? valor
            : throw new FormatException($"\"{texto}\" não é um número inteiro");

    private static decimal LerDecimal(string texto) =>
        decimal.TryParse(texto, NumberStyles.Number, Cultura, out var valor)
            ? valor
            : throw new FormatException($"\"{texto}\" não é um valor válido (use ponto como separador decimal)");

    private static DateTime LerData(string texto) =>
        DateTime.TryParseExact(texto, FormatoData, Cultura, DateTimeStyles.None, out var data)
            ? data
            : throw new FormatException($"\"{texto}\" não é uma data no formato {FormatoData}");

    private static bool LerBooleano(string texto) =>
        bool.TryParse(texto, out var valor)
            ? valor
            : throw new FormatException($"\"{texto}\" não é True nem False");

    private static T LerEnum<T>(string texto) where T : struct, Enum =>
        Enum.TryParse<T>(texto, out var valor) && Enum.IsDefined(valor) && !int.TryParse(texto, out _)
            ? valor
            : throw new FormatException($"\"{texto}\" não é um valor válido de {typeof(T).Name}");
}
