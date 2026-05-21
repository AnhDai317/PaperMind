# System Architecture

PaperMind is built using a highly decoupled, scalable architecture. It separates the ingestion, processing, and storage layers, utilizing modern .NET features for high throughput and resilience.

## High-Level Architecture Diagram

```mermaid
graph TD
    %% Frontend
    Client[React Frontend / UI]

    %% API Layer
    subgraph "API Layer (Minimal APIs)"
        ExtractAPI[POST /api/documents/extract]
        QueueAPI[GET /api/documents]
        ApproveAPI[POST /api/documents/{id}/approve]
        RejectAPI[POST /api/documents/{id}/reject]
    end

    %% Application Layer
    subgraph "Application Layer (CQRS)"
        Mediator[MediatR]
        ExtractCmd[ProcessDocumentCommand]
        StrategyResolver[DocumentStrategyResolver]
        
        subgraph "Strategies"
            InvoiceStrategy[InvoiceProcessingStrategy]
            ContractStrategy[ContractProcessingStrategy]
            CVStrategy[CvProcessingStrategy]
        end
    end

    %% Infrastructure Layer
    subgraph "Infrastructure Layer"
        Gemini[GeminiClient / AI API]
        Polly[Polly Retry/Backoff]
        Repo[(In-Memory DocumentRepository)]
        Queue[DocumentIngestionQueue]
        HostedService[DocumentProcessingBackgroundService]
    end

    %% Flow
    Client --> ExtractAPI
    ExtractAPI --> Mediator
    Mediator --> ExtractCmd
    ExtractCmd --> Queue
    
    HostedService -- Consumes from --> Queue
    HostedService --> Gemini
    Gemini -- Protected by --> Polly
    
    HostedService --> Repo
    
    Client --> QueueAPI
    QueueAPI --> Repo
    
    Client --> ApproveAPI
    ApproveAPI --> StrategyResolver
    StrategyResolver --> InvoiceStrategy
    StrategyResolver --> ContractStrategy
    StrategyResolver --> CVStrategy
    
    InvoiceStrategy --> Repo
    ContractStrategy --> Repo
    CVStrategy --> Repo

    Client --> RejectAPI
    RejectAPI --> Repo
```

## Key Components

1. **API Layer**: Minimal APIs are used for low-ceremony HTTP endpoints.
2. **MediatR**: Decouples API endpoints from the business logic. `ProcessDocumentCommand` orchestrates the flow.
3. **Background Service & Channels**: `DocumentIngestionQueue` uses `System.Threading.Channels` for high-throughput, thread-safe asynchronous processing. `DocumentProcessingBackgroundService` consumes items from this queue without blocking the main web threads.
4. **AI Integration**: The `GeminiClient` makes requests to the LLM. It's wrapped with `Polly` policies to automatically handle transient failures and rate limits (429 Too Many Requests) via exponential backoff.
5. **Strategy Pattern**: The `DocumentStrategyResolver` selects the appropriate processing algorithm (Invoice, Contract, CV) based on the document's type. This makes the system open for extension (easy to add new document types) but closed for modification.
6. **Data Store**: `DocumentRepository` provides persistence and serves the frontend's Review Queue.
