const { useEffect, useState } = React;

function App() {
  const [tickets, setTickets] = useState([]);
  const [status, setStatus] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function loadTickets() {
    setLoading(true);
    setError("");

    try {
      const url = status ? `/api/tickets?status=${encodeURIComponent(status)}` : "/api/tickets";
      const response = await fetch(url);
      if (!response.ok) throw new Error("Unable to load tickets.");
      setTickets(await response.json());
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadTickets();
  }, [status]);

  return (
    <main className="container">
      <h1>Ticket Tracker</h1>
      <p className="subtitle">Senior C# developer assessment</p>

      <div className="toolbar">
        <label>
          Status
          <select value={status} onChange={e => setStatus(e.target.value)}>
            <option value="">All</option>
            <option value="Open">Open</option>
            <option value="InProgress">In progress</option>
            <option value="Resolved">Resolved</option>
            <option value="Closed">Closed</option>
          </select>
        </label>
        <button onClick={loadTickets}>Refresh</button>
      </div>

      {error && <div className="error">{error}</div>}
      {loading ? <p>Loading...</p> : (
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Title</th>
              <th>Status</th>
              <th>Priority</th>
              <th>Assigned To</th>
            </tr>
          </thead>
          <tbody>
            {tickets.map(ticket => (
              <tr key={ticket.id}>
                <td>{ticket.id}</td>
                <td>{ticket.title}</td>
                <td>{ticket.status}</td>
                <td>{ticket.priority}</td>
                <td>{ticket.assignedTo || "Unassigned"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </main>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<App />);
