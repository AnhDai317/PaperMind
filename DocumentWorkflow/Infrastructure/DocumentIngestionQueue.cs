using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DocumentWorkflow.Domain.Entities;

namespace DocumentWorkflow.Infrastructure.Queues;

public interface IDocumentIngestionQueue
{
    ValueTask EnqueueAsync(Document document, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Document> ReadAllAsync(CancellationToken cancellationToken = default);
}

public class DocumentIngestionQueue : IDocumentIngestionQueue
{
    private readonly Channel<Document> _channel;

    public DocumentIngestionQueue(int capacity = 5000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<Document>(options);
    }

    public async ValueTask EnqueueAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(document, cancellationToken);
    }

    public IAsyncEnumerable<Document> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
