using System;
using Xunit;
using DocumentWorkflow.Domain.Entities;
using DocumentWorkflow.Domain.Enums;

namespace DocumentWorkflow.Tests;

public class DocumentTests
{
    [Fact]
    public void UpdateProcessingResult_WithLowConfidence_SetsPendingHumanReview()
    {
        // Arrange
        var document = new Document(Guid.NewGuid(), "Test data");
        
        // Act
        document.UpdateProcessingResult(DocumentType.Invoice, 0.80, "{}", "Low confidence");

        // Assert
        Assert.Equal(DocumentStatus.PendingHumanReview, document.Status);
        Assert.Equal(DocumentType.Invoice, document.Type);
    }

    [Fact]
    public void UpdateProcessingResult_WithHighConfidence_SetsCompleted()
    {
        // Arrange
        var document = new Document(Guid.NewGuid(), "Test data");
        
        // Act
        document.UpdateProcessingResult(DocumentType.Invoice, 0.90, "{}", "High confidence");

        // Assert
        Assert.Equal(DocumentStatus.Completed, document.Status);
    }
}
