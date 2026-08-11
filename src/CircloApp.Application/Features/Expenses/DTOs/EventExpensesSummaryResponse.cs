namespace CircloApp.Application.Features.Expenses.DTOs
{
    public class EventExpensesSummaryResponse
    {
        public Guid EventId { get; set; }
        public decimal TotalCost { get; set; }
        public decimal EqualSharePerPerson { get; set; }
        public List<UserBalanceDto> UserBalances { get; set; } = new();
    }

    public class UserBalanceDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal TotalPaid { get; set; }
        public decimal TotalSettled { get; set; }
        public decimal TotalSettledReceived { get; set; }
        public decimal Balance { get; set; } // Positive = Paid more (Is Owed), Negative = Paid less (Owes)
        public string Status { get; set; } = string.Empty; // "Owed X" or "Owes X"
    }
}
