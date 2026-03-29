import { useContext, useEffect, useState } from "react";
import { AuthContext } from "../commpoment/AuthContext";
import { Table, Spinner } from "react-bootstrap";
import { Link } from "react-router-dom";

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

    fetch(`${base}/stats/user/${userId}`, { headers })
      .then(async (r) => {
        if (!r.ok) throw new Error(`${r.status}`);
        const s = await r.json();
        setStats(Array.isArray(s) ? s : [s]);
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

  const chartData = (stats ?? [])
    .slice() // clone
    .sort((a, b) => new Date(a.timestamp ?? a.date).getTime() - new Date(b.timestamp ?? b.date).getTime())
    .map((b) => ({
      time: new Date(b.timestamp ?? b.date).toLocaleTimeString(), // zobrazit čas na ose X
      gsr: (b.gsrValue ?? b.gsr_value ?? b.value) as number,
    }));

  return (
    <div>
      <h2>Statistiky hráče</h2>

      <section>
        {stats && stats.length > 0 ? (
          <Table striped bordered hover responsive>
            <thead>
              <tr>
                <th>Hra</th>
                <th>Prům. GSR</th>
                <th>Nejlepší skóre</th>
                <th>Počet session</th>
                <th>Poslední hraní</th>
                <th>Detail</th>
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
                  <td>
                    <Link to={`/stats/detail/${encodeURIComponent(s.id)}`} className="btn btn-sm btn-primary">
                      Detail
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <div>Žádné herní statistiky.</div>
        )}
      </section>
    </div>
  );
}
