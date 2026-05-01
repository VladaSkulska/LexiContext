import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
  Box,
  Typography,
  Button,
  Container,
  Card,
  CardContent,
  CardActions,
  Chip,
  Divider,
  CircularProgress,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import { Navbar } from "../components/common/Navbar";
import axiosClient from "../api/axiosClient";
import { CreateDeckModal } from "../components/decks/CreateDeckModal";
import { useTranslation } from "react-i18next";

export const DashboardPage = ({ isDarkMode, toggleTheme }) => {
  const [decks, setDecks] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isCreating, setIsCreating] = useState(false);

  useEffect(() => {
    const fetchDecks = async () => {
      try {
        const response = await axiosClient.get("/Decks");
        setDecks(response.data);
      } catch (error) {
        console.error("Error loading decks:", error);
      } finally {
        setIsLoading(false);
      }
    };
    fetchDecks();
  }, []);

  const handleCreateDeck = async (formData) => {
    setIsCreating(true);
    try {
      const response = await axiosClient.post("/Decks", formData);
      setDecks((prev) => [response.data, ...prev]);
      setIsModalOpen(false);
    } finally {
      setIsCreating(false);
    }
  };

  return (
    <Box sx={{ minHeight: "100vh" }}>
      <Navbar isDarkMode={isDarkMode} toggleTheme={toggleTheme} />
      <Container maxWidth="lg" sx={{ mt: 5, pb: 5 }}>
        <Box
          sx={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            mb: 4,
          }}
        >
          <Typography variant="h4" component="h1" fontWeight="bold">
            {t("dashboard.title")}
          </Typography>
          <Button
            variant="contained"
            color="secondary"
            startIcon={<AddIcon />}
            sx={{ textTransform: "none", borderRadius: 2, fontWeight: "bold" }}
            onClick={() => setIsModalOpen(true)}
          >
            {t("dashboard.createBtn")}
          </Button>
        </Box>

        {isLoading ? (
          <Box sx={{ display: "flex", justifyContent: "center", mt: 10 }}>
            <CircularProgress color="secondary" />
          </Box>
        ) : !decks || decks.length === 0 ? (
          <Typography
            variant="h6"
            color="text.secondary"
            align="center"
            sx={{ mt: 10 }}
          >
            {t("dashboard.noDecks")}
          </Typography>
        ) : (
          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: {
                xs: "1fr",
                sm: "repeat(2, 1fr)",
                md: "repeat(3, 1fr)",
              },
              gap: 3,
              alignItems: "stretch",
            }}
          >
            {decks.map((deck) => (
              <Card
                key={deck.id}
                onClick={() => navigate(`/decks/${deck.id}`)}
                sx={{
                  display: "flex",
                  flexDirection: "column",
                  borderRadius: 3,
                  boxShadow: 3,
                  cursor: "pointer",
                  transition: "transform 0.2s, box-shadow 0.2s",
                  "&:hover": { transform: "translateY(-4px)", boxShadow: 6 },
                }}
              >
                <CardContent
                  sx={{ flexGrow: 1, display: "flex", flexDirection: "column" }}
                >
                  <Typography
                    variant="h6"
                    component="h2"
                    sx={{
                      mb: 2,
                      fontWeight: "bold",
                      display: "-webkit-box",
                      WebkitLineClamp: 2,
                      WebkitBoxOrient: "vertical",
                      overflow: "hidden",
                      textOverflow: "ellipsis",
                      wordBreak: "break-word",
                    }}
                  >
                    {deck.title || t("dashboard.untitled")}
                  </Typography>
                  <Divider sx={{ mb: 2 }} />
                  <Box sx={{ mt: "auto" }}>
                    <Box
                      sx={{
                        display: "flex",
                        justifyContent: "space-between",
                        mb: 1,
                      }}
                    >
                      <Typography variant="body2" color="text.secondary">
                        {t("dashboard.stats.new")}
                      </Typography>
                      <Chip
                        label={deck.newCards ?? 0}
                        size="small"
                        color="info"
                        variant={deck.newCards > 0 ? "filled" : "outlined"}
                      />
                    </Box>
                    <Box
                      sx={{
                        display: "flex",
                        justifyContent: "space-between",
                        mb: 1,
                      }}
                    >
                      <Typography variant="body2" color="text.secondary">
                        {t("dashboard.stats.learning")}
                      </Typography>
                      <Chip
                        label={deck.learningCards ?? 0}
                        size="small"
                        color="error"
                        variant={deck.learningCards > 0 ? "filled" : "outlined"}
                      />
                    </Box>
                    <Box
                      sx={{ display: "flex", justifyContent: "space-between" }}
                    >
                      <Typography variant="body2" color="text.secondary">
                        {t("dashboard.stats.review")}
                      </Typography>
                      <Chip
                        label={deck.toReview ?? 0}
                        size="small"
                        color="success"
                        variant={deck.toReview > 0 ? "filled" : "outlined"}
                      />
                    </Box>
                  </Box>
                </CardContent>
                <CardActions sx={{ p: 2, pt: 0 }}>
                  <Button
                    size="medium"
                    startIcon={<PlayArrowIcon />}
                    fullWidth
                    variant="contained"
                    color="primary"
                    sx={{ textTransform: "none", borderRadius: 2 }}
                    disabled={
                      !deck.newCards && !deck.learningCards && !deck.toReview
                    }
                    onClick={(e) => {
                      e.stopPropagation();
                      navigate(`/study/${deck.id}`);
                    }}
                  >
                    {t("dashboard.studyBtn")}
                  </Button>
                </CardActions>
              </Card>
            ))}
          </Box>
        )}
      </Container>
      <CreateDeckModal
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSubmit={handleCreateDeck}
        isSaving={isCreating}
      />
    </Box>
  );
};
