using System;
using System.Collections.Generic;
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
    public int Version { get; private set; }

    public List<string> AuditTrail { get; private set; }

    public Document(Guid id, string rawText)
    {
        Id = id;
        RawText = rawText;
        Status = DocumentStatus.Received;
        Type = DocumentType.Unknown;
        ExtractedDataJson = string.Empty;
        Reasoning = string.Empty;
        Version = 1;
        AuditTrail = new List<string> { $"[{DateTime.UtcNow:O}] Document Received." };
    }

    public Document Clone()
    {
        var clone = new Document(Id, RawText);
        clone.Type = this.Type;
        clone.Status = this.Status;
        clone.ConfidenceScore = this.ConfidenceScore;
        clone.ExtractedDataJson = this.ExtractedDataJson;
        clone.Reasoning = this.Reasoning;
        clone.Version = this.Version;
        clone.AuditTrail = new List<string>(this.AuditTrail);
        return clone;
    }

    public void IncrementVersion() => Version++;

    public void UpdateProcessingResult(DocumentType type, double confidence, string extractedData, string reasoning)
    {
        Type = type;
        ConfidenceScore = confidence;
        ExtractedDataJson = extractedData;
        Reasoning = reasoning;

        if (confidence < 0.85)
        {
            Status = DocumentStatus.PendingHumanReview;
            AuditTrail.Add($"[{DateTime.UtcNow:O}] AI Extraction Low Confidence ({confidence:P0}). Routed to PendingHumanReview.");
        }
        else
        {
            Status = DocumentStatus.Completed;
            AuditTrail.Add($"[{DateTime.UtcNow:O}] AI Extraction High Confidence ({confidence:P0}). Auto-Approved.");
        }
        IncrementVersion();
    }
    
    public void Approve(DocumentType confirmedType)
    {
        Type = confirmedType;
        Status = DocumentStatus.Completed;
        AuditTrail.Add($"[{DateTime.UtcNow:O}] Manually Approved as {confirmedType} by Reviewer.");
        IncrementVersion();
    }

    public void MarkAsFailed()
    {
        Status = DocumentStatus.Failed;
        AuditTrail.Add($"[{DateTime.UtcNow:O}] Processing Failed.");
        IncrementVersion();
    }
}
