import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { StatsService } from '../api/StatsService';
import { Table, Spinner, Button, Badge, Card, Row, Col } from "react-bootstrap"; // přidej Badge a Card
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

interface BioPoint {
  timestamp: string | Date;
  value: number;
  isPeak?: boolean;
}

interface DetailBioFeedbackData {
  title: string;
  averageGsr: number;
  maxGsr: number;
  minGsr: number;
  peakCount: number;
  timeAboveThreshold: number;
  chartData: BioPoint[];
  baseline: number;
  result?: string;
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

  const userJson = localStorage.getItem("user");
  const currentUserEmail = userJson ? JSON.parse(userJson).email : null;
  const isWin = summary.result?.toLowerCase() === "win" || (summary.result && summary.result === currentUserEmail);

  
  const chartData = summary.chartData
    .slice()
    .sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime())
    .map(p => ({
      time: new Date(p.timestamp).toLocaleTimeString(),
      gsr: p.value,
      peak: p.isPeak ? p.value : null,
    }));

  console.log(summary.title)

  return (
    <div>
      <Button variant="link" onClick={() => navigate(-1)} className="mb-2 p-0">← Zpět k přehledu</Button>

      <div className="d-flex align-items-center gap-3 mb-4">
        <h2 className="m-0">{summary.title}</h2>
        {summary.result && (
          <Badge bg={isWin ? "success" : "danger"} style={{ fontSize: '1.2rem', padding: '8px 16px' }}>
            {isWin ? "🏆 VÍTĚZSTVÍ" : "💀 PROHRA"}
          </Badge>
        )}
      </div>

      {/* Statistiky v kartách pro lepší přehlednost */}
      <Row className="mb-4 g-3">
        {[
          { label: "AVG EDA", val: summary.averageGsr.toFixed(0), unit: "" },
          { label: "MAX EDA", val: summary.maxGsr.toFixed(0), unit: "" },
          { label: "Peaks", val: summary.peakCount, unit: "" },
          { label: "Threshold", val: summary.timeAboveThreshold, unit: "s" },
        ].map((item, idx) => (
          <Col key={idx} xs={6} md={3}>
            <Card className="text-center shadow-sm">
              <Card.Body>
                <div className="text-muted small uppercase">{item.label}</div>
                <div className="h4 m-0 font-weight-bold">{item.val}{item.unit}</div>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>

      <ResponsiveContainer width="100%" height={350}>
        <LineChart data={chartData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="time" minTickGap={20} />
          <YAxis domain={['auto', 'auto']} />
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