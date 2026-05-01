import { useState, useEffect } from "react";
import {
  Box,
  Container,
  Typography,
  Paper,
  CircularProgress,
} from "@mui/material";
import {
  PieChart,
  Pie,
  Cell,
  Tooltip as ChartTooltip,
  Legend,
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
} from "recharts";
import CalendarHeatmap from "react-calendar-heatmap";
import "react-calendar-heatmap/dist/styles.css";
import { Tooltip as ReactTooltip } from "react-tooltip";
import "react-tooltip/dist/react-tooltip.css";

import { Navbar } from "../components/common/Navbar";
import axiosClient from "../api/axiosClient";
import { useTranslation } from "react-i18next";

export const StatisticsPage = ({ isDarkMode, toggleTheme }) => {
  const { t, i18n } = useTranslation();

  const [masteryData, setMasteryData] = useState([]);
  const [forecastData, setForecastData] = useState([]);
  const [activityData, setActivityData] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchAllStatistics = async () => {
      try {
        console.log("--- ЗАВАНТАЖЕННЯ РЕАЛЬНОЇ СТАТИСТИКИ ---");

        const masteryResponse = await axiosClient.get(
          "/Statistics/mastery-level",
        );
        const mData = masteryResponse.data;
        setMasteryData([
          {
            name: t("dashboard.stats.new"),
            value: mData.newCards,
            color: "#2196f3",
          },
          {
            name: t("dashboard.stats.learning"),
            value: mData.learningCards,
            color: "#ff9800",
          },
          {
            name: t("deckDetails.mastered"),
            value: mData.masteredCards,
            color: "#4caf50",
          },
        ]);

        const forecastResponse = await axiosClient.get("/Statistics/forecast");
        setForecastData(
          forecastResponse.data.map((item) => ({
            date: new Date(item.date).toLocaleDateString(i18n.language, {
              day: "numeric",
              month: "short",
            }),
            count: item.count,
          })),
        );

        const activityResponse = await axiosClient.get("/Statistics/activity");

        // Залишаю цей лог, щоб ти могла перевірити в консолі (F12), що присилає база
        console.log("API Response (Activity):", activityResponse.data);

        const realData = activityResponse.data.map((item) => ({
          date: item.date ? item.date.split("T")[0] : "",
          count: item.count || item.cardsStudied || item.CardsStudied || 0,
        }));

        setActivityData(realData);
      } catch (error) {
        console.error("Помилка завантаження статистики:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchAllStatistics();
  }, [t, i18n.language]);

  return (
    <Box
      sx={{
        minHeight: "100vh",
        bgcolor: isDarkMode ? "#121212" : "#f9f9f9",
        color: isDarkMode ? "#fff" : "#000",
      }}
    >
      <Navbar isDarkMode={isDarkMode} toggleTheme={toggleTheme} />

      <style>{`
        .react-calendar-heatmap .color-empty { fill: ${isDarkMode ? "#161b22" : "#ebedf0"}; }
        .react-calendar-heatmap .color-scale-1 { fill: #9be9a8; }
        .react-calendar-heatmap .color-scale-2 { fill: #40c463; }
        .react-calendar-heatmap .color-scale-3 { fill: #30a14e; }
        .react-calendar-heatmap .color-scale-4 { fill: #216e39; }
        .react-calendar-heatmap text { font-size: 10px; fill: currentColor; opacity: 0.5; }
      `}</style>

      <Container maxWidth="lg" sx={{ mt: 5, pb: 5 }}>
        <Typography variant="h4" fontWeight="bold" sx={{ mb: 4 }}>
          {t("statistics.title")}
        </Typography>

        {isLoading ? (
          <Box sx={{ display: "flex", justifyContent: "center", mt: 10 }}>
            <CircularProgress color="secondary" />
          </Box>
        ) : (
          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: { xs: "1fr", md: "1fr 1fr" },
              gap: 4,
            }}
          >
            <Paper
              elevation={3}
              sx={{
                p: 4,
                borderRadius: 4,
                height: 400,
                display: "flex",
                flexDirection: "column",
              }}
            >
              <Typography variant="h6" fontWeight="bold" align="center">
                {t("statistics.mastery.title")}
              </Typography>
              <Box sx={{ flexGrow: 1, minHeight: 300 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={masteryData}
                      cx="50%"
                      cy="50%"
                      innerRadius={70}
                      outerRadius={100}
                      paddingAngle={5}
                      dataKey="value"
                    >
                      {masteryData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color} />
                      ))}
                    </Pie>
                    <ChartTooltip />
                    <Legend verticalAlign="bottom" height={36} />
                  </PieChart>
                </ResponsiveContainer>
              </Box>
            </Paper>

            <Paper
              elevation={3}
              sx={{
                p: 4,
                borderRadius: 4,
                height: 400,
                display: "flex",
                flexDirection: "column",
              }}
            >
              <Typography variant="h6" fontWeight="bold" align="center">
                {t("statistics.forecast.title")}
              </Typography>
              <Box sx={{ flexGrow: 1, minHeight: 300 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={forecastData}
                    margin={{ top: 20, right: 10, left: -20, bottom: 0 }}
                  >
                    <CartesianGrid
                      strokeDasharray="3 3"
                      vertical={false}
                      stroke={isDarkMode ? "#444" : "#eee"}
                    />
                    <XAxis
                      dataKey="date"
                      stroke={isDarkMode ? "#aaa" : "#666"}
                    />
                    <YAxis
                      stroke={isDarkMode ? "#aaa" : "#666"}
                      allowDecimals={false}
                    />
                    <ChartTooltip />
                    <Bar
                      dataKey="count"
                      fill="#ff4081"
                      radius={[4, 4, 0, 0]}
                      maxBarSize={40}
                    />
                  </BarChart>
                </ResponsiveContainer>
              </Box>
            </Paper>

            <Paper
              elevation={3}
              sx={{
                gridColumn: { xs: "1", md: "1 / span 2" },
                p: 4,
                borderRadius: 4,
              }}
            >
              <Typography
                variant="h6"
                fontWeight="bold"
                align="center"
                sx={{ mb: 2 }}
              >
                {t("statistics.activity.title")}
              </Typography>
              <Box sx={{ mt: 3, px: { xs: 0, md: 4 } }}>
                <CalendarHeatmap
                  startDate={
                    new Date(
                      new Date().setFullYear(new Date().getFullYear() - 1),
                    )
                  }
                  endDate={new Date(new Date().getTime() + 86400000)}
                  values={activityData}
                  classForValue={(value) => {
                    if (!value || value.count === 0) return "color-empty";
                    return `color-scale-${Math.min(value.count, 4)}`;
                  }}
                  tooltipDataAttrs={(value) => ({
                    "data-tooltip-content": value?.date
                      ? `${value.date}: ${value.count} cards`
                      : "No activity",
                  })}
                />
                <ReactTooltip />
              </Box>
            </Paper>
          </Box>
        )}
      </Container>
    </Box>
  );
};
