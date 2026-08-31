namespace CircloApp.Application.Features.AI.DTO
{
    public class AiQuestionClassification
    {
        public AiQuestionIntent Intent { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
