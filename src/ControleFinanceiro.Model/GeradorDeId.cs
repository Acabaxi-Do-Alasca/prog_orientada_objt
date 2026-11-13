namespace ControleFinanceiro.Model;

/// <summary>
/// Gera Ids sequenciais para um tipo de entidade. <see cref="Registrar"/> avisa ao gerador
/// sobre um Id que já existe (por exemplo, lido do arquivo), para que o próximo Id novo
/// nunca repita um antigo.
/// </summary>
internal sealed class GeradorDeId
{
    // lock: os testes automatizados criam entidades em paralelo.
    private readonly object _trava = new();
    private int _ultimo;

    public int Proximo()
    {
        lock (_trava)
            return ++_ultimo;
    }

    public void Registrar(int idExistente)
    {
        if (idExistente <= 0)
            throw new ArgumentOutOfRangeException(nameof(idExistente), "O Id deve ser maior que zero.");

        lock (_trava)
            _ultimo = Math.Max(_ultimo, idExistente);
    }
}
