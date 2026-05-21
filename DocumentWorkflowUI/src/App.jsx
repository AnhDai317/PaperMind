import { useState, useEffect } from 'react';
import './index.css';

const API_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

const SAMPLES = {
  invoice: `INVOICE #99812\n\nDate: 2026-05-21\nVendor: Globex Corp\n\nItems:\n- Software License: $1,500.00\n- Cloud Hosting: $250.00\n\nTotal Due: $1,750.00\nTax: 10%`,
  contract: `NON-DISCLOSURE AGREEMENT\n\nEffective Date: 2026-06-01\n\nParties:\n1. Acme Corp (Disclosing Party)\n2. Jane Smith (Receiving Party)\n\nThe Receiving Party agrees to keep all proprietary algorithms confidential. Signatures attached below.`,
  cv: `John Doe - Senior AI Engineer\n\nExperience:\n- 5 years building .NET Microservices\n- Created high-throughput document processing pipelines using MediatR.\n\nSkills: C#, React, Clean Architecture, CQRS, System.Threading.Channels.`,
  unknown: `Here is a random note I wrote on a napkin. Please remember to buy milk and eggs on the way home.`
};

function App() {
  const [activeTab, setActiveTab] = useState('extraction');
  
  // Extraction Tab State
  const [text, setText] = useState('');
  const [step, setStep] = useState(0); 
  const [result, setResult] = useState(null);
  const [error, setError] = useState(null);
  const [abortController, setAbortController] = useState(null);

  // Review Queue Tab State
  const [documents, setDocuments] = useState([]);
  const [loadingQueue, setLoadingQueue] = useState(false);

  useEffect(() => {
    if (activeTab === 'queue') {
      fetchQueue();
    }
  }, [activeTab]);

  const fetchQueue = async () => {
    setLoadingQueue(true);
    try {
      const res = await fetch(`${API_URL}/api/documents`);
      if (res.ok) {
        const data = await res.json();
        setDocuments(data);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingQueue(false);
    }
  };

  const handleProcess = async () => {
    if (!text.trim()) return;
    
    setStep(1);
    setResult(null);
    setError(null);

    const controller = new AbortController();
    setAbortController(controller);

    const timeoutId = setTimeout(() => {
      controller.abort();
    }, 30000);

    try {
      const response = await fetch(`${API_URL}/api/documents/extract`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ text }),
        signal: controller.signal
      });

      clearTimeout(timeoutId);

      if (response.ok) {
        const data = await response.json();
        setResult(data);
        setStep(2);
      } else {
        setError('Server returned an error.');
        setStep(0);
      }
    } catch (err) {
      if (err.name === 'AbortError') {
        setError('Request timed out or was cancelled by the user.');
      } else {
        setError(`Network error connecting to ${API_URL}`);
      }
      setStep(0);
    } finally {
      setAbortController(null);
    }
  };

  const handleCancel = () => {
    if (abortController) {
      abortController.abort();
    }
  };

  const handleApprove = async (id, docType) => {
    try {
      const res = await fetch(`${API_URL}/api/documents/${id}/approve`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ documentType: docType })
      });
      if (res.ok) {
        fetchQueue();
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleReject = async (id) => {
    try {
      const res = await fetch(`${API_URL}/api/documents/${id}/reject`, {
        method: 'POST'
      });
      if (res.ok) {
        fetchQueue();
      }
    } catch (err) {
      console.error(err);
    }
  };

  const getDocTypeString = (typeInt) => {
    const mapping = { 1: 'Invoice', 2: 'Contract', 3: 'CV', 0: 'Unknown' };
    return mapping[typeInt] || 'Unknown';
  };
  
  const getDocStatusString = (statusInt) => {
    const mapping = { 0: 'Received', 1: 'Processing', 2: 'Completed', 3: 'PendingHumanReview', 4: 'Failed' };
    return mapping[statusInt] || 'Unknown';
  };

  const formatJson = (jsonString) => {
    if (!jsonString) return '{}';
    try {
      const obj = typeof jsonString === 'string' ? JSON.parse(jsonString) : jsonString;
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
            <p>Clean Architecture • MediatR • Strategy Pattern • Human-in-the-Loop</p>
          </div>
          <div>
            <div className="type-badge Invoice" style={{ opacity: 0.7, fontSize: '0.8rem' }}>Backend: .NET 8 Web API</div>
          </div>
        </header>

        <div className="tabs">
          <button className={`tab-btn ${activeTab === 'extraction' ? 'active' : ''}`} onClick={() => setActiveTab('extraction')}>Auto Extraction</button>
          <button className={`tab-btn ${activeTab === 'queue' ? 'active' : ''}`} onClick={() => setActiveTab('queue')}>Review Queue 🛡️</button>
        </div>

        {activeTab === 'extraction' && (
          <div className="main-content">
            <div className="glass-panel left">
              <h2 className="panel-title">📝 Input Data</h2>
              <div className="fast-actions">
                <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)', display: 'flex', alignItems: 'center' }}>Test Data:</span>
                <button className="action-btn" onClick={() => setText(SAMPLES.invoice)}>Load Invoice</button>
                <button className="action-btn" onClick={() => setText(SAMPLES.contract)}>Load Contract</button>
                <button className="action-btn" onClick={() => setText(SAMPLES.cv)}>Load CV</button>
                <button className="action-btn" onClick={() => setText(SAMPLES.unknown)}>Load Ambiguous</button>
              </div>

              <textarea 
                value={text} 
                onChange={(e) => setText(e.target.value)}
                placeholder="Paste raw text here... Our Smart Mocking or Gemini AI will detect the type."
              />
              
              {error && <div style={{ color: 'var(--danger)', marginTop: '1rem', fontSize: '0.9rem' }}>{error}</div>}

              <div style={{ display: 'flex', gap: '1rem' }}>
                <button className="submit-btn" style={{ flex: 1 }} onClick={handleProcess} disabled={step === 1 || !text.trim()}>
                  {step === 1 ? '🧠 AI Processing & Routing...' : '🚀 Extract & Route Document'}
                </button>
                {step === 1 && (
                  <button className="submit-btn" style={{ background: 'var(--danger)', flex: 0.3 }} onClick={handleCancel}>
                    Cancel
                  </button>
                )}
              </div>
            </div>

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
                        <span className={`type-badge ${getDocTypeString(result.type)}`}>{getDocTypeString(result.type)}</span>
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
                  </div>
                </div>
              )}
            </div>
          </div>
        )}

        {activeTab === 'queue' && (
          <div className="main-content" style={{ flexDirection: 'column' }}>
            <div className="glass-panel" style={{ flex: 1 }}>
              <h2 className="panel-title">🛡️ Human-in-the-Loop Review Queue</h2>
              {loadingQueue ? (
                <div style={{ padding: '2rem', textAlign: 'center' }}>Loading documents...</div>
              ) : (
                <div className="queue-list">
                  {documents.length === 0 && <div style={{ color: 'var(--text-muted)' }}>No documents in memory repository.</div>}
                  {documents.map(doc => (
                    <div key={doc.id} className="queue-item" style={{ borderLeft: `4px solid ${getDocStatusString(doc.status) === 'PendingHumanReview' ? 'var(--warning)' : 'var(--success)'}` }}>
                      <div className="queue-item-header">
                        <div>
                          <strong>ID:</strong> {doc.id} <br/>
                          <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>Status: {getDocStatusString(doc.status)}</span>
                        </div>
                        <div style={{ textAlign: 'right' }}>
                          <span className={`type-badge ${getDocTypeString(doc.type)}`}>{getDocTypeString(doc.type)}</span><br/>
                          <span style={{ fontSize: '0.85rem' }}>Confidence: {(doc.confidenceScore * 100).toFixed(1)}%</span>
                        </div>
                      </div>
                      <div style={{ background: 'rgba(0,0,0,0.3)', padding: '1rem', borderRadius: '8px', fontSize: '0.9rem', color: '#e2e8f0', fontFamily: 'monospace' }}>
                        {doc.rawText.substring(0, 100)}...
                      </div>
                      
                      <div style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                        <strong>Audit Trail:</strong>
                        <ul style={{ paddingLeft: '1.2rem', marginTop: '0.5rem' }}>
                          {doc.auditTrail.map((log, idx) => (
                            <li key={idx}>{log}</li>
                          ))}
                        </ul>
                      </div>

                      {getDocStatusString(doc.status) === 'PendingHumanReview' && (
                        <div className="queue-actions">
                          <span style={{ fontSize: '0.9rem', alignSelf: 'center', marginRight: '1rem' }}>Approve As:</span>
                          <button className="btn-approve" onClick={() => handleApprove(doc.id, 'Invoice')}>Invoice</button>
                          <button className="btn-approve" onClick={() => handleApprove(doc.id, 'Contract')}>Contract</button>
                          <button className="btn-approve" onClick={() => handleApprove(doc.id, 'CV')}>CV</button>
                          <button className="btn-reject" onClick={() => handleReject(doc.id)}>Reject</button>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </>
  );
}

export default App;
