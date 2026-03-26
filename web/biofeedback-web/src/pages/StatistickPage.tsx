import { useContext, useEffect, useState } from "react";
import { AuthContext } from "../commpoment/AuthContext";
import { Table, Spinner } from "react-bootstrap";

interface StatItem {
  id: string;
  userId: string;
  gameType: string;
  averageGsr: number;
  bestScore: number;
  totalSessions: number;
  lastPlayed: string;
}

export default function StatistikyPage() {
  const { user } = useContext(AuthContext);
  const [stats, setStats] = useState<StatItem[] | null>(null);
  const [bio, setBio] = useState<any[] | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const userJson = localStorage.getItem("user");
    let userId: string | null = null;
    if (userJson) {
      try {
        const userObj = JSON.parse(userJson);
        userId = userObj.id || userObj.userId || userObj.email || null;
      } catch (e) {
        console.warn("Invalid user JSON in localStorage.user", e);
      }
    }

    const token = localStorage.getItem("token") || undefined;
    const base = import.meta.env.VITE_API_URL || "";

    const headers: Record<string, string> = {
      "Content-Type": "application/json",
    };
    if (token) headers["Authorization"] = `Bearer ${token}`;

    if (!userId) {
      setError("Nelze určit userId.");
      return;
    }

    setLoading(true);
    setError(null);

    Promise.all([
      fetch(`${base}/stats/user/${userId}`, { headers }),
      fetch(`${base}/stats/biofeedback/${userId}`, { headers }),
    ])
      .then(async ([r1, r2]) => {
        if (!r1.ok || !r2.ok) {
          const msg = `Server error: ${r1.status} / ${r2.status}`;
          throw new Error(msg);
        }
        const s = await r1.json();
        const b = await r2.json();
        setStats(Array.isArray(s) ? s : [s]);
        setBio(Array.isArray(b) ? b : [b]);
      })
      .catch((err) => setError(err.message || "Chyba při načítání"))
      .finally(() => setLoading(false));
  }, [user]);

  if (!user) return <div>Přihlaste se, prosím.</div>;
  if (loading)
    return (
      <div>
        <Spinner animation="border" /> Načítám statistiky…
      </div>
    );
  if (error) return <div style={{ color: "red" }}>{error}</div>;

  return (
    <div>
      <h2>Statistiky hráče</h2>

      <section>
        <h3>Herní statistiky</h3>
        {stats && stats.length > 0 ? (
          <Table striped bordered hover responsive>
            <thead>
              <tr>
                <th>Hra</th>
                <th>Prům. GSR</th>
                <th>Nejlepší skóre</th>
                <th>Počet session</th>
                <th>Poslední hraní</th>
              </tr>
            </thead>
            <tbody>
              {stats.map((s) => (
                <tr key={s.id}>
                  <td style={{ textTransform: "capitalize" }}>{s.gameType}</td>
                  <td>{s.averageGsr ?? "—"}</td>
                  <td>{s.bestScore ?? "—"}</td>
                  <td>{s.totalSessions ?? "—"}</td>
                  <td>{s.lastPlayed ? new Date(s.lastPlayed).toLocaleString() : "—"}</td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <div>Žádné herní statistiky.</div>
        )}
      </section>

      <section>
        <h3>Biofeedback</h3>
        {bio && bio.length > 0 ? (
          <Table striped bordered hover responsive>
            <thead>
              <tr>
                <th>Timestamp</th>
                <th>GSR</th>
                <th>Data</th>
              </tr>
            </thead>
            <tbody>
              {bio.map((bf, i) => (
                <tr key={i}>
                  <td>{bf.timestamp ? new Date(bf.timestamp).toLocaleString() : bf.date ?? "—"}</td>
                  <td>{bf.gsr_value ?? bf.value ?? "—"}</td>
                  <td style={{ maxWidth: 400 }}>
                    <pre style={{ margin: 0, whiteSpace: "pre-wrap" }}>{JSON.stringify(bf, null, 2)}</pre>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <div>Žádná biofeedback data.</div>
        )}
      </section>
    </div>
  );
}
