namespace CircloApp.Application.Features.PersonalExpenses.DTOs
{
    public class PersonalExpenseCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public bool IsSystem { get; set; }
    }
}
