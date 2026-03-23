import { useContext, useEffect, useState } from "react";
import { AuthContext } from "../commpoment/AuthContext";

type Stat = any;
type BioFeedback = any;

export default function StatistikyPage() {
  const { user } = useContext(AuthContext);
  const [stats, setStats] = useState<Stat | null>(null);
  const [bio, setBio] = useState<BioFeedback[] | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!user?.id) return;
    const token = localStorage.getItem("token") || undefined;
    const base = import.meta.env.VITE_API_URL || "";

    const headers: Record<string,string> = {
      "Content-Type": "application/json",
    };
    if (token) headers["Authorization"] = `Bearer ${token}`;

    setLoading(true);
    setError(null);

    Promise.all([
      fetch(`${base}/stats/user/${user.id}`, { headers }),
      fetch(`${base}/stats/biofeedback/${user.id}`, { headers }),
    ])
      .then(async ([r1, r2]) => {
        if (!r1.ok || !r2.ok) {
          const msg = `Server error: ${r1.status} / ${r2.status}`;
          throw new Error(msg);
        }
        const s = await r1.json();
        const b = await r2.json();
        setStats(s);
        setBio(Array.isArray(b) ? b : [b]);
      })
      .catch((err) => setError(err.message || "Chyba při načítání"))
      .finally(() => setLoading(false));
  }, [user]);

  if (!user) return <div>Přihlaste se, prosím.</div>;
  if (loading) return <div>Načítám statistiky…</div>;
  if (error) return <div style={{ color: "red" }}>{error}</div>;

  return (
    <div>
      <h2>Statistiky hráče</h2>

      <section>
        <h3>Obecné statistiky</h3>
        {stats ? (
          <pre style={{ whiteSpace: "pre-wrap" }}>{JSON.stringify(stats, null, 2)}</pre>
        ) : (
          <div>Žádné statistiky.</div>
        )}
      </section>

      <section>
        <h3>Biofeedback</h3>
        {bio && bio.length > 0 ? (
          <div style={{ maxHeight: 300, overflow: "auto" }}>
            {bio.map((bf, i) => (
              <div key={i} style={{ padding: 8, borderBottom: "1px solid #eee" }}>
                <div><strong>Timestamp:</strong> {bf.timestamp ?? bf.date ?? "—"}</div>
                <div><strong>GSR:</strong> {bf.gsr_value ?? bf.value ?? "—"}</div>
                <pre style={{ margin: 0 }}>{JSON.stringify(bf, null, 2)}</pre>
              </div>
            ))}
          </div>
        ) : (
          <div>Žádná biofeedback data.</div>
        )}
      </section>
    </div>
  );
}
