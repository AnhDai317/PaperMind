namespace DocumentWorkflow.Domain.Enums;

public enum DocumentType 
{ 
    Unknown, 
    Invoice, 
    Contract, 
    CV 
}

public enum DocumentStatus 
{ 
    Received, 
    Processing, 
    Completed, 
    PendingHumanReview, 
    Failed 
}
