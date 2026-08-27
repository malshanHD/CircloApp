using MediatR;

namespace CircloApp.Application.Features.AI.Queries.GetEventAiAnalysis
{
    public record GetEventAiAnalysisQuery(Guid EventId) : IRequest<ExpenseAnalysisResponse>;
}
