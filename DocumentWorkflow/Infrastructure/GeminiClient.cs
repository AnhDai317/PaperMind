using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DocumentWorkflow.Application.DTOs;

namespace DocumentWorkflow.Infrastructure.AI;

public interface IGeminiClient
{
    Task<GeminiExtractionResponse?> ExtractDocumentDataAsync(ReadOnlyMemory<char> documentText, CancellationToken cancellationToken);
}

public class GeminiClient : IGeminiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiClient> _logger;
    private static readonly Regex JsonMarkdownRegex = new Regex(@"```(?:json)?\s*(.*?)\s*```", RegexOptions.Singleline | RegexOptions.Compiled);

    private readonly ISmartMockProvider _smartMockProvider;

    public GeminiClient(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiClient> logger, ISmartMockProvider smartMockProvider)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _smartMockProvider = smartMockProvider;
    }

    public async Task<GeminiExtractionResponse?> ExtractDocumentDataAsync(ReadOnlyMemory<char> documentText, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["GeminiApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("GeminiApiKey is not configured. Falling back to Smart Mocking.");
            return _smartMockProvider.GenerateMock(documentText.ToString());
        }

        var payloadObj = GeminiPayloadFactory.CreatePromptObject(documentText);
        using var content = JsonContent.Create(payloadObj);

        try
        {
            var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro:generateContent?key={apiKey}", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseResponse(responseString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP call to Gemini API failed. Falling back to Smart Mocking.");
            return _smartMockProvider.GenerateMock(documentText.ToString());
        }
    }

    private GeminiExtractionResponse? ParseResponse(string responseString)
    {
        var rawText = ExtractTextFromGeminiResponse(responseString);
        
        try
        {
            return JsonSerializer.Deserialize(rawText, GeminiSerializationContext.Default.GeminiExtractionResponse);
        }
        catch (JsonException)
        {
            var match = JsonMarkdownRegex.Match(rawText);
            if (match.Success)
            {
                return JsonSerializer.Deserialize(match.Groups[1].Value, GeminiSerializationContext.Default.GeminiExtractionResponse);
            }
            throw new InvalidOperationException("Failed to extract valid JSON from Gemini response.");
        }
    }
    
    private string ExtractTextFromGeminiResponse(string rawResponse)
    {
        using var doc = JsonDocument.Parse(rawResponse);
        try 
        {
            return doc.RootElement.GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString() ?? string.Empty;
        }
        catch (KeyNotFoundException)
        {
             return string.Empty;
        }
    }
}

public static class GeminiPayloadFactory
{
    private static readonly string SystemInstruction = 
        "You are an AI Document Extraction Engine. Output ONLY valid JSON matching this schema exactly:\n" +
        "{\n  \"document_type\": \"Invoice|Contract|CV|Unknown\",\n  \"confidence_score\": 0.00-1.00,\n  \"extracted_data\": { },\n  \"reasoning\": \"Brief explanation\"\n}\n" +
        "Do not include any markdown formatting.";

    public static object CreatePromptObject(ReadOnlyMemory<char> documentMemory)
    {
        ReadOnlySpan<char> documentSpan = documentMemory.Span.Trim();
        var safeText = documentSpan.ToString();

        return new
        {
            system_instruction = new { parts = new[] { new { text = SystemInstruction } } },
            contents = new[] { new { parts = new[] { new { text = $"Process the following document:\n\n{safeText}" } } } }
        };
    }
}
