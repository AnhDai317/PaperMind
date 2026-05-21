using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using MediatR;
using System.Reflection;
using DocumentWorkflow.Application.Strategies;
using DocumentWorkflow.Infrastructure.AI;
using DocumentWorkflow.Infrastructure.Queues;
using DocumentWorkflow.Application.Commands;
using DocumentWorkflow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 1. Queue Configuration
builder.Services.AddSingleton<IDocumentIngestionQueue>(new DocumentIngestionQueue(capacity: 5000));

// 2. MediatR Setup
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// 3. Strategy Pattern Setup
builder.Services.AddTransient<IDocumentProcessingStrategy, InvoiceProcessingStrategy>();
builder.Services.AddTransient<IDocumentProcessingStrategy, ContractProcessingStrategy>();
builder.Services.AddTransient<IDocumentProcessingStrategy, CvProcessingStrategy>();
builder.Services.AddTransient<DocumentStrategyResolver>();

// 4. HTTP Client, AI Integration & Polly Resilience
builder.Services.AddHttpClient<IGeminiClient, GeminiClient>()
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

// 5. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowAll");

app.MapPost("/api/documents/extract", async ([FromBody] ExtractRequest request, IMediator mediator) =>
{
    var document = new Document(Guid.NewGuid(), request.Text);
    var command = new ProcessDocumentCommand(document);
    var processedDoc = await mediator.Send(command);
    return Results.Ok(processedDoc);
});

app.Run();

public class ExtractRequest
{
    public string Text { get; set; } = string.Empty;
}
