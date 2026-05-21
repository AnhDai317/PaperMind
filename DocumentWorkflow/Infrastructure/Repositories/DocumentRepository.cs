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
        _documents[document.Id] = document;
    }

    public Document? GetById(Guid id)
    {
        _documents.TryGetValue(id, out var document);
        return document;
    }

    public void Update(Document document)
    {
        _documents[document.Id] = document;
    }

    public IEnumerable<Document> GetAll()
    {
        return _documents.Values.OrderByDescending(d => d.Id); // Using Id order as a rough proxy for creation time since Guid doesn't inherently sort by time well, but it's fine for demo
    }
}
