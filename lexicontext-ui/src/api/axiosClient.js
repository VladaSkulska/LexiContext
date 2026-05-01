import axios from "axios";

const axiosClient = axios.create({
  baseURL: "http://localhost:5027/api",
  headers: {
    "Content-Type": "application/json",
  },
});

axiosClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  },
);

axiosClient.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response && error.response.status === 401) {
      console.warn("Час сесії вийшов. Виконуємо автоматичний вихід.");

      localStorage.removeItem("token");

      if (window.location.pathname !== "/login") {
        localStorage.setItem("session_expired", "true");
        window.location.href = "/login";
      }
    }

    return Promise.reject(error);
  },
);

export default axiosClient;
