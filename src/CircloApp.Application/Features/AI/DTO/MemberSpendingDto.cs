namespace CircloApp.Application.Features.AI.DTO
{
    public class MemberSpendingDto
    {
        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal TotalPaid { get; set; }
    }
}
