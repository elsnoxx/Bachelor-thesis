import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true,
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (originalRequest.url.includes('/login')) {
      return Promise.reject(error);
    }

    // 1. ADDED CHECK: If the refresh request itself fails, don't try it again and immediately log out
    if (originalRequest.url.includes('/refresh')) {
        localStorage.clear();
        window.location.href = '/';
        return Promise.reject(error);
    }

    // If the error is 401 and we haven't retried yet
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        // Call the refresh endpoint
        // NOTE: check whether your API returns "token" or "Token"
        const res = await axios.post(
            `${import.meta.env.VITE_API_URL}/refresh`, 
            {}, 
            { withCredentials: true }
        );

        if (res.status === 200) {
          // FIX: Try both variants to be safe, or check the Swagger
          const newToken = res.data.token || res.data.Token;
          
          if (newToken) {
            localStorage.setItem("token", newToken);
            
            // Repeat the original request
            originalRequest.headers.Authorization = `Bearer ${newToken}`;
            return api(originalRequest);
          }
        }
      } catch (refreshError) {
        // If refresh fails (cookie doesn't exist or has expired)
        console.error("Refresh token failed", refreshError);
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        window.location.href = '/';
      }
    }

    return Promise.reject(error);
  }
);

export default api;