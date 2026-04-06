import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { StatsService } from '../api/StatsService';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
  ReferenceLine,
} from "recharts";
import { Table, Spinner, Button } from "react-bootstrap";

interface BioPoint {
  timestamp: string | Date;
  value: number;
  isPeak?: boolean;
}

interface DetailBioFeedbackData {
  averageGsr: number;
  maxGsr: number;
  minGsr: number;
  peakCount: number;
  timeAboveThreshold: number;
  chartData: BioPoint[];
  baseline: number;
}

export default function StatistikDetailPage() {
  const { sessionId, gameType } = useParams<{ sessionId?: string; gameType?: string }>();
  const id = sessionId ?? gameType;

  const [summary, setSummary] = useState<DetailBioFeedbackData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (!id) { setError("Chybí sessionId"); return; }
    const userJson = localStorage.getItem("user");
    if (!userJson) { setError("Není přihlášen uživatel"); return; }
    const userObj = JSON.parse(userJson);
    const userEmail = userObj.email || userObj.Email || userObj.userEmail || userObj.id;
    if (!userEmail) { setError("Nelze určit email uživatele"); return; }

    setLoading(true);
    StatsService.getSessionDetail(userEmail, id)
      .then(mapped => setSummary(mapped))
      .catch(e => setError(e?.message || String(e) || "Chyba"))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <Spinner animation="border" />;
  if (error) return <div style={{ color: "red" }}>{error}</div>;
  if (!summary) return <div>Žádná data.</div>;

  const chartData = summary.chartData
    .slice()
    .sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime())
    .map(p => ({
      time: new Date(p.timestamp).toLocaleTimeString(),
      gsr: p.value,
      peak: p.isPeak ? p.value : null,
    }));

  return (
    <div>
      <Button variant="link" onClick={() => navigate(-1)}>← Zpět</Button>
      <h2>Detail session: {id}</h2>

      <div style={{ display: "flex", gap: 12, marginBottom: 12, flexWrap: "wrap" }}>
        <div>Average: <strong>{summary.averageGsr.toFixed(3)}</strong></div>
        <div>Max: <strong>{summary.maxGsr.toFixed(3)}</strong></div>
        <div>Min: <strong>{summary.minGsr.toFixed(3)}</strong></div>
        <div>Peaks: <strong>{summary.peakCount}</strong></div>
        <div>Time above threshold: <strong>{summary.timeAboveThreshold}s</strong></div>
        <div>Baseline: <strong>{summary.baseline.toFixed(3)}</strong></div>
      </div>

      <ResponsiveContainer width="100%" height={350}>
        <LineChart data={chartData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="time" minTickGap={20} />
          <YAxis domain={['auto','auto']} />
          <Tooltip />
          <Legend />
          <ReferenceLine y={summary.baseline} stroke="green" strokeDasharray="4 4" label="baseline" />
          <Line type="monotone" dataKey="gsr" stroke="#8884d8" dot={false} isAnimationActive={false} />
          <Line
            type="monotone"
            dataKey="peak"
            stroke="transparent"
            dot={{ stroke: "red", strokeWidth: 2, r: 3 }}
            connectNulls={false}
            isAnimationActive={false}
          />
        </LineChart>
      </ResponsiveContainer>

      <h4 className="mt-3">Raw / chart points</h4>
      <div style={{ maxHeight: 300, overflowY: 'auto' }}>
        <Table striped bordered hover responsive>
          <thead>
            <tr><th>Timestamp</th><th>GSR</th><th>Peak</th></tr>
          </thead>
          <tbody>
            {summary.chartData.map((p, i) => (
              <tr key={i}>
                <td>{new Date(p.timestamp).toLocaleString()}</td>
                <td>{p.value}</td>
                <td>{p.isPeak ? "✓" : ""}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      </div>
    </div>
  );
}