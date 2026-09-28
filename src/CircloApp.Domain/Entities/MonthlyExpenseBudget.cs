namespace CircloApp.Domain.Entities
{
    public class MonthlyExpenseBudget : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal LimitAmount { get; set; }
    }
}
