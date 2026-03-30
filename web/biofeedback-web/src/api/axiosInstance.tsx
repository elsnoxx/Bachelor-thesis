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

    // 1. PŘIDÁNA PODMÍNKA: Pokud selže přímo refresh, nepokoušej se o něj znovu a hned odhlaš
    if (originalRequest.url.includes('/refresh')) {
        localStorage.clear();
        window.location.href = '/';
        return Promise.reject(error);
    }

    // Pokud je chyba 401 a ještě jsme nezkoušeli retry
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        // Volání refresh endpointu
        // POZOR: zkontroluj, zda tvé API vrací "token" nebo "Token"
        const res = await axios.post(
            `${import.meta.env.VITE_API_URL}/refresh`, 
            {}, 
            { withCredentials: true }
        );

        if (res.status === 200) {
          // OPRAVA: Zkus obě varianty pro jistotu, nebo se koukni do Swaggeru
          const newToken = res.data.token || res.data.Token;
          
          if (newToken) {
            localStorage.setItem("token", newToken);
            
            // Zopakuj původní požadavek
            originalRequest.headers.Authorization = `Bearer ${newToken}`;
            return api(originalRequest);
          }
        }
      } catch (refreshError) {
        // Pokud refresh selže (cookie neexistuje nebo expirovala)
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