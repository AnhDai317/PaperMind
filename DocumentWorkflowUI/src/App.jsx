import { useState } from 'react';
import './index.css';

const SAMPLES = {
  invoice: `INVOICE #99812\n\nDate: 2026-05-21\nVendor: Globex Corp\n\nItems:\n- Software License: $1,500.00\n- Cloud Hosting: $250.00\n\nTotal Due: $1,750.00\nTax: 10%`,
  contract: `NON-DISCLOSURE AGREEMENT\n\nEffective Date: 2026-06-01\n\nParties:\n1. Acme Corp (Disclosing Party)\n2. Jane Smith (Receiving Party)\n\nThe Receiving Party agrees to keep all proprietary algorithms confidential. Signatures attached below.`,
  cv: `John Doe - Senior AI Engineer\n\nExperience:\n- 5 years building .NET Microservices\n- Created high-throughput document processing pipelines using MediatR.\n\nSkills: C#, React, Clean Architecture, CQRS, System.Threading.Channels.`
};

function App() {
  const [text, setText] = useState('');
  const [step, setStep] = useState(0); // 0: Input, 1: API Call, 2: Done
  const [result, setResult] = useState(null);
  const [error, setError] = useState(null);

  const handleProcess = async () => {
    if (!text.trim()) return;
    
    setStep(1);
    setResult(null);
    setError(null);

    try {
      const response = await fetch('http://localhost:5000/api/documents/extract', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ text })
      });

      if (response.ok) {
        const data = await response.json();
        setResult(data);
        setStep(2);
      } else {
        setError('Server returned an error.');
        setStep(0);
      }
    } catch (err) {
      setError('Network error. Is the .NET API running on port 5000?');
      setStep(0);
    }
  };

  const getDocType = () => {
    if (!result || !result.type) return 'Unknown';
    const mapping = { 1: 'Invoice', 2: 'Contract', 3: 'CV' };
    return mapping[result.type] || 'Unknown';
  };

  // Syntax highlighting for JSON
  const formatJson = (jsonString) => {
    if (!jsonString) return '{}';
    try {
      const obj = JSON.parse(jsonString);
      const str = JSON.stringify(obj, null, 2);
      return str.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function (match) {
        let cls = 'json-number';
        if (/^"/.test(match)) {
            if (/:$/.test(match)) {
                cls = 'json-key';
            } else {
                cls = 'json-string';
            }
        } else if (/true|false/.test(match)) {
            cls = 'json-number';
        } else if (/null/.test(match)) {
            cls = 'json-number';
        }
        return `<span class="${cls}">${match}</span>`;
      });
    } catch {
      return jsonString;
    }
  };

  return (
    <>
      <div className="ambient-bg">
        <div className="blob blob-1"></div>
        <div className="blob blob-2"></div>
        <div className="blob blob-3"></div>
      </div>
      
      <div className="app-container">
        <header className="header">
          <div>
            <h1>Document Processing Engine</h1>
            <p>Clean Architecture • MediatR • Strategy Pattern • Smart Mocking</p>
          </div>
          <div>
            <div className="type-badge Invoice" style={{ opacity: 0.7, fontSize: '0.8rem' }}>Backend: .NET 8 Web API</div>
          </div>
        </header>

        <div className="main-content">
          {/* LEFT PANEL */}
          <div className="glass-panel left">
            <h2 className="panel-title">📝 Input Data</h2>
            
            <div className="fast-actions">
              <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)', display: 'flex', alignItems: 'center' }}>Test Data:</span>
              <button className="action-btn" onClick={() => setText(SAMPLES.invoice)}>Load Invoice</button>
              <button className="action-btn" onClick={() => setText(SAMPLES.contract)}>Load Contract</button>
              <button className="action-btn" onClick={() => setText(SAMPLES.cv)}>Load CV</button>
            </div>

            <textarea 
              value={text} 
              onChange={(e) => setText(e.target.value)}
              placeholder="Paste raw text here... Our Smart Mocking will automatically detect CVs, Invoices, and Contracts."
            />
            
            {error && <div style={{ color: 'var(--danger)', marginTop: '1rem', fontSize: '0.9rem' }}>{error}</div>}

            <button className="submit-btn" onClick={handleProcess} disabled={step === 1 || !text.trim()}>
              {step === 1 ? '🧠 AI Processing & Routing...' : '🚀 Extract & Route Document'}
            </button>
          </div>

          {/* RIGHT PANEL */}
          <div className="glass-panel right">
            <div className="stepper">
              <div className={`step ${step >= 0 ? 'active' : ''} ${step > 0 ? 'done' : ''}`}>
                <div className="step-icon">{step > 0 ? '✓' : '1'}</div>
                <span>Awaiting Input</span>
              </div>
              <div className={`step ${step >= 1 ? 'active' : ''} ${step > 1 ? 'done' : ''}`}>
                <div className={`step-icon ${step === 1 ? 'pulse-icon' : ''}`}>{step > 1 ? '✓' : '2'}</div>
                <span>MediatR & Gemini</span>
              </div>
              <div className={`step ${step === 2 ? 'active done' : ''}`}>
                <div className="step-icon">{step === 2 ? '✓' : '3'}</div>
                <span>Strategy Routing</span>
              </div>
            </div>

            {step === 0 && !result && (
              <div style={{ flex: 1, display: 'flex', justifyContent: 'center', alignItems: 'center', color: 'var(--text-muted)' }}>
                Please input a document to see the extraction pipeline in action.
              </div>
            )}

            {step === 1 && (
              <div style={{ flex: 1, display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', color: 'var(--primary)', gap: '1.5rem' }}>
                <div style={{ width: '50px', height: '50px', border: '4px solid rgba(129, 140, 248, 0.2)', borderTopColor: 'var(--primary)', borderRadius: '50%', animation: 'spin 1s linear infinite' }}></div>
                <div style={{ textAlign: 'center' }}>
                  <h3 style={{ marginBottom: '0.5rem', color: '#e2e8f0' }}>Calling AI Pipeline</h3>
                  <p style={{ color: 'var(--text-muted)' }}>Executing ProcessDocumentCommand via MediatR...</p>
                </div>
              </div>
            )}

            {step === 2 && result && (
              <div style={{ display: 'flex', flexDirection: 'column', flex: 1, minHeight: 0 }}>
                <div className="results-grid">
                  <div className="metric-card">
                    <div className="metric-label">Detected Document Type</div>
                    <div className="metric-value">
                      <span className={`type-badge ${getDocType()}`}>{getDocType()}</span>
                    </div>
                  </div>
                  <div className="metric-card">
                    <div className="metric-label">AI Confidence Score</div>
                    <div className="metric-value">
                      <span style={{ color: result.confidenceScore >= 0.85 ? 'var(--success)' : 'var(--danger)' }}>
                        {(result.confidenceScore * 100).toFixed(1)}%
                      </span>
                    </div>
                    {result.confidenceScore < 0.85 && (
                      <div style={{ marginTop: '0.5rem', fontSize: '0.8rem', color: 'var(--warning)' }}>
                        ⚠️ Triggered PendingHumanReview status
                      </div>
                    )}
                  </div>
                </div>

                <div className="json-container" dangerouslySetInnerHTML={{ __html: formatJson(result.extractedDataJson) }} />

                <div className="reasoning-box">
                  <strong style={{ color: '#a5b4fc' }}>AI Reasoning:</strong> {result.reasoning}
                  <div style={{ marginTop: '0.5rem', fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                    Backend Strategy Executed: <code>{getDocType()}ProcessingStrategy.cs</code>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </>
  );
}

export default App;
