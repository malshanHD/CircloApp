namespace CircloApp.Domain.Entities
{
    public class PersonalExpenseCategory : BaseEntity
    {
        public Guid? UserId { get; set; }
        public User? User { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<PersonalExpense> Expenses { get; set; } = new List<PersonalExpense>();
    }
}
