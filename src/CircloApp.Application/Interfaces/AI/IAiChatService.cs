namespace CircloApp.Application.Interfaces.AI
{
    public interface IAiChatService
    {
        Task<string> AskAsync(string question, CancellationToken cancellationToken = default);
    }
}
