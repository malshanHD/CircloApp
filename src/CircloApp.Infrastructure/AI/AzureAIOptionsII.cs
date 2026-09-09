namespace CircloApp.Infrastructure.AI
{
    public class AzureAIOptionsII
    {
        public const string SectionName = "AzureAINew";
        public string Endpoint { get; set; } = string.Empty;
        public string Apikey { get; set; } = string.Empty;
        public string ChatDeploymentName { get; set; } = string.Empty;
    }
}
