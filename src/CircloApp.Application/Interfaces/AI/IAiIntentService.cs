using CircloApp.Application.Features.AI_II.Models;

namespace CircloApp.Application.Interfaces.AI
{
    public interface IAiIntentService
    {
        Task<AiQueryIntent> DetermineIntentAsync(string question, CancellationToken cancellationToken);
        Task<AiQueryAnalysis> AnalyzeAsync(string question, CancellationToken cancellationToken);
    }
}
