import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  Box,
  Container,
  Typography,
  Button,
  Paper,
  IconButton,
  CircularProgress,
  LinearProgress,
  Stack,
  Zoom,
  Divider,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import { Navbar } from "../components/common/Navbar";
import axiosClient from "../api/axiosClient";
import { useTranslation } from "react-i18next";

export const StudyPage = ({ isDarkMode, toggleTheme }) => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [cards, setCards] = useState([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isFlipped, setIsFlipped] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [isFinished, setIsFinished] = useState(false);

  useEffect(() => {
    const fetchStudyCards = async () => {
      try {
        const response = await axiosClient.get(`/Cards/deck/${id}/study`);
        setCards(Array.isArray(response.data) ? response.data : []);
      } catch (error) {
        console.error("Помилка завантаження карток:", error);
      } finally {
        setIsLoading(false);
      }
    };
    fetchStudyCards();
  }, [id]);

  const handleFlip = () => setIsFlipped(!isFlipped);

  const handleScore = async (quality) => {
    const currentCard = cards[currentIndex];
    const cardId = currentCard?.cardId || currentCard?.CardId;
    if (!cardId) return;

    try {
      await axiosClient.post(`/Cards/review`, { cardId, quality });
    } catch (error) {
      console.error("Помилка збереження прогресу:", error);
    } finally {
      const isLastCard = currentIndex === cards.length - 1;
      if (quality === 1 || quality === 2) {
        setCards((prevCards) => [...prevCards, currentCard]);
        setIsFlipped(false);
        setTimeout(() => setCurrentIndex((prev) => prev + 1), 150);
      } else {
        if (isLastCard) setIsFinished(true);
        else {
          setIsFlipped(false);
          setTimeout(() => setCurrentIndex((prev) => prev + 1), 150);
        }
      }
    }
  };

  if (isLoading) {
    return (
      <Box
        sx={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          height: "80vh",
        }}
      >
        <CircularProgress color="secondary" />
      </Box>
    );
  }

  if (!cards || cards.length === 0) {
    return (
      <Box
        sx={{ minHeight: "100vh", bgcolor: isDarkMode ? "#121212" : "#f5f5f5" }}
      >
        <Navbar isDarkMode={isDarkMode} toggleTheme={toggleTheme} />
        <Container maxWidth="sm" sx={{ mt: 10, textAlign: "center" }}>
          <Typography variant="h5" gutterBottom>
            {t("study.emptyTitle")}
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
            {t("study.emptyDesc")}
          </Typography>
          <Button
            onClick={() => navigate(`/decks/${id}`)}
            variant="contained"
            sx={{ mt: 3 }}
          >
            {t("study.backBtn")}
          </Button>
        </Container>
      </Box>
    );
  }

  if (isFinished) {
    return (
      <Box
        sx={{ minHeight: "100vh", bgcolor: isDarkMode ? "#121212" : "#f5f5f5" }}
      >
        <Navbar isDarkMode={isDarkMode} toggleTheme={toggleTheme} />
        <Container maxWidth="sm" sx={{ mt: 10, textAlign: "center" }}>
          <Zoom in={true}>
            <Paper elevation={3} sx={{ p: 6, borderRadius: 4 }}>
              <CheckCircleOutlineIcon
                color="success"
                sx={{ fontSize: 80, mb: 2 }}
              />
              <Typography variant="h4" fontWeight="bold" gutterBottom>
                {t("study.successTitle")}
              </Typography>
              <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
                {t("study.successDesc")}
              </Typography>
              <Stack spacing={2}>
                <Button
                  variant="contained"
                  size="large"
                  onClick={() => navigate(`/decks`)}
                  sx={{ borderRadius: 3 }}
                >
                  {t("study.allDecksBtn")}
                </Button>
                <Button
                  variant="outlined"
                  size="large"
                  onClick={() => navigate(`/decks/${id}`)}
                  sx={{ borderRadius: 3 }}
                >
                  {t("study.settingsBtn")}
                </Button>
              </Stack>
            </Paper>
          </Zoom>
        </Container>
      </Box>
    );
  }

  const currentCard = cards[currentIndex];
  if (!currentCard) return null;
  const progress = ((currentIndex + 1) / cards.length) * 100;
  const frontText = currentCard?.front || currentCard?.Front || "";
  const backText = currentCard?.back || currentCard?.Back || "";
  const contextText =
    currentCard?.generatedContext || currentCard?.GeneratedContext || "";
  const translationText =
    currentCard?.contextTranslation || currentCard?.ContextTranslation || "";
  const readingText =
    currentCard?.contextReading || currentCard?.ContextReading || "";

  return (
    <Box
      sx={{
        height: "100vh",
        display: "flex",
        flexDirection: "column",
        overflow: "hidden",
        bgcolor: isDarkMode ? "#121212" : "#f5f5f5",
      }}
    >
      <Navbar isDarkMode={isDarkMode} toggleTheme={toggleTheme} />
      <Container
        maxWidth="sm"
        sx={{
          flexGrow: 1,
          display: "flex",
          flexDirection: "column",
          justifyContent: "center",
          py: 2,
        }}
      >
        <Stack
          direction="row"
          justifyContent="space-between"
          alignItems="center"
          sx={{ mb: 3 }}
        >
          <IconButton onClick={() => navigate(`/decks/${id}`)}>
            <CloseIcon />
          </IconButton>
          <Box sx={{ width: "70%", mx: 2 }}>
            <LinearProgress
              variant="determinate"
              value={progress}
              color="secondary"
              sx={{ height: 10, borderRadius: 5 }}
            />
          </Box>
          <Typography variant="body2" fontWeight="bold">
            {currentIndex + 1} / {cards.length}
          </Typography>
        </Stack>

        <Box
          onClick={handleFlip}
          sx={{
            perspective: "1000px",
            height: "min(50vh, 400px)",
            position: "relative",
            cursor: "pointer",
          }}
        >
          <Paper
            elevation={10}
            sx={{
              width: "100%",
              height: "100%",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              textAlign: "center",
              p: 4,
              borderRadius: 6,
              transition: "transform 0.6s cubic-bezier(0.4, 0, 0.2, 1)",
              transformStyle: "preserve-3d",
              transform: isFlipped ? "rotateY(180deg)" : "rotateY(0deg)",
              bgcolor: isDarkMode ? "#1e1e1e" : "#fff",
              border: "1px solid",
              borderColor: "divider",
            }}
          >
            <Box
              sx={{ backfaceVisibility: "hidden", position: "absolute", p: 3 }}
            >
              <Typography
                variant="h3"
                fontWeight="900"
                color="primary"
                sx={{ wordBreak: "break-word" }}
              >
                {frontText}
              </Typography>
              <Typography
                variant="body2"
                color="text.secondary"
                sx={{ mt: 4, opacity: 0.6 }}
              >
                {t("study.flipHint")}
              </Typography>
            </Box>
            <Box
              sx={{
                backfaceVisibility: "hidden",
                position: "absolute",
                transform: "rotateY(180deg)",
                p: 3,
                width: "100%",
              }}
            >
              <Typography
                variant="h3"
                color="secondary"
                fontWeight="900"
                sx={{ wordBreak: "break-word" }}
              >
                {backText}
              </Typography>
              {contextText && (
                <Box sx={{ mt: 4 }}>
                  <Divider sx={{ mb: 3, width: "60%", mx: "auto" }} />
                  {readingText && (
                    <Typography
                      variant="body2"
                      color="primary"
                      sx={{ mb: 1, letterSpacing: 1, fontWeight: "bold" }}
                    >
                      {readingText}
                    </Typography>
                  )}
                  <Typography variant="h6" sx={{ fontWeight: 500 }}>
                    {contextText}
                  </Typography>
                  {translationText && (
                    <Typography
                      variant="body2"
                      color="text.secondary"
                      sx={{ mt: 1, fontStyle: "italic" }}
                    >
                      {translationText}
                    </Typography>
                  )}
                </Box>
              )}
            </Box>
          </Paper>
        </Box>

        <Box sx={{ mt: 3, minHeight: "80px" }}>
          {isFlipped ? (
            <Zoom in={isFlipped}>
              <Stack direction="row" spacing={2}>
                <Button
                  fullWidth
                  variant="outlined"
                  color="error"
                  size="large"
                  onClick={() => handleScore(1)}
                  sx={{ borderRadius: 3, py: 1.8, fontWeight: "bold" }}
                >
                  {t("study.buttons.again")}
                </Button>
                <Button
                  fullWidth
                  variant="contained"
                  color="warning"
                  size="large"
                  onClick={() => handleScore(2)}
                  sx={{
                    borderRadius: 3,
                    py: 1.8,
                    fontWeight: "bold",
                    color: "#fff",
                  }}
                >
                  {t("study.buttons.hard")}
                </Button>
                <Button
                  fullWidth
                  variant="contained"
                  color="success"
                  size="large"
                  onClick={() => handleScore(3)}
                  sx={{ borderRadius: 3, py: 1.8, fontWeight: "bold" }}
                >
                  {t("study.buttons.good")}
                </Button>
              </Stack>
            </Zoom>
          ) : (
            <Typography
              align="center"
              color="text.secondary"
              sx={{ fontStyle: "italic" }}
            >
              {t("study.thinkHint")}
            </Typography>
          )}
        </Box>
      </Container>
    </Box>
  );
};
