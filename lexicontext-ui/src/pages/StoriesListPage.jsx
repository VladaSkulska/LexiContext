import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  CardActions,
  Button,
  CircularProgress,
  Chip,
  Grid,
} from "@mui/material";
import AutoStoriesIcon from "@mui/icons-material/AutoStories";
import { Navbar } from "../components/common/Navbar";
import axiosClient from "../api/axiosClient";
import { useTranslation } from "react-i18next";

export const StoriesListPage = ({ isDarkMode, toggleTheme }) => {
  const [stories, setStories] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();
  const { t } = useTranslation();

  const getGenreLabel = (genreValue) => {
    if (genreValue === null || genreValue === undefined)
      return t("stories.defaultGenre");
    const keys = [
      "fairyTale",
      "everydayLife",
      "dialogue",
      "businessEmail",
      "newsReport",
      "sciFi",
    ];
    let keyToTranslate = null;

    if (
      typeof genreValue === "number" &&
      genreValue >= 0 &&
      genreValue < keys.length
    ) {
      keyToTranslate = keys[genreValue];
    } else if (
      typeof genreValue === "string" &&
      !isNaN(parseInt(genreValue, 10))
    ) {
      const idx = parseInt(genreValue, 10);
      if (idx >= 0 && idx < keys.length) keyToTranslate = keys[idx];
    } else if (typeof genreValue === "string") {
      const lowerStr = genreValue.toLowerCase();
      keyToTranslate = keys.find((k) => k.toLowerCase() === lowerStr);
    }
    return keyToTranslate
      ? t(`modals.story.genres.${keyToTranslate}`)
      : t("stories.defaultGenre");
  };

  useEffect(() => {
    const fetchStories = async () => {
      try {
        const response = await axiosClient.get("/Stories/my-stories");
        setStories(response.data);
      } catch (error) {
        console.error("Error loading stories:", error);
      } finally {
        setIsLoading(false);
      }
    };
    fetchStories();
  }, []);

  return (
    <Box
      sx={{ minHeight: "100vh", bgcolor: isDarkMode ? "#121212" : "#f9f9f9" }}
    >
      <Navbar isDarkMode={isDarkMode} toggleTheme={toggleTheme} />

      {/* ВИПРАВЛЕНО: Розширено контейнер з lg до xl */}
      <Container maxWidth="xl" sx={{ mt: { xs: 2, md: 5 }, pb: 5 }}>
        {/* Шапка сторінки */}
        <Box
          sx={{
            display: "flex",
            flexDirection: { xs: "column", sm: "row" },
            justifyContent: "space-between",
            alignItems: { xs: "stretch", sm: "center" },
            gap: 2,
            mb: 5,
          }}
        >
          <Typography
            variant="h4"
            fontWeight="900"
            sx={{ textAlign: { xs: "center", sm: "left" } }}
          >
            {t("stories.title")}
          </Typography>
          <Button
            variant="contained"
            color="primary"
            onClick={() => navigate("/decks")}
            sx={{
              borderRadius: 3,
              px: 4,
              py: 1,
              textTransform: "none",
              fontWeight: "bold",
              boxShadow: 3,
            }}
          >
            {t("stories.generateBtn")}
          </Button>
        </Box>

        {isLoading ? (
          <Box sx={{ display: "flex", justifyContent: "center", mt: 10 }}>
            <CircularProgress color="secondary" />
          </Box>
        ) : stories.length === 0 ? (
          <Box sx={{ textAlign: "center", mt: 10, opacity: 0.6 }}>
            <AutoStoriesIcon sx={{ fontSize: 80, mb: 2 }} />
            <Typography variant="h6">{t("stories.noStoriesTitle")}</Typography>
          </Box>
        ) : (
          <Grid
            container
            spacing={3}
            justifyContent="center"
          >
            {stories.map((story) => (
              <Grid
                item
                xs={12}
                sm={6}
                md={4}
                lg={3} // ВИПРАВЛЕНО: Тепер 4 картки в ряд на великих екранах
                key={story.id}
                sx={{ display: "flex" }}
              >
                <Card
                  sx={{
                    display: "flex",
                    flexDirection: "column",
                    width: "100%",
                    borderRadius: 5,
                    border: isDarkMode
                      ? "1px solid rgba(255,255,255,0.1)"
                      : "1px solid rgba(0,0,0,0.05)",
                    boxShadow: "0 10px 30px rgba(0,0,0,0.05)",
                    transition: "all 0.3s ease",
                    "&:hover": {
                      transform: "translateY(-8px)",
                      boxShadow: "0 20px 40px rgba(0,0,0,0.12)",
                      borderColor: "secondary.main",
                    },
                  }}
                >
                  <CardContent sx={{ flexGrow: 1, p: 3 }}>
                    {/* Жанр та Дата */}
                    <Box
                      sx={{
                        display: "flex",
                        justifyContent: "space-between",
                        alignItems: "flex-start",
                        mb: 2.5,
                        gap: 1,
                      }}
                    >
                      <Chip
                        label={getGenreLabel(story.genre)}
                        size="small"
                        color="secondary"
                        sx={{
                          fontWeight: "bold",
                          borderRadius: 1.5,
                          fontSize: "0.75rem",
                          height: 24,
                        }}
                      />
                      <Typography
                        variant="caption"
                        sx={{
                          color: "text.secondary",
                          fontWeight: 600,
                          mt: 0.5,
                        }}
                      >
                        {new Date(story.createdAt).toLocaleDateString()}
                      </Typography>
                    </Box>

                    {/* Заголовок очищений від HTML-тегів фурігани та відцентрований */}
                    <Box sx={{ minHeight: "3.9em", display: "flex", alignItems: "center", justifyContent: "center" }}>
                      <Typography
                        variant="h6"
                        fontWeight="800"
                        align="center"
                        sx={{
                          lineHeight: 1.3,
                          display: "-webkit-box",
                          WebkitLineClamp: 3,
                          WebkitBoxOrient: "vertical",
                          overflow: "hidden",
                          color: "text.primary",
                          width: "100%"
                        }}
                      >
                        {story.title ? story.title.replace(/<rt>.*?<\/rt>/gi, "").replace(/<\/?[^>]+(>|$)/g, "") : ""}
                      </Typography>
                    </Box>
                  </CardContent>

                  <CardActions sx={{ p: 3, pt: 0 }}>
                    <Button
                      fullWidth
                      variant="contained"
                      color="secondary"
                      onClick={() => navigate(`/stories/${story.id}`)}
                      sx={{
                        borderRadius: 2.5,
                        textTransform: "none",
                        fontWeight: "900",
                        py: 1.2,
                        fontSize: "0.95rem",
                        background:
                          "linear-gradient(45deg, #ff4081 30%, #ff79b0 90%)",
                        "&:hover": {
                          background:
                            "linear-gradient(45deg, #f50057 30%, #ff4081 90%)",
                        },
                      }}
                    >
                      {t("common.read")}
                    </Button>
                  </CardActions>
                </Card>
              </Grid>
            ))}
          </Grid>
        )}
      </Container>
    </Box>
  );
};