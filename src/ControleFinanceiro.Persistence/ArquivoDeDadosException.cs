namespace ControleFinanceiro.Persistence;

/// <summary>
/// Falha ao ler ou gravar os arquivos de dados: linha corrompida, falta de permissão, disco cheio...
/// A mensagem diz qual arquivo (e qual linha, na leitura) causou o problema.
/// </summary>
public class ArquivoDeDadosException : Exception
{
    public ArquivoDeDadosException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
