# PaperMind: Intelligent Document Processing Engine

Welcome to the **PaperMind** repository! This project implements an intelligent, AI-powered document processing pipeline with a Human-in-the-Loop (HITL) review queue.

## 🚀 Quick Start

### Backend (.NET 8 Web API)
1. Navigate to the backend folder: `cd DocumentWorkflow`
2. Run the application: `dotnet run` (Runs on `http://localhost:5000` by default)

### Frontend (React + Vite)
1. Navigate to the frontend folder: `cd DocumentWorkflowUI`
2. Install dependencies: `npm install`
3. Start the dev server: `npm run dev`

## 🧠 Design Patterns Used

- **Clean Architecture & CQRS**: Utilizing `MediatR` to decouple request handling and enforce single-responsibility.
- **Strategy Pattern**: The `DocumentStrategyResolver` routes documents dynamically based on AI classification (`InvoiceProcessingStrategy`, `ContractProcessingStrategy`, etc.).
- **Human-in-the-Loop (HITL)**: Documents with an AI confidence score below 85% are automatically routed to a manual review queue to prevent hallucination errors.
- **Resilient AI Integration**: Utilizing `Polly` for exponential backoff and retry policies when communicating with external AI APIs.
- **Background Processing**: Utilizing `IHostedService` (`DocumentProcessingBackgroundService`) and high-performance threading channels (`DocumentIngestionQueue`) for non-blocking ingestion.

## 🎤 Demo Script (2 Minutes)

To ensure a smooth presentation for the judges, follow this flow exactly:

**1. Auto-Approval Flow (Invoice)**
- Open the **Auto Extraction** tab.
- Click **Load Invoice** to populate the test data.
- Click **Extract & Route Document**.
- *Talking point:* "Our system uses MediatR to dispatch the document to the AI. Because the confidence is high, it automatically extracts JSON data and approves it using our Strategy pattern."

**2. Human-in-the-Loop Flow (Ambiguous)**
- Still in the **Auto Extraction** tab, click **Load Ambiguous**.
- Click **Extract & Route Document**.
- *Talking point:* "Here, the AI confidence drops below our 85% threshold. Instead of processing bad data, it halts and routes to our review queue."

**3. Manual Review & Audit Trail**
- Switch to the **Review Queue 🛡️** tab.
- Show the pending document in the queue.
- Expand the **Audit Trail** to show full transparency of the pipeline.
- Click **Approve As: Invoice** or **Reject** to demonstrate manual intervention.
- *Talking point:* "The reviewer can see the AI's reasoning, correct the document type, or reject it completely. This ensures 100% data integrity before hitting our database."
