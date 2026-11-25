import { useEffect, useState } from "react";
import "./App.css";
import "./CommandCentre.css";
import AvlHistoryPanel from "./AvlHistoryPanel";
import MeshNetworkPanel from "./MeshNetworkPanel";
// Smart-X IoT Platform
// Part 1: Sensor registration, telemetry, search, performance and uploads.
// Part 2: FIFO processing, priority alerts, device commands, undo and recommendations.

const API = "http://localhost:5090/api";

async function apiRequest(path, options = {}) {
  const response = await fetch(`${API}${path}`, options);
  const text = await response.text();

  let data;
  try {
    data = text ? JSON.parse(text) : {};
  } catch {
    data = { message: text };
  }

  if (!response.ok) {
    throw new Error(data.message || `API error ${response.status}`);
  }

  return data;
}

function postJson(path, body) {
  return apiRequest(path, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
}

function formatDate(value) {
  return value ? new Date(value).toLocaleString() : "Not available";
}

function App() {
  // Navigation between the three Smart-X pillars
  const [activePage, setActivePage] = useState("telemetry");

  // Part 1: Main dashboard data
  const [sensors, setSensors] = useState([]);
  const [telemetry, setTelemetry] = useState([]);
  const [statistics, setStatistics] = useState({});
  const [loading, setLoading] = useState(false);
  const [apiOnline, setApiOnline] = useState(false);
  const [lastUpdated, setLastUpdated] = useState(null);

  // Part 1: Sensor registration
  const [newSensor, setNewSensor] = useState({
    name: "",
    macAddress: "",
    location: "",
    category: "Temperature",
  });

  // Part 1: Telemetry generation
  const [selectedSensorId, setSelectedSensorId] = useState("");
  const [batchSize, setBatchSize] = useState(10);
  const [lastInsertionTime, setLastInsertionTime] = useState(null);

  // Part 1: Historical search
  const [searchTerm, setSearchTerm] = useState("");
  const [searchResults, setSearchResults] = useState([]);
  const [searchMessage, setSearchMessage] = useState("");
  const [lastSearchTime, setLastSearchTime] = useState(null);

  // Part 1: Performance testing
  const [performanceSize, setPerformanceSize] = useState(1000);
  const [performanceResult, setPerformanceResult] = useState(null);

  // Part 1: File uploads
  const [selectedFile, setSelectedFile] = useState(null);
  const [previewUrl, setPreviewUrl] = useState("");

  // Shared messages and action state
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  // Part 2: Dictionary-based device registry
  const [operations, setOperations] = useState({
    total: 0,
    devices: [],
    disconnectedDeviceIds: [],
  });

  const [selectedDevice, setSelectedDevice] = useState("1");
  const [commandAction, setCommandAction] = useState("restart");

  // Part 2: Stack-based command history
  const [commandHistory, setCommandHistory] = useState({
    undoCount: 0,
    commands: [],
  });

  // Part 2: Queue<T> telemetry processing
  const [fifoQueue, setFifoQueue] = useState({
    pendingCount: 0,
    readings: [],
  });

  const [queueForm, setQueueForm] = useState({
    sensorId: "1",
    temperature: "36",
    humidity: "50",
    batteryLevel: "80",
  });

  // Part 2: PriorityQueue and HashSet alerts
  const [alerts, setAlerts] = useState({
    pendingCount: 0,
    uniqueActiveCount: 0,
    alerts: [],
  });

  const [alertForm, setAlertForm] = useState({
    sensorId: "1",
    type: "Temperature",
    message: "High temperature detected",
    priority: "1",
  });

  // Part 2: SortedDictionary event history
  const [eventHistory, setEventHistory] = useState({
    total: 0,
    events: [],
  });

  const [eventSensorFilter, setEventSensorFilter] = useState("");

  // Part 2: Rule-based predictive recommendations
  const [recommendations, setRecommendations] = useState({
    total: 0,
    recommendations: [],
  });

  // Load Part 1 sensor and telemetry information
  async function loadDashboard() {
    setLoading(true);

    try {
      const [sensorData, telemetryData, statsData] = await Promise.all([
        apiRequest("/Sensors"),
        apiRequest("/Telemetry"),
        apiRequest("/Telemetry/statistics"),
      ]);

      setSensors(sensorData);
      setTelemetry(telemetryData);
      setStatistics(statsData);
      setApiOnline(true);
      setLastUpdated(new Date());

      if (sensorData.length > 0) {
        setSelectedSensorId((previous) =>
          previous || String(sensorData[0].id)
        );
      }
    } catch (err) {
      setApiOnline(false);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  // Load all Part 2 endpoints together
  async function loadOperations() {
    try {
      const [
        deviceData,
        historyData,
        alertData,
        eventData,
        recommendationData,
        queueData,
        searchIntelligenceData,
      ] = await Promise.all([
        apiRequest("/operations/devices"),
        apiRequest("/operations/commands/history"),
        apiRequest("/operations/alerts"),
        apiRequest(
          `/operations/history?limit=50${
            eventSensorFilter
              ? `&sensorId=${encodeURIComponent(eventSensorFilter)}`
              : ""
          }`
        ),
        apiRequest("/operations/recommendations"),
        apiRequest("/Telemetry/queue"),
        apiRequest("/search-intelligence/recommendations"),
      ]);

      setOperations(deviceData);
      setCommandHistory(historyData);
      setAlerts(alertData);
      setEventHistory(eventData);
     

const combinedRecommendations = [
  ...(recommendationData.recommendations || []),
  ...(searchIntelligenceData.recommendations || []),
];

setRecommendations({
  total: combinedRecommendations.length,
  recommendations: combinedRecommendations,
});


      setFifoQueue(queueData);

      if (deviceData.devices?.length > 0) {
        setSelectedDevice((previous) => {
          const exists = deviceData.devices.some(
            (device) => String(device.id) === previous
          );

          return exists
            ? previous
            : String(deviceData.devices[0].id);
        });
      }
    } catch (err) {
      setError(err.message);
    }
  }

  useEffect(() => {
    loadDashboard();
  }, []);

  useEffect(() => {
    if (activePage === "commands") {
      loadOperations();
    }
  }, [activePage, eventSensorFilter]);

  // Shared API action handler
  async function runAction(action, successText, refreshOperations = false) {
    setBusy(true);
    setError("");
    setMessage("");

    try {
      const result = await action();

      setMessage(result?.message || successText);

      if (refreshOperations) {
        await loadOperations();
      }

      return result;
    } catch (err) {
      setError(err.message);
      return null;
    } finally {
      setBusy(false);
    }
  }

  // Register a new IoT sensor
  async function registerSensor(event) {
    event.preventDefault();

    if (
      !newSensor.name.trim() ||
      !newSensor.macAddress.trim() ||
      !newSensor.location.trim()
    ) {
      setError("Please complete all sensor details.");
      return;
    }

    const result = await runAction(
      () =>
        postJson("/Sensors", {
          ...newSensor,
          isOnline: true,
        }),
      "Sensor registered successfully."
    );

    if (result) {
      setNewSensor({
        name: "",
        macAddress: "",
        location: "",
        category: "Temperature",
      });

      await loadDashboard();
    }
  }

  // Generate and transmit a batch of telemetry readings
  async function generateAndTransmitBatch() {
    if (!selectedSensorId) {
      setError("Select a sensor first.");
      return;
    }

    const result = await runAction(async () => {
      const generated = await apiRequest(
        `/Generator/batch/${selectedSensorId}/${batchSize}`
      );

      const transmitted = await postJson(
        "/Telemetry/batch",
        generated.readings
      );

      setLastInsertionTime(
        transmitted.insertionTimeMilliseconds
      );

      return {
        message: `Successfully transmitted ${transmitted.batchSize} readings.`,
      };
    }, "Telemetry transmitted.");

    if (result) {
      await loadDashboard();
    }
  }

  // Search telemetry using the existing Part 1 search API
  async function searchHistoricalTelemetry(event) {
    event.preventDefault();

    if (!searchTerm.trim()) {
      setSearchMessage("Enter a sensor ID.");
      return;
    }

    await runAction(async () => {
      const data = await apiRequest(
        `/Search?term=${encodeURIComponent(searchTerm.trim())}`
      );

      // Record searches for existing sensors in the
// history-aware recommendation engine.
const searchedSensorId = Number(searchTerm.trim());

if (
  Number.isInteger(searchedSensorId) &&
  searchedSensorId > 0 &&
  sensors.some((sensor) => sensor.id === searchedSensorId)
) {
  await postJson("/search-intelligence/record", {
    sensorId: searchedSensorId,
  });
}

      setSearchResults(data.results || []);
      setLastSearchTime(data.searchTimeMilliseconds);

      setSearchMessage(
        `${data.resultCount} result(s) found using ${data.algorithm}.`
      );

      return { message: "Search completed." };
    }, "Search completed.");
  }

  // Measure List<T> insertion and search performance
  async function runPerformanceTest() {
    await runAction(async () => {
      const data = await apiRequest(
        `/Analysis/performance/${performanceSize}`
      );

      setPerformanceResult(data);

      return { message: "Performance test completed." };
    }, "Performance test completed.");
  }

  // Upload image or telemetry evidence
  async function uploadFile() {
    if (!selectedFile) {
      setError("Select a file first.");
      return;
    }

    const formData = new FormData();
    formData.append("file", selectedFile);

    const result = await runAction(
      () =>
        apiRequest("/Files/upload", {
          method: "POST",
          body: formData,
        }),
      "File uploaded successfully."
    );

    if (result) {
      setSelectedFile(null);
      setPreviewUrl("");
    }
  }

  // Send device commands using the Dictionary registry
  async function sendCommand() {
    await runAction(
      () =>
        postJson("/operations/commands", {
          deviceId: Number(selectedDevice),
          action: commandAction,
        }),
      "Command sent.",
      true
    );
  }

  // Undo the most recent command using Stack<T>
  async function undoCommand() {
    await runAction(
      () =>
        apiRequest("/operations/commands/undo", {
          method: "POST",
        }),
      "Last command undone.",
      true
    );
  }

  // Enqueue telemetry for FIFO processing
  async function enqueueTelemetry() {
    await runAction(
      () =>
        postJson(
          "/Telemetry/queue",
          Object.fromEntries(
            Object.entries(queueForm).map(([key, value]) => [
              key,
              Number(value),
            ])
          )
        ),
      "Telemetry reading added to FIFO queue.",
      true
    );
  }

  // Dequeue the oldest reading first
  async function processNextReading() {
    const result = await runAction(
      () =>
        apiRequest("/Telemetry/queue/process-next", {
          method: "POST",
        }),
      "Oldest telemetry reading processed.",
      true
    );

    if (result) {
      await loadDashboard();
    }
  }

  // Add a unique alert to the priority queue
  async function addAlert() {
    await runAction(
      () =>
        postJson("/operations/alerts", {
          sensorId: Number(alertForm.sensorId),
          type: alertForm.type,
          message: alertForm.message,
          priority: Number(alertForm.priority),
        }),
      "Alert submitted.",
      true
    );
  }

  // Process the most urgent alert first
  async function processNextAlert() {
    await runAction(
      () =>
        apiRequest("/operations/alerts/process-next", {
          method: "POST",
        }),
      "Highest-priority alert processed.",
      true
    );
  }

  const onlineSensors = sensors.filter(
    (sensor) => sensor.isOnline
  ).length;

  const latestTelemetry = telemetry.slice(-12).reverse();

  return (
    <div className="app">
      {/* Main navigation */}
      <nav className="main-navigation">
        <button
          className={`nav-button ${
            activePage === "telemetry" ? "selected" : ""
          }`}
          onClick={() => setActivePage("telemetry")}
        >
          Sensor Data &amp; Telemetry
        </button>

        <button
          className={`nav-button ${
            activePage === "commands" ? "selected" : ""
          }`}
          onClick={() => setActivePage("commands")}
        >
          Real-Time Command Stream
        </button>

        <button
          className={`nav-button ${
            activePage === "topology" ? "selected" : ""
          }`}
          onClick={() => setActivePage("topology")}
        >
          Network Topology
        </button>
      </nav>

      {/* Application header */}
      <header className="header">
        <div className="brand-area">
          <div className="brand-icon">SX</div>

          <div>
            <h1>Smart-X</h1>
            <p>IoT Monitoring &amp; Telemetry Platform</p>
          </div>
        </div>

        <div className="header-actions">
          <span
            className={
              apiOnline
                ? "status-badge"
                : "status-badge status-offline"
            }
          >
            {apiOnline ? "● API Online" : "● API Offline"}
          </span>

          <button
            className="secondary-button"
            onClick={() => {
              loadDashboard();
              if (activePage === "commands") loadOperations();
            }}
            disabled={loading}
          >
            Refresh Dashboard
          </button>
        </div>
      </header>

      <main className="dashboard">
        {/* Shared success and error messages */}
        {error && (
          <div className="error-banner" role="alert">
            {error}
          </div>
        )}

        {message && (
          <div className="success-message" role="status">
            {message}
          </div>
        )}

        {/* ==========================================
            PILLAR 1: SENSOR DATA AND TELEMETRY
           ========================================== */}
        {activePage === "telemetry" && (
          <>
            <section className="hero-panel">
              <div>
                <span className="eyebrow">
                  SMART-X CONTROL CENTRE
                </span>

                <h2>Sensor Telemetry Dashboard</h2>

                <p>
                  Register IoT devices, simulate telemetry,
                  monitor ingestion, search historical data
                  and analyse system performance.
                </p>
              </div>

              <div className="hero-meta">
                <span>
                  {apiOnline ? "● Connected" : "● Disconnected"}
                </span>

                <small>
                  Last updated: {formatDate(lastUpdated)}
                </small>
              </div>
            </section>

            {/* Architecture overview */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">
                    SYSTEM ARCHITECTURE
                  </span>
                  <h2>Three Smart-X Pillars</h2>
                </div>
              </div>

              <div className="pillar-grid">
                <article className="pillar-card active-pillar">
                  <div className="pillar-top">
                    <span className="pillar-number">01</span>
                    <span className="active-chip">ACTIVE</span>
                  </div>

                  <h3>Sensor Data Ingestion &amp; Telemetry</h3>

                  <p>
                    Register sensors, generate readings,
                    transmit batches and search telemetry.
                  </p>

                  <div className="pillar-footer">Part 1</div>
                </article>

                <article className="pillar-card active-pillar">
                  <div className="pillar-top">
                    <span className="pillar-number">02</span>
                    <span className="active-chip">ACTIVE</span>
                  </div>

                  <h3>Real-Time Command Stream</h3>

                  <p>
                    Manage device commands, process priority
                    alerts and view automated recommendations.
                  </p>

                  <div className="pillar-footer">Part 2</div>
                </article>

                <article className="pillar-card">
                  <div className="pillar-top">
                    <span className="pillar-number">03</span>
                    <span className="future-chip">FUTURE</span>
                  </div>

                  <h3>Network Topology &amp; Mesh Routing</h3>

                  <p>
                    Future network connectivity and routing
                    visualisation.
                  </p>

                  <div className="pillar-footer">Final POE</div>
                </article>
              </div>
            </section>

            {/* Telemetry statistics */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">LIVE OVERVIEW</span>
                  <h2>Telemetry Pipeline</h2>
                </div>
              </div>

              <div className="stats-grid">
                {[
                  ["Total Sensors", sensors.length],
                  ["Generated", statistics.generatedCount ?? 0],
                  ["Transmitted", statistics.transmittedCount ?? 0],
                  ["Received", statistics.receivedCount ?? 0],
                  ["Records Stored", statistics.totalStored ?? 0],
                  ["Online Sensors", onlineSensors],
                ].map(([label, value]) => (
                  <div className="stat-card" key={label}>
                    <div>
                      <p>{label}</p>
                      <strong>{value}</strong>
                    </div>
                  </div>
                ))}
              </div>
            </section>

            {/* Sensor registration */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">DEVICE MANAGEMENT</span>
                  <h2>Register Sensor</h2>
                  <p>Add a new IoT sensor to the system.</p>
                </div>
              </div>

              <div className="panel-card">
                <form onSubmit={registerSensor}>
                  <div className="registration-grid">
                    {[
                      ["name", "Sensor Name"],
                      ["macAddress", "MAC Address"],
                      ["location", "Deployment Location"],
                    ].map(([field, label]) => (
                      <label key={field}>
                        {label}
                        <input
                          value={newSensor[field]}
                          onChange={(event) =>
                            setNewSensor({
                              ...newSensor,
                              [field]: event.target.value,
                            })
                          }
                          required
                        />
                      </label>
                    ))}

                    <label>
                      Sensor Category
                      <select
                        value={newSensor.category}
                        onChange={(event) =>
                          setNewSensor({
                            ...newSensor,
                            category: event.target.value,
                          })
                        }
                      >
                        {[
                          "Temperature",
                          "Humidity",
                          "Environmental",
                          "Power",
                          "Consumption",
                          "Actuator",
                        ].map((category) => (
                          <option key={category} value={category}>
                            {category}
                          </option>
                        ))}
                      </select>
                    </label>
                  </div>

                  <button
                    className="primary-button"
                    type="submit"
                    disabled={busy}
                  >
                    Register Sensor
                  </button>
                </form>
              </div>
            </section>

            {/* Batch telemetry generation */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">
                    TELEMETRY SIMULATION
                  </span>
                  <h2>Generate &amp; Transmit</h2>
                  <p>
                    Generate simulated readings and send
                    them to the ingestion API.
                  </p>
                </div>
              </div>

              <div className="panel-card">
                <div className="generator-controls">
                  <label>
                    Sensor
                    <select
                      value={selectedSensorId}
                      onChange={(event) =>
                        setSelectedSensorId(event.target.value)
                      }
                    >
                      <option value="">Select sensor</option>

                      {sensors.map((sensor) => (
                        <option key={sensor.id} value={sensor.id}>
                          {sensor.name} — ID {sensor.id}
                        </option>
                      ))}
                    </select>
                  </label>

                  <label>
                    Batch Size
                    <select
                      value={batchSize}
                      onChange={(event) =>
                        setBatchSize(Number(event.target.value))
                      }
                    >
                      {[10, 100, 1000, 5000].map((size) => (
                        <option key={size} value={size}>
                          {size} readings
                        </option>
                      ))}
                    </select>
                  </label>

                  <button
                    className="primary-button"
                    disabled={busy || !selectedSensorId}
                    onClick={generateAndTransmitBatch}
                  >
                    Generate &amp; Transmit
                  </button>
                </div>

                {lastInsertionTime !== null && (
                  <p className="info-message">
                    Last insertion time:{" "}
                    {Number(lastInsertionTime).toFixed(4)} ms
                  </p>
                )}
              </div>
            </section>

            {/* Data structure performance testing */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">DATA STRUCTURES</span>
                  <h2>Performance Test</h2>
                  <p>
                    Compare insertion and search performance
                    for different dataset sizes.
                  </p>
                </div>
              </div>

              <div className="panel-card">
                <div className="performance-controls">
                  <label>
                    Test Size
                    <select
                      value={performanceSize}
                      onChange={(event) =>
                        setPerformanceSize(Number(event.target.value))
                      }
                    >
                      {[10, 100, 1000, 5000, 10000].map((size) => (
                        <option key={size} value={size}>
                          {size} records
                        </option>
                      ))}
                    </select>
                  </label>

                  <button
                    className="primary-button"
                    onClick={runPerformanceTest}
                    disabled={busy}
                  >
                    Run Test
                  </button>
                </div>

                {performanceResult && (
                  <div className="performance-results">
                    {Object.entries(performanceResult).map(
                      ([key, value]) => (
                        <div key={key}>
                          <span>{key}</span>
                          <strong>
                            {typeof value === "object"
                              ? JSON.stringify(value)
                              : String(value)}
                          </strong>
                        </div>
                      )
                    )}
                  </div>
                )}
              </div>
            </section>

            {/* Latest telemetry readings */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">LIVE TELEMETRY</span>
                  <h2>Latest Readings</h2>
                  <p>Showing the 12 newest stored readings.</p>
                </div>
              </div>

              {loading ? (
                <p className="loading-text">Loading telemetry...</p>
              ) : (
                <div className="telemetry-grid">
                  {latestTelemetry.length === 0 && (
                    <div className="empty-state">
                      No telemetry readings available.
                    </div>
                  )}

                  {latestTelemetry.map((reading, index) => (
                    <article
                      className="telemetry-card"
                      key={reading.id || index}
                    >
                      <div className="telemetry-card-header">
                        <div>
                          <span className="sensor-tag">SENSOR</span>
                          <h3>#{reading.sensorId}</h3>
                        </div>

                        <span className="live-dot">LIVE</span>
                      </div>

                      <div className="metric-list">
                        <div className="metric-row">
                          <span>Temperature</span>
                          <strong>
                            {reading.temperature} °C
                          </strong>
                        </div>

                        <div className="metric-row">
                          <span>Humidity</span>
                          <strong>{reading.humidity}%</strong>
                        </div>

                        <div className="metric-row">
                          <span>Battery</span>
                          <strong>{reading.batteryLevel}%</strong>
                        </div>
                      </div>

                      <p className="recorded-time">
                        {formatDate(reading.recordedAt)}
                      </p>
                    </article>
                  ))}
                </div>
              )}
            </section>

            {/* Historical telemetry search */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">DATA SEARCH</span>
                  <h2>Historical Telemetry</h2>
                  <p>Search previous readings by sensor ID.</p>
                </div>
              </div>

              <div className="panel-card">
                <form onSubmit={searchHistoricalTelemetry}>
                  <div className="search-controls">
                    <input
                      type="text"
                      placeholder="Enter sensor ID"
                      value={searchTerm}
                      onChange={(event) =>
                        setSearchTerm(event.target.value)
                      }
                    />

                    <button
                      className="primary-button"
                      type="submit"
                      disabled={busy}
                    >
                      Search History
                    </button>
                  </div>
                </form>

                {searchMessage && (
                  <div className="info-message">
                    {searchMessage}
                    {lastSearchTime !== null &&
                      ` · ${Number(lastSearchTime).toFixed(4)} ms`}
                  </div>
                )}

                <div className="search-results">
                  {searchResults.slice(0, 20).map((reading, index) => (
                    <div
                      className="search-result"
                      key={reading.id || index}
                    >
                      <div>
                        <span>Sensor</span>
                        <strong>#{reading.sensorId}</strong>
                      </div>

                      <div>
                        <span>Temperature</span>
                        <strong>{reading.temperature} °C</strong>
                      </div>

                      <div>
                        <span>Humidity</span>
                        <strong>{reading.humidity}%</strong>
                      </div>

                      <div>
                        <span>Battery</span>
                        <strong>{reading.batteryLevel}%</strong>
                      </div>

                      <div>
                        <span>Recorded</span>
                        <strong>
                          {formatDate(reading.recordedAt)}
                        </strong>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </section>

{/* PART 3: AVL historical telemetry dashboard */}
<AvlHistoryPanel />


            {/* File upload */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">FILE MANAGEMENT</span>
                  <h2>Media &amp; Log Upload</h2>
                  <p>
                    Upload TXT, LOG, CSV and image evidence.
                  </p>
                </div>
              </div>

              <div className="upload-card">
                <div className="upload-icon">↑</div>

                <h3>Upload Sensor Evidence</h3>

                <p>TXT · LOG · CSV · JPG · JPEG · PNG</p>

                <label className="file-select-button">
                  Select File

                  <input
                    type="file"
                    accept=".txt,.log,.csv,.jpg,.jpeg,.png"
                    onChange={(event) => {
                      const file = event.target.files?.[0] || null;

                      setSelectedFile(file);

                      if (previewUrl) {
                        URL.revokeObjectURL(previewUrl);
                      }

                      setPreviewUrl(
                        file?.type.startsWith("image/")
                          ? URL.createObjectURL(file)
                          : ""
                      );
                    }}
                  />
                </label>

                {selectedFile && (
                  <p className="selected-file">
                    Selected: {selectedFile.name}
                  </p>
                )}

                {previewUrl && (
                  <div className="image-preview-container">
                    <img
                      className="image-preview"
                      src={previewUrl}
                      alt="Selected file preview"
                    />
                  </div>
                )}

                <button
                  className="primary-button upload-action"
                  onClick={uploadFile}
                  disabled={!selectedFile || busy}
                >
                  Upload File
                </button>
              </div>
            </section>

            {/* Registered sensors */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">DEVICE REGISTRY</span>
                  <h2>Registered Sensors</h2>
                  <p>
                    Devices available to the telemetry system.
                  </p>
                </div>
              </div>

              <div className="sensor-grid">
                {sensors.map((sensor) => (
                  <article className="device-card" key={sensor.id}>
                    <div className="device-card-top">
                      <span className="device-number">
                        #{sensor.id}
                      </span>

                      <span
                        className={`device-status ${
                          sensor.isOnline ? "online" : "offline"
                        }`}
                      >
                        {sensor.isOnline ? "ONLINE" : "OFFLINE"}
                      </span>
                    </div>

                    <h3>{sensor.name}</h3>

                    <div className="device-details">
                      <div>
                        <span>Category</span>
                        <strong>{sensor.category}</strong>
                      </div>

                      <div>
                        <span>Location</span>
                        <strong>{sensor.location}</strong>
                      </div>

                      <div>
                        <span>MAC Address</span>
                        <strong>{sensor.macAddress}</strong>
                      </div>
                    </div>
                  </article>
                ))}
              </div>
            </section>
          </>
        )}

        {/* ==========================================
            PILLAR 2: REAL-TIME COMMAND STREAM
           ========================================== */}
        {activePage === "commands" && (
          <>
            <section className="hero-panel">
              <div>
                <span className="eyebrow">
                  PART 2 · OPERATIONS
                </span>

                <h2>Real-Time Command Stream</h2>

                <p>
                  Manage connected IoT devices, process
                  telemetry and priority alerts, undo commands
                  and review automated recommendations.
                </p>
              </div>

              <button
                className="secondary-button"
                onClick={loadOperations}
              >
                Refresh Operations
              </button>
            </section>

            {/* Part 2 overview */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">LIVE OVERVIEW</span>
                  <h2>Processing Overview</h2>
                </div>
              </div>

              <div className="stats-grid">
                {[
                  ["Managed Devices", operations.total],
                  [
                    "Disconnected",
                    operations.disconnectedDeviceIds?.length ?? 0,
                  ],
                  ["Priority Alerts", alerts.pendingCount],
                  ["Queued Telemetry", fifoQueue.pendingCount],
                  ["Undo Stack", commandHistory.undoCount],
                  ["Recommendations", recommendations.total],
                ].map(([label, value]) => (
                  <div className="stat-card" key={label}>
                    <div>
                      <p>{label}</p>
                      <strong>{value ?? 0}</strong>
                    </div>
                  </div>
                ))}
              </div>
            </section>

            {/* Dictionary: fast lookup by device ID
                Stack: last-in-first-out command undo */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">
                    DICTIONARY + STACK
                  </span>

                  <h2>Device Controls &amp; Undo</h2>

                  <p>
                    Select a device, send a command and
                    undo the latest command.
                  </p>
                </div>
              </div>

              <div className="panel-card">
                <div className="operations-controls">
                  <label>
                    Device
                    <select
                      value={selectedDevice}
                      onChange={(event) =>
                        setSelectedDevice(event.target.value)
                      }
                    >
                      {(operations.devices || []).map((device) => (
                        <option
                          key={device.id}
                          value={device.id}
                        >
                          #{device.id} · {device.name}
                        </option>
                      ))}
                    </select>
                  </label>

                  <label>
                    Command
                    <select
                      value={commandAction}
                      onChange={(event) =>
                        setCommandAction(event.target.value)
                      }
                    >
                      <option value="on">Turn On</option>
                      <option value="off">Turn Off</option>
                      <option value="restart">Restart</option>
                      <option value="disconnect">Disconnect</option>
                      <option value="connect">Reconnect</option>
                    </select>
                  </label>

                  <button
                    className="primary-button"
                    onClick={sendCommand}
                    disabled={busy || !selectedDevice}
                  >
                    Send Command
                  </button>

                  <button
                    className="secondary-button"
                    onClick={undoCommand}
                    disabled={busy || !commandHistory.undoCount}
                  >
                    Undo Last Command
                  </button>
                </div>

                <div className="operations-card-grid">
                  {(operations.devices || []).map((device) => (
                    <div
                      className="operations-item"
                      key={device.id}
                    >
                      <div className="operations-item-top">
                        <strong>
                          #{device.id} {device.name}
                        </strong>

                        <span
                          className={`device-status ${
                            device.status === "Online"
                              ? "online"
                              : "offline"
                          }`}
                        >
                          {device.status}
                        </span>
                      </div>

                      <small>
                        {device.isOn ? "Powered On" : "Powered Off"}
                        {" · "}
                        Updated {formatDate(device.updatedAt)}
                      </small>
                    </div>
                  ))}
                </div>

                <h3>Command History / Undo Stack</h3>

                <div className="operations-list">
                  {(commandHistory.commands || []).length === 0 ? (
                    <p className="muted">
                      No commands waiting to be undone.
                    </p>
                  ) : (
                    commandHistory.commands.map((command, index) => (
                      <div
                        className="operations-list-row"
                        key={index}
                      >
                        <strong>
                          {command.action?.toUpperCase()}
                          {" · "}
                          Device {command.deviceId}
                        </strong>

                        <span>
                          {formatDate(command.timestamp)}
                        </span>
                      </div>
                    ))
                  )}
                </div>
              </div>
            </section>

            {/* Queue<T>: First In, First Out telemetry */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">QUEUE&lt;T&gt; · FIFO</span>

                  <h2>Telemetry Processing Queue</h2>

                  <p>
                    Add telemetry readings and process
                    the oldest reading first.
                  </p>
                </div>
              </div>

              <div className="panel-card">
                <div className="operations-controls">
                  {[
                    ["sensorId", "Sensor ID"],
                    ["temperature", "Temperature °C"],
                    ["humidity", "Humidity %"],
                    ["batteryLevel", "Battery %"],
                  ].map(([field, label]) => (
                    <label key={field}>
                      {label}

                      <input
                        type="number"
                        step={
                          field === "temperature" ? "0.1" : "1"
                        }
                        value={queueForm[field]}
                        onChange={(event) =>
                          setQueueForm({
                            ...queueForm,
                            [field]: event.target.value,
                          })
                        }
                      />
                    </label>
                  ))}

                  <button
                    className="primary-button"
                    onClick={enqueueTelemetry}
                    disabled={busy}
                  >
                    Enqueue Reading
                  </button>

                  <button
                    className="secondary-button"
                    onClick={processNextReading}
                    disabled={busy || !fifoQueue.pendingCount}
                  >
                    Process Next (FIFO)
                  </button>
                </div>

                <p className="muted">
                  Pending readings: {fifoQueue.pendingCount ?? 0}
                </p>

                <div className="operations-list">
                  {(fifoQueue.readings || []).length === 0 ? (
                    <p className="muted">
                      The telemetry queue is empty.
                    </p>
                  ) : (
                    fifoQueue.readings.map((reading, index) => (
                      <div
                        className="operations-list-row"
                        key={index}
                      >
                        <strong>
                          #{index + 1} · Sensor {reading.sensorId}
                        </strong>

                        <span>
                          {reading.temperature} °C ·{" "}
                          {reading.humidity}% humidity ·{" "}
                          {reading.batteryLevel}% battery
                        </span>
                      </div>
                    ))
                  )}
                </div>
              </div>
            </section>

            {/* PriorityQueue: urgent alerts processed first
                HashSet: prevents duplicate active alerts */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">
                    PRIORITY QUEUE + HASHSET
                  </span>

                  <h2>Priority Alerts</h2>

                  <p>
                    Process critical alerts before warnings
                    and prevent duplicate active alerts.
                  </p>
                </div>
              </div>

              <div className="panel-card">
                <div className="operations-controls">
                  <label>
                    Sensor ID
                    <input
                      type="number"
                      min="1"
                      value={alertForm.sensorId}
                      onChange={(event) =>
                        setAlertForm({
                          ...alertForm,
                          sensorId: event.target.value,
                        })
                      }
                    />
                  </label>

                  <label>
                    Alert Type
                    <input
                      value={alertForm.type}
                      onChange={(event) =>
                        setAlertForm({
                          ...alertForm,
                          type: event.target.value,
                        })
                      }
                    />
                  </label>

                  <label>
                    Message
                    <input
                      value={alertForm.message}
                      onChange={(event) =>
                        setAlertForm({
                          ...alertForm,
                          message: event.target.value,
                        })
                      }
                    />
                  </label>

                  <label>
                    Priority
                    <select
                      value={alertForm.priority}
                      onChange={(event) =>
                        setAlertForm({
                          ...alertForm,
                          priority: event.target.value,
                        })
                      }
                    >
                      <option value="1">1 · Critical</option>
                      <option value="2">2 · Warning</option>
                      <option value="3">3 · Low</option>
                    </select>
                  </label>

                  <button
                    className="primary-button"
                    onClick={addAlert}
                    disabled={busy}
                  >
                    Add Alert
                  </button>

                  <button
                    className="secondary-button"
                    onClick={processNextAlert}
                    disabled={busy || !alerts.pendingCount}
                  >
                    Process Highest Priority
                  </button>
                </div>

                <p className="muted">
                  Pending: {alerts.pendingCount ?? 0}
                  {" · "}
                  Unique active: {alerts.uniqueActiveCount ?? 0}
                </p>

                <div className="operations-list">
                  {(alerts.alerts || []).length === 0 ? (
                    <p className="muted">
                      No active alerts.
                    </p>
                  ) : (
                    alerts.alerts.map((alert, index) => (
                      <div
                        className="operations-list-row"
                        key={index}
                      >
                        <strong>
                          <span
                            className={`priority-pill priority-${alert.priority}`}
                          >
                            P{alert.priority}
                          </span>
                          {" "}
                          {alert.type} · Sensor {alert.sensorId}
                        </strong>

                        <span>{alert.message}</span>
                      </div>
                    ))
                  )}
                </div>
              </div>
            </section>

            {/* SortedDictionary<DateTime, Event>:
                stores events in chronological order */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">
                    SORTEDDICTIONARY
                  </span>

                  <h2>Chronological Event History</h2>

                  <p>
                    Review device commands and alerts,
                    optionally filtered by sensor ID.
                  </p>
                </div>
              </div>

              <div className="panel-card">
                <div className="operations-controls">
                  <label>
                    Filter Sensor ID
                    <input
                      type="number"
                      min="1"
                      placeholder="All sensors"
                      value={eventSensorFilter}
                      onChange={(event) =>
                        setEventSensorFilter(event.target.value)
                      }
                    />
                  </label>

                  <button
                    className="secondary-button"
                    onClick={() => setEventSensorFilter("")}
                  >
                    Clear Filter
                  </button>
                </div>

                <div className="operations-list">
                  {(eventHistory.events || []).length === 0 ? (
                    <p className="muted">
                      No recorded events yet.
                    </p>
                  ) : (
                    eventHistory.events.map((event, index) => (
                      <div
                        className="operations-list-row"
                        key={index}
                      >
                        <strong>
                          {event.type} · Sensor {event.sensorId}
                        </strong>

                        <span>
                          {event.message}
                          {" · "}
                          {formatDate(event.timestamp)}
                        </span>
                      </div>
                    ))
                  )}
                </div>
              </div>
            </section>

            {/* Rule-based recommendation engine.
                These suggestions are generated using
                device status and telemetry thresholds. */}
            <section className="section-block">
              <div className="section-heading">
                <div>
                  <span className="eyebrow">
                    RULE-BASED ENGINE
                  </span>

                  <h2>Suggested Actions & Predictive Insights</h2>

                  <p>
                    Suggested actions based on sensor
                    conditions and device connectivity.
                  </p>
                </div>
              </div>

              <div className="panel-card">
                <div className="operations-list">
                  {(recommendations.recommendations || []).length === 0 ? (
                    <p className="muted">
                      No issues detected by the current rules.
                    </p>
                  ) : (
                    recommendations.recommendations.map(
                      (recommendation, index) => (
                        <div
                          className="operations-list-row"
                          key={index}
                        >
                          <strong>
                            {recommendation.severity}
                            {" · "}
                            Device {recommendation.deviceId}
                          </strong>

                          
<div className="recommendation-content">
  <span>{recommendation.suggestion}</span>

  {recommendation.reason && (
    <small className="recommendation-evidence">
      <strong>Reason:</strong> {recommendation.reason}

      {recommendation.evidenceCount != null && (
        <>
          {" · "}
          <strong>Supporting events:</strong>{" "}
          {recommendation.evidenceCount}
        </>
      )}
    </small>
  )}
</div>

                        </div>
                      )
                    )
                  )}
                </div>
              </div>
            </section>
          </>
        )}

       {/* ==========================================
    PILLAR 3: NETWORK TOPOLOGY & MESH ROUTING
   ========================================== */}
{activePage === "topology" && (
  <MeshNetworkPanel />
)}

      </main>

      <footer className="footer">
        Smart-X · PROG7312 · IoT Telemetry Platform
      </footer>
    </div>
  );
}

export default App;