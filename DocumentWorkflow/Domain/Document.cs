using System;
using DocumentWorkflow.Domain.Enums;

namespace DocumentWorkflow.Domain.Entities;

public class Document
{
    public Guid Id { get; private set; }
    public string RawText { get; private set; }
    public DocumentType Type { get; private set; }
    public DocumentStatus Status { get; private set; }
    public double ConfidenceScore { get; private set; }
    public string ExtractedDataJson { get; private set; }
    public string Reasoning { get; private set; }

    public Document(Guid id, string rawText)
    {
        Id = id;
        RawText = rawText;
        Status = DocumentStatus.Received;
        Type = DocumentType.Unknown;
        ExtractedDataJson = string.Empty;
        Reasoning = string.Empty;
    }

    public void UpdateProcessingResult(DocumentType type, double confidence, string extractedData, string reasoning)
    {
        Type = type;
        ConfidenceScore = confidence;
        ExtractedDataJson = extractedData;
        Reasoning = reasoning;

        if (confidence < 0.85)
        {
            Status = DocumentStatus.PendingHumanReview;
        }
        else
        {
            Status = DocumentStatus.Completed;
        }
    }
    
    public void MarkAsFailed()
    {
        Status = DocumentStatus.Failed;
    }
}
