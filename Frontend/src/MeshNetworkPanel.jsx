
import { useEffect, useMemo, useState } from "react";
import "./MeshNetworkPanel.css";

const API = "http://localhost:5090/api";

async function networkRequest(path, options = {}) {
  const response = await fetch(`${API}${path}`, options);
  const text = await response.text();

  let data;
  try {
    data = text ? JSON.parse(text) : {};
  } catch {
    data = { message: text };
  }

  if (!response.ok) {
    throw new Error(
      data.message || data.title ||
      `API request failed: ${response.status}`
    );
  }

  return data;
}

function edgeKey(sourceId, targetId) {
  return [Number(sourceId), Number(targetId)]
    .sort((a, b) => a - b)
    .join("-");
}

// Position sensors around the network diagram.
function createLayout(nodes) {
  const positions = new Map();
  const sorted = [...nodes].sort((a, b) => a.id - b.id);
  const count = sorted.length;

  sorted.forEach((node, index) => {
    if (count === 1) {
      positions.set(node.id, { x: 450, y: 240 });
      return;
    }

    let radius;
    let position;
    let ringSize;

    if (count <= 9) {
      radius = 190;
      position = index;
      ringSize = count;
    } else {
      const innerCount = Math.ceil(count / 2);

      if (index < innerCount) {
        radius = 105;
        position = index;
        ringSize = innerCount;
      } else {
        radius = 205;
        position = index - innerCount;
        ringSize = count - innerCount;
      }
    }

    const angle =
      -Math.PI / 2 +
      (2 * Math.PI * position) / ringSize;

    positions.set(node.id, {
      x: 450 + Math.cos(angle) * radius,
      y: 245 + Math.sin(angle) * radius,
    });
  });

  return positions;
}

function Metric({ label, value, detail }) {
  return (
    <article className="mesh-metric">
      <span>{label}</span>
      <strong>{value}</strong>
      <small>{detail}</small>
    </article>
  );
}

export default function MeshNetworkPanel() {
  const [topology, setTopology] = useState({
    nodes: [],
    links: [],
    onlineSensorCount: 0,
    activeLinkCount: 0,
  });

  const [mst, setMst] = useState(null);
  const [traversal, setTraversal] = useState(null);

  const [sourceId, setSourceId] = useState("");
  const [targetId, setTargetId] = useState("");
  const [cost, setCost] = useState("4");
  const [startSensorId, setStartSensorId] = useState("");

  const [showMst, setShowMst] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [loaded, setLoaded] = useState(false);

  const nodes = topology.nodes || [];
  const links = topology.links || [];

  const positions = useMemo(
    () => createLayout(nodes),
    [nodes]
  );

  const onlineNodes = nodes.filter(
    (node) => node.isOnline
  );

  const mstEdges = new Set(
    (mst?.edges || []).map((link) =>
      edgeKey(link.sourceId, link.targetId)
    )
  );

  const visitedNodes = new Set(
    traversal?.visitOrder || []
  );

  // Load the graph and its current minimum spanning tree.
  async function loadNetwork() {
    const [networkData, mstData] = await Promise.all([
      networkRequest("/network/topology"),
      networkRequest("/network/mst"),
    ]);

    setTopology(networkData);
    setMst(mstData);
    setLoaded(true);

    const available = networkData.nodes || [];
    const online = available.filter((s) => s.isOnline);

    setSourceId((previous) =>
      available.some((s) => String(s.id) === previous)
        ? previous
        : String(available[0]?.id ?? "")
    );

    setTargetId((previous) =>
      available.some((s) => String(s.id) === previous)
        ? previous
        : String(available[1]?.id ?? "")
    );

    setStartSensorId((previous) =>
      online.some((s) => String(s.id) === previous)
        ? previous
        : String(online[0]?.id ?? "")
    );
  }

  useEffect(() => {
    loadNetwork().catch((err) => {
      setError(err.message);
      setLoaded(true);
    });
  }, []);

  // Create a weighted connection between two sensors.
  async function addLink(event) {
    event.preventDefault();
    setError("");
    setMessage("");

    const from = Number(sourceId);
    const to = Number(targetId);
    const linkCost = Number(cost);

    if (!from || !to || from === to) {
      setError("Choose two different sensors.");
      return;
    }

    if (!Number.isFinite(linkCost) || linkCost <= 0) {
      setError("Connection cost must be greater than zero.");
      return;
    }

    setBusy(true);

    try {
      await networkRequest("/network/links", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          sourceId: from,
          targetId: to,
          cost: linkCost,
        }),
      });

      await loadNetwork();

      setTraversal(null);
      setShowMst(false);
      setMessage(`Connected Sensor ${from} to Sensor ${to}.`);
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  // Remove a connection from the graph.
  async function removeLink(from, to) {
    setBusy(true);
    setError("");
    setMessage("");

    try {
      await networkRequest(
        `/network/links/${from}/${to}`,
        { method: "DELETE" }
      );

      await loadNetwork();

      setTraversal(null);
      setShowMst(false);
      setMessage(
        `Connection between Sensor ${from} and ${to} removed.`
      );
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  // Execute BFS or DFS traversal.
  async function runTraversal(algorithm) {
    if (!startSensorId) {
      setError("Select an online starting sensor.");
      return;
    }

    setBusy(true);
    setError("");
    setMessage("");

    try {
      const result = await networkRequest(
        `/network/traverse?startSensorId=${encodeURIComponent(
          startSensorId
        )}&algorithm=${algorithm}`
      );

      setTraversal(result);
      setShowMst(false);

      setMessage(
        `${result.algorithm} traversal completed successfully.`
      );
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  // Calculate Prim's minimum spanning tree.
  async function runMst() {
    setBusy(true);
    setError("");
    setMessage("");

    try {
      const result = await networkRequest("/network/mst");

      setMst(result);
      setTraversal(null);
      setShowMst(true);
      setMessage("Prim's algorithm completed successfully.");
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  // Reload the graph after changes made in another tab.
  async function refreshNetwork() {
    setBusy(true);
    setError("");
    setMessage("");

    try {
      await loadNetwork();
      setTraversal(null);
      setShowMst(false);
      setMessage("Network topology refreshed.");
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="mesh-dashboard">
      <div className="mesh-heading">
        <div>
          <span className="mesh-eyebrow">
            PART 3 · GRAPH DATA STRUCTURES
          </span>
          <h2>Network Topology &amp; Mesh Routing</h2>
          <p>
            Explore your Smart-X sensor network using
            weighted graphs, BFS, DFS and Prim's algorithm.
          </p>
        </div>

        <button
          type="button"
          className="mesh-secondary-btn"
          onClick={refreshNetwork}
          disabled={busy}
        >
          Refresh Network
        </button>
      </div>

      {error && (
        <div className="mesh-error" role="alert">
          {error}
        </div>
      )}

      {message && (
        <div className="mesh-message" role="status">
          {message}
        </div>
      )}

      {!loaded && (
        <p className="mesh-muted">
          Loading network topology...
        </p>
      )}

      {/* Network overview */}
      <div className="mesh-metrics">
        <Metric
          label="Registered Sensors"
          value={nodes.length}
          detail="Graph vertices"
        />

        <Metric
          label="Online Sensors"
          value={topology.onlineSensorCount}
          detail="Available for routing"
        />

        <Metric
          label="Active Connections"
          value={topology.activeLinkCount}
          detail="Bidirectional graph edges"
        />

        <Metric
          label="Connected Components"
          value={mst?.connectedComponents ?? "—"}
          detail="Online network groups"
        />
      </div>

      {/* Interactive network graph */}
      <section className="mesh-card">
        <div className="mesh-card-heading">
          <div>
            <h3>Live Mesh Network</h3>
            <p>
              Green links belong to the minimum spanning tree
              when Prim's algorithm is selected.
            </p>
          </div>

          <div className="mesh-legend">
            <span>
              <i className="mesh-dot mesh-online" />
              Online
            </span>
            <span>
              <i className="mesh-dot mesh-offline" />
              Offline
            </span>
          </div>
        </div>

        <div className="mesh-graph-container">
          {nodes.length === 0 ? (
            <div className="mesh-empty">
              No registered sensors found.
            </div>
          ) : (
            <svg
              className="mesh-graph"
              viewBox="0 0 900 500"
              role="img"
              aria-label="Smart-X sensor network diagram"
            >
              {/* Render connections behind the sensors */}
              {links.map((link) => {
                const from = positions.get(link.sourceId);
                const to = positions.get(link.targetId);

                if (!from || !to) return null;

                const selected =
                  showMst &&
                  mstEdges.has(
                    edgeKey(link.sourceId, link.targetId)
                  );

                const lineClass = !link.isActive
                  ? "mesh-edge-inactive"
                  : showMst
                    ? selected
                      ? "mesh-edge-mst"
                      : "mesh-edge-dim"
                    : "mesh-edge-normal";

                const middleX = (from.x + to.x) / 2;
                const middleY = (from.y + to.y) / 2;

                return (
                  <g
                    key={edgeKey(
                      link.sourceId,
                      link.targetId
                    )}
                  >
                    <line
                      x1={from.x}
                      y1={from.y}
                      x2={to.x}
                      y2={to.y}
                      className={lineClass}
                    />

                    <text
                      x={middleX}
                      y={middleY - 7}
                      className="mesh-edge-cost"
                      textAnchor="middle"
                    >
                      {link.cost}
                    </text>
                  </g>
                );
              })}

              {/* Render sensor nodes */}
              {nodes.map((node) => {
                const point = positions.get(node.id);

                if (!point) return null;

                const visited =
                  traversal &&
                  visitedNodes.has(node.id);

                return (
                  <g
                    key={node.id}
                    className={
                      node.isOnline
                        ? "mesh-node-online"
                        : "mesh-node-offline"
                    }
                  >
                    <title>
                      {node.name} — Sensor {node.id} —{" "}
                      {node.isOnline ? "Online" : "Offline"}
                    </title>

                    <circle
                      cx={point.x}
                      cy={point.y}
                      r={visited ? 29 : 25}
                      className={
                        visited
                          ? "mesh-node-circle mesh-node-visited"
                          : "mesh-node-circle"
                      }
                    />

                    <text
                      x={point.x}
                      y={point.y + 5}
                      className="mesh-node-label"
                      textAnchor="middle"
                    >
                      S{node.id}
                    </text>

                    <text
                      x={point.x}
                      y={point.y + 44}
                      className="mesh-node-name"
                      textAnchor="middle"
                    >
                      {node.name.length > 18
                        ? `${node.name.slice(0, 16)}...`
                        : node.name}
                    </text>
                  </g>
                );
              })}
            </svg>
          )}
        </div>

        <div className="mesh-graph-footer">
          <span>
            Weighted, undirected adjacency-list graph
          </span>

          <button
            type="button"
            className="mesh-secondary-btn"
            onClick={() => {
              setShowMst(false);
              setTraversal(null);
            }}
          >
            Show All Links
          </button>
        </div>
      </section>

      {/* Add new network connections */}
      <section className="mesh-card">
        <div className="mesh-card-heading">
          <div>
            <h3>Manage Wireless Connections</h3>
            <p>
              Connect any two registered sensors using
              a simulated positive link cost.
            </p>
          </div>
        </div>

        <form
          className="mesh-link-form"
          onSubmit={addLink}
        >
          <label>
            <span>Source Sensor</span>
            <select
              value={sourceId}
              onChange={(e) => setSourceId(e.target.value)}
              required
            >
              <option value="">Select source</option>

              {nodes.map((node) => (
                <option key={node.id} value={node.id}>
                  S{node.id} — {node.name}
                </option>
              ))}
            </select>
          </label>

          <label>
            <span>Target Sensor</span>
            <select
              value={targetId}
              onChange={(e) => setTargetId(e.target.value)}
              required
            >
              <option value="">Select target</option>

              {nodes.map((node) => (
                <option key={node.id} value={node.id}>
                  S{node.id} — {node.name}
                </option>
              ))}
            </select>
          </label>

          <label>
            <span>Link Cost</span>
            <input
              type="number"
              min="0.01"
              step="0.01"
              value={cost}
              onChange={(e) => setCost(e.target.value)}
              required
            />
          </label>

          <button
            type="submit"
            className="mesh-primary-btn"
            disabled={busy || nodes.length < 2}
          >
            Add Connection
          </button>
        </form>

        <h4 className="mesh-subheading">
          Current Network Connections
        </h4>

        {links.length === 0 ? (
          <div className="mesh-empty">
            No connections yet. Add your first link above.
          </div>
        ) : (
          <div className="mesh-link-list">
            {links.map((link) => (
              <div
                className="mesh-link-item"
                key={edgeKey(
                  link.sourceId,
                  link.targetId
                )}
              >
                <div>
                  <strong>
                    S{link.sourceId} — S{link.targetId}
                  </strong>

                  <span>
                    Cost: {link.cost} ·{" "}
                    {link.isActive ? "Active" : "Inactive"}
                  </span>
                </div>

                <button
                  type="button"
                  className="mesh-remove-btn"
                  disabled={busy}
                  onClick={() =>
                    removeLink(
                      link.sourceId,
                      link.targetId
                    )
                  }
                >
                  Remove
                </button>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Graph traversal and MST algorithms */}
      <section className="mesh-card">
        <div className="mesh-card-heading">
          <div>
            <h3>Graph Algorithms Laboratory</h3>
            <p>
              Execute graph algorithms and view their
              results directly on the network diagram.
            </p>
          </div>
        </div>

        <div className="mesh-algorithms">
          <label className="mesh-start-field">
            <span>Traversal Starting Sensor</span>

            <select
              value={startSensorId}
              onChange={(e) =>
                setStartSensorId(e.target.value)
              }
            >
              {onlineNodes.length === 0 && (
                <option value="">
                  No online sensors
                </option>
              )}

              {onlineNodes.map((node) => (
                <option key={node.id} value={node.id}>
                  Sensor {node.id} — {node.name}
                </option>
              ))}
            </select>
          </label>

          <div className="mesh-algorithm-buttons">
            <button
              type="button"
              className="mesh-primary-btn"
              disabled={busy || !startSensorId}
              onClick={() => runTraversal("bfs")}
            >
              Run BFS
            </button>

            <button
              type="button"
              className="mesh-primary-btn"
              disabled={busy || !startSensorId}
              onClick={() => runTraversal("dfs")}
            >
              Run DFS
            </button>

            <button
              type="button"
              className="mesh-mst-btn"
              disabled={busy}
              onClick={runMst}
            >
              Run Prim's MST
            </button>
          </div>
        </div>

        {traversal && (
          <div className="mesh-results">
            <h4>{traversal.algorithm} Traversal Results</h4>

            <p>
              <strong>Starting Sensor:</strong>{" "}
              S{traversal.startSensorId}
            </p>

            <p>
              <strong>Visit Order:</strong>{" "}
              {traversal.visitOrder.length
                ? traversal.visitOrder
                    .map((id) => `S${id}`)
                    .join(" → ")
                : "None"}
            </p>

            <p>
              <strong>Unreachable Online Sensors:</strong>{" "}
              {traversal.unreachableOnlineSensorIds.length
                ? traversal.unreachableOnlineSensorIds
                    .map((id) => `S${id}`)
                    .join(", ")
                : "None"}
            </p>
          </div>
        )}

        {showMst && mst && (
          <div className="mesh-results">
            <h4>Prim's Minimum Spanning Tree</h4>

            <div className="mesh-result-grid">
              <Metric
                label="Total Link Cost"
                value={Number(mst.totalCost).toFixed(2)}
                detail="Sum of selected edge costs"
              />

              <Metric
                label="Selected Edges"
                value={mst.edges.length}
                detail="Minimum spanning connections"
              />

              <Metric
                label="Components"
                value={mst.connectedComponents}
                detail="Separate online groups"
              />
            </div>

            <p>
              <strong>Network Coverage:</strong>{" "}
              {mst.spansAllOnlineSensors
                ? "All online sensors are connected."
                : "Network is disconnected. A minimum spanning forest was returned."}
            </p>

            <p>
              <strong>Selected Links:</strong>{" "}
              {mst.edges.length
                ? mst.edges
                    .map(
                      (edge) =>
                        `S${edge.sourceId}–S${edge.targetId} (${edge.cost})`
                    )
                    .join(", ")
                : "None"}
            </p>
          </div>
        )}
      </section>

      <p className="mesh-note">
        <strong>How it works:</strong> BFS uses a queue;
        DFS uses a stack; Prim's algorithm uses a priority
        queue to minimize total connection cost. Offline
        sensors are excluded from active routing. This is
        a simulated network, not a physical ESP32 connection.
        Graph connections are stored in memory and reset
        when the backend restarts.
      </p>
    </div>
  );
}
