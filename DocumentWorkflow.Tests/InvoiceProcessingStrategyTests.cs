using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using DocumentWorkflow.Application.Strategies;
using DocumentWorkflow.Domain.Entities;
using DocumentWorkflow.Domain.Enums;

namespace DocumentWorkflow.Tests;

public class InvoiceProcessingStrategyTests
{
    [Fact]
    public async Task ProcessAsync_ExecutesSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<InvoiceProcessingStrategy>>();
        var strategy = new InvoiceProcessingStrategy(mockLogger.Object);
        var document = new Document(Guid.NewGuid(), "Invoice text");
        
        // Act
        await strategy.ProcessAsync(document, "{}", CancellationToken.None);

        // Assert
        Assert.Equal(DocumentType.Invoice, strategy.DocumentType);
        // If no exception is thrown, the test passes.
    }
}
