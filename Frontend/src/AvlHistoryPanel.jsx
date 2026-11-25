
import { useEffect, useState } from "react";
import "./AvlHistoryPanel.css";

const API_BASE = "http://localhost:5090/api";

// Convert a date into the format used by datetime-local inputs.
function toLocalInput(date) {
  const local = new Date(
    date.getTime() - date.getTimezoneOffset() * 60000
  );

  return local.toISOString().slice(0, 16);
}

function startOfToday() {
  const date = new Date();
  date.setHours(0, 0, 0, 0);
  return toLocalInput(date);
}

function endOfToday() {
  const date = new Date();
  date.setHours(23, 59, 59, 999);
  return toLocalInput(date);
}

function formatDate(value) {
  return value
    ? new Date(value).toLocaleString()
    : "Not available";
}

async function getJson(path) {
  const response = await fetch(`${API_BASE}${path}`);
  const data = await response.json();

  if (!response.ok) {
    throw new Error(
      typeof data === "string"
        ? data
        : data.message || data.title || "API request failed"
    );
  }

  return data;
}

export default function AvlHistoryPanel() {
  // Date range and optional sensor filter.
  const [start, setStart] = useState(startOfToday);
  const [end, setEnd] = useState(endOfToday);
  const [sensorId, setSensorId] = useState("");

  // AVL tree information returned by our backend.
  const [statistics, setStatistics] = useState(null);
  const [searchResult, setSearchResult] = useState(null);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  // Load the current AVL tree statistics.
  async function refreshStatistics() {
    try {
      const data = await getJson("/History/statistics");
      setStatistics(data);
    } catch (err) {
      setError(err.message);
    }
  }

  // Load statistics when this panel is first displayed.
  useEffect(() => {
    refreshStatistics();
  }, []);

  // Search historical readings using the AVL tree API.
  async function searchHistory(event) {
    event.preventDefault();
    setError("");

    const startDate = new Date(start);
    const endDate = new Date(end);

    if (
      Number.isNaN(startDate.getTime()) ||
      Number.isNaN(endDate.getTime())
    ) {
      setError("Please enter valid start and end dates.");
      return;
    }

    if (startDate > endDate) {
      setError("The start date cannot be after the end date.");
      return;
    }

    if (sensorId.trim()) {
      const id = Number(sensorId);

      if (!Number.isInteger(id) || id <= 0) {
        setError("Sensor ID must be a positive whole number.");
        return;
      }
    }

    setLoading(true);

    try {
      const parameters = new URLSearchParams({
        start: startDate.toISOString(),
        end: endDate.toISOString(),
      });

      if (sensorId.trim()) {
        parameters.set("sensorId", sensorId.trim());
      }

      const result = await getJson(
        `/History/range?${parameters.toString()}`
      );

      setSearchResult(result);

      // Update the tree statistics after searching.
      await refreshStatistics();
    } catch (err) {
      setError(err.message);
      setSearchResult(null);
    } finally {
      setLoading(false);
    }
  }

  const readings = searchResult?.readings || [];

  return (
    <section className="section-block avl-section">
      <div className="section-heading">
        <div>
          <span className="eyebrow">
            PART 3 · ADVANCED DATA STRUCTURES
          </span>

          <h2>AVL Tree Historical Intelligence</h2>

          <p>
            Search timestamped IoT sensor readings using
            a self-balancing AVL tree.
          </p>
        </div>

        <span className="avl-algorithm">
          AVL TREE ACTIVE
        </span>
      </div>

      {/* AVL Tree Statistics */}
      <div className="avl-statistics">
        <article className="avl-stat-card">
          <span>Indexed Readings</span>
          <strong>
            {statistics?.totalIndexedReadings ?? "—"}
          </strong>
          <small>Stored in AVL index</small>
        </article>

        <article className="avl-stat-card">
          <span>Timestamp Nodes</span>
          <strong>
            {statistics?.distinctTimestampNodes ?? "—"}
          </strong>
          <small>Unique timestamp keys</small>
        </article>

        <article className="avl-stat-card">
          <span>Tree Height</span>
          <strong>
            {statistics?.treeHeight ?? "—"}
          </strong>
          <small>Balanced tree levels</small>
        </article>

        <article className="avl-stat-card">
          <span>Algorithm</span>
          <strong className="avl-algorithm-value">
            AVL
          </strong>
          <small>Self-balancing BST</small>
        </article>
      </div>

      {/* Search Form */}
      <div className="avl-search-panel">
        <div className="avl-panel-heading">
          <div>
            <h3>Historical Date-Range Search</h3>
            <p>
              Select a time range and optionally filter
              readings by sensor ID.
            </p>
          </div>

          <button
            type="button"
            className="avl-secondary-button"
            onClick={refreshStatistics}
            disabled={loading}
          >
            Refresh Statistics
          </button>
        </div>

        <form onSubmit={searchHistory}>
          <div className="avl-search-grid">
            <label className="avl-field">
              <span>Start Date &amp; Time</span>
              <input
                type="datetime-local"
                value={start}
                onChange={(event) =>
                  setStart(event.target.value)
                }
                required
              />
            </label>

            <label className="avl-field">
              <span>End Date &amp; Time</span>
              <input
                type="datetime-local"
                value={end}
                onChange={(event) =>
                  setEnd(event.target.value)
                }
                required
              />
            </label>

            <label className="avl-field">
              <span>Sensor ID (Optional)</span>
              <input
                type="number"
                min="1"
                step="1"
                placeholder="All sensors"
                value={sensorId}
                onChange={(event) =>
                  setSensorId(event.target.value)
                }
              />
            </label>
          </div>

          <div className="avl-form-actions">
            <button
              type="submit"
              className="avl-primary-button"
              disabled={loading}
            >
              {loading
                ? "Searching History..."
                : "Search AVL Tree"}
            </button>
          </div>
        </form>

        {error && (
          <div className="avl-error" role="alert">
            {error}
          </div>
        )}
      </div>

      {/* Search Results */}
      {searchResult && (
        <div className="avl-results-panel">
          <div className="avl-panel-heading">
            <div>
              <h3>Historical Query Results</h3>
              <p>
                Results returned in chronological order.
              </p>
            </div>
          </div>

          <div className="avl-result-summary">
            <div>
              <span>Matching Readings</span>
              <strong>
                {searchResult.resultCount}
              </strong>
            </div>

            <div>
              <span>Search Time</span>
              <strong>
                {Number(
                  searchResult.searchTimeMicroseconds ?? 0
                ).toFixed(2)} µs
              </strong>
            </div>

            <div>
              <span>Search Algorithm</span>
              <strong>AVL Range Search</strong>
            </div>
          </div>

          {readings.length === 0 ? (
            <div className="avl-empty">
              No telemetry readings were found for
              the selected date range.
            </div>
          ) : (
            <>
              <div className="avl-table-scroll">
                <table className="avl-table">
                  <thead>
                    <tr>
                      <th>Reading ID</th>
                      <th>Sensor</th>
                      <th>Temperature</th>
                      <th>Humidity</th>
                      <th>Battery</th>
                      <th>Recorded At</th>
                    </tr>
                  </thead>

                  <tbody>
                    {readings.slice(0, 50).map(
                      (reading, index) => (
                        <tr key={`${reading.id}-${index}`}>
                          <td>#{reading.id}</td>
                          <td>
                            Sensor {reading.sensorId}
                          </td>
                          <td>
                            {reading.temperature} °C
                          </td>
                          <td>
                            {reading.humidity}%
                          </td>
                          <td>
                            {reading.batteryLevel}%
                          </td>
                          <td>
                            {formatDate(
                              reading.recordedAt
                            )}
                          </td>
                        </tr>
                      )
                    )}
                  </tbody>
                </table>
              </div>

              {readings.length > 50 && (
                <p className="avl-result-note">
                  Showing the first 50 of{" "}
                  {readings.length} matching readings.
                </p>
              )}
            </>
          )}
        </div>
      )}

      <p className="avl-explanation">
        <strong>Data structure:</strong> The AVL tree
        automatically maintains balance through rotations.
        Timestamp-range searches visit relevant branches
        instead of scanning the entire telemetry collection.
        Readings are currently indexed in memory.
      </p>
    </section>
  );
}
