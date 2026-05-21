using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DocumentWorkflow.Domain.Enums;
using DocumentWorkflow.Domain.Entities;

namespace DocumentWorkflow.Application.Strategies;

public interface IDocumentProcessingStrategy
{
    DocumentType DocumentType { get; }
    Task ProcessAsync(Document document, string extractedDataJson, CancellationToken cancellationToken);
}

public class InvoiceProcessingStrategy : IDocumentProcessingStrategy
{
    public DocumentType DocumentType => DocumentType.Invoice;
    private readonly ILogger<InvoiceProcessingStrategy> _logger;

    public InvoiceProcessingStrategy(ILogger<InvoiceProcessingStrategy> logger)
    {
        _logger = logger;
    }

    public Task ProcessAsync(Document document, string extractedDataJson, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Successfully routed and processed INVOICE {Id}.", document.Id);
        return Task.CompletedTask;
    }
}

public class ContractProcessingStrategy : IDocumentProcessingStrategy
{
    public DocumentType DocumentType => DocumentType.Contract;
    private readonly ILogger<ContractProcessingStrategy> _logger;

    public ContractProcessingStrategy(ILogger<ContractProcessingStrategy> logger)
    {
        _logger = logger;
    }

    public Task ProcessAsync(Document document, string extractedDataJson, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Successfully routed and processed CONTRACT {Id}.", document.Id);
        return Task.CompletedTask;
    }
}

public class CvProcessingStrategy : IDocumentProcessingStrategy
{
    public DocumentType DocumentType => DocumentType.CV;
    private readonly ILogger<CvProcessingStrategy> _logger;

    public CvProcessingStrategy(ILogger<CvProcessingStrategy> logger)
    {
        _logger = logger;
    }

    public Task ProcessAsync(Document document, string extractedDataJson, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Successfully routed and processed CV {Id}.", document.Id);
        return Task.CompletedTask;
    }
}

public class DocumentStrategyResolver
{
    private readonly IEnumerable<IDocumentProcessingStrategy> _strategies;

    public DocumentStrategyResolver(IEnumerable<IDocumentProcessingStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IDocumentProcessingStrategy? Resolve(DocumentType type)
    {
        return _strategies.FirstOrDefault(s => s.DocumentType == type);
    }
}
