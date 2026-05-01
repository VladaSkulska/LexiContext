import { useState, useMemo, useEffect } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import {
  ThemeProvider,
  createTheme,
  CssBaseline,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
} from "@mui/material";
import SyncIcon from "@mui/icons-material/Sync";
import { useTranslation } from "react-i18next";
import axiosClient from "./api/axiosClient";

import { AuthPage } from "./pages/AuthPage";
import { DashboardPage } from "./pages/DashboardPage";
import { DeckDetailsPage } from "./pages/DeckDetailsPage";
import { StoriesListPage } from "./pages/StoriesListPage";
import { StoryReaderPage } from "./pages/StoryReaderPage";
import { StudyPage } from "./pages/StudyPage";
import { StatisticsPage } from "./pages/StatisticsPage";

const ProtectedRoute = ({ children }) => {
  const token = localStorage.getItem("token");
  if (!token) {
    return <Navigate to="/login" replace />;
  }
  return children;
};

const PublicRoute = ({ children }) => {
  const token = localStorage.getItem("token");
  if (token) {
    return <Navigate to="/decks" replace />;
  }
  return children;
};

export default function App() {
  const { i18n } = useTranslation();

  const [isDarkMode, setIsDarkMode] = useState(() => {
    const savedTheme = localStorage.getItem("app_theme");
    return savedTheme === "dark";
  });

  const [sessionChanged, setSessionChanged] = useState(false);

  useEffect(() => {
    const fetchUserSettings = async () => {
      const token = localStorage.getItem("token");
      if (!token) return;

      try {
        const response = await axiosClient.get("/UserSettings");
        const settings = response.data;

        const lang = settings.interfaceLanguage === 2 ? "uk" : "en";
        if (i18n.language !== lang) {
          i18n.changeLanguage(lang);
        }

        const isDark = settings.theme === 1;
        setIsDarkMode(isDark);
      } catch (error) {
        console.error("Не вдалося завантажити налаштування юзера:", error);
      }
    };

    fetchUserSettings();

    // Цей рядок нижче каже ESLint ігнорувати відсутність i18n у списку залежностей
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    localStorage.setItem("app_theme", isDarkMode ? "dark" : "light");
  }, [isDarkMode]);

  const toggleTheme = () => setIsDarkMode((prevMode) => !prevMode);

  useEffect(() => {
    const handleStorageChange = (event) => {
      if (event.key === "token") {
        setSessionChanged(true);
      }
    };
    window.addEventListener("storage", handleStorageChange);
    return () => window.removeEventListener("storage", handleStorageChange);
  }, []);

  const handleReload = () => {
    window.location.reload();
  };

  const theme = useMemo(
    () =>
      createTheme({
        palette: {
          mode: isDarkMode ? "dark" : "light",
          primary: { main: "#1A237E" },
          secondary: { main: "#ff4081" },
        },
        shape: { borderRadius: 12 },
      }),
    [isDarkMode],
  );

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />

      <Dialog
        open={sessionChanged}
        PaperProps={{ sx: { borderRadius: 4, p: 1, maxWidth: 400 } }}
      >
        <DialogTitle sx={{ textAlign: "center", pt: 3 }}>
          <SyncIcon
            color="primary"
            sx={{ fontSize: 40, mb: 1, opacity: 0.8 }}
          />
          <Typography variant="h6" fontWeight="bold">
            Потрібна синхронізація
          </Typography>
        </DialogTitle>
        <DialogContent sx={{ textAlign: "center" }}>
          <Typography variant="body1" color="text.secondary">
            Статус вашого акаунта змінився в іншій вкладці або вікні. Щоб
            продовжити роботу та уникнути помилок, будь ласка, оновіть дані.
          </Typography>
        </DialogContent>
        <DialogActions sx={{ justifyContent: "center", pb: 3, px: 3 }}>
          <Button
            onClick={handleReload}
            variant="contained"
            fullWidth
            size="large"
            sx={{ fontWeight: "bold", textTransform: "none", borderRadius: 2 }}
          >
            Оновити дані
          </Button>
        </DialogActions>
      </Dialog>

      <BrowserRouter>
        <Routes>
          <Route
            path="/"
            element={
              <Navigate
                to={localStorage.getItem("token") ? "/decks" : "/login"}
                replace
              />
            }
          />
          <Route
            path="/login"
            element={
              <PublicRoute>
                <AuthPage />
              </PublicRoute>
            }
          />
          <Route
            path="/decks"
            element={
              <ProtectedRoute>
                <DashboardPage
                  isDarkMode={isDarkMode}
                  toggleTheme={toggleTheme}
                />
              </ProtectedRoute>
            }
          />
          <Route
            path="/decks/:id"
            element={
              <ProtectedRoute>
                <DeckDetailsPage
                  isDarkMode={isDarkMode}
                  toggleTheme={toggleTheme}
                />
              </ProtectedRoute>
            }
          />
          <Route
            path="/study/:id"
            element={
              <ProtectedRoute>
                <StudyPage isDarkMode={isDarkMode} toggleTheme={toggleTheme} />
              </ProtectedRoute>
            }
          />
          <Route
            path="/stories"
            element={
              <ProtectedRoute>
                <StoriesListPage
                  isDarkMode={isDarkMode}
                  toggleTheme={toggleTheme}
                />
              </ProtectedRoute>
            }
          />
          <Route
            path="/story/:id"
            element={
              <ProtectedRoute>
                <StoryReaderPage
                  isDarkMode={isDarkMode}
                  toggleTheme={toggleTheme}
                />
              </ProtectedRoute>
            }
          />
          <Route
            path="/statistics"
            element={
              <ProtectedRoute>
                <StatisticsPage
                  isDarkMode={isDarkMode}
                  toggleTheme={toggleTheme}
                />
              </ProtectedRoute>
            }
          />{" "}
        </Routes>
      </BrowserRouter>
    </ThemeProvider>
  );
}
