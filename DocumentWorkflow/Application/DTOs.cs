using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocumentWorkflow.Application.DTOs;

public class GeminiExtractionResponse
{
    [JsonPropertyName("document_type")]
    public string DocumentType { get; set; } = string.Empty;

    [JsonPropertyName("confidence_score")]
    public double ConfidenceScore { get; set; }

    [JsonPropertyName("extracted_data")]
    public JsonElement ExtractedData { get; set; }

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;
}

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(GeminiExtractionResponse))]
public partial class GeminiSerializationContext : JsonSerializerContext
{
}
