namespace SantiagoConecta.SharedUI.Data
{
    /// <summary>
    /// Permite abrir el chat del asistente (ChatbotWidget) desde cualquier componente.
    /// </summary>
    public sealed class AsistenteIaService
    {
        public event Func<Task>? AbrirSolicitado;

        public Task AbrirAsync() => AbrirSolicitado?.Invoke() ?? Task.CompletedTask;
    }
}
