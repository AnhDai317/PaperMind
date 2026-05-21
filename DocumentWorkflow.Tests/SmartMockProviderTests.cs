using Xunit;
using DocumentWorkflow.Infrastructure.AI;

namespace DocumentWorkflow.Tests;

public class SmartMockProviderTests
{
    [Fact]
    public void GenerateMock_WithInvoiceText_ReturnsInvoiceMock()
    {
        // Arrange
        var provider = new SmartMockProvider();
        
        // Act
        var result = provider.GenerateMock("INVOICE #123\nTotal: 100");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Invoice", result.DocumentType);
        Assert.True(result.ConfidenceScore > 0);
    }

    [Fact]
    public void GenerateMock_WithUnknownText_ReturnsLowConfidence()
    {
        // Arrange
        var provider = new SmartMockProvider();
        
        // Act
        var result = provider.GenerateMock("Random note to buy eggs");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unknown", result.DocumentType);
        Assert.True(result.ConfidenceScore < 0.85);
    }
}
