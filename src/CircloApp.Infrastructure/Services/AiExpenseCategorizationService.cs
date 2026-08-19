using CircloApp.Application.Interfaces;
using Microsoft.SemanticKernel;

namespace CircloApp.Infrastructure.Services
{
    public class AiExpenseCategorizationService : IAiExpenseCategorizationService
    {
        private readonly Kernel _kernel;
        public AiExpenseCategorizationService(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<string> AnalyzeSpendingAsync(string expensesDataJson, CancellationToken cancellationToken = default)
        {
            //var promt = $@"You are an AI financial advisor for the Circlo app.
            //Analyze the following JSON expense data and provide 3 short, actionable financial recommendations:{expensesDataJson}";
            var promt = "Hello How are you";

            var result = await _kernel.InvokePromptAsync(promt, cancellationToken: cancellationToken);
            return result.ToString();
        }
    }
}
