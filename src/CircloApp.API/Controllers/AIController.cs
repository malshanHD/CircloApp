using CircloApp.API.Models;
using CircloApp.Application.Features.AI.Commands;
using CircloApp.Application.Features.AI.Queries.AskCirclo;
using CircloApp.Application.Features.AI.Queries.GetEventAiAnalysis;
using CircloApp.Application.Features.AI.Queries.RAG;
using CircloApp.Application.Features.AI.Queries.SmartQuery;
using CircloApp.Application.Helper;
using CircloApp.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CircloApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAiService _service;
        private readonly IEmbeddingService _embeddingService;
        private readonly IExpenseVectorSearchService _expenseVectorSearchService;
        private readonly IExpensesService _expnsService;

        public AIController(IMediator mediator, IAiService aiService, IEmbeddingService embeddingService, IExpenseVectorSearchService expenseVectorSearchService, IExpensesService expenses)
        {
            _mediator = mediator;
            _service = aiService;
            _embeddingService = embeddingService;
            _expenseVectorSearchService = expenseVectorSearchService;
            _expnsService = expenses;
        }

        [HttpGet("{eventId:guid}")]
        public async Task<IActionResult> GetEventExpenses(Guid eventId)
        {
            var result = await _mediator.Send(new GetEventExpensesSummaryCommand(eventId));

            return Ok(result);
        }

        [HttpGet("events/{eventId:guid}/categories")]
        public async Task<IActionResult> CategorizeEventExpenses(Guid eventId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetEventAiAnalysisQuery(eventId));

            return Ok(result);
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test(string text)
        {
            var embedding = await _embeddingService.GenerateEmbeddingAync(text);

            return Ok(new
            {
                Text = text,
                Dimensions = embedding.Length,
                First10Values = embedding.ToArray().Take(10)
            });
        }

        [HttpPost("events/{eventId:guid}/ask")]
        public async Task<IActionResult> AskCirclo(Guid eventId, [FromBody] AskCircloRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new AskCircloQuery(eventId, request.Question), cancellationToken);
            return Ok(new
            {
                answer = result,
            });
        }

        [HttpGet("embedding/similarity-test")]
        public async Task<IActionResult> TestSimilarity(CancellationToken cancellationToken)
        {
            var uber = await _embeddingService.GenerateEmbeddingAync(
                "Uber from airport",
                cancellationToken);

            var taxi = await _embeddingService.GenerateEmbeddingAync(
                "Taxi to hotel",
                cancellationToken);

            var food = await _embeddingService.GenerateEmbeddingAync(
                "Chicken fried rice",
                cancellationToken);

            var uberTaxiSimilarity =
                VectorHelper.CosineSimilarity(uber, taxi);

            var uberFoodSimilarity =
                VectorHelper.CosineSimilarity(uber, food);

            return Ok(new
            {
                UberVsTaxi = uberTaxiSimilarity,
                UberVsFood = uberFoodSimilarity
            });
        }

        [HttpPost("search/create-index")]
        public async Task<IActionResult> CreateSearchIndex(CancellationToken cancellationToken)
        {
            await _expenseVectorSearchService.CreateIndexAsync(
                cancellationToken);

            return Ok(new
            {
                message = "Search index created successfully."
            });
        }

        [HttpPost("search/upload-test-expense")]
        public async Task<IActionResult> UploadTestExpense(CancellationToken cancellationToken)
        {
            var expenseId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            await _expenseVectorSearchService.UploadExpenseAsync(
                expenseId,
                eventId,
                "Uber from airport",
                3500m,
                cancellationToken);

            return Ok(new
            {
                message = "Expense uploaded successfully.",
                expenseId,
                eventId
            });
        }

        [HttpDelete("search/delete-index")]
        public async Task<IActionResult> DeleteSearchIndex(CancellationToken cancellationToken)
        {
            await _expenseVectorSearchService.DeleteIndexAsync(
                cancellationToken);

            return Ok(new
            {
                message = "Search index deleted successfully."
            });
        }

        [HttpPost("search/upload-sample-expenses")]
        public async Task<IActionResult> UploadSampleExpenses(CancellationToken cancellationToken)
        {
            var eventId = Guid.Parse("ca97321b-e55d-4087-ae7a-264cd4f70174");

            var expenses = await _expnsService.GetEventExpenses(eventId, cancellationToken);

            foreach (var expense in expenses)
            {
                await _expenseVectorSearchService.UploadExpenseAsync(
                    expense.Id,
                    eventId,
                    expense.Description,
                    expense.Amount,
                    cancellationToken);
            }

            return Ok(new
            {
                message = "Sample expenses uploaded.",
                eventId
            });
        }

        [HttpGet("search/vector")]
        public async Task<IActionResult> VectorSearch(Guid eventId, string query, CancellationToken cancellationToken)
        {
            var results =
                await _expenseVectorSearchService.SearchExpenseAsync(
                    eventId,
                    query,
                    cancellationToken);

            return Ok(results);
        }

        [HttpPost("rag/{eventId:guid}")]
        public async Task<IActionResult> AskWithRag(Guid eventId, [FromBody] AskCircloRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new AskCircloRagQuery(eventId, request.Question), cancellationToken);

            return Ok(new
            {
                answer = result
            });
        }

        [HttpPost("ask/{eventId:guid}")]
        public async Task<IActionResult> Ask(Guid eventId, [FromBody] AskCircloRequest request, CancellationToken cancellationToken)
        {
            var answer = await _mediator.Send(new AskCircloSmartQuery(eventId, request.Question, cancellationToken));

            return Ok(new { answer });
        }
    }

    public class AiTestRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}
