using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DocumentWorkflow.Domain.Entities;
using DocumentWorkflow.Domain.Enums;

namespace DocumentWorkflow.Infrastructure.Repositories;

public interface IDocumentRepository
{
    void Add(Document document);
    Document? GetById(Guid id);
    void Update(Document document);
    IEnumerable<Document> GetAll();
}

public class DocumentRepository : IDocumentRepository
{
    private readonly ConcurrentDictionary<Guid, Document> _documents = new();

    public void Add(Document document)
    {
        _documents[document.Id] = document.Clone();
    }

    public Document? GetById(Guid id)
    {
        if (_documents.TryGetValue(id, out var document))
            return document.Clone();
        return null;
    }

    public void Update(Document document)
    {
        if (!_documents.TryGetValue(document.Id, out var existing))
            throw new InvalidOperationException("Document not found.");

        if (existing.Version != document.Version)
            throw new InvalidOperationException("Concurrency conflict: Lost Update detected.");

        if (!_documents.TryUpdate(document.Id, document.Clone(), existing))
            throw new InvalidOperationException("Concurrency conflict: Lost Update detected.");
    }

    public IEnumerable<Document> GetAll()
    {
        return _documents.Values.Select(d => d.Clone()).OrderByDescending(d => d.Id); 
    }
}
