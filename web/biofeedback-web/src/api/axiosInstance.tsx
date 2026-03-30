// src/api/axiosInstance.ts
import axios from 'axios';

const api = axios.create({
  baseURL: `${import.meta.env.VITE_API_URL}`,
  withCredentials: true, // DŮLEŽITÉ: aby se posílaly refresh cookies z tvého C# API
});

// Interceptor pro odchozí požadavky (přidá JWT)
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor pro příchozí odpovědi (řeší 401)
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Pokud dostaneme 401 a ještě jsme se nepokusili o refresh
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        // Volání tvého C# endpointu [HttpPost("refresh")]
        const res = await axios.post(`${import.meta.env.VITE_API_URL}/refresh`, {}, { withCredentials: true });

        if (res.status === 200) {
          const { Token } = res.data;
          localStorage.setItem("token", Token);
          
          // Zopakuj původní požadavek s novým tokenem
          originalRequest.headers.Authorization = `Bearer ${Token}`;
          return api(originalRequest);
        }
      } catch (refreshError) {
        // Refresh selhal (např. vypršela cookie) -> totální odhlášení
        localStorage.clear();
        window.location.href = '/'; 
      }
    }
    return Promise.reject(error);
  }
);

export default api;