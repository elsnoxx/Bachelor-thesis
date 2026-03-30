// src/services/StatsService.ts
import api from '../api/axiosInstance';

export interface BioPoint {
  timestamp: string | Date;
  value: number;
  isPeak?: boolean;
}

export interface DetailBioFeedbackData {
  averageGsr: number;
  maxGsr: number;
  minGsr: number;
  peakCount: number;
  timeAboveThreshold: number;
  chartData: BioPoint[];
  baseline: number;
}

export interface StatItem {
  id: string;
  userId: string;
  gameType: string;
  averageGsr: number;
  bestScore: number;
  totalSessions: number;
  lastPlayed: string;
}

export const StatsService = {
  // Načtení detailu jedné session
  getSessionDetail: async (userEmail: string, sessionId: string): Promise<DetailBioFeedbackData> => {
    const response = await api.get(`/stats/biofeedback/${encodeURIComponent(userEmail)}/summary/${encodeURIComponent(sessionId)}`);
    const data = response.data;

    // Mapování klíčů ze serveru na frontend formát
    return {
      averageGsr: data.averageGsr ?? data.AverageGsr ?? data.average ?? 0,
      maxGsr: data.maxGsr ?? data.MaxGsr ?? data.max ?? 0,
      minGsr: data.minGsr ?? data.MinGsr ?? data.min ?? 0,
      peakCount: data.peakCount ?? data.PeakCount ?? data.peak_count ?? 0,
      timeAboveThreshold: data.timeAboveThreshold ?? data.TimeAboveThreshold ?? data.time_above_threshold ?? 0,
      baseline: data.baseline ?? data.Baseline ?? 0,
      chartData: (data.chartData ?? data.ChartData ?? []).map((p: any) => ({
        timestamp: p.timestamp ?? p.Timestamp ?? p.time ?? p.Time,
        value: p.value ?? p.Value ?? p.gsr ?? p.gsrValue ?? 0,
        isPeak: !!(p.isPeak ?? p.IsPeak)
      }))
    };
  },

  // Načtení seznamu statistik pro uživatele
  getUserStats: async (userId: string): Promise<StatItem[]> => {
    const response = await api.get(`/stats/user/${userId}`);
    const data = response.data;
    return Array.isArray(data) ? data : [data];
  }
};