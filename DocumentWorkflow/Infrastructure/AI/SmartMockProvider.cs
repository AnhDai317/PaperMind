using System.Text.Json;
using DocumentWorkflow.Application.DTOs;

namespace DocumentWorkflow.Infrastructure.AI;

public interface ISmartMockProvider
{
    GeminiExtractionResponse GenerateMock(string text);
}

public class SmartMockProvider : ISmartMockProvider
{
    public GeminiExtractionResponse GenerateMock(string text)
    {
        var lowerText = text.ToLowerInvariant();
        
        if (lowerText.Contains("cv") || lowerText.Contains("resume") || lowerText.Contains("experience"))
        {
            var mock = @"{ ""document_type"": ""CV"", ""confidence_score"": 0.98, ""extracted_data"": { ""name"": ""John Doe"", ""skills"": [""C#"", "".NET 8"", ""React""] }, ""reasoning"": ""Text contains clear indicators of a curriculum vitae such as 'experience' and 'skills'."" }";
            return JsonSerializer.Deserialize(mock, GeminiSerializationContext.Default.GeminiExtractionResponse)!;
        }
        else if (lowerText.Contains("contract") || lowerText.Contains("agreement") || lowerText.Contains("signatures"))
        {
            var mock = @"{ ""document_type"": ""Contract"", ""confidence_score"": 0.92, ""extracted_data"": { ""parties"": [""Party A"", ""Party B""], ""effective_date"": ""2026-06-01"" }, ""reasoning"": ""Found legal terminology such as 'agreement' and 'parties' indicating a contract."" }";
            return JsonSerializer.Deserialize(mock, GeminiSerializationContext.Default.GeminiExtractionResponse)!;
        }
        else if (lowerText.Contains("invoice") || lowerText.Contains("total") || lowerText.Contains("tax"))
        {
            var mock = @"{ ""document_type"": ""Invoice"", ""confidence_score"": 0.99, ""extracted_data"": { ""total_amount"": 1500, ""currency"": ""USD"" }, ""reasoning"": ""Contains pricing, total amounts, and typical invoice structures."" }";
            return JsonSerializer.Deserialize(mock, GeminiSerializationContext.Default.GeminiExtractionResponse)!;
        }

        var unknownMock = @"{ ""document_type"": ""Unknown"", ""confidence_score"": 0.45, ""extracted_data"": {}, ""reasoning"": ""Could not confidently identify the document type. Needs human review."" }";
        return JsonSerializer.Deserialize(unknownMock, GeminiSerializationContext.Default.GeminiExtractionResponse)!;
    }
}
