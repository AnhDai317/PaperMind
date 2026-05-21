using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DocumentWorkflow.Domain.Entities;
using DocumentWorkflow.Domain.Enums;
using DocumentWorkflow.Application.Strategies;
using DocumentWorkflow.Infrastructure.AI;

namespace DocumentWorkflow.Application.Commands;

public record ProcessDocumentCommand(Document Document) : IRequest<Document>;

public class ProcessDocumentCommandHandler : IRequestHandler<ProcessDocumentCommand, Document>
{
    private readonly IGeminiClient _geminiClient;
    private readonly DocumentStrategyResolver _strategyResolver;
    private readonly ILogger<ProcessDocumentCommandHandler> _logger;

    public ProcessDocumentCommandHandler(IGeminiClient geminiClient, DocumentStrategyResolver strategyResolver, ILogger<ProcessDocumentCommandHandler> logger)
    {
        _geminiClient = geminiClient;
        _strategyResolver = strategyResolver;
        _logger = logger;
    }

    public async Task<Document> Handle(ProcessDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = request.Document;
        var memoryText = document.RawText.AsMemory();

        _logger.LogInformation("Sending Document {Id} to Gemini...", document.Id);
        
        var aiResponse = await _geminiClient.ExtractDocumentDataAsync(memoryText, cancellationToken);

        if (aiResponse == null)
        {
            _logger.LogWarning("AI Response was null for {Id}", document.Id);
            document.MarkAsFailed();
            return document;
        }

        if (!Enum.TryParse<DocumentType>(aiResponse.DocumentType, true, out var parsedType))
        {
            parsedType = DocumentType.Unknown;
        }

        document.UpdateProcessingResult(
            parsedType, 
            aiResponse.ConfidenceScore, 
            aiResponse.ExtractedData.ValueKind != System.Text.Json.JsonValueKind.Undefined ? aiResponse.ExtractedData.GetRawText() : "{}", 
            aiResponse.Reasoning);

        if (document.Status == DocumentStatus.PendingHumanReview)
        {
            _logger.LogWarning("Document {Id} requires human review. Confidence: {Score}", document.Id, document.ConfidenceScore);
            return document;
        }

        var strategy = _strategyResolver.Resolve(parsedType);
        if (strategy != null)
        {
            await strategy.ProcessAsync(document, document.ExtractedDataJson, cancellationToken);
        }
        else
        {
            _logger.LogWarning("No strategy found for type: {Type}", parsedType);
        }

        return document;
    }
}
