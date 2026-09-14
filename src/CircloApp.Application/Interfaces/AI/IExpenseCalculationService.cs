namespace CircloApp.Application.Interfaces.AI
{
    public interface IExpenseCalculationService
    {
        Task<decimal> CalculateTotalAsync(Guid eventId, string question, CancellationToken cancellationToken);
    }
}
