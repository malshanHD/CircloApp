namespace CircloApp.Application.Features.PersonalExpenses.DTOs
{
    public class CreatePersonalExpenseRequest
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        /// <summary>
        /// The date when the expense actually occurred.
        /// </summary>
        public DateOnly ExpenseDate { get; set; }
        /// <summary>
        /// Optional. Uncategorized will be used when no category is selected.
        /// </summary>
        public Guid? CategoryId { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Note { get; set; }
    }
}
