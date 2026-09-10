namespace CircloApp.Application.Interfaces.AI
{
    public interface IExpenseSearchIndexer
    {
        Task IndexExpenseAsync(Guid expenseId, Guid eventId, string description, decimal amount, CancellationToken cancellationToken);
    }
}
