using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;
using DocumentWorkflow.Infrastructure.Queues;
using DocumentWorkflow.Application.Commands;
using DocumentWorkflow.Domain.Entities;

namespace DocumentWorkflow.Infrastructure.Services;

public class DocumentProcessingBackgroundService : BackgroundService
{
    private readonly IDocumentIngestionQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentProcessingBackgroundService> _logger;

    public DocumentProcessingBackgroundService(
        IDocumentIngestionQueue queue,
        IServiceProvider serviceProvider,
        ILogger<DocumentProcessingBackgroundService> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("High-Throughput Document Processing Engine is initializing...");

        // Simulate sending a document on startup
        _ = Task.Run(async () =>
        {
            await Task.Delay(2000, stoppingToken);
            _logger.LogInformation("Injecting test document into queue...");
            await _queue.EnqueueAsync(new Document(Guid.NewGuid(), "INVOICE #12345 TOTAL: $500.00"), stoppingToken);
        }, stoppingToken);

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
            CancellationToken = stoppingToken
        };

        await Parallel.ForEachAsync(_queue.ReadAllAsync(stoppingToken), options, async (document, ct) =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var command = new ProcessDocumentCommand(document);
                await mediator.Send(command, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Catastrophic error occurred processing document {DocumentId}.", document.Id);
            }
        });
    }
}
