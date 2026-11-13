using ControleFinanceiro.Persistence;
using ControleFinanceiro.Service;

namespace ControleFinanceiro.Presentation.WinForms;

static class Program
{
    /// <summary>
    /// Monta as camadas: repositório em arquivo texto → serviço → tela.
    /// Opcionalmente recebe na linha de comando a pasta dos dados (útil para uma demonstração separada dos dados reais).
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var pastaDosDados = args.Length > 0
            ? Path.GetFullPath(args[0])
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ControleFinanceiroCeliaDoces");

        ServicoFinanceiro servico;
        try
        {
            servico = new ServicoFinanceiro(new RepositorioArquivoTexto(pastaDosDados));
            servico.CriarCategoriasIniciaisSeNecessario();
        }
        catch (ArquivoDeDadosException ex)
        {
            // Fecha sem abrir a tela: continuar e salvar por cima apagaria o que ainda dá para recuperar no arquivo.
            MessageBox.Show(
                $"Não foi possível abrir os dados do sistema.\n\n{ex.Message}\n\nCorrija ou mova o arquivo indicado e abra o programa de novo.",
                "Controle Financeiro — Célia Doces", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.Run(new FormPrincipal(servico, pastaDosDados));
    }
}
