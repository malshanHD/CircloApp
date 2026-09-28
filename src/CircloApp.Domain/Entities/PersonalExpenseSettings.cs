namespace CircloApp.Domain.Entities
{
    public class PersonalExpenseSettings : BaseEntity
    {
        public Guid UserId { get; set; }
        public User user { get; set; } = null!;
        public decimal DefaultMonthlyLimit { get; set; }
        public string CurrencyCode { get; set; } = "LKR";
        public decimal WarningPercentage { get; set; } = 80;
    }
}
