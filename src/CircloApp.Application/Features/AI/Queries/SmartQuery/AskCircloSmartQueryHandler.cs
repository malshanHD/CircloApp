using CircloApp.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircloApp.Application.Features.AI.Queries.SmartQuery
{
    public class AskCircloSmartQueryHandler : IRequestHandler<AskCircloSmartQuery, string>
    {
        private readonly IAiService _aiService;

        public AskCircloSmartQueryHandler(IAiService aiService)
        {
            _aiService = aiService;
        }

        public async Task<string> Handle(AskCircloSmartQuery request, CancellationToken cancellationToken)
        {
            return await _aiService.AskCircloSmartAsync(request.EventId, request.Question, cancellationToken);
        }
    }
}
